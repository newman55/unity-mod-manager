using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnityModManagerNet
{
    /// <summary>
    /// [0.20.0]
    /// </summary>
    [Serializable]
    public class KeyBinding
    {
        public KeyCode keyCode = KeyCode.None;
        public byte modifiers;

        private int m_Index = -1;
        internal int Index
        {
            get
            {
                if (m_Index == -1)
                {
                    m_Index = Array.FindIndex(KeysCode, x => x == keyCode.ToString());
                }
                if (m_Index == -1)
                {
                    Change(KeyCode.None);
                    return 0;
                }
                return m_Index;
            }
        }

        internal static object KeyControlZero = new object();
        private object m_KeyControl;
        internal object KeyControl
        {
            get
            {
                if (!LegacyInputDisabled || keyboard == null)
                    return KeyControlZero;

                if (m_KeyControl == null && keyCode != KeyCode.None)
                {
                    if (LegacyToInputSystemMap.TryGetValue(keyCode.ToString(), out var name))
                    {
                        m_KeyControl = keyControlFromStringPI.GetValue(keyboard, new object[] { name });
                    }
                }
                if (m_KeyControl == null)
                {
                    m_KeyControl = KeyControlZero;
                    if (keyCode != KeyCode.None)
                    {
                        UnityModManager.Logger.Log($"Key {keyCode} was not found in new Input System.");
                    }
                }
                return m_KeyControl;
            }
        }

        /// <summary>
        /// [0.22.12]
        /// </summary>
        public static bool LegacyInputDisabled { get; private set; }

        static KeyBinding()
        {
            var k = EnabledKeys.Keys.Intersect(Enum.GetNames(typeof(KeyCode)));
            KeysCode = k.ToArray();
            KeysName = k.Select(x => EnabledKeys[x]).ToArray();
        }

        static Type inputControlType;
        static Type keyboardType;
        static Type keyControlType;

        static PropertyInfo currentKeyboardPI;
        static PropertyInfo keyControlFromStringPI;

        static PropertyInfo isPressedPI;
        static PropertyInfo wasPressedThisFramePI;
        static PropertyInfo wasReleasedThisFramePI;

        static object keyboard;
        static object keyControlCtrl;
        static object keyControlShift;
        static object keyControlAlt;
        internal static object keyControlEscape;

        internal static string[] KeysCode;
        internal static string[] KeysName;

        private static bool hasErrors = true;

        internal static void Initialize()
        {
            try
            {
                if (Input.GetKey(KeyCode.Space))
                {
                    LegacyInputDisabled = false;
                }
            }
            catch (Exception e)
            {
                LegacyInputDisabled = true;
                UnityModManager.Logger.Log($"Legacy Input is disabled.");
            }

            if (LegacyInputDisabled)
            {
                try
                {
                    var assembly = Assembly.Load("Unity.InputSystem");

                    inputControlType = assembly.GetType("UnityEngine.InputSystem.InputControl");
                    if (inputControlType == null)
                        UnityModManager.Logger.Error("Type UnityEngine.InputSystem.InputControl not found.");

                    keyboardType = assembly.GetType("UnityEngine.InputSystem.Keyboard");
                    if (keyboardType == null)
                        UnityModManager.Logger.Error("Type UnityEngine.InputSystem.Keyboard not found.");

                    keyControlType = assembly.GetType("UnityEngine.InputSystem.Controls.KeyControl");
                    if (keyControlType == null)
                        UnityModManager.Logger.Error("Type UnityEngine.InputSystem.Controls.KeyControl not found.");

                    currentKeyboardPI = keyboardType.GetProperty("current", BindingFlags.Public | BindingFlags.Static);
                    if (currentKeyboardPI == null)
                        UnityModManager.Logger.Error("Property current not found.");

                    keyControlFromStringPI = inputControlType.GetProperty("Item");
                    if (keyControlFromStringPI == null)
                        UnityModManager.Logger.Error("Property Item not found.");

                    isPressedPI = keyControlType.GetProperty("isPressed");
                    if (isPressedPI == null)
                        UnityModManager.Logger.Error("Property isPressed not found.");

                    wasPressedThisFramePI = keyControlType.GetProperty("wasPressedThisFrame");
                    if (wasPressedThisFramePI == null)
                        UnityModManager.Logger.Error("Property wasPressedThisFrame not found.");

                    wasReleasedThisFramePI = keyControlType.GetProperty("wasReleasedThisFrame");
                    if (wasReleasedThisFramePI == null)
                        UnityModManager.Logger.Error("Property wasReleasedThisFrame not found.");

                    hasErrors = currentKeyboardPI == null || keyControlFromStringPI == null || isPressedPI == null || wasPressedThisFramePI == null || wasReleasedThisFramePI == null;

                    return;
                }
                catch(Exception e)
                {
                    hasErrors = true;
                    UnityModManager.Logger.LogException(e);
                    UnityModManager.Logger.Error("Legacy Input was marked as disabled, but new Input System was not found.");
                }
            }
        }

        internal static void BindKeyboard()
        {
            if (LegacyInputDisabled)
            {
                if (hasErrors)
                    return;

                var currentKeyboard = currentKeyboardPI.GetValue(null, null);
                if (currentKeyboard != keyboard)
                {
                    keyboard = currentKeyboard;

                    if (keyboard != null)
                    {
                        UnityModManager.Logger.Log("Detected keyboard.");

                        keyControlCtrl = keyControlFromStringPI.GetValue(keyboard, new object[] { "ctrl" });
                        if (keyControlCtrl == null)
                            UnityModManager.Logger.Error("Value keyControlCtrl is null.");

                        keyControlShift = keyControlFromStringPI.GetValue(keyboard, new object[] { "shift" });
                        if (keyControlShift == null)
                            UnityModManager.Logger.Error("Value keyControlShift is null.");

                        keyControlAlt = keyControlFromStringPI.GetValue(keyboard, new object[] { "alt" });
                        if (keyControlAlt == null)
                            UnityModManager.Logger.Error("Value keyControlAlt is null.");

                        keyControlEscape = keyControlFromStringPI.GetValue(keyboard, new object[] { "escape" });
                        if (keyControlEscape == null)
                            UnityModManager.Logger.Error("Value keyControlEscape is null.");

                        hasErrors = hasErrors || keyControlCtrl == null || keyControlShift == null || keyControlAlt == null;
                    }
                }
            }
        }

        public static bool Ctrl()
        {
            if (LegacyInputDisabled)
            {
                return !hasErrors && (bool)isPressedPI.GetValue(keyControlCtrl, null);
            }
            else
            {
                return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            }
        }

        public static bool Shift()
        {
            if (LegacyInputDisabled)
            {
                return !hasErrors && (bool)isPressedPI.GetValue(keyControlShift, null);
            }
            else
            {
                return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            }
        }

        public static bool Alt()
        {
            if (LegacyInputDisabled)
            {
                return !hasErrors && (bool)isPressedPI.GetValue(keyControlAlt, null);
            }
            else
            {
                return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            }
        }

        public void Change(KeyCode key, bool ctrl, bool shift, bool alt)
        {
            Change(key, (byte)((ctrl ? 1 : 0) + (shift ? 2 : 0) + (alt ? 4 : 0)));
        }

        public void Change(KeyCode key, byte modifier = 0)
        {
            keyCode = key;
            modifiers = modifier;
            m_Index = -1;
            m_KeyControl = null;
        }

        private bool CheckModifierKeys()
        {
            return ((modifiers & 1) == 0 && !Ctrl() || (modifiers & 1) == 1 && Ctrl()) && ((modifiers & 2) == 0 && !Shift() || (modifiers & 2) == 2 && Shift()) && ((modifiers & 4) == 0 && !Alt() || (modifiers & 4) == 4 && Alt());
        }

        /// <summary>
        /// Use to poll key status.
        /// </summary>
        public bool Pressed()
        {
            var b = keyCode != KeyCode.None && CheckModifierKeys();
            if (LegacyInputDisabled)
            {
                return !hasErrors && b && KeyControl != KeyControlZero && (bool)isPressedPI.GetValue(KeyControl, null);
            }
            else
            {
                return b && Input.GetKey(keyCode);
            }
        }

        /// <summary>
        /// Use to poll key status.
        /// </summary>
        public bool Down()
        {
            var b = keyCode != KeyCode.None && CheckModifierKeys();
            if (LegacyInputDisabled)
            {
                return !hasErrors && b && KeyControl != KeyControlZero && (bool)wasPressedThisFramePI.GetValue(KeyControl, null);
            }
            else
            {
                return b && Input.GetKeyDown(keyCode);
            }
        }

        /// <summary>
        /// Use to poll key status.
        /// </summary>
        public bool Up()
        {
            var b = keyCode != KeyCode.None && CheckModifierKeys();
            if (LegacyInputDisabled)
            {
                return !hasErrors && b && KeyControl != KeyControlZero && (bool)wasReleasedThisFramePI.GetValue(KeyControl, null);
            }
            else
            {
                return b && Input.GetKeyUp(keyCode);
            }
        }

        private readonly static Dictionary<string, string> EnabledKeys = new Dictionary<string, string>
        {
            { "None", "None" },
            { "BackQuote", "~" },
            { "Tab", "Tab" },
            { "Space", "Space" },
            { "Return", "Enter" },

            { "Alpha0", "0" },
            { "Alpha1", "1" },
            { "Alpha2", "2" },
            { "Alpha3", "3" },
            { "Alpha4", "4" },
            { "Alpha5", "5" },
            { "Alpha6", "6" },
            { "Alpha7", "7" },
            { "Alpha8", "8" },
            { "Alpha9", "9" },
            { "Minus", "-" },
            { "Equals", "=" },
            { "Backspace", "Backspace" },

            { "F1", "F1" },
            { "F2", "F2" },
            { "F3", "F3" },
            { "F4", "F4" },
            { "F5", "F5" },
            { "F6", "F6" },
            { "F7", "F7" },
            { "F8", "F8" },
            { "F9", "F9" },
            { "F10", "F10" },
            { "F11", "F11" },
            { "F12", "F12" },

            { "A", "A" },
            { "B", "B" },
            { "C", "C" },
            { "D", "D" },
            { "E", "E" },
            { "F", "F" },
            { "G", "G" },
            { "H", "H" },
            { "I", "I" },
            { "J", "J" },
            { "K", "K" },
            { "L", "L" },
            { "M", "M" },
            { "N", "N" },
            { "O", "O" },
            { "P", "P" },
            { "Q", "Q" },
            { "R", "R" },
            { "S", "S" },
            { "T", "T" },
            { "U", "U" },
            { "V", "V" },
            { "W", "W" },
            { "X", "X" },
            { "Y", "Y" },
            { "Z", "Z" },

            { "LeftBracket", "[" },
            { "RightBracket", "]" },
            { "Semicolon", ";" },
            { "Quote", "'" },
            { "Backslash", "\\" },
            { "Comma", "," },
            { "Period", "." },
            { "Slash", "/" },

            { "Insert", "Insert" },
            { "Home", "Home" },
            { "Delete", "Delete" },
            { "End", "End" },
            { "PageUp", "Page Up" },
            { "PageDown", "Page Down" },
            { "UpArrow", "Up Arrow" },
            { "DownArrow", "Down Arrow" },
            { "RightArrow", "Right Arrow" },
            { "LeftArrow", "Left Arrow" },

            { "KeypadDivide", "Numpad /" },
            { "KeypadMultiply", "Numpad *" },
            { "KeypadMinus", "Numpad -" },
            { "KeypadPlus", "Numpad +" },
            { "KeypadEnter", "Numpad Enter" },
            { "KeypadPeriod", "Numpad Del" },
            { "Keypad0", "Numpad 0" },
            { "Keypad1", "Numpad 1" },
            { "Keypad2", "Numpad 2" },
            { "Keypad3", "Numpad 3" },
            { "Keypad4", "Numpad 4" },
            { "Keypad5", "Numpad 5" },
            { "Keypad6", "Numpad 6" },
            { "Keypad7", "Numpad 7" },
            { "Keypad8", "Numpad 8" },
            { "Keypad9", "Numpad 9" },

            { "RightShift", "Right Shift" },
            { "LeftShift", "Left Shift" },
            { "RightControl", "Right Ctrl" },
            { "LeftControl", "Left Ctrl" },
            { "RightAlt", "Right Alt" },
            { "LeftAlt", "Left Alt" },

            { "Pause", "Pause" },
            { "Escape", "Escape" },
            { "Numlock", "Num Lock" },
            { "CapsLock", "Caps Lock" },
            { "ScrollLock", "Scroll Lock" },
            { "Print", "Print Screen" },
        };

        private readonly static Dictionary<string, string> LegacyToInputSystemMap = new Dictionary<string, string>
        {
            { "Backspace", "backspace" },
            { "Tab", "tab" },
            { "Pause", "pause" },
            { "Escape", "escape" },
            { "Space", "space" },
            { "Return", "enter" },
            { "Quote", "quote" },
            { "Comma", "comma" },
            { "Minus", "minus" },
            { "Period", "period" },
            { "Slash", "slash" },
            { "Alpha0", "0" },
            { "Alpha1", "1" },
            { "Alpha2", "2" },
            { "Alpha3", "3" },
            { "Alpha4", "4" },
            { "Alpha5", "5" },
            { "Alpha6", "6" },
            { "Alpha7", "7" },
            { "Alpha8", "8" },
            { "Alpha9", "9" },
            { "Semicolon", "semicolon" },
            { "Equals", "equals" },
            { "LeftBracket", "leftBracket" },
            { "Backslash", "backslash" },
            { "RightBracket", "rightBracket" },
            { "BackQuote", "backquote" },
            { "A", "a" },
            { "B", "b" },
            { "C", "c" },
            { "D", "d" },
            { "E", "e" },
            { "F", "f" },
            { "G", "g" },
            { "H", "h" },
            { "I", "i" },
            { "J", "j" },
            { "K", "k" },
            { "L", "l" },
            { "M", "m" },
            { "N", "n" },
            { "O", "o" },
            { "P", "p" },
            { "Q", "q" },
            { "R", "r" },
            { "S", "s" },
            { "T", "t" },
            { "U", "u" },
            { "V", "v" },
            { "W", "w" },
            { "X", "x" },
            { "Y", "y" },
            { "Z", "z" },
            { "Keypad0", "numpad0" },
            { "Keypad1", "numpad1" },
            { "Keypad2", "numpad2" },
            { "Keypad3", "numpad3" },
            { "Keypad4", "numpad4" },
            { "Keypad5", "numpad5" },
            { "Keypad6", "numpad6" },
            { "Keypad7", "numpad7" },
            { "Keypad8", "numpad8" },
            { "Keypad9", "numpad9" },
            { "KeypadPeriod", "numpadPeriod" },
            { "KeypadDivide", "numpadDivide" },
            { "KeypadMultiply", "numpadMultiply" },
            { "KeypadMinus", "numpadMinus" },
            { "KeypadPlus", "numpadPlus" },
            { "KeypadEnter", "numpadEnter" },
            { "UpArrow", "upArrow" },
            { "DownArrow", "downArrow" },
            { "RightArrow", "rightArrow" },
            { "LeftArrow", "leftArrow" },
            { "Insert", "insert" },
            { "Home", "home" },
            { "End", "end" },
            { "Delete", "delete" },
            { "PageUp", "pageUp" },
            { "PageDown", "pageDown" },
            { "F1", "f1" },
            { "F2", "f2" },
            { "F3", "f3" },
            { "F4", "f4" },
            { "F5", "f5" },
            { "F6", "f6" },
            { "F7", "f7" },
            { "F8", "f8" },
            { "F9", "f9" },
            { "F10", "f10" },
            { "F11", "f11" },
            { "F12", "f12" },
            { "Numlock", "numLock" },
            { "CapsLock", "capsLock" },
            { "ScrollLock", "scrollLock" },
            { "RightShift", "rightShift" },
            { "LeftShift", "leftShift" },
            { "RightControl", "rightCtrl" },
            { "LeftControl", "leftCtrl" },
            { "RightAlt", "rightAlt" },
            { "LeftAlt", "leftAlt" },
            { "Print", "printScreen" },
        };
    }
}
