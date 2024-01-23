using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using UnityEngine;
using dnlib.DotNet;

namespace UnityModManagerNet
{
    public partial class UnityModManager
    {
        private static readonly Version VER_0 = new Version();
        private static readonly Version VER_0_13 = new Version(0, 13);
        private static readonly Version VER_2018_2 = new Version(2018, 2);

        /// <summary>
        /// Contains version of UnityEngine
        /// </summary>
        public static Version unityVersion { get; private set; }

        /// <summary>
        /// Contains version of a game, if configured [0.15.0]
        /// </summary>
        public static Version gameVersion { get; private set; } = new Version();

        /// <summary>
        /// Contains version of UMM
        /// </summary>
        public static Version version { get; private set; } = typeof(UnityModManager).Assembly.GetName().Version;

        private static ModuleDefMD thisModuleDef = ModuleDefMD.Load(typeof(UnityModManager).Module);

        private static bool forbidDisableMods;
        
        /// <summary>
        /// List of all mods
        /// </summary>
        public static readonly List<ModEntry> modEntries = new List<ModEntry>();

        /// <summary>
        /// Path to Mods folder
        /// </summary>
        public static string modsPath { get; private set; }

        /// <summary>
        /// [0.28.0]
        /// </summary>
        internal static readonly List<TextureReplacer.Skin> allSkins = new List<TextureReplacer.Skin>();

        /// <summary>
        /// [0.26.0]
        /// </summary>
        public delegate void ToggleModsListen(ModEntry modEntry, bool result);

        /// <summary>
        /// [0.26.0]
        /// </summary>
        public static event ToggleModsListen toggleModsListen;

        /// <summary>
        /// Does the OnSessionStart support [0.27.0]
        /// </summary>
        public static bool IsSupportOnSessionStart => !string.IsNullOrEmpty(Config.SessionStartPoint);

        /// <summary>
        /// Does the OnSessionStop support [0.27.0]
        /// </summary>
        public static bool IsSupportOnSessionStop => !string.IsNullOrEmpty(Config.SessionStopPoint);

        [Obsolete("Please use modsPath!!!!This is compatible with mod of ver before 0.13")]
        public static string OldModsPath = "";

        internal static Param Params { get; set; } = new Param();
        internal static GameInfo Config { get; set; } = new GameInfo();

        internal static bool started;
        internal static bool initialized;

        public static void Main()
        {
            AppDomain.CurrentDomain.AssemblyLoad += OnLoad;
        }

        static void OnLoad(object sender, AssemblyLoadEventArgs args)
        {
            var name = args.LoadedAssembly.GetName().Name;
            //Console.WriteLine(name);
            if (name == "Assembly-CSharp" || name == "GH.Runtime" || name == "AtomGame" || name == "Game" /*Cloud Meadow*/)
            {
                AppDomain.CurrentDomain.AssemblyLoad -= OnLoad;
                Injector.Run(true);
            }
        }

