/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/

using System;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

using MediaFoundation;
using MediaFoundation.EVR;
using MediaFoundation.Misc;
using MediaFoundation.Transform;

using DirectShowLib;

namespace EVRPresenter
{
    [ComVisible(true),
   Guid("DD1BE413-E999-47f1-A107-9BC1F3DCB6C7"),
   ClassInterface(ClassInterfaceType.None)]
    public class EVRCustomPresenter : COMBase, IMFVideoDeviceID, IMFVideoPresenter, IMFClockStateSink, IMFRateSupport, IMFGetService, IMFTopologyServiceLookupClient, IMFVideoDisplayControl
    {
        #region Definitions

        const Int64 PRESENTATION_CURRENT_POSITION = 0x7fffffffffffffff;
        static Guid MFSamplePresenter_SampleCounter = Guid.NewGuid();
        public static Guid MFSamplePresenter_SampleSwapChain = new Guid(0xad885bd1, 0x7def, 0x414a, 0xb5, 0xb0, 0xd3, 0xd2, 0x63, 0xd6, 0xe9, 0x6d);


        static MFRatio g_DefaultFrameRate;

        protected enum RENDER_STATE
        {
            RENDER_STATE_STARTED = 1,
            RENDER_STATE_STOPPED,
            RENDER_STATE_PAUSED,
            RENDER_STATE_SHUTDOWN,  // Initial state. 

            // State transitions:

            // InitServicePointers                  -> STOPPED
            // ReleaseServicePointers               -> SHUTDOWN
            // IMFClockStateSink::OnClockStart      -> STARTED
            // IMFClockStateSink::OnClockRestart    -> STARTED
            // IMFClockStateSink::OnClockPause      -> PAUSED
            // IMFClockStateSink::OnClockStop       -> STOPPED
        }

        protected enum FRAMESTEP_STATE
        {
            FRAMESTEP_NONE,             // Not frame stepping.
            FRAMESTEP_WAITING_START,    // Frame stepping, but the clock is not started.
            FRAMESTEP_PENDING,          // Clock is started. Waiting for samples.
            FRAMESTEP_SCHEDULED,        // Submitted a sample for rendering.
            FRAMESTEP_COMPLETE          // Sample was rendered. 

            // State transitions:

            // MFVP_MESSAGE_STEP                -> WAITING_START
            // OnClockStart/OnClockRestart      -> PENDING
            // MFVP_MESSAGE_PROCESSINPUTNOTIFY  -> SUBMITTED
            // OnSampleFree                     -> COMPLETE
            // MFVP_MESSAGE_CANCEL              -> NONE
            // OnClockStop                      -> NONE
            // OnClockSetRate( non-zero )       -> NONE
        }

        protected class FrameStep
        {
            public FrameStep()
            {
                state = FRAMESTEP_STATE.FRAMESTEP_NONE;
                steps = 0;
                pSampleNoRef = IntPtr.Zero;
                samples = new Queue();
            }

            public FRAMESTEP_STATE state;          // Current frame-step state.
            public Queue samples;        // List of pending samples for frame-stepping.
            public int steps;          // Number of steps left.
            public IntPtr pSampleNoRef;   // Identifies the frame-step sample.
        }

        #endregion

        public EVRCustomPresenter()
        {
            g_DefaultFrameRate.Denominator = 30;
            g_DefaultFrameRate.Numerator = 1;

            m_RenderState = RENDER_STATE.RENDER_STATE_SHUTDOWN;
            m_pD3DPresentEngine = null;
            m_pClock = null;
            m_pMixer = null;
            m_pMediaEventSink = null;
            m_pMediaType = null;
            m_bSampleNotify = false;
            m_bRepaint = false;
            m_bEndStreaming = false;
            m_bPrerolled = false;
            m_fRate = 1.0f;
            m_TokenCounter = 0;
            m_SampleFreeCB = new AsyncCallback(this);

            // Initial source rectangle = (0,0,1,1)
            m_nrcSource = new MFVideoNormalizedRect();
            m_nrcSource.top = 0;
            m_nrcSource.left = 0;
            m_nrcSource.bottom = 1;
            m_nrcSource.right = 1;

            m_pD3DPresentEngine = new D3DPresentEngine();

            m_scheduler = new Scheduler();			// Manages scheduling of samples.
            m_scheduler.SetCallback(m_pD3DPresentEngine);

            m_FrameStep = new FrameStep();            // Frame-stepping information.

            // Samples and scheduling
            m_SamplePool = new SamplePool();           // Pool of allocated samples.
        }

        ~EVRCustomPresenter()
        {
            SafeRelease(m_pClock);
            SafeRelease(m_pMixer);
            SafeRelease(m_pMediaEventSink);
            SafeRelease(m_pMediaType);

            // Deletable objects
            m_pD3DPresentEngine.Dispose(); m_pD3DPresentEngine = null;
        }

        #region IMFVideoDeviceID Members

        public void GetDeviceID(out Guid pDeviceID)
        {
            Guid IID_IDirect3DDevice9 = new Guid(0xd0223b96, 0xbf7a, 0x43fd, 0x92, 0xbd, 0xa4, 0x3b, 0xd, 0x82, 0xb9, 0xeb);
            // This presenter is built on Direct3D9, so the device ID is 
            // IID_IDirect3DDevice9. (Same as the standard presenter.)
            pDeviceID = IID_IDirect3DDevice9;
        }

        #endregion

        #region IMFVideoPresenter Members

        void IMFVideoPresenter.GetCurrentMediaType(out MediaFoundation.IMFVideoMediaType ppMediaType)
        {
            lock (this)
            {
                CheckShutdown();

                if (m_pMediaType == null)
                {
                    throw new COMException("IMFVideoPresenter.GetCurrentMediaType", MFError.MF_E_NOT_INITIALIZED);
                }

                // The function returns an IMFVideoMediaType pointer, and we store our media
                // type as an IMFMediaType pointer, so we need to QI.

                ppMediaType = (IMFVideoMediaType)m_pMediaType;
            }
        }

        void IMFVideoPresenter.OnClockPause(long hnsSystemTime)
        {
            ((IMFClockStateSink)this).OnClockPause(hnsSystemTime);
        }

        void IMFVideoPresenter.OnClockRestart(long hnsSystemTime)
        {
            ((IMFClockStateSink)this).OnClockRestart(hnsSystemTime);
        }

        void IMFVideoPresenter.OnClockSetRate(long hnsSystemTime, float flRate)
        {
            ((IMFClockStateSink)this).OnClockSetRate(hnsSystemTime, flRate);
        }

        void IMFVideoPresenter.OnClockStart(long hnsSystemTime, long llClockStartOffset)
        {
            ((IMFClockStateSink)this).OnClockStart(hnsSystemTime, llClockStartOffset);
        }

        void IMFVideoPresenter.OnClockStop(long hnsSystemTime)
        {
            ((IMFClockStateSink)this).OnClockStop(hnsSystemTime);
        }

        void IMFVideoPresenter.ProcessMessage(MFVPMessageType eMessage, IntPtr ulParam)
        {
            lock (this)
            {
                CheckShutdown();

                switch (eMessage)
                {
                    // Flush all pending samples.
                    case MFVPMessageType.Flush:
                        Flush();
                        break;

                    // Renegotiate the media type with the mixer.
                    case MFVPMessageType.InvalidateMediaType:
                        RenegotiateMediaType();
                        break;

                    // The mixer received a new input sample. 
                    case MFVPMessageType.ProcessInputNotify:
                        ProcessInputNotify();
                        break;

                    // Streaming is about to start.
                    case MFVPMessageType.BeginStreaming:
                        BeginStreaming();
                        break;

                    // Streaming has ended. (The EVR has stopped.)
                    case MFVPMessageType.EndStreaming:
                        EndStreaming();
                        break;

                    // All input streams have ended.
                    case MFVPMessageType.EndOfStream:
                        // Set the EOS flag. 
                        m_bEndStreaming = true;
                        // Check if it's time to send the EC_COMPLETE event to the EVR.
                        CheckEndOfStream();
                        break;

                    // Frame-stepping is starting.
                    case MFVPMessageType.Step:
                        PrepareFrameStep(ulParam.ToInt32());
                        break;

                    // Cancels frame-stepping.
                    case MFVPMessageType.CancelStep:
                        CancelFrameStep();
                        break;

                    default:
                        throw new COMException("ProcessMessage", E_InvalidArgument); // Unknown message. (This case should never occur.)
                }
            }
        }

