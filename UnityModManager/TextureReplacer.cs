using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using TinyJson;
using UnityEngine;
using static UnityModManagerNet.UnityModManager.TextureReplacer;

namespace UnityModManagerNet
{
    public partial class UnityModManager 
    {
        internal class TextureReplacer
        {
            public class Skin
            {
                public struct conditions
                {
                    public string MaterialName;
                    public string ObjectName;
                    public string ObjectComponent;
                    public string Custom;
                    [IgnoreJson] public bool IsEmpty => string.IsNullOrEmpty(MaterialName) && string.IsNullOrEmpty(ObjectName) && string.IsNullOrEmpty(ObjectComponent) && string.IsNullOrEmpty(Custom);
                }
                public class texture
                {
                    public string Path;
                    public Texture2D Texture;
                    public Texture2D Previous;
                }

                [IgnoreJson] public ModEntry modEntry;
                public string Name;
                public string Tags;
                public conditions Conditions;
                [IgnoreJson] public Dictionary<string, texture> textures;

                public override string ToString()
                {
                    return $"{Name} ({modEntry.Info.DisplayName})";
                }

                public void WriteFile(string filePath)
                {
                    try
                    {
                        File.WriteAllText(filePath, JSONWriter.ToJson(this));
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.ToString());
                        Logger.Error($"Error file creating '{filePath}'.");
                    }
                }

                public static Skin ReadFile(string filePath)
                {
                    try
                    {
                        return JSONParser.FromJson<TextureReplacer.Skin>(File.ReadAllText(filePath));
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.ToString());
                        Logger.Error($"Can't read file '{filePath}'.");
                        return null;
                    }
                }

                public static implicit operator bool(Skin exists)
                {
                    return exists != null;
                }

                public bool Equals(Skin other)
                {
                    return Name.Equals(other.Name) && modEntry.Info.Equals(other.modEntry.Info);
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj))
                    {
                        return false;
                    }
                    return obj is Skin skin && Equals(skin);
                }

                public override int GetHashCode()
                {
                    return Name.GetHashCode() + modEntry.Info.GetHashCode();
                }
            }

            public static void Start()
            {
            }
        }
    }
}
