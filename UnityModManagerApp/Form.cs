using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;

namespace UnityModManagerNet.Installer
{
    [Serializable]
    public partial class UnityModManagerForm : Form
    {
        const string REG_PATH = @"HKEY_CURRENT_USER\Software\UnityModManager";

        private static readonly Version VER_0_13 = new Version(0, 13);
        private static readonly Version VER_0_22 = new Version(0, 22);

        private static readonly Version HARMONY_VER = new Version(2, 0);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hwnd);

        public UnityModManagerForm()
        {
            if (!Utils.IsUnixPlatform() && CheckApplicationAlreadyRunning(out var process))
            {
                SetForegroundWindow(process.MainWindowHandle);
                Close();
                return;
            }
            InitializeComponent();
            Load += UnityModManagerForm_Load;
        }

        private void UnityModManagerForm_Load(object sender, EventArgs e)
        {
            Init();
            InitPageMods();
        }

        static bool CheckApplicationAlreadyRunning(out Process result)
        {
            result = null;
            var id = Process.GetCurrentProcess().Id;
            var name = Process.GetCurrentProcess().ProcessName;
            foreach(var p in Process.GetProcessesByName(name))
            {
                if (p.Id != id)
                {
                    result = p;
                    return true;
                }
            }
            return false;
        }

        [Flags]
        enum LibIncParam { Normal = 0, Minimal_lt_0_22 = 1 }

        static readonly Dictionary<string, LibIncParam> libraryFiles = new Dictionary<string, LibIncParam>
        {
            { "0Harmony.dll", LibIncParam.Normal },
            { "0Harmony12.dll", LibIncParam.Minimal_lt_0_22 },
            { "0Harmony-1.2.dll", LibIncParam.Minimal_lt_0_22 },
            { "dnlib.dll", LibIncParam.Normal },
            { "System.Xml.dll", LibIncParam.Normal },
            { nameof(UnityModManager) + ".dll", LibIncParam.Normal },
            { nameof(UnityModManager) + ".xml", LibIncParam.Normal },
        };

        static List<string> libraryPaths;

        public static UnityModManagerForm instance = null;

        static Config config = null;
        static Param param = null;
        static Version version = null;

        static string gamePath = null;
        static string managedPath = null;
        static string managerPath = null;
        static string entryAssemblyPath = null;
        static string injectedEntryAssemblyPath = null;
        static string managerAssemblyPath = null;
        static string entryPoint = null;
        static string injectedEntryPoint = null;

        static string gameExePath = null;

        static string doorstopFilename = "winhttp.dll";
        static string doorstopConfigFilename = "doorstop_config.ini";
        static string doorstopPath = null;
        static string doorstopConfigPath = null;

        static ModuleDefMD assemblyDef = null;
        static ModuleDefMD injectedAssemblyDef = null;
        static ModuleDefMD managerDef = null;

        //static string machineConfigPath = null;
        //static XDocument machineDoc = null;

        GameInfo selectedGame => (GameInfo)gameList.SelectedItem;
        Param.GameParam selectedGameParams = null;
        ModInfo selectedMod => listMods.SelectedItems.Count > 0 ? mods.Find(x => x.DisplayName == listMods.SelectedItems[0].Text) : null;