        #endregion

        #region IMFClockStateSink Members

        void MediaFoundation.IMFClockStateSink.OnClockPause(long hnsSystemTime)
        {
            TRACE(("OnClockPause"));

            lock (this)
            {
                // We cannot pause the clock after shutdown.
                CheckShutdown();

                // Set the state. (No other actions are necessary.)
                m_RenderState = RENDER_STATE.RENDER_STATE_PAUSED;
            }
        }

        void MediaFoundation.IMFClockStateSink.OnClockRestart(long hnsSystemTime)
        {
            TRACE(("OnClockRestart"));

            lock (this)
            {
                CheckShutdown();

                // The EVR calls OnClockRestart only while paused.
                Debug.Assert(m_RenderState == RENDER_STATE.RENDER_STATE_PAUSED);

                m_RenderState = RENDER_STATE.RENDER_STATE_STARTED;

                // Possibly we are in the middle of frame-stepping OR we have samples waiting 
                // in the frame-step queue. Deal with these two cases first:
                StartFrameStep();

                // Now resume the presentation loop.
                ProcessOutputLoop();
            }
        }

        void MediaFoundation.IMFClockStateSink.OnClockSetRate(long hnsSystemTime, float flRate)
        {
            TRACE(string.Format("OnClockSetRate (rate={0})", flRate));

            // Note: 
            // The presenter reports its maximum rate through the IMFRateSupport interface.
            // Here, we assume that the EVR honors the maximum rate.

            lock (this)
            {
                CheckShutdown();

                // If the rate is changing from zero (scrubbing) to non-zero, cancel the 
                // frame-step operation.
                if ((m_fRate == 0.0f) && (flRate != 0.0f))
                {
                    CancelFrameStep();
                    m_FrameStep.samples.Clear();
                }

                m_fRate = flRate;

                // Tell the scheduler about the new rate.
                m_scheduler.SetClockRate(flRate);

            }
        }

        void MediaFoundation.IMFClockStateSink.OnClockStart(long hnsSystemTime, long llClockStartOffset)
        {
            TRACE(String.Format("OnClockStart (offset = %I64d)", llClockStartOffset));

            lock (this)
            {
                // We cannot start after shutdown.
                CheckShutdown();

                m_RenderState = RENDER_STATE.RENDER_STATE_STARTED;

                // Check if the clock is already active (not stopped). 
                if (IsActive())
                {
                    // If the clock position changes while the clock is active, it 
                    // is a seek request. We need to flush all pending samples.
                    if (llClockStartOffset != PRESENTATION_CURRENT_POSITION)
                    {
                        Flush();
                    }
                }
                else
                {
                    // The clock has started from the stopped state. 

                    // Possibly we are in the middle of frame-stepping OR have samples waiting 
                    // in the frame-step queue. Deal with these two cases first:
                    StartFrameStep();
                }

                // Now try to get new output samples from the mixer.
                ProcessOutputLoop();
            }
        }

        void MediaFoundation.IMFClockStateSink.OnClockStop(long hnsSystemTime)
        {
            TRACE(("OnClockStop"));

            lock (this)
            {
                CheckShutdown();

                if (m_RenderState != RENDER_STATE.RENDER_STATE_STOPPED)
                {
                    m_RenderState = RENDER_STATE.RENDER_STATE_STOPPED;
                    Flush();

                    // If we are in the middle of frame-stepping, cancel it now.
                    if (m_FrameStep.state != FRAMESTEP_STATE.FRAMESTEP_NONE)
                    {
                        CancelFrameStep();
                    }
                }
            }
        }

        #endregion

        #region IMFRateSupport Members

        public void GetFastestRate(MFRateDirection eDirection, bool fThin, out float pflRate)
        {
            lock (this)
            {
                float fMaxRate = 0.0f;

                CheckShutdown();

                // Get the maximum *forward* rate.
                fMaxRate = GetMaxRate(fThin);

                // For reverse playback, it's the negative of fMaxRate.
                if (eDirection == MFRateDirection.Reverse)
                {
                    fMaxRate = -fMaxRate;
                }

                pflRate = fMaxRate;
            }
        }

        public void GetSlowestRate(MFRateDirection eDirection, bool fThin, out float pflRate)
        {
            lock (this)
            {
                CheckShutdown();

                // There is no minimum playback rate, so the minimum is zero.
                pflRate = 0;
            }
        }

        public void IsRateSupported(bool fThin, float flRate, MfFloat pflNearestSupportedRate)
        {
            lock (this)
            {

                float fMaxRate = 0.0f;
                float fNearestRate = flRate;   // If we support fRate, then fRate *is* the nearest.

                CheckShutdown();

                // Find the maximum forward rate.
                // Note: We have no minimum rate (ie, we support anything down to 0).
                fMaxRate = GetMaxRate(fThin);

                if (Math.Abs(flRate) > fMaxRate)
                {
                    // The nearest supported rate is fMaxRate.
                    fNearestRate = fMaxRate;
                    if (flRate < 0)
                    {
                        // Negative for reverse playback.
                        fNearestRate = -fNearestRate;
                    }

                    // The (absolute) requested rate exceeds the maximum rate.
                    throw new COMException("IsRateSupported", MFError.MF_E_UNSUPPORTED_RATE);
                }

                // Return the nearest supported rate.
                if (pflNearestSupportedRate != null)
                {
                    pflNearestSupportedRate = fNearestRate;
                }

            }
        }

        #endregion

        #region IMFGetService Members

        public void GetService(Guid guidService, Guid riid, out object ppvObject)
        {
            ppvObject = null;
            int hr = 0;

            // The only service GUID that we support is MR_VIDEO_RENDER_SERVICE.
            if (guidService != MFServices.MR_VIDEO_RENDER_SERVICE)
            {
                throw new COMException("EVRCustomPresenter::GetService", MFError.MF_E_UNSUPPORTED_SERVICE);
            }

            bool bAgain = false;
            // First try to get the service interface from the D3DPresentEngine object.
            try
            {
                m_pD3DPresentEngine.GetService(guidService, riid, out ppvObject);
            }
            catch
            {
                bAgain = true;
            }

            if (bAgain)
            {
                // Next, QI to check if this object supports the interface.
                IntPtr ppv;
                IntPtr ipThis = Marshal.GetIUnknownForObject(this);

                try
                {
                    hr = Marshal.QueryInterface(ipThis, ref riid, out ppv);

                    if (hr >= 0)
                    {
                        Marshal.Release(ipThis); // Release QueryInterface
                        ppvObject = Marshal.GetObjectForIUnknown(ppv); // Includes AddRef
                    }
                    if (hr < 0)
                    {
                        Marshal.ThrowExceptionForHR(hr);
                    }
                }
                finally
                {
                    Marshal.Release(ipThis); // Release GetIUnknownForObject
                }

            }
            //return hr;
        }

        #endregion

        #region IMFTopologyServiceLookupClient Members

