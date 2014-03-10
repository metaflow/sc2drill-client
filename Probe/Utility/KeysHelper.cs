using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Probe.Utility
{
    static class KeysHelper
    {
        public static Dictionary<Keys, Keys> KeysSynonyms = new Dictionary<Keys, Keys>()
                                                          {
                                                              {Keys.LControlKey, Keys.Control},
                                                              {Keys.RControlKey, Keys.Control},
                                                              {Keys.ControlKey, Keys.Control},
                                                              {Keys.ShiftKey, Keys.Shift},
                                                              {Keys.LShiftKey, Keys.Shift},
                                                              {Keys.RShiftKey, Keys.Shift},
                                                              {Keys.LMenu, Keys.Alt},
                                                              {Keys.RMenu, Keys.Alt},
                                                              {Keys.LWin, Keys.RWin},
                                                              {Keys.RWin, Keys.LWin}
                                                          };

        #region strings and Keys dictionary
        private static Dictionary<string, Keys> _stringToKey = new Dictionary<string, Keys>() { 
           {"A", Keys.A},
            {"B", Keys.B},
            {"C", Keys.C},
            {"D", Keys.D},
            {"E", Keys.E},
            {"F", Keys.F},
            {"G", Keys.G},
            {"H", Keys.H},
            {"I", Keys.I},
            {"J", Keys.J},
            {"K", Keys.K},
            {"L", Keys.L},
            {"M", Keys.M},
            {"N", Keys.N},
            {"O", Keys.O},
            {"P", Keys.P},
            {"Q", Keys.Q},
            {"R", Keys.R},
            {"S", Keys.S},
            {"T", Keys.T},
            {"U", Keys.U},
            {"V", Keys.V},
            {"W", Keys.W},
            {"X", Keys.X},
            {"Y", Keys.Y},
            {"Z", Keys.Z},
            {"1", Keys.D1},
            {"2", Keys.D2},
            {"3", Keys.D3},
            {"4", Keys.D4},
            {"5", Keys.D5},
            {"6", Keys.D6},
            {"7", Keys.D7},
            {"8", Keys.D8},
            {"9", Keys.D9},
            {"0", Keys.D0},
            {"Space", Keys.Space},
            {"Tab", Keys.Tab},
            {"Ctrl", Keys.ControlKey},
            {"Alt", Keys.Alt},
            {"Shift", Keys.Shift},
            {"Windows", Keys.LWin},
            {"Applications key", Keys.Apps},
            {"Caps Lock", Keys.CapsLock},
            {"Num Lock", Keys.NumLock},
            {"Scroll Lock", Keys.Scroll},
            {"Backspace", Keys.Back},
            {"Enter", Keys.Return},
            {"Insert", Keys.Insert},
            {"Delete", Keys.Delete},
            {"Home", Keys.Home},
            {"End", Keys.End},
            {"Page Up", Keys.PageUp},
            {"Page Down", Keys.PageDown},
            {"Print Screen", Keys.PrintScreen},
            {"Pause", Keys.Pause},
            {"Up", Keys.Up},
            {"Down", Keys.Down},
            {"Left", Keys.Left},
            {"Right", Keys.Right},
            {"'", Keys.Oem7},
            {",", Keys.Oemcomma},
            {".", Keys.OemPeriod},
            {"/", Keys.OemQuestion},
            {";", Keys.Oem1},
            {"[", Keys.OemOpenBrackets},
            {"]", Keys.Oem6},
            {"-", Keys.OemMinus},
            {"=", Keys.Oemplus},
            {"\\", Keys.Oem5},
            {"~", Keys.Oemtilde},
            {"Num 0", Keys.NumPad0},
            {"Num 1", Keys.NumPad1},
            {"Num 2", Keys.NumPad2},
            {"Num 3", Keys.NumPad3},
            {"Num 4", Keys.NumPad4},
            {"Num 5", Keys.NumPad5},
            {"Num 6", Keys.NumPad6},
            {"Num 7", Keys.NumPad7},
            {"Num 8", Keys.NumPad8},
            {"Num 9", Keys.NumPad9},
            {"Num Add", Keys.Add},
            {"Num Multiply", Keys.Multiply},
            {"Num Divide", Keys.Divide},
            {"Num Substract", Keys.Subtract},
            {"F1", Keys.F1},
            {"F2", Keys.F2},
            {"F3", Keys.F3},
            {"F4", Keys.F4},
            {"F5", Keys.F5},
            {"F6", Keys.F6},
            {"F7", Keys.F7},
            {"F8", Keys.F8},
            {"F9", Keys.F9},
            {"F10", Keys.F10},
            {"F11", Keys.F11},
            {"F12", Keys.F12},
            {"F13", Keys.F13},
            {"F14", Keys.F14},
            {"F15", Keys.F15},
            {"F16", Keys.F16},
            {"F17", Keys.F17},
            {"F18", Keys.F18},
            {"F19", Keys.F19},
            {"F20", Keys.F20},
            {"F21", Keys.F21},
            {"F22", Keys.F22},
            {"F23", Keys.F23},
            {"F24", Keys.F24}
        };

        private static Dictionary<Keys, string> _keysToString = new Dictionary<Keys, string>()
        {
            {Keys.Control, "Ctrl"},
            {Keys.Shift, "Shift"},
            {Keys.D0, "0"},
            {Keys.D1, "1"},
            {Keys.D2, "2"},
            {Keys.D3, "3"},
            {Keys.D4, "4"},
            {Keys.D5, "5"},
            {Keys.D6, "6"},
            {Keys.D7, "7"},
            {Keys.D8, "8"},
            {Keys.D9, "9"},
            {Keys.Scroll, "Scroll Lock"},
            {Keys.NumPad0, "Num 0"},
            {Keys.NumPad1, "Num 1"},
            {Keys.NumPad2, "Num 2"},
            {Keys.NumPad3, "Num 3"},
            {Keys.NumPad4, "Num 4"},
            {Keys.NumPad5, "Num 5"},
            {Keys.NumPad6, "Num 6"},
            {Keys.NumPad7, "Num 7"},
            {Keys.NumPad8, "Num 8"},
            {Keys.NumPad9, "Num 9"}
        };
        #endregion

        public static Keys StringToKey(string s)
        {
            return _stringToKey.ContainsKey(s) ? _stringToKey[s] : Keys.None;
        }

        public static string KeyToString(Keys k)
        {
            if (_keysToString.ContainsKey(k))
            {
                return _keysToString[k];
            }
            if (KeysSynonyms.ContainsKey(k) && _keysToString.ContainsKey(KeysSynonyms[k]))
            {
                return _keysToString[KeysSynonyms[k]];
            }
            return k.ToString();
        }

        public static bool IsAlpha(Keys keys)
        {
            return (keys.ToString().Length == 1);
        }

        public static bool IsNumeric(Keys keys)
        {
            return (keys >= Keys.D0) && (keys <= Keys.D9);
        }

        public static bool IsModifier(Keys keys)
        {
            var k = keys;
            if (KeysSynonyms.ContainsKey(k)) k = KeysSynonyms[k];
            return (k == Keys.Shift) || (k == Keys.Control) || (k == Keys.Alt);
        }
    }
}