        public static bool Initialize()
        {
            if (initialized)
                return true;

            initialized = true;

            Logger.Clear();

            Logger.Log($"Initialize.");
            Logger.Log($"Version: {version}.");
            try
            {
                Logger.Log($"OS: {Environment.OSVersion} {Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE")}.");
                Logger.Log($"Net Framework: {Environment.Version}.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            unityVersion = ParseVersion(Application.unityVersion);
            Logger.Log($"Unity Engine: {unityVersion}.");

            if (!Assembly.GetExecutingAssembly().Location.Contains($"Managed{Path.DirectorySeparatorChar}UnityModManager"))
            {
                Logger.Error($"Duplicate files found {Assembly.GetExecutingAssembly().Location}. The UnityModManager folder must be located only in \\Game\\*Data\\Managed\\ directory. This folder is created automatically after installation via UnityModManager.exe.");
            }

            Config = GameInfo.Load();
            if (Config == null)
            {
                return false;
            }

            Logger.Log($"Game: {Config.Name}.");
            Logger.NativeLog($"IsSupportOnSessionStart: {IsSupportOnSessionStart}.");
            Logger.NativeLog($"IsSupportOnSessionStop: {IsSupportOnSessionStop}.");

            Params = Param.Load();

            modsPath = Path.Combine(Environment.CurrentDirectory, Config.ModsDirectory);
            if (!Directory.Exists(modsPath))
            {
                var modsPath2 = Path.Combine(Path.GetDirectoryName(Environment.CurrentDirectory), Config.ModsDirectory);

                if (Directory.Exists(modsPath2))
                {
                    modsPath = modsPath2;
                }
                else
                {
                    Directory.CreateDirectory(modsPath);
                }
            }

            Logger.Log($"Mods path: {modsPath}.");
            OldModsPath = modsPath;

            KeyBinding.Initialize();

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            return true;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (assembly != null)
            {
                return assembly;
            }

            string filename = null;
            if (args.Name.StartsWith("0Harmony12"))
            {
                filename = "0Harmony12.dll";
            }
            else if (args.Name.StartsWith("0Harmony, Version=1.") || args.Name.StartsWith("0Harmony-1.2"))
            {
                filename = "0Harmony-1.2.dll";
            }
            else if (args.Name.StartsWith("0Harmony, Version=2."))
            {
                filename = "0Harmony.dll";
            }

            if (filename != null)
            {
                string filepath = Path.Combine(Path.GetDirectoryName(typeof(UnityModManager).Assembly.Location), filename);
                if (File.Exists(filepath))
                {
                    try
                    {
                        return Assembly.LoadFile(filepath);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.ToString());
                    }
                }
            }

            return null;
        }

        public static void Start()
        {
            if (started)
                return;

            try
            {
                _Start();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                OpenUnityFileLog();
            }
        }

