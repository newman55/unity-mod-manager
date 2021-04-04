using System;
using System.Windows.Forms;

namespace UnityModManagerNet.Downloader
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
#if NETCOREAPP3_1_OR_GREATER
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
#endif
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DownloaderForm());
        }
    }
}
