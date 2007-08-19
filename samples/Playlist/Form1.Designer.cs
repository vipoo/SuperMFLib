namespace Playlist
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
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addToPlaylistToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeFromPlaylistToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.lbPlaylist = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tbCurrentTime = new System.Windows.Forms.TextBox();
            this.tbDuration = new System.Windows.Forms.TextBox();
            this.tbPresentationTime = new System.Windows.Forms.TextBox();
            this.bnPlay = new System.Windows.Forms.Button();
            this.bnStop = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.lbEventNotification = new System.Windows.Forms.ListBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(670, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToPlaylistToolStripMenuItem,
            this.removeFromPlaylistToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // addToPlaylistToolStripMenuItem
            // 
            this.addToPlaylistToolStripMenuItem.Name = "addToPlaylistToolStripMenuItem";
            this.addToPlaylistToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.addToPlaylistToolStripMenuItem.Text = "&Add to Playlist";
            this.addToPlaylistToolStripMenuItem.Click += new System.EventHandler(this.addToPlaylistToolStripMenuItem_Click);
            // 
            // removeFromPlaylistToolStripMenuItem
            // 
            this.removeFromPlaylistToolStripMenuItem.Enabled = false;
            this.removeFromPlaylistToolStripMenuItem.Name = "removeFromPlaylistToolStripMenuItem";
            this.removeFromPlaylistToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.removeFromPlaylistToolStripMenuItem.Text = "&Remove from Playlist";
            this.removeFromPlaylistToolStripMenuItem.Click += new System.EventHandler(this.removeFromPlaylistToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(183, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(24, 71);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Playlist";
            // 
            // lbPlaylist
            // 
            this.lbPlaylist.FormattingEnabled = true;
            this.lbPlaylist.HorizontalScrollbar = true;
            this.lbPlaylist.Location = new System.Drawing.Point(27, 102);
            this.lbPlaylist.Name = "lbPlaylist";
            this.lbPlaylist.Size = new System.Drawing.Size(382, 212);
            this.lbPlaylist.TabIndex = 2;
            this.lbPlaylist.DoubleClick += new System.EventHandler(this.lbPlaylist_DoubleClick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(424, 102);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "Current time:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(424, 133);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(110, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "Segment Duration:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(424, 164);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(106, 15);
            this.label4.TabIndex = 5;
            this.label4.Text = "Presentation time:";
            // 
            // tbCurrentTime
            // 
            this.tbCurrentTime.BackColor = System.Drawing.SystemColors.Control;
            this.tbCurrentTime.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbCurrentTime.Location = new System.Drawing.Point(540, 100);
            this.tbCurrentTime.Name = "tbCurrentTime";
            this.tbCurrentTime.Size = new System.Drawing.Size(49, 13);
            this.tbCurrentTime.TabIndex = 6;
            this.tbCurrentTime.Text = "00:00:00";
            // 
            // tbDuration
            // 
            this.tbDuration.BackColor = System.Drawing.SystemColors.Control;
            this.tbDuration.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbDuration.Location = new System.Drawing.Point(540, 133);
            this.tbDuration.Name = "tbDuration";
            this.tbDuration.Size = new System.Drawing.Size(49, 13);
            this.tbDuration.TabIndex = 7;
            this.tbDuration.Text = "00:00:00";
            // 
            // tbPresentationTime
            // 
            this.tbPresentationTime.BackColor = System.Drawing.SystemColors.Control;
            this.tbPresentationTime.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbPresentationTime.Location = new System.Drawing.Point(540, 167);
            this.tbPresentationTime.Name = "tbPresentationTime";
            this.tbPresentationTime.Size = new System.Drawing.Size(49, 13);
            this.tbPresentationTime.TabIndex = 8;
            this.tbPresentationTime.Text = "00:00:00";
            // 
            // bnPlay
            // 
            this.bnPlay.Enabled = false;
            this.bnPlay.Location = new System.Drawing.Point(27, 338);
            this.bnPlay.Name = "bnPlay";
            this.bnPlay.Size = new System.Drawing.Size(75, 23);
            this.bnPlay.TabIndex = 9;
            this.bnPlay.Text = "&Play";
            this.bnPlay.UseVisualStyleBackColor = true;
            this.bnPlay.Click += new System.EventHandler(this.Play_Click);
            // 
            // bnStop
            // 
            this.bnStop.Enabled = false;
            this.bnStop.Location = new System.Drawing.Point(119, 338);
            this.bnStop.Name = "bnStop";
            this.bnStop.Size = new System.Drawing.Size(75, 23);
            this.bnStop.TabIndex = 10;
            this.bnStop.Text = "&Stop";
            this.bnStop.UseVisualStyleBackColor = true;
            this.bnStop.Click += new System.EventHandler(this.Stop_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(28, 389);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Event notification";
            // 
            // lbEventNotification
            // 
            this.lbEventNotification.FormattingEnabled = true;
            this.lbEventNotification.Location = new System.Drawing.Point(26, 410);
            this.lbEventNotification.Name = "lbEventNotification";
            this.lbEventNotification.Size = new System.Drawing.Size(614, 95);
            this.lbEventNotification.TabIndex = 12;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Audio files|*.mp3;*.wma|Windows Media|*.wma|MP3|*.mp3|All files|*.*";
            this.openFileDialog1.Multiselect = true;
            this.openFileDialog1.ReadOnlyChecked = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(670, 516);
            this.Controls.Add(this.lbEventNotification);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.bnStop);
            this.Controls.Add(this.bnPlay);
            this.Controls.Add(this.tbPresentationTime);
            this.Controls.Add(this.tbDuration);
            this.Controls.Add(this.tbCurrentTime);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lbPlaylist);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Sequencer Source Playback .NET";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addToPlaylistToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeFromPlaylistToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox lbPlaylist;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbCurrentTime;
        private System.Windows.Forms.TextBox tbDuration;
        private System.Windows.Forms.TextBox tbPresentationTime;
        private System.Windows.Forms.Button bnPlay;
        private System.Windows.Forms.Button bnStop;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListBox lbEventNotification;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}

