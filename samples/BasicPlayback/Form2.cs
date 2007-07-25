using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MF_BasicPlayback
{
    public partial class fmURL : Form
    {
        public fmURL()
        {
            InitializeComponent();
        }

        private void bnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}