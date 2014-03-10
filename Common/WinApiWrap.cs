using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Probe.Common
{
    public static class WinApiWrap
    {
        [StructLayout(LayoutKind.Sequential)]    // Required by user32.dll
        public struct RECT
        {
            public uint Left;
            public uint Top;
            public uint Right;
            public uint Bottom;
        };


        [StructLayout(LayoutKind.Sequential)]    // Required by user32.dll
        public struct GUITHREADINFO
        {
            public uint cbSize;
            public uint flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public RECT rcCaret;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll", EntryPoint = "GetGUIThreadInfo")]
        public static extern bool GetGUIThreadInfo(uint tId, out GUITHREADINFO threadInfo);

        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, out Point position);

        [DllImport("user32.dll")]
        static extern int GetFocus();

        [DllImport("user32.dll")]
        static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern Int32 GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, StringBuilder lParam);

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool EnumThreadWindows(int threadId, EnumWindowsProc callback, IntPtr lParam);

        [DllImport("user32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [DllImport("user32")]
        //http://www.codeproject.com/KB/cs/Auto_Clicker_Bar.aspx
        public static extern int SetCursorPos(int x, int y);

        private const int MOUSEEVENTF_MOVE = 0x0001; /* mouse move */
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002; /* left button down */
        private const int MOUSEEVENTF_LEFTUP = 0x0004; /* left button up */
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008; /* right button down */

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll")]
        //http://stackoverflow.com/questions/1316681/getting-mouse-position-in-c
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        public static void MinimizeWindow(IntPtr hWnd)
        {
            ShowWindowAsync(hWnd, 2);
        }

        const int WM_SETTEXT = 12;
        const int WM_GETTEXT = 13;

        public static Point GetCaretPosition()
        {
            var result = new Point(0, 0);
            var guiInfo = GetCurrentThreadGuiInfo();
            result.X = (int)guiInfo.rcCaret.Left;
            result.Y = (int)guiInfo.rcCaret.Bottom;
            ClientToScreen(guiInfo.hwndCaret, out result);
            return result;
        }

        public static RECT GetWindowRectangle(IntPtr hWnd)
        {
            RECT rct = new RECT();
            GetWindowRect(hWnd, ref rct);
            return rct;
        }

        public static Point GetMouseCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            return lpPoint;
        }

        public static void SetMouseCursorPosition(Point p)
        {
            SetCursorPos(p.X, p.Y);
        }

        public static void SendLeftMouseClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        public static GUITHREADINFO GetCurrentThreadGuiInfo()
        {
            var guiInfo = new GUITHREADINFO();
            guiInfo.cbSize = (uint)Marshal.SizeOf(guiInfo);
            // Get GuiThreadInfo into guiInfo
            GetGUIThreadInfo(0, out guiInfo);
            return guiInfo;
        }

        public static string GetChildWindowsText(IntPtr hWnd)
        {
            string result = "";
            EnumChildWindows(hWnd, (hChildWnd, lParam) =>
            {
                result += string.Format("::({0}){1}", GetWindowClassName(hChildWnd), GetWindowText(hChildWnd));
                var r = GetWindowRectangle(hChildWnd);
                result += String.Format("[{0},{1},{2},{3}]\r\n", r.Left, r.Top, r.Right, r.Bottom);
                return true;
            }, IntPtr.Zero);
            return result;
        }

        public static IntPtr FindChildWindow(IntPtr hWnd, string className, string text)
        {
            var result = IntPtr.Zero;
            EnumChildWindows(hWnd, (hChildWnd, lParam) =>
                                       {
                                           bool f = string.IsNullOrEmpty(className) || className.ToLower() == GetWindowClassName(hChildWnd).ToLower();
                                           f &= string.IsNullOrEmpty(text) || text.ToLower() == GetWindowText(hChildWnd).ToLower();
                                           if (f) result = hChildWnd;
                                           return !f;
                                       }, IntPtr.Zero);
            return result;
        }

        public static string GetWindowText(IntPtr hWnd)
        {
            StringBuilder builder = new StringBuilder(32000);
            SendMessage(hWnd, WM_GETTEXT, builder.Capacity, builder);
            return builder.ToString();
        }

        public static Process GetProcessByHandle(IntPtr hwnd)
        {
            try
            {
                uint processID;
                GetWindowThreadProcessId(hwnd, out processID);
                return Process.GetProcessById((int)processID);
            }
            catch { return null; }
        }

        public static string GetFocusedControlName()
        {
            return GetWindowClassName(GetCurrentThreadGuiInfo().hwndFocus);
        }

        public static string GetWindowClassName(IntPtr hwnd)
        {
            var result = new StringBuilder(1000);
            GetClassName(hwnd, result, result.Capacity);
            return result.ToString();
        }

        public static string GetFocusedControlText()
        {
            var info = GetCurrentThreadGuiInfo();
            return GetWindowText(info.hwndFocus);
        }

        public static int GetCurrentThreadKeyboardLayout()
        {
            uint processID;
            var id = GetWindowThreadProcessId(GetForegroundWindow(), out processID);
            var l = GetKeyboardLayout((uint) id);
            InputLanguageCollection installedInputLanguages = InputLanguage.InstalledInputLanguages;
            CultureInfo c = null;
            for (int i = 0; i < installedInputLanguages.Count; i++)
            {
                if (l == installedInputLanguages[i].Handle) c = installedInputLanguages[i].Culture;
            }
            return c != null ? c.KeyboardLayoutId : 0;
        }
    }
}
