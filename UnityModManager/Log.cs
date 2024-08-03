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
                protected readonly string PrefixException;

                public ModLogger(string Id)
                {
                    Prefix = $"[{Id}] ";
                    PrefixError = $"[{Id}] [Error] ";
                    PrefixCritical = $"[{Id}] [Critical] ";
                    PrefixWarning = $"[{Id}] [Warning] ";
                    PrefixException = $"[{Id}] [Exception] ";
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

                /// <summary>
                /// [0.17.0]
                /// </summary>
                public void LogException(string key, Exception e)
                {
                    UnityModManager.Logger.LogException(key, e, PrefixException);
                }

                /// <summary>
                /// [0.17.0]
                /// </summary>
                public void LogException(Exception e)
                {
                    UnityModManager.Logger.LogException(null, e, PrefixException);
                }
            }
        }

        public static class Logger
        {
            const string Prefix = "[Manager] ";
            const string PrefixError = "[Manager] [Error] ";
            const string PrefixException = "[Manager] [Exception] ";

            public static readonly string filepath = Path.Combine(Path.GetDirectoryName(typeof(GameInfo).Assembly.Location), "Log.txt");

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

            /// <summary>
            /// [0.17.0]
            /// </summary>
            public static void LogException(Exception e)
            {
                LogException(null, e, PrefixException);
            }

            /// <summary>
            /// [0.17.0]
            /// </summary>
            public static void LogException(string key, Exception e)
            {
                LogException(key, e, PrefixException);
            }

            /// <summary>
            /// [0.17.0]
            /// </summary>
            public static void LogException(string key, Exception e, string prefix)
            {
                if (string.IsNullOrEmpty(key))
                    Write($"{prefix}{e.GetType().Name} - {e.Message}");
                else
                    Write($"{prefix}{key}: {e.GetType().Name} - {e.Message}");
                Console.WriteLine(e.ToString());
            }
            
            private static bool hasErrors;
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
                    var result = history.Skip(historyCapacity).ToArray();
                    history.Clear();
                    history.AddRange(result);
                }
            }

            private static float timer;

            internal static void Watcher(float dt)
            {
                if (buffer.Count >= bufferCapacity || timer > 0.5f)
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
                    if (buffer.Count > 0 && !hasErrors)
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
                catch (UnauthorizedAccessException e)
                {
                    hasErrors = true;
                    Console.WriteLine(PrefixException + e.ToString());
                    Console.WriteLine(Prefix + "Uncheck the read-only box from the UnityModManager folder.");
                    history.Add(PrefixException + e.ToString());
                    history.Add(Prefix + "Uncheck the read-only box from the UnityModManager folder.");
                }
                catch (Exception e)
                {
                    hasErrors = true;
                    Console.WriteLine(PrefixException + e.ToString());
                    history.Add(PrefixException + e.ToString());
                }

                buffer.Clear();
                timer = 0;
            }

            public static void Clear()
            {
                buffer.Clear();
                history.Clear();
                if (File.Exists(filepath) && !hasErrors)
                {
                    try
                    {
                        File.Delete(filepath);
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        hasErrors = true;
                        Console.WriteLine(PrefixException + e.ToString());
                        Console.WriteLine(Prefix + "Uncheck the read-only box from the UnityModManager folder.");
                        history.Add(PrefixException + e.ToString());
                        history.Add(Prefix + "Uncheck the read-only box from the UnityModManager folder.");
                    }
                    catch (Exception e)
                    {
                        hasErrors = true;
                        Console.WriteLine(PrefixException + e.ToString());
                        history.Add(PrefixException + e.ToString());
                    }
                }
            }
        }
    }
}
