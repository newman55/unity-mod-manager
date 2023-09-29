using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityModManagerNet
{
    public partial class UnityModManager
    {
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
    }
}
