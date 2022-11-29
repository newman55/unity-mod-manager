using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace UnityModManagerNet.Downloader
{
    static class Utils
    {
        public static Version ParseVersion(string str)
        {
            var array = str.Split('.', ',');
            if (array.Length >= 3)
            {
                var regex = new Regex(@"\D");
                return new Version(int.Parse(regex.Replace(array[0], "")), int.Parse(regex.Replace(array[1], "")), int.Parse(regex.Replace(array[2], "")));
            }

            return new Version();
        }

        public static bool HasNetworkConnection()
        {
            try
            {
                using (var ping = new Ping())
                {
                    return ping.Send("8.8.8.8", 2000).Status == IPStatus.Success;
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        public static bool IsUnixPlatform()
        {
            int p = (int)Environment.OSVersion.Platform;
            return (p == 4) || (p == 6) || (p == 128);
        }
    }
}
