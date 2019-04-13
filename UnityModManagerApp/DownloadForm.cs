using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UnityModManagerNet.Installer
{
    public partial class DownloadForm : Form
    {
        public UnityModManager.Repository.Release release;
        public string tempFilepath { get; private set; }

        public DownloadForm()
        {
            InitializeComponent();
        }

        public DownloadForm(UnityModManager.Repository.Release release)
        {
            this.release = release;
            InitializeComponent();
            Start();
        }

        private void Start()
        {
            try
            {
                var dir = Path.Combine(Path.GetTempPath(), "UnityModManager");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                tempFilepath = Path.Combine(dir, $"{release.Id}.zip");

                status.Text = $"Downloading {release.Id} {release.Version} ...";

                using (var wc = new WebClient())
                {
                    wc.Encoding = Encoding.UTF8;
                    wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                    wc.DownloadFileCompleted += Wc_DownloadFileCompleted;
                    wc.DownloadFileAsync(new Uri(release.DownloadUrl), tempFilepath);
                }
            }
            catch (Exception e)
            {
                status.Text = e.Message;
                Log.Print(e.Message);
            }
        }

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void Wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                status.Text = e.Error.Message;
                Log.Print(e.Error.Message);
                return;
            }
            if (!e.Cancelled)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
