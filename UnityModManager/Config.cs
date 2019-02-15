using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace UnityModManagerNet
{
    public partial class UnityModManager
    {
        public sealed class Param
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
            public int ShowOnStart = 1;
            public float WindowWidth;
            public float WindowHeight;

            public List<Mod> ModParams = new List<Mod>();

            static readonly string filepath = Path.Combine(Path.GetDirectoryName(typeof(Param).Assembly.Location), "Params.xml");

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
                    Logger.Error($"Can't write file '{filepath}'.");
                    Debug.LogException(e);
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
                        Logger.Error($"Can't read file '{filepath}'.");
                        Debug.LogException(e);
                    }
                }
                return new Param();
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
            public string EntryPoint;
            public string StartingPoint;
            public string UIStartingPoint;
            public string GameExe;

            static readonly string filepath = Path.Combine(Path.GetDirectoryName(typeof(GameInfo).Assembly.Location), "Config.xml");

            public static GameInfo Load()
            {
                try
                {
                    using (var stream = File.OpenRead(filepath))
                    {
                        return new XmlSerializer(typeof(GameInfo)).Deserialize(stream) as GameInfo;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error($"Can't read file '{filepath}'.");
                    Debug.LogException(e);
                    return null;
                }
            }
        }
    }
}
