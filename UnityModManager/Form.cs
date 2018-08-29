using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Text.RegularExpressions;

namespace UnityModManagerNet.Installer
{
    public partial class UnityModManagerForm : Form
    {
        public UnityModManagerForm()
        {
            InitializeComponent();
            Init();
        }

        public static UnityModManagerForm instance = null;

        static Config config = null;
        static Param param = null;
        static Version version = null;
        static string currentGamePath = null;
        static string currentManagedPath = null;

        GameInfo selectedGame => (GameInfo)gameList.SelectedItem;

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
            version = UnityModManager.ParseVersion(versionString);

            config = Config.Load();
            param = Param.Load();

            if (config != null && config.GameInfo != null && config.GameInfo.Length > 0)
            {
                gameList.Items.AddRange(config.GameInfo);
                GameInfo selected = null;
                if (!string.IsNullOrEmpty(param.LastGameSelected))
                {
                    selected = config.GameInfo.FirstOrDefault(x => x.Name == param.LastGameSelected);
                }
                gameList.SelectedItem = selected ?? config.GameInfo.First();
            }
            else
            {
                InactiveForm();
                Log.Print($"Error parsing file '{Config.filename}'.");
                return;
            }
        }

        private void UnityModLoaderForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            param.Save();
        }

        private void InactiveForm()
        {
            btnInstall.Enabled = false;
            btnRemove.Enabled = false;
        }

        private bool IsValid(GameInfo gameIfno)
        {
            if (selectedGame == null)
            {
                Log.Print("Game is null.");
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
                        output += $"{field.Name} is null.";
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

            currentGamePath = "";
            if (!param.ExtractGamePath(selectedGame.Name, out var result))
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
            btnOpenFolder.Text = selectedGame.Folder;
            folderBrowserDialog.SelectedPath = currentGamePath;
            currentManagedPath = FindManagedFolder(currentGamePath);

            var assemblyPath = Path.Combine(currentManagedPath, selectedGame.AssemblyName);
            ModuleDefMD assembly = null;

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

            var modManagerType = typeof(UnityModManager);
            var modManagerDefInjected = assembly.Types.FirstOrDefault(x => x.Name == modManagerType.Name);
            if (modManagerDefInjected != null)
            {
                btnInstall.Enabled = false;
                btnRemove.Enabled = true;

                var versionString = modManagerDefInjected.Fields.First(x => x.Name == nameof(UnityModManager.version)).Constant.Value.ToString();
                var version2 = UnityModManager.ParseVersion(versionString);
                installedVersion.Text = versionString;
                if (version != version2)
                    btnInstall.Enabled = true;
            }
            else
            {
                installedVersion.Text = "-";
                btnInstall.Enabled = true;
                btnRemove.Enabled = false;
            }
        }

        private string FindGameFolder(string str)
        {
            string[] disks = new string[] { @"C:\", @"D:\", @"E:\", @"F:\" };
            string[] roots = new string[] { "Games", "Program files", "Program files (x86)", "" };
            string[] folders = new string[] { @"Steam\SteamApps\common", "" };
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
            var regex = new Regex(".*_Data$");
            var dirictory = new DirectoryInfo(str);
            foreach (var dir in dirictory.GetDirectories())
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
            Inject(Actions.Remove);
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            Inject(Actions.Install);
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                inputLog.Clear();
                param.SaveGamePath(selectedGame.Name, folderBrowserDialog.SelectedPath);
                //Log.Print(folderBrowserDialog.SelectedPath);
                CheckState();
            }
        }

        private void gameList_Changed(object sender, EventArgs e)
        {
            inputLog.Clear();
            var selected = (GameInfo)((ComboBox)sender).SelectedItem;
            if (selected != null)
                Log.Print($"Game changed to '{selected.Name}'.");

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
            var backupAssemblyPath = $"{assemblyPath}.backup";

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

                string className = null;
                string methodName = null;
                string placeType = null;

                var pos = selectedGame.PatchMethod.LastIndexOf('.');
                if (pos != -1)
                {
                    className = selectedGame.PatchMethod.Substring(0, pos);

                    var pos2 = selectedGame.PatchMethod.LastIndexOf(':');
                    if (pos2 != -1)
                    {
                        methodName = selectedGame.PatchMethod.Substring(pos + 1, pos2 - pos - 1);
                        placeType = selectedGame.PatchMethod.Substring(pos2 + 1).ToLower();

                        if (placeType != "after" && placeType != "before")
                            Log.Print($"Parameter '{placeType}' in '{selectedGame.PatchMethod}' is unknown.");
                    }
                    else
                    {
                        methodName = selectedGame.PatchMethod.Substring(pos + 1);
                    }
                }
                else
                {
                    Log.Print($"Function name error '{selectedGame.PatchMethod}'.");
                    return false;
                }

                //            var array = selectedGame.PatchMethod.Split('.');
                //if (array != null && array.Length > 1)
                //{
                //	if (array.Length == 2)
                //	{
                //		className = array[0];
                //		methodName = array[1];
                //	}
                //	else if (array.Length == 3)
                //	{
                //		className = $"{array[0]}.{array[1]}";
                //		methodName = array[2];
                //	}
                //}

                var targetClass = assembly.Types.FirstOrDefault(x => x.FullName == className);
                if (targetClass == null)
                {
                    Log.Print($"Class '{className}' not found.");
                    return false;
                }

                var targetMethod = targetClass.Methods.FirstOrDefault(x => x.Name == methodName);
                if (targetMethod == null)
                {
                    Log.Print($"Method '{methodName}' not found.");
                    return false;
                }

                var modManagerType = typeof(UnityModManager);

                switch (action)
                {
                    case Actions.Install:
                        try
                        {
                            Log.Print($"Backup for '{selectedGame.AssemblyName}'.");
                            File.Copy(assemblyPath, backupAssemblyPath, true);

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
                            var modManagerDef = ModuleDefMD.Load(modManagerType.Module);
                            var modManager = modManagerDef.Types.First(x => x.Name == modManagerType.Name);
                            var modManagerModsDir = modManager.Fields.First(x => x.Name == nameof(UnityModManager.modsDirname));
                            modManagerModsDir.Constant.Value = selectedGame.ModsDirectory;
                            var modManagerModInfo = modManager.Fields.First(x => x.Name == nameof(UnityModManager.infoFilename));
                            modManagerModInfo.Constant.Value = selectedGame.ModInfo;
                            modManagerDef.Types.Remove(modManager);
                            assembly.Types.Add(modManager);

                            var instr = OpCodes.Call.ToInstruction(modManager.Methods.First(x => x.Name == nameof(UnityModManager.Start)));
                            if (string.IsNullOrEmpty(placeType) || placeType == "after")
                            {
                                targetMethod.Body.Instructions.Insert(targetMethod.Body.Instructions.Count - 1, instr);
                            }
                            else if (placeType == "before")
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

        private static bool RestoreBackup()
        {
            var assemblyPath = Path.Combine(currentManagedPath, instance.selectedGame.AssemblyName);
            var backupAssemblyPath = $"{assemblyPath}.backup";

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

        private static void CopyLibraries()
        {
            string[] files = new string[]
            {
                "0Harmony.dll"
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

                    File.Copy(path, $"{path}.backup", true);
                }

                File.Copy(file, path, true);
                Log.Print($"'{file}' is copied to game folder.");
            }
        }

        private void folderBrowserDialog_HelpRequest(object sender, EventArgs e)
        {
        }
    }
}
