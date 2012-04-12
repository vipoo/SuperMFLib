/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/

using System;
using System.Windows.Forms;

using MediaFoundation;

namespace MFCaptureToFile
{
    public partial class Form1 : Form
    {
        private const int WM_APP_PREVIEW_ERROR = 0x8000 + 1;
        private const int DBT_DEVTYP_DEVICEINTERFACE = 0x00000005;
        private const int DBT_DEVNODES_CHANGED = 0x0007;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int WM_DEVICECHANGE = 0x0219;
        private Guid KSCATEGORY_CAPTURE = new Guid("65E8773D-8F56-11D0-A3B9-00A0C9223196");

        private const int TARGET_BIT_RATE = 240 * 1000;

        private DeviceList m_devices = new DeviceList(CLSID.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_GUID);
        private CCapture m_pCapture = null;
        private RegisterDeviceNotifications m_rdn;

        // The Items in the cbDeviceList are of this type.  This allows us to keep the
        // IMFActivate with the sFriendlyName.
        private class DeviceInfo
        {
            public string sFriendlyName;
            public IMFActivate iActivate;

            public DeviceInfo(string sName, IMFActivate ia)
            {
                sFriendlyName = sName;
                iActivate = ia;
            }

            public override string ToString()
            {
                return sFriendlyName;
            }
        }

        public Form1()
        {
            InitializeComponent();

            int hr = 0;

            // Set the default output file name
            tbOutputFile.Text = "capture.mp4";

            // Register for device notifications as capture devices get added or removed
            m_rdn = new RegisterDeviceNotifications(this.Handle, DBT_DEVTYP_DEVICEINTERFACE, KSCATEGORY_CAPTURE);

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
                if (m_devices.Count() == 0)
                {
                    MessageBox.Show(
                        this,
                        "Could not find any video capture devices.",
                        "MFCaptureToFile",
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
            m_rdn.Dispose();

            // End any active captures
            if (m_pCapture != null)
            {
                m_pCapture.EndCaptureSession();
            }

            // Clean the device list
            m_devices.Clear();

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
                    sName = sName.Substring(0, sName.Length - 4) + ".mp4";
                }
                else
                {
                    sName = sName.Substring(0, sName.Length - 4) + ".wmv";
                }
            }
            tbOutputFile.Text = sName;
        }

        private void OnDeviceChange(IntPtr reason, IntPtr pHdr)
        {
            int iValue = reason.ToInt32();
            if (iValue == DBT_DEVNODES_CHANGED || iValue == DBT_DEVICEARRIVAL)
            {
                // Check for added/removed devices, regardless of whether
                // the application is capturing video at this time.

                UpdateDeviceList();
                UpdateUI();
            }

            // Now check if the current video capture device was lost.

            if (pHdr == IntPtr.Zero)
            {
                // We receive several messages.  Wait for the one that provides the symbolic name
                return;
            }

            int dt = RegisterDeviceNotifications.ParseDeviceType(pHdr);

            if (dt != DBT_DEVTYP_DEVICEINTERFACE)
            {
                // Wrong type of notification
                return;
            }

            int hr = 0;
            bool bDeviceLost = false;

            // If we are capturing, check and see if the device was the one we were using.  We do this
            // by comparing the symbolic name of the capture device to the symbolic name of the changed
            // device.
            if (m_pCapture != null && m_pCapture.IsCapturing())
            {
                string sSym = RegisterDeviceNotifications.ParseDeviceSymbolicName(pHdr);
                hr = m_pCapture.CheckDeviceLost(sSym, out bDeviceLost);

                if (hr < 0 || bDeviceLost)
                {
                    StopCapture();

                    MessageBox.Show(this, "The capture device was removed or lost.", "Lost Device", MessageBoxButtons.OK);
                }
            }
        }

        private void NotifyError(string sErrorMessage, int hrErr)
        {
            string sMsg = string.Format("{0} (HRESULT = 0x{1:x})", sErrorMessage, hrErr);

            MessageBox.Show(this, sMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void StartCapture()
        {
            EncodingParameters eparams;

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
            }

            if (hr >= 0)
            {
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

            hr = m_pCapture.EndCaptureSession();

            m_pCapture = null;

            UpdateDeviceList();

            // NOTE: Updating the device list releases the existing IMFActivate 
            // pointers. This ensures that the current instance of the video capture 
            // source is released.

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
            DeviceInfo di = cbDeviceList.SelectedItem as DeviceInfo;
            ppActivate = di.iActivate;

            return 0;
        }

        private int UpdateDeviceList()
        {
            int hr = 0;

            string szFriendlyName = null;
            IMFActivate ia = null;

            // Remove any previous list
            cbDeviceList.Items.Clear();

            // Remove any previous list
            m_devices.Clear();

            // Query MF for the devices
            hr = m_devices.EnumerateDevices();

            if (hr < 0) { goto done; }

            // Walk the list
            cbDeviceList.BeginUpdate();
            for (int iDevice = 0; iDevice < m_devices.Count(); iDevice++)
            {
                // Get the friendly name of the device and its activator
                hr = m_devices.GetDeviceAndName(iDevice, out szFriendlyName, out ia);

                if (hr < 0) { goto done; }

                // Add it to the combo box
                int iIndex = cbDeviceList.Items.Add(new DeviceInfo(szFriendlyName, ia));
            }
            cbDeviceList.EndUpdate();

            // If there's at least 1 item
            if (m_devices.Count() > 0)
            {
                // Select the first item.
                cbDeviceList.SelectedIndex = 0;
            }

        done:
            return hr;
        }

        private void UpdateUI()
        {
            bool bEnable = m_devices.Count() == 0;  // Are there any capture devices?
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
