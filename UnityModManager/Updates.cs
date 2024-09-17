using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using static UnityModManagerNet.UnityModManager;

namespace UnityModManagerNet
{
    public partial class UnityModManager
    {
        public class NexusModInfo
        {
            public string name;
            public int mod_id;
            public string domain_name;
            public string version;
        }

        private static void CheckModUpdates()
        {
            Logger.Log("Checking updates.");

            if (!HasNetworkConnection())
            {
                Logger.Log("No network connection or firewall blocked.");
                return;
            }

            Params.LastUpdateCheck = DateTime.Now; 

            var urls = new HashSet<string>();
            var nexusUrls = new HashSet<string>();

            foreach (var modEntry in modEntries)
            {
                if (!string.IsNullOrEmpty(modEntry.Info.Repository))
                {
                    urls.Add(modEntry.Info.Repository);
                }

                if (!string.IsNullOrEmpty(modEntry.Info.HomePage) && ParseNexusUrl(modEntry.Info.HomePage, out string nexusGame, out string nexusModId))
                {
                    nexusUrls.Add(modEntry.Info.HomePage);
                }
            }

            if (urls.Count > 0)
            {
                foreach (var url in urls)
                {
                    if (unityVersion < new Version(5, 4))
                    {
                        UI.Instance.StartCoroutine(DownloadString_5_3(url, ParseRepository));
                    }
                    else
                    {
                        UI.Instance.StartCoroutine(DownloadString(url, ParseRepository));
                    }
                }
            }

            if (nexusUrls.Count > 0)
            {
                foreach (var url in nexusUrls)
                {
                    if (unityVersion >= new Version(5, 4))
                    {
                        UI.Instance.StartCoroutine(DownloadString(url, ParseNexus));
                    }
                }
            }
        }

        private static void ParseRepository(string json, string url)
        {
            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            try
            {
                var repository = TinyJson.JSONParser.FromJson<Repository>(json);
                if (repository != null && repository.Releases != null && repository.Releases.Length > 0)
                {
                    foreach (var release in repository.Releases)
                    {
                        if (!string.IsNullOrEmpty(release.Id) && !string.IsNullOrEmpty(release.Version))
                        {
                            var modEntry = FindMod(release.Id);
                            if (modEntry != null)
                            {
                                var ver = ParseVersion(release.Version);
                                if (modEntry.Version < ver && (modEntry.NewestVersion == null || modEntry.NewestVersion < ver))
                                {
                                    modEntry.NewestVersion = ver;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log(string.Format("Error checking mod updates on '{0}'.", url));
                Logger.Log(e.Message);
            }
        }

        private static void ParseNexus(string json, string url)
        {
            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            try
            {
                var result = TinyJson.JSONParser.FromJson<NexusModInfo>(json);
                if (result != null && !string.IsNullOrEmpty(result.version))
                {
                    var mod = modEntries.Find((x) => x.Info.HomePage == url);
                    if (mod != null)
                    {
                        var ver = ParseVersion(result.version);
                        if (mod.Version < ver && (mod.NewestVersion == null || mod.NewestVersion < ver))
                        {
                            mod.NewestVersion = ver;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log(string.Format("Error checking mod updates on '{0}'.", url));
                Logger.Log(e.Message);
            }
        }

        public static bool HasNetworkConnection()
        {
            //try
            //{
            //    using (var ping = new System.Net.NetworkInformation.Ping())
            //    {
            //        return ping.Send("8.8.8.8", 3000).Status == IPStatus.Success;
            //    }
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //}

            try
            {
                var timeout = 2000;
                var ping = new UnityEngine.Ping("8.8.8.8");
                while (!ping.isDone)
                {
                    Thread.Sleep(10);
                    timeout -= 10;
                    if (timeout <= 0)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return false;
        }

        private static bool nexusApiSupportLogged;

        private static IEnumerator DownloadString(string url, UnityAction<string, string> handler)
        {
            var orgUrl = url;
            var www = UnityWebRequest.Get(url);
            if (ParseNexusUrl(url, out string nexusGame, out string nexusModId))
            {
                if (string.IsNullOrEmpty(InstallerParams.APIkey))
                {
                    if (!nexusApiSupportLogged)
                    {
                        nexusApiSupportLogged = true;
                        Logger.Log($"The nexus api key is missing. Without it, you won't be able to check for updates. You can configure it via the UnityModManager Installer. If you don't need it, just ignore.");
                    }
                    yield break;
                }
                url = $"https://api.nexusmods.com/v1/games/{nexusGame}/mods/{nexusModId}.json";
                www = UnityWebRequest.Get(url);
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("apikey", InstallerParams.APIkey);
                www.SetRequestHeader("Application-Version", version.ToString());
                www.SetRequestHeader("Application-Name", "UnityModManager");
            }
            yield return www.Send();

            MethodInfo isError;
            var ver = ParseVersion(Application.unityVersion);
            if (ver.Major >= 2017)
            {
                isError = typeof(UnityWebRequest).GetMethod("get_isNetworkError");
            }
            else
            {
                isError = typeof(UnityWebRequest).GetMethod("get_isError");
            }

            if (isError == null || (bool)isError.Invoke(www, null))
            {
                Logger.Log(www.error);
                Logger.Log(string.Format("Error downloading '{0}'.", url));
                yield break;
            }
            handler(www.downloadHandler.text, orgUrl);
        }

        private static IEnumerator DownloadString_5_3(string url, UnityAction<string, string> handler)
        {
            var www = new WWW(url);
            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                Logger.Log(www.error);
                Logger.Log(string.Format("Error downloading '{0}'.", url));
                yield break;
            }

            handler(www.text, url);
        }
    }
}
