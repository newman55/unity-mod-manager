using System;
using System.IO;

namespace UnityModManagerNet.Installer
{
    static class Log
    {
        private static bool firstLine = true;
        public const string fileLog = "Log.txt";
        static StreamWriter stream;

        public static void Print(string str, bool append = false)
        {
            if (append)
            {
                UnityModManagerForm.instance.statusLabel.Text += str;
                UnityModManagerForm.instance.statusLabel.ToolTipText += str;
            }
            else
            {
                UnityModManagerForm.instance.statusLabel.Text = str;
                UnityModManagerForm.instance.statusLabel.ToolTipText = str;
                if (firstLine)
                {
                    firstLine = false;
                    str = $"[{DateTime.Now.ToShortTimeString()}] {str}";
                }
                else
                {
                    str = $"\r\n[{DateTime.Now.ToShortTimeString()}] {str}";
                }
            }

            stream?.Write(str);
            UnityModManagerForm.instance.inputLog.AppendText(str);
        }

        public static void Append(string str)
        {
            Print(str, true);
        }

        public static void Init()
        {
            try
            {
                File.Delete(fileLog);
                stream = new StreamWriter(new FileStream(fileLog, FileMode.OpenOrCreate, FileAccess.Write));
                stream.AutoFlush = true;
            }
            catch (UnauthorizedAccessException)
            {
                Print("Write error, insufficient permissions. Try to run app as administrator.");
            }
        }
    }
}
