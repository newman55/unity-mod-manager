using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using UnityEngine;

namespace UnityModManagerNet
{
    public partial class UnityModManager
    {
        public const string version = "0.12.4";
        public const string modsDirname = "Mods";
        public const string infoFilename = "info.json";
        public const string patchTarget = "";

        public static Version unityVersion;

        private static Version mVersion = new Version();

        public class Repository 
        {
            [Serializable]
            public class Release : IEquatable<Release>
            {
                public string Id;
                public string Version;
                public string DownloadUrl;

                public bool Equals(Release other)
                {
                    return Id.Equals(other.Id);
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj))
                    {
                        return false;
                    }
                    return obj is Release obj2 && Equals(obj2);
                }

                public override int GetHashCode()
                {
                    return Id.GetHashCode();
                }
            }

            public Release[] Releases;
        }

        public class ModSettings
        {
            public virtual void Save(ModEntry modEntry)
            {
                Save(this, modEntry);
            }

            public virtual string GetPath(ModEntry modEntry)
            {
                return Path.Combine(modEntry.Path, "Settings.xml");
            }

            public static void Save<T>(T data, ModEntry modEntry) where T : ModSettings, new()
            {
                var filepath = data.GetPath(modEntry);
                try
                {
                    using (var writer = new StreamWriter(filepath))
                    {
                        var serializer = new XmlSerializer(typeof(T));
                        serializer.Serialize(writer, data);
                    }
                }
                catch (Exception e)
                {
                    modEntry.Logger.Error($"Can't save {filepath}.");
                    modEntry.Logger.Error(e.ToString());
                }
            }

            public static T Load<T>(ModEntry modEntry) where T : ModSettings, new()
            {
                var t = new T();
                var filepath = t.GetPath(modEntry);
                if (File.Exists(filepath))
                {
                    try
                    {
                        using (var stream = File.OpenRead(filepath))
                        {
                            var serializer = new XmlSerializer(typeof(T));
                            var result = (T)serializer.Deserialize(stream);
                            return result;
                        }
                    }
                    catch (Exception e)
                    {
                        modEntry.Logger.Error($"Can't read {filepath}.");
                        modEntry.Logger.Error(e.ToString());
                    }
                }
                
                return t;
            }
        }

        public class ModInfo : IEquatable<ModInfo>
        {
            public string Id;

            public string DisplayName;

            public string Author;

            public string Version;

            public string ManagerVersion;

            public string[] Requirements;

            public string AssemblyName;

            public string EntryMethod;

            public string HomePage;

            public string Repository;

            public static implicit operator bool(ModInfo exists)
            {
                return exists != null;
            }

            public bool Equals(ModInfo other)
            {
                return Id.Equals(other.Id);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }
                return obj is ModInfo modInfo && Equals(modInfo);
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }

        public class ModEntry
        {
            public readonly ModInfo Info;

            public readonly string Path;

            Assembly mAssembly = null;
            public Assembly Assembly => mAssembly;

            public readonly Version Version = null;

            public readonly Version ManagerVersion = null;

            public Version NewestVersion;

            public readonly Dictionary<string, Version> Requirements = new Dictionary<string, Version>();

            public readonly ModLogger Logger = null;

            public bool HasUpdate = false;

            //public ModSettings Settings = null;

            public Func<ModEntry, bool, bool> OnToggle = null;

            public Action<ModEntry> OnGUI = null;

            public Action<ModEntry> OnSaveGUI = null;

            Dictionary<long, MethodInfo> mCache = new Dictionary<long, MethodInfo>();

            bool mStarted = false;
            public bool Started => mStarted;

            bool mErrorOnLoading = false;
            public bool ErrorOnLoading => mErrorOnLoading;

            public bool Enabled = true;
            //public bool Enabled => Enabled;

            public bool Toggleable => OnToggle != null;

            bool mActive = false;
            public bool Active
            {
                get => mActive;
                set
                {
                    if (!mStarted || mErrorOnLoading)
                        return;

                    try
                    {
                        if (value)
                        {
                            if (mActive)
                                return;

                            if (OnToggle == null || OnToggle(this, true))
                            {
                                mActive = true;
                                this.Logger.Log($"Active.");
                            }
                        }
                        else
                        {
                            if (!mActive)
                                return;

                            if (OnToggle != null && OnToggle(this, false))
                            {
                                mActive = false;
                                this.Logger.Log($"Inactive.");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        this.Logger.Error($"Error trying to call 'OnToggle' function.");
                        this.Logger.Error(e.ToString());
                    }
                }
            }

            public ModEntry(ModInfo info, string path)
            {
                Info = info;
                Path = path;
                Logger = new ModLogger(Info.Id);
                Version = ParseVersion(info.Version);
                ManagerVersion = !string.IsNullOrEmpty(info.ManagerVersion) ? ParseVersion(info.ManagerVersion) : new Version();

                if (info.Requirements != null && info.Requirements.Length > 0)
                {
                    var regex = new Regex(@"(.*)-(\d\.\d\.\d).*");
                    foreach (var id in info.Requirements)
                    {
                        var match = regex.Match(id);
                        if (match.Success)
                        {
                            Requirements.Add(match.Groups[1].Value, ParseVersion(match.Groups[2].Value));
                            continue;
                        }
                        if (!Requirements.ContainsKey(id))
                            Requirements.Add(id, null);
                    }
                }
            }

            public bool Load()
            {
                if (mStarted)
                {
                    if (mErrorOnLoading)
                        return false;

                    return true;
                }

                mErrorOnLoading = false;

                this.Logger.Log($"Version '{Info.Version}'. Loading.");
                if (string.IsNullOrEmpty(Info.AssemblyName))
                {
                    mErrorOnLoading = true;
                    this.Logger.Error($"{nameof(Info.AssemblyName)} is null.");
                }

                if (string.IsNullOrEmpty(Info.EntryMethod))
                {
                    mErrorOnLoading = true;
                    this.Logger.Error($"{nameof(Info.EntryMethod)} is null.");
                }

                if (!string.IsNullOrEmpty(Info.ManagerVersion))
                {
                    if (ManagerVersion > GetVersion())
                    {
                        mErrorOnLoading = true;
                        this.Logger.Error($"Mod Manager must be version '{Info.ManagerVersion}' or higher.");
                    }
                }

                if (Requirements.Count > 0)
                {
                    foreach (var item in Requirements)
                    {
                        var id = item.Key;
                        var mod = FindMod(id);
                        if (mod == null || mod.Assembly == null)
                        {
                            mErrorOnLoading = true;
                            this.Logger.Error($"Required mod '{id}' not loaded.");
                        }
                        //else if (!mod.Enabled)
                        //{
                        //    mErrorOnLoading = true;
                        //    this.Logger.Error($"Required mod '{id}' disabled.");
                        //}
                        else if (!mod.Active)
                        {
                            this.Logger.Log($"Required mod '{id}' inactive.");
                        }
                        else if (item.Value != null)
                        {
                            if (item.Value > mod.Version)
                            {
                                mErrorOnLoading = true;
                                this.Logger.Error($"Required mod '{id}' must be version '{item.Value}' or higher.");
                            }
                        }
                    }
                }

                if (mErrorOnLoading)
                    return false;

                string assemblyPath = System.IO.Path.Combine(Path, Info.AssemblyName);
                if (File.Exists(assemblyPath))
                {
                    try
                    {
                        if (mAssembly == null)
                            mAssembly = Assembly.LoadFile(assemblyPath);
                    }
                    catch (Exception exception)
                    {
                        mErrorOnLoading = true;
                        this.Logger.Error($"Error loading file '{assemblyPath}'.");
                        this.Logger.Error(exception.ToString());
                        return false;
                    }

                    object[] param = new object[] { this };
                    Type[] types = new Type[] { typeof(ModEntry) };
                    if (FindMethod(Info.EntryMethod, types, false) == null)
                    {
                        param = null;
                        types = null;
                    }

                    if (!Invoke(Info.EntryMethod, out var result, param, types) || result != null && (bool)result == false)
                    {
                        mErrorOnLoading = true;
                        this.Logger.Log($"Not loaded.");
                    }

                    mStarted = true;

                    if (!mErrorOnLoading && Enabled)
                    {
                        Active = true;
                        return true;
                    }
                }
                else
                {
                    mErrorOnLoading = true;
                    this.Logger.Error($"'{assemblyPath}' not found.");
                }

                return false;
            }

            public bool Invoke(string namespaceClassnameMethodname, out object result, object[] param = null, Type[] types = null)
            {
                result = null;
                try
                {
                    var methodInfo = FindMethod(namespaceClassnameMethodname, types);
                    if (methodInfo != null)
                    {
                        result = methodInfo.Invoke(null, param);
                        return true;
                    }
                }
                catch (Exception exception)
                {
                    this.Logger.Error($"Error trying to call '{namespaceClassnameMethodname}'.");
                    this.Logger.Error($"{exception.GetType().Name} - {exception.Message}");
                    Debug.LogException(exception);
                }

                return false;
            }

            MethodInfo FindMethod(string namespaceClassnameMethodname, Type[] types, bool showLog = true)
            {
                long key = namespaceClassnameMethodname.GetHashCode();
                if (types != null)
                {
                    foreach (var val in types)
                    {
                        key += val.GetHashCode();
                    }
                }

                if (!mCache.TryGetValue(key, out var methodInfo))
                {
                    if (mAssembly != null)
                    {
                        string classString = null;
                        string methodString = null;
                        var pos = namespaceClassnameMethodname.LastIndexOf('.');
                        if (pos != -1)
                        {
                            classString = namespaceClassnameMethodname.Substring(0, pos);
                            methodString = namespaceClassnameMethodname.Substring(pos + 1);
                        }
                        else
                        {
                            if (showLog)
                                this.Logger.Error($"Function name error '{namespaceClassnameMethodname}'.");

                            goto Exit;
                        }
                        var type = mAssembly.GetType(classString);
                        if (type != null)
                        {
                            if (types == null)
                                types = new Type[0];

                            methodInfo = type.GetMethod(methodString, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, types, new ParameterModifier[0]);
                            if (methodInfo == null)
                            {
                                if (showLog)
                                {
                                    if (types.Length > 0)
                                    {
                                        this.Logger.Log($"Method '{namespaceClassnameMethodname}[{string.Join(", ", types.Select(x => x.Name).ToArray())}]' not found.");
                                    }
                                    else
                                    {
                                        this.Logger.Log($"Method '{namespaceClassnameMethodname}' not found.");
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (showLog)
                                this.Logger.Error($"Class '{classString}' not found.");
                        }
                    }
                    else
                    {
                        if (showLog)
                            UnityModManager.Logger.Error($"Can't find method '{namespaceClassnameMethodname}'. Mod '{Info.Id}' is not loaded.");
                    }

                    Exit:

                    mCache[key] = methodInfo;
                }

                return methodInfo;
            }

            public class ModLogger
            {
                protected readonly string Prefix;
                protected readonly string PrefixError;
                protected readonly string PrefixCritical;
                protected readonly string PrefixWarning;

                public ModLogger(string Id)
                {
                    Prefix = $"[{Id}] ";
                    PrefixError = $"[{Id}] [Error] ";
                    PrefixCritical = $"[{Id}] [Critical] ";
                    PrefixWarning = $"[{Id}] [Warning] ";
                }

                public void Log(string str)
                {
                    UnityModManager.Logger.Log(str, Prefix);
                }

                public void Error(string str)
                {
                    UnityModManager.Logger.Log(str, PrefixError);
                }

                public void Critical(string str)
                {
                    UnityModManager.Logger.Log(str, PrefixCritical);
                }

                public void Warning(string str)
                {
                    UnityModManager.Logger.Log(str, PrefixWarning);
                }
            }
        }

        public static readonly List<ModEntry> modEntries = new List<ModEntry>();
        public static readonly string modsPath = Path.Combine(Environment.CurrentDirectory, modsDirname);

        private static Param mParams = new Param();
        public static Param Params => mParams;

        public static bool isStarted = false;

        public static void Start()
        {
            if (isStarted)
            {
                Logger.Log($"Cancel start. Already started.");
                return;
            }

            mVersion = ParseVersion(version);
            unityVersion = ParseVersion(Application.unityVersion);

            Logger.Clear();

            Console.WriteLine();
            Console.WriteLine();
            Logger.Log($"Version '{version}'. Initialize.");

            if (Directory.Exists(modsPath))
            {
                Logger.Log($"Parsing mods.");

                int countMods = 0;

                foreach (string dir in Directory.GetDirectories(modsPath))
                {
                    string jsonPath = Path.Combine(dir, infoFilename);
                    if (File.Exists(jsonPath))
                    {
                        countMods++;
                        Logger.Log($"Reading file '{jsonPath}'.");
                        try
                        {
                            ModInfo modInfo = JsonUtility.FromJson<ModInfo>(File.ReadAllText(jsonPath));
                            if (string.IsNullOrEmpty(modInfo.Id))
                            {
                                Logger.Error($"Id is null.");
                                continue;
                            }
                            if (modEntries.Exists(x => x.Info.Id == modInfo.Id))
                            {
                                Logger.Error($"Id '{modInfo.Id}' already uses another mod.");
                                continue;
                            }
                            if (string.IsNullOrEmpty(modInfo.AssemblyName))
                                modInfo.AssemblyName = modInfo.Id + ".dll";

                            ModEntry modEntry = new ModEntry(modInfo, dir + Path.DirectorySeparatorChar);
                            modEntries.Add(modEntry);
                        }
                        catch (Exception exception)
                        {
                            Logger.Error($"Error parsing file '{jsonPath}'.");
                            Logger.Log(exception.Message);
                        }
                    }
                    else
                    {
                        //Logger.Log($"File not found '{jsonPath}'.");
                    }
                }

                if (modEntries.Count > 0)
                {
                    Logger.Log($"Sorting mods.");
                    modEntries.Sort(Compare);

                    mParams = Param.Load();

                    Logger.Log($"Loading mods.");
                    foreach (var mod in modEntries)
                    {
                        mod.Load();
                    }
                }

                Logger.Log($"Finish. Found {countMods} mods. Successful loaded {modEntries.Count(x => x.Active)} mods.".ToUpper());
                Console.WriteLine();
                Console.WriteLine();
            }
            else
            {
                Directory.CreateDirectory(modsPath);
            }

            if (!UI.Load())
            {
                Logger.Error($"Can't load UI.");
            }

            isStarted = true;
        }

        private static int Compare(ModEntry x, ModEntry y)
        {
            if (x.Requirements.Count > 0 && x.Requirements.ContainsKey(y.Info.Id))
            {
                return 1;
            }

            if (y.Requirements.Count > 0 && y.Requirements.ContainsKey(x.Info.Id))
            {
                return -1;
            }

            return String.Compare(x.Info.Id, y.Info.Id, StringComparison.Ordinal);
        }

        public static ModEntry FindMod(string id)
        {
            return modEntries.FirstOrDefault(x => x.Info.Id == id);
        }

        public static Version ParseVersion(string str)
        {
            var array = str.Split('.', ',');
            if (array.Length >= 3)
            {
                var regex = new Regex(@"\D");
                return new Version(int.Parse(regex.Replace(array[0], "")), int.Parse(regex.Replace(array[1], "")), int.Parse(regex.Replace(array[2], "")));
            }

            Logger.Error($"Error parsing version {str}");
            return new Version();
        }

        public static Version GetVersion()
        {
            return mVersion;
        }

        public static void SaveSettingsAndParams()
        {
            mParams.Save();
            foreach(var mod in modEntries)
            {
                if (mod.OnSaveGUI != null)
                    mod.OnSaveGUI(mod);
            }
        }

        public class Param
        {
            [Serializable]
            public class Mod
            {
                [XmlAttribute]
                public string Id;
                [XmlAttribute]
                public bool Enabled = true;
            }

            public int ShortcutKeyId = 0;
            public int CheckUpdates = 1;

            public List<Mod> ModParams = new List<Mod>();

            public static readonly string filepath = Path.Combine(modsPath, "UnityModManager.xml");

            public void Save()
            {
                try
                {
                    ModParams.Clear();
                    foreach (var mod in modEntries)
                    {
                        ModParams.Add(new Mod { Id = mod.Info.Id, Enabled = mod.Enabled });
                    }
                    using (var writer = new StreamWriter(filepath))
                    {
                        var serializer = new XmlSerializer(typeof(Param));
                        serializer.Serialize(writer, this);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e.ToString());
                }
            }

            public static Param Load()
            {
                if (File.Exists(filepath))
                {
                    try
                    {
                        using (var stream = File.OpenRead(filepath))
                        {
                            var serializer = new XmlSerializer(typeof(Param));
                            var result = serializer.Deserialize(stream) as Param;
                            foreach (var item in result.ModParams)
                            {
                                var mod = FindMod(item.Id);
                                if (mod != null)
                                {
                                    mod.Enabled = item.Enabled;
                                }
                            }
                            return result;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.ToString());
                    }
                }
                return new Param();
            }
        }

        public static class Logger
        {
            const string Prefix = "[Manager] ";
            const string PrefixError = "[Manager] [Error] ";
            public static readonly string filepath = Path.Combine(modsPath, "UnityModManager.log");

            public static void Log(string str)
            {
                Log(str, Prefix);
            }

            public static void Log(string str, string prefix)
            {
                Write(prefix + str);
            }

            public static void Error(string str)
            {
                Error(str, PrefixError);
            }

            public static void Error(string str, string prefix)
            {
                Write(prefix + str);
            }

            public static void Write(string str)
            {
                Console.WriteLine(str);

                try
                {
                    using (StreamWriter writer = File.AppendText(filepath))
                    {
                        writer.WriteLine(str);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            public static void Clear()
            {
                if (File.Exists(filepath))
                {
                    try
                    {
                        File.Delete(filepath);
                        using (File.Create(filepath))
                        {
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }
    }
}
