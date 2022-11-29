using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Net;
using System.Net.NetworkInformation;
using UnityModManagerNet.ConsoleInstaller;

namespace UnityModManagerNet.Installer
{
    public partial class UnityModManagerForm : Form
    {
        readonly Dictionary<GameInfo, HashSet<UnityModManager.Repository.Release>> repositories = new Dictionary<GameInfo, HashSet<UnityModManager.Repository.Release>>();

        private void CheckModUpdates()
        {
            if (selectedGame == null)
                return;

            if (!HasNetworkConnection())
            {
                return;
            }

            if (!repositories.ContainsKey(selectedGame))
                repositories.Add(selectedGame, new HashSet<UnityModManager.Repository.Release>());

            var urls = new HashSet<string>();
            foreach (var mod in mods)
            {
                if (!string.IsNullOrEmpty(mod.Repository))
                {
                    urls.Add(mod.Repository);
                }
            }

            if (urls.Count > 0)
            {
                foreach (var url in urls)
                {
                    try
                    {
                        using (var wc = new WebClient())
                        {
                            wc.Encoding = System.Text.Encoding.UTF8;
                            wc.DownloadStringCompleted += (sender, e) => { ModUpdates_DownloadStringCompleted(sender, e, selectedGame, url); };
                            wc.DownloadStringAsync(new Uri(url));
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Print(e.Message);
                        Log.Print($"Error checking updates on '{url}'.");
                    }
                }
            }
        }

        private void ModUpdates_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e, GameInfo game, string url)
        {
            if (e.Error != null)
            {
                Log.Print(e.Error.Message);
                return;
            }

            if (!e.Cancelled && !string.IsNullOrEmpty(e.Result) && repositories.ContainsKey(game))
            {
                try
                {
                    var repository = JsonConvert.DeserializeObject<UnityModManager.Repository>(e.Result);
                    if (repository == null || repository.Releases == null || repository.Releases.Length == 0)
                        return;

                    listMods.Invoke((MethodInvoker)delegate
                    {
                        foreach(var v in repository.Releases)
                        {
                            repositories[game].Add(v);
                        }
                        if (selectedGame == game)
                            RefreshModList();
                    });
                }
                catch (Exception ex)
                {
                    Log.Print(ex.Message);
                    Log.Print($"Error checking updates on '{url}'.");
                }
            }
        }

        private void CheckLastVersion()
        {
            if (string.IsNullOrEmpty(config.Repository))
                return;

            Log.Print("Checking for updates.");

            if (!HasNetworkConnection())
            {
                Log.Print("No network connection or firewall blocked.");
                return;
            }

            try
            {
                using (var wc = new WebClient())
                {
                    wc.Encoding = System.Text.Encoding.UTF8;
                    wc.DownloadStringCompleted += LastVersion_DownloadStringCompleted;
                    wc.DownloadStringAsync(new Uri(config.Repository));
                }
            }
            catch (Exception e)
            {
                Log.Print(e.Message);
                Log.Print($"Error checking update.");
            }
        }

        private void LastVersion_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Log.Print(e.Error.Message);
                return;
            }

            if (!e.Cancelled && !string.IsNullOrEmpty(e.Result))
            {
                try
                {
                    var repository = JsonConvert.DeserializeObject<UnityModManager.Repository>(e.Result);
                    if (repository == null || repository.Releases == null || repository.Releases.Length == 0)
                        return;

                    var release = repository.Releases.FirstOrDefault(x => x.Id == nameof(UnityModManager));
                    if (release != null && !string.IsNullOrEmpty(release.Version))
                    {
                        var ver = Utils.ParseVersion(release.Version);
                        if (version < ver)
                        {
                            //btnDownloadUpdate.Visible = true;
                            btnDownloadUpdate.Text = $"Download {release.Version}";
                            Log.Print($"Update is available.");
                        }
                        else
                        {
                            Log.Print($"No updates.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Print(ex.Message);
                    Log.Print($"Error checking update.");
                }
            }
        }

        public static bool HasNetworkConnection()
        {
            try
            {
                using (var ping = new Ping())
                {
                    return ping.Send("8.8.8.8", 3000).Status == IPStatus.Success;
                }
            }
            catch (Exception e)
            {
                Log.Print(e.Message);
            }

            return false;
        }
    }
}