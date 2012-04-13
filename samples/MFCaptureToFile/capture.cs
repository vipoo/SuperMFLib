/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Security;

using MediaFoundation;
using MediaFoundation.ReadWrite;
using MediaFoundation.Misc;
using MediaFoundation.Transform;
using MediaFoundation.Alt;

namespace MFCaptureToFile
{
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
            if (m_Activator != null)
            {
                Marshal.ReleaseComObject(m_Activator);

                m_Activator = null;
                m_FriendlyName = null;
                m_SymbolicName = null;
            }
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
                GC.SuppressFinalize(this);
            }
            m_FriendlyName = null;
        }
    }

    class CCapture : COMBase, IMFSourceReaderCallback
    {
        private const int MF_SOURCE_READER_FIRST_VIDEO_STREAM = unchecked((int)0xfffffffc);

        public struct EncodingParameters
        {
            public Guid subtype;
            public int bitrate;
        }

        private IntPtr m_hwndEvent;        // Application window to receive events. 
        private int m_iMessageID;
        private IMFSourceReaderAsync m_pReader;
        private IMFSinkWriter m_pWriter;
        private bool m_bFirstSample;
        private long m_llBaseTime;
        private string m_pwszSymbolicLink;

        [DllImport("user32")]
        private extern static int PostMessage(
            IntPtr handle,
            int msg,
            IntPtr wParam,
            IntPtr lParam
            );

        public CCapture(IntPtr hwnd, int iMessageID)
        {
            m_pReader = null;
            m_pWriter = null;
            m_hwndEvent = hwnd;
            m_iMessageID = iMessageID;
            m_bFirstSample = false;
            m_llBaseTime = 0;
            m_pwszSymbolicLink = null;
        }

        #region IMFSourceReaderCallback methods

        public int OnEvent(int a, IMFMediaEvent b)
        {
            return S_Ok;
        }

        public int OnFlush(int a)
        {
            return S_Ok;
        }

        public int OnReadSample(
            int hrStatus,
            int dwStreamIndex,
            MF_SOURCE_READER_FLAG dwStreamFlags,
            long llTimeStamp,
            IMFSample pSample      // Can be null
            )
        {
            int hr = S_Ok;

            try
            {
                lock (this)
                {
                    if (!IsCapturing())
                    {
                        return S_Ok;
                    }


                    if (Failed(hrStatus))
                    {
                        hr = hrStatus;
                        goto done;
                    }

                    if (pSample != null)
                    {
                        if (m_bFirstSample)
                        {
                            m_llBaseTime = llTimeStamp;
                            m_bFirstSample = false;
                        }

                        // rebase the time stamp
                        llTimeStamp -= m_llBaseTime;

                        hr = pSample.SetSampleTime(llTimeStamp);

                        if (Failed(hr)) { goto done; }

                        hr = m_pWriter.WriteSample(0, pSample);

                        if (Failed(hr)) { goto done; }
                    }

                    // Read another sample.
                    hr = m_pReader.ReadSample(
                        MF_SOURCE_READER_FIRST_VIDEO_STREAM,
                        0,
                        IntPtr.Zero,   // actual
                        IntPtr.Zero,   // flags
                        IntPtr.Zero,   // timestamp
                        IntPtr.Zero    // sample
                        );

                done:
                    if (Failed(hr))
                    {
                        PostMessage(m_hwndEvent, m_iMessageID, new IntPtr(hr), IntPtr.Zero);
                    }
                }
            }
            catch (Exception e)
            {
                hr = Marshal.GetHRForException(e);
            }

            return hr;
        }

        #endregion

        private int OpenMediaSource(IMFMediaSource pSource)
        {
            int hr = S_Ok;

            IMFAttributes pAttributes = null;

            hr = MFExtern.MFCreateAttributes(out pAttributes, 2);

            if (Succeeded(hr))
            {
                hr = pAttributes.SetUnknown(MFAttributesClsid.MF_SOURCE_READER_ASYNC_CALLBACK, this);
            }

            if (Succeeded(hr))
            {
                IMFSourceReader pReader;

                hr = MFExtern.MFCreateSourceReaderFromMediaSource(
                    pSource,
                    pAttributes,
                    out pReader
                    );
                m_pReader = (IMFSourceReaderAsync)pReader;
            }

            SafeRelease(pAttributes);

            return hr;
        }

        public int StartCapture(
            IMFActivate pActivate,
            string pwszFileName,
            EncodingParameters param
            )
        {
            int hr = S_Ok;

            IMFMediaSource pSource = null;
            object pS;

            lock (this)
            {
                // Create the media source for the device.
                hr = pActivate.ActivateObject(
                    typeof(IMFMediaSource).GUID,
                    out pS
                    );
                pSource = (IMFMediaSource)pS;

                // Get the symbolic link. This is needed to handle device-
                // loss notifications. (See CheckDeviceLost.)

                if (Succeeded(hr))
                {
                    int iSize;
                    hr = pActivate.GetAllocatedString(
                        MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK,
                        out m_pwszSymbolicLink,
                        out iSize
                        );
                }

                if (Succeeded(hr))
                {
                    hr = OpenMediaSource(pSource);
                }

                // Create the sink writer 
                if (Succeeded(hr))
                {
                    hr = MFExtern.MFCreateSinkWriterFromURL(
                        pwszFileName,
                        null,
                        null,
                        out m_pWriter
                        );
                }

                // Set up the encoding parameters.
                if (Succeeded(hr))
                {
                    hr = ConfigureCapture(param);
                }

                if (Succeeded(hr))
                {
                    m_bFirstSample = true;
                    m_llBaseTime = 0;

                    // Request the first video frame.

                    hr = m_pReader.ReadSample(
                        MF_SOURCE_READER_FIRST_VIDEO_STREAM,
                        0,
                        IntPtr.Zero,
                        IntPtr.Zero,
                        IntPtr.Zero,
                        IntPtr.Zero
                        );
                }

                SafeRelease(pSource);
            }

            return hr;
        }

        public int EndCaptureSession()
        {
            int hr = S_Ok;

            lock (this)
            {
                if (m_pWriter != null)
                {
                    hr = m_pWriter.Finalize_();
                    SafeRelease(m_pWriter);
                    m_pWriter = null;
                }

                if (m_pReader != null)
                {
                    SafeRelease(m_pReader);
                    m_pReader = null;
                }
            }

            return hr;
        }

        public bool IsCapturing()
        {
            bool bIsCapturing;

            lock (this)
            {
                bIsCapturing = (m_pWriter != null);
            }

            return bIsCapturing;
        }

        public int CheckDeviceLost(string sSym, out bool pbDeviceLost)
        {
            pbDeviceLost = false;

            lock (this)
            {
                if (!IsCapturing())
                {
                    goto done;
                }

                if (m_pwszSymbolicLink != null)
                {
                    if (string.Compare(m_pwszSymbolicLink, sSym, true) == 0)
                    {
                        pbDeviceLost = true;
                    }
                }
            }

        done:

            return S_Ok;
        }

        int ConfigureSourceReader(IMFSourceReaderAsync pReader)
        {
            // The list of acceptable types.
            Guid[] subtypes = { 
                MFMediaType.NV12, MFMediaType.YUY2, MFMediaType.UYVY,
                MFMediaType.RGB32, MFMediaType.RGB24, MFMediaType.IYUV
            };

            int hr = S_Ok;
            bool bUseNativeType = false;

            Guid subtype;

            IMFMediaType pType = null;

            // If the source's native format matches any of the formats in 
            // the list, prefer the native format.

            // Note: The camera might support multiple output formats, 
            // including a range of frame dimensions. The application could
            // provide a list to the user and have the user select the
            // camera's output format. That is outside the scope of this
            // sample, however.

            hr = pReader.GetNativeMediaType(
                MF_SOURCE_READER_FIRST_VIDEO_STREAM,
                0,  // Type index
                out pType
                );

            if (Failed(hr)) { goto done; }

            hr = pType.GetGUID(MFAttributesClsid.MF_MT_SUBTYPE, out subtype);

            if (Failed(hr)) { goto done; }

            for (int i = 0; i < subtypes.Length; i++)
            {
                if (subtype == subtypes[i])
                {
                    hr = pReader.SetCurrentMediaType(
                        MF_SOURCE_READER_FIRST_VIDEO_STREAM,
                        IntPtr.Zero,
                        pType
                        );

                    bUseNativeType = true;
                    break;
                }
            }

            if (!bUseNativeType)
            {
                // None of the native types worked. The camera might offer 
                // output of a compressed type such as MJPEG or DV.

                // Try adding a decoder.

                for (int i = 0; i < subtypes.Length; i++)
                {
                    hr = pType.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, subtypes[i]);

                    if (Failed(hr)) { goto done; }

                    hr = pReader.SetCurrentMediaType(
                        MF_SOURCE_READER_FIRST_VIDEO_STREAM,
                        IntPtr.Zero,
                        pType
                        );

                    if (Succeeded(hr))
                    {
                        break;
                    }
                }
            }

        done:
            SafeRelease(pType);
            return hr;
        }

        int ConfigureEncoder(
            EncodingParameters eparams,
            IMFMediaType pType,
            IMFSinkWriter pWriter,
            out int pdwStreamIndex
            )
        {
            int hr = S_Ok;

            IMFMediaType pType2 = null;

            hr = MFExtern.MFCreateMediaType(out pType2);

            if (Succeeded(hr))
            {
                hr = pType2.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);
            }

            if (Succeeded(hr))
            {
                hr = pType2.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, eparams.subtype);
            }

            if (Succeeded(hr))
            {
                hr = pType2.SetUINT32(MFAttributesClsid.MF_MT_AVG_BITRATE, eparams.bitrate);
            }

            if (Succeeded(hr))
            {
                hr = CopyAttribute(pType, pType2, MFAttributesClsid.MF_MT_FRAME_SIZE);
            }

            if (Succeeded(hr))
            {
                hr = CopyAttribute(pType, pType2, MFAttributesClsid.MF_MT_FRAME_RATE);
            }

            if (Succeeded(hr))
            {
                hr = CopyAttribute(pType, pType2, MFAttributesClsid.MF_MT_PIXEL_ASPECT_RATIO);
            }

            if (Succeeded(hr))
            {
                hr = CopyAttribute(pType, pType2, MFAttributesClsid.MF_MT_INTERLACE_MODE);
            }

            pdwStreamIndex = 0;
            if (Succeeded(hr))
            {
                hr = pWriter.AddStream(pType2, out pdwStreamIndex);
            }

            SafeRelease(pType2);

            return hr;
        }

        int ConfigureCapture(EncodingParameters eparam)
        {
            int hr = S_Ok;
            int sink_stream = 0;

            IMFMediaType pType = null;

            hr = ConfigureSourceReader(m_pReader);

            if (Succeeded(hr))
            {
                hr = m_pReader.GetCurrentMediaType(
                    MF_SOURCE_READER_FIRST_VIDEO_STREAM,
                    out pType
                    );
            }

            if (Succeeded(hr))
            {
                hr = ConfigureEncoder(eparam, pType, m_pWriter, out sink_stream);
            }

            if (Succeeded(hr))
            {
                // Register the color converter DSP for this process, in the video 
                // processor category. This will enable the sink writer to enumerate
                // the color converter when the sink writer attempts to match the
                // media types.

                hr = MFExtern.MFTRegisterLocalByCLSID(
                    typeof(CColorConvertDMO).GUID,
                    MFTransformCategory.MFT_CATEGORY_VIDEO_PROCESSOR,
                    "",
                    MFT_EnumFlag.SyncMFT,
                    0,
                    null,
                    0,
                    null
                    );
            }

            if (Succeeded(hr))
            {
                hr = m_pWriter.SetInputMediaType(sink_stream, pType, null);
            }

            if (Succeeded(hr))
            {
                hr = m_pWriter.BeginWriting();
            }

            SafeRelease(pType);

            return hr;
        }

        int CopyAttribute(IMFAttributes pSrc, IMFAttributes pDest, Guid key)
        {
            PropVariant var = new PropVariant();

            int hr = S_Ok;

            hr = pSrc.GetItem(key, var);
            if (Succeeded(hr))
            {
                hr = pDest.SetItem(key, var);
            }

            return hr;
        }
    }
}
