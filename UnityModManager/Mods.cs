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
        List<UnityModManager.ModInfo> mods = new List<UnityModManager.ModInfo>();

        private void InitPageMods()
        {
            splitContainerMods.Panel2.AllowDrop = true;
            splitContainerMods.Panel2.DragEnter += new DragEventHandler(Mods_DragEnter);
            splitContainerMods.Panel2.DragDrop += new DragEventHandler(Mods_DragDrop);

            //LoadMods();
            //RefreshModList();
        }

        private void Mods_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void Mods_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {

                var modsPath = Path.Combine(Application.StartupPath, selectedGame.Name);
                if (!Directory.Exists(modsPath))
                {
                    Directory.CreateDirectory(modsPath);
                }

                var target = Path.Combine(modsPath, Path.GetFileName(file));
                File.Copy(file, target);

                InstallMod(target);

            }
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

            mods.Clear();
            LoadMods();
            LoadProgramDirMods();
            RefreshModList();

        }

        private void InstallMod(string file)
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

            if (Path.GetExtension(file) == ".zip")
            {
                try
                {
                    ZipFile zip = ZipFile.Read(file);
                    zip.ExtractAll(modsPath, ExtractExistingFileAction.OverwriteSilently);
                    Log.Print($"Unpack '{Path.GetFileName(file)}' - success.");
                }
                catch (Exception ex)
                {
                    Log.Print(ex.Message);
                    Log.Print($"Error when unpacking '{Path.GetFileName(file)}'.");
                }
            }
            else
            {
                Log.Print($"Only zip files are possible.");
            }

            mods.Clear();
            LoadMods();
            LoadProgramDirMods();
            RefreshModList();
        }

        private void LoadMods()
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
                            var modInfo = JsonConvert.DeserializeObject<UnityModManager.ModInfo>(File.ReadAllText(jsonPath));
                            modInfo.status = "installed";
                            mods.Add(modInfo);
                        }
                        catch (Exception e)
                        {
                            Log.Print(e.Message);
                            Log.Print($"Error parsing file '{jsonPath}'.");
                        }
                    }
                }
            }
        }

        private void LoadProgramDirMods()
        {
            if (selectedGame == null)
                return;

            var modsList = Directory.GetFiles(Path.Combine(Application.StartupPath, selectedGame.Name), "*.zip",SearchOption.AllDirectories);

            foreach (var modzip in modsList)
            {
                try { 
                    ZipFile zip = ZipFile.Read(modzip);

                    foreach (ZipEntry e in zip)
                    {
                        if (e.FileName.EndsWith(selectedGame.ModInfo))
                        {
                            using (StreamReader s = new StreamReader(e.OpenReader()))
                            {
                                var modInfo = JsonConvert.DeserializeObject<UnityModManager.ModInfo>(s.ReadToEnd());
                                modInfo.status = "not installed";
                                modInfo.ZipPath = modzip;

                                var modInfoCurrent = mods.FindIndex(m => m.DisplayName == modInfo.DisplayName);
                                
                                if(modInfoCurrent == -1) {
                                    mods.Add(modInfo);
                                } else if (ParseVersion(mods[modInfoCurrent].Version) < ParseVersion(modInfo.Version))
                                {
                                    if(mods[modInfoCurrent].status == "installed")
                                    {
                                        mods[modInfoCurrent].status = $"update to {modInfo.Version}";
                                        mods[modInfoCurrent].ZipPath = modzip;
                                    }
                                }
                                

                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Print(e.Message);
                    Log.Print($"Error parsing file '{modzip}'.");
                }
            }
        }
        
        private void RefreshModList()
        {
            listMods.Items.Clear();

            if (mods.Count == 0)
                return;

            mods.Sort((x, y) => x.DisplayName.CompareTo(y.DisplayName));

            foreach (var mod in mods)
            {
                var listItem = new ListViewItem(mod.DisplayName);
                listItem.SubItems.Add(mod.Version);
                if (!string.IsNullOrEmpty(mod.ManagerVersion))
                {
                    listItem.SubItems.Add(mod.ManagerVersion);
                    if (version < ParseVersion(mod.ManagerVersion))
                        listItem.ForeColor = System.Drawing.Color.FromArgb(192, 0, 0);
                } else
                {
                    listItem.SubItems.Add("");
                }
                listItem.SubItems.Add(mod.status);
                listMods.Items.Add(listItem);
            }
        }
    }
}
