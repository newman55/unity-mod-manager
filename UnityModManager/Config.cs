using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UnityModManagerNet.Installer
{
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
        [XmlElement]
        public GameInfo[] GameInfo;

        public const string filename = "UnityModManagerConfig.xml";

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
            public string Path;
        }

        public string LastGameSelected;
        public List<GamePath> GamePaths = new List<GamePath>();

        public const string filename = "UnityModManagerParams.xml";

        public void SaveGamePath(string name, string path)
        {
            bool needSave = false;
            var item = GamePaths.FirstOrDefault(x => x.Name == name);
            if (item != null)
            {
                if (item.Path != path)
                {
                    item.Path = path;
                    needSave = true;
                }
            }
            else
            {
                GamePaths.Add(new GamePath { Name = name, Path = path });
                needSave = true;
            }
            if (needSave)
            {
                //Save();
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
            try
            {
                using (var writer = new StreamWriter(filename))
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
            if (File.Exists(filename))
            {
                try
                {
                    using (var stream = File.OpenRead(filename))
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
