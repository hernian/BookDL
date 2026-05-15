using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace BookDL.Infrastructure
{
    public interface IWinApi
    {
        void SetWindowOwner(IntPtr hWndTarget, IntPtr hWndOwner);
        IntPtr FindWindowByTitleContains(string keyword);
        Process GetWindowProcess(IntPtr hWnd);
        void SetForeground(IntPtr hWnd);
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

        
    }
}
