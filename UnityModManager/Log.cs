using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UnityModManagerNet
{
    public partial class UnityModManager
    {
        public partial class ModEntry
        {
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

                public void NativeLog(string str)
                {
                    UnityModManager.Logger.NativeLog(str, Prefix);
                }
            }
        }

        public static class Logger
        {
            const string Prefix = "[Manager] ";
            const string PrefixError = "[Manager] [Error] ";

            public static readonly string filepath = Path.Combine(Path.Combine(Application.dataPath, Path.Combine("Managed", nameof(UnityModManager))), "Log.txt");

            public static void NativeLog(string str)
            {
                NativeLog(str, Prefix);
            }

            public static void NativeLog(string str, string prefix)
            {
                Write(prefix + str, true);
            }

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

            private static int bufferCapacity = 100;
            private static List<string> buffer = new List<string>(bufferCapacity);
            internal static int historyCapacity = 200;
            internal static List<string> history = new List<string>(historyCapacity * 2);

            private static void Write(string str, bool onlyNative = false)
            {
                if (str == null)
                    return;

                Console.WriteLine(str);

                if (onlyNative)
                    return;

                buffer.Add(str);
                history.Add(str);

                if (history.Count >= historyCapacity * 2)
                {
                    var result = history.Skip(historyCapacity);
                    history.Clear();
                    history.AddRange(result);
                }
            }

            private static float timer;

            internal static void Watcher(float dt)
            {
                if (buffer.Count >= bufferCapacity || timer > 1f)
                {
                    WriteBuffers();
                }
                else
                {
                    timer += dt;
                }
            }

            internal static void WriteBuffers()
            {
                try
                {
                    if (buffer.Count > 0)
                    {
                        if (!File.Exists(filepath))
                        {
                            using (File.Create(filepath))
                            {; }
                        }
                        using (StreamWriter writer = File.AppendText(filepath))
                        {
                            foreach (var str in buffer)
                            {
                                writer.WriteLine(str);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                buffer.Clear();
                timer = 0;
            }

            public static void Clear()
            {
                buffer.Clear();
                history.Clear();
                if (File.Exists(filepath))
                {
                    try
                    {
                        File.Delete(filepath);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }
    }
}
