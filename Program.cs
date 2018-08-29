using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace UnityModManagerNet.Installer
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UnityModManagerForm());
        }
    }
}
