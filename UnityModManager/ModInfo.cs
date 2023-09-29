using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityModManagerNet
{
    public partial class UnityModManager
    {
        public class ModInfo : IEquatable<ModInfo>
        {
            public string Id;

            public string DisplayName;

            public string Author;

            public string Version;

            public string ManagerVersion;

            public string GameVersion;

            public string[] Requirements;

            public string[] LoadAfter;

            public string AssemblyName;

            public string EntryMethod;

            public string HomePage;

            public string Repository;

            public string ContentType;

            /// <summary>
            /// Used for RoR2 game [0.17.0]
            /// </summary>
            [NonSerialized]
            public bool IsCheat = true;

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
    }
}
