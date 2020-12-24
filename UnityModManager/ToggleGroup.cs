using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityModManagerNet
{
    public partial class UnityModManager
    {
        public partial class UI
        {
            class PopupToggleGroup_GUI
            {
                internal static readonly List<PopupToggleGroup_GUI> mList = new List<PopupToggleGroup_GUI>();
                internal readonly HashSet<int> mDestroyCounter = new HashSet<int>();

                private const int MARGIN = 100;

                private int mId;
                private Rect mWindowRect;
                private Vector2 mScrollPosition;
                private int mWidth;
                private int mHeight;
                private int mRecalculateFrame;

                private bool Recalculating
                {
                    get { return mRecalculateFrame == Time.frameCount; }
                }

                private bool mOpened;
                public bool Opened
                {
                    get { return mOpened; }
                    set
                    {
                        mOpened = value;
                        if (value)
                            Reset();
                    }
                }

                public int? newSelected;
                public int selected;

                public readonly string[] values;
                public string title;
                public int unique;

                public PopupToggleGroup_GUI(string[] values)
                {
                    mId = GetNextWindowId();
                    mList.Add(this);
                    this.values = values;
                }

                public void Button(string text = null, GUIStyle style = null, params GUILayoutOption[] option)
                {
                    mDestroyCounter.Clear();
                    if (GUILayout.Button(text ?? values[selected], style ?? GUI.skin.button, option))
                    {
                        if (!Opened)
                        {
                            foreach (var popup in mList)
                            {
                                popup.Opened = false;
                            }
                            Opened = true;
                            return;
                        }

                        Opened = false;
                    }
                }

                public void Render()
                {
                    if (Recalculating)
                    {
                        mWindowRect = GUILayout.Window(mId, mWindowRect, WindowFunction, "", window);
                        if (mWindowRect.width > 0)
                        {
                            mWidth = (int)(Math.Min(Math.Max(mWindowRect.width, 250), Screen.width - MARGIN));
                            mHeight = (int)(Math.Min(mWindowRect.height, Screen.height - MARGIN));
                            mWindowRect.x = (int)(Math.Max(Screen.width - mWidth, 0) / 2);
                            mWindowRect.y = (int)(Math.Max(Screen.height - mHeight, 0) / 2);
                        }
                    }
                    else
                    {
                        mWindowRect = GUILayout.Window(mId, mWindowRect, WindowFunction, "", window, GUILayout.Width(mWidth), GUILayout.Height(mHeight + 20));
                        GUI.BringWindowToFront(mId);
                    }
                }

                private void WindowFunction(int windowId)
                {
                    if (title != null)
                        GUILayout.Label(title, h1);
                    if (!Recalculating)
                        mScrollPosition = GUILayout.BeginScrollView(mScrollPosition);
                    if (values != null)
                    {
                        int i = 0;
                        foreach (var option in values)
                        {
                            if (GUILayout.Button(i == selected ? "<b>" + option + "</b>" : option))
                            {
                                newSelected = i;
                                Opened = false;
                            }
                            i++;
                        }
                    }
                    if (!Recalculating)
                        GUILayout.EndScrollView();
                    //if (GUILayout.Button("Close", button))
                    //    Opened = false;
                }

                internal void Reset()
                {
                    mRecalculateFrame = Time.frameCount;
                    mWindowRect = new Rect(-9000, 0, 0, 0);
                }
            }

            /// <summary>
            /// [0.18.0]
            /// </summary>
            /// <returns>
            /// Returns true if the value has changed.
            /// </returns>
            public static bool PopupToggleGroup(ref int selected, string[] values, GUIStyle style = null, params GUILayoutOption[] buttonOption)
            {
                var changed = false;
                var newSelected = selected;
                PopupToggleGroup(selected, values, (i) => { changed = true; newSelected = i; }, style, buttonOption);
                selected = newSelected;
                return changed;
            }

            /// <summary>
            /// [0.18.0]
            /// </summary>
            /// <returns>
            /// Returns true if the value has changed.
            /// </returns>
            public static bool PopupToggleGroup(ref int selected, string[] values, string title, GUIStyle style = null, params GUILayoutOption[] buttonOption)
            {
                var changed = false;
                var newSelected = selected;
                PopupToggleGroup(selected, values, (i) => { changed = true; newSelected = i; }, title, style, buttonOption);
                selected = newSelected;
                return changed;
            }

            /// <summary>
            /// [0.22.15]
            /// </summary>
            /// <returns>
            /// Returns true if the value has changed.
            /// </returns>
            public static bool PopupToggleGroup(ref int selected, string[] values, string title, int unique, GUIStyle style = null, params GUILayoutOption[] buttonOption)
            {
                var changed = false;
                var newSelected = selected;
                PopupToggleGroup(selected, values, (i) => { changed = true; newSelected = i; }, title, unique, style, buttonOption);
                selected = newSelected;
                return changed;
            }

            /// <summary>
            /// [0.16.0]
            /// </summary>
            public static void PopupToggleGroup(int selected, string[] values, Action<int> onChange, GUIStyle style = null, params GUILayoutOption[] buttonOption)
            {
                PopupToggleGroup(selected, values, onChange, null, style, buttonOption);
            }

            /// <summary>
            /// [0.16.0]
            /// </summary>
            public static void PopupToggleGroup(int selected, string[] values, Action<int> onChange, string title, GUIStyle style = null, params GUILayoutOption[] buttonOption)
            {
                PopupToggleGroup(selected, values, onChange, title, 0, style, buttonOption);
            }

            /// <summary>
            /// [0.22.15]
            /// </summary>
            public static void PopupToggleGroup(int selected, string[] values, Action<int> onChange, string title, int unique, GUIStyle style = null, params GUILayoutOption[] buttonOption)
            {
                if (values == null)
                {
                    throw new ArgumentNullException("values");
                }
                if (onChange == null)
                {
                    throw new ArgumentNullException("onChange");
                }
                if (values.Length == 0)
                {
                    throw new IndexOutOfRangeException();
                }
                bool needInvoke = false;
                if (selected >= values.Length)
                {
                    selected = values.Length - 1;
                    needInvoke = true;
                }
                else if (selected < 0)
                {
                    selected = 0;
                    needInvoke = true;
                }
                PopupToggleGroup_GUI obj = null;
                foreach (var item in PopupToggleGroup_GUI.mList)
                {
                    if (unique == 0 && item.title == title && item.values.SequenceEqual(values) || unique != 0 && item.unique == unique && item.title == title)
                    {
                        obj = item;
                        break;
                    }
                }
                if (obj == null)
                {
                    obj = new PopupToggleGroup_GUI(values);
                    obj.title = title;
                    obj.unique = unique;
                }
                if (obj.newSelected != null && selected != obj.newSelected.Value && obj.newSelected.Value < values.Length)
                {
                    selected = obj.newSelected.Value;
                    needInvoke = true;
                }
                obj.selected = selected;
                obj.newSelected = null;
                obj.Button(null, style, buttonOption);
                if (needInvoke)
                {
                    try
                    {
                        onChange.Invoke(selected);
                    }
                    catch (Exception e)
                    {
                        Logger.Error("PopupToggleGroup: " + e.GetType() + " - " + e.Message);
                        Console.WriteLine(e.ToString());
                    }
                }
            }

            /// <summary>
            /// [0.18.0]
            /// </summary>
            /// <returns>
            /// Returns true if the value has changed.
            /// </returns>
            public static bool ToggleGroup(ref int selected, string[] values, GUIStyle style = null, params GUILayoutOption[] option)
            {
                var changed = false;
                var newSelected = selected;
                ToggleGroup(selected, values, (i) => { changed = true; newSelected = i; }, style, option);
                selected = newSelected;
                return changed;
            }

            /// <summary>
            /// [0.16.0]
            /// </summary>
            public static void ToggleGroup(int selected, string[] values, Action<int> onChange, GUIStyle style = null, params GUILayoutOption[] option)
            {
                if (values == null)
                {
                    throw new ArgumentNullException("values");
                }
                if (onChange == null)
                {
                    throw new ArgumentNullException("onChange");
                }
                if (values.Length == 0)
                {
                    throw new IndexOutOfRangeException();
                }
                bool needInvoke = false;
                if (selected >= values.Length)
                {
                    selected = values.Length - 1;
                    needInvoke = true;
                }
                else if (selected < 0)
                {
                    selected = 0;
                    needInvoke = true;
                }

                int i = 0;
                foreach (var str in values)
                {
                    var prev = selected == i;
                    var value = GUILayout.Toggle(prev, str, style ?? GUI.skin.toggle, option);
                    if (value && !prev)
                    {
                        selected = i;
                        needInvoke = true;
                    }
                    i++;
                }
                if (needInvoke)
                {
                    try
                    {
                        onChange.Invoke(selected);
                    }
                    catch (Exception e)
                    {
                        Logger.Error("ToggleGroup: " + e.GetType() + " - " + e.Message);
                        Console.WriteLine(e.ToString());
                    }
                }
            }
        }
    }
}
