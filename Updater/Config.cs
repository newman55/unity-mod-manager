using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace UnityModManagerNet.Downloader
{
    public class Config
    {
        public string Repository;
    }

    public class Repository
    {
        [Serializable]
        public class Release
        {
            public string Id;
            public string Version;
            public string DownloadUrl;
        }

        public Release[] Releases;
    }
}