        public void InitServicePointers(IMFTopologyServiceLookup pLookup)
        {
            TRACE(("InitServicePointers"));
            if (pLookup == null)
            {
                throw new COMException("EVRCustomPresenter::InitServicePointers", E_Pointer);
            }

            int dwObjectCount = 0;

            lock (this)
            {

                // Do not allow initializing when playing or paused.
                if (IsActive())
                {
                    throw new COMException("EVRCustomPresenter::InitServicePointers", MFError.MF_E_INVALIDREQUEST);
                }

                SafeRelease(m_pClock);
                SafeRelease(m_pMixer);
                SafeRelease(m_pMediaEventSink);

                // Ask for the clock. Optional, because the EVR might not have a clock.
                dwObjectCount = 1;
                object[] o = new object[1];

                pLookup.LookupService(
                    MFServiceLookupType.Global,   // Not used.
                    0,                          // Reserved.
                    MFServices.MR_VIDEO_RENDER_SERVICE,    // Service to look up.
                    typeof(IMFClock).GUID,         // Interface to look up.
                    out o,
                    ref dwObjectCount              // Number of elements in the previous parameter.
                    );
                m_pClock = o[0] as IMFClock;

                // Ask for the mixer. (Required.)
                dwObjectCount = 1;

                pLookup.LookupService(
                    MFServiceLookupType.Global,
                    0,
                    MFServices.MR_VIDEO_MIXER_SERVICE,
                    typeof(IMFTransform).GUID,
                    out o,
                    ref dwObjectCount
                    );
                m_pMixer = o[0] as IMFTransform;

                // Make sure that we can work with this mixer.
                ConfigureMixer(m_pMixer);

                // Ask for the EVR's event-sink interface. (Required.)
                dwObjectCount = 1;

                pLookup.LookupService(
                    MFServiceLookupType.Global,
                    0,
                    MFServices.MR_VIDEO_RENDER_SERVICE,
                    typeof(IMediaEventSink).GUID,
                    out o,
                    ref dwObjectCount
                    );
                m_pMediaEventSink = o[0] as IMediaEventSink;

                // Successfully initialized. Set the state to "stopped."
                m_RenderState = RENDER_STATE.RENDER_STATE_STOPPED;

            }
        }

        public void ReleaseServicePointers()
        {
            TRACE(("ReleaseServicePointers"));

            // Enter the shut-down state.
            {
                lock (this)
                {
                    m_RenderState = RENDER_STATE.RENDER_STATE_SHUTDOWN;
                }
            }

            // Flush any samples that were scheduled.
            Flush();

            // Clear the media type and release related resources (surfaces, etc).
            SetMediaType(null);

            // Release all services that were acquired from InitServicePointers.
            SafeRelease(m_pClock);
            SafeRelease(m_pMixer);
            SafeRelease(m_pMediaEventSink);
        }

        #endregion

        #region IMFVideoDisplayControl Members

        public void GetAspectRatioMode(out MFVideoAspectRatioMode pdwAspectRatioMode)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GetBorderColor(out int pClr)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GetCurrentImage(MediaFoundation.Misc.BitmapInfoHeader pBih, out IntPtr pDib, out int pcbDib, out long pTimeStamp)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GetFullscreen(out bool pfFullscreen)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GetIdealVideoSize(MediaFoundation.Misc.SIZE pszMin, MediaFoundation.Misc.SIZE pszMax)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GetNativeVideoSize(MediaFoundation.Misc.SIZE pszVideo, MediaFoundation.Misc.SIZE pszARVideo)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GetRenderingPrefs(out MFVideoRenderPrefs pdwRenderFlags)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GetVideoPosition(MFVideoNormalizedRect pnrcSource, MediaFoundation.Misc.RECT prcDest)
        {
            lock (this)
            {
                if (pnrcSource == null || prcDest == null)
                {
                    throw new COMException("EVRCustomPresenter::GetVideoPosition", E_Pointer);
                }

                pnrcSource = m_nrcSource;
                prcDest = m_pD3DPresentEngine.GetDestinationRect();
            }
        }

        public void GetVideoWindow(out IntPtr phwndVideo)
        {
            lock (this)
            {
                // The D3DPresentEngine object stores the handle.
                phwndVideo = m_pD3DPresentEngine.GetVideoWindow();
            }
        }

        public void RepaintVideo()
        {
            lock (this)
            {
                CheckShutdown();

                // Ignore the request if we have not presented any samples yet.
                if (m_bPrerolled)
                {
                    m_bRepaint = true;
                    ProcessOutput();
                }
            }
        }

        public void SetAspectRatioMode(MFVideoAspectRatioMode dwAspectRatioMode)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void SetBorderColor(int Clr)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void SetFullscreen(bool fFullscreen)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void SetRenderingPrefs(MFVideoRenderPrefs dwRenderFlags)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void SetVideoPosition(MFVideoNormalizedRect pnrcSource, MediaFoundation.Misc.RECT prcDest)
        {
            lock (this)
            {

                // Validate parameters.

                // One parameter can be NULL, but not both.
                if (pnrcSource == null && prcDest == null)
                {
                    throw new COMException("EVRCustomPresenter::SetVideoPosition", E_Pointer);
                }

                // Validate the rectangles.
                if (pnrcSource != null)
                {
                    // The source rectangle cannot be flipped.
                    if ((pnrcSource.left > pnrcSource.right) ||
                        (pnrcSource.top > pnrcSource.bottom))
                    {
                        throw new COMException("EVRCustomPresenter::SetVideoPosition", E_InvalidArgument);
                    }

                    // The source rectangle has range (0..1)
                    if ((pnrcSource.left < 0) || (pnrcSource.right > 1) ||
                        (pnrcSource.top < 0) || (pnrcSource.bottom > 1))
                    {
                        throw new COMException("EVRCustomPresenter::SetVideoPosition 2", E_InvalidArgument);
                    }
                }

                if (prcDest != null)
                {
                    // The destination rectangle cannot be flipped.
                    if ((prcDest.left > prcDest.right) ||
                        (prcDest.top > prcDest.bottom))
                    {
                        throw new COMException("EVRCustomPresenter::SetVideoPosition 3", E_InvalidArgument);
                    }
                }

                // Update the source rectangle. Source clipping is performed by the mixer.
                if (pnrcSource != null)
                {
                    m_nrcSource = pnrcSource;

                    if (m_pMixer != null)
                    {
                        SetMixerSourceRect(m_pMixer, m_nrcSource);
                    }
                }

                // Update the destination rectangle.
                if (prcDest != null)
                {
                    RECT rcOldDest = m_pD3DPresentEngine.GetDestinationRect();

                    // Check if the destination rectangle changed.
                    if (!Utils.EqualRect(rcOldDest, prcDest))
                    {
                        m_pD3DPresentEngine.SetDestinationRect(prcDest);

                        // Set a new media type on the mixer.
                        if (m_pMixer != null)
                        {
                            try
                            {
                                RenegotiateMediaType();
                            }
                            catch (COMException e)
                            {
                                if (e.ErrorCode == MFError.MF_E_TRANSFORM_TYPE_NOT_SET)
                                {
                                    // This error means that the mixer is not ready for the media type.
                                    // Not a failure case -- the EVR will notify us when we need to set
                                    // the type on the mixer.
                                }
                                else
                                {
                                    throw;
                                }
                            }
                            // The media type changed. Request a repaint of the current frame.
                            m_bRepaint = true;
                            ProcessOutput(); // Ignore errors, the mixer might not have a video frame.
                        }
                    }
                }

            }
        }

        public void SetVideoWindow(IntPtr hwndVideo)
        {
            lock (this)
            {
                if (!Extern.IsWindow(hwndVideo))
                {
                    throw new COMException("EVRCustomPresenter::SetVideoWindow", E_InvalidArgument);
                }

                IntPtr oldHwnd = m_pD3DPresentEngine.GetVideoWindow();

                // If the window has changed, notify the D3DPresentEngine object.
                // This will cause a new Direct3D device to be created.
                if (oldHwnd != hwndVideo)
                {
                    m_pD3DPresentEngine.SetVideoWindow(hwndVideo);

                    // Tell the EVR that the device has changed.
                    NotifyEvent((int)EventCode.DisplayChanged, IntPtr.Zero, IntPtr.Zero);
                }

            }
        }

