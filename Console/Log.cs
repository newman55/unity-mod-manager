using System;
using System.IO;

namespace UnityModManagerNet.ConsoleInstaller
{
    public class Log
    {
        public static Log Instance { get; set; }
        protected static bool firstLine = true;
        public const string fileLog = "Log.txt";
        protected static StreamWriter stream;

        public static void Print(string str, bool append = false)
        {
            Instance.Write(str, append);
        }

        public static void Append(string str)
        {
            Print(str, true);
        }

        public static void Init<T>() where T: Log
        {
            try
            {
                Instance = Activator.CreateInstance<T>();
                File.Delete(fileLog);
                stream = new StreamWriter(new FileStream(fileLog, FileMode.OpenOrCreate, FileAccess.Write));
                stream.AutoFlush = true;
            }
            catch (UnauthorizedAccessException)
            {
                Print("Write error, insufficient permissions. Try to run app as administrator.");
            }
        }

        public virtual void Write(string str, bool append = false)
        {
            if (append)
            {
                Console.Write(str);
            }
            else
            {
                if (firstLine)
                {
                    firstLine = false;
                    str = $"[{DateTime.Now.ToShortTimeString()}] {str}";
                }
                else
                {
                    str = $"\r\n[{DateTime.Now.ToShortTimeString()}] {str}";
                }
                Console.Write(str);
            }

            stream?.Write(str);
        }
    }
}
