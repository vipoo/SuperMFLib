namespace MFCaptureAlt
{
    partial class WinMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.picbCapture = new System.Windows.Forms.PictureBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.smnuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.selectCaptureDeviceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.picbCapture)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // picbCapture
            // 
            this.picbCapture.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picbCapture.Location = new System.Drawing.Point(0, 27);
            this.picbCapture.Name = "picbCapture";
            this.picbCapture.Size = new System.Drawing.Size(611, 390);
            this.picbCapture.TabIndex = 0;
            this.picbCapture.TabStop = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.smnuFile});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(611, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // smnuFile
            // 
            this.smnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectCaptureDeviceToolStripMenuItem});
            this.smnuFile.Name = "smnuFile";
            this.smnuFile.Size = new System.Drawing.Size(37, 20);
            this.smnuFile.Text = "File";
            // 
            // selectCaptureDeviceToolStripMenuItem
            // 
            this.selectCaptureDeviceToolStripMenuItem.Name = "selectCaptureDeviceToolStripMenuItem";
            this.selectCaptureDeviceToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.selectCaptureDeviceToolStripMenuItem.Text = "Select Capture Device ...";
            this.selectCaptureDeviceToolStripMenuItem.Click += new System.EventHandler(this.selectCaptureDeviceToolStripMenuItem_Click);
            // 
            // WinMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(611, 414);
            this.Controls.Add(this.picbCapture);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "WinMain";
            this.Text = "MFCaptureAlt";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.WinMain_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.picbCapture)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picbCapture;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem smnuFile;
        private System.Windows.Forms.ToolStripMenuItem selectCaptureDeviceToolStripMenuItem;
    }
}