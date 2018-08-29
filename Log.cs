using System;
using System.IO;

namespace UnityModManagerNet.Installer
{
    static class Log
    {
        public const string fileLog = "UnityModManager.log";

        public static void Print(string str)
        {
            str = $"[{DateTime.Now.ToShortTimeString()}] {str}\r\n";

            UnityModManagerForm.instance.inputLog.AppendText(str);

            try
            {
                using (StreamWriter writer = File.AppendText(fileLog))
                {
                    writer.Write(str);
                }
            }
            catch (Exception e)
            {
            }
        }

        public static void Init()
        {
            File.Delete(fileLog);
        }
    }
}
