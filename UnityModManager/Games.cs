using System;
using System.Reflection;
using UnityEngine;

namespace UnityModManagerNet
{
    public partial class UnityModManager
    {
        class GameScripts
        {
            public static void WhenStartManager()
            {
                try
                {
                    if (Config.Name == "Risk of Rain 2")
                    {
                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            if (assembly.ManifestModule.Name == "Assembly-CSharp.dll")
                            {
                                assembly.GetType("RoR2.RoR2Application").GetField("isModded", BindingFlags.Public | BindingFlags.Static).SetValue(null, true);

                                break;
                            }
                        }
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
