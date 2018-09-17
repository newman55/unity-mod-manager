using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UnityModManagerNet.Installer
{
    public static class Utils
    {
        public static Version ParseVersion(string str)
        {
            var array = str.Split('.', ',');
            if (array.Length >= 3)
            {
                var regex = new Regex(@"\D");
                return new Version(int.Parse(regex.Replace(array[0], "")), int.Parse(regex.Replace(array[1], "")), int.Parse(regex.Replace(array[2], "")));
            }

            Log.Print($"Error parsing version '{str}'.");
            return new Version();
        }
    }
}
