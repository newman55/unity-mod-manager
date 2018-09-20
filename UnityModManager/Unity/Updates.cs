using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine;

namespace UnityModManagerNet
{
    public partial class UnityModManager
    {
        private static readonly object mLock = new object();
        private static readonly List<string> mJsonResult = new List<string>();

        private static void CheckModUpdates()
        {
            if (ServicePointManager.ServerCertificateValidationCallback == null)
                ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;

            if (!HasNetworkConnection())
            {
                Logger.Log("No network connection.");
                return;
            }

            var urls = new HashSet<string>();

            foreach (var modEntry in modEntries)
            {
                if (!string.IsNullOrEmpty(modEntry.Info.Repository))
                {
                    urls.Add(modEntry.Info.Repository);
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
                            wc.Encoding = Encoding.UTF8;
                            wc.DownloadStringCompleted += ModUpdates_DownloadStringCompleted;
                            wc.DownloadStringAsync(new Uri(url));
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log(string.Format("Error checking mod updates on '{0}'.", url));
                        Logger.Log(e.Message);
                    }
                }
            }
        }

        private static void ModUpdates_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Logger.Log(e.Error.Message);
                return;
            }

            if (!e.Cancelled && !string.IsNullOrEmpty(e.Result))
            {
                lock (mLock)
                {
                    mJsonResult.Add(e.Result);
                }
            }
        }

        private static void ParseJsonResult()
        {
            lock (mLock)
            {
                foreach (var str in mJsonResult)
                {
                    try
                    {
                        var repository = JsonUtility.FromJson<Repository>(str);
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
                        Logger.Log("Error parsing mod updates.");
                        Logger.Log(e.Message);
                    }
                }

                mJsonResult.Clear();
            }
        }

        public static bool RemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool isOk = true;
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                for (int i = 0; i < chain.ChainStatus.Length; i++)
                {
                    if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown)
                    {
                        continue;
                    }

                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build((X509Certificate2)certificate);
                    if (!chainIsValid)
                    {
                        isOk = false;
                        break;
                    }
                }
            }

            return isOk;
        }

        public static bool HasNetworkConnection()
        {
            try
            {
                using (var ping = new System.Net.NetworkInformation.Ping())
                {
                    return ping.Send("www.google.com.mx", 1000).Status == IPStatus.Success;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return false;
        }
    }
}
