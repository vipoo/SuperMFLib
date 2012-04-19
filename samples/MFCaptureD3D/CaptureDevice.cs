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
using System.Runtime.InteropServices;
using System.Security;

using MediaFoundation;
using MediaFoundation.Misc;

namespace MFCaptureD3D
{
    partial class CaptureDevice : Form, IDisposable
    {
        const int WM_DEVICECHANGE = 0x0219;
        // Category for capture devices
        private readonly Guid KSCATEGORY_CAPTURE = new Guid("65E8773D-8F56-11D0-A3B9-00A0C9223196");

        private RegisterDeviceNotifications m_rdn;

        public CaptureDevice()
        {
            InitializeComponent();

            m_rdn = new RegisterDeviceNotifications(this.Handle, KSCATEGORY_CAPTURE);
            LoadDevicesList();
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_DEVICECHANGE:
                    OnDeviceChange(m.WParam, m.LParam);
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        public MFDevice SelectedCaptureDevice
        {
            get { return lstbDevices.SelectedItem as MFDevice; }
        }

        private void LoadDevicesList()
        {
            // Populate the list with the friendly names of the devices.
            MFDevice[] arDevices = MFDevice.GetDevicesOfCat(CLSID.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_GUID);

            lstbDevices.BeginUpdate();
            lstbDevices.Items.Clear();

            foreach (MFDevice m in arDevices)
            {
                lstbDevices.Items.Add(m);
            }
            lstbDevices.EndUpdate();

            bttOK.Enabled = false;
        }

        private void OnDeviceChange(IntPtr reason, IntPtr pHdr)
        {
            // Check for the right category of event
            if (m_rdn.CheckEventDetails(reason, pHdr))
            {
                ClearOld();
                LoadDevicesList();
            }
        }

        private void bttCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void bttOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void lstbDevices_DoubleClick(object sender, EventArgs e)
        {
            if (lstbDevices.SelectedIndex >= 0)
            {
                bttOK_Click(null, null);
            }
        }

        private void ClearOld()
        {
            foreach (MFDevice m in lstbDevices.Items)
            {
                m.Dispose();
            }
        }

        public new void Dispose()
        {
            ClearOld();
            m_rdn.Dispose();
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        private void lstbDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            bttOK.Enabled = lstbDevices.SelectedIndex >= 0;
        }
    }

    class MFDevice : IDisposable
    {
        private IMFActivate m_Activator;
        private string m_FriendlyName;
        private string m_SymbolicName;

        public MFDevice(IMFActivate Mon)
        {
            m_Activator = Mon;
            m_FriendlyName = null;
            m_SymbolicName = null;
        }

        ~MFDevice()
        {
            Dispose();
        }

        public IMFActivate Activator
        {
            get
            {
                return m_Activator;
            }
        }

        public string Name
        {
            get
            {
                if (m_FriendlyName == null)
                {
                    int hr = 0;
                    int iSize = 0;

                    hr = m_Activator.GetAllocatedString(
                        MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME,
                        out m_FriendlyName,
                        out iSize
                        );
                }

                return m_FriendlyName;
            }
        }

        /// <summary>
        /// Returns a unique identifier for a device
        /// </summary>
        public string SymbolicName
        {
            get
            {
                if (m_SymbolicName == null)
                {
                    int iSize;
                    int hr = m_Activator.GetAllocatedString(
                        MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK,
                        out m_SymbolicName,
                        out iSize
                        );
                }

                return m_SymbolicName;
            }
        }

        /// <summary>
        /// Returns an array of DsDevices of type devcat.
        /// </summary>
        /// <param name="cat">Any one of FilterCategory</param>
        public static MFDevice[] GetDevicesOfCat(Guid FilterCategory)
        {
            // Use arrayList to build the retun list since it is easily resizable
            MFDevice[] devret = null;
            IMFActivate[] ppDevices;

            //////////

            int hr = 0;
            IMFAttributes pAttributes = null;

            // Initialize an attribute store. We will use this to
            // specify the enumeration parameters.

            hr = MFExtern.MFCreateAttributes(out pAttributes, 1);

            // Ask for source type = video capture devices
            if (hr >= 0)
            {
                hr = pAttributes.SetGUID(
                    MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE,
                    FilterCategory
                    );
            }

            // Enumerate devices.
            int cDevices;
            if (hr >= 0)
            {
                hr = MFExtern.MFEnumDeviceSources(pAttributes, out ppDevices, out cDevices);

                if (hr >= 0)
                {
                    devret = new MFDevice[cDevices];

                    for (int x = 0; x < cDevices; x++)
                    {
                        devret[x] = new MFDevice(ppDevices[x]);
                    }
                }
            }

            if (pAttributes != null)
            {
                Marshal.ReleaseComObject(pAttributes);
            }

            return devret;
        }

