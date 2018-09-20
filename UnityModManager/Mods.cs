using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using Ionic.Zip;

namespace UnityModManagerNet.Installer
{
    public partial class UnityModManagerForm : Form
    {
        readonly List<ModInfo> mods = new List<ModInfo>();

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
            var newMods = new List<ModInfo>();
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
                                newMods.Add(modInfo);
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
            if (newMods.Count > 0)
            {
                foreach (var modInfo in newMods)
                {
                    var tempList = new List<ModInfo>();
                    foreach (var filepath in Directory.GetFiles(Path.Combine(modsPath, modInfo.Id), "*.zip", SearchOption.AllDirectories))
                    {
                        var mod = ReadModInfoFromZip(filepath);
                        if (mod && !mod.EqualsVersion(modInfo))
                        {
                            tempList.Add(mod);
                        }
                    }
                    tempList = tempList.OrderBy(x => x.ParsedVersion).ToList();
                    while (tempList.Count > 2)
                    {
                        var item = tempList.First();
                        try
                        {
                            tempList.Remove(item);
                            File.Delete(item.Path);
                        }
                        catch (Exception ex)
                        {
                            Log.Print(ex.Message);
                            Log.Print($"Can't delete old temp file '{item.Path}'.");
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
                                modInfo.Path = dir;
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
                    modInfo.AvailableVersions.Add(modInfo.ParsedVersion, filepath);
                    mods.Add(modInfo);
                }
                else 
                {
                    if (!mods[index].AvailableVersions.ContainsKey(modInfo.ParsedVersion))
                    {
                        mods[index].AvailableVersions.Add(modInfo.ParsedVersion, filepath);
                    }
                }
            }
        }

        private void RefreshModList()
        {
            listMods.Items.Clear();

            if (selectedGame == null || mods.Count == 0 || tabControl.SelectedIndex != 1)
                return;

            mods.Sort((x, y) => x.DisplayName.CompareTo(y.DisplayName));

            foreach (var modInfo in mods)
            {
                string status;
                if (modInfo.Status == ModStatus.Installed)
                {
                    var res = repositories.ContainsKey(selectedGame) ? repositories[selectedGame].FirstOrDefault(x => x.Id == modInfo.Id) : null;
                    var web = !string.IsNullOrEmpty(res?.Version) ? Utils.ParseVersion(res.Version) : new Version();
                    var local = modInfo.AvailableVersions.Keys.Max(x => x) ?? new Version();
                    var newest = web > local ? web : local;

                    if (newest > modInfo.ParsedVersion)
                    {
                        status = $"Available {newest}";
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
                                modInfo.Path = zip.Name;
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

        private void ModcontextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            installToolStripMenuItem.Visible = false;
            uninstallToolStripMenuItem.Visible = false;
            updateToolStripMenuItem.Visible = false;
            revertToolStripMenuItem.Visible = false;
            wwwToolStripMenuItem1.Visible = false;

            var modInfo = selectedMod;
            if (!modInfo)
            {
                e.Cancel = true;
                return;
            }

            if (modInfo.Status == ModStatus.Installed)
            {
                uninstallToolStripMenuItem.Visible = true;
                var newest = modInfo.AvailableVersions.Keys.Max(x => x);
                if (newest != null && newest > modInfo.ParsedVersion)
                {
                    updateToolStripMenuItem.Text = $"Update to {newest}";
                    updateToolStripMenuItem.Visible = true;
                }
                var previous = modInfo.AvailableVersions.Keys.Where(x => x < modInfo.ParsedVersion).Max(x => x);
                if (previous != null)
                {
                    revertToolStripMenuItem.Text = $"Revert to {previous}";
                    revertToolStripMenuItem.Visible = true;
                }
            }
            else if (modInfo.Status == ModStatus.NotInstalled)
            {
                installToolStripMenuItem.Visible = true;
            }

            if (!string.IsNullOrEmpty(modInfo.HomePage))
            {
                wwwToolStripMenuItem1.Visible = true;
            }
        }

        private void installToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var modInfo = selectedMod;
            if (modInfo)
            {
                var newest = modInfo.AvailableVersions.OrderByDescending(x => x.Key).FirstOrDefault();
                if (!string.IsNullOrEmpty(newest.Value))
                {
                    InstallMod(newest.Value);
                }
            }
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            installToolStripMenuItem_Click(sender, e);
        }

        private void uninstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var modInfo = selectedMod;
            if (modInfo)
            {
                UninstallMod(modInfo.Id);
            }
        }

        private void revertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var modInfo = selectedMod;
            if (modInfo)
            {
                var previous = modInfo.AvailableVersions.Where(x => x.Key < modInfo.ParsedVersion).OrderByDescending(x => x.Key).FirstOrDefault();
                if (!string.IsNullOrEmpty(previous.Value))
                {
                    InstallMod(previous.Value);
                }
            }
        }

        private void wwwToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var modInfo = selectedMod;
            if (modInfo)
            {
                System.Diagnostics.Process.Start(modInfo.HomePage);
            }
        }
    }
}
