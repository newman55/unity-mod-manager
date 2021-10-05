using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using Ionic.Zip;
using UnityModManagerNet.ConsoleInstaller;

namespace UnityModManagerNet.Installer
{
    public partial class UnityModManagerForm : Form
    {
        readonly List<ModInfo> mods = new List<ModInfo>();

        private void InitPageMods()
        {
            btnModInstall.Click += btnModInstall_Click;
            splitContainerModsInstall.Panel2.AllowDrop = true;
            splitContainerModsInstall.Panel2.DragEnter += new DragEventHandler(Mods_DragEnter);
            splitContainerModsInstall.Panel2.DragDrop += new DragEventHandler(Mods_DragDrop);
        }

        private void btnModInstall_Click(object sender, EventArgs e)
        {
            var result = modInstallFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (modInstallFileDialog.FileNames.Length == 0)
                    return;

                SaveAndInstallZipFiles(modInstallFileDialog.FileNames);
                ReloadMods();
                RefreshModList();
            }
        }

        private void Mods_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void Mods_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 0)
                return;

            //Drag and drop files on OS X are in the format /.file/id=6571367.2773272
            if (Environment.OSVersion.Platform == PlatformID.Unix && files[0].StartsWith("/.file"))
            {
                files = files.Select(f => Utils.ResolveOSXFileUrl(f)).ToArray();
            }
            SaveAndInstallZipFiles(files);
            ReloadMods();
            RefreshModList();
        }

        private void SaveAndInstallZipFiles(string[] files)
        {
            var programModsPath = Path.Combine(Application.StartupPath, selectedGame.Folder);
            var newMods = new List<ModInfo>();
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
                                var dir = Path.Combine(programModsPath, modInfo.Id);
                                if (!Directory.Exists(dir))
                                {
                                    Directory.CreateDirectory(dir);
                                }
                                var target = Path.Combine(dir, $"{modInfo.Id}-{modInfo.Version.Replace('.', '-')}.zip");
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
                    foreach (var filepath in Directory.GetFiles(Path.Combine(programModsPath, modInfo.Id), "*.zip", SearchOption.AllDirectories))
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
                            Log.Print($"Can't delete old archive '{item.Path}'.");
                            break;
                        }
                    }
                }
            }
        }

        private void UninstallMod(ModInfo modInfo)
        {
            if (selectedGame == null)
            {
                Log.Print("Select a game.");
                return;
            }

            var modsPath = Path.Combine(gamePath, selectedGame.ModsDirectory);
            if (!Directory.Exists(modsPath))
            {
                Log.Print("Install the UnityModManager.");
                return;
            }

            if (Directory.Exists(modInfo.Path))
            {
                try
                {
                    Directory.Delete(modInfo.Path, true);
                    Log.Print($"Deleting '{modInfo.Id}' - SUCCESS.");
                }
                catch (Exception ex)
                {
                    Log.Print(ex.Message);
                    Log.Print($"Error when uninstalling '{modInfo.Id}'.");
                }
            }
            else
            {
                Log.Print($"Directory '{modInfo.Path}' - not found.");
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

            var modsPath = Path.Combine(gamePath, selectedGame.ModsDirectory);
            if (!Directory.Exists(modsPath))
            {
                Log.Print("Install the UnityModManager.");
                return;
            }

            try
            {
                var modInfo = ReadModInfoFromZip(zip);
                if (modInfo == null)
                {
                    Log.Print($"{Path.GetFileName(zip.Name)} is not supported.");
                    return;
                }
                var modInstalled = mods.Find(x => x.Id == modInfo.Id && x.Status == ModStatus.Installed);
                var modPath = modInstalled ? modInstalled.Path : Path.Combine(modsPath, modInfo.Id);
                var replaceModDir = "";
                if (zip.EntriesSorted.Count(x => x.FileName.Equals(selectedGame.ModInfo, StringComparison.CurrentCultureIgnoreCase)) == 0)
                {
                    modPath = modsPath;
                    if (modInstalled)
                    {
                        replaceModDir = modInstalled.Path.Split(Path.DirectorySeparatorChar).Last();
                    }
                }
                
                foreach (var entry in zip.EntriesSorted)
                {
                    var filename = entry.FileName;
                    if (!string.IsNullOrEmpty(replaceModDir))
                    {
                        var pos = filename.IndexOf(Path.AltDirectorySeparatorChar);
                        filename = replaceModDir + filename.Substring(pos, filename.Length - pos);
                    }
                    if (entry.IsDirectory)
                    {
                        Directory.CreateDirectory(Path.Combine(modPath, filename));
                    }
                    else
                    {
                        Directory.CreateDirectory(Path.Combine(modPath, Path.GetDirectoryName(filename)));
                        using (FileStream fs = new FileStream(Path.Combine(modPath, filename), FileMode.Create, FileAccess.Write))
                        {
                            entry.Extract(fs);
                        }
                    }
                }
                Log.Print($"Unpacking '{Path.GetFileName(zip.Name)}' - SUCCESS.");
            }
            catch (Exception ex)
            {
                Log.Print(ex.Message);
                Log.Print(ex.StackTrace);
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

            var modsPath = Path.Combine(gamePath, selectedGame.ModsDirectory);
            if (Directory.Exists(modsPath))
            {
                foreach (var dir in Directory.GetDirectories(modsPath))
                {
                    string jsonPath = Path.Combine(dir, selectedGame.ModInfo);
                    if (!File.Exists(jsonPath)) jsonPath = Path.Combine(dir, selectedGame.ModInfo.ToLower());
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

            var dir = Path.Combine(Application.StartupPath, selectedGame.Folder);
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
                    var release = repositories.ContainsKey(selectedGame) ? repositories[selectedGame].FirstOrDefault(x => x.Id == modInfo.Id) : null;
                    var web = !string.IsNullOrEmpty(release?.Version) ? Utils.ParseVersion(release.Version) : new Version();
                    var local = modInfo.AvailableVersions.Keys.Max(x => x) ?? new Version();

                    if (local > modInfo.ParsedVersion && local >= web)
                    {
                        status = $"Update {local}";
                    }
                    else if (web > modInfo.ParsedVersion && web > local)
                    {
                        status = string.IsNullOrEmpty(release.DownloadUrl) ? $"Available {web}" : $"Download {web}";
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
                if (modInfo.Status == ModStatus.NotInstalled)
                {
                    listItem.SubItems.Add(modInfo.AvailableVersions.Count > 0 ? modInfo.AvailableVersions.Keys.Max(x => x).ToString() : modInfo.Version);
                }
                else
                {
                    listItem.SubItems.Add(modInfo.Version);
                }
                if (!string.IsNullOrEmpty(modInfo.ManagerVersion))
                {
                    listItem.SubItems.Add(modInfo.ManagerVersion);
                    if (version < Utils.ParseVersion(modInfo.ManagerVersion))
                    {
                        listItem.ForeColor = System.Drawing.Color.FromArgb(192, 0, 0);
                        status = "Need to update UMM";
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
                    if (e.FileName.EndsWith(selectedGame.ModInfo, StringComparison.InvariantCultureIgnoreCase))
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
            openFolderToolStripMenuItem.Visible = false;

            var modInfo = selectedMod;
            if (!modInfo)
            {
                e.Cancel = true;
                return;
            }

            if (modInfo.Status == ModStatus.Installed)
            {
                uninstallToolStripMenuItem.Visible = true;
                openFolderToolStripMenuItem.Visible = true;

                Version inRepository = new Version();
                if (repositories.ContainsKey(selectedGame))
                {
                    var release = repositories[selectedGame].FirstOrDefault(x => x.Id == modInfo.Id);
                    if (release != null && !string.IsNullOrEmpty(release.DownloadUrl) && !string.IsNullOrEmpty(release.Version))
                    {
                        var ver = Utils.ParseVersion(release.Version);
                        if (modInfo.ParsedVersion < ver)
                        {
                            inRepository = ver;
                            updateToolStripMenuItem.Text = $"Update to {release.Version}";
                            updateToolStripMenuItem.Visible = true;
                        }
                    }
                }

                var newest = modInfo.AvailableVersions.Keys.Max(x => x);
                if (newest != null && newest > modInfo.ParsedVersion && inRepository <= newest)
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
            var modInfo = selectedMod;
            if (modInfo)
            {
                if (repositories.ContainsKey(selectedGame))
                {
                    var release = repositories[selectedGame].FirstOrDefault(x => x.Id == modInfo.Id);
                    if (release != null && !string.IsNullOrEmpty(release.DownloadUrl) && !string.IsNullOrEmpty(release.Version) && modInfo.AvailableVersions.All(x => x.Key < Utils.ParseVersion(release.Version)))
                    {
                        var downloadForm = new DownloadMod(release);
                        var result = downloadForm.ShowDialog();
                        if (result == DialogResult.OK)
                        {
                            SaveAndInstallZipFiles(new string[] { downloadForm.tempFilepath });
                            ReloadMods();
                            RefreshModList();
                        }
                        return;
                    }
                }
                installToolStripMenuItem_Click(sender, e);
            }
        }

        private void uninstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var modInfo = selectedMod;
            if (modInfo)
            {
                UninstallMod(modInfo);
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
                    UninstallMod(modInfo);
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

        private void openFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var modInfo = selectedMod;
            if (modInfo)
            {
                System.Diagnostics.Process.Start(modInfo.Path);
            }
        }
    }
}
