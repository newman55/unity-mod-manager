using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Ionic.Zip;

namespace UnityModManagerNet.Installer
{
    public partial class UnityModManagerForm : Form
    {
        List<ModInfo> mods = new List<ModInfo>();

        private void InitPageMods()
        {
            splitContainerMods.Panel2.AllowDrop = true;
            splitContainerMods.Panel2.DragEnter += new DragEventHandler(Mods_DragEnter);
            splitContainerMods.Panel2.DragDrop += new DragEventHandler(Mods_DragDrop);
        }

        private void Mods_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void Mods_DragDrop(object sender, DragEventArgs e)
        {
            var modsPath = Path.Combine(Application.StartupPath, selectedGame.Name);
            var installedList = new List<ModInfo>();
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string filepath in files)
            {
                try
                {
                    if (Path.GetExtension(filepath) == ".zip")
                    {
                        using (var zip = ZipFile.Read(filepath))
                        {
                            InstallMod(zip, false);
                            var modInfo = ReadModInfoFromZip(zip);
                            if (modInfo)
                            {
                                installedList.Add(modInfo);
                                var dir = Path.Combine(modsPath, modInfo.Id);
                                if (!Directory.Exists(dir))
                                {
                                    Directory.CreateDirectory(dir);
                                }
                                var target = Path.Combine(dir, Path.GetFileName(filepath));
                                if (filepath != target)
                                    File.Copy(filepath, target, true);
                            }
                        }
                    }
                    else
                    {
                        Log.Print($"Only zip files are possible.");
                    }
                }
                catch (Exception ex)
                {
                    Log.Print(ex.Message);
                    Log.Print($"Error when installing file '{Path.GetFileName(filepath)}'.");
                }
            }

            // delete old zip files if count > 2
            if (installedList.Count > 0)
            {
                foreach (var modInfo in installedList)
                {
                    var tempList = new List<ModInfo>();
                    foreach (var filepath in Directory.GetFiles(Path.Combine(modsPath, modInfo.Id), "*.zip", SearchOption.AllDirectories))
                    {
                        var mod = ReadModInfoFromZip(filepath);
                        if (mod && !mod.EqualsVersion(modInfo))
                        {
                            mod.temporary = filepath;
                            tempList.Add(mod);
                        }
                    }
                    tempList = tempList.OrderBy(x => x.parsedVersion).ToList();
                    while (tempList.Count > 2)
                    {
                        var item = tempList.First();
                        try
                        {
                            tempList.Remove(item);
                            File.Delete(item.temporary as string);
                        }
                        catch (Exception ex)
                        {
                            Log.Print(ex.Message);
                            Log.Print($"Can't delete old temp file '{item.temporary as string}'.");
                            break;
                        }
                    }
                }
            }

            ReloadMods();
            RefreshModList();
        }

        private void UninstallMod(string name)
        {
            if (selectedGame == null)
            {
                Log.Print("Select a game.");
                return;
            }

            var modsPath = Path.Combine(currentGamePath, selectedGame.ModsDirectory);
            if (!Directory.Exists(modsPath))
            {
                Log.Print("Install the UnityModManager.");
                return;
            }

            var modPath = Path.Combine(modsPath, name);

            if (Directory.Exists(modPath))
            {
                try
                {
                    Directory.Delete(modPath, true);
                    Log.Print($"Deleting '{name}' - success.");
                }
                catch (Exception ex)
                {
                    Log.Print(ex.Message);
                    Log.Print($"Error when uninstalling '{name}'.");
                }
            }

            ReloadMods();
            RefreshModList();
        }

        private void InstallMod(string filepath)
        {
            if (!File.Exists(filepath))
            {
                Log.Print($"File not found '{Path.GetFileName(filepath)}'.");
            }
            try
            {
                using (var zip = ZipFile.Read(filepath))
                {
                    InstallMod(zip);
                }
            }
            catch (Exception e)
            {
                Log.Print(e.Message);
                Log.Print($"Error when installing '{Path.GetFileName(filepath)}'.");
            }
        }

        private void InstallMod(ZipFile zip, bool reloadMods = true)
        {
            if (selectedGame == null)
            {
                Log.Print("Select a game.");
                return;
            }

            var modsPath = Path.Combine(currentGamePath, selectedGame.ModsDirectory);
            if (!Directory.Exists(modsPath))
            {
                Log.Print("Install the UnityModManager.");
                return;
            }

            try
            {
                zip.ExtractAll(modsPath, ExtractExistingFileAction.OverwriteSilently);
                Log.Print($"Unpacking '{Path.GetFileName(zip.Name)}' - success.");
            }
            catch (Exception ex)
            {
                Log.Print(ex.Message);
                Log.Print($"Error when unpacking '{Path.GetFileName(zip.Name)}'.");
            }

            if (reloadMods)
            {
                ReloadMods();
                RefreshModList();
            }
        }

