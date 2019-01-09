﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace UnityModManagerNet.Installer
{
    public partial class UnityModManagerForm : Form
    {
        public UnityModManagerForm()
        {
            InitializeComponent();
            Init();
            InitPageMods();
        }

        public static UnityModManagerForm instance = null;

        static Config config = null;
        static Param param = null;
        static Version version = null;
        static string currentGamePath = null;
        static string currentManagedPath = null;

        GameInfo selectedGame => (GameInfo)gameList.SelectedItem;
        ModInfo selectedMod => listMods.SelectedItems.Count > 0 ? mods.Find(x => x.DisplayName == listMods.SelectedItems[0].Text) : null;

        private void Init()
        {
            FormBorderStyle = FormBorderStyle.FixedDialog;
            instance = this;

            Log.Init();

            var modManagerType = typeof(UnityModManager);
            var modManagerDef = ModuleDefMD.Load(modManagerType.Module);
            var modManager = modManagerDef.Types.First(x => x.Name == modManagerType.Name);
            var versionString = modManager.Fields.First(x => x.Name == nameof(UnityModManager.version)).Constant.Value.ToString();
            currentVersion.Text = versionString;
            version = Utils.ParseVersion(versionString);

            config = Config.Load();
            param = Param.Load();

            if (config != null && config.GameInfo != null && config.GameInfo.Length > 0)
            {
                config.GameInfo = config.GameInfo.OrderBy(x => x.Name).ToArray();
                gameList.Items.AddRange(config.GameInfo);

                GameInfo selected = null;
                if (!string.IsNullOrEmpty(param.LastSelectedGame))
                {
                    selected = config.GameInfo.FirstOrDefault(x => x.Name == param.LastSelectedGame);
                }
                gameList.SelectedItem = selected ?? config.GameInfo.First();
            }
            else
            {
                InactiveForm();
                Log.Print($"Error parsing file '{Config.filename}'.");
                return;
            }

            CheckLastVersion();
        }

        private void UnityModLoaderForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Properties.Settings.Default.Save();
            param.Save();
        }

        private void InactiveForm()
        {
            btnInstall.Enabled = false;
            btnRemove.Enabled = false;
            btnRestore.Enabled = false;
            tabControl.TabPages[1].Enabled = false;
        }

        private bool IsValid(GameInfo gameIfno)
        {
            if (selectedGame == null)
            {
                Log.Print("Select a game.");
                return false;
            }

            string output = "";
            foreach (var field in typeof(GameInfo).GetFields())
            {
                if (!field.IsStatic && field.IsPublic)
                {
                    var value = field.GetValue(gameIfno);
                    if (value == null || value.ToString() == "")
                    {
                        output += $"Field '{field.Name}' is null. ";
                    }
                }
            }
            if (!string.IsNullOrEmpty(output))
            {
                Log.Print(output);
                return false;
            }

            return true;
        }

        private void CheckState()
        {
            if (!IsValid(selectedGame))
            {
                InactiveForm();
                return;
            }

            btnInstall.Text = "Install";

            currentGamePath = "";
            if (!param.ExtractGamePath(selectedGame.Name, out var result) || !Directory.Exists(result))
            {
                result = FindGameFolder(selectedGame.Folder);
                if (string.IsNullOrEmpty(result))
                {
                    InactiveForm();
                    btnOpenFolder.ForeColor = System.Drawing.Color.FromArgb(192, 0, 0);
                    btnOpenFolder.Text = "Select Game Folder";
                    folderBrowserDialog.SelectedPath = null;
                    Log.Print($"Game folder '{selectedGame.Folder}' not found.");
                    return;
                }
                Log.Print($"Game folder detected as '{result}'.");
                param.SaveGamePath(selectedGame.Name, result);
            }
            else
            {
                Log.Print($"Game folder set as '{result}'.");
            }
            currentGamePath = result;
            btnOpenFolder.ForeColor = System.Drawing.Color.Black;
            btnOpenFolder.Text = new DirectoryInfo(result).Name;
            folderBrowserDialog.SelectedPath = currentGamePath;
            currentManagedPath = FindManagedFolder(currentGamePath);

            var assemblyPath = Path.Combine(currentManagedPath, selectedGame.AssemblyName);
            ModuleDefMD assembly = null;

            var originalAssemblyPath = $"{assemblyPath}.original_";
            btnRestore.Enabled = File.Exists(originalAssemblyPath);

            if (File.Exists(assemblyPath))
            {
                try
                {
                    assembly = ModuleDefMD.Load(File.ReadAllBytes(assemblyPath));
                }
                catch (Exception e)
                {
                    InactiveForm();
                    Log.Print(e.Message);
                    return;
                }
            }
            else
            {
                InactiveForm();
                Log.Print($"'{selectedGame.AssemblyName}' not found.");
                return;
            }

            tabControl.TabPages[1].Enabled = true;

            var modManagerType = typeof(UnityModManager);
            var modManagerDefInjected = assembly.Types.FirstOrDefault(x => x.Name == modManagerType.Name);
            if (modManagerDefInjected != null)
            {
                btnInstall.Text = "Update";
                btnInstall.Enabled = false;
                btnRemove.Enabled = true;

                var versionString = modManagerDefInjected.Fields.First(x => x.Name == nameof(UnityModManager.version)).Constant.Value.ToString();
                var version2 = Utils.ParseVersion(versionString);
                installedVersion.Text = versionString;
                if (version != version2)
                {
                    btnInstall.Enabled = true;
                }
            }
            else
            {
                installedVersion.Text = "-";
                btnInstall.Enabled = true;
                btnRemove.Enabled = false;
            }

            btnRestore.Enabled = IsDirty(assembly) && btnRestore.Enabled;
        }

        private string FindGameFolder(string str)
        {
            string[] disks = new string[] { @"C:\", @"D:\", @"E:\", @"F:\" };
            string[] roots = new string[] { "Games", "Program files", "Program files (x86)", "" };
            string[] folders = new string[] { @"Steam\SteamApps\common", @"GoG Galaxy\Games", "" };
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                disks = new string[] { Environment.GetEnvironmentVariable("HOME") };
                roots = new string[] { "Library/Application Support", ".steam" };
                folders = new string[] { "Steam/SteamApps/common", "steam/steamapps/common" };
            }
            foreach (var disk in disks)
            {
                foreach (var root in roots)
                {
                    foreach (var folder in folders)
                    {
                        var path = Path.Combine(disk, root);
                        path = Path.Combine(path, folder);
                        path = Path.Combine(path, str);
                        if (Directory.Exists(path))
                        {
                            return path;
                        }
                    }
                }
            }
            return null;
        }

        private string FindManagedFolder(string str)
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                var appName = $"{Path.GetFileName(str)}.app";
                if (!Directory.Exists(Path.Combine(str, appName)))
                {
                    appName = Directory.GetDirectories(str).FirstOrDefault(dir => dir.EndsWith(".app"));
                }
                var path = Path.Combine(str, $"{appName}/Contents/Resources/Data/Managed");
                if (Directory.Exists(path))
                {
                    return path;
                }
            }
            var regex = new Regex(".*_Data$");
            var directory = new DirectoryInfo(str);
            foreach (var dir in directory.GetDirectories())
            {
                var match = regex.Match(dir.Name);
                if (match.Success)
                {
                    var path = Path.Combine(str, $"{dir.Name}{Path.DirectorySeparatorChar}Managed");
                    if (Directory.Exists(path))
                    {
                        return path;
                    }
                }
            }
            return str;
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (Inject(Actions.Remove))
                btnInstall.Text = "Install";
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            if (Inject(Actions.Install))
                btnInstall.Text = "Update";
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            if (RestoreOriginal())
                CheckState();
        }

        private void btnDownloadUpdate_Click(object sender, EventArgs e)
        {
            var downloaderFile = "Downloader.exe";
            if (File.Exists(downloaderFile))
            {
                Process.Start(downloaderFile);
            }
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                param.SaveGamePath(selectedGame.Name, folderBrowserDialog.SelectedPath);
                CheckState();
            }
        }

        private void gameList_Changed(object sender, EventArgs e)
        {
            var selected = (GameInfo)((ComboBox)sender).SelectedItem;
            if (selected != null)
                Log.Print($"Game changed to '{selected.Name}'.");

            param.LastSelectedGame = selected.Name;

            CheckState();
        }

        enum Actions
        {
            Install,
            Remove
        };

        private bool Inject(Actions action, ModuleDefMD assembly = null, bool save = true)
        {
            var assemblyPath = Path.Combine(currentManagedPath, selectedGame.AssemblyName);
            var backupAssemblyPath = $"{assemblyPath}.backup_";
            var originalAssemblyPath = $"{assemblyPath}.original_";

            if (File.Exists(assemblyPath))
            {
                if (assembly == null)
                {
                    try
                    {
                        assembly = ModuleDefMD.Load(File.ReadAllBytes(assemblyPath));
                    }
                    catch (Exception e)
                    {
                        Log.Print(e.Message);
                        return false;
                    }
                }

                var modManagerType = typeof(UnityModManager);

                switch (action)
                {
                    case Actions.Install:
                        try
                        {
                            if (!Utils.ParsePatchTarget(assembly, selectedGame.PatchTarget, out var targetMethod, out var insertionPlace))
                            {
                                return false;
                            }

                            Log.Print($"Backup for '{selectedGame.AssemblyName}'.");
                            File.Copy(assemblyPath, backupAssemblyPath, true);

                            if (!IsDirty(assembly))
                            {
                                File.Copy(assemblyPath, originalAssemblyPath, true);
                            }

                            CopyLibraries();

                            var modsPath = Path.Combine(currentGamePath, selectedGame.ModsDirectory);
                            if (!Directory.Exists(modsPath))
                            {
                                Directory.CreateDirectory(modsPath);
                            }

                            var typeInjectorInstalled = assembly.Types.FirstOrDefault(x => x.Name == modManagerType.Name);
                            if (typeInjectorInstalled != null)
                            {
                                if (!Inject(Actions.Remove, assembly, false))
                                {
                                    Log.Print("Installation failed. Can't uninstall the previous version.");
                                    return false;
                                }
                            }

                            Log.Print("Applying patch...");

                            if (!IsDirty(assembly))
                            {
                                MakeDirty(assembly);
                            }

                            var modManagerDef = ModuleDefMD.Load(modManagerType.Module);
                            var modManager = modManagerDef.Types.First(x => x.Name == modManagerType.Name);
                            var modManagerModsDir = modManager.Fields.First(x => x.Name == nameof(UnityModManager.modsDirname));
                            modManagerModsDir.Constant.Value = selectedGame.ModsDirectory;
                            var modManagerModInfo = modManager.Fields.First(x => x.Name == nameof(UnityModManager.infoFilename));
                            modManagerModInfo.Constant.Value = selectedGame.ModInfo;
                            var modManagerPatchTarget = modManager.Fields.First(x => x.Name == nameof(UnityModManager.patchTarget));
                            modManagerPatchTarget.Constant.Value = selectedGame.PatchTarget;
                            modManagerDef.Types.Remove(modManager);
                            assembly.Types.Add(modManager);

                            var instr = OpCodes.Call.ToInstruction(modManager.Methods.First(x => x.Name == nameof(UnityModManager.Start)));
                            if (string.IsNullOrEmpty(insertionPlace) || insertionPlace == "after")
                            {
                                targetMethod.Body.Instructions.Insert(targetMethod.Body.Instructions.Count - 1, instr);
                            }
                            else if (insertionPlace == "before")
                            {
                                targetMethod.Body.Instructions.Insert(0, instr);
                            }

                            if (save)
                            {
                                assembly.Write(assemblyPath);
                                Log.Print("Installation was successful.");
                            }

                            installedVersion.Text = currentVersion.Text;
                            btnInstall.Enabled = false;
                            btnRemove.Enabled = true;
                            btnRestore.Enabled = File.Exists(originalAssemblyPath);

                            return true;
                        }
                        catch (Exception e)
                        {
                            Log.Print(e.Message);
                            if (!File.Exists(assemblyPath))
                                RestoreBackup();
                        }

                        break;

                    case Actions.Remove:
                        try
                        {
                            var modManagerInjected = assembly.Types.FirstOrDefault(x => x.Name == modManagerType.Name);
                            if (modManagerInjected != null)
                            {
                                Log.Print("Removing patch...");
                                var patchTarget = selectedGame.PatchTarget;
                                var modManagerPatchTarget = modManagerInjected.Fields.FirstOrDefault(x => x.Name == nameof(UnityModManager.patchTarget));
                                if (modManagerPatchTarget != null && !string.IsNullOrEmpty((string)modManagerPatchTarget.Constant.Value))
                                {
                                    patchTarget = (string)modManagerPatchTarget.Constant.Value;
                                }
                                if (!Utils.ParsePatchTarget(assembly, patchTarget, out var targetMethod, out var insertionPlace))
                                {
                                    return false;
                                }

                                var instr = OpCodes.Call.ToInstruction(modManagerInjected.Methods.First(x => x.Name == nameof(UnityModManager.Start)));
                                for (int i = 0; i < targetMethod.Body.Instructions.Count; i++)
                                {
                                    if (targetMethod.Body.Instructions[i].OpCode == instr.OpCode
                                        && targetMethod.Body.Instructions[i].Operand == instr.Operand)
                                    {
                                        targetMethod.Body.Instructions.RemoveAt(i);
                                        break;
                                    }
                                }

                                assembly.Types.Remove(modManagerInjected);

                                if (!IsDirty(assembly))
                                {
                                    MakeDirty(assembly);
                                }

                                if (save)
                                {
                                    assembly.Write(assemblyPath);
                                    Log.Print("Removal was successful.");
                                }

                                installedVersion.Text = "-";
                                btnInstall.Enabled = true;
                                btnRemove.Enabled = false;
                            }

                            return true;
                        }
                        catch (Exception e)
                        {
                            Log.Print(e.Message);
                            if (!File.Exists(assemblyPath))
                                RestoreBackup();
                        }

                        break;
                }
            }
            else
            {
                Log.Print($"'{assemblyPath}' not found.");
                return false;
            }

            return false;
        }

        private static bool IsDirty(ModuleDefMD assembly)
        {
            return assembly.Types.FirstOrDefault(x => x.FullName == typeof(Marks.IsDirty).FullName || x.Name == typeof(UnityModManager).Name) != null;
        }

        private static void MakeDirty(ModuleDefMD assembly)
        {
            var moduleDef = ModuleDefMD.Load(typeof(Marks.IsDirty).Module);
            var typeDef = moduleDef.Types.FirstOrDefault(x => x.FullName == typeof(Marks.IsDirty).FullName);
            moduleDef.Types.Remove(typeDef);
            assembly.Types.Add(typeDef);
        }

        private static bool RestoreBackup()
        {
            var assemblyPath = Path.Combine(currentManagedPath, instance.selectedGame.AssemblyName);
            var backupAssemblyPath = $"{assemblyPath}.backup_";

            try
            {
                if (File.Exists(backupAssemblyPath))
                {
                    File.Copy(backupAssemblyPath, assemblyPath, true);
                    Log.Print("Backup restored.");
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Print(e.Message);
            }

            return false;
        }

        private static bool RestoreOriginal()
        {
            var assemblyPath = Path.Combine(currentManagedPath, instance.selectedGame.AssemblyName);
            var originalAssemblyPath = $"{assemblyPath}.original_";

            try
            {
                if (File.Exists(originalAssemblyPath))
                {
                    File.Copy(originalAssemblyPath, assemblyPath, true);
                    Log.Print("Original files restored.");
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Print(e.Message);
            }

            return false;
        }

        private static void CopyLibraries()
        {
            string[] files = new string[]
            {
                "0Harmony12.dll"
            };

            foreach (var file in files)
            {
                var path = Path.Combine(currentManagedPath, file);
                if (File.Exists(path))
                {
                    var source = new FileInfo(file);
                    var dest = new FileInfo(path);
                    if (dest.Length == source.Length)
                        continue;

                    File.Copy(path, $"{path}.backup_", true);
                }

                File.Copy(file, path, true);
                Log.Print($"'{file}' is copied to game folder.");
            }
        }

        private void folderBrowserDialog_HelpRequest(object sender, EventArgs e)
        {
        }

        private void tabs_Changed(object sender, EventArgs e)
        {
            switch (tabControl.SelectedIndex)
            {
                case 1: // Mods
                    ReloadMods();
                    RefreshModList();
                    if (selectedGame != null && !repositories.ContainsKey(selectedGame))
                        CheckModUpdates();
                    break;
            }
        }
    }
}
