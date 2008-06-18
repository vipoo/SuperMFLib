/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/

using System;
using System.Collections;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

using MediaFoundation;
using MediaFoundation.EVR;
using MediaFoundation.Misc;

namespace EVRPresenter
{
    public class D3DPresentEngine : COMBase, IDisposable
    {
        const int PRESENTER_BUFFER_COUNT = 3;

        int m_DeviceResetToken;     // Reset token for the D3D device manager.

        IntPtr m_hwnd;                 // Application-provided destination window.
        RECT m_rcDestRect;           // Destination rectangle.
        D3DDISPLAYMODE m_DisplayMode;          // Adapter's display mode.

        // COM interfaces
        IDirect3D9Ex m_pD3D9;
        IDirect3DDevice9Ex m_pDevice;
        IDirect3DDeviceManager9 m_pDeviceManager;        // Direct3D device manager.
        IDirect3DSurface9 m_pSurfaceRepaint;       // Surface for repaint requests.

        public D3DPresentEngine()
        {
            m_hwnd = IntPtr.Zero;
            m_DeviceResetToken = 0;
            m_pD3D9 = null;
            m_pDevice = null;
            m_pDeviceManager = null;
            m_pSurfaceRepaint = null;

            m_rcDestRect = new RECT();
            //m_rcDestRect.Empty();

            InitializeD3D();

            CreateD3DDevice();
        }


        //-----------------------------------------------------------------------------
        // Destructor
        //-----------------------------------------------------------------------------

