using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UnityModManagerNet
{
    public partial class UnityModManager
    {
        public partial class UI
        {
            internal class WindowParams
            {
                public int? Width { get; set; }
                public int? Height { get; set; }
            }

            internal class Window_GUI
            {
                internal static readonly List<Window_GUI> mList = new List<Window_GUI>();
                internal readonly HashSet<int> mDestroyCounter = new HashSet<int>();

                private const int MARGIN = 50;

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
                    internal set
                    {
                        mOpened = value;
                        if (value)
                        {
                            Reset();
                        }
                    }
                }

                public WindowParams Params { get; internal set; }

                internal string title;
                internal int unique;
                internal Action<Window_GUI> onGui;
                internal Action onClose;

                public Window_GUI(Action<Window_GUI> onGui, Action onClose, string title, int unique, WindowParams @params = null)
                {
                    mId = GetNextWindowId();
                    mList.Add(this);
                    this.onGui = onGui;
                    this.onClose = onClose;
                    this.unique = unique;
                    this.title = title;
                    Params = @params ?? new WindowParams();
                }

                public void Render()
                {
                    if (Recalculating)
                    {
                        mWindowRect = GUILayout.Window(mId, mWindowRect, WindowFunction, "", window);
                        if (mWindowRect.width > 0)
                        {
                            mWidth = (int)(Math.Min(Params.Width ?? mWindowRect.width, Screen.width - MARGIN * 2));
                            mHeight = (int)(Math.Min(Params.Height ?? mWindowRect.height, Screen.height - MARGIN * 2));
                            mWindowRect.x = (int)(Math.Max(Screen.width - mWidth, 0) / 2);
                            mWindowRect.y = (int)(Math.Max(Screen.height - mHeight, 0) / 2);
                        }
                    }
                    else
                    {
                        mWindowRect = GUILayout.Window(mId, mWindowRect, WindowFunction, "", window, GUILayout.Width(mWidth), GUILayout.Height(mHeight + 10));
                        GUI.BringWindowToFront(mId);
                    }
                }

                private void WindowFunction(int windowId)
                {
                    if (title != null)
                        GUILayout.Label(title, h1);
                    if (!Recalculating)
                        mScrollPosition = GUILayout.BeginScrollView(mScrollPosition);
                    onGui.Invoke(this);
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

                /// <summary>
                /// []
                /// </summary>
                public void Close()
                {
                    if (!Opened) return;

                    Opened = false;

                    if (onClose != null)
                    {
                        try
                        {
                            onClose();
                        }
                        catch (Exception e)
                        {
                            Logger.Error("Window.OnClose: " + e.GetType() + " - " + e.Message);
                            Console.WriteLine(e.ToString());
                        }
                    }
                }
            }

            /// <summary>
            /// Creates and displays window. Caches by title or unique. (deferred) []
            /// </summary>
            internal static void ShowWindow(Action<Window_GUI> onGui, string title, int unique, WindowParams windowParams = null)
            {
                ShowWindow(onGui, null, title, unique, windowParams); 
            }

            /// <summary>
            /// Creates and displays window. Caches by title or unique saving window parameters. (deferred) []
            /// </summary>
            internal static void ShowWindow(Action<Window_GUI> onGui, Action onClose, string title, int unique, WindowParams windowParams = null)
            {
                if (onGui == null)
                {
                    throw new ArgumentNullException("onGui");
                }

                Window_GUI obj = null;
                foreach (var item in Window_GUI.mList)
                {
                    if (unique == 0 && item.title == title || unique != 0 && item.unique == unique)
                    {
                        item.Close();
                        obj = item;
                        obj.title = title;
                        obj.onGui = onGui;
                        obj.onClose = onClose;
                        break;
                    }
                }
                if (obj == null)
                {
                    obj = new Window_GUI(onGui, onClose, title, unique, windowParams);
                }
                obj.Opened = true;
            }
        }
    }
}
