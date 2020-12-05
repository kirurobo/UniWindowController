using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestLibUniWinC
{
    public partial class FormMain : Form
    {
        UniWinC uniwinc;

        public FormMain()
        {
            InitializeComponent();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            uniwinc = new UniWinC();

            // ファイルドロップ時、出力
            UniWinC.RegisterDropFilesCallback(msg => {
                Console.Write(msg);
            });

            // 解像度変更時、モニター数を出力
            UniWinC.RegisterDisplayChangedCallback(count => {
                Console.WriteLine("Monitors: " + count);
            });

            //  モニタ一覧を表示
            PrintMonitorInfo();
        }

        /// <summary>
        /// 現在接続されているモニタを列挙
        /// </summary>
        private void PrintMonitorInfo()
        {
            int monitors = UniWinC.GetMonitorCount();

            int currentMonitorIndex = UniWinC.GetCurrentMonitor();

            string message = "Current monitor: " + currentMonitorIndex + "\r\n";

            for (int i = 0; i < monitors; i++)
            {
                float x, y, w, h;
                bool result = UniWinC.GetMonitorRectangle(i, out x, out y, out w, out h);
                message += String.Format(
                    "Monitor {0}: X:{1}, Y:{2} - W:{3}, H:{4}\r\n",
                    i, x, y, w, h
                    );
            }
            Console.WriteLine(message);
            textBoxMessage.Text = message;
        }

        private void buttonCheck_Click(object sender, EventArgs e)
        {
            var pos = uniwinc.GetWindowPosition();
            var size = uniwinc.GetWindowSize();
            var hwnd = UniWinC.GetWindowHandle();
            var pid = UniWinC.GetMyProcessId();
            var myPid = System.Diagnostics.Process.GetCurrentProcess().Id;
            var clientSize = this.ClientSize;

            string message = String.Format(
                "Pos. {0}, {1}\r\nSize {2}, {3}\r\nClient {4}, {5}\r\nhWnd {6:X} / {7:X}\r\nPID {8} / {9}",
                pos.x, pos.y, size.x, size.y, clientSize.Width, clientSize.Height,
                hwnd.ToInt32(), this.Handle.ToInt32(), pid, myPid);


            Console.WriteLine(message);
            textBoxMessage.Text = message;
        }

        private void checkBoxTransparent_CheckedChanged(object sender, EventArgs e)
        {
            uniwinc.EnableTransparent(checkBoxTransparent.Checked);
        }

        private void checkBoxTopmost_CheckedChanged(object sender, EventArgs e)
        {
            uniwinc.EnableTopmost(checkBoxTopmost.Checked);
        }

        private void FormMain_Resize(object sender, EventArgs e)
        {
            Console.WriteLine("Resized!");
        }

        private void buttonShowMonitorInfo_Click(object sender, EventArgs e)
        {
            PrintMonitorInfo();
        }

        private void checkBoxAllowDrop_CheckedChanged(object sender, EventArgs e)
        {
            UniWinC.SetAllowDrop(checkBoxAllowDrop.Checked);
        }
    }
}
