using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine;
using HarmonyLib;

namespace UnityModManagerNet
{
    //[ComVisible(true)]
    //public class RngWrapper : RandomNumberGenerator
    //{
    //    readonly RNGCryptoServiceProvider _wrapped;

    //    static RngWrapper()
    //    {
    //        Injector.Run();
    //    }

    //    public RngWrapper()
    //    {
    //        this._wrapped = new RNGCryptoServiceProvider();
    //    }

    //    public RngWrapper(string str)
    //    {
    //        this._wrapped = new RNGCryptoServiceProvider(str);
    //    }

    //    public RngWrapper(byte[] rgb)
    //    {
    //        this._wrapped = new RNGCryptoServiceProvider(rgb);
    //    }

    //    public RngWrapper(CspParameters cspParams)
    //    {
    //        this._wrapped = new RNGCryptoServiceProvider(cspParams);
    //    }

    //    public override void GetBytes(byte[] data)
    //    {
    //        this._wrapped.GetBytes(data);
    //    }

    //    public override void GetNonZeroBytes(byte[] data)
    //    {
    //        this._wrapped.GetNonZeroBytes(data);
    //    }
    //}

    public class Injector
    {
        static bool usePrefix = false;

        public static void Run(bool doorstop = false)
        {
            try
            {
                _Run(doorstop);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                UnityModManager.OpenUnityFileLog();
            }
        }

        private static void _Run(bool doorstop)
        {
            Console.WriteLine();
            Console.WriteLine();
            UnityModManager.Logger.Log("Injection...");

            if (!UnityModManager.Initialize())
            {
                UnityModManager.Logger.Log($"Cancel start due to an error.");
                UnityModManager.OpenUnityFileLog();
                return;
            }

            Fixes.Apply();

            if (!string.IsNullOrEmpty(UnityModManager.Config.StartingPoint))
            {
                if (!doorstop && UnityModManager.Config.StartingPoint == UnityModManager.Config.EntryPoint)
                {
                    UnityModManager.Start();
                }
                else
                {
                    if (TryGetEntryPoint(UnityModManager.Config.StartingPoint, out var @class, out var method, out var place))
                    {
                        usePrefix = (place == "before");
                        var harmony = new HarmonyLib.Harmony(nameof(UnityModManager));
                        var prefix = typeof(Injector).GetMethod(nameof(Prefix_Start), BindingFlags.Static | BindingFlags.NonPublic);
                        var postfix = typeof(Injector).GetMethod(nameof(Postfix_Start), BindingFlags.Static | BindingFlags.NonPublic);
                        harmony.Patch(method, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
                        UnityModManager.Logger.Log("Injection successful.");
                    }
                    else
                    {
                        UnityModManager.Logger.Log("Injection canceled.");
                        UnityModManager.OpenUnityFileLog();
                        return;
                    }
                }
            }
            else
            {
                UnityModManager.Start();
            }

            if (!string.IsNullOrEmpty(UnityModManager.Config.UIStartingPoint))
            {
                if (TryGetEntryPoint(UnityModManager.Config.UIStartingPoint, out var @class, out var method, out var place))
                {
                    usePrefix = (place == "before");
                    var harmony = new HarmonyLib.Harmony(nameof(UnityModManager));
                    var prefix = typeof(Injector).GetMethod(nameof(Prefix_Show), BindingFlags.Static | BindingFlags.NonPublic);
                    var postfix = typeof(Injector).GetMethod(nameof(Postfix_Show), BindingFlags.Static | BindingFlags.NonPublic);
                    harmony.Patch(method, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
                }
                else
                {
                    UnityModManager.OpenUnityFileLog();
                    return;
                }
            }
            else if (UnityModManager.UI.Instance)
            {
                UnityModManager.UI.Instance.FirstLaunch();
            }
        }

        static void Prefix_Start()
        {
            if (usePrefix)
                UnityModManager.Start();
        }

        static void Postfix_Start()
        {
            if (!usePrefix)
                UnityModManager.Start();
        }

        static void Prefix_Show()
        {
            if (usePrefix && UnityModManager.UI.Instance)
                UnityModManager.UI.Instance.FirstLaunch();
        }

        static void Postfix_Show()
        {
            if (!usePrefix && UnityModManager.UI.Instance)
                UnityModManager.UI.Instance.FirstLaunch();
        }

        internal static bool TryGetEntryPoint(string str, out Type foundClass, out MethodInfo foundMethod, out string insertionPlace)
        {
            foundClass = null;
            foundMethod = null;
            insertionPlace = null;

            if (TryParseEntryPoint(str, out string assemblyName, out _, out _, out _))
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.ManifestModule.Name == assemblyName)
                    {
                        return TryGetEntryPoint(assembly, str, out foundClass, out foundMethod, out insertionPlace);
                    }
                }
                UnityModManager.Logger.Error($"Assembly '{assemblyName}' not found.");

                return false;
            }

            return false;
        }

        internal static bool TryGetEntryPoint(Assembly assembly, string str, out Type foundClass, out MethodInfo foundMethod, out string insertionPlace)
        {
            foundClass = null;
            foundMethod = null;

            if (!TryParseEntryPoint(str, out _, out var className, out var methodName, out insertionPlace))
            {
                return false;
            }

            foundClass = assembly.GetType(className);
            if (foundClass == null)
            {
                UnityModManager.Logger.Error($"Class '{className}' not found.");
                return false;
            }

            foundMethod = foundClass.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (foundMethod == null)
            {
                UnityModManager.Logger.Error($"Method '{methodName}' not found.");
                return false;
            }

            return true;
        }

        internal static bool TryParseEntryPoint(string str, out string assembly, out string @class, out string method, out string insertionPlace)
        {
            assembly = string.Empty;
            @class = string.Empty;
            method = string.Empty;
            insertionPlace = string.Empty;

            var regex = new Regex(@"(?:(?<=\[)(?'assembly'.+(?>\.dll))(?=\]))|(?:(?'class'[\w|\.]+)(?=\.))|(?:(?<=\.)(?'func'\w+))|(?:(?<=\:)(?'mod'\w+))", RegexOptions.IgnoreCase);
            var matches = regex.Matches(str);
            var groupNames = regex.GetGroupNames();

            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    foreach (var group in groupNames)
                    {
                        if (match.Groups[group].Success)
                        {
                            switch (group)
                            {
                                case "assembly":
                                    assembly = match.Groups[group].Value;
                                    break;
                                case "class":
                                    @class = match.Groups[group].Value;
                                    break;
                                case "func":
                                    method = match.Groups[group].Value;
                                    if (method == "ctor")
                                        method = ".ctor";
                                    else if (method == "cctor")
                                        method = ".cctor";
                                    break;
                                case "mod":
                                    insertionPlace = match.Groups[group].Value.ToLower();
                                    break;
                            }
                        }
                    }
                }
            }

            var hasError = false;

            if (string.IsNullOrEmpty(assembly))
            {
                hasError = true;
                UnityModManager.Logger.Error("Assembly name not found.");
            }

            if (string.IsNullOrEmpty(@class))
            {
                hasError = true;
                UnityModManager.Logger.Error("Class name not found.");
            }

            if (string.IsNullOrEmpty(method))
            {
                hasError = true;
                UnityModManager.Logger.Error("Method name not found.");
            }

            if (hasError)
            {
                UnityModManager.Logger.Error($"Error parsing EntryPoint '{str}'.");
                return false;
            }

            return true;
        }
    }
}
