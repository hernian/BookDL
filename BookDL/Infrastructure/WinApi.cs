using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace BookDL.Infrastructure
{
    public enum ZOrderOption
    {
        Unchanged,
        Top,
        Bottom,
        TopMost,
        NoTopMost
    }

    public enum WindowVisibility
    {
        Unchanged,
        Show,
        Hide
    }

    public record struct SetWindowPosParam(
        bool ChangeSize = false,
        bool ChangePosition = false,
        ZOrderOption ZOrder = ZOrderOption.Unchanged,
        bool Activate = false,
        WindowVisibility Visibility = WindowVisibility.Unchanged,
        double Left = 0,
        double Top = 0,
        double Width = 0,
        double Height = 0);


    public interface IWinApi
    {
        void SetWindowOwner(IntPtr hWndTarget, IntPtr hWndOwner);
        IntPtr FindWindowByTitleContains(string keyword);
        Process GetWindowProcess(IntPtr hWnd);
        void SetForeground(IntPtr hWnd);

        Rect GetWorkingArea(IntPtr hWnd);
        void SetWindowPos(IntPtr hWnd, SetWindowPosParam swpp);
    }
    public partial class WinApi : IWinApi
    {
        private const int GWLP_HWNDPARENT = -8;
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        public void SetWindowOwner(IntPtr hWndTarget, IntPtr hWndOwner)
        {
            SetWindowLongPtr(hWndTarget, GWLP_HWNDPARENT, hWndOwner);
            SetWindowPos(hWndTarget, HWND_TOP, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }

        public IntPtr FindWindowByTitleContains(string keyword)
        {
            IntPtr found = IntPtr.Zero;
            var sb = new System.Text.StringBuilder(256);

            EnumWindows((hWnd, _) =>
            {
                if (!IsWindowVisible(hWnd)) return true;

                GetWindowText(hWnd, sb, sb.Capacity);
                var title = sb.ToString();
                Debug.WriteLine($"WindowTitle: {title}");
                if (title.Contains(keyword))
                {
                    found = hWnd;
                    return false; // 列挙停止
                }
                return true;
            }, IntPtr.Zero);

            return found;
        }

        public Process GetWindowProcess(IntPtr hWnd)
        {
            GetWindowThreadProcessId(hWnd, out uint pid);
            var process = Process.GetProcessById((int)pid);
            return process;
        }

        public void SetForeground(IntPtr hWnd)
        {
            SetForegroundWindow(hWnd);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);

        // hWndInsertAfter 用の定数
        public static readonly IntPtr HWND_TOP = new IntPtr(0);
        public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        // uFlags
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_NOREDRAW = 0x0008;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_FRAMECHANGED = 0x0020;
        public const uint SWP_SHOWWINDOW = 0x0040;
        public const uint SWP_HIDEWINDOW = 0x0080;
        public const uint SWP_NOCOPYBITS = 0x0100;
        public const uint SWP_NOOWNERZORDER = 0x0200;
        public const uint SWP_NOSENDCHANGING = 0x0400;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private const int MONITOR_DEFAULTTONEAREST = 2;

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, int dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public int dwFlags;
        }

        public Rect GetWorkingArea(IntPtr hWnd)
        {
            IntPtr hMonitor = MonitorFromWindow(hWnd, MONITOR_DEFAULTTONEAREST);

            MONITORINFO mi = new MONITORINFO();
            mi.cbSize = Marshal.SizeOf(typeof(MONITORINFO));

            if (!GetMonitorInfo(hMonitor, ref mi))
                throw new System.ComponentModel.Win32Exception();

            return new Rect(
                mi.rcWork.Left,
                mi.rcWork.Top,
                mi.rcWork.Right - mi.rcWork.Left,
                mi.rcWork.Bottom - mi.rcWork.Top
            );
        }

        public void SetWindowPos(IntPtr hWnd, SetWindowPosParam p)
        {
            // hWndInsertAfter の決定
            IntPtr insertAfter = p.ZOrder switch
            {
                ZOrderOption.Top => HWND_TOP,
                ZOrderOption.Bottom => HWND_BOTTOM,
                ZOrderOption.TopMost => HWND_TOPMOST,
                ZOrderOption.NoTopMost => HWND_NOTOPMOST,
                _ => IntPtr.Zero // Unchanged
            };

            // フラグ生成
            uint flags = 0;

            if (!p.ChangeSize)
                flags |= SWP_NOSIZE;

            if (!p.ChangePosition)
                flags |= SWP_NOMOVE;

            if (p.ZOrder == ZOrderOption.Unchanged)
                flags |= SWP_NOZORDER;

            if (!p.Activate)
                flags |= SWP_NOACTIVATE;

            flags |= p.Visibility switch
            {
                WindowVisibility.Show => SWP_SHOWWINDOW,
                WindowVisibility.Hide => SWP_HIDEWINDOW,
                _ => 0
            };

            // double → int（再現性のため Math.Round）
            int x = (int)Math.Round(p.Left);
            int y = (int)Math.Round(p.Top);
            int w = (int)Math.Round(p.Width);
            int h = (int)Math.Round(p.Height);

            SetWindowPos(hWnd, insertAfter, x, y, w, h, flags);
        }
    }
}
