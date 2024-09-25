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
            static readonly List<PopupWindow> mPopupList = new List<PopupWindow>();

            class PopupWindow
            {
                public HashSet<int> DestroyCounter { get; set; } = new HashSet<int>();

                internal const int MARGIN = 100;

                internal int mId;
                internal Rect mWindowRect;
                internal Vector2 mScrollPosition;
                internal int mWidth;
                internal int mHeight;
                internal int mRecalculateFrame;

                public PopupWindow()
                {
                    mId = GetNextWindowId();
                    mPopupList.Add(this);
                }

                internal bool Recalculating
                {
                    get { return mRecalculateFrame == Time.frameCount; }
                }

                internal bool mOpened;
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

                public void Reset()
                {
                    mRecalculateFrame = Time.frameCount;
                    mWindowRect = new Rect(-9000, 0, 0, 0);
                }

                public virtual void Render()
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

                public virtual void WindowFunction(int windowId)
                {
                    throw new NotImplementedException();
                }
            }

            class PopupToggleGroup_GUI : PopupWindow
            {
                public int? newSelected;
                public int selected;

                public readonly string[] values;
                public string title;
                public int unique;

                public PopupToggleGroup_GUI(string[] values) : base()
                {
                    this.values = values;
                }

                public void Button(string text = null, GUIStyle style = null, params GUILayoutOption[] option)
                {
                    DestroyCounter.Clear();
                    if (GUILayout.Button(text ?? values[selected], style ?? GUI.skin.button, option))
                    {
                        if (!Opened)
                        {
                            foreach (var popup in mPopupList)
                            {
                                popup.Opened = false;
                            }
                            Opened = true;
                            return;
                        }

                        Opened = false;
                    }
                }

                public override void WindowFunction(int windowId)
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
            }

            /// <summary>
            /// Single choice checkboxes popup. 
            /// </summary>
            /// <remarks>
            /// <para>[0.18.0]</para>
            /// </remarks>
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
            /// Single choice checkboxes popup. 
            /// </summary>
            /// <remarks>
            /// The <paramref name="title"/> must be unique.
            /// <para>[0.18.0]</para>
            /// </remarks>
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
            /// Single choice checkboxes popup. 
            /// </summary>
            /// <remarks>
            /// The <paramref name="unique"/> must be unique.
            /// <para>[0.22.15]</para>
            /// </remarks>
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
            /// Single choice checkboxes popup. 
            /// </summary>
            /// <remarks>
            /// <para>[0.16.0]</para>
            /// </remarks>
            public static void PopupToggleGroup(int selected, string[] values, Action<int> onChange, GUIStyle style = null, params GUILayoutOption[] buttonOption)
            {
                PopupToggleGroup(selected, values, onChange, null, style, buttonOption);
            }

            /// <summary>
            /// Single choice checkboxes popup. 
            /// </summary>
            /// <remarks>
            /// The <paramref name="title"/> must be unique.
            /// <para>[0.16.0]</para>
            /// </remarks>
            public static void PopupToggleGroup(int selected, string[] values, Action<int> onChange, string title, GUIStyle style = null, params GUILayoutOption[] buttonOption)
            {
                PopupToggleGroup(selected, values, onChange, title, 0, style, buttonOption);
            }

            /// <summary>
            /// Single choice checkboxes popup. 
            /// </summary>
            /// <remarks>
            /// The <paramref name="unique"/> must be unique.
            /// <para>[0.22.15]</para>
            /// </remarks>
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
                foreach (var popup in mPopupList)
                {
                    if (popup is PopupToggleGroup_GUI item && (unique == 0 && item.title == title && item.values.SequenceEqual(values) || unique != 0 && item.unique == unique && item.title == title))
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

            class PopupToggleMulti_GUI : PopupWindow
            {
                public int? newSelected;
                public int selected;

                public readonly string[] values;
                public readonly int[] flags;
                public string title;
                public int unique;

                public PopupToggleMulti_GUI(string[] values, int[] flags) : base()
                {
                    this.values = values;
                    this.flags = flags;
                }

                public void Button(string text = null, GUIStyle style = null, params GUILayoutOption[] option)
                {
                    DestroyCounter.Clear();
                    var first = -1;
                    var count = 0;
                    int i = 0;
                    foreach(var f in flags)
                    {
                        if ((selected & f) == f)
                        {
                            first = first >= 0 ? first : i;
                            count++;
                        }
                        i++;
                    }
                    if (GUILayout.Button(text ?? (count >= 2 ? $"{values[first]} and {count-1} more" : count == 1 ? values[first] : "None"), style ?? GUI.skin.button, option))
                    {
                        if (!Opened)
                        {
                            foreach (var popup in mPopupList)
                            {
                                popup.Opened = false;
                            }
                            Opened = true;
                            return;
                        }

                        Opened = false;
                    }
                }

                public override void WindowFunction(int windowId)
                {
                    if (title != null)
                        GUILayout.Label(title, h1);
                    if (!Recalculating)
                        mScrollPosition = GUILayout.BeginScrollView(mScrollPosition);
                    if (values != null && flags != null)
                    {
                        int i = 0;
                        foreach (var str in values)
                        {
                            var prev = (selected & flags[i]) == flags[i];
                            var value = GUILayout.Toggle(prev, str);
                            if (value != prev)
                            {
                                if ((selected & flags[i]) == flags[i])
                                    newSelected = selected ^ flags[i];
                                else
                                    newSelected = selected | flags[i];
                            }
                            i++;
                        }
                    }
                    if (!Recalculating)
                        GUILayout.EndScrollView();
                    if (GUILayout.Button("Close", button))
                        Opened = false;
                }
            }

            /// <summary>
            /// Multi choice checkboxes popup.
            /// </summary>
            /// <remarks>
            /// Use only with bits.
            /// <para>[0.31.0]</para>
            /// </remarks>
            /// <returns>
            /// Returns true if the value has changed.
            /// </returns>
            public static bool PopupToggleMulti(ref int selected, Type enumType, GUIStyle style = null, params GUILayoutOption[] option)
            {
                var changed = false;
                var newSelected = selected;
                PopupToggleMulti(selected, enumType, (i) => { changed = true; newSelected = i; }, style, option);
                selected = newSelected;
                return changed;
            }

            /// <summary>
            /// Multi choice checkboxes popup.
            /// </summary>
            /// <remarks>
            /// Use only with bits. The <paramref name="title"/> must be unique.
            /// <para>[0.31.0]</para>
            /// </remarks>
            /// <returns>
            /// Returns true if the value has changed.
            /// </returns>
            public static bool PopupToggleMulti(ref int selected, Type enumType, string title, GUIStyle style = null, params GUILayoutOption[] option)
            {
                var changed = false;
                var newSelected = selected;
                PopupToggleMulti(selected, enumType, (i) => { changed = true; newSelected = i; }, title, style, option);
                selected = newSelected;
                return changed;
            }

            /// <summary>
            /// Multi choice checkboxes popup.
            /// </summary>
            /// <remarks>
            /// Use only with bits. The <paramref name="unique"/> must be unique.
            /// <para>[0.31.0]</para>
            /// </remarks>
            /// <returns>
            /// Returns true if the value has changed.
            /// </returns>
            public static bool PopupToggleMulti(ref int selected, Type enumType, string title, int unique, GUIStyle style = null, params GUILayoutOption[] option)
            {
                var changed = false;
                var newSelected = selected;
                PopupToggleMulti(selected, enumType, (i) => { changed = true; newSelected = i; }, title, unique, style, option);
                selected = newSelected;
                return changed;
            }

            /// <summary>
            /// Multi choice checkboxes popup.
            /// </summary>
            /// <remarks>
            /// Use only with bits.
            /// <para>[0.31.0]</para>
            /// </remarks>
            public static void PopupToggleMulti(int selected, Type enumType, Action<int> onChange, GUIStyle style = null, params GUILayoutOption[] option)
            {
                PopupToggleMulti(selected, Enum.GetNames(enumType), Enum.GetValues(enumType).Cast<int>().ToArray(), onChange, style, option);
            }

            /// <summary>
            /// Multi choice checkboxes popup.
            /// </summary>
            /// <remarks>
            /// Use only with bits. The <paramref name="title"/> must be unique.
            /// <para>[0.31.0]</para>
            /// </remarks>
            public static void PopupToggleMulti(int selected, Type enumType, Action<int> onChange, string title, GUIStyle style = null, params GUILayoutOption[] option)
            {
                PopupToggleMulti(selected, Enum.GetNames(enumType), Enum.GetValues(enumType).Cast<int>().ToArray(), onChange, title, style, option);
            }

            /// <summary>
            /// Multi choice checkboxes popup.
            /// </summary>
            /// <remarks>
            /// Use only with bits. The <paramref name="unique"/> must be unique.
            /// <para>[0.31.0]</para>
            /// </remarks>
            public static void PopupToggleMulti(int selected, Type enumType, Action<int> onChange, string title, int unique, GUIStyle style = null, params GUILayoutOption[] option)
            {
                PopupToggleMulti(selected, Enum.GetNames(enumType), Enum.GetValues(enumType).Cast<int>().ToArray(), onChange, title, unique, style, option);
            }

            /// <summary>
            /// Multi choice checkboxes popup.
            /// </summary>
            /// <remarks>
            /// Use only with bits.
            /// <para>[0.31.0]</para>
            /// </remarks>
            /// <returns>
            /// Returns true if the value has changed.
            /// </returns>
            public static bool PopupToggleMulti(ref int selected, string[] names, int[] flags, GUIStyle style = null, params GUILayoutOption[] option)
            {
                var changed = false;
                var newSelected = selected;
                PopupToggleMulti(selected, names, flags, (i) => { changed = true; newSelected = i; }, style, option);
                selected = newSelected;
                return changed;
            }

            /// <summary>
            /// Multi choice checkboxes popup.
            /// </summary>
            /// <remarks>
            /// Use only with bits. The <paramref name="title"/> must be unique.
            /// <para>[0.31.0]</para>
            /// </remarks>
            /// <returns>
            /// Returns true if the value has changed.
            /// </returns>
            public static bool PopupToggleMulti(ref int selected, string[] names, int[] flags, string title, GUIStyle style = null, params GUILayoutOption[] option)
            {
                var changed = false;
                var newSelected = selected;
                PopupToggleMulti(selected, names, flags, (i) => { changed = true; newSelected = i; }, title, style, option);
                selected = newSelected;
                return changed;
            }

            /// <summary>
            /// Multi choice checkboxes popup.
            /// </summary>
            /// <remarks>
            /// Use only with bits. The <paramref name="unique"/> must be unique.
            /// <para>[0.31.0]</para>
            /// </remarks>
            /// <returns>
            /// Returns true if the value has changed.
            /// </returns>
            public static bool PopupToggleMulti(ref int selected, string[] names, int[] flags, string title, int unique, GUIStyle style = null, params GUILayoutOption[] option)
            {
                var changed = false;
                var newSelected = selected;
                PopupToggleMulti(selected, names, flags, (i) => { changed = true; newSelected = i; }, title, unique, style, option);
                selected = newSelected;
                return changed;
            }

            /// <summary>
            /// Multi choice checkboxes popup.
            /// </summary>
            /// <remarks>
            /// Use only with bits.
            /// <para>[0.31.0]</para>
            /// </remarks>
            public static void PopupToggleMulti(int selected, string[] names, int[] flags, Action<int> onChange, GUIStyle style = null, params GUILayoutOption[] option)
            {
                PopupToggleMulti(selected, names, flags, onChange, null, 0, style, option);
            }

            /// <summary>
            /// Multi choice checkboxes popup.
            /// </summary>
            /// <remarks>
            /// Use only with bits. The <paramref name="title"/> must be unique.
            /// <para>[0.31.0]</para>
            /// </remarks>
            public static void PopupToggleMulti(int selected, string[] names, int[] flags, Action<int> onChange, string title, GUIStyle style = null, params GUILayoutOption[] option)
            {
                PopupToggleMulti(selected, names, flags, onChange, title, 0, style, option);
            }

            /// <summary>
            /// Multi choice checkboxes popup.
            /// </summary>
            /// <remarks>
            /// Use only with bits. The <paramref name="unique"/> must be unique.
            /// <para>[0.31.0]</para>
            /// </remarks>
            public static void PopupToggleMulti(int selected, string[] names, int[] flags, Action<int> onChange, string title, int unique, GUIStyle style = null, params GUILayoutOption[] buttonOption)
            {
                if (names == null)
                {
                    throw new ArgumentNullException("names");
                }
                if (flags == null)
                {
                    throw new ArgumentNullException("flags");
                }
                if (onChange == null)
                {
                    throw new ArgumentNullException("onChange");
                }
                if (names.Length == 0 || flags.Length == 0 || names.Length != flags.Length)
                {
                    throw new IndexOutOfRangeException();
                }
                var allBits = 0;
                foreach (var f in flags)
                {
                    allBits |= f;
                }
                bool needInvoke = false;
                if (selected > allBits)
                {
                    selected = allBits;
                    needInvoke = true;
                }
                else if (selected < 0)
                {
                    selected = 0;
                    needInvoke = true;
                }
                PopupToggleMulti_GUI obj = null;
                foreach (var popup in mPopupList)
                {
                    if (popup is PopupToggleMulti_GUI item && (unique == 0 && item.title == title && item.values.SequenceEqual(names) || unique != 0 && item.unique == unique && item.title == title))
                    {
                        obj = item;
                        break;
                    }
                }
                if (obj == null)
                {
                    obj = new PopupToggleMulti_GUI(names, flags);
                    obj.title = title;
                    obj.unique = unique;
                }
                if (obj.newSelected != null && selected != obj.newSelected.Value && obj.newSelected.Value <= allBits)
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
                        Logger.Error("PopupToggleMulti: " + e.GetType() + " - " + e.Message);
                        Console.WriteLine(e.ToString());
                    }
                }
            }

            /// <summary>
            /// Single choice checkboxes [0.18.0]
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
            /// Single choice checkboxes [0.16.0]
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

            /// <summary>
            /// Multi choice checkboxes.
            /// </summary>
            /// <remarks>
            /// Use only with bits.
            /// <para>[0.31.0]</para>
            /// </remarks>
            /// <returns>
            /// Returns true if the value has changed.
            /// </returns>
            public static bool ToggleMulti(ref int selected, Type enumType, GUIStyle style = null, params GUILayoutOption[] option)
            {
                var changed = false;
                var newSelected = selected;
                ToggleMulti(selected, enumType, (i) => { changed = true; newSelected = i; }, style, option);
                selected = newSelected;
                return changed;
            }

            /// <summary>
            /// Multi choice checkboxes.
            /// </summary>
            /// <remarks>
            /// Use only with bits.
            /// <para>[0.31.0]</para>
            /// </remarks>
            public static void ToggleMulti(int selected, Type enumType, Action<int> onChange, GUIStyle style = null, params GUILayoutOption[] option)
            {
                ToggleMulti(selected, Enum.GetNames(enumType), Enum.GetValues(enumType).Cast<int>().ToArray(), onChange, style, option);
            }

            /// <summary>
            /// Multi choice checkboxes.
            /// </summary>
            /// <remarks>
            /// Use only with bits.
            /// <para>[0.31.0]</para>
            /// </remarks>
            /// <returns>
            /// Returns true if the value has changed.
            /// </returns>
            public static bool ToggleMulti(ref int selected, string[] names, int[] flags, GUIStyle style = null, params GUILayoutOption[] option)
            {
                var changed = false;
                var newSelected = selected;
                ToggleMulti(selected, names, flags, (i) => { changed = true; newSelected = i; }, style, option);
                selected = newSelected;
                return changed;
            }

            /// <summary>
            /// Multi choice checkboxes.
            /// </summary>
            /// <remarks>
            /// Use only with bits.
            /// <para>[0.31.0]</para>
            /// </remarks>
            public static void ToggleMulti(int selected, string[] names, int[] flags, Action<int> onChange, GUIStyle style = null, params GUILayoutOption[] option)
            {
                if (names == null)
                {
                    throw new ArgumentNullException("names");
                }
                if (flags == null)
                {
                    throw new ArgumentNullException("flags");
                }
                if (onChange == null)
                {
                    throw new ArgumentNullException("onChange");
                }
                if (names.Length == 0 || flags.Length == 0 || names.Length != flags.Length)
                {
                    throw new IndexOutOfRangeException();
                }
                var allBits = 0;
                foreach (var f in flags)
                {
                    allBits |= f;
                }
                bool needInvoke = false;
                if (selected > allBits)
                {
                    selected = allBits;
                    needInvoke = true;
                }
                else if (selected < 0)
                {
                    selected = 0;
                    needInvoke = true;
                }

                int i = 0;
                foreach (var str in names)
                {
                    var prev = (selected & flags[i]) == flags[i];
                    var value = GUILayout.Toggle(prev, str, style ?? GUI.skin.toggle, option);
                    if (value != prev)
                    {
                        if ((selected & flags[i]) == flags[i])
                            selected ^= flags[i];
                        else
                            selected |= flags[i];
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
                        Logger.Error("MultiToggleGroup: " + e.GetType() + " - " + e.Message);
                        Console.WriteLine(e.ToString());
                    }
                }
            }
        }
    }
}
