using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace UnityModManagerNet.Installer
{
    public enum ModStatus { NotInstalled, Installed }

    public class ModInfo : UnityModManager.ModInfo
    {
        [JsonIgnore]
        public ModStatus Status;

        [JsonIgnore]
        public Dictionary<Version, string> AvailableVersions = new Dictionary<Version, string>();

        [JsonIgnore]
        public Version ParsedVersion;

        [JsonIgnore]
        public string Path;

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(Id))
            {
                return false;
            }
            if (string.IsNullOrEmpty(DisplayName))
            {
                DisplayName = Id;
            }
            if (ParsedVersion == null)
            {
                ParsedVersion = Utils.ParseVersion(Version);
            }

            return true;
        }

        public bool EqualsVersion(ModInfo other)
        {
            return other != null && Id.Equals(other.Id) && Version.Equals(other.Version);
        }
    }

    [Serializable]
    public class GameInfo
    {
        [XmlAttribute]
        public string Name;
        public string Folder;
        public string ModsDirectory;
        public string ModInfo;
        public string AssemblyName;
        public string PatchTarget;

        public override string ToString()
        {
            return Name;
        }
    }

    public class Config
    {
        public const string filename = "UnityModManagerConfig.xml";

        public string Repository;

        [XmlElement]
        public GameInfo[] GameInfo;

        public static void Create()
        {
            try
            {
                using (var writer = new StreamWriter(filename))
                {
                    var config = new Config()
                    {
                        GameInfo = new GameInfo[]
                        {
                            new GameInfo
                            {
                                Name = "Game",
                                Folder = "Folder",
                                ModsDirectory = "Mods",
                                ModInfo = "Info.json",
                                AssemblyName = "Assembly-CSharp.dll",
                                PatchTarget = "App.Awake"
                            }
                        }
                    };
                    var serializer = new XmlSerializer(typeof(Config));
                    serializer.Serialize(writer, config);
                }

                Log.Print($"'{filename}' auto created.");
            }
            catch (Exception e)
            {
                Log.Print(e.Message);
            }
        }

        public static Config Load()
        {
            if (File.Exists(filename))
            {
                try
                {
                    using (var stream = File.OpenRead(filename))
                    {
                        var serializer = new XmlSerializer(typeof(Config));
                        return serializer.Deserialize(stream) as Config;
                    }
                }
                catch (Exception e)
                {
                    Log.Print(e.Message);
                }
            }
            return null;
        }
    }

    public class Param
    {
        [Serializable]
        public class GamePath
        {
            [XmlAttribute]
            public string Name;
            [XmlAttribute]
            public string Path;
        }

        public string LastSelectedGame;
        public List<GamePath> GamePaths = new List<GamePath>();

        public const string filename = "Params.xml";

        public void SaveGamePath(string name, string path)
        {
            var item = GamePaths.FirstOrDefault(x => x.Name == name);
            if (item != null)
            {
                item.Path = path;
            }
            else
            {
                GamePaths.Add(new GamePath { Name = name, Path = path });
            }
        }

        public bool ExtractGamePath(string name, out string result)
        {
            if (GamePaths != null && GamePaths.Count > 0)
            {
                var item = GamePaths.FirstOrDefault(x => x.Name == name);
                if (item != null && !string.IsNullOrEmpty(item.Path))
                {
                    result = item.Path;
                    return true;
                }
            }

            result = null;
            return false;
        }

        public void Save()
        {
            var path = Path.Combine(Application.LocalUserAppDataPath, filename);
            try
            {
                using (var writer = new StreamWriter(path))
                {
                    var serializer = new XmlSerializer(typeof(Param));
                    serializer.Serialize(writer, this);
                }
            }
            catch (Exception e)
            {
                Log.Print(e.Message);
            }
        }

        public static Param Load()
        {
            var path = Path.Combine(Application.LocalUserAppDataPath, filename);
            if (File.Exists(path))
            {
                try
                {
                    using (var stream = File.OpenRead(path))
                    {
                        var serializer = new XmlSerializer(typeof(Param));
                        return serializer.Deserialize(stream) as Param;
                    }
                }
                catch (Exception e)
                {
                    Log.Print(e.Message);
                }
            }
            return new Param();
        }
    }

    
}
