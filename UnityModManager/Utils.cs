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
            var folders = new string[] { Application.persistentDataPath, Application.dataPath };
            foreach(var folder in folders)
            {
                var filepath = Path.Combine(folder, "output_log.txt");
                if (File.Exists(filepath))
                {
                    Thread.Sleep(500);
                    Application.OpenURL(filepath);
                    return;
                }
            }
        }

        public static Version ParseVersion(string str)
        {
            var array = str.Split('.');
            if (array.Length >= 3)
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
        /// Only works on ARGB32, RGB24 and Alpha8 textures that are marked readable
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="size"></param>
        public static void ResizeToIfLess(this Texture2D tex, int size)
        {
            float target = size;
            var t = Mathf.Max(tex.width / target, tex.height / target);
            if (t < 1f)
            {
                TextureScale.Bilinear(tex, (int)(tex.width / t), (int)(tex.height / t));
            }
        }

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

    public static class TextureScale
    {
        public class ThreadData
        {
            public int start;
            public int end;

            public ThreadData(int s, int e)
            {
                start = s;
                end = e;
            }
        }

        private static Color[] texColors;
        private static Color[] newColors;
        private static int w;
        private static float ratioX;
        private static float ratioY;
        private static int w2;
        private static int finishCount;
        private static Mutex mutex;

        public static void Point(Texture2D tex, int newWidth, int newHeight)
        {
            ThreadedScale(tex, newWidth, newHeight, false);
        }

        public static void Bilinear(Texture2D tex, int newWidth, int newHeight)
        {
            ThreadedScale(tex, newWidth, newHeight, true);
        }

        private static void ThreadedScale(Texture2D tex, int newWidth, int newHeight, bool useBilinear)
        {
            texColors = tex.GetPixels();
            newColors = new Color[newWidth * newHeight];
            if (useBilinear)
            {
                ratioX = 1.0f / ((float)newWidth / (tex.width - 1));
                ratioY = 1.0f / ((float)newHeight / (tex.height - 1));
            }
            else
            {
                ratioX = ((float)tex.width) / newWidth;
                ratioY = ((float)tex.height) / newHeight;
            }
            w = tex.width;
            w2 = newWidth;
            var cores = Mathf.Min(SystemInfo.processorCount, newHeight);
            var slice = newHeight / cores;

            finishCount = 0;
            if (mutex == null)
            {
                mutex = new Mutex(false);
            }
            if (cores > 1)
            {
                int i = 0;
                ThreadData threadData;
                for (i = 0; i < cores - 1; i++)
                {
                    threadData = new ThreadData(slice * i, slice * (i + 1));
                    ParameterizedThreadStart ts =
                        useBilinear ? new ParameterizedThreadStart(BilinearScale) : new ParameterizedThreadStart(PointScale);
                    Thread thread = new Thread(ts);
                    thread.Start(threadData);
                }
                threadData = new ThreadData(slice * i, newHeight);
                if (useBilinear)
                {
                    BilinearScale(threadData);
                }
                else
                {
                    PointScale(threadData);
                }
                while (finishCount < cores)
                {
                    Thread.Sleep(1);
                }
            }
            else
            {
                ThreadData threadData = new ThreadData(0, newHeight);
                if (useBilinear)
                {
                    BilinearScale(threadData);
                }
                else
                {
                    PointScale(threadData);
                }
            }

            tex.Resize(newWidth, newHeight);
            tex.SetPixels(newColors);
            tex.Apply();

            texColors = null;
            newColors = null;
        }

        public static void BilinearScale(System.Object obj)
        {
            ThreadData threadData = (ThreadData)obj;
            for (var y = threadData.start; y < threadData.end; y++)
            {
                int yFloor = (int)Mathf.Floor(y * ratioY);
                var y1 = yFloor * w;
                var y2 = (yFloor + 1) * w;
                var yw = y * w2;

                for (var x = 0; x < w2; x++)
                {
                    int xFloor = (int)Mathf.Floor(x * ratioX);
                    var xLerp = x * ratioX - xFloor;
                    newColors[yw + x] = ColorLerpUnclamped(ColorLerpUnclamped(texColors[y1 + xFloor], texColors[y1 + xFloor + 1], xLerp),
                        ColorLerpUnclamped(texColors[y2 + xFloor], texColors[y2 + xFloor + 1], xLerp), y * ratioY - yFloor);
                }
            }

            mutex.WaitOne();
            finishCount++;
            mutex.ReleaseMutex();
        }

        public static void PointScale(System.Object obj)
        {
            ThreadData threadData = (ThreadData)obj;
            for (var y = threadData.start; y < threadData.end; y++)
            {
                var thisY = (int)(ratioY * y) * w;
                var yw = y * w2;
                for (var x = 0; x < w2; x++)
                {
                    newColors[yw + x] = texColors[(int)(thisY + ratioX * x)];
                }
            }

            mutex.WaitOne();
            finishCount++;
            mutex.ReleaseMutex();
        }

        private static Color ColorLerpUnclamped(Color c1, Color c2, float value)
        {
            return new Color(c1.r + (c2.r - c1.r) * value, c1.g + (c2.g - c1.g) * value, c1.b + (c2.b - c1.b) * value, c1.a + (c2.a - c1.a) * value);
        }
    }
}
