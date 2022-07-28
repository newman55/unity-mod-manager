using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;

namespace UnityModManagerNet.ConsoleInstaller
{
    class UnityModManagerConsole
    {
        const string REG_PATH = @"HKEY_CURRENT_USER\Software\UnityModManager";

        public static readonly Version VER_0_13 = new Version(0, 13);
        public static readonly Version VER_0_22 = new Version(0, 22);

        internal static readonly Version HARMONY_VER = new Version(2, 0);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hwnd);

        [Flags]
        enum Actions { Install = 1, Update = 2, Delete = 4, Restore = 8 }

        static void Main(string[] args)
        {
            try
            {
                if (CheckApplicationAlreadyRunning(out var process))
                {
                    if (!Utils.IsUnixPlatform())
                        SetForegroundWindow(process.MainWindowHandle);
                    return;
                }
            }
            catch (Exception _) { }

            Init();

            System.Console.ReadKey();
        }

        static bool CheckApplicationAlreadyRunning(out Process result)
        {
            result = null;
            var id = Process.GetCurrentProcess().Id;
            var name = Process.GetCurrentProcess().ProcessName;
            foreach (var p in Process.GetProcessesByName(name))
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
        enum LibIncParam { Normal = 0, Skip = 1, Minimal_lt_0_22 = 2 }

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

        static GameInfo selectedGame;
        static Param.GameParam selectedGameParams = null;

        private static void Init()
        {
            Log.Init<Log>();
#if NET35
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls| (SecurityProtocolType)768 | (SecurityProtocolType)3072;
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
                                    setValue.Invoke(null, new object[] { REG_PATH, "ExePath", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UnityModManager.exe") });
                                    setValue.Invoke(null, new object[] { REG_PATH, "Path", AppDomain.CurrentDomain.BaseDirectory });
                                }
                            }
                        }
                        break;
                    }
                }
            }

            version = typeof(UnityModManager).Assembly.GetName().Version;

            config = Config.Load();
            param = Param.Load();

            if (config != null && config.GameInfo != null && config.GameInfo.Length > 0)
            {
                config.GameInfo = config.GameInfo.OrderBy(x => x.Name).ToArray();

                if (!string.IsNullOrEmpty(param.LastSelectedGame))
                {
                    selectedGame = config.GameInfo.FirstOrDefault(x => x.Name == param.LastSelectedGame);
                    selectedGameParams = param.GetGameParam(selectedGame);
                }
                if (selectedGame != null)
                {
                    Log.Print($"Selected '{selectedGame}' game. Do you want to change selection? Yes:<y> Continue:<any>");
                    var k = Console.ReadKey();
                    if (k.Key == ConsoleKey.Y)
                    {
                        SelectGame();
                    }
                }
                else
                {
                    SelectGame();
                }
            }
            else
            {
                Log.Print($"Error parsing file '{Config.filename}'.");
                Close();
                return;
            }

            ReadGameAssets();

            //CheckLastVersion();
        }

        static void SelectGame()
        {
            Log.Print($"Enter a number between 1 and {config.GameInfo.Length}.");
            int i = 1;
            foreach (var g in config.GameInfo)
            {
                Log.Print($"{i++}. {g}");
            }
            Log.Print($"Enter a number between 1 and {config.GameInfo.Length}.");
            ReadAgain:
            Log.Print("Number: ");
            var l = Console.ReadLine();
            if (string.IsNullOrEmpty(l))
            {
                Close();
                return;
            }
            if (int.TryParse(l, out var n))
            {
                if (n > 0 && n <= config.GameInfo.Length)
                {
                    selectedGame = config.GameInfo[n - 1];
                }
                else
                {
                    Log.Print($"The number must be between 1 and {config.GameInfo.Length}.");
                    goto ReadAgain;
                }
            }
            else
            {
                Log.Print($"The key must be a number.");
                goto ReadAgain;
            }
            Log.Print($"Selected '{selectedGame}' game.");
            if (!IsValid(selectedGame))
            {
                Close();
            }
            else
            {
                selectedGameParams = param.GetGameParam(selectedGame);
                param.LastSelectedGame = selectedGame.Name;
            }
        }

        static void Close()
        {
            Log.Print("Terminate.");
            Process.GetCurrentProcess().CloseMainWindow();
        }

        static bool IsValid(GameInfo gameInfo)
        {
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

        static void ReadGameAssets()
        {
            gamePath = "";
            if (string.IsNullOrEmpty(selectedGameParams.Path) || !Directory.Exists(selectedGameParams.Path))
            {
                var result = Utils.FindGameFolder(selectedGame.Folder);
                if (string.IsNullOrEmpty(result))
                {
                    Log.Print($"Game folder '{selectedGame.Folder}' not found.");
                    SelectGameFolder();
                }
                else
                {
                    selectedGameParams.Path = result;
                    Log.Print($"Game path detected as '{result}'. Do you want to change it? Yes:<y> Continue:<any>");
                    var k = Console.ReadKey(true);
                    if (k.Key == ConsoleKey.Y)
                    {
                        SelectGameFolder();
                    }
                }
            }

            if (!Utils.IsUnixPlatform() && !Directory.GetFiles(selectedGameParams.Path, "*.exe", SearchOption.TopDirectoryOnly).Any())
            {
                Log.Print("Select the game folder where an exe file is located.");
                SelectGameFolder();
            }

            if (Utils.IsMacPlatform() && !selectedGameParams.Path.EndsWith(".app"))
            {
                Log.Print("Select the game folder where name ending with '.app'.");
                SelectGameFolder();
            }

            if (!string.IsNullOrEmpty(selectedGame.Comment))
            {
                Log.Print(selectedGame.Comment);
            }

            param.Sync(config.GameInfo);
            param.Save();

            Utils.TryParseEntryPoint(selectedGame.EntryPoint, out var assemblyName);

            gamePath = selectedGameParams.Path;
            if (File.Exists(Path.Combine(gamePath, "GameAssembly.dll")))
            {
                Log.Print("This game version (IL2CPP) is not supported.");
                return;
            }
            managedPath = Utils.FindManagedFolder(gamePath);
            if (managedPath == null)
            {
                Log.Print("Select the game folder that contains the 'Data' folder.");
                return;
            }
            Log.Print($"Managed folder detected as '{managedPath}'.");
            managerPath = Path.Combine(managedPath, nameof(UnityModManager));
            entryAssemblyPath = Path.Combine(managedPath, assemblyName);
            managerAssemblyPath = Path.Combine(managerPath, typeof(UnityModManager).Module.Name);
            entryPoint = selectedGame.EntryPoint;
            injectedEntryPoint = selectedGame.EntryPoint;

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

            if (File.Exists(Path.Combine(managedPath, "System.Xml.dll")))
            {
                libraryFiles["System.Xml.dll"] = LibIncParam.Skip;
            }

            var gameSupportVersion = !string.IsNullOrEmpty(selectedGame.MinimalManagerVersion) ? Utils.ParseVersion(selectedGame.MinimalManagerVersion) : VER_0_22;
            libraryPaths = new List<string>();
            foreach (var item in libraryFiles)
            {
                if ((item.Value & LibIncParam.Minimal_lt_0_22) > 0 && gameSupportVersion >= VER_0_22 || (item.Value & LibIncParam.Skip) > 0)
                {
                    continue;
                }
                libraryPaths.Add(Path.Combine(managerPath, item.Key));
            }

            var parent = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent;
            for (int i = 0; i < 3; i++)
            {
                if (parent == null)
                    break;

                if (parent.FullName == gamePath)
                {
                    Log.Print("UMM Installer should not be located in the game folder.");
                    Close();
                    return;
                }
                parent = parent.Parent;
            }

            Refresh:
            doorstopPath = Path.Combine(gamePath, doorstopFilename);
            doorstopConfigPath = Path.Combine(gamePath, doorstopConfigFilename);
            injectedEntryAssemblyPath = entryAssemblyPath;
            assemblyDef = null;
            injectedAssemblyDef = null;
            managerDef = null;

            try
            {
                assemblyDef = ModuleDefMD.Load(File.ReadAllBytes(entryAssemblyPath));
            }
            catch (Exception e)
            {
                Log.Print(e.ToString() + Environment.NewLine + entryAssemblyPath);
                Close();
                return;
            }

            var useOldPatchTarget = false;
            GameInfo.filepathInGame = Path.Combine(managerPath, "Config.xml");
            if (File.Exists(GameInfo.filepathInGame))
            {
                var gameConfig = GameInfo.ImportFromGame();
                if (gameConfig == null || !Utils.TryParseEntryPoint(gameConfig.EntryPoint, out assemblyName))
                {
                    Close();
                    return;
                }
                injectedEntryPoint = gameConfig.EntryPoint;
                injectedEntryAssemblyPath = Path.Combine(managedPath, assemblyName);
            }
            else if (!string.IsNullOrEmpty(selectedGame.OldPatchTarget))
            {
                if (!Utils.TryParseEntryPoint(selectedGame.OldPatchTarget, out assemblyName))
                {
                    Close();
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
                Log.Print(e.ToString() + Environment.NewLine + injectedEntryAssemblyPath + Environment.NewLine + managerAssemblyPath);
                Close();
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

            managerDef = managerDef ?? injectedAssemblyDef;

            Actions actions = 0;
            if (selectedGameParams.InstallType == InstallType.Assembly && Utils.IsDirty(injectedAssemblyDef) && File.Exists($"{injectedEntryAssemblyPath}.original_"))
            {
                actions |= Actions.Restore;
            }

            var managerInstalled = managerDef.Types.FirstOrDefault(x => x.Name == managerType.Name);
            if (managerInstalled != null && (hasInjectedAssembly || selectedGameParams.InstallType == InstallType.DoorstopProxy))
            {
                //btnInstall.Text = "Update";
                //btnInstall.Enabled = false;
                //btnRemove.Enabled = true;

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

                //installedVersion.Text = version2.ToString();
                if (version > version2 && v0_12_Installed == null)
                {
                    //btnInstall.Enabled = true;
                    actions |= Actions.Update;
                }

                Log.Print($"Manager-{version2} is installed on [{selectedGame}] as [{selectedGameParams.InstallType}].");
                actions |= Actions.Delete;
            }
            else
            {
                Log.Print($"Manager-{version} is not installed on [{selectedGame}].");
                //installedVersion.Text = "-";
                //btnInstall.Enabled = true;
                //btnRemove.Enabled = false;
                actions |= Actions.Install;
            }

            Log.Print("Enter key for command or press enter to exit.");
            for (int i = (int)Actions.Install; i <= (int)Actions.Restore; i = i << 1)
            {
                if (actions.HasFlag((Actions)i))
                {
                    Log.Print($"{((Actions)i).ToString().First().ToString().ToUpper()}. {(Actions)i}");
                }
            }

            ReadAgain:
            Log.Print("Key: ");
            var c = Console.ReadLine();
            if (string.IsNullOrEmpty(c))
            {
                Close();
                return;
            }
            c = c.ToLower();
            if (c == "i" && actions.HasFlag(Actions.Install))
            {
                Log.Print($"Installation method is selected as [{selectedGameParams.InstallType}]. Do you want to change it? Yes:<y> Continue:<any>");
                var k = Console.ReadKey(true);
                if (k.Key == ConsoleKey.Y)
                {
                    int i = 1;
                    for (InstallType t = InstallType.Assembly; t <= InstallType.DoorstopProxy; t++)
                    {
                        if (unavailableMethods.Contains(t) || disabledMethods.Contains(t))
                            continue;

                        Log.Print($"{i++}. {t}");
                    }

                    ReadAgain2:
                    Log.Print("Key: ");
                    c = Console.ReadLine();
                    if (string.IsNullOrEmpty(c))
                    {
                        Close();
                        return;
                    }

                    bool changed = false;
                    i = 1;
                    for (InstallType t = InstallType.Assembly; t <= InstallType.DoorstopProxy; t++)
                    {
                        if (unavailableMethods.Contains(t) || disabledMethods.Contains(t))
                            continue;

                        if (c == i.ToString())
                        {
                            selectedGameParams.InstallType = t;
                            changed = true;
                            param.Save();
                            break;
                        }
                        i++;
                    }

                    if (!changed)
                        goto ReadAgain2;

                }

                if (!TestWritePermissions())
                {
                    Close();
                    return;
                }

                if (!TestCompatibility())
                {
                    Close();
                    return;
                }

                var modsPath = Path.Combine(gamePath, selectedGame.ModsDirectory);
                if (!Directory.Exists(modsPath))
                {
                    Directory.CreateDirectory(modsPath);
                }

                if (selectedGameParams.InstallType == InstallType.DoorstopProxy)
                {
                    InstallDoorstop(Actions.Install);
                }
                else
                {
                    InjectAssembly(Actions.Install, assemblyDef);
                }

                goto Refresh;
            }
            else if (c == "u" && actions.HasFlag(Actions.Update))
            {
                Log.Print($"Installation method is selected as [{selectedGameParams.InstallType}].");

                if (!TestWritePermissions())
                {
                    Close();
                    return;
                }

                if (!TestCompatibility())
                {
                    Close();
                    return;
                }

                var modsPath = Path.Combine(gamePath, selectedGame.ModsDirectory);
                if (!Directory.Exists(modsPath))
                {
                    Directory.CreateDirectory(modsPath);
                }

                if (selectedGameParams.InstallType == InstallType.DoorstopProxy)
                {
                    InstallDoorstop(Actions.Install);
                }
                else
                {
                    InjectAssembly(Actions.Install, assemblyDef);
                }

                goto Refresh;
            }
            else if (c == "d" && actions.HasFlag(Actions.Delete))
            {
                Log.Print($"Installation method is selected as [{selectedGameParams.InstallType}].");

                if (!TestWritePermissions())
                {
                    Close();
                    return;
                }

                if (selectedGameParams.InstallType == InstallType.DoorstopProxy)
                {
                    InstallDoorstop(Actions.Delete);
                }
                else
                {
                    InjectAssembly(Actions.Delete, injectedAssemblyDef);
                }

                goto Refresh;
            }
            else if (c == "r" && actions.HasFlag(Actions.Restore))
            {
                if (selectedGameParams.InstallType == InstallType.Assembly)
                {
                    var injectedEntryAssemblyPath = Path.Combine(managedPath, injectedAssemblyDef.Name);
                    var originalAssemblyPath = $"{injectedEntryAssemblyPath}.original_";
                    RestoreOriginal(injectedEntryAssemblyPath, originalAssemblyPath);
                }

                goto Refresh;
            }
            else
            {
                goto ReadAgain;
            }

        }

        static void SelectGameFolder()
        {
            Log.Print("Enter the full path to the game folder, for example С:\\Program Files\\Steam\\steamapps\\common\\YourGame\\");
            ReadAgain:
            Log.Print("Path: ");
            var l = Console.ReadLine();
            if (string.IsNullOrEmpty(l))
            {
                Close();
                return;
            }
            l = l.Replace("\"", string.Empty);
            l = l.Replace("'", string.Empty);
            if (!Directory.Exists(l))
            {
                Log.Print($"Path '{l}' does not exist.");
                goto ReadAgain;
            }

            selectedGameParams.Path = l;
        }

        static bool TestWritePermissions()
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

        static bool TestCompatibility()
        {
            foreach (var f in new DirectoryInfo(gamePath).GetFiles("0Harmony.dll", SearchOption.AllDirectories))
            {
                if (!f.FullName.EndsWith(Path.Combine("UnityModManager", "0Harmony.dll")))
                {
                    //var asm = Assembly.ReflectionOnlyLoad(File.ReadAllBytes(f.FullName));
                    //if (asm.GetName().Version < HARMONY_VER)
                    var asm = ModuleDefMD.Load(File.ReadAllBytes(f.FullName));
                    if (asm.Assembly.Version < HARMONY_VER)
                    {
                        Log.Print($"Game has extra library 0Harmony.dll in path {f.FullName}, which may not be compatible with UMM. Recommended to delete it.");
                        return false;
                    }
                    Log.Print($"Game has extra library 0Harmony.dll in path {f.FullName}.");
                }
            }

            return true;
        }

        static bool InstallDoorstop(Actions action, bool write = true)
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

                        if (!InstallDoorstop(Actions.Delete, false))
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
                        var relativeManagerAssemblyPath = managerAssemblyPath.Substring(gamePath.Length).Trim(Path.DirectorySeparatorChar);
                        File.WriteAllText(doorstopConfigPath, "[General]" + Environment.NewLine + "enabled = true" + Environment.NewLine + "target_assembly = " + relativeManagerAssemblyPath);

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

                case Actions.Delete:
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
                            DoactionLibraries(Actions.Delete);
                            DoactionGameConfig(Actions.Delete);
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

        static bool InjectAssembly(Actions action, ModuleDefMD assemblyDef, bool write = true)
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

                            if (!Utils.IsDirty(assemblyDef))
                            {
                                File.Copy(assemblyPath, originalAssemblyPath, true);
                                Utils.MakeDirty(assemblyDef);
                            }

                            if (!InjectAssembly(Actions.Delete, injectedAssemblyDef, assemblyDef != injectedAssemblyDef))
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

                case Actions.Delete:
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

                                if (!Utils.IsDirty(assemblyDef))
                                {
                                    Utils.MakeDirty(assemblyDef);
                                }

                                if (write)
                                {
                                    assemblyDef.Write(assemblyPath);
                                    DoactionLibraries(Actions.Delete);
                                    DoactionGameConfig(Actions.Delete);
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

        static void DoactionLibraries(Actions action)
        {
            if (action == Actions.Install)
            {
                Log.Print($"Copying files to game...");
            }
            else
            {
                Log.Print($"Deleting files from game...");
            }

            foreach (var destpath in libraryPaths)
            {
                var filename = Path.GetFileName(destpath);
                if (action == Actions.Install)
                {
                    var sourcepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
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

            if (action == Actions.Delete)
            {
                foreach (var file in Directory.GetFiles(managerPath, "*.dll"))
                {
                    var filename = Path.GetFileName(file);
                    Log.Print($"  {filename}");
                    File.Delete(file);
                }
            }
        }

        static void DoactionGameConfig(Actions action)
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

        static bool RestoreOriginal(string file, string backup)
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
    }
}
