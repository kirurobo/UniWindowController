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
            this.comboBoxFitMonitor = new System.Windows.Forms.ComboBox();
            this.buttonFitMonitor = new System.Windows.Forms.Button();
            this.timerMainLoop = new System.Windows.Forms.Timer(this.components);
            this.trackBarAlpha = new System.Windows.Forms.TrackBar();
            this.groupBoxWindowSettings = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxBottommost = new System.Windows.Forms.CheckBox();
            this.comboBoxTransparentType = new System.Windows.Forms.ComboBox();
            this.checkBoxTopmost = new System.Windows.Forms.CheckBox();
            this.checkBoxTransparent = new System.Windows.Forms.CheckBox();
            this.groupBoxInformation = new System.Windows.Forms.GroupBox();
            this.buttonShowMonitorInfo = new System.Windows.Forms.Button();
            this.buttonCheck = new System.Windows.Forms.Button();
            this.textBoxMessage = new System.Windows.Forms.TextBox();
            this.groupBoxFileHandling = new System.Windows.Forms.GroupBox();
            this.checkBoxAllowDrop = new System.Windows.Forms.CheckBox();
            this.buttonSaveFile = new System.Windows.Forms.Button();
            this.buttonOpenFile = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarAlpha)).BeginInit();
            this.groupBoxWindowSettings.SuspendLayout();
            this.groupBoxInformation.SuspendLayout();
            this.groupBoxFileHandling.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBoxFitMonitor
            // 
            this.comboBoxFitMonitor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFitMonitor.FormattingEnabled = true;
            this.comboBoxFitMonitor.Location = new System.Drawing.Point(24, 144);
            this.comboBoxFitMonitor.Name = "comboBoxFitMonitor";
            this.comboBoxFitMonitor.Size = new System.Drawing.Size(121, 23);
            this.comboBoxFitMonitor.TabIndex = 3;
            // 
            // buttonFitMonitor
            // 
            this.buttonFitMonitor.Location = new System.Drawing.Point(161, 140);
            this.buttonFitMonitor.Margin = new System.Windows.Forms.Padding(4);
            this.buttonFitMonitor.Name = "buttonFitMonitor";
            this.buttonFitMonitor.Size = new System.Drawing.Size(67, 29);
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
            // trackBarAlpha
            // 
            this.trackBarAlpha.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarAlpha.LargeChange = 16;
            this.trackBarAlpha.Location = new System.Drawing.Point(425, 21);
            this.trackBarAlpha.Maximum = 255;
            this.trackBarAlpha.Name = "trackBarAlpha";
            this.trackBarAlpha.Size = new System.Drawing.Size(289, 56);
            this.trackBarAlpha.SmallChange = 8;
            this.trackBarAlpha.TabIndex = 4;
            this.trackBarAlpha.TickFrequency = 16;
            this.trackBarAlpha.Value = 255;
            this.trackBarAlpha.Scroll += new System.EventHandler(this.trackBarAlpha_Scroll);
            // 
            // groupBoxWindowSettings
            // 
            this.groupBoxWindowSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxWindowSettings.BackColor = System.Drawing.SystemColors.Control;
            this.groupBoxWindowSettings.Controls.Add(this.label1);
            this.groupBoxWindowSettings.Controls.Add(this.checkBoxBottommost);
            this.groupBoxWindowSettings.Controls.Add(this.comboBoxTransparentType);
            this.groupBoxWindowSettings.Controls.Add(this.buttonFitMonitor);
            this.groupBoxWindowSettings.Controls.Add(this.comboBoxFitMonitor);
            this.groupBoxWindowSettings.Controls.Add(this.trackBarAlpha);
            this.groupBoxWindowSettings.Controls.Add(this.checkBoxTopmost);
            this.groupBoxWindowSettings.Controls.Add(this.checkBoxTransparent);
            this.groupBoxWindowSettings.Location = new System.Drawing.Point(12, 12);
            this.groupBoxWindowSettings.Name = "groupBoxWindowSettings";
            this.groupBoxWindowSettings.Size = new System.Drawing.Size(731, 173);
            this.groupBoxWindowSettings.TabIndex = 5;
            this.groupBoxWindowSettings.TabStop = false;
            this.groupBoxWindowSettings.Text = "Window settings";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(368, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 15);
            this.label1.TabIndex = 7;
            this.label1.Text = "Alpha";
            // 
            // checkBoxBottommost
            // 
            this.checkBoxBottommost.AutoSize = true;
            this.checkBoxBottommost.Location = new System.Drawing.Point(14, 101);
            this.checkBoxBottommost.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxBottommost.Name = "checkBoxBottommost";
            this.checkBoxBottommost.Size = new System.Drawing.Size(107, 19);
            this.checkBoxBottommost.TabIndex = 3;
            this.checkBoxBottommost.Text = "Bottommost";
            this.checkBoxBottommost.UseVisualStyleBackColor = true;
            this.checkBoxBottommost.CheckedChanged += new System.EventHandler(this.checkBoxBottommost_CheckedChanged);
            // 
            // comboBoxTransparentType
            // 
            this.comboBoxTransparentType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTransparentType.FormattingEnabled = true;
            this.comboBoxTransparentType.Location = new System.Drawing.Point(144, 35);
            this.comboBoxTransparentType.Name = "comboBoxTransparentType";
            this.comboBoxTransparentType.Size = new System.Drawing.Size(121, 23);
            this.comboBoxTransparentType.TabIndex = 3;
            this.comboBoxTransparentType.SelectedIndexChanged += new System.EventHandler(this.comboBoxTransparentType_SelectedIndexChanged);
            // 
            // checkBoxTopmost
            // 
            this.checkBoxTopmost.AutoSize = true;
            this.checkBoxTopmost.Location = new System.Drawing.Point(16, 74);
            this.checkBoxTopmost.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxTopmost.Name = "checkBoxTopmost";
            this.checkBoxTopmost.Size = new System.Drawing.Size(84, 19);
            this.checkBoxTopmost.TabIndex = 4;
            this.checkBoxTopmost.Text = "Topmost";
            this.checkBoxTopmost.UseVisualStyleBackColor = true;
            this.checkBoxTopmost.CheckedChanged += new System.EventHandler(this.checkBoxTopmost_CheckedChanged);
            // 
            // checkBoxTransparent
            // 
            this.checkBoxTransparent.AutoSize = true;
            this.checkBoxTransparent.Location = new System.Drawing.Point(16, 37);
            this.checkBoxTransparent.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxTransparent.Name = "checkBoxTransparent";
            this.checkBoxTransparent.Size = new System.Drawing.Size(105, 19);
            this.checkBoxTransparent.TabIndex = 6;
            this.checkBoxTransparent.Text = "Transparent";
            this.checkBoxTransparent.UseVisualStyleBackColor = true;
            this.checkBoxTransparent.CheckedChanged += new System.EventHandler(this.checkBoxTransparent_CheckedChanged);
            // 
            // groupBoxInformation
            // 
            this.groupBoxInformation.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxInformation.BackColor = System.Drawing.SystemColors.Control;
            this.groupBoxInformation.Controls.Add(this.buttonShowMonitorInfo);
            this.groupBoxInformation.Controls.Add(this.buttonCheck);
            this.groupBoxInformation.Controls.Add(this.textBoxMessage);
            this.groupBoxInformation.Location = new System.Drawing.Point(12, 306);
            this.groupBoxInformation.Name = "groupBoxInformation";
            this.groupBoxInformation.Size = new System.Drawing.Size(731, 364);
            this.groupBoxInformation.TabIndex = 6;
            this.groupBoxInformation.TabStop = false;
            this.groupBoxInformation.Text = "Information";
            // 
            // buttonShowMonitorInfo
            // 
            this.buttonShowMonitorInfo.Location = new System.Drawing.Point(144, 22);
            this.buttonShowMonitorInfo.Margin = new System.Windows.Forms.Padding(4);
            this.buttonShowMonitorInfo.Name = "buttonShowMonitorInfo";
            this.buttonShowMonitorInfo.Size = new System.Drawing.Size(129, 29);
            this.buttonShowMonitorInfo.TabIndex = 3;
            this.buttonShowMonitorInfo.Text = "Monitor Info.";
            this.buttonShowMonitorInfo.UseVisualStyleBackColor = true;
            this.buttonShowMonitorInfo.Click += new System.EventHandler(this.buttonShowMonitorInfo_Click);
            // 
            // buttonCheck
            // 
            this.buttonCheck.Location = new System.Drawing.Point(7, 22);
            this.buttonCheck.Margin = new System.Windows.Forms.Padding(4);
            this.buttonCheck.Name = "buttonCheck";
            this.buttonCheck.Size = new System.Drawing.Size(129, 29);
            this.buttonCheck.TabIndex = 4;
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
            this.textBoxMessage.Location = new System.Drawing.Point(7, 59);
            this.textBoxMessage.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxMessage.Multiline = true;
            this.textBoxMessage.Name = "textBoxMessage";
            this.textBoxMessage.Size = new System.Drawing.Size(717, 298);
            this.textBoxMessage.TabIndex = 2;
            // 
            // groupBoxFileHandling
            // 
            this.groupBoxFileHandling.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFileHandling.BackColor = System.Drawing.SystemColors.Control;
            this.groupBoxFileHandling.Controls.Add(this.checkBoxAllowDrop);
            this.groupBoxFileHandling.Controls.Add(this.buttonSaveFile);
            this.groupBoxFileHandling.Controls.Add(this.buttonOpenFile);
            this.groupBoxFileHandling.Location = new System.Drawing.Point(12, 205);
            this.groupBoxFileHandling.Name = "groupBoxFileHandling";
            this.groupBoxFileHandling.Size = new System.Drawing.Size(731, 68);
            this.groupBoxFileHandling.TabIndex = 5;
            this.groupBoxFileHandling.TabStop = false;
            this.groupBoxFileHandling.Text = "File handling";
            // 
            // checkBoxAllowDrop
            // 
            this.checkBoxAllowDrop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxAllowDrop.AutoSize = true;
            this.checkBoxAllowDrop.Location = new System.Drawing.Point(574, 29);
            this.checkBoxAllowDrop.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxAllowDrop.Name = "checkBoxAllowDrop";
            this.checkBoxAllowDrop.Size = new System.Drawing.Size(125, 19);
            this.checkBoxAllowDrop.TabIndex = 8;
            this.checkBoxAllowDrop.Text = "Allow drop files";
            this.checkBoxAllowDrop.UseVisualStyleBackColor = true;
            this.checkBoxAllowDrop.Click += new System.EventHandler(this.checkBoxAllowDrop_CheckedChanged);
            // 
            // buttonSaveFile
            // 
            this.buttonSaveFile.Location = new System.Drawing.Point(153, 23);
            this.buttonSaveFile.Margin = new System.Windows.Forms.Padding(4);
            this.buttonSaveFile.Name = "buttonSaveFile";
            this.buttonSaveFile.Size = new System.Drawing.Size(129, 29);
            this.buttonSaveFile.TabIndex = 6;
            this.buttonSaveFile.Text = "Save-panel";
            this.buttonSaveFile.UseVisualStyleBackColor = true;
            this.buttonSaveFile.Click += new System.EventHandler(this.buttonSaveFile_Click);
            // 
            // buttonOpenFile
            // 
            this.buttonOpenFile.Location = new System.Drawing.Point(16, 23);
            this.buttonOpenFile.Margin = new System.Windows.Forms.Padding(4);
            this.buttonOpenFile.Name = "buttonOpenFile";
            this.buttonOpenFile.Size = new System.Drawing.Size(129, 29);
            this.buttonOpenFile.TabIndex = 7;
            this.buttonOpenFile.Text = "Open-panel";
            this.buttonOpenFile.UseVisualStyleBackColor = true;
            this.buttonOpenFile.Click += new System.EventHandler(this.buttonOpenFile_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(755, 682);
            this.Controls.Add(this.groupBoxFileHandling);
            this.Controls.Add(this.groupBoxInformation);
            this.Controls.Add(this.groupBoxWindowSettings);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormMain";
            this.Text = "TestLibUniWinC";
            this.Shown += new System.EventHandler(this.FormMain_Shown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FormMain_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FormMain_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FormMain_MouseUp);
            this.Resize += new System.EventHandler(this.FormMain_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarAlpha)).EndInit();
            this.groupBoxWindowSettings.ResumeLayout(false);
            this.groupBoxWindowSettings.PerformLayout();
            this.groupBoxInformation.ResumeLayout(false);
            this.groupBoxInformation.PerformLayout();
            this.groupBoxFileHandling.ResumeLayout(false);
            this.groupBoxFileHandling.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ComboBox comboBoxFitMonitor;
        private System.Windows.Forms.Button buttonFitMonitor;
        private System.Windows.Forms.Timer timerMainLoop;
        private System.Windows.Forms.TrackBar trackBarAlpha;
        private System.Windows.Forms.GroupBox groupBoxWindowSettings;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxBottommost;
        private System.Windows.Forms.ComboBox comboBoxTransparentType;
        private System.Windows.Forms.CheckBox checkBoxTopmost;
        private System.Windows.Forms.CheckBox checkBoxTransparent;
        private System.Windows.Forms.GroupBox groupBoxInformation;
        private System.Windows.Forms.Button buttonShowMonitorInfo;
        private System.Windows.Forms.Button buttonCheck;
        private System.Windows.Forms.TextBox textBoxMessage;
        private System.Windows.Forms.GroupBox groupBoxFileHandling;
        private System.Windows.Forms.CheckBox checkBoxAllowDrop;
        private System.Windows.Forms.Button buttonSaveFile;
        private System.Windows.Forms.Button buttonOpenFile;
    }
}

