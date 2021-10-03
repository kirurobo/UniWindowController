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

        static bool monitorChanged = false;
        static bool filesDropped = false;
        static string droppedFiles = "";

        /// <summary>
        /// 値を変更中で、GUI操作を反映させたくないときtrueとする
        /// </summary>
        bool isAplying = false;


        public FormMain()
        {
            InitializeComponent();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            uniwinc = new UniWinC();

            UniWinC.RegisterWindowStyleChangedCallback(type => {
                Console.WriteLine($"Style changed: {type}");
            });

            // ファイルドロップ時、その内容を出力
            UniWinC.RegisterDropFilesCallback(msg => {
                droppedFiles = msg;
                filesDropped = true;
            });

            // 解像度変更時、モニター一覧を更新
            UniWinC.RegisterMonitorChangedCallback(count => {
                monitorChanged = true;
            });
            UpdateMonitorCombobox();    // 初回の一覧取得

            //  モニタ一覧を表示
            PrintMonitorInfo();

            // 定期的にフラグを監視して処理
            timerMainLoop.Start();
        }

        /// <summary>
        /// モニタ一覧を更新
        /// </summary>
        private void UpdateMonitorCombobox()
        {
            int count = UniWinC.GetMonitorCount();
            int index = comboBoxFitMonitor.SelectedIndex;

            comboBoxFitMonitor.Items.Clear();

            for (int i = 0; i < count; i++)
            {
                comboBoxFitMonitor.Items.Add($"Monitor {i}");
            }

            if (index >= count) index = count - 1;
            if (index < 0) index = 0;
            comboBoxFitMonitor.SelectedIndex = index;
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

        private void PrintWindowInfo()
        {
            var pos = uniwinc.GetWindowPosition();
            var size = uniwinc.GetWindowSize();
            var hwnd = UniWinC.GetWindowHandle();
            var hdesktop = UniWinC.GetDesktopWindowHandle();
            var pid = UniWinC.GetMyProcessId();
            var myPid = System.Diagnostics.Process.GetCurrentProcess().Id;
            var clientSize = this.ClientSize;

            string message = String.Format(
                "Pos. {0}, {1}\r\nSize {2}, {3}\r\nClient {4}, {5}\r\nhWnd {6:X} / {7:X}\r\nPID {8} / {9}\r\nDesktop {10:X}",
                pos.x, pos.y, size.x, size.y, clientSize.Width, clientSize.Height,
                hwnd.ToInt32(), this.Handle.ToInt32(), pid, myPid, hdesktop.ToInt32());


            Console.WriteLine(message);
            textBoxMessage.Text = message;
        }

        private void buttonCheck_Click(object sender, EventArgs e)
        {
            PrintWindowInfo();
        }

        private void buttonOpenFile_Click(object sender, EventArgs e)
        {
            UniWinC.ShowOpenFilePanel(0b0111);
        }

        /// <summary>
        /// 選択されたモニタにウィンドウを移動
        /// </summary>
        private void FitToMonitor(int monitor)
        {
            float x, y, w, h;
            if (UniWinC.GetMonitorRectangle(monitor, out x, out y, out w, out h))
            {
                UniWinC.SetPosition(x, y);
                //UniWinC.SetSize(w / 2, h / 2);
                UniWinC.SetSize(w, h);
            }
        }


        private void checkBoxTransparent_CheckedChanged(object sender, EventArgs e)
        {
            if (isAplying) return;

            isAplying = true;
            uniwinc.EnableTransparent(checkBoxTransparent.Checked);
            isAplying = false;
        }

        private void checkBoxTopmost_CheckedChanged(object sender, EventArgs e)
        {
            if (isAplying) return;

            isAplying = true;
            checkBoxBottommost.Checked = false;
            uniwinc.EnableTopmost(checkBoxTopmost.Checked);
            isAplying = false;
        }

        private void checkBoxBottommost_CheckedChanged(object sender, EventArgs e)
        {
            if (isAplying) return;

            isAplying = true;
            checkBoxTopmost.Checked = false;
            uniwinc.EnableBottommost(checkBoxBottommost.Checked);
            isAplying = false;
        }

        private void checkBoxAllowDrop_CheckedChanged(object sender, EventArgs e)
        {
            if (isAplying) return;

            isAplying = true;
            UniWinC.SetAllowDrop(checkBoxAllowDrop.Checked);
            isAplying = false;
        }

        private void FormMain_Resize(object sender, EventArgs e)
        {
            //Console.WriteLine("Resized!");
        }

        private void buttonShowMonitorInfo_Click(object sender, EventArgs e)
        {
            PrintMonitorInfo();
        }

        private void buttonFitMonitor_Click(object sender, EventArgs e)
        {
            FitToMonitor(comboBoxFitMonitor.SelectedIndex);
            PrintWindowInfo();
        }

        private void timerMainLoop_Tick(object sender, EventArgs e)
        {
            if (monitorChanged)
            {
                // 解像度が変化した後の処理
                UpdateMonitorCombobox();
                monitorChanged = false;
            }

            if (filesDropped)
            {
                // ファイルがドロップされた後の処理
                Console.WriteLine(droppedFiles);
                filesDropped = false;
            }
        }
    }
}
