using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UnityModManagerNet
{
    public partial class UnityModManager
    {
        public class UI : MonoBehaviour
        {
            internal static bool Load()
            {
                new GameObject(typeof(UI).FullName, typeof(UI));

                return true;
            }

            private static UI mInstance = null;

            public static UI Instance
            {
                get { return mInstance; }
            }

            private bool mOpened = false;

            private float mWindowWidth = 960f;
            private Rect mWindowRect = new Rect(0, 0, 0, 0);

            private void Awake()
            {
                mInstance = this;
                DontDestroyOnLoad(this);

                mOpened = true;
                BlockGameUI(mOpened);

                CalculateWindowPos();
            }

            private void OnGUI()
            {
                if (mOpened)
                {
                    GUI.backgroundColor = Color.black;
                    mWindowRect = GUILayout.Window(0, mWindowRect, WindowFunction, "Mod Manager", GUILayout.Height(Screen.height - 200));
                }
            }

            public int tabId = 0;
            public string[] tabStrings = new string[] { "Mods", "Logs" };

            private readonly Dictionary<string, float> mColumns = new Dictionary<string, float>
            {
                {"Name", 150f},
                {"Version", 60f},
                {"Requirements", 150f},
                {"Settings", 100f},
                {"On/Off", 50f},
                {"Status", 50f}
            };

            private long mFilelogLength = 0;
            private string[] mLogStrings = new string[0];
            private Vector2 mScrollPositionTab1;
            private Vector2 mScrollPositionTab2;

            private int showModSettings = -1;

            private void CalculateWindowPos()
            {
                mWindowRect = new Rect((Screen.width - mWindowWidth) / 2f, 100f, 0, 0);
            }

            private void WindowFunction(int windowId)
            {
                //			GUI.DragWindow(new Rect(0, 0, 10000, 20));

                int tab = tabId;
                tab = GUILayout.Toolbar(tab, tabStrings, GUILayout.Width(150 * 2));
                if (tab != tabId)
                {
                    tabId = tab;
                    CalculateWindowPos();
                }

                var amountWidth = mColumns.Sum(x => x.Value);
                var minWidth = GUILayout.MinWidth(mWindowWidth);

                switch (tabStrings[tabId])
                {
                    case "Mods":
                        {
                            mScrollPositionTab1 = GUILayout.BeginScrollView(mScrollPositionTab1, minWidth);

                            var mods = UnityModManager.modEntries;
                            var colWidth = mColumns.Select(x => GUILayout.Width(x.Value / amountWidth * (mWindowWidth - 60))).ToArray();

                            GUILayout.BeginVertical("box");

                            GUILayout.BeginHorizontal("box");
                            for (int i = 0; i < mColumns.Count; i++)
                            {
                                GUILayout.Label(mColumns.Keys.ElementAt(i), colWidth[i]);
                            }
                            GUILayout.EndHorizontal();

                            for (int i = 0, c = mods.Count; i < c; i++)
                            {
                                int k = -1;
                                GUILayout.BeginVertical("box");
                                GUILayout.BeginHorizontal();
                                
                                GUILayout.Label(mods[i].Info.DisplayName, colWidth[++k]);
                                GUILayout.Label(mods[i].Info.Version, colWidth[++k]);

                                if (mods[i].Requirements.Count > 0)
                                {
                                    GUILayout.Label(string.Join("\r\n", mods[i].Info.Requirements), colWidth[++k]);
                                }
                                else
                                {
                                    GUILayout.Label("-", colWidth[++k]);
                                }

                                if (mods[i].OnGUI != null)
                                {
                                    if (GUILayout.Button("Settings", colWidth[++k]))
                                    {
                                        showModSettings = (showModSettings == i) ? -1 : i;
                                    }
                                }
                                else
                                {
                                    GUILayout.Label("", colWidth[++k]);
                                }

                                var action = mods[i].Enabled;
                                action = GUILayout.Toggle(action, new GUIContent("", !mods[i].Toggleable ? "Need reboot." : ""), colWidth[++k]);
                                if (action != mods[i].Enabled)
                                {
                                    mods[i].Enabled = action;
                                    if (mods[i].Toggleable)
                                        mods[i].Active = action;
                                }

                                if (mods[i].Active)
                                {
                                    GUILayout.Label("Active", colWidth[++k]);
                                }
                                else
                                {
                                    GUILayout.Label("Inactive", colWidth[++k]);
                                }
                                
                                GUILayout.EndHorizontal();

                                if (showModSettings == i)
                                {
                                    //GUILayout.BeginHorizontal();
                                    GUILayout.Label("Settings");
                                    mods[i].OnGUI(mods[i]);
                                    //GUILayout.EndHorizontal();
                                }

                                GUILayout.EndVertical();
                            }

                            GUILayout.EndVertical();

                            GUILayout.EndScrollView();

                            if (GUI.changed)
                            {
                            }
                            break;
                        }

                    case "Logs":
                        {
                            mScrollPositionTab2 = GUILayout.BeginScrollView(mScrollPositionTab2, minWidth);

#if UNITY_EDITOR
					var filepath = Application.dataPath + "/UnityModManager.log";
#else
                            var filepath = UnityModManager.Logger.filepath;
#endif
                            if (File.Exists(filepath))
                            {
                                var fileinfo = new FileInfo(filepath);
                                if (mFilelogLength != fileinfo.Length)
                                {
                                    mFilelogLength = fileinfo.Length;
                                    mLogStrings = File.ReadAllLines(filepath);
                                }

                                for (int i = Mathf.Max(0, mLogStrings.Length - 200); i < mLogStrings.Length; i++)
                                {
                                    GUILayout.Label(mLogStrings[i]);
                                }

                                GUILayout.EndScrollView();

                                if (GUILayout.Button("Clear", GUILayout.Width(150)))
                                {
                                    UnityModManager.Logger.Clear();
                                }
                            }

                            if (GUI.changed)
                            {
                            }
                            break;
                        }
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Close", GUILayout.Width(150)))
                {
                    ToggleWindow(!mOpened);
                }
                if (GUILayout.Button("Save", GUILayout.Width(150)))
                {
                    SaveSettingsAndParams();
                }
                GUILayout.EndHorizontal();
            }

            public void ToggleWindow(bool value)
            {
                mOpened = value;
                BlockGameUI(value);
                showModSettings = -1;
//                if (!mOpened)
//                {
//#if !UNITY_EDITOR
//                    SaveSettingsAndParams();
//#endif
//                }
            }

            private void Update()
            {
                if (Input.GetKeyUp(KeyCode.F10) && (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)))
                {
                    ToggleWindow(!mOpened);
                }
            }

            private void OnDestroy()
            {
#if !UNITY_EDITOR
                SaveSettingsAndParams();
#endif
            }

            private GameObject mCanvas = null;

            private void BlockGameUI(bool value)
            {
                if (value)
                {
                    mCanvas = new GameObject("", typeof(Canvas), typeof(GraphicRaycaster));
                    mCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                    mCanvas.GetComponent<Canvas>().sortingOrder = Int16.MaxValue;
                    DontDestroyOnLoad(mCanvas);
                    var panel = new GameObject("", typeof(Image));
                    panel.transform.SetParent(mCanvas.transform);
                    panel.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0);
                    panel.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                    panel.GetComponent<RectTransform>().offsetMin = Vector2.zero;
                    panel.GetComponent<RectTransform>().offsetMax = Vector2.zero;
                }
                else
                {
                    Destroy(mCanvas);
                }
            }
        }
    }
}
