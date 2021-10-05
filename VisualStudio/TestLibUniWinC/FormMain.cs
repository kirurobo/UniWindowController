using System;
using System.Windows.Forms;
using UnityEngine;
using Kirurobo;

namespace TestLibUniWinC
{
    public partial class FormMain : Form
    {
        UniWinCore uniwinc;


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
            uniwinc = new UniWinCore();
            uniwinc.AttachMyWindow();

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
            int count = uniwinc.GetMonitorCount();
            int index = comboBoxFitMonitor.SelectedIndex;

            comboBoxFitMonitor.Items.Clear();

            for (int i = 0; i < count; i++)
            {
                comboBoxFitMonitor.Items.Add($"Monitor {i}");
            }

            if (index >= count) index = count - 1;
            if (index < 0)
            {
                index = 0;
            } else
            {
                comboBoxFitMonitor.SelectedIndex = index;
            }
        }

        /// <summary>
        /// 現在接続されているモニタを列挙
        /// </summary>
        private void PrintMonitorInfo()
        {
            int monitors = uniwinc.GetMonitorCount();

            int currentMonitorIndex = uniwinc.GetCurrentMonitor();

            string message = "Current monitor: " + currentMonitorIndex + "\r\n";

            for (int i = 0; i < monitors; i++)
            {
                Vector2 pos, size;
                bool result = uniwinc.GetMonitorRectangle(i, out pos, out size);

                message += String.Format(
                    "Monitor {0}: X:{1}, Y:{2} - W:{3}, H:{4}\r\n",
                    i, pos.x, pos.y, size.x, size.y
                    );
            }
            Console.WriteLine(message);
            textBoxMessage.Text = message;
        }

        private void PrintWindowInfo()
        {
            var pos = uniwinc.GetWindowPosition();
            var size = uniwinc.GetWindowSize();
            //var hwnd = UniWinC.GetWindowHandle();
            //var hdesktop = UniWinC.GetDesktopWindowHandle();
            //var pid = UniWinC.GetMyProcessId();
            var myPid = System.Diagnostics.Process.GetCurrentProcess().Id;
            var clientSize = this.ClientSize;

            string message = String.Format(
                "Pos. {0}, {1}\r\nSize {2}, {3}\r\nClient {4}, {5}\r\nhWnd {6:X}\r\nPID {9}\r\n",
                pos.x, pos.y, size.x, size.y, clientSize.Width, clientSize.Height,
                "", this.Handle.ToInt32(),
                "", myPid
                );


            Console.WriteLine(message);
            textBoxMessage.Text = message;
        }

        private void DumpStringArray(string[] array)
        {
            string text = String.Join(Environment.NewLine, array);

            Console.WriteLine(text);
            textBoxMessage.Text = text;
        }

        private void buttonCheck_Click(object sender, EventArgs e)
        {
            PrintWindowInfo();
        }

        private void buttonOpenFile_Click(object sender, EventArgs e)
        {
            Kirurobo.FilePanel.Settings ds = new Kirurobo.FilePanel.Settings();
            ds.title = "Open files";
            Kirurobo.FilePanel.OpenFilePanel(ds, (files)=> { DumpStringArray(files); });
        }

        private void buttonSaveFile_Click(object sender, EventArgs e)
        {
            Kirurobo.FilePanel.Settings ds = new Kirurobo.FilePanel.Settings();
            ds.title = "Save file (Actually not be written)";
            Kirurobo.FilePanel.SaveFilePanel(ds, (files) => { DumpStringArray(files); });
        }

        /// <summary>
        /// 選択されたモニタにウィンドウを移動
        /// </summary>
        private void FitToMonitor(int monitor)
        {
            uniwinc.FitToMonitor(monitor);
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
            uniwinc.SetAllowDrop(checkBoxAllowDrop.Checked);
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

        private void PerformEvent()
        {
            string[] files;

            // ウインドウスタイルの変化
            if (uniwinc.ObserveWindowStyleChanged(out var type))
            {
                Console.WriteLine($"Style changed: {type}");
            }

            // ファイルドロップ時、その内容を出力
            if (uniwinc.ObserveDroppedFiles(out files))
            {
                // ファイルがドロップされた後の処理
                string text = String.Join(Environment.NewLine, files);
                Console.WriteLine("Drop");
                Console.WriteLine(text);
                textBoxMessage.Text = text;
            }

            // 解像度変更時、モニター一覧を更新
            if (uniwinc.ObserveMonitorChanged()) {
                // 解像度が変化した後の処理
                UpdateMonitorCombobox();
            }
        }

        // UnityのUpdateの代わりに定期的に実行するメソッド
        private void timerMainLoop_Tick(object sender, EventArgs e)
        {
            uniwinc.Update();
            
            PerformEvent();
        }
    }
}
