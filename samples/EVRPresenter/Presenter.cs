/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released
as public domain.  It is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security;
using System.Reflection;

using MediaFoundation;
using MediaFoundation.Alt;
using MediaFoundation.EVR;
using MediaFoundation.Misc;
using MediaFoundation.Transform;
using MediaFoundation.Utility;
using DirectShowLib;
using NSHack;
using System.Drawing;

namespace EVRPresenter
{
    [ComVisible(true),
     Guid("DD1BE413-E999-47f1-A107-9BC1F3DCB6C7"),
     ClassInterface(ClassInterfaceType.None)]
    public class EVRCustomPresenter : COMBase,
       IMFVideoDeviceID,
       IMFVideoPresenter,
       IMFRateSupport,
       IMFTopologyServiceLookupClientAlt,
       IMFClockStateSink,
       IMFGetServiceAlt,
       IMFVideoDisplayControl,
       IMFAsyncCallback,
       IQualProp
    {
        #region Definitions

        public static Guid MFSamplePresenter_SampleSwapChain = new Guid(0xad885bd1, 0x7def, 0x414a, 0xb5, 0xb0, 0xd3, 0xd2, 0x63, 0xd6, 0xe9, 0x6d);
        protected const long PRESENTATION_CURRENT_POSITION = 0x7fffffffffffffff;
        protected static Guid MFSamplePresenter_SampleCounter = new Guid(0xcce75b6, 0xf22, 0x4422, 0x88, 0x30, 0x1c, 0x6e, 0xd6, 0x9d, 0x9b, 0x8b);
        protected static MFRatio g_DefaultFrameRate = new MFRatio(1, 30);

        [DllImport("user32.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        protected extern static bool IsWindow(
            IntPtr hwnd
            );

        [DllImport("mf.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall), SuppressUnmanagedCodeSecurity]
        protected static extern int DllCanUnloadNow();

        protected enum RenderState
        {
            Started = 1,
            Stopped,
            Paused,
            Shutdown   // Initial state.

            // State transitions:

            // InitServicePointers                  -> STOPPED
            // ReleaseServicePointers               -> SHUTDOWN
            // IMFClockStateSink::OnClockStart      -> STARTED
            // IMFClockStateSink::OnClockRestart    -> STARTED
            // IMFClockStateSink::OnClockPause      -> PAUSED
            // IMFClockStateSink::OnClockStop       -> STOPPED
        }

        protected enum FrameStepRate
        {
            None,             // Not frame stepping.
            WaitingStart,    // Frame stepping, but the clock is not started.
            Pending,          // Clock is started. Waiting for samples.
            Scheduled,        // Submitted a sample for rendering.
            Complete          // Sample was rendered.

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
                state = FrameStepRate.None;
                steps = 0;
                pSampleNoRef = IntPtr.Zero;
                samples = new Queue<IMFSample>();
            }

            public void SetSample(object obj)
            {
                pSampleNoRef = Marshal.GetIUnknownForObject(obj);
                Marshal.Release(pSampleNoRef); // No add-ref.
            }

            public bool CompareSample(object obj)
            {
                // QI the sample for IUnknown and compare it to our cached value.
                IntPtr ip1 = Marshal.GetIUnknownForObject(obj);
                Marshal.Release(ip1);

                return ip1 == pSampleNoRef;
            }

            public FrameStepRate state;          // Current frame-step state.
            public Queue<IMFSample> samples;        // List of pending samples for frame-stepping.
            public int steps;          // Number of steps left.
            public IntPtr pSampleNoRef;   // Identifies the frame-step sample.
        }

        #endregion

        #region Members

        protected int m_iDiscarded;

        protected RenderState m_RenderState;          // Rendering state.
        protected FrameStep m_FrameStep;            // Frame-stepping information.

        // Samples and scheduling
        protected Scheduler m_scheduler;        // Manages scheduling of samples.
        protected SamplePool m_SamplePool;           // Pool of allocated samples.
        protected int m_TokenCounter;         // Counter. Incremented whenever we create new samples.

        // Rendering state
        protected bool m_bSampleNotify;     // Did the mixer signal it has an input sample?
        protected bool m_bRepaint;              // Do we need to repaint the last sample?
        protected bool m_bPrerolled;            // Have we presented at least one sample?
        protected bool m_bEndStreaming;     // Did we reach the end of the stream (EOS)?

        protected MFVideoNormalizedRect m_nrcSource;            // Source rectangle.
        protected float m_fRate;                // Playback rate.

        // Deletable objects.
        protected D3DPresentEngine m_pD3DPresentEngine;    // Rendering engine. (Never null if the constructor succeeds.)

        // COM interfaces.
        protected IMFClock m_pClock;               // The EVR's clock.
        protected IMFTransform m_pMixer;               // The mixer.
        protected IMediaEventSink m_pMediaEventSink;      // The EVR's event-sink interface.
        protected IHack m_h2;
        protected IMFMediaType m_pMediaType;           // Output media type

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public EVRCustomPresenter()
        {
            if (System.Threading.Thread.CurrentThread.GetApartmentState() != System.Threading.ApartmentState.MTA)
            {
                throw new Exception("Unsupported theading model");
            }

            m_iDiscarded = 0;
            m_pClock = null;
            m_pMixer = null;
            m_pMediaEventSink = null;
            m_h2 = null;
            m_pMediaType = null;

            m_bSampleNotify = false;
            m_bRepaint = false;
            m_bEndStreaming = false;
            m_bPrerolled = false;

            m_RenderState = RenderState.Shutdown;
            m_fRate = 1.0f;
            m_TokenCounter = 0;

            m_pD3DPresentEngine = new D3DPresentEngine();
            m_FrameStep = new FrameStep();            // Frame-stepping information.

            m_nrcSource = new MFVideoNormalizedRect(0.0f, 0.0f, 1.0f, 1.0f);
            m_scheduler = new Scheduler(D3DPresentEngine.PRESENTER_BUFFER_COUNT, m_pD3DPresentEngine);          // Manages scheduling of samples.
            m_SamplePool = new SamplePool(D3DPresentEngine.PRESENTER_BUFFER_COUNT);           // Pool of allocated samples.

            // Force load of mf.dll now, rather than when we try to start streaming
            DllCanUnloadNow();
        }

        ~EVRCustomPresenter()
        {
            Debug.WriteLine("~EVRCustomPresenter");

            SafeRelease(m_pClock); m_pClock = null;
            SafeRelease(m_pMixer); m_pMixer = null;
            SafeRelease(m_h2); m_h2 = null;
            //SafeRelease(m_pMediaEventSink);
            SafeRelease(m_pMediaType); m_pMediaType = null;

            // Deletable objects
            if (m_pD3DPresentEngine != null)
            {
                m_pD3DPresentEngine.Dispose();
                m_pD3DPresentEngine = null;
            }

            if (m_scheduler != null)
            {
                m_scheduler.Flush();
                m_scheduler = null;
            }

            if (m_SamplePool != null)
            {
                m_SamplePool.Clear();
                m_SamplePool = null;
            }
        }

        #region IMFVideoDeviceID Members

        public int GetDeviceID(out Guid pDeviceID)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                m_pD3DPresentEngine.GetDeviceID(out pDeviceID);

                return S_Ok;
            }
            catch (Exception e)
            {
                pDeviceID = Guid.Empty;
                return Marshal.GetHRForException(e);
            }
        }

        #endregion

        #region IMFVideoPresenter Members

        public int GetCurrentMediaType(out IMFVideoMediaType ppMediaType)
        {
            // Make sure we *never* leave this entry point with an exception
            try
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

                return S_Ok;
            }
            catch (Exception e)
            {
                ppMediaType = null;
                return Marshal.GetHRForException(e);
            }
        }