        #endregion

        #region Protected

        // CheckShutdown: 
        //     Returns MF_E_SHUTDOWN if the presenter is shutdown.
        //     Call this at the start of any methods that should fail after shutdown.
        protected void CheckShutdown()
        {
            if (m_RenderState == RENDER_STATE.RENDER_STATE_SHUTDOWN)
            {
                throw new COMException("CheckShutdown", MFError.MF_E_SHUTDOWN);
            }
        }

        // IsActive: The "active" state is started or paused.
        protected bool IsActive()
        {
            return ((m_RenderState == RENDER_STATE.RENDER_STATE_STARTED) || (m_RenderState == RENDER_STATE.RENDER_STATE_PAUSED));
        }

        // IsScrubbing: Scrubbing occurs when the frame rate is 0.
        protected bool IsScrubbing() { return m_fRate == 0.0f; }

        // NotifyEvent: Send an event to the EVR through its IMediaEventSink interface.
        protected void NotifyEvent(int EventCode, IntPtr Param1, IntPtr Param2)
        {
            if (m_pMediaEventSink != null)
            {
                m_pMediaEventSink.Notify((EventCode)EventCode, Param1, Param2);
            }
        }

        protected float GetMaxRate(bool bThin)
        {
            // Non-thinned:
            // If we have a valid frame rate and a monitor refresh rate, the maximum 
            // playback rate is equal to the refresh rate. Otherwise, the maximum rate 
            // is unbounded (FLT_MAX).

            // Thinned: The maximum rate is unbounded.

            float fMaxRate = float.MaxValue;
            MFRatio fps;
            int MonitorRateHz = 0;

            if (!bThin && (m_pMediaType != null))
            {
                fps = Utils.GetFrameRate(m_pMediaType);
                MonitorRateHz = m_pD3DPresentEngine.RefreshRate();

                if (fps.Denominator != 0 && fps.Numerator != 0 && MonitorRateHz != 0)
                {
                    fMaxRate = Utils.MulDiv(MonitorRateHz, fps.Denominator, fps.Numerator);
                }
            }

            return fMaxRate;
        }

        // Mixer operations
        protected void ConfigureMixer(IMFTransform pMixer)
        {
            Guid deviceID = Guid.Empty;

            IMFVideoDeviceID pDeviceID = null;

            try
            {
                // Make sure that the mixer has the same device ID as ourselves.
                pDeviceID = (IMFVideoDeviceID)pMixer;
                pDeviceID.GetDeviceID(out deviceID);

                if (deviceID != typeof(IDirect3DDevice9).GUID)
                {
                    throw new COMException("ConfigureMixer", MFError.MF_E_INVALIDREQUEST);
                }

                // Set the zoom rectangle (ie, the source clipping rectangle).
                SetMixerSourceRect(pMixer, m_nrcSource);
            }
            finally
            {
                SafeRelease(pDeviceID);
            }
        }

        // Formats
        protected void CreateOptimalVideoType(IMFMediaType pProposed, out IMFMediaType ppOptimal)
        {
            try
            {
                RECT rcOutput;
                //ZeroMemory(&rcOutput, sizeof(rcOutput));

                MFVideoArea displayArea;
                //ZeroMemory(&displayArea, sizeof(displayArea));

                IMFMediaType pOptimalType = null;
                VideoTypeBuilder pmtOptimal = null;

                // Create the helper object to manipulate the optimal type.
                VideoTypeBuilder.Create(out pmtOptimal);

                // Clone the proposed type.
                pmtOptimal.CopyFrom(pProposed);

                // Modify the new type.

                // For purposes of this SDK sample, we assume 
                // 1) The monitor's pixels are square.
                // 2) The presenter always preserves the pixel aspect ratio.

                // Set the pixel aspect ratio (PAR) to 1:1 (see assumption #1, above)
                pmtOptimal.SetPixelAspectRatio(1, 1);

                // Get the output rectangle.
                rcOutput = m_pD3DPresentEngine.GetDestinationRect();
                if (Utils.IsRectEmpty(rcOutput))
                {
                    // Calculate the output rectangle based on the media type.
                    CalculateOutputRectangle(pProposed, out rcOutput);
                }

                // Set the extended color information: Use BT.709 
                pmtOptimal.SetYUVMatrix(MFVideoTransferMatrix.BT709);
                pmtOptimal.SetTransferFunction(MFVideoTransferFunction.Func709);
                pmtOptimal.SetVideoPrimaries(MFVideoPrimaries.BT709);
                pmtOptimal.SetVideoNominalRange(MFNominalRange.MFNominalRange_16_235);
                pmtOptimal.SetVideoLighting(MFVideoLighting.Dim);

                // Set the target rect dimensions. 
                pmtOptimal.SetFrameDimensions(rcOutput.right, rcOutput.bottom);

                // Set the geometric aperture, and disable pan/scan.
                displayArea = Utils.MakeArea(0, 0, rcOutput.right, rcOutput.bottom);

                pmtOptimal.SetPanScanEnabled(false);

                pmtOptimal.SetGeometricAperture(displayArea);

                // Set the pan/scan aperture and the minimum display aperture. We don't care
                // about them per se, but the mixer will reject the type if these exceed the 
                // frame dimentions.
                pmtOptimal.SetPanScanAperture(displayArea);
                pmtOptimal.SetMinDisplayAperture(displayArea);

                // Return the pointer to the caller.
                pmtOptimal.GetMediaType(out pOptimalType);

                ppOptimal = pOptimalType;
            }
            finally
            {
                //SafeRelease(pOptimalType);
                //SafeRelease(pmtOptimal);
            }
        }

        protected void CalculateOutputRectangle(IMFMediaType pProposed, out RECT prcOutput)
        {
            int srcWidth = 0, srcHeight = 0;

            MFRatio inputPAR; // = { 0, 0 };
            MFRatio outputPAR; // = { 0, 0 };
            RECT rcOutput = new RECT(); // = { 0, 0, 0, 0 };

            MFVideoArea displayArea;
            //ZeroMemory(&displayArea, sizeof(displayArea));

            VideoTypeBuilder pmtProposed = null;

            // Helper object to read the media type.
            VideoTypeBuilder.Create(pProposed, out pmtProposed);

            // Get the source's frame dimensions.
            pmtProposed.GetFrameDimensions(out srcWidth, out srcHeight);

            // Get the source's display area. 
            pmtProposed.GetVideoDisplayArea(out displayArea);

            // Calculate the x,y offsets of the display area.
            int offsetX = Utils.GetOffset(displayArea.OffsetX);
            int offsetY = Utils.GetOffset(displayArea.OffsetY);

            // Use the display area if valid. Otherwise, use the entire frame.
            if (displayArea.Area.cx != 0 &&
                displayArea.Area.cy != 0 &&
                offsetX + displayArea.Area.cx <= (srcWidth) &&
                offsetY + displayArea.Area.cy <= (srcHeight))
            {
                rcOutput.left = offsetX;
                rcOutput.right = offsetX + displayArea.Area.cx;
                rcOutput.top = offsetY;
                rcOutput.bottom = offsetY + displayArea.Area.cy;
            }
            else
            {
                rcOutput.left = 0;
                rcOutput.top = 0;
                rcOutput.right = srcWidth;
                rcOutput.bottom = srcHeight;
            }

            // rcOutput is now either a sub-rectangle of the video frame, or the entire frame.

            // If the pixel aspect ratio of the proposed media type is different from the monitor's, 
            // letterbox the video. We stretch the image rather than shrink it.

            inputPAR = pmtProposed.GetPixelAspectRatio();    // Defaults to 1:1

            outputPAR.Denominator = outputPAR.Numerator = 1; // This is an assumption of the sample.

            // Adjust to get the correct picture aspect ratio.
            prcOutput = CorrectAspectRatio(rcOutput, inputPAR, outputPAR);

        }

