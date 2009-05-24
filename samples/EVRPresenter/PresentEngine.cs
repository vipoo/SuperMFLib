/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released
as public domain.  It is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;

using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.Utility;
using D3D;

namespace EVRPresenter
{
    public class D3DPresentEngine : COMBase, IDisposable
    {
        public const int PRESENTER_BUFFER_COUNT = 3;

        public enum DeviceState
        {
            DeviceOK,
            DeviceReset,    // The device was reset OR re-created.
            DeviceRemoved  // The device was removed.
        }

        #region Externs

        protected enum MonitorFlags
        {
            DefaultToNull = 0x00000000,
            DefaultToPrimary = 0x00000001,
            DefaultToNearest = 0x00000002
        }

        [DllImport("user32.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        protected static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        protected static extern IntPtr MonitorFromWindow(
          IntPtr hwnd,       // handle to a window
          MonitorFlags dwFlags    // determine return value
            );

        [DllImport("user32.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        protected static extern bool GetClientRect(
          IntPtr hwnd,       // handle to a window
          [Out] MFRect r    // determine return value
            );

        #endregion

        #region Member variables

        protected int m_DeviceResetToken;     // Reset token for the D3D device manager.

        protected int m_iFrames;

        protected IntPtr m_hwnd;                 // Application-provided destination window.
        protected MFRect m_rcDestRect;           // Destination rectangle.
        protected D3DDISPLAYMODE m_DisplayMode;          // Adapter's display mode.

        // COM interfaces
        protected IDirect3D9Ex m_pD3D9;
        protected IDirect3DDevice9Ex m_pDevice;
        protected IDirect3DDeviceManager9 m_pDeviceManager;        // Direct3D device manager.
        protected IDirect3DSurface9 m_pSurfaceRepaint;       // Surface for repaint requests.

        #endregion

        public D3DPresentEngine()
        {
            m_iFrames = 0;
            m_hwnd = IntPtr.Zero;
            m_DeviceResetToken = 0;
            m_pD3D9 = null;
            m_pDevice = null;
            m_pDeviceManager = null;
            m_pSurfaceRepaint = null;

            m_rcDestRect = new MFRect();
            m_DisplayMode = new D3DDISPLAYMODE();

            InitializeD3D();

            CreateD3DDevice();
        }

        ~D3DPresentEngine()
        {
            Dispose();
        }

        //-----------------------------------------------------------------------------
        // GetService
        //
        // Returns a service interface from the presenter engine.
        // The presenter calls this method from inside it's implementation of
        // IMFGetService::GetService.
        //
        // Classes that derive from D3DPresentEngine can override this method to return
        // other interfaces. If you override this method, call the base method from the
        // derived class.
        //-----------------------------------------------------------------------------

        public void GetService(Guid guidService, Guid riid, out object ppv)
        {
            if (riid == typeof(IDirect3DDeviceManager9).GUID)
            {
                if (m_pDeviceManager == null)
                {
                    throw new COMException("m_pDeviceManager not yet available", MFError.MF_E_UNSUPPORTED_SERVICE);
                }
                else
                {
                    ppv = m_pDeviceManager;
                }
            }
            else
            {
                throw new COMException("GetService requested unknown interface", MFError.MF_E_UNSUPPORTED_SERVICE);
            }
        }
        public MFRect GetDestinationRect() { return m_rcDestRect; }
        public IntPtr GetVideoWindow() { return m_hwnd; }
        public int RefreshRate() { return m_DisplayMode.RefreshRate; }

        //-----------------------------------------------------------------------------
        // CheckFormat
        //
        // Queries whether the D3DPresentEngine can use a specified Direct3D format.
        //-----------------------------------------------------------------------------

        public void CheckFormat(int iformat)
        {
            int uAdapter = 0;
            D3DDEVTYPE type = D3DDEVTYPE.HAL;

            D3DDISPLAYMODE mode;
            D3DDEVICE_CREATION_PARAMETERS dparams;

            D3DFORMAT format = (D3DFORMAT)iformat;

            if (m_pDevice != null)
            {
                m_pDevice.GetCreationParameters(out dparams);

                uAdapter = dparams.AdapterOrdinal;
                type = dparams.DeviceType;
            }

            m_pD3D9.GetAdapterDisplayMode(uAdapter, out mode);

            m_pD3D9.CheckDeviceType(uAdapter, type, mode.Format, format, true);
        }

        //-----------------------------------------------------------------------------
        // SetVideoWindow
        //
        // Sets the window where the video is drawn.
        //-----------------------------------------------------------------------------

        public void SetVideoWindow(IntPtr hwnd)
        {
            // Assertions: EVRCustomPresenter checks these cases.
            Debug.Assert(hwnd != IntPtr.Zero);
            Debug.Assert(hwnd != m_hwnd);

            lock (this)
            {
                m_hwnd = hwnd;

                UpdateDestRect();

                // Recreate the device.
                CreateD3DDevice();
            }
        }

        //-----------------------------------------------------------------------------
        // SetDestinationRect
        //
        // Sets the region within the video window where the video is drawn.
        //-----------------------------------------------------------------------------

        public void SetDestinationRect(MFRect rcDest)
        {
            if (!m_rcDestRect.Equals(rcDest))
            {
                lock (this)
                {
                    m_rcDestRect.left = rcDest.left;
                    m_rcDestRect.right = rcDest.right;
                    m_rcDestRect.top = rcDest.top;
                    m_rcDestRect.bottom = rcDest.bottom;

                    UpdateDestRect();
                }
            }
        }

        //-----------------------------------------------------------------------------
        // CreateVideoSamples
        //
        // Creates video samples based on a specified media type.
        //
        // pFormat: Media type that describes the video format.
        // videoSampleQueue: List that will contain the video samples.
        //
        // Note: For each video sample, the method creates a swap chain with a
        // single back buffer. The video sample object holds a pointer to the swap
        // chain's back buffer surface. The mixer renders to this surface, and the
        // D3DPresentEngine renders the video frame by presenting the swap chain.
        //-----------------------------------------------------------------------------

        public void CreateVideoSamples(
            IMFMediaType pFormat,
            Queue<IMFSample> videoSampleQueue
            )
        {
            if (m_hwnd == IntPtr.Zero)
            {
                throw new COMException("D3DPresentEngine::CreateVideoSamples", MFError.MF_E_INVALIDREQUEST);
            }

            if (pFormat == null)
            {
                throw new COMException("D3DPresentEngine::CreateVideoSamples", MFError.MF_E_UNEXPECTED);
            }

            D3DPRESENT_PARAMETERS pp;

            IDirect3DSwapChain9 pSwapChain = null;    // Swap chain
            IMFSample pVideoSample = null;            // Sampl

            lock (this)
            {
                ReleaseResources();

                try
                {
                    // Get the swap chain parameters from the media type.
                    GetSwapChainPresentParameters(pFormat, out pp);

                    UpdateDestRect();

                    // Create the video samples.
                    for (int i = 0; i < PRESENTER_BUFFER_COUNT; i++)
                    {
                        // Create a new swap chain.
                        m_pDevice.CreateAdditionalSwapChain(pp, out pSwapChain);

                        // Create the video sample from the swap chain.
                        CreateD3DSample(pSwapChain, out pVideoSample);

                        // Add it to the list.
                        videoSampleQueue.Enqueue(pVideoSample);

                        // Set the swap chain pointer as a custom attribute on the sample. This keeps
                        // a reference count on the swap chain, so that the swap chain is kept alive
                        // for the duration of the sample's lifetime.
                        pVideoSample.SetUnknown(EVRCustomPresenter.MFSamplePresenter_SampleSwapChain, pSwapChain);

                        //SafeRelease(pVideoSample);
                        SafeRelease(pSwapChain); pSwapChain = null;
                    }
                }
                catch
                {
                    ReleaseResources();
                }
                finally
                {
                    SafeRelease(pSwapChain); pSwapChain = null;
                    //SafeRelease(pVideoSample);
                }
            }
        }

        //-----------------------------------------------------------------------------
        // ReleaseResources
        //
        // Released Direct3D resources used by this object.
        //-----------------------------------------------------------------------------

        public void ReleaseResources()
        {
            SafeRelease(m_pSurfaceRepaint); m_pSurfaceRepaint = null;
            m_iFrames = 0;
        }

        //-----------------------------------------------------------------------------
        // CheckDeviceState
        //
        // Tests the Direct3D device state.
        //
        // pState: Receives the state of the device (OK, reset, removed)
        //-----------------------------------------------------------------------------

        public void CheckDeviceState(out DeviceState pState)
        {
            lock (this)
            {
                // Check the device state. Not every failure code is a critical failure.
                int hr = m_pDevice.CheckDeviceState(m_hwnd);

                pState = DeviceState.DeviceOK;

                switch (hr)
                {
                    case S_Ok:
                    case (int)D3DError.S_PresentOccluded:
                    case (int)D3DError.S_PresentModeChanged:
                        // state is DeviceOK
                        break;

                    case (int)D3DError.DeviceLost:
                    case (int)D3DError.DeviceHung:
                        // Lost/hung device. Destroy the device and create a new one.
                        CreateD3DDevice();
                        pState = DeviceState.DeviceReset;
                        break;

                    case (int)D3DError.DeviceRemoved:
                        // This is a fatal error.
                        pState = DeviceState.DeviceRemoved;
                        break;

                    case E_InvalidArgument:
                        // CheckDeviceState can return E_INVALIDARG if the window is not valid
                        // We'll assume that the window was destroyed; we'll recreate the device
                        // if the application sets a new window.
                        break;

                    default:
                        throw new COMException("D3DPresentEngine::CheckDeviceState", hr);
                }

            }
        }

        //-----------------------------------------------------------------------------
        // PresentSample
        //
        // Presents a video frame.
        //
        // pSample:  Pointer to the sample that contains the surface to present. If
        //           this parameter is NULL, the method paints a black rectangle.
        // llTarget: Target presentation time.
        //
        // This method is called by the scheduler and/or the presenter.
        //-----------------------------------------------------------------------------

        public void PresentSample(IMFSample pSample, long llTarget)
        {
            IMFMediaBuffer pBuffer = null;
            IDirect3DSurface9 pSurface = null;
            IDirect3DSwapChain9 pSwapChain = null;
            object o;

            try
            {
                if (pSample != null)
                {
                    // Get the buffer from the sample.
                    pSample.GetBufferByIndex(0, out pBuffer);

                    // Get the surface from the buffer.
                    MFExtern.MFGetService(pBuffer, MFServices.MR_BUFFER_SERVICE, typeof(IDirect3DSurface9).GUID, out o);
                    pSurface = o as IDirect3DSurface9;
                }
                else if (m_pSurfaceRepaint != null)
                {
                    // Redraw from the last surface.
                    pSurface = m_pSurfaceRepaint;
                }

                if (pSurface != null)
                {
                    // Get the swap chain from the surface.
                    pSurface.GetContainer(typeof(IDirect3DSwapChain9).GUID, out o);
                    pSwapChain = o as IDirect3DSwapChain9;

                    // Present the swap chain.
                    PresentSwapChain(pSwapChain, pSurface);

                    // Store this pointer in case we need to repaint the surface.
                    if (m_pSurfaceRepaint != pSurface)
                    {
                        SafeRelease(m_pSurfaceRepaint);
                        m_pSurfaceRepaint = pSurface;
                    }
                }
                else
                {
                    // No surface. All we can do is paint a black rectangle.
                    PaintFrameWithGDI();
                }
            }
            catch (Exception e)
            {
                int hr = Marshal.GetHRForException(e);
                if (hr == (int)D3DError.DeviceLost || hr == (int)D3DError.DeviceNotReset || hr == (int)D3DError.DeviceHung)
                {
                    // We failed because the device was lost. Fill the destination rectangle.
                    PaintFrameWithGDI();

                    // Ignore. We need to reset or re-create the device, but this method
                    // is probably being called from the scheduler thread, which is not the
                    // same thread that created the device. The Reset(Ex) method must be
                    // called from the thread that created the device.

                    // The presenter will detect the state when it calls CheckDeviceState()
                    // on the next sample.
                }
            }
            finally
            {
                SafeRelease(pSwapChain); pSwapChain = null;
                //SafeRelease(pSurface); pSurface = null;
                SafeRelease(pBuffer); pBuffer = null;
            }
        }

        public int GetFrames() { return m_iFrames; }

        public void GetDeviceID(out Guid pDeviceID)
        {
            // This presenter is built on Direct3D9, so the device ID is
            // IID_IDirect3DDevice9. (Same as the standard presenter.)
            pDeviceID = typeof(IDirect3DDevice9).GUID; // IID_IDirect3DDevice9;
        }

        #region private/protected methods

        //-----------------------------------------------------------------------------
        // InitializeD3D
        //
        // Initializes Direct3D and the Direct3D device manager.
        //-----------------------------------------------------------------------------

        protected void InitializeD3D()
        {
            Debug.Assert(m_pD3D9 == null);
            Debug.Assert(m_pDeviceManager == null);

            // Create Direct3D
            D3DExtern.Direct3DCreate9Ex(D3DExtern.D3D_SDK_VERSION, out m_pD3D9);

            // Create the device manager
            D3DExtern.DXVA2CreateDirect3DDeviceManager9(out m_DeviceResetToken, out m_pDeviceManager);
        }

        //-----------------------------------------------------------------------------
        // CreateD3DDevice
        //
        // Creates the Direct3D device.
        //-----------------------------------------------------------------------------

        protected void CreateD3DDevice()
        {
            IntPtr hwnd = IntPtr.Zero;
            IntPtr hMonitor = IntPtr.Zero;
            int uAdapterID = 0;
            D3DCREATE vp = 0;

            D3DCAPS9 ddCaps;

            IDirect3DDevice9Ex pDevice = null;

            // Hold the lock because we might be discarding an exisiting device.
            lock (this)
            {
                if ((m_pD3D9 == null) || (m_pDeviceManager == null))
                {
                    throw new COMException("D3DPresentEngine::CreateD3DDevice", MFError.MF_E_NOT_INITIALIZED);
                }

                hwnd = GetDesktopWindow();

                // Note: The presenter creates additional swap chains to present the
                // video frames. Therefore, it does not use the device's implicit
                // swap chain, so the size of the back buffer here is 1 x 1.

                D3DPRESENT_PARAMETERS pp = new D3DPRESENT_PARAMETERS();

                pp.BackBufferWidth = 1;
                pp.BackBufferHeight = 1;
                pp.Windowed = true;
                pp.SwapEffect = D3DSWAPEFFECT.Copy;
                pp.BackBufferFormat = D3DFORMAT.Unknown;
                pp.hDeviceWindow = hwnd;
                pp.Flags = D3DPRESENTFLAG.Video;
                pp.PresentationInterval = D3DPRESENT_INTERVAL.Default;

                // Find the monitor for this window.
                if (m_hwnd != IntPtr.Zero)
                {
                    hMonitor = MonitorFromWindow(m_hwnd, MonitorFlags.DefaultToNearest);

                    // Find the corresponding adapter.
                    FindAdapter(m_pD3D9 as IDirect3D9, hMonitor, out uAdapterID);
                }

                // Get the device caps for this adapter.
                m_pD3D9.GetDeviceCaps(uAdapterID, D3DDEVTYPE.HAL, out ddCaps);

                if ((ddCaps.DevCaps & D3DDEVCAPS.HWTRANSFORMANDLIGHT) > 0)
                {
                    vp = D3DCREATE.HardwareVertexProcessing;
                }
                else
                {
                    vp = D3DCREATE.SoftwareVertexProcessing;
                }

                // Create the device.
                m_pD3D9.CreateDeviceEx(
                    uAdapterID,
                    D3DDEVTYPE.HAL,
                    pp.hDeviceWindow,
                    vp | D3DCREATE.NoWindowChanges | D3DCREATE.MultiThreaded | D3DCREATE.FPU_PRESERVE,
                    pp,
                    null,
                    out pDevice
                    );

                // Get the adapter display mode.
                m_pD3D9.GetAdapterDisplayMode(uAdapterID, out m_DisplayMode);

                // Reset the D3DDeviceManager with the new device
                m_pDeviceManager.ResetDevice(pDevice, m_DeviceResetToken);

                if (m_pDevice != pDevice)
                {
                    SafeRelease(m_pDevice);

                    m_pDevice = pDevice;
                }

                //SafeRelease(pDevice);
            }
        }

        //-----------------------------------------------------------------------------
        // CreateD3DSample
        //
        // Creates an sample object (IMFSample) to hold a Direct3D swap chain.
        //-----------------------------------------------------------------------------

        protected void CreateD3DSample(IDirect3DSwapChain9 pSwapChain, out IMFSample ppVideoSample)
        {
            IDirect3DSurface9 pSurface = null;

            // Caller holds the object lock.
            try
            {
                // Get the back buffer surface.
                pSwapChain.GetBackBuffer(0, D3DBACKBUFFER_TYPE.Mono, out pSurface);

                // Create the sample.
                MFExtern.MFCreateVideoSampleFromSurface(pSurface, out ppVideoSample);
            }
            finally
            {
                SafeRelease(pSurface); pSurface = null;
            }
        }

        //-----------------------------------------------------------------------------
        // PresentSwapChain
        //
        // Presents a swap chain that contains a video frame.
        //
        // pSwapChain: Pointer to the swap chain.
        // pSurface: Pointer to the swap chain's back buffer surface.

        //
        // Note: This method simply calls IDirect3DSwapChain9::Present, but a derived
        // class could do something fancier.
        //-----------------------------------------------------------------------------

        protected void PresentSwapChain(IDirect3DSwapChain9 pSwapChain, IDirect3DSurface9 pSurface)
        {
            if (m_hwnd == IntPtr.Zero)
            {
                throw new COMException("D3DPresentEngine::PresentSwapChain", MFError.MF_E_INVALIDREQUEST);
            }

            pSwapChain.Present(null, m_rcDestRect, m_hwnd, null, 0);
            m_iFrames++;
        }

        //-----------------------------------------------------------------------------
        // PaintFrameWithGDI
        //
        // Fills the destination rectangle with black.
        //-----------------------------------------------------------------------------

        protected void PaintFrameWithGDI()
        {
            Graphics g = Graphics.FromHwnd(m_hwnd);

            try
            {
                g.FillRectangle(Brushes.Black, m_rcDestRect);
            }
            finally
            {
                g.Dispose();
            }
        }

        //-----------------------------------------------------------------------------
        // GetSwapChainPresentParameters
        //
        // Given a media type that describes the video format, fills in the
        // D3DPRESENT_PARAMETERS for creating a swap chain.
        //-----------------------------------------------------------------------------

        protected void GetSwapChainPresentParameters(IMFMediaType pType, out D3DPRESENT_PARAMETERS pPP)
        {
            pPP = new D3DPRESENT_PARAMETERS();
            // Caller holds the object lock.

            int width = 0, height = 0;
            int d3dFormat = 0;

            VideoTypeBuilder pTypeHelper = null;

            if (m_hwnd == IntPtr.Zero)
            {
                throw new COMException("D3DPresentEngine::GetSwapChainPresentParameters", MFError.MF_E_INVALIDREQUEST);
            }

            try
            {
                // Create the helper object for reading the proposed type.
                pTypeHelper = new VideoTypeBuilder(pType);

                // Get some information about the video format.
                pTypeHelper.GetFrameDimensions(out width, out height);
                pTypeHelper.GetFourCC(out d3dFormat);

                pPP.BackBufferWidth = width;
                pPP.BackBufferHeight = height;
                pPP.Windowed = true;
                pPP.SwapEffect = D3DSWAPEFFECT.Copy;
                pPP.BackBufferFormat = (D3DFORMAT)d3dFormat;
                pPP.hDeviceWindow = m_hwnd;
                pPP.Flags = D3DPRESENTFLAG.Video;
                pPP.PresentationInterval = D3DPRESENT_INTERVAL.Default;

                D3DDEVICE_CREATION_PARAMETERS dparams;
                m_pDevice.GetCreationParameters(out dparams);

                if (dparams.DeviceType != D3DDEVTYPE.HAL)
                {
                    pPP.Flags |= D3DPRESENTFLAG.LockableBackbuffer;
                }
            }
            catch { }

            //SafeRelease(pTypeHelper);
        }

        //-----------------------------------------------------------------------------
        // UpdateDestRect
        //
        // Updates the target rectangle by clipping it to the video window's client
        // area.
        //
        // Called whenever the application sets the video window or the destination
        // rectangle.
        //-----------------------------------------------------------------------------

        protected void UpdateDestRect()
        {
            if (m_hwnd != IntPtr.Zero)
            {
                MediaFoundation.Misc.MFRect rcView = new MFRect();
                GetClientRect(m_hwnd, rcView);

                // Clip the destination rectangle to the window's client area.
                if (m_rcDestRect.right > rcView.right)
                {
                    m_rcDestRect.right = rcView.right;
                }

                if (m_rcDestRect.bottom > rcView.bottom)
                {
                    m_rcDestRect.bottom = rcView.bottom;
                }
            }
        }

        //-----------------------------------------------------------------------------
        // Static functions
        //-----------------------------------------------------------------------------

        //-----------------------------------------------------------------------------
        // FindAdapter
        //
        // Given a handle to a monitor, returns the ordinal number that D3D uses to
        // identify the adapter.
        //-----------------------------------------------------------------------------

        protected static void FindAdapter(IDirect3D9 pD3D9, IntPtr hMonitor, out int puAdapterID)
        {
            int cAdapters = 0;
            puAdapterID = -1;

            cAdapters = pD3D9.GetAdapterCount();
            for (int i = 0; i < cAdapters; i++)
            {
                IntPtr hMonitorTmp = pD3D9.GetAdapterMonitor(i);

                if (hMonitorTmp == IntPtr.Zero)
                {
                    break;
                }
                if (hMonitorTmp == hMonitor)
                {
                    puAdapterID = i;
                    break;
                }
            }

            if (puAdapterID == -1)
            {
                throw new COMException("D3DPresentEngine::FindAdapter", E_Fail);
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            SafeRelease(m_pDevice); m_pDevice = null;
            SafeRelease(m_pSurfaceRepaint); m_pSurfaceRepaint = null;
            SafeRelease(m_pDeviceManager); m_pDeviceManager = null;
            SafeRelease(m_pD3D9); m_pD3D9 = null;
        }

        #endregion
    }
}
