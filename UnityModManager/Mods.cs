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
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                if (Path.GetExtension(file) == ".zip")
                {
                    try
                    {
                        ZipFile zip = ZipFile.Read(file);
                        zip.ExtractAll(modsPath, ExtractExistingFileAction.OverwriteSilently);
                        Log.Print($"Unpack '{Path.GetFileName(file)}' - success.");
                    }
                    catch(Exception ex)
                    {
                        Log.Print(ex.Message);
                        Log.Print($"Error when unpacking '{Path.GetFileName(file)}'.");
                    }
                }
                else
                {
                    Log.Print($"Only zip files are possible.");
                }
            }

            LoadMods();
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

        private void RefreshModList()
        {
            listMods.Items.Clear();

            if (mods.Count == 0)
                return;

            foreach(var mod in mods)
            {
                var listItem = new ListViewItem(mod.DisplayName);
                listItem.SubItems.Add(mod.Version);
                if (!string.IsNullOrEmpty(mod.ManagerVersion))
                {
                    listItem.SubItems.Add(mod.ManagerVersion);
                    if (version < ParseVersion(mod.ManagerVersion))
                        listItem.ForeColor = System.Drawing.Color.FromArgb(192, 0, 0);
                }
                listMods.Items.Add(listItem);
            }
        }
    }
}