        protected void SetMediaType(IMFMediaType pMediaType)
        {
            // Note: pMediaType can be NULL (to clear the type)

            // Clearing the media type is allowed in any state (including shutdown).
            if (pMediaType == null)
            {
                SafeRelease(m_pMediaType);
                ReleaseResources();
                return;
            }

            try
            {

                MFRatio fps; // = { 0, 0 };
                Queue sampleQueue = new Queue();

                IMFSample pSample = null;

                // Cannot set the media type after shutdown.
                CheckShutdown();

                // Check if the new type is actually different.
                // Note: This function safely handles NULL input parameters.
                if (Utils.AreMediaTypesEqual(m_pMediaType, pMediaType))
                {
                    return; // Nothing more to do.
                }

                // We're really changing the type. First get rid of the old type.
                SafeRelease(m_pMediaType);
                ReleaseResources();

                // Initialize the presenter engine with the new media type.
                // The presenter engine allocates the samples. 

                m_pD3DPresentEngine.CreateVideoSamples(pMediaType, sampleQueue);

                // Mark each sample with our token counter. If this batch of samples becomes
                // invalid, we increment the counter, so that we know they should be discarded. 
                IEnumerator x = sampleQueue.GetEnumerator();

                while (x.MoveNext())
                {
                    pSample = x.Current as IMFSample;
                    pSample.SetUINT32(MFSamplePresenter_SampleCounter, m_TokenCounter);
                }

                // Add the samples to the sample pool.
                m_SamplePool.Initialize(sampleQueue);

                // Set the frame rate on the scheduler. 
                fps = Utils.GetFrameRate(pMediaType);

                if ((fps.Numerator != 0) && (fps.Denominator != 0))
                {
                    m_scheduler.SetFrameRate(fps);
                }
                else
                {
                    // NOTE: The mixer's proposed type might not have a frame rate, in which case 
                    // we'll use an arbitary default. (Although it's unlikely the video source
                    // does not have a frame rate.)
                    m_scheduler.SetFrameRate(g_DefaultFrameRate);
                }

                // Store the media type.
                Debug.Assert(pMediaType != null);
                m_pMediaType = pMediaType;

            }
            catch
            {
                ReleaseResources();
                throw;
            }
        }

        protected void IsMediaTypeSupported(IMFMediaType pMediaType)
        {
            VideoTypeBuilder pProposed = null;

            D3DFORMAT d3dFormat = D3DFORMAT.D3DFMT_UNKNOWN;
            bool bCompressed = false;
            MFVideoInterlaceMode InterlaceMode = MFVideoInterlaceMode.Unknown;
            MFVideoArea VideoCropArea;
            int width = 0, height = 0;

            try
            {

                // Helper object for reading the proposed type.
                VideoTypeBuilder.Create(pMediaType, out pProposed);

                // Reject compressed media types.
                pProposed.IsCompressedFormat(out bCompressed);
                if (bCompressed)
                {
                    throw new COMException("EVRCustomPresenter::IsMediaTypeSupported", MFError.MF_E_INVALIDMEDIATYPE);
                }

                // Validate the format.
                int i;
                pProposed.GetFourCC(out i);
                d3dFormat = (D3DFORMAT)i;

                // The D3DPresentEngine checks whether the format can be used as
                // the back-buffer format for the swap chains.
                m_pD3DPresentEngine.CheckFormat(d3dFormat);

                // Reject interlaced formats.
                pProposed.GetInterlaceMode(out InterlaceMode);
                if (InterlaceMode != MFVideoInterlaceMode.Progressive)
                {
                    throw new COMException("EVRCustomPresenter::IsMediaTypeSupported 2", MFError.MF_E_INVALIDMEDIATYPE);
                }

                pProposed.GetFrameDimensions(out width, out height);

                // Validate the various apertures (cropping regions) against the frame size.
                // Any of these apertures may be unspecified in the media type, in which case 
                // we ignore it. We just want to reject invalid apertures.

                try
                {
                    pProposed.GetPanScanAperture(out VideoCropArea);
                    ValidateVideoArea(VideoCropArea, width, height);
                }
                catch { }

                try
                {
                    pProposed.GetGeometricAperture(out VideoCropArea);
                    ValidateVideoArea(VideoCropArea, width, height);
                }
                catch { }

                try
                {
                    pProposed.GetMinDisplayAperture(out VideoCropArea);
                    ValidateVideoArea(VideoCropArea, width, height);
                }
                catch { }

            }
            finally
            {
                SafeRelease(pProposed);
            }
        }

        // Message handlers
        protected void Flush()
        {
            m_bPrerolled = false;

            // The scheduler might have samples that are waiting for
            // their presentation time. Tell the scheduler to flush.

            // This call blocks until the scheduler threads discards all scheduled samples.
            m_scheduler.Flush();

            // Flush the frame-step queue.
            m_FrameStep.samples.Clear();

            if (m_RenderState == RENDER_STATE.RENDER_STATE_STOPPED)
            {
                // Repaint with black.
                m_pD3DPresentEngine.PresentSample(null, 0);
            }
        }

        protected void RenegotiateMediaType()
        {
            TRACE(("RenegotiateMediaType"));

            bool bFoundMediaType = false;

            IMFMediaType pMixerType = null;
            IMFMediaType pOptimalType = null;
            IMFVideoMediaType pVideoType = null;

            if (m_pMixer == null)
            {
                throw new COMException("RenegotiateMediaType", MFError.MF_E_INVALIDREQUEST);
            }

            try
            {
                // Loop through all of the mixer's proposed output types.
                int iTypeIndex = 0;
                while (!bFoundMediaType)
                {
                    SafeRelease(pMixerType);
                    SafeRelease(pOptimalType);

                    // Step 1. Get the next media type supported by mixer.
                    m_pMixer.GetOutputAvailableType(0, iTypeIndex++, out pMixerType);

                    // From now on, if anything in this loop fails, try the next type,
                    // until we succeed or the mixer runs out of types.

                    // Step 2. Check if we support this media type. 

                    try
                    {
                        // Note: None of the modifications that we make later in CreateOptimalVideoType
                        // will affect the suitability of the type, at least for us. (Possibly for the mixer.)
                        IsMediaTypeSupported(pMixerType);

                        // Step 3. Adjust the mixer's type to match our requirements.
                        CreateOptimalVideoType(pMixerType, out pOptimalType);

                        // Step 4. Check if the mixer will accept this media type.
                        m_pMixer.SetOutputType(0, pOptimalType, MFTSetTypeFlags.TestOnly);

                        // Step 5. Try to set the media type on ourselves.
                        SetMediaType(pOptimalType);

                        // Step 6. Set output media type on mixer.
                        try
                        {
                            m_pMixer.SetOutputType(0, pOptimalType, 0);
                            bFoundMediaType = true;
                        }
                        catch
                        {
                            SetMediaType(null);
                        }

                    }
                    catch
                    {
                    }
                }
            }
            finally
            {
                SafeRelease(pMixerType);
                SafeRelease(pOptimalType);
                SafeRelease(pVideoType);
            }
        }

        protected void ProcessInputNotify()
        {
            // Set the flag that says the mixer has a new sample.
            m_bSampleNotify = true;

            if (m_pMediaType == null)
            {
                // We don't have a valid media type yet.
                throw new COMException("ProcessInputNotify", MFError.MF_E_TRANSFORM_TYPE_NOT_SET);
            }
            else
            {
                // Try to process an output sample.
                ProcessOutputLoop();
            }
        }

        protected void BeginStreaming()
        {
            // Start the scheduler thread. 
            m_scheduler.StartScheduler(m_pClock);
        }

