
using System.Text.RegularExpressions;

namespace UnityModManagerNet.Installer
{
    public class Utils : ConsoleInstaller.Utils
    {
        public static bool ParseNexusUrl(string url, out string game, out string id)
        {
            game = null;
            id = null;
            var regex = new Regex(@"https:\/\/www\.nexusmods\.com\/(\w+)\/mods\/(\d+)", RegexOptions.IgnoreCase);
            var matches = regex.Matches(url);
            foreach (Match match in matches)
            {
                game = match.Groups[1].Value;
                id = match.Groups[2].Value;
                return true;
            }
            return false;
        }
    }
}