        private void Init()
        {
            instance = this;
            btnRemove.Click += btnRemove_Click;
            btnInstall.Click += btnInstall_Click;
            btnRestore.Click += btnRestore_Click;
            gameList.SelectedIndexChanged += gameList_Changed;
            btnOpenFolder.Click += btnOpenFolder_Click;
            btnDownloadUpdate.Click += btnDownloadUpdate_Click;

            Log.Init();

            if (!DesignMode)
            {
                Height = UnityModManagerApp.Properties.Settings.Default.WindowHeight;
            }

#if NET35
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
#else
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#endif
            if (!Utils.IsUnixPlatform())
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var registry = asm.GetType("Microsoft.Win32.Registry");
                    if (registry != null)
                    {
                        var getValue = registry.GetMethod("GetValue", new Type[] { typeof(string), typeof(string), typeof(object) });
                        if (getValue != null)
                        {
                            var exePath = getValue.Invoke(null, new object[] { REG_PATH, "ExePath", string.Empty }) as string;
                            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
                            {
                                var setValue = registry.GetMethod("SetValue", new Type[] { typeof(string), typeof(string), typeof(object) });
                                if (setValue != null)
                                {
                                    setValue.Invoke(null, new object[] { REG_PATH, "ExePath", Path.Combine(Application.StartupPath, "UnityModManager.exe") });
                                    setValue.Invoke(null, new object[] { REG_PATH, "Path", Application.StartupPath });
                                }
                            }
                        }
                        break;
                    }
                }
            }

            for (var i = (InstallType)0; i < InstallType.Count; i++)
            {
                var btn = new RadioButton();
                btn.Name = i.ToString();
                btn.Text = i.ToString();
                btn.Dock = DockStyle.Left;
                btn.AutoSize = true;
                btn.Click += installType_Click;
                installTypeGroup.Controls.Add(btn);
            }

            version = typeof(UnityModManager).Assembly.GetName().Version;
            currentVersion.Text = version.ToString();

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
                selected = selected ?? config.GameInfo.First();
                gameList.SelectedItem = selected;
                selectedGameParams = param.GetGameParam(selected);
            }
            else
            {
                InactiveForm();
                Log.Print($"Error parsing file '{Config.filename}'.");
                return;
            }

            CheckLastVersion();
        }

        private void installType_Click(object sender, EventArgs e)
        {
            var btn = (sender as RadioButton);
            if (!btn.Checked)
                return;

            selectedGameParams.InstallType = (InstallType)Enum.Parse(typeof(InstallType), btn.Name);

            RefreshForm();
        }

        private void UnityModLoaderForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (config == null)
                return;

            UnityModManagerApp.Properties.Settings.Default.WindowHeight = Height;
            UnityModManagerApp.Properties.Settings.Default.Save();
            param.Sync(config.GameInfo);
            param.Save();
        }

        private void InactiveForm()
        {
            btnInstall.Enabled = false;
            btnRemove.Enabled = false;
            btnRestore.Enabled = false;
            tabControl.TabPages[1].Enabled = false;
            installedVersion.Text = "-";

            foreach (var ctrl in installTypeGroup.Controls)
            {
                if (ctrl is RadioButton btn)
                {
                    btn.Enabled = false;
                }
            }
        }

        private bool IsValid(GameInfo gameInfo)
        {
            if (selectedGame == null)
            {
                Log.Print("Select a game.");
                return false;
            }

            var ignoreFields = new List<string>
            {
                nameof(GameInfo.GameExe),
                nameof(GameInfo.StartingPoint),
                nameof(GameInfo.UIStartingPoint),
                nameof(GameInfo.OldPatchTarget),
                nameof(GameInfo.GameVersionPoint),
                nameof(GameInfo.Comment),
                nameof(GameInfo.MinimalManagerVersion),
                nameof(GameInfo.ExtraFilesUrl)
            };

            var prefix = (!string.IsNullOrEmpty(gameInfo.Name) ? $"[{gameInfo.Name}]" : "[?]");
            var hasError = false;
            foreach (var field in typeof(GameInfo).GetFields())
            {
                if (!field.IsStatic && field.IsPublic && !ignoreFields.Exists(x => x == field.Name))
                {
                    var value = field.GetValue(gameInfo);
                    if (value == null || value.ToString() == "")
                    {
                        hasError = true;
                        Log.Print($"{prefix} Field '{field.Name}' is empty.");
                    }
                }
            }

            if (hasError)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(gameInfo.EntryPoint))
                if (!Utils.TryParseEntryPoint(gameInfo.EntryPoint, out _))
                {
                    return false;
                }

            if (!string.IsNullOrEmpty(gameInfo.StartingPoint))
                if (!Utils.TryParseEntryPoint(gameInfo.StartingPoint, out _))
                {
                    return false;
                }

            if (!string.IsNullOrEmpty(gameInfo.UIStartingPoint))
                if (!Utils.TryParseEntryPoint(gameInfo.UIStartingPoint, out _))
                {
                    return false;
                }

            if (!string.IsNullOrEmpty(gameInfo.OldPatchTarget))
                if (!Utils.TryParseEntryPoint(gameInfo.OldPatchTarget, out _))
                {
                    return false;
                }

            return true;
        }

        private void RefreshForm()
        {
            if (!IsValid(selectedGame))
            {
                InactiveForm();
                return;
            }

            btnInstall.Text = "Install";
            btnRestore.Enabled = false;

            gamePath = "";
            if (string.IsNullOrEmpty(selectedGameParams.Path) || !Directory.Exists(selectedGameParams.Path))
            {
                var result = FindGameFolder(selectedGame.Folder);
                if (string.IsNullOrEmpty(result))
                {
                    InactiveForm();
                    btnOpenFolder.ForeColor = System.Drawing.Color.FromArgb(192, 0, 0);
                    btnOpenFolder.Text = "Select";
                    folderBrowserDialog.SelectedPath = null;
                    Log.Print($"Game folder '{selectedGame.Folder}' not found.");
                    return;
                }
                Log.Print($"Game path detected as '{result}'.");
                selectedGameParams.Path = result;
            }

            if (!Utils.IsUnixPlatform() && !Directory.GetFiles(selectedGameParams.Path, "*.exe", SearchOption.TopDirectoryOnly).Any())
            {
                InactiveForm();
                Log.Print("Select the game folder where an exe file is located.");
                return;
            }

            if (Utils.IsMacPlatform() && !selectedGameParams.Path.EndsWith(".app"))
            {
                InactiveForm();
                Log.Print("Select the game folder where name ending with '.app'.");
                return;
            }

            Utils.TryParseEntryPoint(selectedGame.EntryPoint, out var assemblyName);

            gamePath = selectedGameParams.Path;
            btnOpenFolder.ForeColor = System.Drawing.Color.Black;
            btnOpenFolder.Text = new DirectoryInfo(gamePath).Name;
            folderBrowserDialog.SelectedPath = gamePath;
            if (File.Exists(Path.Combine(gamePath, "GameAssembly.dll")))
            {
                InactiveForm();
                Log.Print("This game version (IL2CPP) is not supported.");
                return;
            }
            managedPath = FindManagedFolder(gamePath);
            if (managedPath == null)
            {
                InactiveForm();
                Log.Print("Select the game folder that contains the 'Data' folder.");
                return;
            }
            managerPath = Path.Combine(managedPath, nameof(UnityModManager));
            entryAssemblyPath = Path.Combine(managedPath, assemblyName);
            injectedEntryAssemblyPath = entryAssemblyPath;
            managerAssemblyPath = Path.Combine(managerPath, typeof(UnityModManager).Module.Name);
            entryPoint = selectedGame.EntryPoint;
            injectedEntryPoint = selectedGame.EntryPoint;
            assemblyDef = null;
            injectedAssemblyDef = null;
            managerDef = null;

            if (!string.IsNullOrEmpty(selectedGame.GameExe))
            {
                if (selectedGame.GameExe.Contains('*'))
                {
                    foreach (var file in new DirectoryInfo(gamePath).GetFiles(selectedGame.GameExe, SearchOption.TopDirectoryOnly))
                    {
                        selectedGame.GameExe = file.Name;
                    }
                }
                gameExePath = Path.Combine(gamePath, selectedGame.GameExe);
            }
            else
            {
                gameExePath = string.Empty;
            }

            doorstopPath = Path.Combine(gamePath, doorstopFilename);
            doorstopConfigPath = Path.Combine(gamePath, doorstopConfigFilename);

            var gameSupportVersion = !string.IsNullOrEmpty(selectedGame.MinimalManagerVersion) ? Utils.ParseVersion(selectedGame.MinimalManagerVersion) : VER_0_22;
            libraryPaths = new List<string>();
            foreach (var item in libraryFiles)
            {
                if ((item.Value & LibIncParam.Minimal_lt_0_22) > 0 && gameSupportVersion >= VER_0_22)
                {
                    continue;
                }
                libraryPaths.Add(Path.Combine(managerPath, item.Key));
            }

            var parent = new DirectoryInfo(Application.StartupPath).Parent;
            for(int i = 0; i < 3; i++)
            {
                if (parent == null)
                    break;

                if (parent.FullName == gamePath)
                {
                    InactiveForm();
                    Log.Print("UMM Installer should not be located in the game folder.");
                    return;
                }
                parent = parent.Parent;
            }

            //machineConfigPath = string.Empty;
            //machineDoc = null;

            //if (!string.IsNullOrEmpty(selectedGame.MachineConfig))
            //{
            //    machineConfigPath = Path.Combine(gamePath, selectedGame.MachineConfig);
            //    try
            //    {
            //        machineDoc = XDocument.Load(machineConfigPath);
            //    }
            //    catch (Exception e)
            //    {
            //        InactiveForm();
            //        Log.Print(e.ToString());
            //        return;
            //    }
            //}

            try
            {
                assemblyDef = ModuleDefMD.Load(File.ReadAllBytes(entryAssemblyPath));
            }
            catch (Exception e)
            {
                InactiveForm();
                Log.Print(e.ToString() + Environment.NewLine + entryAssemblyPath);
                return;
            }

            var useOldPatchTarget = false;
            GameInfo.filepathInGame = Path.Combine(managerPath, "Config.xml");
            if (File.Exists(GameInfo.filepathInGame))
            {
                var gameConfig = GameInfo.ImportFromGame();
                if(gameConfig == null || !Utils.TryParseEntryPoint(gameConfig.EntryPoint, out assemblyName))
                {
                    InactiveForm();
                    return;
                }
                injectedEntryPoint = gameConfig.EntryPoint;
                injectedEntryAssemblyPath = Path.Combine(managedPath, assemblyName);
            }
            else if (!string.IsNullOrEmpty(selectedGame.OldPatchTarget))
            {
                if (!Utils.TryParseEntryPoint(selectedGame.OldPatchTarget, out assemblyName))
                {
                    InactiveForm();
                    return;
                }
                useOldPatchTarget = true;
                injectedEntryPoint = selectedGame.OldPatchTarget;
                injectedEntryAssemblyPath = Path.Combine(managedPath, assemblyName);
            }

            try
            {
                if (injectedEntryAssemblyPath == entryAssemblyPath)
                {
                    injectedAssemblyDef = assemblyDef;
                }
                else
                {
                    injectedAssemblyDef = ModuleDefMD.Load(File.ReadAllBytes(injectedEntryAssemblyPath));
                }
                if (File.Exists(managerAssemblyPath))
                    managerDef = ModuleDefMD.Load(File.ReadAllBytes(managerAssemblyPath));
            }
            catch (Exception e)
            {
                InactiveForm();
                Log.Print(e.ToString() + Environment.NewLine + injectedEntryAssemblyPath + Environment.NewLine + managerAssemblyPath);
                return;
            }

            var disabledMethods = new List<InstallType>();
            var unavailableMethods = new List<InstallType>();

            var managerType = typeof(UnityModManager);
            var starterType = typeof(Injection.UnityModManagerStarter);

            Rescan:
            var v0_12_Installed = injectedAssemblyDef.Types.FirstOrDefault(x => x.Name == managerType.Name);
            var newWayInstalled = injectedAssemblyDef.Types.FirstOrDefault(x => x.Name == starterType.Name);
            var hasInjectedAssembly = v0_12_Installed != null || newWayInstalled != null;

            if (useOldPatchTarget && !hasInjectedAssembly)
            {
                useOldPatchTarget = false;
                injectedEntryPoint = selectedGame.EntryPoint;
                injectedEntryAssemblyPath = entryAssemblyPath;
                injectedAssemblyDef = assemblyDef;
                goto Rescan;
            }

            //if (machineDoc == null)
            //{
            //    unavailableMethods.Add(InstallType.Config);
            //    selectedGameParams.InstallType = InstallType.Assembly;
            //}
            //else if (hasInjectedAssembly)
            //{
            //    disabledMethods.Add(InstallType.Config);
            //    selectedGameParams.InstallType = InstallType.Assembly;
            //}
            //else if (machineDoc.Descendants("cryptoClass").Any(x => x.HasAttributes && x.FirstAttribute.Name.LocalName == "ummRngWrapper"))
            //{
            //    disabledMethods.Add(InstallType.Assembly);
            //    selectedGameParams.InstallType = InstallType.Config;
            //}

            if (Utils.IsUnixPlatform() || !File.Exists(gameExePath))
            {
                unavailableMethods.Add(InstallType.DoorstopProxy);
                selectedGameParams.InstallType = InstallType.Assembly;
            }
            else if (File.Exists(doorstopPath))
            {
                disabledMethods.Add(InstallType.Assembly);
                selectedGameParams.InstallType = InstallType.DoorstopProxy;
            }
            
            if (hasInjectedAssembly)
            {
                disabledMethods.Add(InstallType.DoorstopProxy);
                selectedGameParams.InstallType = InstallType.Assembly;
            }

            foreach (var ctrl in installTypeGroup.Controls)
            {
                if (ctrl is RadioButton btn)
                {
                    if (unavailableMethods.Exists(x => x.ToString() == btn.Name))
                    {
                        btn.Visible = false;
                        btn.Enabled = false;
                        continue;
                    }
                    if (disabledMethods.Exists(x => x.ToString() == btn.Name))
                    {
                        btn.Visible = true;
                        btn.Enabled = false;
                        continue;
                    }

                    btn.Visible = true;
                    btn.Enabled = true;
                    btn.Checked = btn.Name == selectedGameParams.InstallType.ToString();
                }
            }

            installTypeGroup.PerformLayout();

            //if (selectedGameParams.InstallType == InstallType.Config)
            //{
            //    btnRestore.Enabled = IsDirty(machineDoc) && File.Exists($"{machineConfigPath}.original_");
            //}

            if (selectedGameParams.InstallType == InstallType.Assembly)
            {
                btnRestore.Enabled = IsDirty(injectedAssemblyDef) && File.Exists($"{injectedEntryAssemblyPath}.original_");
            }

            tabControl.TabPages[1].Enabled = true;

            managerDef = managerDef ?? injectedAssemblyDef;

            var managerInstalled = managerDef.Types.FirstOrDefault(x => x.Name == managerType.Name);
            if (managerInstalled != null && (hasInjectedAssembly || selectedGameParams.InstallType == InstallType.DoorstopProxy))
            {
                btnInstall.Text = "Update";
                btnInstall.Enabled = false;
                btnRemove.Enabled = true;

                Version version2;
                if (v0_12_Installed != null)
                {
                    var versionString = managerInstalled.Fields.First(x => x.Name == nameof(UnityModManager.version)).Constant.Value.ToString();
                    version2 = Utils.ParseVersion(versionString);
                }
                else
                {
                    version2 = managerDef.Assembly.Version;
                }
                
                installedVersion.Text = version2.ToString();
                if (version > version2 && v0_12_Installed == null)
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
        }

        //private void btnRunGame_SizeChanged(object sender, EventArgs e)
        //{
        //    var btn = sender as Button;
        //    btn.Location = new System.Drawing.Point((int)(btn.Parent.Size.Width / 2f - btn.Size.Width / 2f), btn.Location.Y);
        //}

        //private void btnRunGame_Click(object sender, EventArgs e)
        //{
            //Process.Start(gameExePath);
        //}

        private string FindGameFolder(string str)
        {
            string[] disks = new string[] { @"C:\", @"D:\", @"E:\", @"F:\" };
            string[] roots = new string[] { "Games", "Program files", "Program files (x86)", "" };
            string[] folders = new string[] { @"Steam\SteamApps\common", @"GoG Galaxy\Games", "" };
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                disks = new string[] { Environment.GetEnvironmentVariable("HOME") };
                roots = new string[] { "Library/Application Support", ".steam" };
                folders = new string[] { "Steam/SteamApps/common", "steam/steamapps/common", "Steam/steamapps/common" };
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
                            if (Utils.IsMacPlatform())
                            {
                                foreach (var dir in Directory.GetDirectories(path))
                                {
                                    if (dir.EndsWith(".app"))
                                    {
                                        path = Path.Combine(path, dir);
                                        break;
                                    }
                                }
                            }
                            return path;
                        }
                    }
                }
            }
            return null;
        }

        private string FindManagedFolder(string path)
        {
            if (Utils.IsMacPlatform())
            {
                var dir = $"{path}/Contents/Resources/Data/Managed";
                if (Directory.Exists(dir))
                {
                    return dir;
                }
            }

            foreach (var di in new DirectoryInfo(path).GetDirectories())
            {
                if ((di.Attributes & System.IO.FileAttributes.ReparsePoint) != 0)
                    continue;

                var dir = di.FullName;
                if (dir.EndsWith("Managed"))
                {
                    if (File.Exists(Path.Combine(dir, "Assembly-CSharp.dll")) || File.Exists(Path.Combine(dir, "UnityEngine.dll")))
                    {
                        return dir;
                    }
                }
                var result = FindManagedFolder(dir);
                if (!string.IsNullOrEmpty(result))
                    return result;
            }

            return null;
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (!TestWritePermissions())
            {
                return;
            }
            //if (selectedGameParams.InstallType == InstallType.Config)
            //{
            //    InjectConfig(Actions.Remove, machineDoc);
            //}
            if (selectedGameParams.InstallType == InstallType.DoorstopProxy)
            {
                InstallDoorstop(Actions.Remove);
            }
            else
            {
                InjectAssembly(Actions.Remove, injectedAssemblyDef);
            }

            RefreshForm();
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            if (!TestWritePermissions())
            {
                return;
            }
            if (!TestCompatibility())
            {
                return;
            }
            var modsPath = Path.Combine(gamePath, selectedGame.ModsDirectory);
            if (!Directory.Exists(modsPath))
            {
                Directory.CreateDirectory(modsPath);
            }

            //if (selectedGameParams.InstallType == InstallType.Config)
            //{
            //    InjectConfig(Actions.Install, machineDoc);
            //}
            if (selectedGameParams.InstallType == InstallType.DoorstopProxy)
            {
                InstallDoorstop(Actions.Install);
            }
            else
            {
                InjectAssembly(Actions.Install, assemblyDef);
            }

            RefreshForm();
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            //if (selectedGameParams.InstallType == InstallType.Config)
            //{
            //    var originalConfigPath = $"{machineConfigPath}.original_";
            //    RestoreOriginal(machineConfigPath, originalConfigPath);
            //}
            //else
            //{
                
            //}

            if (selectedGameParams.InstallType == InstallType.Assembly)
            {
                var injectedEntryAssemblyPath = Path.Combine(managedPath, injectedAssemblyDef.Name);
                var originalAssemblyPath = $"{injectedEntryAssemblyPath}.original_";
                RestoreOriginal(injectedEntryAssemblyPath, originalAssemblyPath);
            }

            RefreshForm();
        }

        private void btnDownloadUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnDownloadUpdate.Text == "Home Page")
                {
                    if (!string.IsNullOrEmpty(config.HomePage))
                        Process.Start(config.HomePage);
                }
                else
                {
                    Process.Start("Downloader.exe");
                }
            }
            catch(Exception ex)
            {
                Log.Print(ex.ToString());
            }
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                selectedGameParams.Path = folderBrowserDialog.SelectedPath;
                RefreshForm();
            }
        }

        private void gameList_Changed(object sender, EventArgs e)
        {
            notesTextBox.Text = "";
            additionallyGroupBox.Visible = false;
            extraFilesTextBox.Text = "";
            extraFilesGroupBox.Visible = false;

            var selected = (GameInfo)((ComboBox)sender).SelectedItem;
            if (selected != null)
            {
                Log.Print($"Game changed to '{selected.Name}'.");
                param.LastSelectedGame = selected.Name;
                selectedGameParams = param.GetGameParam(selected);
                if (!string.IsNullOrEmpty(selectedGameParams.Path))
                    Log.Print($"Game path '{selectedGameParams.Path}'.");

                if (!string.IsNullOrEmpty(selected.Comment))
                {
                    notesTextBox.Text = selected.Comment;
                    additionallyGroupBox.Visible = true;
                }
                
                if (!string.IsNullOrEmpty(selected.ExtraFilesUrl))
                {
                    extraFilesTextBox.Text = $"Click on the Manual and unzip archive to game folder. Or click on the Auto for automatic installation. This must be done before installing mod loader to game.";
                    extraFilesGroupBox.Visible = true;
                }
            }

            RefreshForm();
        }

        enum Actions
        {
            Install,
            Remove
        };

        private bool InstallDoorstop(Actions action, bool write = true)
        {
            var gameConfigPath = GameInfo.filepathInGame;

            var success = false;
            switch (action)
            {
                case Actions.Install:
                    try
                    {
                        Log.Print("=======================================");

                        if (!Directory.Exists(managerPath))
                            Directory.CreateDirectory(managerPath);

                        Utils.MakeBackup(doorstopPath);
                        Utils.MakeBackup(doorstopConfigPath);
                        Utils.MakeBackup(libraryPaths);

                        if (!InstallDoorstop(Actions.Remove, false))
                        {
                            Log.Print("Installation failed. Can't uninstall the previous version.");
                            goto EXIT;
                        }

                        Log.Print($"Copying files to game...");
                        var arch = Utils.UnmanagedDllIs64Bit(gameExePath);
                        var filename = arch == true ? "winhttp_x64.dll" : "winhttp_x86.dll";
                        Log.Print($"  '{filename}'");
                        File.Copy(filename, doorstopPath, true);
                        Log.Print($"  '{doorstopConfigFilename}'");
                        File.WriteAllText(doorstopConfigPath, "[UnityDoorstop]" + Environment.NewLine + "enabled = true" + Environment.NewLine + "targetAssembly = " + managerAssemblyPath);

                        DoactionLibraries(Actions.Install);
                        DoactionGameConfig(Actions.Install);
                        Log.Print("Installation was successful.");

                        success = true;
                    }
                    catch (Exception e)
                    {
                        Log.Print(e.ToString());
                        Utils.RestoreBackup(doorstopPath);
                        Utils.RestoreBackup(doorstopConfigPath);
                        Utils.RestoreBackup(libraryPaths);
                        Utils.RestoreBackup(gameConfigPath);
                        Log.Print("Installation failed.");
                    }
                    break;

                case Actions.Remove:
                    try
                    {
                        if (write)
                        {
                            Log.Print("=======================================");
                        }

                        Utils.MakeBackup(gameConfigPath);
                        if (write)
                        {
                            
                            Utils.MakeBackup(doorstopPath);
                            Utils.MakeBackup(doorstopConfigPath);
                            Utils.MakeBackup(libraryPaths);
                        }

                        Log.Print($"Deleting files from game...");
                        Log.Print($"  '{doorstopFilename}'");
                        File.Delete(doorstopPath);
                        Log.Print($"  '{doorstopConfigFilename}'");
                        File.Delete(doorstopConfigPath);

                        if (write)
                        {
                            DoactionLibraries(Actions.Remove);
                            DoactionGameConfig(Actions.Remove);
                            Log.Print("Removal was successful.");
                        }

                        success = true;
                    }
                    catch (Exception e)
                    {
                        Log.Print(e.ToString());
                        if (write)
                        {
                            Utils.RestoreBackup(doorstopPath);
                            Utils.RestoreBackup(doorstopConfigPath);
                            Utils.RestoreBackup(libraryPaths);
                            Utils.RestoreBackup(gameConfigPath);
                            Log.Print("Removal failed.");
                        }
                    }
                    break;
            }

            EXIT:

            if (write)
            {
                try
                {
                    Utils.DeleteBackup(doorstopPath);
                    Utils.DeleteBackup(doorstopConfigPath);
                    Utils.DeleteBackup(libraryPaths);
                    Utils.DeleteBackup(gameConfigPath);
                }
                catch (Exception)
                {
                }
            }

            return success;
        }

        //private bool InjectConfig(Actions action, XDocument doc = null, bool write = true)
        //{
        //    var originalMachineConfigPath = $"{machineConfigPath}.original_";
        //    var gameConfigPath = GameInfo.filepathInGame;

        //    var success = false;

        //    switch (action)
        //    {
        //        case Actions.Install:
        //            try
        //            {
        //                if (!Directory.Exists(managerPath))
        //                    Directory.CreateDirectory(managerPath);

        //                Utils.MakeBackup(machineConfigPath);
        //                Utils.MakeBackup(libraryPaths);

        //                if (!IsDirty(doc))
        //                {
        //                    File.Copy(machineConfigPath, originalMachineConfigPath, true);
        //                }
        //                MakeDirty(doc);

        //                if (!InjectConfig(Actions.Remove, doc, false))
        //                {
        //                    Log.Print("Installation failed. Can't uninstall the previous version.");
        //                    goto EXIT;
        //                }

        //                Log.Print($"Applying patch to '{Path.GetFileName(machineConfigPath)}'...");

        //                foreach (var mapping in doc.Descendants("cryptoNameMapping"))
        //                {
        //                    foreach(var cryptoClasses in mapping.Elements("cryptoClasses"))
        //                    {
        //                        if (!cryptoClasses.Elements("cryptoClass").Any(x => x.FirstAttribute.Name.LocalName == "ummRngWrapper"))
        //                        {
        //                            cryptoClasses.Add(new XElement("cryptoClass", new XAttribute("ummRngWrapper", "UnityModManagerNet.RngWrapper, UnityModManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")));
        //                        }
        //                    }
        //                    if (!mapping.Elements("nameEntry").Any(x => x.LastAttribute.Value == "ummRngWrapper"))
        //                    {
        //                        //mapping.Add(new XElement("nameEntry", new XAttribute("name", "RandomNumberGenerator"), new XAttribute("class", "ummRngWrapper")));
        //                        mapping.Add(new XElement("nameEntry", new XAttribute("name", "System.Security.Cryptography.RandomNumberGenerator"), new XAttribute("class", "ummRngWrapper")));
        //                    }
        //                    break;
        //                }

        //                doc.Save(machineConfigPath);
        //                DoactionLibraries(Actions.Install);
        //                DoactionGameConfig(Actions.Install);
        //                Log.Print("Installation was successful.");

        //                success = true;
        //            }
        //            catch (Exception e)
        //            {
        //                Log.Print(e.ToString());
        //                Utils.RestoreBackup(machineConfigPath);
        //                Utils.RestoreBackup(libraryPaths);
        //                Utils.RestoreBackup(gameConfigPath);
        //                Log.Print("Installation failed.");
        //            }

        //            break;

        //        case Actions.Remove:
        //            try
        //            {
        //                Utils.MakeBackup(gameConfigPath);
        //                if (write)
        //                {
        //                    Utils.MakeBackup(machineConfigPath);
        //                    Utils.MakeBackup(libraryPaths);
        //                }

        //                Log.Print("Removing patch...");

        //                MakeDirty(doc);

        //                foreach (var mapping in doc.Descendants("cryptoNameMapping"))
        //                {
        //                    foreach (var cryptoClasses in mapping.Elements("cryptoClasses"))
        //                    {
        //                        foreach (var cryptoClass in cryptoClasses.Elements("cryptoClass"))
        //                        {
        //                            if (cryptoClass.FirstAttribute.Name.LocalName == "ummRngWrapper")
        //                            {
        //                                cryptoClass.Remove();
        //                            }
        //                        }
        //                    }
        //                    foreach (var nameEntry in mapping.Elements("nameEntry"))
        //                    {
        //                        if (nameEntry.LastAttribute.Value == "ummRngWrapper")
        //                        {
        //                            nameEntry.Remove();
        //                        }
        //                    }
        //                    break;
        //                }

        //                if (write)
        //                {
        //                    doc.Save(machineConfigPath);
        //                    DoactionLibraries(Actions.Remove);
        //                    DoactionGameConfig(Actions.Remove);
        //                    Log.Print("Removal was successful.");
        //                }

        //                success = true;
        //            }
        //            catch (Exception e)
        //            {
        //                Log.Print(e.ToString());
        //                if (write)
        //                {
        //                    Utils.RestoreBackup(machineConfigPath);
        //                    Utils.RestoreBackup(libraryPaths);
        //                    Utils.RestoreBackup(gameConfigPath);
        //                    Log.Print("Removal failed.");
        //                }
        //            }

        //            break;
        //    }

        //    EXIT:

        //    if (write)
        //    {
        //        try
        //        {
        //            Utils.DeleteBackup(machineConfigPath);
        //            Utils.DeleteBackup(libraryPaths);
        //            Utils.DeleteBackup(gameConfigPath);
        //        }
        //        catch (Exception)
        //        {
        //        }
        //    }

        //    return success;
        //}

        private bool InjectAssembly(Actions action, ModuleDefMD assemblyDef, bool write = true)
        {
            var managerType = typeof(UnityModManager);
            var starterType = typeof(Injection.UnityModManagerStarter);
            var gameConfigPath = GameInfo.filepathInGame;

            var assemblyPath = Path.Combine(managedPath, assemblyDef.Name);
            var originalAssemblyPath = $"{assemblyPath}.original_";

            var success = false;

            switch (action)
            {
                case Actions.Install:
                    {
                        try
                        {
                            Log.Print("=======================================");

                            if (!Directory.Exists(managerPath))
                                Directory.CreateDirectory(managerPath);

                            Utils.MakeBackup(assemblyPath);
                            Utils.MakeBackup(libraryPaths);

                            if (!IsDirty(assemblyDef))
                            {
                                File.Copy(assemblyPath, originalAssemblyPath, true);
                                MakeDirty(assemblyDef);
                            }

                            if (!InjectAssembly(Actions.Remove, injectedAssemblyDef, assemblyDef != injectedAssemblyDef))
                            {
                                Log.Print("Installation failed. Can't uninstall the previous version.");
                                goto EXIT;
                            }

                            Log.Print($"Applying patch to '{Path.GetFileName(assemblyPath)}'...");

                            if (!Utils.TryGetEntryPoint(assemblyDef, entryPoint, out var methodDef, out var insertionPlace, true))
                            {
                                goto EXIT;
                            }

                            var starterDef = ModuleDefMD.Load(starterType.Module);
                            var starter = starterDef.Types.First(x => x.Name == starterType.Name);
                            starterDef.Types.Remove(starter);
                            assemblyDef.Types.Add(starter);

                            var instr = OpCodes.Call.ToInstruction(starter.Methods.First(x => x.Name == nameof(Injection.UnityModManagerStarter.Start)));
                            if (insertionPlace == "before")
                            {
                                methodDef.Body.Instructions.Insert(0, instr);
                            }
                            else
                            {
                                methodDef.Body.Instructions.Insert(methodDef.Body.Instructions.Count - 1, instr);
                            }

                            assemblyDef.Write(assemblyPath);
                            DoactionLibraries(Actions.Install);
                            DoactionGameConfig(Actions.Install);

                            Log.Print("Installation was successful.");

                            success = true;
                        }
                        catch (Exception e)
                        {
                            Log.Print(e.ToString());
                            Utils.RestoreBackup(assemblyPath);
                            Utils.RestoreBackup(libraryPaths);
                            Utils.RestoreBackup(gameConfigPath);
                            Log.Print("Installation failed.");
                        }
                    }
                    break;

                case Actions.Remove:
                    {
                        try
                        {
                            if (write)
                            {
                                Log.Print("=======================================");
                            }

                            Utils.MakeBackup(gameConfigPath);

                            var v0_12_Installed = assemblyDef.Types.FirstOrDefault(x => x.Name == managerType.Name);
                            var newWayInstalled = assemblyDef.Types.FirstOrDefault(x => x.Name == starterType.Name);

                            if (v0_12_Installed != null || newWayInstalled != null)
                            {
                                if (write)
                                {
                                    Utils.MakeBackup(assemblyPath);
                                    Utils.MakeBackup(libraryPaths);
                                }

                                Log.Print("Removing patch...");

                                Instruction instr = null;

                                if (newWayInstalled != null)
                                {
                                    instr = OpCodes.Call.ToInstruction(newWayInstalled.Methods.First(x => x.Name == nameof(Injection.UnityModManagerStarter.Start)));
                                }
                                else if (v0_12_Installed != null)
                                {
                                    instr = OpCodes.Call.ToInstruction(v0_12_Installed.Methods.First(x => x.Name == nameof(UnityModManager.Start)));
                                }

                                if (!string.IsNullOrEmpty(injectedEntryPoint))
                                {
                                    if (!Utils.TryGetEntryPoint(assemblyDef, injectedEntryPoint, out var methodDef, out _, true))
                                    {
                                        goto EXIT;
                                    }

                                    for (int i = 0; i < methodDef.Body.Instructions.Count; i++)
                                    {
                                        if (methodDef.Body.Instructions[i].OpCode == instr.OpCode && methodDef.Body.Instructions[i].Operand == instr.Operand)
                                        {
                                            methodDef.Body.Instructions.RemoveAt(i);
                                            break;
                                        }
                                    }
                                }

                                if (newWayInstalled != null)
                                    assemblyDef.Types.Remove(newWayInstalled);
                                else if (v0_12_Installed != null)
                                    assemblyDef.Types.Remove(v0_12_Installed);

                                if (!IsDirty(assemblyDef))
                                {
                                    MakeDirty(assemblyDef);
                                }

                                if (write)
                                {
                                    assemblyDef.Write(assemblyPath);
                                    DoactionLibraries(Actions.Remove);
                                    DoactionGameConfig(Actions.Remove);
                                    Log.Print("Removal was successful.");
                                }
                            }

                            success = true;
                        }
                        catch (Exception e)
                        {
                            Log.Print(e.ToString());
                            if (write)
                            {
                                Utils.RestoreBackup(assemblyPath);
                                Utils.RestoreBackup(libraryPaths);
                                Utils.RestoreBackup(gameConfigPath);
                                Log.Print("Removal failed.");
                            }
                        }
                    }
                    break;
            }

            EXIT:

            if (write)
            {
                try
                {
                    Utils.DeleteBackup(assemblyPath);
                    Utils.DeleteBackup(libraryPaths);
                    Utils.DeleteBackup(gameConfigPath);
                }
                catch (Exception)
                {
                }
            }

            return success;
        }

        //private static bool IsDirty(XDocument doc)
        //{
        //    return doc.Root.Element("mscorlib").Attribute(nameof(UnityModManager)) != null;
        //}

        //private static void MakeDirty(XDocument doc)
        //{
        //    doc.Root.Element("mscorlib").SetAttributeValue(nameof(UnityModManager), UnityModManager.version);
        //}

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

        private bool TestWritePermissions()
        {
            var success = true;

            if (selectedGameParams.InstallType == InstallType.DoorstopProxy)
            {
                success &= Utils.RemoveReadOnly(doorstopPath);
                success &= Utils.RemoveReadOnly(doorstopConfigPath);
            }
            else
            {
                success &= Utils.RemoveReadOnly(entryAssemblyPath);
                if (injectedEntryAssemblyPath != entryAssemblyPath)
                    success &= Utils.RemoveReadOnly(injectedEntryAssemblyPath);
            }

            if (Directory.Exists(managerPath))
            {
                foreach (var f in Directory.GetFiles(managerPath))
                {
                    success &= Utils.RemoveReadOnly(f);
                }
            }

            if (!success)
                return false;

            success &= Utils.IsDirectoryWritable(managedPath);
            success &= Utils.IsFileWritable(managerAssemblyPath);
            success &= Utils.IsFileWritable(GameInfo.filepathInGame);

            foreach (var file in libraryPaths)
            {
                success &= Utils.IsFileWritable(file);
            }

            //if (machineDoc != null)
            //{
            //    success = Utils.IsFileWritable(machineConfigPath) && success;
            //}
            if (selectedGameParams.InstallType == InstallType.DoorstopProxy)
            {
                success &= Utils.IsFileWritable(doorstopPath);
                success &= Utils.IsFileWritable(doorstopConfigPath);
            }
            else
            {
                success &= Utils.IsFileWritable(entryAssemblyPath);
                if (injectedEntryAssemblyPath != entryAssemblyPath)
                    success &= Utils.IsFileWritable(injectedEntryAssemblyPath);
            }

            return success;
        }

        private bool TestCompatibility()
        {
            foreach (var f in new DirectoryInfo(gamePath).GetFiles("0Harmony.dll", SearchOption.AllDirectories))
            {
                if (!f.FullName.EndsWith(Path.Combine("UnityModManager", "0Harmony.dll")))
                {
                    var asm = Assembly.ReflectionOnlyLoad(File.ReadAllBytes(f.FullName));
                    if (asm.GetName().Version < HARMONY_VER)
                    {
                        Log.Print($"Game has extra library 0Harmony.dll in path {f.FullName}, which may not be compatible with UMM. Recommended to delete it.");
                        return false;
                    }
                    Log.Print($"Game has extra library 0Harmony.dll in path {f.FullName}.");
                }
            }

            return true;
        }

        private static bool RestoreOriginal(string file, string backup)
        {
            try
            {
                File.Copy(backup, file, true);
                Log.Print("Original files restored.");
                File.Delete(backup);
                return true;
            }
            catch (Exception e)
            {
                Log.Print(e.Message);
            }

            return false;
        }

        private static void DoactionLibraries(Actions action)
        {
            if (action == Actions.Install)
            {
                Log.Print($"Copying files to game...");
            }
            else
            {
                Log.Print($"Deleting files from game...");
            }

            foreach(var destpath in libraryPaths)
            {
                var filename = Path.GetFileName(destpath);
                if (action == Actions.Install)
                {
                    var sourcepath = Path.Combine(Application.StartupPath, filename);
                    if (File.Exists(destpath))
                    {
                        var source = new FileInfo(sourcepath);
                        var dest = new FileInfo(destpath);
                        if (dest.LastWriteTimeUtc == source.LastWriteTimeUtc)
                            continue;

                        //File.Copy(path, $"{path}.old_", true);
                    }

                    Log.Print($"  {filename}");
                    File.Copy(sourcepath, destpath, true);
                }
                else
                {
                    if (File.Exists(destpath))
                    {
                        Log.Print($"  {filename}");
                        File.Delete(destpath);
                    }
                }
            }
        }

        private void DoactionGameConfig(Actions action)
        {
            if (action == Actions.Install)
            {
                Log.Print($"Creating configs...");
                Log.Print($"  Config.xml");

                selectedGame.ExportToGame();
            }
            else
            {
                Log.Print($"Deleting configs...");
                if (File.Exists(GameInfo.filepathInGame))
                {
                    Log.Print($"  Config.xml");
                    File.Delete(GameInfo.filepathInGame);
                }
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

        private void notesTextBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }

        private void extraFilesAutoButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(selectedGame.ExtraFilesUrl))
            {
                var form = new DownloadExtraFiles(selectedGame.ExtraFilesUrl, gamePath);
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                }
            }
        }

        private void extraFilesManualButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(selectedGame.ExtraFilesUrl))
            {
                Process.Start(selectedGame.ExtraFilesUrl);
            }
        }
    }
}