        protected void EndStreaming()
        {
            m_scheduler.StopScheduler();
        }

        protected void CheckEndOfStream()
        {
            if (!m_bEndStreaming)
            {
                // The EVR did not send the MFVP_MESSAGE_ENDOFSTREAM message.
                return;
            }

            if (m_bSampleNotify)
            {
                // The mixer still has input. 
                return;
            }

            if (m_SamplePool.AreSamplesPending())
            {
                // Samples are still scheduled for rendering.
                return;
            }

            // Everything is complete. Now we can tell the EVR that we are done.
            NotifyEvent((int)EventCode.Complete, IntPtr.Zero, IntPtr.Zero);
            m_bEndStreaming = false;
        }

        // Managing samples
        protected void ProcessOutputLoop()
        {
            int hr = S_Ok;

            // Process as many samples as possible.
            while (hr == S_Ok)
            {
                // If the mixer doesn't have a new input sample, break from the loop.
                if (!m_bSampleNotify)
                {
                    throw new COMException("ProcessOutputLoop", MFError.MF_E_TRANSFORM_NEED_MORE_INPUT);
                }

                // Try to process a sample.
                hr = ProcessOutput();

                // NOTE: ProcessOutput can return S_FALSE to indicate it did not process a sample.
                // If so, we break out of the loop.
            }

            if (hr == MFError.MF_E_TRANSFORM_NEED_MORE_INPUT)
            {
                // The mixer has run out of input data. Check if we're at the end of the stream.
                CheckEndOfStream();
            }
        }

        protected int ProcessOutput()
        {
            Debug.Assert(m_bSampleNotify || m_bRepaint);  // See note above.

            int hr = S_Ok;
            ProcessOutputStatus dwStatus = 0;
            Int64 mixerStartTime = 0, mixerEndTime = 0;
            Int64 systemTime = 0;
            bool bRepaint = m_bRepaint; // Temporarily store this state flag.  

            MFTOutputDataBuffer[] dataBuffer = new MFTOutputDataBuffer[1];
            //ZeroMemory(&dataBuffer, sizeof(dataBuffer));

            IMFSample pSample = null;

            // If the clock is not running, we present the first sample,
            // and then don't present any more until the clock starts. 

            if ((m_RenderState != RENDER_STATE.RENDER_STATE_STARTED) &&  // Not running.
                 !m_bRepaint &&                             // Not a repaint request.
                 m_bPrerolled                               // At least one sample has been presented.
                 )
            {
                return S_False;
            }

            // Make sure we have a pointer to the mixer.
            if (m_pMixer == null)
            {
                return MFError.MF_E_INVALIDREQUEST;
            }

            try
            {
                // Try to get a free sample from the video sample pool.
                try
                {
                    m_SamplePool.GetSample(out pSample);
                }
                catch (Exception e)
                {
                    hr = Marshal.GetHRForException(e);
                    if (hr == MFError.MF_E_SAMPLEALLOCATOR_EMPTY)
                    {
                        return S_False; // No free samples. We'll try again when a sample is released.
                    }
                    throw;
                }

                // From now on, we have a valid video sample pointer, where the mixer will
                // write the video data.
                Debug.Assert(pSample != null);

                // (If the following assertion fires, it means we are not managing the sample pool correctly.)
                //Debug.Assert(MFGetAttributeUINT32(pSample, MFSamplePresenter_SampleCounter, -1) == m_TokenCounter);

                if (m_bRepaint)
                {
                    // Repaint request. Ask the mixer for the most recent sample.
                    SetDesiredSampleTime(pSample, m_scheduler.LastSampleTime(), m_scheduler.FrameDuration());
                    m_bRepaint = true; // OK to clear this flag now.
                }
                else
                {
                    // Not a repaint request. Clear the desired sample time; the mixer will
                    // give us the next frame in the stream.
                    ClearDesiredSampleTime(pSample);

                    if (m_pClock != null)
                    {
                        // Latency: Record the starting time for the ProcessOutput operation. 
                        m_pClock.GetCorrelatedTime(0, out mixerStartTime, out systemTime);
                    }
                }

                // Now we are ready to get an output sample from the mixer. 
                dataBuffer[0].dwStreamID = 0;
                dataBuffer[0].pSample = Marshal.GetIUnknownForObject(pSample);
                Marshal.Release(dataBuffer[0].pSample);
                dataBuffer[0].dwStatus = 0;

                try
                {
                    m_pMixer.ProcessOutput(0, 1, dataBuffer, out dwStatus);
                }
                catch (Exception e)
                {
                    hr = Marshal.GetHRForException(e);
                }

                if (Failed(hr))
                {
                    // Return the sample to the pool.
                    m_SamplePool.ReturnSample(pSample);

                    // Handle some known error codes from ProcessOutput.
                    if (hr == MFError.MF_E_TRANSFORM_TYPE_NOT_SET)
                    {
                        // The mixer's format is not set. Negotiate a new format.
                        RenegotiateMediaType();
                    }
                    else if (hr == MFError.MF_E_TRANSFORM_STREAM_CHANGE)
                    {
                        // There was a dynamic media type change. Clear our media type.
                        SetMediaType(null);
                    }
                    else if (hr == MFError.MF_E_TRANSFORM_NEED_MORE_INPUT)
                    {
                        // The mixer needs more input. 
                        // We have to wait for the mixer to get more input.
                        m_bSampleNotify = false;
                    }
                }
                else
                {
                    // We got an output sample from the mixer.

                    if (m_pClock != null && !bRepaint)
                    {
                        // Latency: Record the ending time for the ProcessOutput operation,
                        // and notify the EVR of the latency. 

                        m_pClock.GetCorrelatedTime(0, out mixerEndTime, out systemTime);

                        Int64 latencyTime = mixerEndTime - mixerStartTime;
                        int EC_PROCESSING_LATENCY = 0x21;
                        NotifyEvent(EC_PROCESSING_LATENCY, new IntPtr(latencyTime), IntPtr.Zero);
                    }

                    // Set up notification for when the sample is released.
                    TrackSample(pSample);

                    // Schedule the sample.
                    if ((m_FrameStep.state == FRAMESTEP_STATE.FRAMESTEP_NONE) || bRepaint)
                    {
                        DeliverSample(pSample, bRepaint);
                    }
                    else
                    {
                        // We are frame-stepping (and this is not a repaint request).
                        DeliverFrameStepSample(pSample);
                    }
                    m_bPrerolled = true; // We have presented at least one sample now.
                }
            }
            finally
            {
                // Release any events that were returned from the ProcessOutput method. 
                // (We don't expect any events from the mixer, but this is a good practice.)
                //ReleaseEventCollection(1, dataBuffer);

                SafeRelease(pSample);
            }

            return S_Ok;
        }

        protected void DeliverSample(IMFSample pSample, bool bRepaint)
        {
            Debug.Assert(pSample != null);

            DeviceState state = DeviceState.DeviceOK;

            // If we are not actively playing, OR we are scrubbing (rate = 0) OR this is a 
            // repaint request, then we need to present the sample immediately. Otherwise, 
            // schedule it normally.

            bool bPresentNow = ((m_RenderState != RENDER_STATE.RENDER_STATE_STARTED) || IsScrubbing() || bRepaint);

            // Check the D3D device state.
            int hr = S_Ok;
            try
            {
                m_pD3DPresentEngine.CheckDeviceState(out state);
                m_scheduler.ScheduleSample(pSample, bPresentNow);
            }
            catch (Exception e)
            {
                // Notify the EVR that we have failed during streaming. The EVR will notify the 
                // pipeline (ie, it will notify the Filter Graph Manager in DirectShow or the 
                // Media Session in Media Foundation).
                hr = Marshal.GetHRForException(e);
                NotifyEvent((int)EventCode.ErrorAbort, new IntPtr(hr), IntPtr.Zero);
                throw;
            }

            if (state == DeviceState.DeviceReset)
            {
                // The Direct3D device was re-set. Notify the EVR.
                NotifyEvent((int)EventCode.DisplayChanged, IntPtr.Zero, IntPtr.Zero);
            }
        }