        private static void _Start()
        {
            if (!Initialize())
            {
                Logger.Log($"Cancel start due to an error.");
                OpenUnityFileLog();
                return;
            }

            Logger.Log($"Starting.");

            started = true;

            ParseGameVersion();

            GameScripts.Init();

            GameScripts.OnBeforeLoadMods();

            if (Directory.Exists(modsPath))
            {
                Logger.Log($"Parsing mods.");

                Dictionary<string, ModEntry> mods = new Dictionary<string, ModEntry>();

                int countMods = 0;

                foreach (string dir in Directory.GetDirectories(modsPath))
                {
                    string jsonPath = Path.Combine(dir, Config.ModInfo);
                    if (!File.Exists(jsonPath))
                    {
                        jsonPath = Path.Combine(dir, Config.ModInfo.ToLower());
                    }
                    ModEntry modEntry;
                    if (File.Exists(jsonPath))
                    {
                        countMods++;
                        Logger.Log($"Reading file '{jsonPath}'.");
                        try
                        {
                            //ModInfo modInfo = JsonUtility.FromJson<ModInfo>(File.ReadAllText(jsonPath));
                            ModInfo modInfo = TinyJson.JSONParser.FromJson<ModInfo>(File.ReadAllText(jsonPath));
                            if (string.IsNullOrEmpty(modInfo.Id))
                            {
                                Logger.Error($"Id is null.");
                                continue;
                            }
                            if (mods.ContainsKey(modInfo.Id))
                            {
                                Logger.Error($"Id '{modInfo.Id}' already uses another mod.");
                                continue;
                            }
                            if (string.IsNullOrEmpty(modInfo.AssemblyName))
                            {
                                if (File.Exists(Path.Combine(dir, modInfo.Id + ".dll")))
                                {
                                    modInfo.AssemblyName = modInfo.Id + ".dll";
                                }
                            }

                            modEntry = new ModEntry(modInfo, dir + Path.DirectorySeparatorChar);
                            mods.Add(modInfo.Id, modEntry);
                        }
                        catch (Exception exception)
                        {
                            Logger.Error($"Error parsing file '{jsonPath}'.");
                            Debug.LogException(exception);
                            continue;
                        }

                        var trFolder = Path.Combine(dir, "TextureReplacer");
                        if (Directory.Exists(trFolder))
                        {
                            foreach (string skinDir in Directory.GetDirectories(trFolder))
                            {
                                try
                                {
                                    string trJsonPath = Path.Combine(skinDir, "skin.json");
                                    TextureReplacer.Skin skin;
                                    if (File.Exists(trJsonPath))
                                    {
                                        skin = TinyJson.JSONParser.FromJson<TextureReplacer.Skin>(File.ReadAllText(trJsonPath));
                                    }
                                    else
                                    {
                                        skin = new TextureReplacer.Skin() { Name = new DirectoryInfo(skinDir).Name };
                                    }
                                    skin.modEntry = modEntry;
                                    skin.textures = new Dictionary<string, TextureReplacer.Skin.texture>();
                                    modEntry.Skins.Add(skin);

                                    foreach (string file in Directory.GetFiles(skinDir))
                                    {
                                        if (file.EndsWith("skin.json"))
                                        {
                                        }
                                        else if (file.EndsWith(".png") || file.EndsWith(".jpg"))
                                        {
                                            skin.textures[Path.GetFileNameWithoutExtension(file)] = new TextureReplacer.Skin.texture { Path = file };
                                        }
                                        else
                                        {
                                            Logger.Log($"Unsupported file format for '{file}'.");
                                        }
                                    }
                                }
                                catch (Exception exception)
                                {
                                    Logger.Error($"Error");
                                    Debug.LogException(exception);
                                }
                            }
                        }
                    }
                    else
                    {
                        //Logger.Log($"File not found '{jsonPath}'.");
                    }
                }

                if (mods.Count > 0)
                {
                    Logger.Log($"Sorting mods.");
                    TopoSort(mods);

                    Params.ReadModParams();

                    Logger.Log($"Loading mods.");
                    foreach (var mod in modEntries)
                    {
                        if (!mod.Enabled)
                        {
                            mod.Logger.Log("To skip (disabled).");
                        }
                        else
                        {
                            mod.Active = true;
                        }
                    }
                }

                //ApplySkins();

                Logger.Log($"Finish. Successful loaded {modEntries.Count(x => !x.ErrorOnLoading)}/{countMods} mods.".ToUpper());
                Console.WriteLine();

                var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.ManifestModule.Name == "UnityModManager.dll");
                if (assemblies.Count() > 1)
                {
                    Logger.Error($"Detected extra copies of UMM.");
                    foreach (var ass in assemblies)
                    {
                        Logger.Log($"- {ass.CodeBase}");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
            else
            {
                Logger.Log($"Directory '{modsPath}' not exists.");
            }

            GameScripts.OnAfterLoadMods();

            //if (!UI.Load())
            //{
            //    Logger.Error($"Can't load UI.");
            //}
        }

        static MethodInfo GetTexturePropertyNames = typeof(Material).GetMethod("GetTexturePropertyNames", new Type[] { typeof(List<string>) });
        static List<string> texturePropertyNames = new List<string>();

        internal static void ApplySkins()
        {
            if (unityVersion < VER_2018_2)
                return;

            Logger.Log($"Replacing textures.");

            var materials = Resources.FindObjectsOfTypeAll<Material>();

            foreach (var skin in allSkins)
            {
                if (skin.Conditions.IsEmpty)
                {
                    foreach(var mat in materials)
                    {
                        texturePropertyNames.Clear();
                        GetTexturePropertyNames.Invoke(mat, new object[] { texturePropertyNames });

                        foreach(var p in texturePropertyNames)
                        {
                            var tex = mat.GetTexture(p);
                            if (tex && !string.IsNullOrEmpty(tex.name) && tex is Texture2D tex2d)
                            {
                                foreach(var kv in skin.textures)
                                {
                                    if (tex.name == kv.Key)
                                    {
                                        mat.SetTexture(p, kv.Value.Texture);
                                        Logger.Log($"Replaced texture '{tex.name}' in material '{mat.name}'.");
                                        if (!kv.Value.Previous)
                                            kv.Value.Previous = tex2d;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void ParseGameVersion()
        {
            if (string.IsNullOrEmpty(Config.GameVersionPoint)) return;

            try
            {
                Logger.Log("Start parsing game version.");

                var version = TryGetValueFromDllPoint(Config.GameVersionPoint)?.ToString();
                if (version == null) return;

                Logger.Log($"Found game version string: '{version}'.");

                gameVersion = ParseVersion(version);
                Logger.Log($"Game version detected as '{gameVersion}'.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                OpenUnityFileLog();
            }
        }

        private static object TryGetValueFromDllPoint(string point)
        {
            var regex = new Regex(@"^\[(.+\.dll)\](\w+)((?:\.\w+(?:\(\))?)+)$", RegexOptions.IgnoreCase);
            var match = regex.Match(point);

            if (!match.Success)
            {
                Logger.Error($"Malformed DLL point: '{point}'");
                return null;
            }

            var dll = match.Groups[1].Value;
            var path = match.Groups[2].Value;
            var subpaths = match.Groups[3].Value.Trim('.').Split('.');

            var asm = Assembly.Load(dll);
            if (asm == null)
            {
                Logger.Error($"File '{dll}' not found.");
                return null;
            }

            Type cls = asm.GetType(path);
            var i = 0;

            for (; i < subpaths.Length; i++)
            {
                var pathElement = subpaths[i];

                if (pathElement.EndsWith("()")) break;

                path += "." + pathElement;
                var newCls = asm.GetType(path);
                if (newCls != null) cls = newCls;
                else if (cls != null) break;
            }

            if (cls == null)
            {
                Logger.Error($"No class found at '{path}'");
                return null;
            }
            else if (i == subpaths.Length)
            {
                Logger.Error($"Could not provide a value because '{path}' is a type");
                return null;
            }

            object instance = null;

            for (var first = i; i < subpaths.Length; i++)
            {
                var pathElement = subpaths[i];

                if (pathElement.EndsWith("()"))
                {
                    pathElement = pathElement.Substring(0, pathElement.Length - 2);
                }

                if (!GetValueFromMember(cls, ref instance, pathElement, i == first)) return null;

                if (instance == null)
                {
                    Logger.Error($"'{cls.FullName}.{pathElement}' returned null");
                    return null;
                }

                cls = instance.GetType();
            }

            return instance;
        }

        private static bool GetValueFromMember(Type cls, ref object instance, string name, bool _static)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | (_static ? BindingFlags.Static : BindingFlags.FlattenHierarchy | BindingFlags.Instance);

            var field = cls.GetField(name, flags);
            if (field != null)
            {
                instance = field.GetValue(instance);
                return true;
            }

            var property = cls.GetProperty(name, flags);
            if (property != null)
            {
                instance = property.GetValue(instance, null);
                return true;
            }

            var method = cls.GetMethod(name, flags, null, Type.EmptyTypes, null);
            if (method != null)
            {
                instance = method.Invoke(instance, null);
                return true;
            }

            Logger.Error($"Class '{cls.FullName}' does not have a {(_static ? "static" : "non-static")} member '{name}'");
            return false;
        }

        private static void DFS(string id, Dictionary<string, ModEntry> mods)
        {
            if (modEntries.Any(m => m.Info.Id == id))
            {
                return;
            }
            foreach (var req in mods[id].Requirements.Keys)
            {
                if (mods.ContainsKey(req))
                    DFS(req, mods);
            }
            foreach (var req in mods[id].LoadAfter)
            {
                if (mods.ContainsKey(req))
                    DFS(req, mods);
            }
            modEntries.Add(mods[id]);
        }

        private static void TopoSort(Dictionary<string, ModEntry> mods)
        {
            foreach (var id in mods.Keys)
            {
                DFS(id, mods);
            }
        }

        public static ModEntry FindMod(string id)
        {
            return modEntries.FirstOrDefault(x => x.Info.Id == id);
        }

        public static Version GetVersion()
        {
            return version;
        }

        public static void SaveSettingsAndParams()
        {
            Params.Save();
            foreach (var mod in modEntries)
            {
                if (mod.Active && mod.OnSaveGUI != null)
                {
                    try
                    {
                        mod.OnSaveGUI(mod);
                    }
                    catch (Exception e)
                    {
                        mod.Logger.LogException("OnSaveGUI", e);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Copies a value from an old assembly to a new one [0.14.0]
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SaveOnReloadAttribute : Attribute
    {
    }

    /// <summary>
    /// Allows reloading [0.14.1]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EnableReloadingAttribute : Attribute
    {
    }
}
