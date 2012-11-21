/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released
as public domain.  It is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
 *
Written by:
Gerardo Hernandez
BrightApp.com

Modified by snarfle
*****************************************************************************/
using System;
using System.Windows.Forms;

using MediaFoundation.Misc;

namespace MFCaptureD3D
{
    partial class WinMain : Form
    {
        const int WM_APP = 0x8000;
        const int WM_APP_PREVIEW_ERROR = WM_APP + 2;
        const int WM_SIZE = 0x0005;
        const int WM_DEVICECHANGE = 0x0219;

        // Category for capture devices
        private readonly Guid KSCATEGORY_CAPTURE = new Guid("65E8773D-8F56-11D0-A3B9-00A0C9223196");

        private CPreview m_pPreview = null;
        private RegisterDeviceNotifications m_rdn;

        public WinMain()
        {
            InitializeComponent();

            m_rdn = new RegisterDeviceNotifications(this.Handle, KSCATEGORY_CAPTURE);

            // Create the object that manages video preview.
            m_pPreview = new CPreview(picbCapture.Handle, this.Handle);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_SIZE:
                    if (m_pPreview != null)
                    {
                        m_pPreview.ResizeVideo();
                    }
                    break;

                case WM_APP_PREVIEW_ERROR:
                    NotifyError("An error occurred.", (int)m.WParam);
                    break;

                case WM_DEVICECHANGE:
                    OnDeviceChange(m.WParam, m.LParam);
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private void NotifyError(string sErrorMessage, int hrErr)
        {
            m_pPreview.CloseDevice();

            string sErrMsg = MFError.GetErrorText(hrErr);
            string sMsg = string.Format("{0} (HRESULT = 0x{1:x}:{2})", sErrorMessage, hrErr, sErrMsg);

            MessageBox.Show(this, sMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void selectCaptureDeviceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (CaptureDevice DevSelection = new CaptureDevice())
            {
                if (DevSelection.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    // Give this source to the CPlayer object for preview.
                    int hr = m_pPreview.SetDevice(DevSelection.SelectedCaptureDevice);
                    MFError.ThrowExceptionForHR(hr);
                }
            }
        }

        private void WinMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_pPreview.Dispose();
            m_pPreview = null;
        }

        private void OnDeviceChange(IntPtr reason, IntPtr pHdr)
        {
            // Check for the right category of event
            if (m_rdn.CheckEventDetails(reason, pHdr))
            {
                if (m_pPreview != null)
                {
                    string sSym = RegisterDeviceNotifications.ParseDeviceSymbolicName(pHdr);
                    if (m_pPreview.CheckDeviceLost(sSym))
                    {
                        NotifyError("Lost the capture device", 0);
                    }
                }
            }
        }
    }
}
