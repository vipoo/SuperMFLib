namespace MF_BasicPlayback
{
    partial class fmURL
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
            this.tbURL = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.bnOk = new System.Windows.Forms.Button();
            this.bnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tbURL
            // 
            this.tbURL.Location = new System.Drawing.Point(12, 41);
            this.tbURL.Name = "tbURL";
            this.tbURL.Size = new System.Drawing.Size(260, 20);
            this.tbURL.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(117, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Enter the URL to open:";
            // 
            // bnOk
            // 
            this.bnOk.Location = new System.Drawing.Point(110, 109);
            this.bnOk.Name = "bnOk";
            this.bnOk.Size = new System.Drawing.Size(75, 23);
            this.bnOk.TabIndex = 2;
            this.bnOk.Text = "Ok";
            this.bnOk.UseVisualStyleBackColor = true;
            this.bnOk.Click += new System.EventHandler(this.bnOk_Click);
            // 
            // bnCancel
            // 
            this.bnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bnCancel.Location = new System.Drawing.Point(191, 109);
            this.bnCancel.Name = "bnCancel";
            this.bnCancel.Size = new System.Drawing.Size(75, 23);
            this.bnCancel.TabIndex = 3;
            this.bnCancel.Text = "Cancel";
            this.bnCancel.UseVisualStyleBackColor = true;
            // 
            // fmURL
            // 
            this.AcceptButton = this.bnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bnCancel;
            this.ClientSize = new System.Drawing.Size(284, 143);
            this.Controls.Add(this.bnCancel);
            this.Controls.Add(this.bnOk);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbURL);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "fmURL";
            this.Text = "Open URL";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button bnOk;
        private System.Windows.Forms.Button bnCancel;
        public System.Windows.Forms.TextBox tbURL;
    }
}