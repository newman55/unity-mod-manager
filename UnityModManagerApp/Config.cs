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
    public enum InstallType { Assembly, DoorstopProxy, /*Config,*/ Count }

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

    [XmlRoot("Config")]
    public class GameInfo
    {
        [XmlAttribute]
        public string Name;
        public string Folder;
        public string ModsDirectory;
        public string ModInfo;
        public string GameExe;
        public string EntryPoint;
        public string StartingPoint;
        public string UIStartingPoint;
        public string GameVersionPoint;
        public string OldPatchTarget;
        //public string MachineConfig;

        public override string ToString()
        {
            return Name;
        }

        public static string filepathInGame;

        public void ExportToGame()
        {
            try
            {
                using (var writer = new StreamWriter(filepathInGame))
                {
                    new XmlSerializer(typeof(GameInfo)).Serialize(writer, this);
                }
            }
            catch (Exception e)
            {
                Log.Print($"Error file creating '{filepathInGame}'.");
                throw e;
            }
        }

        public static GameInfo ImportFromGame()
        {
            try
            {
                using (var stream = File.OpenRead(filepathInGame))
                {
                    return new XmlSerializer(typeof(GameInfo)).Deserialize(stream) as GameInfo;
                }
            }
            catch (Exception e)
            {
                Log.Print(e.ToString());
                Log.Print($"Can't read file '{filepathInGame}'.");
                return null;
            }
        }
    }

    public sealed class Config
    {
        public const string filename = "UnityModManagerConfig.xml";

        public string Repository;
        public string HomePage;

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
                                EntryPoint = "[Assembly-CSharp.dll]App.Awake",
                                GameExe = "Game.exe"
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
                Log.Print(e.ToString());
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
                        var result = serializer.Deserialize(stream) as Config;
                        OnDeserialize(result);
                        return result;
                    }
                }
                catch (Exception e)
                {
                    Log.Print(e.ToString());
                }
            }
            return null;
        }

        private static void OnDeserialize(Config value)
        {

        }
    }

    public sealed class Param
    {
        [Serializable]
        public class GameParam
        {
            [XmlAttribute]
            public string Name;
            public string Path;
            public InstallType InstallType = InstallType.DoorstopProxy;
        }

        public string LastSelectedGame;
        public List<GameParam> GameParams = new List<GameParam>();

        public const string filename = "Params.xml";

        static GameParam CreateGameParam(GameInfo gameInfo)
        {
            return new GameParam { Name = gameInfo.Name };
        }

        public GameParam GetGameParam(GameInfo gameInfo)
        {
            var result = GameParams.FirstOrDefault(x => x.Name == gameInfo.Name);
            if (result == null)
            {
                result = CreateGameParam(gameInfo);
                GameParams.Add(result);
            }
            return result;
        }
    
        public void Sync(GameInfo[] gameInfos)
        {
            int i = 0;
            while(i < GameParams.Count)
            {
                if (gameInfos.Any(x => x.Name == GameParams[i].Name))
                {
                    i++;
                }
                else
                {
                    GameParams.RemoveAt(i);
                }
            }
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
                Log.Print(e.ToString());
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
                    Log.Print(e.ToString());
                }
            }
            return new Param();
        }
    }

    
}