        private void ReloadMods()
        {
            mods.Clear();

            if (selectedGame == null)
                return;

            var modsPath = Path.Combine(currentGamePath, selectedGame.ModsDirectory);
            if (Directory.Exists(modsPath))
            {
                foreach (var dir in Directory.GetDirectories(modsPath))
                {
                    string jsonPath = Path.Combine(dir, selectedGame.ModInfo);
                    if (File.Exists(jsonPath))
                    {
                        try
                        {
                            var modInfo = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(jsonPath));
                            if (modInfo && modInfo.IsValid())
                            {
                                modInfo.Status = ModStatus.Installed;
                                mods.Add(modInfo);
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Print(e.Message);
                            Log.Print($"Error parsing file '{jsonPath}'.");
                        }
                    }
                }
            }

            LoadZipMods();
        }

        private void LoadZipMods()
        {
            if (selectedGame == null)
                return;

            var dir = Path.Combine(Application.StartupPath, selectedGame.Name);
            if (!Directory.Exists(dir))
                return;

            foreach (var filepath in Directory.GetFiles(dir, "*.zip", SearchOption.AllDirectories))
            {
                var modInfo = ReadModInfoFromZip(filepath);
                if (!modInfo)
                    continue;

                var index = mods.FindIndex(m => m.Id == modInfo.Id);
                if (index == -1)
                {
                    modInfo.Status = ModStatus.NotInstalled;
                    modInfo.AvailableVersions.Add(modInfo.parsedVersion, filepath);
                    mods.Add(modInfo);
                }
                else 
                {
                    if (!mods[index].AvailableVersions.ContainsKey(modInfo.parsedVersion))
                    {
                        mods[index].AvailableVersions.Add(modInfo.parsedVersion, filepath);
                    }
                }
            }
        }

        private void RefreshModList()
        {
            listMods.Items.Clear();

            if (mods.Count == 0)
                return;

            mods.Sort((x, y) => x.DisplayName.CompareTo(y.DisplayName));

            foreach (var modInfo in mods)
            {
                string status;
                if (modInfo.Status == ModStatus.Installed)
                {
                    var newest = modInfo.AvailableVersions.Keys.Max(x => x);
                    if (newest != null && newest > modInfo.parsedVersion)
                    {
                        status = $"Newest {newest}";
                    }
                    else
                    {
                        status = "OK";
                    }
                }
                else if (modInfo.Status == ModStatus.NotInstalled)
                {
                    status = "";
                }
                else
                {
                    status = "";
                }

                var listItem = new ListViewItem(modInfo.DisplayName);
                listItem.SubItems.Add(modInfo.Version);
                if (!string.IsNullOrEmpty(modInfo.ManagerVersion))
                {
                    listItem.SubItems.Add(modInfo.ManagerVersion);
                    if (version < Utils.ParseVersion(modInfo.ManagerVersion))
                    {
                        listItem.ForeColor = System.Drawing.Color.FromArgb(192, 0, 0);
                        status = "Need to update UMM.";
                    }
                }
                else
                {
                    listItem.SubItems.Add("");
                }
                listItem.SubItems.Add(status);
                listMods.Items.Add(listItem);
            }
        }

        private ModInfo ReadModInfoFromZip(string filepath)
        {
            try
            {
                using (var zip = ZipFile.Read(filepath))
                {
                    return ReadModInfoFromZip(zip);
                }
            }
            catch (Exception e)
            {
                Log.Print(e.Message);
                Log.Print($"Error parsing file '{Path.GetFileName(filepath)}'.");
            }

            return null;
        }

        private ModInfo ReadModInfoFromZip(ZipFile zip)
        {
            try
            {
                foreach (ZipEntry e in zip)
                {
                    if (e.FileName.EndsWith(selectedGame.ModInfo))
                    {
                        using (var s = new StreamReader(e.OpenReader()))
                        {
                            var modInfo = JsonConvert.DeserializeObject<ModInfo>(s.ReadToEnd());
                            if (modInfo.IsValid())
                            {
                                return modInfo;
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Print(e.Message);
                Log.Print($"Error parsing file '{Path.GetFileName(zip.Name)}'.");
            }

            return null;
        }
    }
}
