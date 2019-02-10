using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UnityModManagerNet
{
    public partial class UnityModManager
    {
        public static void OpenUnityFileLog()
        {
            var folders = new string[] { Application.persistentDataPath, Application.dataPath };
            foreach(var folder in folders)
            {
                var filepath = Path.Combine(folder, "output_log.txt");
                if (File.Exists(filepath))
                {
                    Application.OpenURL(filepath);
                    return;
                }
            }
        }

        public static Version ParseVersion(string str)
        {
            var array = str.Split('.', ',');
            if (array.Length >= 3)
            {
                var regex = new Regex(@"\D");
                return new Version(int.Parse(regex.Replace(array[0], "")), int.Parse(regex.Replace(array[1], "")), int.Parse(regex.Replace(array[2], "")));
            }

            Logger.Error($"Error parsing version {str}");
            return new Version();
        }

        public static bool IsUnixPlatform()
        {
            int p = (int)Environment.OSVersion.Platform;
            return (p == 4) || (p == 6) || (p == 128);
        }
    }
}
