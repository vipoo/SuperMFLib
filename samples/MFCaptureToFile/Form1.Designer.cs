namespace MFCaptureToFile
{
    partial class Form1
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cbDeviceList = new System.Windows.Forms.ComboBox();
            this.tbOutputFile = new System.Windows.Forms.TextBox();
            this.rbMP4 = new System.Windows.Forms.RadioButton();
            this.rbWMV = new System.Windows.Forms.RadioButton();
            this.bnCapture = new System.Windows.Forms.Button();
            this.gbOptions = new System.Windows.Forms.GroupBox();
            this.gbOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(64, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Device";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(42, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Capture File";
            // 
            // cbDeviceList
            // 
            this.cbDeviceList.FormattingEnabled = true;
            this.cbDeviceList.Location = new System.Drawing.Point(125, 20);
            this.cbDeviceList.Name = "cbDeviceList";
            this.cbDeviceList.Size = new System.Drawing.Size(287, 21);
            this.cbDeviceList.TabIndex = 2;
            // 
            // tbOutputFile
            // 
            this.tbOutputFile.Location = new System.Drawing.Point(125, 59);
            this.tbOutputFile.Name = "tbOutputFile";
            this.tbOutputFile.Size = new System.Drawing.Size(287, 20);
            this.tbOutputFile.TabIndex = 3;
            // 
            // rbMP4
            // 
            this.rbMP4.AutoSize = true;
            this.rbMP4.Checked = true;
            this.rbMP4.Location = new System.Drawing.Point(9, 18);
            this.rbMP4.Name = "rbMP4";
            this.rbMP4.Size = new System.Drawing.Size(47, 17);
            this.rbMP4.TabIndex = 4;
            this.rbMP4.TabStop = true;
            this.rbMP4.Text = "MP4";
            this.rbMP4.UseVisualStyleBackColor = true;
            this.rbMP4.CheckedChanged += new System.EventHandler(this.rbMP4_CheckedChanged);
            // 
            // rbWMV
            // 
            this.rbWMV.AutoSize = true;
            this.rbWMV.Location = new System.Drawing.Point(9, 50);
            this.rbWMV.Name = "rbWMV";
            this.rbWMV.Size = new System.Drawing.Size(52, 17);
            this.rbWMV.TabIndex = 5;
            this.rbWMV.Text = "WMV";
            this.rbWMV.UseVisualStyleBackColor = true;
            // 
            // bnCapture
            // 
            this.bnCapture.Location = new System.Drawing.Point(276, 130);
            this.bnCapture.Name = "bnCapture";
            this.bnCapture.Size = new System.Drawing.Size(84, 30);
            this.bnCapture.TabIndex = 6;
            this.bnCapture.Text = "Start Capture";
            this.bnCapture.UseVisualStyleBackColor = true;
            this.bnCapture.Click += new System.EventHandler(this.bnCapture_Click);
            // 
            // gbOptions
            // 
            this.gbOptions.Controls.Add(this.rbMP4);
            this.gbOptions.Controls.Add(this.rbWMV);
            this.gbOptions.Location = new System.Drawing.Point(42, 90);
            this.gbOptions.Name = "gbOptions";
            this.gbOptions.Size = new System.Drawing.Size(69, 79);
            this.gbOptions.TabIndex = 7;
            this.gbOptions.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(424, 172);
            this.Controls.Add(this.gbOptions);
            this.Controls.Add(this.bnCapture);
            this.Controls.Add(this.tbOutputFile);
            this.Controls.Add(this.cbDeviceList);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.gbOptions.ResumeLayout(false);
            this.gbOptions.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbDeviceList;
        private System.Windows.Forms.TextBox tbOutputFile;
        private System.Windows.Forms.RadioButton rbMP4;
        private System.Windows.Forms.RadioButton rbWMV;
        private System.Windows.Forms.Button bnCapture;
        private System.Windows.Forms.GroupBox gbOptions;
    }
}