        ~D3DPresentEngine()
        {
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
                    throw new COMException("D3DPresentEngine::GetService", MFError.MF_E_UNSUPPORTED_SERVICE);
                }
                else
                {
                    ppv = m_pDeviceManager;
                }
            }
            else
            {
                throw new COMException("D3DPresentEngine::GetService 2", MFError.MF_E_UNSUPPORTED_SERVICE);
            }
        }

        public RECT GetDestinationRect() { return m_rcDestRect; }
        public IntPtr GetVideoWindow() { return m_hwnd; }
        public int RefreshRate() { return m_DisplayMode.RefreshRate; }



        //-----------------------------------------------------------------------------
        // CheckFormat
        //
        // Queries whether the D3DPresentEngine can use a specified Direct3D format.
        //-----------------------------------------------------------------------------

        public void CheckFormat(D3DFORMAT format)
        {
            int uAdapter = 0;
            D3DDEVTYPE type = D3DDEVTYPE.D3DDEVTYPE_HAL;

            D3DDISPLAYMODE mode;
            D3DDEVICE_CREATION_PARAMETERS dparams;

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

        public void SetDestinationRect(RECT rcDest)
        {
            if (!Utils.EqualRect(rcDest, m_rcDestRect))
            {
                lock (this)
                {
                    m_rcDestRect = rcDest;

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
            Queue videoSampleQueue
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

                        SafeRelease(pVideoSample);
                        SafeRelease(pSwapChain);
                    }

                    // Let the derived class create any additional D3D resources that it needs.
                    OnCreateVideoSamples(pp);
                }
                catch
                {
                    ReleaseResources();
                }
                finally
                {
                    SafeRelease(pSwapChain);
                    SafeRelease(pVideoSample);
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
            // Let the derived class release any resources it created.
            OnReleaseResources();

            SafeRelease(m_pSurfaceRepaint);
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
                    case (int)D3DError.S_PRESENT_OCCLUDED:
                    case (int)D3DError.S_PRESENT_MODE_CHANGED:
                        // state is DeviceOK
                        break;

                    case (int)D3DError.D3DERR_DEVICELOST:
                    case (int)D3DError.D3DERR_DEVICEHUNG:
                        // Lost/hung device. Destroy the device and create a new one.
                        CreateD3DDevice();
                        pState = DeviceState.DeviceReset;
                        break;

                    case (int)D3DError.D3DERR_DEVICEREMOVED:
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
        //           this parameter is null, the method paints a black rectangle.
        // llTarget: Target presentation time.
        //
        // This method is called by the scheduler and/or the presenter.
        //-----------------------------------------------------------------------------

        public void PresentSample(IMFSample pSample, Int64 llTarget)
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
                    m_pSurfaceRepaint = pSurface;
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
                if (hr == (int)D3DError.D3DERR_DEVICELOST || hr == (int)D3DError.D3DERR_DEVICENOTRESET || hr == (int)D3DError.D3DERR_DEVICEHUNG)
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
                SafeRelease(pSwapChain);
                SafeRelease(pSurface);
                SafeRelease(pBuffer);
            }
        }



        //-----------------------------------------------------------------------------
        // private/protected methods
        //-----------------------------------------------------------------------------


        //-----------------------------------------------------------------------------
        // InitializeD3D
        // 
        // Initializes Direct3D and the Direct3D device manager.
        //-----------------------------------------------------------------------------

        public void InitializeD3D()
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

        public void CreateD3DDevice()
        {
            IntPtr hwnd = IntPtr.Zero;
            IntPtr hMonitor = IntPtr.Zero;
            int uAdapterID = 0;
            D3DCREATE vp = 0;

            D3DCAPS9 ddCaps;
            //ZeroMemory(ddCaps, sizeof(ddCaps));

            IDirect3DDevice9Ex pDevice = null;

            // Hold the lock because we might be discarding an exisiting device.
            lock (this)
            {
                if ((m_pD3D9 == null) || (m_pDeviceManager == null))
                {
                    throw new COMException("D3DPresentEngine::CreateD3DDevice", MFError.MF_E_NOT_INITIALIZED);
                }

                hwnd = Extern.GetDesktopWindow();

                // Note: The presenter creates additional swap chains to present the
                // video frames. Therefore, it does not use the device's implicit 
                // swap chain, so the size of the back buffer here is 1 x 1.

                D3DPRESENT_PARAMETERS pp = new D3DPRESENT_PARAMETERS();
                //ZeroMemory(pp, sizeof(pp));

                pp.BackBufferWidth = 1;
                pp.BackBufferHeight = 1;
                pp.Windowed = true;
                pp.SwapEffect = D3DSWAPEFFECT.D3DSWAPEFFECT_COPY;
                pp.BackBufferFormat = D3DFORMAT.D3DFMT_UNKNOWN;
                pp.hDeviceWindow = hwnd;
                pp.Flags = D3DPRESENTFLAG.VIDEO;
                pp.PresentationInterval = D3DPRESENT_INTERVAL.DEFAULT;

                // Find the monitor for this window.
                if (m_hwnd != IntPtr.Zero)
                {
                    hMonitor = Extern.MonitorFromWindow(m_hwnd, MonitorFlags.DEFAULTTONEAREST);

                    // Find the corresponding adapter.
                    FindAdapter(m_pD3D9 as IDirect3D9, hMonitor, out uAdapterID);
                }

                // Get the device caps for this adapter.
                m_pD3D9.GetDeviceCaps(uAdapterID, D3DDEVTYPE.D3DDEVTYPE_HAL, out ddCaps);

                if ((ddCaps.DevCaps & D3DDEVCAPS.HWTRANSFORMANDLIGHT) > 0)
                {
                    vp = D3DCREATE.HARDWARE_VERTEXPROCESSING;
                }
                else
                {
                    vp = D3DCREATE.SOFTWARE_VERTEXPROCESSING;
                }

                // Create the device.
                m_pD3D9.CreateDeviceEx(
                    uAdapterID,
                    D3DDEVTYPE.D3DDEVTYPE_HAL,
                    pp.hDeviceWindow,
                    vp | D3DCREATE.NOWINDOWCHANGES | D3DCREATE.MULTITHREADED | D3DCREATE.FPU_PRESERVE,
                    ref pp,
                    null,
                    out pDevice
                    );

                // Get the adapter display mode.
                m_pD3D9.GetAdapterDisplayMode(uAdapterID, out m_DisplayMode);

                // Reset the D3DDeviceManager with the new device 
                m_pDeviceManager.ResetDevice(pDevice, m_DeviceResetToken);

                SafeRelease(m_pDevice);

                m_pDevice = pDevice;

                //SafeRelease(pDevice);
            }
        }


        //-----------------------------------------------------------------------------
        // CreateD3DSample
        //
        // Creates an sample object (IMFSample) to hold a Direct3D swap chain.
        //-----------------------------------------------------------------------------

        public void CreateD3DSample(IDirect3DSwapChain9 pSwapChain, out IMFSample ppVideoSample)
        {
            IDirect3DSurface9 pSurface = null;
            IMFSample pSample = null;

            // Caller holds the object lock.
            try
            {
                int clrBlack = Utils.D3DCOLOR_ARGB(0xFF, 0x00, 0x00, 0x00);

                // Get the back buffer surface.
                pSwapChain.GetBackBuffer(0, D3DBACKBUFFER_TYPE.D3DBACKBUFFER_TYPE_MONO, out pSurface);

                // Fill it with black.
                m_pDevice.ColorFill(pSurface, null, clrBlack);

                // Create the sample.
                MFExtern.MFCreateVideoSampleFromSurface(pSurface, out pSample);

                // Return the pointer to the caller.
                ppVideoSample = pSample;

            }
            finally
            {
                SafeRelease(pSurface);
                SafeRelease(pSample);
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

        public void PresentSwapChain(IDirect3DSwapChain9 pSwapChain, IDirect3DSurface9 pSurface)
        {
            if (m_hwnd == IntPtr.Zero)
            {
                throw new COMException("D3DPresentEngine::PresentSwapChain", MFError.MF_E_INVALIDREQUEST);
            }

            pSwapChain.Present(null, m_rcDestRect, m_hwnd, null, 0);
        }

        //-----------------------------------------------------------------------------
        // PaintFrameWithGDI
        // 
        // Fills the destination rectangle with black.
        //-----------------------------------------------------------------------------

        public void PaintFrameWithGDI()
        {
            IntPtr hdc = Extern.GetDC(m_hwnd);

            if (hdc != IntPtr.Zero)
            {
                IntPtr hBrush = Extern.CreateSolidBrush(Utils.RGB(0, 0, 0));

                if (hBrush != IntPtr.Zero)
                {
                    Extern.FillRect(hdc, m_rcDestRect, hBrush);
                    Extern.DeleteObject(hBrush);
                }

                Extern.ReleaseDC(m_hwnd, hdc);
            }
        }


        //-----------------------------------------------------------------------------
        // GetSwapChainPresentParameters
        //
        // Given a media type that describes the video format, fills in the
        // D3DPRESENT_PARAMETERS for creating a swap chain.
        //-----------------------------------------------------------------------------

        public void GetSwapChainPresentParameters(IMFMediaType pType, out D3DPRESENT_PARAMETERS pPP)
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
                //ZeroMemory(pPP, Marshal.SizeOf(typeof(D3DPRESENT_PARAMETERS)));

                // Create the helper object for reading the proposed type.
                VideoTypeBuilder.Create(pType, out pTypeHelper);

                // Get some information about the video format.
                pTypeHelper.GetFrameDimensions(out width, out height);
                pTypeHelper.GetFourCC(out d3dFormat);

                pPP.Empty();
                pPP.BackBufferWidth = width;
                pPP.BackBufferHeight = height;
                pPP.Windowed = true;
                pPP.SwapEffect = D3DSWAPEFFECT.D3DSWAPEFFECT_COPY;
                pPP.BackBufferFormat = (D3DFORMAT)d3dFormat;
                pPP.hDeviceWindow = m_hwnd;
                pPP.Flags = D3DPRESENTFLAG.VIDEO;
                pPP.PresentationInterval = D3DPRESENT_INTERVAL.DEFAULT;

                D3DDEVICE_CREATION_PARAMETERS dparams;
                m_pDevice.GetCreationParameters(out dparams);

                if (dparams.DeviceType != D3DDEVTYPE.D3DDEVTYPE_HAL)
                {
                    pPP.Flags |= D3DPRESENTFLAG.LOCKABLE_BACKBUFFER;
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

        public void UpdateDestRect()
        {
            //if (m_hwnd == IntPtr.Zero)
            //{
            //    return S_False;
            //}


            RECT rcView;
            Extern.GetClientRect(m_hwnd, out rcView);

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

        //-----------------------------------------------------------------------------
        // Static functions
        //-----------------------------------------------------------------------------

        //-----------------------------------------------------------------------------
        // FindAdapter
        //
        // Given a handle to a monitor, returns the ordinal number that D3D uses to 
        // identify the adapter.
        //-----------------------------------------------------------------------------

        public void FindAdapter(IDirect3D9 pD3D9, IntPtr hMonitor, out int puAdapterID)
        {
            int cAdapters = 0;
            int uAdapterID = -1;

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
                    uAdapterID = i;
                    break;
                }
            }

            if (uAdapterID != -1)
            {
                puAdapterID = uAdapterID;
            }
            else
            {
                throw new COMException("D3DPresentEngine::FindAdapter", E_Fail);
            }
        }

        protected void OnCreateVideoSamples(D3DPRESENT_PARAMETERS pp)
        {
        }

        protected void OnReleaseResources()
        {
        }


        #region IDisposable Members

        public void Dispose()
        {
            SafeRelease(m_pDevice);
            SafeRelease(m_pSurfaceRepaint);
            SafeRelease(m_pDeviceManager);
            SafeRelease(m_pD3D9);

            m_pDevice = null;
            m_pSurfaceRepaint = null;
            m_pDeviceManager = null;
            m_pD3D9 = null;
        }

        #endregion
    }
}
