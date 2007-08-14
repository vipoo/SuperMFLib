/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released
as public domain.  It is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
*****************************************************************************/

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MF_ProtectedPlayback
{
    public partial class Form3 : Form
    {
        #region externs

        [DllImport("user32", CharSet = CharSet.Auto)]
        private extern static int PostMessage(
            IntPtr handle, int msg, IntPtr wParam, IntPtr lParam);

        #endregion

        #region Members

        private string m_szUrl;
        private byte[] m_byteArray;
        private IntPtr m_hwnd;

        #endregion

        public Form3(string szUrl, byte[] byteArray, IntPtr hwnd)
        {
            m_szUrl = szUrl;
            m_byteArray = byteArray;
            m_hwnd = hwnd;

            InitializeComponent();
        }

        private void Form3_Shown(object sender, EventArgs e)
        {
            webBrowser1.Navigate(m_szUrl, "_self", m_byteArray, "Content-Type: application/x-www-form-urlencoded\r\n");
        }

        private void Form3_FormClosed(object sender, FormClosedEventArgs e)
        {
            PostMessage(m_hwnd, ContentProtectionManager.WM_APP_BROWSER_DONE, IntPtr.Zero, IntPtr.Zero);
        }
    }
}