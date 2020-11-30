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
            this.buttonCheck = new System.Windows.Forms.Button();
            this.textBoxMessage = new System.Windows.Forms.TextBox();
            this.checkBoxTransparent = new System.Windows.Forms.CheckBox();
            this.checkBoxTopmost = new System.Windows.Forms.CheckBox();
            this.buttonShowMonitorInfo = new System.Windows.Forms.Button();
            this.checkBoxAllowDrop = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // buttonCheck
            // 
            this.buttonCheck.Location = new System.Drawing.Point(22, 11);
            this.buttonCheck.Name = "buttonCheck";
            this.buttonCheck.Size = new System.Drawing.Size(97, 23);
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
            this.textBoxMessage.Location = new System.Drawing.Point(22, 72);
            this.textBoxMessage.Multiline = true;
            this.textBoxMessage.Name = "textBoxMessage";
            this.textBoxMessage.Size = new System.Drawing.Size(300, 114);
            this.textBoxMessage.TabIndex = 1;
            // 
            // checkBoxTransparent
            // 
            this.checkBoxTransparent.AutoSize = true;
            this.checkBoxTransparent.Location = new System.Drawing.Point(156, 16);
            this.checkBoxTransparent.Name = "checkBoxTransparent";
            this.checkBoxTransparent.Size = new System.Drawing.Size(85, 16);
            this.checkBoxTransparent.TabIndex = 2;
            this.checkBoxTransparent.Text = "Transparent";
            this.checkBoxTransparent.UseVisualStyleBackColor = true;
            this.checkBoxTransparent.CheckedChanged += new System.EventHandler(this.checkBoxTransparent_CheckedChanged);
            // 
            // checkBoxTopmost
            // 
            this.checkBoxTopmost.AutoSize = true;
            this.checkBoxTopmost.Location = new System.Drawing.Point(247, 16);
            this.checkBoxTopmost.Name = "checkBoxTopmost";
            this.checkBoxTopmost.Size = new System.Drawing.Size(68, 16);
            this.checkBoxTopmost.TabIndex = 2;
            this.checkBoxTopmost.Text = "Topmost";
            this.checkBoxTopmost.UseVisualStyleBackColor = true;
            this.checkBoxTopmost.CheckedChanged += new System.EventHandler(this.checkBoxTopmost_CheckedChanged);
            // 
            // buttonShowMonitorInfo
            // 
            this.buttonShowMonitorInfo.Location = new System.Drawing.Point(22, 44);
            this.buttonShowMonitorInfo.Name = "buttonShowMonitorInfo";
            this.buttonShowMonitorInfo.Size = new System.Drawing.Size(97, 23);
            this.buttonShowMonitorInfo.TabIndex = 0;
            this.buttonShowMonitorInfo.Text = "Monitor Info.";
            this.buttonShowMonitorInfo.UseVisualStyleBackColor = true;
            this.buttonShowMonitorInfo.Click += new System.EventHandler(this.buttonShowMonitorInfo_Click);
            // 
            // checkBoxAllowDrop
            // 
            this.checkBoxAllowDrop.AutoSize = true;
            this.checkBoxAllowDrop.Location = new System.Drawing.Point(156, 48);
            this.checkBoxAllowDrop.Name = "checkBoxAllowDrop";
            this.checkBoxAllowDrop.Size = new System.Drawing.Size(104, 16);
            this.checkBoxAllowDrop.TabIndex = 2;
            this.checkBoxAllowDrop.Text = "Allow drop files";
            this.checkBoxAllowDrop.UseVisualStyleBackColor = true;
            this.checkBoxAllowDrop.CheckedChanged += new System.EventHandler(this.checkBoxAllowDrop_CheckedChanged);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(343, 198);
            this.Controls.Add(this.checkBoxTopmost);
            this.Controls.Add(this.checkBoxAllowDrop);
            this.Controls.Add(this.checkBoxTransparent);
            this.Controls.Add(this.textBoxMessage);
            this.Controls.Add(this.buttonShowMonitorInfo);
            this.Controls.Add(this.buttonCheck);
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
    }
}

