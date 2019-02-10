using System;
using System.IO;

namespace UnityModManagerNet.Installer
{
    static class Log
    {
        private static bool firstLine = true;
        public const string fileLog = "Log.txt";

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

        public static void Append(string str)
        {
            Print(str, true);
        }

        public static void Init()
        {
            File.Delete(fileLog);
        }
    }
}
