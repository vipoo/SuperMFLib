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
using System.Runtime.InteropServices;
using System.Diagnostics;

using MediaFoundation;
using MediaFoundation.ReadWrite;
using MediaFoundation.Alt;
using MediaFoundation.Misc;

namespace MFCaptureD3D
{
    class CPreview : COMBase, IMFSourceReaderCallback, IDisposable
    {
        #region Definitions

        [DllImport("user32")]
        private extern static int PostMessage(
            IntPtr handle,
            int msg,
            IntPtr wParam,
            IntPtr lParam
            );

        private const int WM_APP = 0x8000;
        private const int WM_APP_PREVIEW_ERROR = WM_APP + 2;

        #endregion

        #region Member Variables

        private IMFSourceReaderAsync m_pReader;

        private IntPtr m_hwndEvent;       // App window to receive events.

        private DrawDevice m_draw;             // Manages the Direct3D device.

        private string m_pwszSymbolicLink;

        #endregion

        // Constructor
        public CPreview(IntPtr hVideo, IntPtr hEvent)
        {
            m_pReader = null;
            m_hwndEvent = hEvent;
            m_pwszSymbolicLink = null;
            m_draw = new DrawDevice();

            int hr = MFExtern.MFStartup(0x20070, MFStartup.Lite);
            MFError.ThrowExceptionForHR(hr);

            hr = m_draw.CreateDevice(hVideo);
            MFError.ThrowExceptionForHR(hr);
        }

#if DEBUG
        ~CPreview()
        {
            // Was Dispose called?
            Debug.Assert(m_draw == null);
        }
#endif

        #region Public Methods

        public static void CloseMediaSession()
        {
            // Shutdown the Media Foundation platform
            int hr = MFExtern.MFShutdown();
            MFError.ThrowExceptionForHR(hr);
        }

        //-------------------------------------------------------------------
        // SetDevice
        //
        // Set up preview for a specified video capture device.
        //-------------------------------------------------------------------

