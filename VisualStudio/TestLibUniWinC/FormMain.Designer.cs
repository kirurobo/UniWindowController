namespace TestLibUniWinC
{
    partial class FormMain
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.buttonCheck = new System.Windows.Forms.Button();
            this.textBoxMessage = new System.Windows.Forms.TextBox();
            this.checkBoxTransparent = new System.Windows.Forms.CheckBox();
            this.checkBoxTopmost = new System.Windows.Forms.CheckBox();
            this.buttonShowMonitorInfo = new System.Windows.Forms.Button();
            this.checkBoxAllowDrop = new System.Windows.Forms.CheckBox();
            this.comboBoxFitMonitor = new System.Windows.Forms.ComboBox();
            this.buttonFitMonitor = new System.Windows.Forms.Button();
            this.timerMainLoop = new System.Windows.Forms.Timer(this.components);
            this.checkBoxBottommost = new System.Windows.Forms.CheckBox();
            this.buttonOpenFile = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonCheck
            // 
            this.buttonCheck.Location = new System.Drawing.Point(36, 17);
            this.buttonCheck.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.buttonCheck.Name = "buttonCheck";
            this.buttonCheck.Size = new System.Drawing.Size(161, 35);
            this.buttonCheck.TabIndex = 0;
            this.buttonCheck.Text = "Window Info.";
            this.buttonCheck.UseVisualStyleBackColor = true;
            this.buttonCheck.Click += new System.EventHandler(this.buttonCheck_Click);
            // 
            // textBoxMessage
            // 
            this.textBoxMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxMessage.BackColor = System.Drawing.Color.Black;
            this.textBoxMessage.ForeColor = System.Drawing.Color.White;
            this.textBoxMessage.Location = new System.Drawing.Point(36, 222);
            this.textBoxMessage.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.textBoxMessage.Multiline = true;
            this.textBoxMessage.Name = "textBoxMessage";
            this.textBoxMessage.Size = new System.Drawing.Size(636, 223);
            this.textBoxMessage.TabIndex = 1;
            // 
            // checkBoxTransparent
            // 
            this.checkBoxTransparent.AutoSize = true;
            this.checkBoxTransparent.Location = new System.Drawing.Point(208, 24);
            this.checkBoxTransparent.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.checkBoxTransparent.Name = "checkBoxTransparent";
            this.checkBoxTransparent.Size = new System.Drawing.Size(125, 22);
            this.checkBoxTransparent.TabIndex = 2;
            this.checkBoxTransparent.Text = "Transparent";
            this.checkBoxTransparent.UseVisualStyleBackColor = true;
            this.checkBoxTransparent.CheckedChanged += new System.EventHandler(this.checkBoxTransparent_CheckedChanged);
            // 
            // checkBoxTopmost
            // 
            this.checkBoxTopmost.AutoSize = true;
            this.checkBoxTopmost.Location = new System.Drawing.Point(208, 68);
            this.checkBoxTopmost.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.checkBoxTopmost.Name = "checkBoxTopmost";
            this.checkBoxTopmost.Size = new System.Drawing.Size(99, 22);
            this.checkBoxTopmost.TabIndex = 2;
            this.checkBoxTopmost.Text = "Topmost";
            this.checkBoxTopmost.UseVisualStyleBackColor = true;
            this.checkBoxTopmost.CheckedChanged += new System.EventHandler(this.checkBoxTopmost_CheckedChanged);
            // 
            // buttonShowMonitorInfo
            // 
            this.buttonShowMonitorInfo.Location = new System.Drawing.Point(36, 110);
            this.buttonShowMonitorInfo.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.buttonShowMonitorInfo.Name = "buttonShowMonitorInfo";
            this.buttonShowMonitorInfo.Size = new System.Drawing.Size(161, 35);
            this.buttonShowMonitorInfo.TabIndex = 0;
            this.buttonShowMonitorInfo.Text = "Monitor Info.";
            this.buttonShowMonitorInfo.UseVisualStyleBackColor = true;
            this.buttonShowMonitorInfo.Click += new System.EventHandler(this.buttonShowMonitorInfo_Click);
            // 
            // checkBoxAllowDrop
            // 
            this.checkBoxAllowDrop.AutoSize = true;
            this.checkBoxAllowDrop.Location = new System.Drawing.Point(359, 24);
            this.checkBoxAllowDrop.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.checkBoxAllowDrop.Name = "checkBoxAllowDrop";
            this.checkBoxAllowDrop.Size = new System.Drawing.Size(147, 22);
            this.checkBoxAllowDrop.TabIndex = 2;
            this.checkBoxAllowDrop.Text = "Allow drop files";
            this.checkBoxAllowDrop.UseVisualStyleBackColor = true;
            this.checkBoxAllowDrop.CheckedChanged += new System.EventHandler(this.checkBoxAllowDrop_CheckedChanged);
            // 
            // comboBoxFitMonitor
            // 
            this.comboBoxFitMonitor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxFitMonitor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFitMonitor.FormattingEnabled = true;
            this.comboBoxFitMonitor.Location = new System.Drawing.Point(430, 110);
            this.comboBoxFitMonitor.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBoxFitMonitor.Name = "comboBoxFitMonitor";
            this.comboBoxFitMonitor.Size = new System.Drawing.Size(150, 26);
            this.comboBoxFitMonitor.TabIndex = 3;
            // 
            // buttonFitMonitor
            // 
            this.buttonFitMonitor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFitMonitor.Location = new System.Drawing.Point(590, 106);
            this.buttonFitMonitor.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.buttonFitMonitor.Name = "buttonFitMonitor";
            this.buttonFitMonitor.Size = new System.Drawing.Size(84, 35);
            this.buttonFitMonitor.TabIndex = 0;
            this.buttonFitMonitor.Text = "Fit";
            this.buttonFitMonitor.UseVisualStyleBackColor = true;
            this.buttonFitMonitor.Click += new System.EventHandler(this.buttonFitMonitor_Click);
            // 
            // timerMainLoop
            // 
            this.timerMainLoop.Interval = 50;
            this.timerMainLoop.Tick += new System.EventHandler(this.timerMainLoop_Tick);
            // 
            // checkBoxBottommost
            // 
            this.checkBoxBottommost.AutoSize = true;
            this.checkBoxBottommost.Location = new System.Drawing.Point(359, 68);
            this.checkBoxBottommost.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.checkBoxBottommost.Name = "checkBoxBottommost";
            this.checkBoxBottommost.Size = new System.Drawing.Size(124, 22);
            this.checkBoxBottommost.TabIndex = 2;
            this.checkBoxBottommost.Text = "Bottommost";
            this.checkBoxBottommost.UseVisualStyleBackColor = true;
            this.checkBoxBottommost.CheckedChanged += new System.EventHandler(this.checkBoxBottommost_CheckedChanged);
            // 
            // buttonOpenFile
            // 
            this.buttonOpenFile.Location = new System.Drawing.Point(36, 177);
            this.buttonOpenFile.Margin = new System.Windows.Forms.Padding(5);
            this.buttonOpenFile.Name = "buttonOpenFile";
            this.buttonOpenFile.Size = new System.Drawing.Size(100, 35);
            this.buttonOpenFile.TabIndex = 0;
            this.buttonOpenFile.Text = "Open";
            this.buttonOpenFile.UseVisualStyleBackColor = true;
            this.buttonOpenFile.Click += new System.EventHandler(this.buttonOpenFile_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(146, 177);
            this.buttonClose.Margin = new System.Windows.Forms.Padding(5);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(100, 35);
            this.buttonClose.TabIndex = 0;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonOpenFile_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(710, 465);
            this.Controls.Add(this.comboBoxFitMonitor);
            this.Controls.Add(this.checkBoxBottommost);
            this.Controls.Add(this.checkBoxTopmost);
            this.Controls.Add(this.checkBoxAllowDrop);
            this.Controls.Add(this.checkBoxTransparent);
            this.Controls.Add(this.textBoxMessage);
            this.Controls.Add(this.buttonFitMonitor);
            this.Controls.Add(this.buttonShowMonitorInfo);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonOpenFile);
            this.Controls.Add(this.buttonCheck);
            this.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.Name = "FormMain";
            this.Text = "Form1";
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.Resize += new System.EventHandler(this.FormMain_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCheck;
        private System.Windows.Forms.TextBox textBoxMessage;
        private System.Windows.Forms.CheckBox checkBoxTransparent;
        private System.Windows.Forms.CheckBox checkBoxTopmost;
        private System.Windows.Forms.Button buttonShowMonitorInfo;
        private System.Windows.Forms.CheckBox checkBoxAllowDrop;
        private System.Windows.Forms.ComboBox comboBoxFitMonitor;
        private System.Windows.Forms.Button buttonFitMonitor;
        private System.Windows.Forms.Timer timerMainLoop;
        private System.Windows.Forms.CheckBox checkBoxBottommost;
        private System.Windows.Forms.Button buttonOpenFile;
        private System.Windows.Forms.Button buttonClose;
    }
}