        protected void TrackSample(IMFSample pSample)
        {
            IMFTrackedSample pTracked = null;

            pTracked = (IMFTrackedSample)pSample;
            pTracked.SetAllocator(m_SampleFreeCB, null);
        }

        protected void ReleaseResources()
        {
            m_TokenCounter++;

            Flush();

            m_SamplePool.Clear();

            m_pD3DPresentEngine.ReleaseResources();
        }

        // Frame-stepping
        protected void PrepareFrameStep(int cSteps)
        {
            // Cache the step count.
            m_FrameStep.steps += cSteps;

            // Set the frame-step state. 
            m_FrameStep.state = FRAMESTEP_STATE.FRAMESTEP_WAITING_START;

            // If the clock is are already running, we can start frame-stepping now.
            // Otherwise, we will start when the clock starts.
            if (m_RenderState == RENDER_STATE.RENDER_STATE_STARTED)
            {
                StartFrameStep();
            }
        }

        protected void StartFrameStep()
        {
            Debug.Assert(m_RenderState == RENDER_STATE.RENDER_STATE_STARTED);

            IMFSample pSample = null;

            try
            {
                if (m_FrameStep.state == FRAMESTEP_STATE.FRAMESTEP_WAITING_START)
                {
                    // We have a frame-step request, and are waiting for the clock to start.
                    // Set the state to "pending," which means we are waiting for samples.
                    m_FrameStep.state = FRAMESTEP_STATE.FRAMESTEP_PENDING;

                    // If the frame-step queue already has samples, process them now.
                    while (m_FrameStep.samples.Count > 0 && (m_FrameStep.state == FRAMESTEP_STATE.FRAMESTEP_PENDING))
                    {
                        pSample = (IMFSample)m_FrameStep.samples.Dequeue();
                        DeliverFrameStepSample(pSample);
                        SafeRelease(pSample);

                        // We break from this loop when:
                        //   (a) the frame-step queue is empty, or
                        //   (b) the frame-step operation is complete.
                    }
                }
                else if (m_FrameStep.state == FRAMESTEP_STATE.FRAMESTEP_NONE)
                {
                    // We are not frame stepping. Therefore, if the frame-step queue has samples, 
                    // we need to process them normally.
                    while (m_FrameStep.samples.Count > 0)
                    {
                        pSample = (IMFSample)m_FrameStep.samples.Dequeue();
                        DeliverSample(pSample, false);
                        SafeRelease(pSample);
                    }
                }
            }
            finally
            {
                SafeRelease(pSample);
            }
        }

        protected void DeliverFrameStepSample(IMFSample pSample)
        {
            // For rate 0, discard any sample that ends earlier than the clock time.
            if (IsScrubbing() && (m_pClock != null) && IsSampleTimePassed(m_pClock, pSample))
            {
                // Discard this sample.
            }
            else if (m_FrameStep.state >= FRAMESTEP_STATE.FRAMESTEP_SCHEDULED)
            {
                // A frame was already submitted. Put this sample on the frame-step queue, 
                // in case we are asked to step to the next frame. If frame-stepping is
                // cancelled, this sample will be processed normally.
                m_FrameStep.samples.Enqueue(pSample);
            }
            else
            {
                // We're ready to frame-step.

                // Decrement the number of steps.
                if (m_FrameStep.steps > 0)
                {
                    m_FrameStep.steps--;
                }

                if (m_FrameStep.steps > 0)
                {
                    // This is not the last step. Discard this sample.
                }
                else if (m_FrameStep.state == FRAMESTEP_STATE.FRAMESTEP_WAITING_START)
                {
                    // This is the right frame, but the clock hasn't started yet. Put the
                    // sample on the frame-step queue. When the clock starts, the sample
                    // will be processed.
                    m_FrameStep.samples.Enqueue(pSample);
                }
                else
                {
                    // This is the right frame *and* the clock has started. Deliver this sample.
                    DeliverSample(pSample, false);

                    // QI for IUnknown so that we can identify the sample later.
                    // (Per COM rules, an object alwayss return the same pointer when QI'ed for IUnknown.)

                    // Save this value.
                    m_FrameStep.pSampleNoRef = Marshal.GetIUnknownForObject(pSample);
                    Marshal.Release(m_FrameStep.pSampleNoRef); // No add-ref. 

                    // NOTE: We do not AddRef the IUnknown pointer, because that would prevent the 
                    // sample from invoking the OnSampleFree callback after the sample is presented. 
                    // We use this IUnknown pointer purely to identify the sample later; we never
                    // attempt to dereference the pointer.

                    // Update our state.
                    m_FrameStep.state = FRAMESTEP_STATE.FRAMESTEP_SCHEDULED;
                }
            }
        }

        protected void CompleteFrameStep(IMFSample pSample)
        {
            Int64 hnsSampleTime = 0;
            Int64 hnsSystemTime = 0;

            // Update our state.
            m_FrameStep.state = FRAMESTEP_STATE.FRAMESTEP_COMPLETE;
            m_FrameStep.pSampleNoRef = IntPtr.Zero;

            // Notify the EVR that the frame-step is complete.
            NotifyEvent((int)EventCode.StepComplete, IntPtr.Zero, IntPtr.Zero); // FALSE = completed (not cancelled)

            // If we are scrubbing (rate == 0), also send the "scrub time" event.
            if (IsScrubbing())
            {
                // Get the time stamp from the sample.

                try
                {
                    pSample.GetSampleTime(out hnsSampleTime);
                }
                catch
                {
                    // No time stamp. Use the current presentation time.
                    if (m_pClock != null)
                    {
                        m_pClock.GetCorrelatedTime(0, out hnsSampleTime, out hnsSystemTime);
                    }
                }

                const int EC_SCRUB_TIME = 0x23;
                NotifyEvent(EC_SCRUB_TIME, new IntPtr((int)hnsSampleTime), new IntPtr(hnsSampleTime >> 32));
            }
        }

        protected void CancelFrameStep()
        {
            FRAMESTEP_STATE oldState = m_FrameStep.state;

            m_FrameStep.state = FRAMESTEP_STATE.FRAMESTEP_NONE;
            m_FrameStep.steps = 0;
            m_FrameStep.pSampleNoRef = IntPtr.Zero;
            // Don't clear the frame-step queue yet, because we might frame step again.

            if (oldState > FRAMESTEP_STATE.FRAMESTEP_NONE && oldState < FRAMESTEP_STATE.FRAMESTEP_COMPLETE)
            {
                // We were in the middle of frame-stepping when it was cancelled.
                // Notify the EVR.
                NotifyEvent((int)EventCode.StepComplete, new IntPtr(1), IntPtr.Zero); // TRUE = cancelled
            }
        }

        // Callbacks

