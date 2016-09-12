namespace UltimateFishBot.Classes.Helpers
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;

    using UltimateFishBot.Properties;

    internal class Win32
    {
        private const uint WM_LBUTTONDOWN = 513;

        private const uint WM_LBUTTONUP = 514;

        private const uint WM_RBUTTONDOWN = 516;

        private const uint WM_RBUTTONUP = 517;

        private static int LastRectX;

        private static int LastRectY;

        private static int LastX;

        private static int LastY;

        public enum keyState
        {
            KEYDOWN = 0, 

            EXTENDEDKEY = 1, 

            KEYUP = 2
        }

        public static void ActivateApp(string processName)
        {
            var p = Process.GetProcessesByName(processName);

            // Activate the first application we find with this name
            if (p.Count() > 0) SetForegroundWindow(p[0].MainWindowHandle);
        }

        public static void ActivateWow()
        {
            ActivateApp(Settings.Default.ProcName);
            ActivateApp(Settings.Default.ProcName + "-64");
            ActivateApp("World Of Warcraft");
        }

        public static CursorInfo GetCurrentCursor()
        {
            var myInfo = new CursorInfo();
            myInfo.cbSize = Marshal.SizeOf(myInfo);
            GetCursorInfo(out myInfo);
            return myInfo;
        }

        public static Bitmap GetCursorIcon(CursorInfo actualCursor, int width = 35, int height = 35)
        {
            Bitmap actualCursorIcon = null;

            try
            {
                actualCursorIcon = new Bitmap(width, height);
                var g = Graphics.FromImage(actualCursorIcon);
                DrawIcon(g.GetHdc(), 0, 0, actualCursor.hCursor);
                g.ReleaseHdc();
            }
            catch (Exception)
            {
            }

            return actualCursorIcon;
        }

        public static CursorInfo GetNoFishCursor()
        {
            var WoWRect = GetWowRectangle();
            MoveMouse(WoWRect.X + 10, WoWRect.Y + 45);
            LastRectX = WoWRect.X;
            LastRectY = WoWRect.Y;
            Thread.Sleep(15);
            var myInfo = new CursorInfo();
            myInfo.cbSize = Marshal.SizeOf(myInfo);
            GetCursorInfo(out myInfo);
            return myInfo;
        }

        public static Rectangle GetWowRectangle()
        {
            var Wow = FindWindow("GxWindowClass", "World Of Warcraft");
            var Win32ApiRect = new Rect();
            GetWindowRect(Wow, ref Win32ApiRect);
            var myRect = new Rectangle();
            myRect.X = Win32ApiRect.Left;
            myRect.Y = Win32ApiRect.Top;
            myRect.Width = Win32ApiRect.Right - Win32ApiRect.Left;
            myRect.Height = Win32ApiRect.Bottom - Win32ApiRect.Top;
            return myRect;
        }

        public static void MoveMouse(int x, int y)
        {
            if (SetCursorPos(x, y))
            {
                LastX = x;
                LastY = y;
            }
        }

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        public static void SendKey(string sKeys)
        {
            if (sKeys != " ")
            {
                if (Settings.Default.UseAltKey) sKeys = "%(" + sKeys + ")"; // %(X) : Use the alt key
                else sKeys = "{" + sKeys + "}"; // {X} : Avoid UTF-8 errors (é, è, ...)
            }

            SendKeys.Send(sKeys);
        }

        public static bool SendKeyboardAction(Keys key, keyState state)
        {
            return SendKeyboardAction((byte)key.GetHashCode(), state);
        }

        public static bool SendKeyboardAction(byte key, keyState state)
        {
            return keybd_event(key, 0, (uint)state, (UIntPtr)0);
        }

        public static void SendMouseClick()
        {
            var Wow = FindWindow("GxWindowClass", "World Of Warcraft");
            var dWord = MakeDWord(LastX - LastRectX, LastY - LastRectY);

            if (Settings.Default.ShiftLoot) SendKeyboardAction(16, keyState.KEYDOWN);

            SendNotifyMessage(Wow, WM_RBUTTONDOWN, (UIntPtr)1, (IntPtr)dWord);
            Thread.Sleep(100);
            SendNotifyMessage(Wow, WM_RBUTTONUP, (UIntPtr)1, (IntPtr)dWord);

            if (Settings.Default.ShiftLoot) SendKeyboardAction(16, keyState.KEYUP);
        }

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern bool DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool GetCursorInfo(out CursorInfo pci);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        [DllImport("user32.dll")]
        private static extern bool keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private static long MakeDWord(int LoWord, int HiWord)
        {
            return (HiWord << 16) | (LoWord & 0xFFFF);
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SendNotifyMessage(IntPtr hWnd, uint Msg, UIntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential)]
        public struct CursorInfo
        {
            public int cbSize;

            public int flags;

            public IntPtr hCursor;

            public Point ptScreenPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int x;

            public int y;
        }

        private struct Rect
        {
            public int Left;

            public int Top;

            public int Right;

            public int Bottom;
        }
    }
}