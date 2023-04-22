using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLibUniWinC
{
    /// <summary>
    /// 個々のウィンドウ情報
    /// </summary>
    internal class WindowInfo
    {
        public IntPtr Handle;
        public string Text;
        public string ProcessName;
        public bool IsChild;

        public WindowInfo(IntPtr hWnd, Process process, string title, bool isChild = false)
        {
            this.ProcessName = process.ProcessName;
            this.Handle = hWnd;
            this.Text = title;
            this.IsChild = isChild;
        }

        override public string ToString()
        {
            if (string.IsNullOrEmpty(Text)) return $"{Handle.ToString("X8")} {ProcessName}";
            else return $"{Handle.ToString("X8")} {ProcessName}-{Text}";
        }
    }
}
