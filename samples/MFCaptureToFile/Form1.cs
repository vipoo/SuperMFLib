/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/

using System;
using System.Windows.Forms;

using MediaFoundation;
using MediaFoundation.Misc;

namespace MFCaptureToFile
{
    public partial class Form1 : Form
    {
        // Windows device change message
        private const int WM_DEVICECHANGE = 0x0219;

        // App-specific event ID
        private const int WM_APP_PREVIEW_ERROR = 0x8000 + 1;

        // Category for capture devices
        private Guid KSCATEGORY_CAPTURE = new Guid("65E8773D-8F56-11D0-A3B9-00A0C9223196");

        private const int TARGET_BIT_RATE = 240 * 1000;

        private CCapture m_pCapture = null;
        private RegisterDeviceNotifications m_rdn;

        public Form1()
        {
            InitializeComponent();

            int hr = 0;

            // Set the default output file name
            tbOutputFile.Text = "capture.mp4";

            // Register for device notifications as capture devices get added or removed
            m_rdn = new RegisterDeviceNotifications(this.Handle, KSCATEGORY_CAPTURE);

            // Init MF
            hr = MFExtern.MFStartup(0x00020070, MFStartup.Full);

            // Enumerate the video capture devices.
            if (hr >= 0)
            {
                // Populate the device list
                hr = UpdateDeviceList();
            }

            if (hr >= 0)
            {
                // Enable/disable ui controls
                UpdateUI();

                // If there are no capture devices
                if (cbDeviceList.Items.Count == 0)
                {
                    MessageBox.Show(
                        this,
                        "Could not find any video capture devices.",
                        Application.ProductName,
                        MessageBoxButtons.OK
                        );
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            // Listen for operating system messages.
            switch (m.Msg)
            {
                case WM_DEVICECHANGE:
                    OnDeviceChange(m.WParam, m.LParam);
                    break;

                case WM_APP_PREVIEW_ERROR:
                    NotifyError("Error during capture", m.WParam.ToInt32());
                    break;
            }
            base.WndProc(ref m);
        }

        private void bnCapture_Click(object sender, EventArgs e)
        {
            if (m_pCapture != null && m_pCapture.IsCapturing())
            {
                StopCapture();
            }
            else
            {
                StartCapture();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Turn off device notifications
            if (m_rdn != null)
            {
                m_rdn.Dispose();
                m_rdn = null;
            }

            // End any active captures
            if (m_pCapture != null)
            {
                m_pCapture.EndCaptureSession();
                m_pCapture = null;
            }

            // Shut down MF
            MFExtern.MFShutdown();
        }

        private void rbMP4_CheckedChanged(object sender, EventArgs e)
        {
            // Change the extension of the file when the radio buttons change

            string sName = tbOutputFile.Text;

            int iExtPos = sName.Length - 4;
            if ((iExtPos > 0) && (sName[iExtPos] == '.'))
            {
                if (rbMP4.Checked)
                {
                    tbOutputFile.Text = sName.Substring(0, sName.Length - 4) + ".mp4";
                }
                else
                {
                    tbOutputFile.Text = sName.Substring(0, sName.Length - 4) + ".wmv";
                }
            }
        }

        private void OnDeviceChange(IntPtr reason, IntPtr pHdr)
        {
            // Check for the right category of event
            if (m_rdn.CheckEventDetails(reason, pHdr))
            {
                UpdateDeviceList();
                UpdateUI();

                // If we are capturing, check and see if the device was the one we were using.  We do this
                // by comparing the symbolic name of the capture device to the symbolic name of the changed
                // device.
                if (m_pCapture != null && m_pCapture.IsCapturing())
                {
                    bool bDeviceLost = false;
                    string sSym = RegisterDeviceNotifications.ParseDeviceSymbolicName(pHdr);
                    int hr = m_pCapture.CheckDeviceLost(sSym, out bDeviceLost);

                    if (hr < 0 || bDeviceLost)
                    {
                        StopCapture();

                        MessageBox.Show(this, "The capture device was removed or lost.", "Lost Device", MessageBoxButtons.OK);
                    }
                }
            }
        }

        private void NotifyError(string sErrorMessage, int hrErr)
        {
            string sErrMsg = MFError.GetErrorText(hrErr);
            string sMsg = string.Format("{0} (HRESULT = 0x{1:x}:{2})", sErrorMessage, hrErr, sErrMsg);

            MessageBox.Show(this, sMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            StopCapture();
        }

        private void StartCapture()
        {
            CCapture.EncodingParameters eparams;

            if (rbWMV.Checked)
            {
                eparams.subtype = MFMediaType.WMV3;
            }
            else
            {
                eparams.subtype = MFMediaType.H264;
            }

            eparams.bitrate = TARGET_BIT_RATE;

            int hr = 0;

            IMFActivate pActivate = null;

            // Create the media source for the capture device.

            hr = GetSelectedDevice(out pActivate);

            // Start capturing.

            if (hr >= 0)
            {
                m_pCapture = new CCapture(this.Handle, WM_APP_PREVIEW_ERROR);
                hr = m_pCapture.StartCapture(pActivate, tbOutputFile.Text, eparams);
            }

            if (hr >= 0)
            {
                UpdateUI();
            }

            if (hr < 0)
            {
                NotifyError("Error starting capture.", hr);
            }
        }

        private void StopCapture()
        {
            int hr = 0;

            if (m_pCapture != null)
            {
                hr = m_pCapture.EndCaptureSession();
            }

            m_pCapture = null;

            UpdateDeviceList();

            UpdateUI();

            if (hr < 0)
            {
                NotifyError("Error stopping capture. File might be corrupt.", hr);
            }
        }

        private int GetSelectedDevice(out IMFActivate ppActivate)
        {
            // First get the index of the selected item in the combo box.
            int iListIndex = cbDeviceList.SelectedIndex;

            if (iListIndex < 0)
            {
                ppActivate = null;
                return -1;
            }

            // Parse out the IMFActivate
            MFDevice di = cbDeviceList.SelectedItem as MFDevice;
            ppActivate = di.Activator;

            return 0;
        }

        private int UpdateDeviceList()
        {
            // Remove any previous list
            cbDeviceList.Items.Clear();
            cbDeviceList.Text = "";

            // Query MF for the devices
            MFDevice [] arDevices = MFDevice.GetDevicesOfCat(CLSID.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_GUID);

            // Walk the list
            cbDeviceList.BeginUpdate();
            for (int iDevice = 0; iDevice < arDevices.Length; iDevice++)
            {
                // Add it to the combo box
                cbDeviceList.Items.Add(arDevices[iDevice]);
            }
            cbDeviceList.EndUpdate();

            // If there's at least 1 item
            if (cbDeviceList.Items.Count > 0)
            {
                // Select the first item.
                cbDeviceList.SelectedIndex = 0;
            }

            return 0;
        }

        private void UpdateUI()
        {
            bool bEnable = cbDeviceList.Items.Count == 0;  // Are there any capture devices?
            bool bCapturing = (m_pCapture != null);     // Is video capture in progress now?

            if (bCapturing)
            {
                bnCapture.Text = "Stop Capture";
            }
            else
            {
                bnCapture.Text = "Start Capture";
            }

            gbOptions.Enabled = !bCapturing && !bEnable;
            cbDeviceList.Enabled = !bCapturing && !bEnable;
            tbOutputFile.Enabled = !bCapturing && !bEnable;
            bnCapture.Enabled = !bEnable;
        }
    }
}
