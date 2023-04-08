using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TestLibUniWinC
{
    /// <summary>
    /// 開かれているウィンドウ一覧を保持するクラス
    /// </summary>
    internal class WindowList
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate bool EnumWindowsDelegate(IntPtr hWnd, long lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumWindows(EnumWindowsDelegate lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumChildWindows(IntPtr hWnd, EnumWindowsDelegate lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetWindowText(IntPtr hWnd, [MarshalAs(UnmanagedType.LPStr)] StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetClassName(IntPtr hWnd, [MarshalAs(UnmanagedType.LPStr)] StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out long lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);


        /// <summary>
        /// 現在開かれているウィンドウ情報を一通り保持する辞書
        /// </summary>
        private List<WindowInfo> windowList = new List<WindowInfo>();


        /// <summary>
        /// ウィンドウクラス一覧を取得
        /// </summary>
        public void Load(bool includeChildren = false)
        {
            windowList.Clear();

            // 重複を無くすため、列挙したhWndを記憶
            List<IntPtr> hWndList = new List<IntPtr>();

            EnumWindows(new EnumWindowsDelegate(delegate (IntPtr hWnd, long lParam)
            {
                if (hWndList.Contains(hWnd)) return true;

                StringBuilder sb = new StringBuilder(1024);

                //// ウィンドウタイトルがないものは除外するなら下記2行の代わりにこの行を使う
                //if (IsWindowVisible(hWnd) != 0 && GetWindowText(hWnd, sb, sb.Capacity) != 0)

                GetWindowText(hWnd, sb, sb.Capacity);
                if (IsWindowVisible(hWnd))
                {
                    GetWindowThreadProcessId(hWnd, out long pid);
                    Process p = Process.GetProcessById((int)pid);

                    windowList.Add(new WindowInfo(hWnd, p, sb.ToString()));
                }

                if (includeChildren)
                {
                    // 子ウィンドウも一覧に含める
                    EnumChildWindows(hWnd, new EnumWindowsDelegate(delegate (IntPtr hWndChild, long lParamChild)
                    {
                        if (hWndList.Contains(hWnd)) return true;

                        StringBuilder sbChild = new StringBuilder(1024);
                        if (IsWindowVisible(hWndChild) &&   GetWindowText(hWndChild, sbChild, sbChild.Capacity) != 0)
                        {
                            GetWindowThreadProcessId(hWndChild, out long pid);
                            Process p = Process.GetProcessById((int)pid);

                            windowList.Add(new WindowInfo(hWndChild, p, sbChild.ToString(), true));
                        }

                        return true;
                    }), IntPtr.Zero);
                }

                return true;
            }), IntPtr.Zero);
        }

        public WindowInfo[] GetArray()
        {
            return windowList.ToArray();
        }
    }
}