        public override string ToString()
        {
            return Name;
        }

        public void Dispose()
        {
            if (m_Activator != null)
            {
                Marshal.ReleaseComObject(m_Activator);
                m_Activator = null;
            }
            m_FriendlyName = null;
            m_SymbolicName = null;

            GC.SuppressFinalize(this);
        }
    }

    class RegisterDeviceNotifications : IDisposable
    {
        #region Definitions

        [StructLayout(LayoutKind.Sequential)]
        private class DEV_BROADCAST_HDR
        {
            public int dbch_size;
            public int dbch_devicetype;
            public int dbch_reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        private class DEV_BROADCAST_DEVICEINTERFACE
        {
            public int dbcc_size;
            public int dbcc_devicetype;
            public int dbcc_reserved;
            public Guid dbcc_classguid;
            public char dbcc_name;
        }

        [DllImport("User32.dll",
            CharSet = CharSet.Unicode,
            ExactSpelling = true,
            EntryPoint = "RegisterDeviceNotificationW",
            SetLastError = true),
        SuppressUnmanagedCodeSecurity]
        private static extern IntPtr RegisterDeviceNotification(
            IntPtr hDlg,
            [MarshalAs(UnmanagedType.LPStruct)] DEV_BROADCAST_DEVICEINTERFACE di,
            int dwFlags
            );

        [DllImport("User32.dll", ExactSpelling = true, SetLastError = true), SuppressUnmanagedCodeSecurity]
        [return:MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnregisterDeviceNotification(
            IntPtr hDlg
            );

        private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
        private const int DBT_DEVTYP_DEVICEINTERFACE = 0x00000005;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;

        #endregion

        // Handle of the notification.  Used by unregister
        IntPtr m_hdevnotify = IntPtr.Zero;

        // Category of events
        Guid m_Category;

        public RegisterDeviceNotifications(IntPtr hWnd, Guid gCat)
        {
            m_Category = gCat;

            DEV_BROADCAST_DEVICEINTERFACE di = new DEV_BROADCAST_DEVICEINTERFACE();

            // Register to be notified of events of category gCat
            di.dbcc_size = Marshal.SizeOf(di);
            di.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
            di.dbcc_classguid = gCat;

            m_hdevnotify = RegisterDeviceNotification(
                hWnd,
                di,
                DEVICE_NOTIFY_WINDOW_HANDLE
                );

            // If it failed, throw an exception
            if (m_hdevnotify == IntPtr.Zero)
            {
                int i = unchecked((int)0x80070000);
                i += Marshal.GetLastWin32Error();
                throw new COMException("Failed to RegisterDeviceNotifications", i);
            }
        }

        public void Dispose()
        {
            if (m_hdevnotify != IntPtr.Zero)
            {
                UnregisterDeviceNotification(m_hdevnotify);
                m_hdevnotify = IntPtr.Zero;
            }
        }

        // Static routine to parse out the device type from the IntPtr received in WndProc
        public bool CheckEventDetails(IntPtr pReason, IntPtr pHdr)
        {
            int iValue = pReason.ToInt32();

            // Check the event type
            if (iValue != DBT_DEVICEREMOVECOMPLETE && iValue != DBT_DEVICEARRIVAL)
                return false;

            // Do we have device details yet?
            if (pHdr == IntPtr.Zero)
                return false;

            // Parse the first chunk
            DEV_BROADCAST_HDR pBH = new DEV_BROADCAST_HDR();
            Marshal.PtrToStructure(pHdr, pBH);

            // Check the device type
            if (pBH.dbch_devicetype != DBT_DEVTYP_DEVICEINTERFACE)
                return false;

            // Only parse this if the right device type
            DEV_BROADCAST_DEVICEINTERFACE pDI = new DEV_BROADCAST_DEVICEINTERFACE();
            Marshal.PtrToStructure(pHdr, pDI);

            return (pDI.dbcc_classguid == m_Category);
        }

        // Static routine to parse out the Symbolic name from the IntPtr received in WndProc
        public static string ParseDeviceSymbolicName(IntPtr pHdr)
        {
            IntPtr ip = Marshal.OffsetOf(typeof(DEV_BROADCAST_DEVICEINTERFACE), "dbcc_name");
            return Marshal.PtrToStringUni(pHdr + (ip.ToInt32()));
        }
    }

}