        public int SetDevice(MFDevice pDevice)
        {
            int hr = S_Ok;

            IMFActivate pActivate = pDevice.Activator;
            IMFMediaSource pSource = null;
            IMFAttributes pAttributes = null;
            object o = null;

            lock (this)
            {
                try
                {
                    // Release the current device, if any.
                    hr = CloseDevice();

                    if (Succeeded(hr))
                    {
                        // Create the media source for the device.
                        hr = pActivate.ActivateObject(typeof(IMFMediaSource).GUID, out o);
                    }

                    if (Succeeded(hr))
                    {
                        pSource = (IMFMediaSource)o;
                    }

                    // Get Symbolic device link
                    m_pwszSymbolicLink = pDevice.SymbolicName;

                    //
                    // Create the source reader.
                    //

                    // Create an attribute store to hold initialization settings.

                    if (Succeeded(hr))
                    {
                        hr = MFExtern.MFCreateAttributes(out pAttributes, 2);
                    }

                    if (Succeeded(hr))
                    {
                        hr = pAttributes.SetUINT32(MFAttributesClsid.MF_READWRITE_DISABLE_CONVERTERS, 1);
                    }

                    if (Succeeded(hr))
                    {
                        hr = pAttributes.SetUnknown(MFAttributesClsid.MF_SOURCE_READER_ASYNC_CALLBACK, this);
                    }

                    IMFSourceReader pRead = null;
                    if (Succeeded(hr))
                    {
                        hr = MFExtern.MFCreateSourceReaderFromMediaSource(pSource, pAttributes, out pRead);
                    }

                    if (Succeeded(hr))
                    {
                        m_pReader = (IMFSourceReaderAsync)pRead;
                    }

                    if (Succeeded(hr))
                    {
                        // Try to find a suitable output type.
                        for (int i = 0; ; i++)
                        {
                            IMFMediaType pType;
                            hr = m_pReader.GetNativeMediaType((int)MF_SOURCE_READER.FirstVideoStream, i, out pType);
                            if (Failed(hr))
                            {
                                break;
                            }

                            try
                            {
                                hr = TryMediaType(pType);

                                SafeRelease(pType);
                                pType = null;

                                if (Succeeded(hr))
                                {
                                    // Found an output type.
                                    break;
                                }
                            }
                            finally
                            {
                                SafeRelease(pType);
                            }
                        }
                    }

                    if (Succeeded(hr))
                    {
                        // Ask for the first sample.
                        hr = m_pReader.ReadSample((int)MF_SOURCE_READER.FirstVideoStream, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                    }

                    if (Failed(hr))
                    {
                        if (pSource != null)
                        {
                            pSource.Shutdown();

                            // NOTE: The source reader shuts down the media source
                            // by default, but we might not have gotten that far.
                        }
                        CloseDevice();
                    }
                }
                finally
                {
                    SafeRelease(pSource);
                    SafeRelease(pAttributes);
                }
            }

            return hr;
        }

        //-------------------------------------------------------------------
        //  CloseDevice
        //
        //  Releases all resources held by this object.
        //-------------------------------------------------------------------

        public int CloseDevice()
        {
            lock (this)
            {
                SafeRelease(m_pReader);
                m_pReader = null;

                m_pwszSymbolicLink = null;
            }

            return S_Ok;
        }

        //-------------------------------------------------------------------
        //  ResizeVideo
        //  Resizes the video rectangle.
        //
        //  The application should call this method if the size of the video
        //  window changes; e.g., when the application receives WM_SIZE.
        //-------------------------------------------------------------------

        public int ResizeVideo()
        {
            int hr = S_Ok;

            lock (this)
            {
                hr = m_draw.ResetDevice();
            }

            return hr;
        }

        public bool CheckDeviceLost(string sName)
        {
            return string.Compare(sName, m_pwszSymbolicLink, true) == 0;
        }

        public void Dispose()
        {
            CloseDevice();

            m_draw.DestroyDevice();
            m_draw = null;

            GC.SuppressFinalize(this);
        }

        #endregion

        #region Protected Methods

        // NotifyState: Notifies the application when an error occurs.
        protected void NotifyError(int hr)
        {
            TRACE("NotifyError: 0x" + hr.ToString("X"));
            PostMessage(m_hwndEvent, WM_APP_PREVIEW_ERROR, new IntPtr(hr), IntPtr.Zero);
        }

        //-------------------------------------------------------------------
        // TryMediaType
        //
        // Test a proposed video format.
        //-------------------------------------------------------------------
        protected int TryMediaType(IMFMediaType pType)
        {
            int hr = S_Ok;

            bool bFound = false;
            Guid subtype;

            hr = pType.GetGUID(MFAttributesClsid.MF_MT_SUBTYPE, out subtype);
            if (Failed(hr))
            {
                return hr;
            }

            // Do we support this type directly?
            if (m_draw.IsFormatSupported(subtype))
            {
                bFound = true;
            }
            else
            {
                // Can we decode this media type to one of our supported
                // output formats?

                for (int i = 0; ; i++)
                {
                    // Get the i'th format.
                    hr = m_draw.GetFormat(i, out subtype);
                    if (Failed(hr)) { break; }

                    hr = pType.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, subtype);
                    if (Failed(hr)) { break; }

                    // Try to set this type on the source reader.
                    hr = m_pReader.SetCurrentMediaType((int)MF_SOURCE_READER.FirstVideoStream, IntPtr.Zero, pType);
                    if (Succeeded(hr))
                    {
                        bFound = true;
                        break;
                    }
                }
            }

            if (bFound)
            {
                hr = m_draw.SetVideoType(pType);
            }

            return hr;
        }

        #endregion

        #region IMFSourceReaderCallback Members

        // IMFSourceReaderCallback methods

        //-------------------------------------------------------------------
        // OnReadSample
        //
        // Called when the IMFMediaSource::ReadSample method completes.
        //-------------------------------------------------------------------
        public int OnReadSample(int hrStatus, int dwStreamIndex, MF_SOURCE_READER_FLAG dwStreamFlags, long llTimestamp, IMFSample pSample)
        {
            int hr = hrStatus;
            IMFMediaBuffer pBuffer = null;

            lock (this)
            {
                try
                {
                    if (Succeeded(hr))
                    {
                        if (pSample != null)
                        {
                            // Get the video frame buffer from the sample.
                            hr = pSample.GetBufferByIndex(0, out pBuffer);

                            // Draw the frame.

                            if (Succeeded(hr))
                            {
                                hr = m_draw.DrawFrame(pBuffer);
                            }
                        }
                    }

                    // Request the next frame.
                    if (Succeeded(hr))
                    {
                        // Ask for the first sample.
                        hr = m_pReader.ReadSample((int)MF_SOURCE_READER.FirstVideoStream, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                    }

                    if (Failed(hr))
                    {
                        NotifyError(hr);
                    }
                }
                finally
                {
                    //SafeRelease(pBuffer);
                    SafeRelease(pSample);
                }
            }

            return hr;
        }

        public int OnEvent(int dwStreamIndex, IMFMediaEvent pEvent)
        {
            return S_Ok;
        }

        public int OnFlush(int dwStreamIndex)
        {
            return S_Ok;
        }

        #endregion
    }

}