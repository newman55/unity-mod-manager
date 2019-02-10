using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using Ionic.Zip;
using Newtonsoft.Json;

namespace UnityModManagerNet.Downloader
{
    public partial class DownloaderForm : Form
    {
        const string updateFile = "update.zip";
        const string configFile = "UnityModManagerConfig.xml";
        const string managerName = "UnityModManager";
        const string managerFile = "UnityModManager.dll";
        const string managerAppName = "UnityModManager";
        const string managerAppFile = "UnityModManager.exe";

        public DownloaderForm()
        {
            InitializeComponent();
            Start();
        }

        public void Start()
        {
            //string[] args = Environment.GetCommandLineArgs();
            //if (args.Length <= 1 || string.IsNullOrEmpty(args[1]))
            //    return;

            if (!Utils.HasNetworkConnection())
            {
                status.Text = $"No network connection.";
                return;
            }

            try
            {
                Config config;
                using (var stream = File.OpenRead(configFile))
                {
                    var serializer = new XmlSerializer(typeof(Config));
                    config = serializer.Deserialize(stream) as Config;
                }
                if (config == null || string.IsNullOrEmpty(config.Repository))
                {
                    status.Text = $"Error parsing '{configFile}'.";
                    return;
                }
                if (File.Exists(updateFile))
                {
                    File.Delete(updateFile);
                }
                
                string result = null;
                using (var wc = new WebClient())
                {
                    wc.Encoding = Encoding.UTF8;
                    result = wc.DownloadString(new Uri(config.Repository));
                }
                var repository = JsonConvert.DeserializeObject<Repository>(result);
                if (repository == null || repository.Releases.Length == 0)
                {
                    status.Text = $"Error parsing '{config.Repository}'.";
                    return;
                }
                var release = repository.Releases.FirstOrDefault(x => x.Id == managerName);
                if (File.Exists(managerFile))
                {
                    var managerAssembly = Assembly.ReflectionOnlyLoad(File.ReadAllBytes(managerFile));
                    if (Utils.ParseVersion(release.Version) <= managerAssembly.GetName().Version)
                    {
                        status.Text = $"No updates.";
                        return;
                    }
                }
                status.Text = $"Downloading {release.Version} ...";
                using (var wc = new WebClient())
                {
                    wc.Encoding = Encoding.UTF8;
                    wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                    wc.DownloadFileCompleted += Wc_DownloadFileCompleted;
                    wc.DownloadFileAsync(new Uri(release.DownloadUrl), updateFile);
                }
            }
            catch (Exception e)
            {
                status.Text = e.Message;
            }
        }

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void Wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                status.Text = e.Error.Message;
                return;
            }
            if (!e.Cancelled)
            {
                var success = false;
                try
                {
                    foreach (var p in Process.GetProcessesByName(managerAppName))
                    {
                        status.Text = "Waiting for the UnityModManager to close.";
                        p.CloseMainWindow();
                        p.WaitForExit();
                    }
                    using (var zip = ZipFile.Read(updateFile))
                    {
                        foreach (var entry in zip.EntriesSorted)
                        {
                            if (entry.IsDirectory)
                            {
                                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, entry.FileName));
                            }
                            else
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(Environment.CurrentDirectory, entry.FileName)));
                                using (FileStream fs = new FileStream(Path.Combine(Environment.CurrentDirectory, entry.FileName), FileMode.Create, FileAccess.Write))
                                {
                                    entry.Extract(fs);
                                }
                            }
                        }
                    }
                    status.Text = "Done.";
                    success = true;
                }
                catch (Exception ex)
                {
                    status.Text = ex.Message;
                }

                if (File.Exists(updateFile))
                {
                    File.Delete(updateFile);
                }

                if (success)
                {
                    if (!Utils.IsUnixPlatform() && Process.GetProcessesByName(managerAppName).Length == 0)
                    {
                        if (File.Exists(managerAppFile))
                        {
                            SetForegroundWindow(Process.Start(managerAppFile).MainWindowHandle);
                        }
                    }
                    Application.Exit();
                }
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hwnd);
    }
}