        // Callback when a video sample is released.
        public void OnSampleFree(IMFAsyncResult pResult)
        {
            object pObject = null;
            IMFSample pSample = null;
            //IUnknown pUnk = null;

            try
            {
                // Get the sample from the async result object.
                pResult.GetObject(out pObject);
                pSample = (IMFSample)pObject;

                // If this sample was submitted for a frame-step, then the frame step is complete.
                if (m_FrameStep.state == FRAMESTEP_STATE.FRAMESTEP_SCHEDULED)
                {
                    // QI the sample for IUnknown and compare it to our cached value.
                    IntPtr ip = Marshal.GetIUnknownForObject(pSample);
                    Marshal.Release(ip);

                    if (m_FrameStep.pSampleNoRef == ip)
                    {
                        // Notify the EVR. 
                        CompleteFrameStep(pSample);
                    }

                    // Note: Although pObject is also an IUnknown pointer, it's not guaranteed
                    // to be the exact pointer value returned via QueryInterface, hence the 
                    // need for the second QI.
                }

                lock (this)
                {
                    if (Utils.MFGetAttributeUINT32(pSample, MFSamplePresenter_SampleCounter, -1) == m_TokenCounter)
                    {
                        // Return the sample to the sample pool.
                        m_SamplePool.ReturnSample(pSample);

                        // Now that a free sample is available, process more data if possible.
                        ProcessOutputLoop();
                    }

                }

            }
            catch (Exception e)
            {
                int hr = Marshal.GetHRForException(e);
                NotifyEvent((int)EventCode.ErrorAbort, new IntPtr(hr), IntPtr.Zero);
            }
            finally
            {
                SafeRelease(pObject);
                SafeRelease(pSample);
            }
        }

        protected void SetMixerSourceRect(IMFTransform pMixer, MFVideoNormalizedRect nrcSource)
        {
            if (pMixer == null)
            {
                throw new COMException("SetMixerSourceRect", E_Pointer);
            }

            IMFAttributes pAttributes = null;

            pMixer.GetAttributes(out pAttributes);

            Utils.MFSetBlob(pAttributes, MFAttributesClsid.VIDEO_ZOOM_RECT, nrcSource);

            SafeRelease(pAttributes);
        }

        protected void ValidateVideoArea(MFVideoArea area, int width, int height)
        {
            float fOffsetX = Utils.MFOffsetToFloat(area.OffsetX);
            float fOffsetY = Utils.MFOffsetToFloat(area.OffsetY);

            if (((int)fOffsetX + area.Area.cx > (int)width) ||
                 ((int)fOffsetY + area.Area.cy > (int)height))
            {
                throw new COMException("ValidateVideoArea", MFError.MF_E_INVALIDMEDIATYPE);
            }
        }

        protected void SetDesiredSampleTime(IMFSample pSample, long hnsSampleTime, long hnsDuration)
        {
            if (pSample == null)
            {
                throw new COMException("SetDesiredSampleTime", E_Pointer);
            }

            IMFDesiredSample pDesired = null;

            pDesired = (IMFDesiredSample)pSample;
            // This method has no return value.
            pDesired.SetDesiredSampleTimeAndDuration(hnsSampleTime, hnsDuration);

            SafeRelease(pDesired);
        }

        protected void ClearDesiredSampleTime(IMFSample pSample)
        {
            if (pSample == null)
            {
                throw new COMException("ClearDesiredSampleTime", E_Pointer);
            }

            IMFDesiredSample pDesired = null;
            object pUnkSwapChain = null;

            // We store some custom attributes on the sample, so we need to cache them
            // and reset them.
            //
            // This works around the fact that IMFDesiredSample::Clear() removes all of the
            // attributes from the sample. 

            int counter;
            pSample.GetUINT32(MFSamplePresenter_SampleCounter, out counter);

            try
            {
                Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");
                pSample.GetUnknown(MFSamplePresenter_SampleSwapChain, IID_IUnknown, out pUnkSwapChain);
            }
            catch { }


            pDesired = (IMFDesiredSample)pSample;

            // This method has no return value.
            pDesired.Clear();

            pSample.SetUINT32(MFSamplePresenter_SampleCounter, counter);

            if (pUnkSwapChain != null)
            {
                pSample.SetUnknown(MFSamplePresenter_SampleSwapChain, pUnkSwapChain);
            }

            //SAFE_RELEASE(pUnkSwapChain);
            //SAFE_RELEASE(pDesired);
        }

        protected bool IsSampleTimePassed(IMFClock pClock, IMFSample pSample)
        {
            Debug.Assert(pClock != null);
            Debug.Assert(pSample != null);

            if (pSample == null || pClock == null)
            {
                throw new COMException("IsSampleTimePassed", E_Pointer);
            }

            bool bRet = false;
            Int64 hnsTimeNow = 0;
            Int64 hnsSystemTime = 0;
            Int64 hnsSampleStart = 0;
            Int64 hnsSampleDuration = 0;

            // The sample might lack a time-stamp or a duration, and the
            // clock might not report a time.

            try
            {
                pClock.GetCorrelatedTime(0, out hnsTimeNow, out hnsSystemTime);
                pSample.GetSampleTime(out hnsSampleStart);
                pSample.GetSampleDuration(out hnsSampleDuration);
                if (hnsSampleStart + hnsSampleDuration < hnsTimeNow)
                {
                    bRet = true;
                }
            }
            catch { }

            return bRet;
        }

        protected RECT CorrectAspectRatio(RECT src, MFRatio srcPAR, MFRatio destPAR)
        {
            // Start with a rectangle the same size as src, but offset to the origin (0,0).
            RECT rc = new RECT(0, 0, src.right - src.left, src.bottom - src.top);

            // If the source and destination have the same PAR, there is nothing to do.
            // Otherwise, adjust the image size, in two steps:
            //  1. Transform from source PAR to 1:1
            //  2. Transform from 1:1 to destination PAR.

            if ((srcPAR.Numerator != destPAR.Numerator) || (srcPAR.Denominator != destPAR.Denominator))
            {
                // Correct for the source's PAR.

                if (srcPAR.Numerator > srcPAR.Denominator)
                {
                    // The source has "wide" pixels, so stretch the width.
                    rc.right = Utils.MulDiv(rc.right, srcPAR.Numerator, srcPAR.Denominator);
                }
                else if (srcPAR.Numerator > srcPAR.Denominator)
                {
                    // The source has "tall" pixels, so stretch the height.
                    rc.bottom = Utils.MulDiv(rc.bottom, srcPAR.Denominator, srcPAR.Numerator);
                }
                // else: PAR is 1:1, which is a no-op.


                // Next, correct for the target's PAR. This is the inverse operation of the previous.

                if (destPAR.Numerator > destPAR.Denominator)
                {
                    // The destination has "tall" pixels, so stretch the width.
                    rc.bottom = Utils.MulDiv(rc.bottom, destPAR.Denominator, destPAR.Numerator);
                }
                else if (destPAR.Numerator > destPAR.Denominator)
                {
                    // The destination has "fat" pixels, so stretch the height.
                    rc.right = Utils.MulDiv(rc.right, destPAR.Numerator, destPAR.Denominator);
                }
                // else: PAR is 1:1, which is a no-op.
            }

            return rc;
        }

        #endregion

        #region Members

        protected RENDER_STATE m_RenderState;          // Rendering state.
        protected FrameStep m_FrameStep;            // Frame-stepping information.

        // Samples and scheduling
        protected Scheduler m_scheduler;			// Manages scheduling of samples.
        protected SamplePool m_SamplePool;           // Pool of allocated samples.
        protected int m_TokenCounter;         // Counter. Incremented whenever we create new samples.

        // Rendering state
        protected bool m_bSampleNotify;		// Did the mixer signal it has an input sample?
        protected bool m_bRepaint;				// Do we need to repaint the last sample?
        protected bool m_bPrerolled;	        // Have we presented at least one sample?
        protected bool m_bEndStreaming;		// Did we reach the end of the stream (EOS)?

        protected MFVideoNormalizedRect m_nrcSource;            // Source rectangle.
        protected float m_fRate;                // Playback rate.

        // Deletable objects.
        protected D3DPresentEngine m_pD3DPresentEngine;    // Rendering engine. (Never null if the constructor succeeds.)

        // COM interfaces.
        protected IMFClock m_pClock;               // The EVR's clock.
        protected IMFTransform m_pMixer;               // The mixer.
        protected IMediaEventSink m_pMediaEventSink;      // The EVR's event-sink interface.
        protected IMFMediaType m_pMediaType;           // Output media type
        protected AsyncCallback m_SampleFreeCB;

        #endregion

    }
}