        int IMFVideoPresenter.OnClockPause(long hnsSystemTime)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                return ((IMFClockStateSink)this).OnClockPause(hnsSystemTime);
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        int IMFVideoPresenter.OnClockRestart(long hnsSystemTime)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                return ((IMFClockStateSink)this).OnClockRestart(hnsSystemTime);
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        int IMFVideoPresenter.OnClockSetRate(long hnsSystemTime, float flRate)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                return ((IMFClockStateSink)this).OnClockSetRate(hnsSystemTime, flRate);
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        int IMFVideoPresenter.OnClockStart(long hnsSystemTime, long llClockStartOffset)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                return ((IMFClockStateSink)this).OnClockStart(hnsSystemTime, llClockStartOffset);
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        int IMFVideoPresenter.OnClockStop(long hnsSystemTime)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                return ((IMFClockStateSink)this).OnClockStop(hnsSystemTime);
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        int IMFVideoPresenter.ProcessMessage(MFVPMessageType eMessage, IntPtr ulParam)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                lock (this)
                {
                    //Debug.WriteLine(eMessage);

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

                return S_Ok;
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        #endregion

        #region IMFClockStateSink Members

        public int OnClockPause(long hnsSystemTime)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                TRACE(("OnClockPause"));

                lock (this)
                {
                    // We cannot pause the clock after shutdown.
                    CheckShutdown();

                    // Set the state. (No other actions are necessary.)
                    m_RenderState = RenderState.Paused;
                }

                return S_Ok;
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        public int OnClockRestart(long hnsSystemTime)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                TRACE(("OnClockRestart"));

                lock (this)
                {
                    CheckShutdown();

                    // The EVR calls OnClockRestart only while paused.
                    Debug.Assert(m_RenderState == RenderState.Paused);

                    m_RenderState = RenderState.Started;

                    // Possibly we are in the middle of frame-stepping OR we have samples waiting
                    // in the frame-step queue. Deal with these two cases first:
                    StartFrameStep();

                    // Now resume the presentation loop.
                    ProcessOutputLoop();
                }

                return S_Ok;
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        public int OnClockSetRate(long hnsSystemTime, float flRate)
        {
            // Make sure we *never* leave this entry point with an exception
            try
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

                return S_Ok;
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        public int OnClockStart(long hnsSystemTime, long llClockStartOffset)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                TRACE(String.Format("OnClockStart (offset = {0})", llClockStartOffset));

                lock (this)
                {
                    // We cannot start after shutdown.
                    CheckShutdown();

                    // Check if the clock is already active (not stopped).
                    if (IsActive())
                    {
                        // May have been paused
                        m_RenderState = RenderState.Started;

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
                        m_RenderState = RenderState.Started;

                        // Possibly we are in the middle of frame-stepping OR have samples waiting
                        // in the frame-step queue. Deal with these two cases first:
                        StartFrameStep();
                    }

                    // Now try to get new output samples from the mixer.
                    ProcessOutputLoop();
                }

                return S_Ok;
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        public int OnClockStop(long hnsSystemTime)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                TRACE(("OnClockStop"));

                lock (this)
                {
                    CheckShutdown();

                    if (m_RenderState != RenderState.Stopped)
                    {
                        m_RenderState = RenderState.Stopped;
                        Flush();

                        // If we are in the middle of frame-stepping, cancel it now.
                        if (m_FrameStep.state != FrameStepRate.None)
                        {
                            CancelFrameStep();
                        }
                    }
                }

                return S_Ok;
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        #endregion

        #region IMFRateSupport Members

        public int GetFastestRate(MFRateDirection eDirection, bool fThin, out float pflRate)
        {
            // Make sure we *never* leave this entry point with an exception
            try
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

                return S_Ok;
            }
            catch (Exception e)
            {
                pflRate = 0;
                return Marshal.GetHRForException(e);
            }
        }

        public int GetSlowestRate(MFRateDirection eDirection, bool fThin, out float pflRate)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                lock (this)
                {
                    CheckShutdown();

                    // There is no minimum playback rate, so the minimum is zero.
                    pflRate = 0;
                }

                return S_Ok;
            }
            catch (Exception e)
            {
                pflRate = 0;
                return Marshal.GetHRForException(e);
            }
        }

        public int IsRateSupported(bool fThin, float flRate, MfFloat pflNearestSupportedRate)
        {
            // Make sure we *never* leave this entry point with an exception
            try
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

                return S_Ok;
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        #endregion

        #region IMFGetServiceAlt Members

        public int GetService(Guid guidService, Guid riid, out IntPtr ppvObject)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                ppvObject = IntPtr.Zero;
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
                    object o;

                    m_pD3DPresentEngine.GetService(guidService, riid, out o);
                    IntPtr ip = Marshal.GetIUnknownForObject(o);
                    Marshal.Release(ip);

                    hr = Marshal.QueryInterface(ip, ref riid, out ppvObject);
                    DsError.ThrowExceptionForHR(hr);
                }
                catch
                {
                    bAgain = true;
                }

                if (bAgain)
                {
                    // Next, QI to check if this object supports the interface.
                    IntPtr ipThis = Marshal.GetIUnknownForObject(this);

                    try
                    {
                        hr = Marshal.QueryInterface(ipThis, ref riid, out ppvObject);

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
                return S_Ok;
            }
            catch (Exception e)
            {
                ppvObject = IntPtr.Zero;
                return Marshal.GetHRForException(e);
            }
        }

        #endregion

        #region IMFTopologyServiceLookupClient Members

        //public void InitServicePointers(IMFTopologyServiceLookup pLookup)
        public int InitServicePointers(IntPtr p1Lookup)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                TRACE(("InitServicePointers"));

                int hr;
                int dwObjectCount = 0;
                IMFTopologyServiceLookup pLookup = null;
                IHack h1 = (IHack)new Hack();

                try
                {
                    h1.Set(p1Lookup, typeof(IMFTopologyServiceLookup).GUID, true);

                    pLookup = (IMFTopologyServiceLookup)h1;

                    lock (this)
                    {
                        // Do not allow initializing when playing or paused.
                        if (IsActive())
                        {
                            throw new COMException("EVRCustomPresenter::InitServicePointers", MFError.MF_E_INVALIDREQUEST);
                        }

                        SafeRelease(m_pClock); m_pClock = null;
                        SafeRelease(m_pMixer); m_pMixer = null;
                        SafeRelease(m_h2); m_h2 = null;
                        m_pMediaEventSink = null; // SafeRelease(m_pMediaEventSink);

                        dwObjectCount = 1;
                        object[] o = new object[1];

                        try
                        {
                            // Ask for the clock. Optional, because the EVR might not have a clock.
                            hr = pLookup.LookupService(
                                MFServiceLookupType.Global,   // Not used.
                                0,                          // Reserved.
                                MFServices.MR_VIDEO_RENDER_SERVICE,    // Service to look up.
                                typeof(IMFClock).GUID,         // Interface to look up.
                                o,
                                ref dwObjectCount              // Number of elements in the previous parameter.
                                );
                            MFError.ThrowExceptionForHR(hr);
                            m_pClock = (IMFClock)o[0];
                        }
                        catch { }

                        // Ask for the mixer. (Required.)
                        dwObjectCount = 1;

                        hr = pLookup.LookupService(
                            MFServiceLookupType.Global,
                            0,
                            MFServices.MR_VIDEO_MIXER_SERVICE,
                            typeof(IMFTransform).GUID,
                            o,
                            ref dwObjectCount
                            );
                        MFError.ThrowExceptionForHR(hr);
                        m_pMixer = (IMFTransform)o[0];

                        // Make sure that we can work with this mixer.
                        ConfigureMixer(m_pMixer);

                        // Ask for the EVR's event-sink interface. (Required.)
                        dwObjectCount = 1;

                        IMFTopologyServiceLookupAlt pLookup2 = (IMFTopologyServiceLookupAlt)pLookup;
                        IntPtr[] p2 = new IntPtr[1];

                        hr = pLookup2.LookupService(
                            MFServiceLookupType.Global,
                            0,
                            MFServices.MR_VIDEO_RENDER_SERVICE,
                            typeof(IMediaEventSink).GUID,
                            p2,
                            ref dwObjectCount
                            );
                        MFError.ThrowExceptionForHR(hr);

                        m_h2 = (IHack)new Hack();

                        m_h2.Set(p2[0], typeof(IMediaEventSink).GUID, false);

                        m_pMediaEventSink = (IMediaEventSink)m_h2;

                        // Successfully initialized. Set the state to "stopped."
                        m_RenderState = RenderState.Stopped;
                    }
                }
                finally
                {
                    SafeRelease(h1);
                }
                return S_Ok;
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        public int ReleaseServicePointers()
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                TRACE(("ReleaseServicePointers"));

                // Enter the shut-down state.
                {
                    lock (this)
                    {
                        m_RenderState = RenderState.Shutdown;
                    }
                }

                // Flush any samples that were scheduled.
                Flush();

                // Clear the media type and release related resources (surfaces, etc).
                SetMediaType(null);

                // Release all services that were acquired from InitServicePointers.
                SafeRelease(m_pClock); m_pClock = null;
                SafeRelease(m_pMixer); m_pMixer = null;
                SafeRelease(m_h2); m_h2 = null;
                m_pMediaEventSink = null; // SafeRelease(m_pMediaEventSink);

                return S_Ok;
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        #endregion

        #region IMFVideoDisplayControl Members

        public int GetAspectRatioMode(out MFVideoAspectRatioMode pdwAspectRatioMode)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                throw new NotImplementedException("The method or operation is not implemented.");
            }
            catch (Exception e)
            {
                pdwAspectRatioMode = MFVideoAspectRatioMode.None;
                return Marshal.GetHRForException(e);
            }
        }

        public int GetBorderColor(out int pClr)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                throw new NotImplementedException("The method or operation is not implemented.");
            }
            catch (Exception e)
            {
                pClr = 0;
                return Marshal.GetHRForException(e);
            }
        }

        public int GetCurrentImage(MediaFoundation.Misc.BitmapInfoHeader pBih, out IntPtr pDib, out int pcbDib, out long pTimeStamp)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                throw new NotImplementedException("The method or operation is not implemented.");
            }
            catch (Exception e)
            {
                pTimeStamp = 0;
                pDib = IntPtr.Zero;
                pcbDib = 0;
                return Marshal.GetHRForException(e);
            }
        }

        public int GetFullscreen(out bool pfFullscreen)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                throw new NotImplementedException("The method or operation is not implemented.");
            }
            catch (Exception e)
            {
                pfFullscreen = false;
                return Marshal.GetHRForException(e);
            }
        }

        public int GetIdealVideoSize(Size pszMin, Size pszMax)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                throw new NotImplementedException("The method or operation is not implemented.");
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        public int GetNativeVideoSize(Size pszVideo, Size pszARVideo)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                throw new NotImplementedException("The method or operation is not implemented.");
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        public int GetRenderingPrefs(out MFVideoRenderPrefs pdwRenderFlags)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                throw new NotImplementedException("The method or operation is not implemented.");
            }
            catch (Exception e)
            {
                pdwRenderFlags = MFVideoRenderPrefs.None;
                return Marshal.GetHRForException(e);
            }
        }

        public int SetAspectRatioMode(MFVideoAspectRatioMode dwAspectRatioMode)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                throw new NotImplementedException("The method or operation is not implemented.");
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        public int SetBorderColor(int Clr)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                throw new NotImplementedException("The method or operation is not implemented.");
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        public int SetFullscreen(bool fFullscreen)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                throw new NotImplementedException("The method or operation is not implemented.");
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        public int SetRenderingPrefs(MFVideoRenderPrefs dwRenderFlags)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                throw new NotImplementedException("The method or operation is not implemented.");
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        public int SetVideoPosition(MFVideoNormalizedRect pnrcSource, MFRect prcDest)
        {
            // Make sure we *never* leave this entry point with an exception
            try
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
                            throw new COMException("Bad source", E_InvalidArgument);
                        }

                        // The source rectangle has range (0..1)
                        if ((pnrcSource.left < 0) || (pnrcSource.right > 1) ||
                            (pnrcSource.top < 0) || (pnrcSource.bottom > 1))
                        {
                            throw new COMException("source has invalid values", E_InvalidArgument);
                        }
                    }

                    if (prcDest != null)
                    {
                        // The destination rectangle cannot be flipped.
                        if ((prcDest.left > prcDest.right) ||
                            (prcDest.top > prcDest.bottom))
                        {
                            throw new COMException("bad destination", E_InvalidArgument);
                        }
                    }

                    // Update the source rectangle. Source clipping is performed by the mixer.
                    if (pnrcSource != null)
                    {
                        m_nrcSource.CopyFrom(pnrcSource);

                        if (m_pMixer != null)
                        {
                            SetMixerSourceRect(m_pMixer, m_nrcSource);
                        }
                    }

                    // Update the destination rectangle.
                    if (prcDest != null)
                    {
                        MFRect rcOldDest = m_pD3DPresentEngine.GetDestinationRect();

                        // Check if the destination rectangle changed.
                        if (!rcOldDest.Equals(prcDest))
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

                return S_Ok;
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        public int SetVideoWindow(IntPtr hwndVideo)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                lock (this)
                {
                    if (!IsWindow(hwndVideo))
                    {
                        throw new COMException("Invalid window handle", E_InvalidArgument);
                    }

                    IntPtr oldHwnd = m_pD3DPresentEngine.GetVideoWindow();

                    // If the window has changed, notify the D3DPresentEngine object.
                    // This will cause a new Direct3D device to be created.
                    if (oldHwnd != hwndVideo)
                    {
                        m_pD3DPresentEngine.SetVideoWindow(hwndVideo);

                        // Tell the EVR that the device has changed.
                        NotifyEvent(EventCode.DisplayChanged, IntPtr.Zero, IntPtr.Zero);
                    }
                }

                return S_Ok;
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        public int GetVideoPosition(MFVideoNormalizedRect pnrcSource, MFRect prcDest)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                lock (this)
                {
                    if (pnrcSource == null || prcDest == null)
                    {
                        throw new COMException("EVRCustomPresenter::GetVideoPosition", E_Pointer);
                    }

                    pnrcSource.CopyFrom(m_nrcSource);
                    prcDest.CopyFrom(m_pD3DPresentEngine.GetDestinationRect());
                }

                return S_Ok;
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        public int GetVideoWindow(out IntPtr phwndVideo)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                lock (this)
                {
                    // The D3DPresentEngine object stores the handle.
                    phwndVideo = m_pD3DPresentEngine.GetVideoWindow();
                }

                return S_Ok;
            }
            catch (Exception e)
            {
                phwndVideo = IntPtr.Zero;
                return Marshal.GetHRForException(e);
            }
        }

        public int RepaintVideo()
        {
            // Make sure we *never* leave this entry point with an exception
            try
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

                return S_Ok;
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        #endregion

        #region IMFAsyncCallback Members

        public int GetParameters(out MFASync pdwFlags, out MFAsyncCallbackQueue pdwQueue)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                pdwQueue = MFAsyncCallbackQueue.Undefined;
                pdwFlags = MFASync.None;
                return Marshal.GetHRForException(e);
            }
        }

        public int Invoke(IMFAsyncResult pAsyncResult)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                try
                {
                    OnSampleFree(pAsyncResult);
                }
                finally
                {
                    SafeRelease(pAsyncResult);
                }

                return S_Ok;
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        #endregion

        #region IQualProp Members

        public int get_FramesDroppedInRenderer(out int pcFrames)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                pcFrames = m_iDiscarded;
                return S_Ok;
            }
            catch (Exception e)
            {
                pcFrames = 0;
                return Marshal.GetHRForException(e);
            }
        }

        public int get_FramesDrawn(out int pcFramesDrawn)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                pcFramesDrawn = m_pD3DPresentEngine.GetFrames();
                return S_Ok;
            }
            catch (Exception e)
            {
                pcFramesDrawn = 0;
                return Marshal.GetHRForException(e);
            }
        }

        public int get_AvgFrameRate(out int piAvgFrameRate)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                piAvgFrameRate = 0;
                return E_NotImplemented;
            }
            catch (Exception e)
            {
                piAvgFrameRate = 0;
                return Marshal.GetHRForException(e);
            }
        }

        public int get_Jitter(out int iJitter)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                iJitter = 0;
                return E_NotImplemented;
            }
            catch (Exception e)
            {
                iJitter = 0;
                return Marshal.GetHRForException(e);
            }
        }

        public int get_AvgSyncOffset(out int piAvg)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                piAvg = 0;
                return E_NotImplemented;
            }
            catch (Exception e)
            {
                piAvg = 0;
                return Marshal.GetHRForException(e);
            }
        }

        public int get_DevSyncOffset(out int piDev)
        {
            // Make sure we *never* leave this entry point with an exception
            try
            {
                piDev = 0;
                return E_NotImplemented;
            }
            catch (Exception e)
            {
                piDev = 0;
                return Marshal.GetHRForException(e);
            }
        }

        #endregion

        #region Protected

        protected static int
        MFGetAttributeUINT32Alt(
            IMFAttributes pAttributes,
            Guid guidKey,
            int unDefault
            )
        {
            int unRet;

            IHack h = new Hack() as IHack;

            try
            {
                IntPtr ip = Marshal.GetIUnknownForObject(pAttributes);

                h.Set(ip, typeof(IMFAttributes).GUID, false);

                IMFAttributes a = (IMFAttributes)h;

                try
                {
                    int hr = a.GetUINT32(guidKey, out unRet);
                    MFError.ThrowExceptionForHR(hr);
                }
                catch
                {
                    unRet = unDefault;
                }
            }
            finally
            {
                Marshal.ReleaseComObject(h);
            }

            return unRet;
        }

        // CheckShutdown:
        //     Returns MF_E_SHUTDOWN if the presenter is shutdown.
        //     Call this at the start of any methods that should fail after shutdown.
        protected void CheckShutdown()
        {
            if (m_RenderState == RenderState.Shutdown)
            {
                throw new COMException("CheckShutdown", MFError.MF_E_SHUTDOWN);
            }
        }

        // IsActive: The "active" state is started or paused.
        protected bool IsActive()
        {
            return ((m_RenderState == RenderState.Started) || (m_RenderState == RenderState.Paused));
        }

        // IsScrubbing: Scrubbing occurs when the frame rate is 0.
        protected bool IsScrubbing() { return m_fRate == 0.0f; }

        // NotifyEvent: Send an e vent to the EVR through its IMediaEventSink interface.
        protected void NotifyEvent(EventCode ec, IntPtr Param1, IntPtr Param2)
        {
            if (m_pMediaEventSink != null)
            {
                m_pMediaEventSink.Notify(ec, Param1, Param2);
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
            int hr;
            Guid deviceID = Guid.Empty;
            Guid myDeviceId;

            IMFVideoDeviceID pDeviceID = null;
            m_pD3DPresentEngine.GetDeviceID(out myDeviceId);

            try
            {
                // Make sure that the mixer has the same device ID as ourselves.
                pDeviceID = (IMFVideoDeviceID)pMixer;
                hr = pDeviceID.GetDeviceID(out deviceID);
                MFError.ThrowExceptionForHR(hr);

                if (deviceID != myDeviceId)
                {
                    throw new COMException("ConfigureMixer", MFError.MF_E_INVALIDREQUEST);
                }

                // Set the zoom rectangle (ie, the source clipping rectangle).
                SetMixerSourceRect(pMixer, m_nrcSource);
            }
            finally
            {
                //SafeRelease(pDeviceID);
            }
        }

        // Formats
        protected void CreateOptimalVideoType(IMFMediaType pProposed, out IMFMediaType ppOptimal)
        {
            try
            {
                MFRect rcOutput;
                MFVideoArea displayArea;

                IMFMediaType pOptimalType = null;

                // Create the helper object to manipulate the optimal type.
                VideoTypeBuilder pmtOptimal = new VideoTypeBuilder();

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
                if (rcOutput.IsEmpty())
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
                displayArea = new MFVideoArea(0, 0, rcOutput.right, rcOutput.bottom);

                pmtOptimal.SetPanScanEnabled(false);

                pmtOptimal.SetGeometricAperture(displayArea);

                // Set the pan/scan aperture and the minimum display aperture. We don't care
                // about them per se, but the mixer will reject the type if these exceed the
                // frame dimentions.
                pmtOptimal.SetPanScanAperture(displayArea);
                pmtOptimal.SetMinDisplayAperture(displayArea);

                // Return the pointer to the caller.
                pmtOptimal.GetMediaType(out pOptimalType);
                pmtOptimal.Dispose();

                ppOptimal = pOptimalType;
            }
            finally
            {
                //SafeRelease(pOptimalType);
                //SafeRelease(pmtOptimal);
            }
        }

        protected void CalculateOutputRectangle(IMFMediaType pProposed, out MFRect prcOutput)
        {
            int srcWidth = 0, srcHeight = 0;

            MFRatio inputPAR;
            MFRatio outputPAR;
            MFRect rcOutput = new MFRect();

            MFVideoArea displayArea;

            VideoTypeBuilder pmtProposed = null;

            // Helper object to read the media type.
            pmtProposed = new VideoTypeBuilder(pProposed);

            // Get the source's frame dimensions.
            pmtProposed.GetFrameDimensions(out srcWidth, out srcHeight);

            // Get the source's display area.
            pmtProposed.GetVideoDisplayArea(out displayArea);

            // Calculate the x,y offsets of the display area.
            int offsetX = (int)displayArea.OffsetX.GetOffset();
            int offsetY = (int)displayArea.OffsetY.GetOffset();

            // Use the display area if valid. Otherwise, use the entire frame.
            if (displayArea.Area.Width != 0 &&
                displayArea.Area.Height != 0 &&
                offsetX + displayArea.Area.Width <= (srcWidth) &&
                offsetY + displayArea.Area.Height <= (srcHeight))
            {
                rcOutput.left = offsetX;
                rcOutput.right = offsetX + displayArea.Area.Width;
                rcOutput.top = offsetY;
                rcOutput.bottom = offsetY + displayArea.Area.Height;
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

            pmtProposed.Dispose();
        }

        protected void SetMediaType(IMFMediaType pMediaType)
        {
            // Note: pMediaType can be NULL (to clear the type)
            int hr;

            // Clearing the media type is allowed in any state (including shutdown).
            if (pMediaType == null)
            {
                SafeRelease(m_pMediaType);
                m_pMediaType = null;
                ReleaseResources();
                return;
            }

            try
            {
                MFRatio fps;
                Queue<IMFSample> sampleQueue = new Queue<IMFSample>();

                // Cannot set the media type after shutdown.
                CheckShutdown();

                // Check if the new type is actually different.
                // Note: This function safely handles NULL input parameters.
                if (Utils.AreMediaTypesEqual(m_pMediaType, pMediaType))
                {
                    return; // Nothing more to do.
                }

                // We're really changing the type. First get rid of the old type.
                SafeRelease(m_pMediaType); m_pMediaType = null;

                ReleaseResources();

                // Initialize the presenter engine with the new media type.
                // The presenter engine allocates the samples.

                m_pD3DPresentEngine.CreateVideoSamples(pMediaType, sampleQueue);

                // Mark each sample with our token counter. If this batch of samples becomes
                // invalid, we increment the counter, so that we know they should be discarded.

                foreach (IMFSample pSample1 in sampleQueue)
                {
                    hr = pSample1.SetUINT32(MFSamplePresenter_SampleCounter, m_TokenCounter);
                    MFError.ThrowExceptionForHR(hr);
                }

                // Add the samples to the sample pool.
                m_SamplePool.Initialize(sampleQueue);

                // Initialize takes over the queue
                sampleQueue = null;

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
                if (pMediaType != m_pMediaType)
                {
                    m_pMediaType = pMediaType;
                }
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

            bool bCompressed = false;
            MFVideoInterlaceMode InterlaceMode = MFVideoInterlaceMode.Unknown;
            MFVideoArea VideoCropArea;
            int width = 0, height = 0;

            try
            {
                // Helper object for reading the proposed type.
                pProposed = new VideoTypeBuilder(pMediaType);

                // Reject compressed media types.
                pProposed.IsCompressedFormat(out bCompressed);
                if (bCompressed)
                {
                    throw new COMException("Compressed formats not supported", MFError.MF_E_INVALIDMEDIATYPE);
                }

                // Validate the format.
                int i;
                pProposed.GetFourCC(out i);

                // The D3DPresentEngine checks whether the format can be used as
                // the back-buffer format for the swap chains.
                m_pD3DPresentEngine.CheckFormat(i);

                // Reject interlaced formats.
                pProposed.GetInterlaceMode(out InterlaceMode);
                if (InterlaceMode != MFVideoInterlaceMode.Progressive)
                {
                    throw new COMException("Interlaced formats not supported", MFError.MF_E_INVALIDMEDIATYPE);
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
                pProposed.Dispose();
                //SafeRelease(pMediaType);
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
            while (m_FrameStep.samples.Count > 0)
            {
                SafeRelease(m_FrameStep.samples.Dequeue());
            }

            if (m_RenderState == RenderState.Stopped)
            {
                // Repaint with black.
                m_pD3DPresentEngine.PresentSample(null, 0);
            }
        }

        protected void RenegotiateMediaType()
        {
            TRACE(("RenegotiateMediaType"));

            int hr;
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
                    SafeRelease(pMixerType); pMixerType = null;
                    SafeRelease(pOptimalType); pOptimalType = null;

                    // Step 1. Get the next media type supported by mixer.
                    hr = m_pMixer.GetOutputAvailableType(0, iTypeIndex++, out pMixerType);
                    MFError.ThrowExceptionForHR(hr);

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
                        hr = m_pMixer.SetOutputType(0, pOptimalType, MFTSetTypeFlags.TestOnly);
                        MFError.ThrowExceptionForHR(hr);

                        // Step 5. Try to set the media type on ourselves.
                        SetMediaType(pOptimalType);

                        // Step 6. Set output media type on mixer.
                        try
                        {
                            hr = m_pMixer.SetOutputType(0, pOptimalType, 0);
                            MFError.ThrowExceptionForHR(hr);
                            bFoundMediaType = true;

                            // Don't free this one.  We're using it.
                            pOptimalType = null;
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
                SafeRelease(pOptimalType); pOptimalType = null;
                SafeRelease(pMixerType); pMixerType = null;
                SafeRelease(pVideoType); pVideoType = null;
            }
        }

        protected void ProcessInputNotify()
        {
            // Set the flag that says the mixer has a new sample.
            m_bSampleNotify = true;

            if (m_pMediaType == null)
            {
                // We don't have a valid media type yet.
                throw new COMException("We don't have a valid media type yet", MFError.MF_E_TRANSFORM_TYPE_NOT_SET);
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
            NotifyEvent(EventCode.Complete, IntPtr.Zero, IntPtr.Zero);
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
                    hr = MFError.MF_E_TRANSFORM_NEED_MORE_INPUT;
                    break;
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

        protected void ReleaseEventCollection(int cOutputBuffers, MFTOutputDataBuffer[] pBuffers)
        {
            for (int i = 0; i < cOutputBuffers; i++)
            {
                SafeRelease(pBuffers[i].pEvents);
                pBuffers[i].pEvents = null;
            }
        }

        protected int ProcessOutput()
        {
            Debug.Assert(m_bSampleNotify || m_bRepaint);  // See note above.

            int hr = S_Ok;
            ProcessOutputStatus dwStatus = 0;
            long mixerStartTime = 0, mixerEndTime = 0;
            long systemTime = 0;
            bool bRepaint = m_bRepaint; // Temporarily store this state flag.

            MFTOutputDataBuffer[] dataBuffer = new MFTOutputDataBuffer[1];

            IMFSample pSample = null;

            // If the clock is not running, we present the first sample,
            // and then don't present any more until the clock starts.

            if ((m_RenderState != RenderState.Started) &&  // Not running.
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

            if (!m_bRepaint)
            {
                MFTOutputStatusFlags osf;
                hr = m_pMixer.GetOutputStatus(out osf);
                MFError.ThrowExceptionForHR(hr);

                if ((osf & MFTOutputStatusFlags.SampleReady) == 0)
                {
                    m_bSampleNotify = false;
                    return S_Ok;
                }
            }

            try
            {
                // Try to get a free sample from the video sample pool.
                m_SamplePool.GetSample(out pSample);

                if (pSample == null)
                {
                    return S_False; // No free samples. We'll try again when a sample is released.
                }

                // (If the following assertion fires, it means we are not managing the sample pool correctly.)
                //Debug.Assert(MFGetAttributeUINT32(pSample, MFSamplePresenter_SampleCounter, -1) == m_TokenCounter);

                if (m_bRepaint)
                {
                    // Repaint request. Ask the mixer for the most recent sample.
                    SetDesiredSampleTime(pSample, m_scheduler.LastSampleTime(), m_scheduler.FrameDuration());
                    m_bRepaint = false; // OK to clear this flag now.
                }
                else
                {
                    // Not a repaint request. Clear the desired sample time; the mixer will
                    // give us the next frame in the stream.
                    ClearDesiredSampleTime(pSample);

                    if (m_pClock != null)
                    {
                        // Latency: Record the starting time for the ProcessOutput operation.
                        hr = m_pClock.GetCorrelatedTime(0, out mixerStartTime, out systemTime);
                        MFError.ThrowExceptionForHR(hr);
                    }
                }

                // Now we are ready to get an output sample from the mixer.
                dataBuffer[0].dwStreamID = 0;
                dataBuffer[0].dwStatus = 0;
                dataBuffer[0].pEvents = null;
                dataBuffer[0].pSample = Marshal.GetIUnknownForObject(pSample);

                try
                {
                    hr = m_pMixer.ProcessOutput(0, 1, dataBuffer, out dwStatus);
                    MFError.ThrowExceptionForHR(hr);

                    // Release any events that were returned from the ProcessOutput method.
                    // (We don't expect any events from the mixer, but this is a good practice.)
                    ReleaseEventCollection(dataBuffer.Length, dataBuffer);
                }
                catch (Exception e)
                {
                    hr = Marshal.GetHRForException(e);
                }
                finally
                {
                    Marshal.Release(dataBuffer[0].pSample);
                    //SafeRelease(dataBuffer[0].pSample);
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
                    else
                    {
                        MFError.ThrowExceptionForHR(hr);
                    }
                }
                else
                {
                    // We got an output sample from the mixer.

                    if (m_pClock != null && !bRepaint)
                    {
                        // Latency: Record the ending time for the ProcessOutput operation,
                        // and notify the EVR of the latency.

                        hr = m_pClock.GetCorrelatedTime(0, out mixerEndTime, out systemTime);
                        MFError.ThrowExceptionForHR(hr);
                        long latencyTime = mixerEndTime - mixerStartTime;

                        GCHandle gh = GCHandle.Alloc(latencyTime, GCHandleType.Pinned);

                        try
                        {
                            // This event (EventCode.ProcessingLatency) isn't defined until DirectShowNet v2.1
                            NotifyEvent((EventCode)0x21, gh.AddrOfPinnedObject(), IntPtr.Zero);
                        }
                        finally
                        {
                            gh.Free();
                        }
                    }

                    // Set up notification for when the sample is released.
                    TrackSample(pSample);

                    // Schedule the sample.
                    if ((m_FrameStep.state == FrameStepRate.None) || bRepaint)
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
                //SafeRelease(pSample);
            }

            return S_Ok;
        }

        protected void DeliverSample(IMFSample pSample, bool bRepaint)
        {
            Debug.Assert(pSample != null);

            D3DPresentEngine.DeviceState state;

            // If we are not actively playing, OR we are scrubbing (rate = 0) OR this is a
            // repaint request, then we need to present the sample immediately. Otherwise,
            // schedule it normally.

            bool bPresentNow = ((m_RenderState != RenderState.Started) || IsScrubbing() || bRepaint);

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
                NotifyEvent(EventCode.ErrorAbort, new IntPtr(hr), IntPtr.Zero);
                throw;
            }

            if (state == D3DPresentEngine.DeviceState.DeviceReset)
            {
                // The Direct3D device was re-set. Notify the EVR.
                NotifyEvent(EventCode.DisplayChanged, IntPtr.Zero, IntPtr.Zero);
            }
        }

        protected void TrackSample(IMFSample pSample)
        {
            IMFTrackedSample pTracked = null;

            pTracked = (IMFTrackedSample)pSample;
            int hr = pTracked.SetAllocator(this, null);
            MFError.ThrowExceptionForHR(hr);
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
            m_FrameStep.state = FrameStepRate.WaitingStart;

            // If the clock is are already running, we can start frame-stepping now.
            // Otherwise, we will start when the clock starts.
            if (m_RenderState == RenderState.Started)
            {
                StartFrameStep();
            }
        }

        protected void StartFrameStep()
        {
            Debug.Assert(m_RenderState == RenderState.Started);

            IMFSample pSample = null;

            try
            {
                if (m_FrameStep.state == FrameStepRate.WaitingStart)
                {
                    // We have a frame-step request, and are waiting for the clock to start.
                    // Set the state to "pending," which means we are waiting for samples.
                    m_FrameStep.state = FrameStepRate.Pending;

                    // If the frame-step queue already has samples, process them now.
                    while (m_FrameStep.samples.Count > 0 && (m_FrameStep.state == FrameStepRate.Pending))
                    {
                        pSample = (IMFSample)m_FrameStep.samples.Dequeue();
                        DeliverFrameStepSample(pSample);
                        //SafeRelease(pSample);

                        // We break from this loop when:
                        //   (a) the frame-step queue is empty, or
                        //   (b) the frame-step operation is complete.
                    }
                }
                else if (m_FrameStep.state == FrameStepRate.None)
                {
                    // We are not frame stepping. Therefore, if the frame-step queue has samples,
                    // we need to process them normally.
                    while (m_FrameStep.samples.Count > 0)
                    {
                        pSample = (IMFSample)m_FrameStep.samples.Dequeue();
                        DeliverSample(pSample, false);
                        //SafeRelease(pSample);
                    }
                }
            }
            finally
            {
                //SafeRelease(pSample);
            }
        }

        protected void DeliverFrameStepSample(IMFSample pSample)
        {
            // For rate 0, discard any sample that ends earlier than the clock time.
            if (IsScrubbing() && (m_pClock != null) && IsSampleTimePassed(m_pClock, pSample))
            {
                // Discard this sample.
                Marshal.ReleaseComObject(pSample);
            }
            else if (m_FrameStep.state >= FrameStepRate.Scheduled)
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
                    Marshal.ReleaseComObject(pSample);
                }
                else if (m_FrameStep.state == FrameStepRate.WaitingStart)
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

                    // Save this value.
                    m_FrameStep.SetSample(pSample);

                    // NOTE: We do not AddRef the IUnknown pointer, because that would prevent the
                    // sample from invoking the OnSampleFree callback after the sample is presented.
                    // We use this IUnknown pointer purely to identify the sample later; we never
                    // attempt to dereference the pointer.

                    // Update our state.
                    m_FrameStep.state = FrameStepRate.Scheduled;
                }
            }
        }

        protected void CompleteFrameStep(IMFSample pSample)
        {
            int hr;
            long hnsSampleTime = 0;
            long hnsSystemTime = 0;

            // Update our state.
            m_FrameStep.state = FrameStepRate.Complete;
            m_FrameStep.pSampleNoRef = IntPtr.Zero;

            // Notify the EVR that the frame-step is complete.
            NotifyEvent(EventCode.StepComplete, IntPtr.Zero, IntPtr.Zero); // FALSE = completed (not cancelled)

            // If we are scrubbing (rate == 0), also send the "scrub time" event.
            if (IsScrubbing())
            {
                // Get the time stamp from the sample.

                try
                {
                    hr = pSample.GetSampleTime(out hnsSampleTime);
                    MFError.ThrowExceptionForHR(hr);
                }
                catch
                {
                    // No time stamp. Use the current presentation time.
                    if (m_pClock != null)
                    {
                        hr = m_pClock.GetCorrelatedTime(0, out hnsSampleTime, out hnsSystemTime);
                        MFError.ThrowExceptionForHR(hr);
                    }
                }

                // This event (EventCode.ScrubTime) isn't defined until DirectShowNet v2.1
                NotifyEvent((EventCode)0x23, new IntPtr((int)hnsSampleTime), new IntPtr(hnsSampleTime >> 32));
            }
        }

        protected void CancelFrameStep()
        {
            FrameStepRate oldState = m_FrameStep.state;

            m_FrameStep.state = FrameStepRate.None;
            m_FrameStep.steps = 0;
            m_FrameStep.pSampleNoRef = IntPtr.Zero;
            // Don't clear the frame-step queue yet, because we might frame step again.

            if (oldState > FrameStepRate.None && oldState < FrameStepRate.Complete)
            {
                // We were in the middle of frame-stepping when it was cancelled.
                // Notify the EVR.
                NotifyEvent(EventCode.StepComplete, new IntPtr(1), IntPtr.Zero); // TRUE = cancelled
            }
        }

        // Callbacks

        // Callback when a video sample is released.
        public void OnSampleFree(IMFAsyncResult pResult)
        {
            object pObject = null;
            IMFSample pSample = null;

            try
            {
                // Get the sample from the async result object.
                int hr = pResult.GetStatus();
                MFError.ThrowExceptionForHR(hr);

                hr = pResult.GetObject(out pObject);
                MFError.ThrowExceptionForHR(hr);
                pSample = (IMFSample)pObject;

                // If this sample was submitted for a frame-step, then the frame step is complete.
                if (m_FrameStep.state == FrameStepRate.Scheduled)
                {
                    if (m_FrameStep.CompareSample(pSample))
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
                    if (MFGetAttributeUINT32Alt(pSample, MFSamplePresenter_SampleCounter, -1) == m_TokenCounter)
                    {
                        // Return the sample to the sample pool.
                        m_SamplePool.ReturnSample(pSample);

                        // Now that a free sample is available, process more data if possible.
                        ProcessOutputLoop();
                    }
                    else
                    {
                        Marshal.ReleaseComObject(pSample);
                    }
                }

            }
            catch (Exception e)
            {
                int hr = Marshal.GetHRForException(e);
                NotifyEvent(EventCode.ErrorAbort, new IntPtr(hr), IntPtr.Zero);
            }
            finally
            {
                //SafeRelease(pObject); pObject = null;
                //SafeRelease(pSample);
                //SafeRelease(pResult);
            }
        }

        protected void SetMixerSourceRect(IMFTransform pMixer, MFVideoNormalizedRect nrcSource)
        {
            if (pMixer == null)
            {
                throw new COMException("SetMixerSourceRect", E_Pointer);
            }

            int hr;
            IMFAttributes pAttributes = null;

            hr = pMixer.GetAttributes(out pAttributes);
            MFError.ThrowExceptionForHR(hr);

            Utils.MFSetBlob(pAttributes, MFAttributesClsid.VIDEO_ZOOM_RECT, nrcSource);

            SafeRelease(pAttributes); pAttributes = null;
        }

        protected void ValidateVideoArea(MFVideoArea area, int width, int height)
        {
            float fOffsetX = area.OffsetX.GetOffset();
            float fOffsetY = area.OffsetY.GetOffset();

            if (((int)fOffsetX + area.Area.Width > width) ||
                 ((int)fOffsetY + area.Area.Height > height))
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

            //SafeRelease(pDesired);
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
            int hr;

            hr = pSample.GetUINT32(MFSamplePresenter_SampleCounter, out counter);
            MFError.ThrowExceptionForHR(hr);

            try
            {
                Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");
                hr = pSample.GetUnknown(MFSamplePresenter_SampleSwapChain, IID_IUnknown, out pUnkSwapChain);
                MFError.ThrowExceptionForHR(hr);
            }
            catch { }


            pDesired = (IMFDesiredSample)pSample;

            hr = pDesired.Clear();
            MFError.ThrowExceptionForHR(hr);

            hr = pSample.SetUINT32(MFSamplePresenter_SampleCounter, counter);
            MFError.ThrowExceptionForHR(hr);

            if (pUnkSwapChain != null)
            {
                hr = pSample.SetUnknown(MFSamplePresenter_SampleSwapChain, pUnkSwapChain);
                MFError.ThrowExceptionForHR(hr);
            }

            SafeRelease(pUnkSwapChain);
            //SafeRelease(pDesired);
        }

        protected bool IsSampleTimePassed(IMFClock pClock, IMFSample pSample)
        {
            Debug.Assert(pClock != null);
            Debug.Assert(pSample != null);

            if (pSample == null || pClock == null)
            {
                throw new COMException("IsSampleTimePassed", E_Pointer);
            }

            int hr;
            bool bRet = false;
            long hnsTimeNow = 0;
            long hnsSystemTime = 0;
            long hnsSampleStart = 0;
            long hnsSampleDuration = 0;

            // The sample might lack a time-stamp or a duration, and the
            // clock might not report a time.

            try
            {
                hr = pClock.GetCorrelatedTime(0, out hnsTimeNow, out hnsSystemTime);
                MFError.ThrowExceptionForHR(hr);
                hr = pSample.GetSampleTime(out hnsSampleStart);
                MFError.ThrowExceptionForHR(hr);
                hr = pSample.GetSampleDuration(out hnsSampleDuration);
                MFError.ThrowExceptionForHR(hr);

                if (hnsSampleStart + hnsSampleDuration < hnsTimeNow)
                {
                    bRet = true;
                }
            }
            catch { }

            return bRet;
        }

        protected MFRect CorrectAspectRatio(MFRect src, MFRatio srcPAR, MFRatio destPAR)
        {
            // Start with a rectangle the same size as src, but offset to the origin (0,0).
            MFRect rc = new MFRect(0, 0, src.right - src.left, src.bottom - src.top);

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
    }
}
