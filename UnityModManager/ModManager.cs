using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UnityModManagerNet
{
    public class UnityModManager
    {
        public const string version = "0.8.0";
        public const string modsDirname = "Mods";
        public const string infoFilename = "info.json";

        public class ModInfo
        {
            public string Id;

            public string DisplayName;

            public string Author;

            public string Version;

            public string ModManager;

            public string[] Requirements;

            public string AssemblyName;

            public string EntryMethod;
        }

        public class ModEntry
        {
            public readonly ModInfo Info;

            public readonly string Path;

            Assembly mAssembly = null;
            public Assembly Assembly => mAssembly;

            public readonly Version Version = null;

            public readonly Dictionary<string, Version> Requirements = new Dictionary<string, Version>();

            public readonly ModLogger Logger = null;

            public Func<ModEntry, bool> OnEnable = null;
            public Func<ModEntry, bool> OnDisable = null;

            Dictionary<long, MethodInfo> mCache = new Dictionary<long, MethodInfo>();

            bool mEnabled;
            public bool Enabled
            {
                get => mEnabled;
                set
                {
                    try
                    {
                        if (value)
                        {
                            if (mEnabled)
                                return;

                            if (OnEnable != null)
                            {
                                if (OnEnable(this))
                                {
                                    mEnabled = true;
                                    this.Logger.Log($"Enabled.");
                                }
                                return;
                            }
                            this.Logger.Log($"Enabled.");
                        }
                        else
                        {
                            if (!mEnabled)
                                return;

                            if (OnDisable != null)
                            {
                                if (OnDisable(this))
                                {
                                    mEnabled = false;
                                    this.Logger.Log($"Disabled.");
                                }
                                return;
                            }
                            this.Logger.Log($"Disabled.");
                        }

                        mEnabled = value;
                    }
                    catch (Exception e)
                    {
                        this.Logger.Error($"Error trying to call 'Enable/Disable' function.");
                        this.Logger.Error(e.Message);
                    }
                }
            }

            public ModEntry(ModInfo info, string path)
            {
                Info = info;
                Path = path;
                Logger = new ModLogger(Info.Id);
                Version = ParseVersion(info.Version);

                if (info.Requirements != null && info.Requirements.Length > 0)
                {
                    var regex = new Regex(@"(.*)-(\d\.\d\.\d).*");
                    foreach (var id in info.Requirements)
                    {
                        var match = regex.Match(id);
                        if (match.Success)
                        {
                            Requirements.Add(match.Groups[1].Value, ParseVersion(match.Groups[2].Value));
                            continue;
                        }
                        Requirements.Add(id, null);
                    }
                }
            }

            public void Load()
            {
                this.Logger.Log($"Version '{Info.Version}'. Loading.");
                if (string.IsNullOrEmpty(Info.AssemblyName))
                {
                    this.Logger.Error($"{nameof(Info.AssemblyName)} is null.");
                    return;
                }

                if (string.IsNullOrEmpty(Info.EntryMethod))
                {
                    this.Logger.Error($"{nameof(Info.EntryMethod)} is null.");
                    return;
                }

                if (!string.IsNullOrEmpty(Info.ModManager))
                {
                    var needVersion = ParseVersion(Info.ModManager);
                    if (needVersion > GetVersion())
                    {
                        this.Logger.Error($"ModManager must be version '{Info.ModManager}' or higher.");
                        return;
                    }
                }

                if (Requirements.Count > 0)
                {
                    foreach (var item in Requirements)
                    {
                        var id = item.Key;
                        var mod = FindMod(id);
                        if (mod == null || mod.Assembly == null)
                        {
                            this.Logger.Error($"Required mod '{id}' not loaded.");
                            return;
                        }
                        else if (!mod.Enabled)
                        {
                            this.Logger.Log($"Required mod '{id}' disabled.");
                        }
                        else if (item.Value != null)
                        {
                            if (item.Value > mod.Version)
                            {
                                this.Logger.Error($"Required mod '{id}' must be version '{item.Value}' or higher.");
                                return;
                            }
                        }
                    }
                }

                string assemblyPath = System.IO.Path.Combine(Path, Info.AssemblyName);
                if (File.Exists(assemblyPath))
                {
                    try
                    {
                        mAssembly = Assembly.LoadFile(assemblyPath);

                        object[] param = new object[] { this };
                        Type[] types = new Type[] { typeof(ModEntry) };
                        if (!MethodHasParams(Info.EntryMethod, types))
                        {
                            param = null;
                            types = null;
                        }

                        if (!Invoke(Info.EntryMethod, out var result, param, types) || result != null && (bool)result == false)
                        {
                            this.Logger.Log($"Not loaded.");
                            return;
                        }

                        Enabled = true;
                    }
                    catch (Exception exception)
                    {
                        this.Logger.Error($"Error loading file '{assemblyPath}'.");
                        this.Logger.Error(exception.Message);
                    }
                }
                else
                {
                    this.Logger.Error($"'{assemblyPath}' not found.");
                }
            }

            public bool Invoke(string namespaceClassnameMethodname, out object result)
            {
                return Invoke(namespaceClassnameMethodname, out result, null, new Type[0]);
            }

            public bool Invoke(string namespaceClassnameMethodname, out object result, object[] param, Type[] types)
            {
                result = null;
                try
                {
                    var methodInfo = FindMethod(namespaceClassnameMethodname, param, types);
                    if (methodInfo != null)
                    {
                        result = methodInfo.Invoke(null, param);
                        return true;
                    }
                }
                catch (Exception exception)
                {
                    this.Logger.Error($"Error trying to call '{namespaceClassnameMethodname}'.");
                    this.Logger.Error(exception.Message);
                }

                return false;
            }

            MethodInfo FindMethod(string namespaceClassnameMethodname, object[] param, Type[] types)
            {
                long key = namespaceClassnameMethodname.GetHashCode();
                foreach (var val in types)
                {
                    key += val.GetHashCode();
                }

                if (!mCache.TryGetValue(key, out var methodInfo))
                {
                    if (mAssembly != null)
                    {
                        string classString = null;
                        string methodString = null;
                        var pos = namespaceClassnameMethodname.LastIndexOf('.');
                        if (pos != -1)
                        {
                            classString = namespaceClassnameMethodname.Substring(0, pos);
                            methodString = namespaceClassnameMethodname.Substring(pos + 1);
                        }
                        else
                        {
                            this.Logger.Error($"Function name error '{namespaceClassnameMethodname}'.");
                            return null;
                        }

                        var type = mAssembly.GetType(classString);
                        if (type != null)
                        {
                            if (param == null && types == null)
                                types = new Type[0];

                            methodInfo = type.GetMethod(methodString, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, types, new ParameterModifier[0]);

                            if (methodInfo == null)
                                this.Logger.Error($"Method '{namespaceClassnameMethodname}' not found.");
                        }
                        else
                        {
                            this.Logger.Error($"Class '{classString}' not found.");
                        }
                    }
                    else
                    {
                        UnityModManager.Logger.Error($"Can't to call '{namespaceClassnameMethodname}'. Mod '{Info.Id}' is not loaded.");
                    }

                    mCache[key] = methodInfo;
                }

                return methodInfo;
            }

            public bool MethodHasParams(string namespaceClassnameMethodname, Type[] types)
            {
                var methodInfo = FindMethod(namespaceClassnameMethodname, null, types);
                if (methodInfo != null)
                {
                    return true;
                }

                return false;
            }

            public class ModLogger
            {
                protected readonly string Prefix;
                protected readonly string PrefixError;
                protected readonly string PrefixCritical;
                protected readonly string PrefixWarning;

                public ModLogger(string Id)
                {
                    Prefix = $"[{Id}] ";
                    PrefixError = $"[{Id}] [Error] ";
                    PrefixCritical = $"[{Id}] [Critical] ";
                    PrefixWarning = $"[{Id}] [Warning] ";
                }

                public void Log(string str)
                {
                    UnityModManager.Logger.Log(str, Prefix);
                }

                public void Error(string str)
                {
                    UnityModManager.Logger.Log(str, PrefixError);
                }

                public void Critical(string str)
                {
                    UnityModManager.Logger.Log(str, PrefixCritical);
                }

                public void Warning(string str)
                {
                    UnityModManager.Logger.Log(str, PrefixWarning);
                }
            }
        }

        public static readonly List<ModEntry> modEntries = new List<ModEntry>();
        public static readonly string modsPath = Path.Combine(Environment.CurrentDirectory, modsDirname);

        public static bool isStarted = false;

        public static void Start()
        {
            if (isStarted)
            {
                Logger.Log($"Cancel start. Already started.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine();
            Logger.Log($"Version '{version}'. Initialize.");

            if (Directory.Exists(modsPath))
            {
                Logger.Log($"Parsing mods.");

                int countMods = 0;

                foreach (string dir in Directory.GetDirectories(modsPath))
                {
                    string jsonPath = Path.Combine(dir, infoFilename);
                    if (File.Exists(jsonPath))
                    {
                        countMods++;
                        Logger.Log($"Reading file '{jsonPath}'.");
                        try
                        {
                            ModInfo modInfo = JsonUtility.FromJson<ModInfo>(File.ReadAllText(jsonPath));
                            if (string.IsNullOrEmpty(modInfo.Id))
                            {
                                Logger.Error($"Id is null.");
                                continue;
                            }
                            if (modEntries.Exists(x => x.Info.Id == modInfo.Id))
                            {
                                modInfo.Id += "." + modInfo.Author;
                                if (modEntries.Exists(x => x.Info.Id == modInfo.Id))
                                {
                                    Logger.Log($"Id '{modInfo.Id}' already uses another mod.");
                                    continue;
                                }
                            }
                            if (string.IsNullOrEmpty(modInfo.AssemblyName))
                                modInfo.AssemblyName = modInfo.Id + ".dll";

                            ModEntry modEntry = new ModEntry(modInfo, dir + Path.DirectorySeparatorChar);
                            modEntries.Add(modEntry);
                        }
                        catch (Exception exception)
                        {
                            Logger.Error($"Error parsing file '{jsonPath}'.");
                            Logger.Log(exception.Message);
                        }
                    }
                    else
                    {
                        Logger.Error($"'{jsonPath}' not found.");
                    }
                }

                if (modEntries.Count > 0)
                {
                    Logger.Log($"Sorting mods.");
                    modEntries.Sort(Compare);

                    Logger.Log($"Loading mods.");
                    foreach (var mod in modEntries)
                    {
                        mod.Load();
                    }
                }

                Logger.Log($"Finish. Found {countMods} mods. Successful loaded {modEntries.Count(x => x.Enabled)} mods.".ToUpper());
                Console.WriteLine();
                Console.WriteLine();
            }
            else
            {
                Directory.CreateDirectory(modsPath);
            }

            isStarted = true;
        }

        private static int Compare(ModEntry x, ModEntry y)
        {
            if (x.Requirements.Count > 0 && x.Requirements.ContainsKey(y.Info.Id))
            {
                return 1;
            }

            if (y.Requirements.Count > 0 && y.Requirements.ContainsKey(x.Info.Id))
            {
                return -1;
            }

            return String.Compare(x.Info.Id, y.Info.Id, StringComparison.Ordinal);
        }

        public static ModEntry FindMod(string id)
        {
            return modEntries.FirstOrDefault(x => x.Info.Id == id);
        }

        public static Version ParseVersion(string str)
        {
            var array = str.Split('.', ',');
            if (array.Length >= 3)
            {
                return new Version(int.Parse(array[0]), int.Parse(array[1]), int.Parse(array[2]));
            }

            Logger.Error($"Error parsing version {str}");
            return new Version();
        }

        public static Version GetVersion()
        {
            return ParseVersion(version);
        }

        public static class Logger
        {
            const string Prefix = "[UnityModManager] ";
            const string PrefixError = "[UnityModManager] [Error] ";
            static readonly string LogPath = Path.Combine(modsPath, "UnityModManager.log");
            static StreamWriter mWriter = null;
            static bool mIsInit = false;

            public static void Log(string str)
            {
                Log(str, Prefix);
            }

            public static void Log(string str, string prefix)
            {
                Write(prefix + str);
            }

            public static void Error(string str)
            {
                Error(str, PrefixError);
            }

            public static void Error(string str, string prefix)
            {
                Write(prefix + str);
            }

            public static void Write(string str)
            {
                Console.WriteLine(str);

                try
                {
                    using (StreamWriter writer = File.AppendText(LogPath))
                    {
                        writer.Write(str);
                    }
                }
                catch (Exception e)
                {
                }
            }
        }
    }
}
