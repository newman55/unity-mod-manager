using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityModManagerNet
{
    public partial class UnityModManager
    {
        class GameScripts
        {
            readonly static List<GameScript> scripts = new List<GameScript>();

            public static void Init()
            {
                var currentGame = Config.Name.Replace(" ", "").Replace(":", "");
                foreach (var t in typeof(GameScripts).GetNestedTypes(BindingFlags.NonPublic))
                {
                    if (t.IsClass && t.IsSubclassOf(typeof(GameScript)) && t.Name == currentGame)
                    {
                        var script = (GameScript)Activator.CreateInstance(t);
                        scripts.Add(script);
                        Logger.Log($"Initialize game script {t.Name}");
                    }
                }
            }

            public static void OnBeforeLoadMods()
            {
                foreach (var o in scripts)
                {
                    try
                    {
                        o.OnBeforeLoadMods();
                    }
                    catch (Exception e)
                    {
                        Logger.LogException("OnBeforeLoadMods", e);
                    }
                }
            }

            public static void OnAfterLoadMods()
            {
                foreach (var o in scripts)
                {
                    try
                    {
                        o.OnAfterLoadMods();
                    }
                    catch (Exception e)
                    {
                        Logger.LogException("OnAfterLoadMods", e);
                    }
                }
            }

            public static void OnModToggle(ModEntry modEntry, bool value)
            {
                foreach(var o in scripts)
                {
                    try
                    {
                        o.OnModToggle(modEntry, value);
                    }
                    catch (Exception e)
                    {
                        Logger.LogException("OnModToggle", e);
                    }
                }
            }

            public static void OnToggleWindow(bool value)
            {
                foreach (var o in scripts)
                {
                    try
                    {
                        o.OnToggleWindow(value);
                    }
                    catch (Exception e)
                    {
                        Logger.LogException("OnToggleWindow", e);
                    }
                }
            }

            class GameScript
            {
                public virtual void OnModToggle(ModEntry modEntry, bool value) { }
                public virtual void OnBeforeLoadMods() { }
                public virtual void OnAfterLoadMods() { }
                /// <summary>
                /// [0.21.3]
                /// </summary>
                public virtual void OnToggleWindow(bool value) { }
            }

            // Insert here a class named as game to execute custom script for a game.

            /*
            class RiskofRain2 : GameScript
            {
                public override void OnModToggle(ModEntry modEntry, bool value)
                {
                    if (modEntry.Info.IsCheat)
                    {
                        if (value)
                        {
                            SetModded(true);
                        }
                        else if (modEntries.All(x => x == modEntry || !x.Info.IsCheat))
                        {
                            SetModded(false);
                        }
                    }
                }

                public override void OnBeforeLoadMods()
                {
                    forbidDisableMods = true;
                }

                private static FieldInfo mFieldModded;
                public static FieldInfo FieldModded
                {
                    get
                    {
                        if (mFieldModded == null)
                        {
                            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                            {
                                if (assembly.ManifestModule.Name == "Assembly-CSharp.dll")
                                {
                                    mFieldModded = assembly.GetType("RoR2.RoR2Application").GetField("isModded", BindingFlags.Public | BindingFlags.Static);
                                    break;
                                }
                            }
                        }
                        return mFieldModded;
                    }
                }

                public static bool GetModded()
                {
                    return (bool)FieldModded.GetValue(null);
                }

                public static void SetModded(bool value)
                {
                    FieldModded.SetValue(null, value);
                }
            }
            */
        }
    }
}
