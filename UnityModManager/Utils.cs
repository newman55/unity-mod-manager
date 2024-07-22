using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace UnityModManagerNet
{
    public partial class UnityModManager
    {
        public static void OpenUnityFileLog()
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                var folders = new string[] { Application.persistentDataPath, Application.dataPath };
                var files = new string[] { "Player.log", "output_log.txt" };
                foreach (var folder in folders)
                {
                    foreach (var file in files)
                    {
                        var filepath = Path.Combine(folder, file);
                        if (File.Exists(filepath))
                        {
                            Thread.Sleep(500);
                            Application.OpenURL(filepath);
                            return;
                        }
                    }
                }
            }).Start();
        }

        public static Version ParseVersion(string str)
        {
            var array = str.Split('.');
            if (array.Length >= 4)
            {
                var regex = new Regex(@"\D");
                return new Version(int.Parse(regex.Replace(array[0], "")), int.Parse(regex.Replace(array[1], "")), int.Parse(regex.Replace(array[2], "")), int.Parse(regex.Replace(array[3], "")));
            }
            else if (array.Length >= 3)
            {
                var regex = new Regex(@"\D");
                return new Version(int.Parse(regex.Replace(array[0], "")), int.Parse(regex.Replace(array[1], "")), int.Parse(regex.Replace(array[2], "")));
            }
            else if (array.Length >= 2)
            {
                var regex = new Regex(@"\D");
                return new Version(int.Parse(regex.Replace(array[0], "")), int.Parse(regex.Replace(array[1], "")));
            }
            else if (array.Length >= 1)
            {
                var regex = new Regex(@"\D");
                return new Version(int.Parse(regex.Replace(array[0], "")), 0);
            }

            Logger.Error($"Error parsing version {str}");
            return new Version();
        }

        public static bool IsUnixPlatform()
        {
            int p = (int)Environment.OSVersion.Platform;
            return (p == 4) || (p == 6) || (p == 128);
        }

        public static bool IsMacPlatform()
        {
            int p = (int)Environment.OSVersion.Platform;
            return (p == 6);
        }

        public static bool IsLinuxPlatform()
        {
            int p = (int)Environment.OSVersion.Platform;
            return (p == 4) || (p == 128);
        }
    }

    /// <summary>
    /// [0.18.0]
    /// </summary>
    public interface ICopyable
    {
    }

    /// <summary>
    /// [0.18.0]
    /// </summary>
    [Flags]
    public enum CopyFieldMask { Any = 0, Matching = 1, Public = 2, Serialized = 4, SkipNotSerialized = 8, OnlyCopyAttr = 16 };

    /// <summary>
    /// [0.18.0]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class CopyFieldsAttribute : Attribute
    {
        public CopyFieldMask Mask;

        public CopyFieldsAttribute(CopyFieldMask Mask)
        {
            this.Mask = Mask;
        }
    }

    /// <summary>
    /// [0.18.0]
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class CopyAttribute : Attribute
    {
        public string Alias;

        public CopyAttribute()
        {
        }

        public CopyAttribute(string Alias)
        {
            this.Alias = Alias;
        }
    }

    public static partial class Extensions
    {
        /// <summary>
        /// [0.18.0]
        /// </summary>
        public static void CopyFieldsTo<T1,T2>(this T1 from, ref T2 to) 
            where T1 : ICopyable, new()
            where T2 : new()
        {
            object obj = to;
            Utils.CopyFields<T1,T2>(from, obj, CopyFieldMask.OnlyCopyAttr);
            to = (T2)obj;
        }
    }

    public static partial class Utils
    {
        /// <summary>
        /// [0.18.0]
        /// </summary>
        public static void CopyFields<T1, T2>(object from, object to, CopyFieldMask defaultMask)
            where T1 : new()
            where T2 : new()
        {
            CopyFieldMask mask = defaultMask;
            foreach (CopyFieldsAttribute attr in typeof(T1).GetCustomAttributes(typeof(CopyFieldsAttribute), false))
            {
                mask = attr.Mask;
            }

            var fields = typeof(T1).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var f in fields)
            {
                CopyAttribute a = new CopyAttribute();
                var attributes = f.GetCustomAttributes(typeof(CopyAttribute), false);
                if (attributes.Length > 0)
                {
                    foreach (CopyAttribute a_ in attributes)
                    {
                        a = a_;
                    }
                }
                else
                {
                    if ((mask & CopyFieldMask.OnlyCopyAttr) == 0 && ((mask & CopyFieldMask.SkipNotSerialized) == 0 || !f.IsNotSerialized)
                        && ((mask & CopyFieldMask.Public) > 0 && f.IsPublic
                        || (mask & CopyFieldMask.Serialized) > 0 && f.GetCustomAttributes(typeof(SerializeField), false).Length > 0
                        || (mask & CopyFieldMask.Public) == 0 && (mask & CopyFieldMask.Serialized) == 0))
                    {
                    }
                    else
                    {
                        continue;
                    }
                }

                if (string.IsNullOrEmpty(a.Alias))
                    a.Alias = f.Name;

                var f2 = typeof(T2).GetField(a.Alias, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (f2 == null)
                {
                    if ((mask & CopyFieldMask.Matching) == 0)
                        UnityModManager.Logger.Error($"Field '{typeof(T2).Name}.{a.Alias}' not found");
                    continue;
                }
                if (f.FieldType != f2.FieldType)
                {
                    UnityModManager.Logger.Error($"Fields '{typeof(T1).Name}.{f.Name}' and '{typeof(T2).Name}.{f2.Name}' have different types");
                    continue;
                }
                f2.SetValue(to, f.GetValue(from));
            }
        }
    }
}
