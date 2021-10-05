using System;
using System.IO;

namespace UnityModManagerNet.Installer
{
    class Log : ConsoleInstaller.Log
    {
        public override void Write(string str, bool append = false)
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
            stream?.Write(str);
        }
    }
}
