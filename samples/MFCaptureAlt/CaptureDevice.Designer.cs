namespace MFCaptureAlt
{
    partial class CaptureDevice
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
            this.lstbDevices = new System.Windows.Forms.ListBox();
            this.bttOK = new System.Windows.Forms.Button();
            this.bttCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lstbDevices
            // 
            this.lstbDevices.FormattingEnabled = true;
            this.lstbDevices.Location = new System.Drawing.Point(12, 22);
            this.lstbDevices.Name = "lstbDevices";
            this.lstbDevices.Size = new System.Drawing.Size(280, 121);
            this.lstbDevices.TabIndex = 0;
            this.lstbDevices.SelectedIndexChanged += new System.EventHandler(this.lstbDevices_SelectedIndexChanged);
            this.lstbDevices.DoubleClick += new System.EventHandler(this.lstbDevices_DoubleClick);
            // 
            // bttOK
            // 
            this.bttOK.Enabled = false;
            this.bttOK.Location = new System.Drawing.Point(298, 29);
            this.bttOK.Name = "bttOK";
            this.bttOK.Size = new System.Drawing.Size(117, 33);
            this.bttOK.TabIndex = 1;
            this.bttOK.Text = "OK";
            this.bttOK.UseVisualStyleBackColor = true;
            this.bttOK.Click += new System.EventHandler(this.bttOK_Click);
            // 
            // bttCancel
            // 
            this.bttCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bttCancel.Location = new System.Drawing.Point(298, 86);
            this.bttCancel.Name = "bttCancel";
            this.bttCancel.Size = new System.Drawing.Size(117, 33);
            this.bttCancel.TabIndex = 2;
            this.bttCancel.Text = "Cancel";
            this.bttCancel.UseVisualStyleBackColor = true;
            this.bttCancel.Click += new System.EventHandler(this.bttCancel_Click);
            // 
            // CaptureDevice
            // 
            this.AcceptButton = this.bttOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bttCancel;
            this.ClientSize = new System.Drawing.Size(436, 174);
            this.ControlBox = false;
            this.Controls.Add(this.bttCancel);
            this.Controls.Add(this.bttOK);
            this.Controls.Add(this.lstbDevices);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CaptureDevice";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select Device";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lstbDevices;
        private System.Windows.Forms.Button bttOK;
        private System.Windows.Forms.Button bttCancel;
    }
}