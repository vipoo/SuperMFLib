/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.Utils;
using Utils;

namespace WavSourceFilter
{
    internal class Utils
    {
        const long ONE_SECOND = 10000000;

        static public bool IsAligned(long t, int align)
        {
            return (t % align) == 0;
        }

        static public int AlignUp(int num, int mult)
        {
            Debug.Assert(num >= 0);
            int tmp = num + mult - 1;
            return tmp - (tmp % mult);
        }

        static public long MulDiv(int a, int b, int c)
        {
            long l = a;
            l *= b;
            l /= c;

            return l;
        }

        /// <summary>
        /// QueueEventWithIUnknown
        /// </summary>
        /// <remarks>Helper function to queue an event with an IUnknown pointer value.</remarks>
        /// <param name="pMEG">Media event generator that will queue the event.</param>
        /// <param name="meType">Media event type.</param>
        /// <param name="hrStatus">Status code for the event.</param>
        /// <param name="pUnk">IUnknown pointer value.</param>
        /// <returns></returns>
        static public int QueueEventWithIUnknown(
            IMFMediaEventGenerator pMEG,
            MediaEventType meType,
            int hrStatus,
            object pUnk)
        {
            // Queue the event.
            int hr = pMEG.QueueEvent(meType, Guid.Empty, hrStatus, pUnk);

            return hr;
        }

        static public long AudioDurationFromBufferSize(WaveFormatEx pWav, int cbAudioDataSize)
        {
            Debug.Assert(pWav != null);

            if (pWav.nAvgBytesPerSec == 0)
            {
                return 0;
            }
            return Utils.MulDiv(cbAudioDataSize, 10000000, pWav.nAvgBytesPerSec);
        }

        static public long BufferSizeFromAudioDuration(WaveFormatEx pWav, long duration)
        {
            long cbSize = duration * pWav.nAvgBytesPerSec / ONE_SECOND;

            int ulRemainder = (int)(cbSize % pWav.nBlockAlign);

            // Round up to the next block.
            if (ulRemainder != 0)
            {
                cbSize += pWav.nBlockAlign - ulRemainder;
            }

            return cbSize;
        }
    }

    [ClassInterface(ClassInterfaceType.None)]
    public class WavStream : COMBase, IMFMediaStreamAlt, IDisposable
    {
        #region Member Variables

        xLog m_Log;

        bool m_IsShutdown;           // Flag to indicate if source's Shutdown() method was called.
        long m_rtCurrentPosition;    // Current position in the stream, in 100-ns units
        bool m_discontinuity;        // Is the next sample a discontinuity?
        bool m_EOS;                  // Did we reach the end of the stream?

        CWavRiffParser m_Riff;
        IMFMediaEventQueueAlt m_pEventQueue;         // Event generator helper.
        WavSource m_pSource;             // Parent media source
        IMFStreamDescriptor m_pStreamDescriptor;   // Stream descriptor for this stream.

        #endregion

        public WavStream(WavSource pSource, CWavRiffParser pRiff, IMFStreamDescriptor pSD, int hr)
        {
            m_pEventQueue = null;
            m_Log = new xLog("WavStream");
#if false
            m_nRefCount(0),
            m_IsShutdown(false),
            m_rtCurrentPosition(0),
            m_discontinuity(false),
            m_EOS(false)
#endif

            m_pSource = pSource;

            m_pStreamDescriptor = pSD;

            m_Riff = pRiff;

            // Create the media event queue.
            hr = MFDllAlt.MFCreateEventQueue(out m_pEventQueue);

        }

        ~WavStream()
        {
            Debug.Assert(m_IsShutdown);
        }

        #region IMFMediaEventGenerator methods

        public int BeginGetEvent(
            //IMFAsyncCallback pCallback,
            IntPtr pCallback,
            object punkState
            //IntPtr o
            )
        {
            m_Log.WriteLine("-BeginGetEvent");
            int hr = S_Ok;

            lock (this)
            {
                hr = CheckShutdown();

                if (Succeeded(hr))
                {
                    hr = m_pEventQueue.BeginGetEvent(pCallback, punkState);
                }
            }

            return hr;
        }

        public int EndGetEvent(
            //IMFAsyncResult pResult, 
            IntPtr pResult, 
            out IMFMediaEvent ppEvent
            //IntPtr ppEvent
            )
        {
            m_Log.WriteLine("-EndGetEvent");
            int hr = S_Ok;
            ppEvent = null;

            lock (this)
            {
                hr = CheckShutdown();

                if (Succeeded(hr))
                {
                    hr = m_pEventQueue.EndGetEvent(pResult, out ppEvent);
                }
            }

            return hr;
        }

        public int GetEvent(MFEventFlag dwFlags, out IMFMediaEvent ppEvent)
        {
            m_Log.WriteLine("-GetEvent");
            int hr = S_Ok;
            ppEvent = null;

            IMFMediaEventQueue pQueue = null;

            { // scope for lock

                lock (this)
                {

                    hr = CheckShutdown();

                    if (Succeeded(hr))
                    {
                        pQueue = m_pEventQueue as IMFMediaEventQueue;
                    }
                }

            }

            if (Succeeded(hr))
            {
                hr = pQueue.GetEvent(dwFlags, out ppEvent);
            }

            //not needed SAFE_RELEASE(pQueue);

            return hr;
        }

        public int QueueEvent(MediaEventType met, Guid guidExtendedType, int hrStatus, object pvValue)
        {
            m_Log.WriteLine("-QueueEvent");
            int hr = S_Ok;

            lock (this)
            {
                hr = CheckShutdown();

                if (Succeeded(hr))
                {
                    int iObjectSize;

                    if (IntPtr.Size == 4)
                    {
                        iObjectSize = 16;
                    }
                    else
                    {
                        iObjectSize = 24;
                    }
                    IntPtr ip = Marshal.AllocCoTaskMem(iObjectSize);
                    try
                    {
                        Marshal.GetNativeVariantForObject(pvValue, ip);
                        hr = m_pEventQueue.QueueEventParamVar(met, guidExtendedType, hrStatus, ip);
                    }
                    finally
                    {
                        Marshal.FreeCoTaskMem(ip);
                    }
                }
            }

            return hr;
        }

        #endregion

        #region IMFMediaStream methods.

        public int GetMediaSource(out IMFMediaSource ppMediaSource)
        {
            m_Log.WriteLine("-GetMediaSource");
            int hr;
            ppMediaSource = null;

            lock (this)
            {
                if (m_pSource == null)
                {
                    return E_Unexpected;
                }

                hr = CheckShutdown();

                if (Succeeded(hr))
                {
                    ppMediaSource = m_pSource as IMFMediaSource;
                }
            }

            return hr;
        }

        public int GetStreamDescriptor(out IMFStreamDescriptor ppStreamDescriptor)
        {
            m_Log.WriteLine("-GetStreamDescriptor");
            int hr;
            ppStreamDescriptor = null;

            lock (this)
            {
                if (m_pStreamDescriptor == null)
                {
                    return E_Unexpected;
                }

                hr = CheckShutdown();

                if (Succeeded(hr))
                {
                    ppStreamDescriptor = m_pStreamDescriptor;
                }
            }

            return hr;
        }

        public int RequestSample(IUnknown pToken)
        {
            m_Log.WriteLine("-RequestSample");

            if (m_pSource == null)
            {
                return E_Unexpected;
            }

            int hr = S_Ok;

            IMFSample pSample = null;  // Sample to deliver.
            bool bReachedEOS = false;   // true if we hit end-of-stream during this method.

            {
                // Scope for critical section
                lock (this)
                {

                    // Check if we are shut down.
                    hr = CheckShutdown();

                    // Check if we already reached the end of the stream.
                    if (Succeeded(hr))
                    {
                        if (m_EOS)
                        {
                            hr = MFError.MF_E_END_OF_STREAM;
                        }
                    }

                    // Check the source is stopped.
                    if (Succeeded(hr))
                    {
                        // GetState does not hold the source's critical section. Safe to call.
                        if (m_pSource.GetState() == WavSource.State.Stopped)
                        {
                            hr = MFError.MF_E_INVALIDREQUEST;
                        }
                    }

                    // If we Succeeded to here, we are able to deliver a sample.

                    // Create a new audio sample.
                    if (Succeeded(hr))
                    {
                        hr = CreateAudioSample(out pSample);
                    }

                    // If the caller provided a token, attach it to the sample as
                    // an attribute.
                    if (Succeeded(hr))
                    {
                        // NOTE: If we processed sample requests asynchronously, we would
                        // need to call AddRef on the token and put the token onto a FIFO
                        // queue. See documenation for IMFMediaStream::RequestSample.
                        if (pToken != null)
                        {
                            object o = pToken;
                            hr = pSample.SetUnknown(MFAttributesClsid.MFSampleExtension_Token, o);
                        }
                    }

                    // Send the MEMediaSample event with the new sample.
                    if (Succeeded(hr))
                    {
                        object o = pSample;
                        hr = QueueEvent(MediaEventType.MEMediaSample, Guid.Empty, hr, o);
                    }

                    // See if we reached the end of the stream.
                    if (Succeeded(hr))
                    {
                        hr = CheckEndOfStream();    // This method sends MEEndOfStream if needed.
                        bReachedEOS = m_EOS;        // Cache this flag in a local variable.
                    }

                    //probably not needed SAFE_RELEASE(pSample);
                }

            }  // Scope for critical section lock.


            // We only have one stream, so the end of the stream is also the end of the
            // presentation. Therefore, when we reach the end of the stream, we need to
            // queue the end-of-presentation event from the source. Logically we would do
            // this inside the CheckEndOfStream method. However, we cannot hold the
            // source's critical section while holding the stream's critical section, at
            // risk of deadlock.

            if (Succeeded(hr))
            {
                if (bReachedEOS)
                {
                    object o = null;

                    hr = m_pSource.QueueEvent(MediaEventType.MEEndOfPresentation, Guid.Empty, S_Ok, o);
                }
            }

            return hr;
        }

        #endregion

        #region Other Public methods

        internal long GetCurrentPosition() 
        {
            m_Log.WriteLine("GetCurrentPosition");
            return m_rtCurrentPosition; 
        }

        internal int SetPosition(long rtNewPosition)
        {
            m_Log.WriteLine("SetPosition");
            int hr;

            lock (this)
            {

                // Check if the requested position is beyond the end of the stream.
                long duration = Utils.AudioDurationFromBufferSize(m_Riff.Format(), m_Riff.Chunk().DataSize());

                if (rtNewPosition > duration)
                {
                    return E_InvalidArgument;
                }

                hr = S_Ok;

                if (m_rtCurrentPosition != rtNewPosition)
                {
                    long offset = Utils.BufferSizeFromAudioDuration(m_Riff.Format(), rtNewPosition);

                    // The chunk size is a int. So if our calculations are correct, there is no
                    // way that the maximum valid seek position can be larger than a int.
                    Debug.Assert(offset <= int.MaxValue);

                    hr = m_Riff.MoveToChunkOffset((int)offset);
                    if (Succeeded(hr))
                    {
                        m_rtCurrentPosition = rtNewPosition;
                        m_discontinuity = true;
                        m_EOS = false;
                    }
                }
            }

            return hr;
        }

        internal int Shutdown()
        {
            m_Log.WriteLine("Shutdown");
            lock (this)
            {

                // Shut down the event queue.
                if (m_pEventQueue != null)
                {
                    m_pEventQueue.Shutdown();
                }

                // Release objects
                //SAFE_RELEASE(m_pEventQueue);
                //SAFE_RELEASE(m_pSource);
                //SAFE_RELEASE(m_pStreamDescriptor);
                //SAFE_RELEASE(m_Riff);

                m_IsShutdown = true;
            }

            return S_Ok;
        }

        #endregion

        #region Private methods

        private int CheckEndOfStream()
        {
            m_Log.WriteLine("CheckEndOfStream");
            int hr = S_Ok;

            if (m_Riff.BytesRemainingInChunk() < m_Riff.Format().nBlockAlign)
            {
                // The remaining data is smaller than the audio block size. (In theory there shouldn't be
                // partial bits of data at the end, so we should reach an even zero bytes, but the file
                // might not be authored correctly.)
                m_EOS = true;

                // Send the end-of-stream event,
                object o = null;

                hr = QueueEvent(MediaEventType.MEEndOfStream, Guid.Empty, S_Ok, o);
            }
            return hr;
        }

        private int CheckShutdown()
        {
            m_Log.WriteLine("CheckShutdown");
            int hr;

            if (m_IsShutdown)
            {
                hr = MFError.MF_E_SHUTDOWN;
            }
            else
            {
                hr = S_Ok;
            }

            return hr;
        }

        private int CreateAudioSample(out IMFSample ppSample)
        {
            m_Log.WriteLine("CreateAudioSample");
            int hr = S_Ok;
            ppSample = null;

            IMFMediaBuffer pBuffer = null;
            IMFSample pSample = null;

            int cbBuffer = 0;
            IntPtr pData = IntPtr.Zero;
            long duration = 0;
            bool bBufferLocked = false;

            // Start with one second of data, rounded up to the nearest block.
            cbBuffer = Utils.AlignUp(m_Riff.Format().nAvgBytesPerSec, m_Riff.Format().nBlockAlign);

            // Don't request any more than what's left.
            cbBuffer = Math.Min(cbBuffer, m_Riff.BytesRemainingInChunk());

            // Create the buffer.
            hr = MFDll.MFCreateMemoryBuffer(cbBuffer, out pBuffer);

            if (Succeeded(hr))
            {
                int a, b;

                // Get a pointer to the buffer memory.
                hr = pBuffer.Lock(out pData, out a, out b);

                // Set this flag so that we're sure to unlock the buffer, even if
                // the next call fails.
                bBufferLocked = Succeeded(hr);
            }

            // Fill the buffer
            if (Succeeded(hr))
            {
                hr = m_Riff.ReadDataFromChunk(pData, cbBuffer);
            }

            // Unlock the buffer.
            if (bBufferLocked)
            {
                hr = pBuffer.Unlock();
            }

            // Set the size of the valid data in the buffer.
            if (Succeeded(hr))
            {
                hr = pBuffer.SetCurrentLength(cbBuffer);
            }

            // Create a new sample and add the buffer to it.
            if (Succeeded(hr))
            {
                hr = MFDll.MFCreateSample(out pSample);
            }

            if (Succeeded(hr))
            {
                hr = pSample.AddBuffer(pBuffer);
            }

            // Set the time stamps, duration, and sample flags.
            if (Succeeded(hr))
            {
                hr = pSample.SetSampleTime(m_rtCurrentPosition);
            }

            if (Succeeded(hr))
            {
                duration = Utils.AudioDurationFromBufferSize(m_Riff.Format(), cbBuffer);
                hr = pSample.SetSampleDuration(duration);
            }

            if (Succeeded(hr))
            {
                // Set the discontinuity flag.
                if (m_discontinuity)
                {
                    hr = pSample.SetUINT32(MFAttributesClsid.MFSampleExtension_Discontinuity, 1);
                }
            }

            if (Succeeded(hr))
            {
                // Update our current position.
                m_rtCurrentPosition += duration;

                // Give the pointer to the caller.
                ppSample = pSample;
            }


            //SAFE_RELEASE(pBuffer);
            //SAFE_RELEASE(pSample);

            return hr;

        }

        #endregion


        #region IDisposable Members

        public void Dispose()
        {
            if (m_Riff != null)
            {
                m_Riff.Dispose();
                m_Riff = null;
            }

            if (m_pEventQueue != null)
            {
                Marshal.ReleaseComObject(m_pEventQueue);
                m_pEventQueue = null;
            }

            if (m_pSource != null)
            {
                //m_pSource.Dispose(); Children don't dispose their parents...
                m_pSource = null;
            }

            if (m_pStreamDescriptor != null)
            {
                Marshal.ReleaseComObject(m_pStreamDescriptor);
                m_pStreamDescriptor = null;
            }
            if (m_Log != null)
            {
                m_Log.Dispose();
                m_Log = null;
            }
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    [ClassInterface(ClassInterfaceType.None)]
    public class WavSource : COMBase, IMFMediaSource, IDisposable
    {
        #region Defines

        const int WAVE_FORMAT_PCM = 1;
        const long ONE_SECOND = 10000000;

        public enum State
        {
            Stopped,
            Paused,
            Started
        }

        #endregion

        #region Member Variables

        xLog m_Log;

        IMFMediaEventQueue m_pEventQueue;             // Event generator helper
        IMFPresentationDescriptor m_pPresentationDescriptor; // Default presentation

        WavStream m_pStream;                 // Media stream. Can be NULL is no stream is selected.

        bool m_IsShutdown;               // Flag to indicate if Shutdown() method was called.
        State m_state;                    // Current state (running, stopped, paused)

        CWavRiffParser m_pRiff;

        #endregion

        public WavSource()
        {
            m_Log = new xLog("WavSource");
            m_state = State.Stopped;

            // Create the media event queue.
            int hr = MFDll.MFCreateEventQueue(out m_pEventQueue);
        }

        ~WavSource()
        {
            m_Log.Dispose();
            m_pStream = null;
        }

        #region IMFMediaEventGenerator methods

        // All of the IMFMediaEventGenerator methods do the following:
        // 1. Check for shutdown status.
        // 2. Call the event generator helper object.

        public int BeginGetEvent(IMFAsyncCallback pCallback, object punkState)
        {
            int hr = S_Ok;
            m_Log.WriteLine("-BeginGetEvent");

            lock (this)
            {
                hr = CheckShutdown();

                if (Succeeded(hr))
                {
                    hr = m_pEventQueue.BeginGetEvent(pCallback, punkState);
                }
            }

            return hr;
        }

        public int EndGetEvent(IMFAsyncResult pResult, out IMFMediaEvent ppEvent)
        {
            int hr = S_Ok;
            ppEvent = null;
            m_Log.WriteLine("-EndGetEvent");

            lock (this)
            {
                hr = CheckShutdown();

                if (Succeeded(hr))
                {
                    hr = m_pEventQueue.EndGetEvent(pResult, out ppEvent);
                }
            }

            return hr;
        }

        public int GetEvent(MFEventFlag dwFlags, out IMFMediaEvent ppEvent)
        {
            // NOTE: GetEvent can block indefinitely, so we don't hold the
            //       WavSource lock. This requires some juggling with the
            //       event queue pointer.

            int hr = S_Ok;
            ppEvent = null;
            m_Log.WriteLine("-GetEvent");

            IMFMediaEventQueue pQueue = null;

            { // scope for lock

                lock (this)
                {
                    // Check shutdown
                    hr = CheckShutdown();

                    if (Succeeded(hr))
                    {
                        pQueue = m_pEventQueue;
                    }
                }

            }   // release lock

            if (Succeeded(hr))
            {
                hr = pQueue.GetEvent(dwFlags, out ppEvent);
            }

            // not needed SAFE_RELEASE(pQueue);

            return hr;
        }

        public int QueueEvent(MediaEventType met, Guid guidExtendedType, int hrStatus, object pvValue)
        {
            int hr = S_Ok;
            m_Log.WriteLine("-QueueEvent");

            lock (this)
            {
                hr = CheckShutdown();

                if (Succeeded(hr))
                {
                    int iObjectSize;

                    if (IntPtr.Size == 4)
                    {
                        iObjectSize = 16;
                    }
                    else
                    {
                        iObjectSize = 24;
                    }
                    IntPtr ip = Marshal.AllocCoTaskMem(iObjectSize);
                    try
                    {
                        Marshal.GetNativeVariantForObject(pvValue, ip);
                        hr = m_pEventQueue.QueueEventParamVar(met, guidExtendedType, hrStatus, ip);
                    }
                    finally
                    {
                        Marshal.FreeCoTaskMem(ip);
                    }
                }
            }

            return hr;
        }

        #endregion

        #region IMFMediaSource methods

        //-------------------------------------------------------------------
        // Name: CreatePresentationDescriptor
        // Description: Returns a copy of the default presentation descriptor.
        //-------------------------------------------------------------------

        public int CreatePresentationDescriptor(out IMFPresentationDescriptor ppPresentationDescriptor)
        {
            int hr;
            ppPresentationDescriptor = null;
            m_Log.WriteLine("-CreatePresentationDescriptor");

            lock (this)
            {
                hr = CheckShutdown();

                if (Succeeded(hr))
                {
                    if (m_pPresentationDescriptor == null)
                    {
                        hr = CreatePresentationDescriptor();
                    }
                }

                if (Succeeded(hr))
                {
                    // Clone our default presentation descriptor.
                    hr = m_pPresentationDescriptor.Clone(out ppPresentationDescriptor);
                }
            }

            return hr;
        }

        //-------------------------------------------------------------------
        // Name: GetCharacteristics
        // Description: Returns flags the describe the source.
        //-------------------------------------------------------------------

        public int GetCharacteristics(out MFMediaSourceCharacteristics pdwCharacteristics)
        {
            int hr;
            pdwCharacteristics = MFMediaSourceCharacteristics.None;
            m_Log.WriteLine("-GetCharacteristics");

            lock (this)
            {
                hr = CheckShutdown();

                if (Succeeded(hr))
                {
                    pdwCharacteristics = MFMediaSourceCharacteristics.CanPause | MFMediaSourceCharacteristics.CanSeek;
                }
            }

            return hr;

        }

        //-------------------------------------------------------------------
        // Name: Start
        // Description: Switches to running state.
        //-------------------------------------------------------------------

        public int Start(
                   IMFPresentationDescriptor pPresentationDescriptor,
                   Guid pguidTimeFormat,
                   object pvarStartPosition
                   )
        {
            int hr;
            m_Log.WriteLine("-Start");

            lock (this)
            {
                object var = null;
                long llStartOffset = 0;
                bool bIsSeek = false;
                bool bIsRestartFromCurrentPosition = false;

                //PropVariantInit(var);

                // Fail if the source is shut down.
                hr = CheckShutdown();

                if (Succeeded(hr))
                {
                    // Check parameters.
                    // Start position and presentation descriptor cannot be null.
                    if (pPresentationDescriptor == null) // pvarStartPosition == null || 
                    {
                        hr = E_InvalidArgument;
                    }
                }

                if (Succeeded(hr))
                {
                    // Check the time format. Must be "reference time" units.
                    if ((pguidTimeFormat != null) && (pguidTimeFormat != Guid.Empty))
                    {
                        // Unrecognized time format GUID.
                        hr = MFError.MF_E_UNSUPPORTED_TIME_FORMAT;
                    }
                }

                if (Succeeded(hr))
                {
                    // Check the start position.
                    if (pvarStartPosition == null)
                    {
                        // Start position is "current position".
                        // For stopped, that means 0. Otherwise, use the current position.
                        if (m_state == State.Stopped)
                        {
                            llStartOffset = 0;
                        }
                        else
                        {
                            llStartOffset = GetCurrentPosition();
                            bIsRestartFromCurrentPosition = true;
                        }
                    }
                    else if (pvarStartPosition.GetType() == typeof(long))
                    {
                        // Start position is given in pvarStartPosition in 100-ns units.
                        llStartOffset = (long)pvarStartPosition;

                        if (m_state != State.Stopped)
                        {
                            // Source is running or paused, so this is a seek.
                            bIsSeek = true;
                        }

                    }
                    else
                    {
                        // We don't support this time format.
                        hr = MFError.MF_E_UNSUPPORTED_TIME_FORMAT;
                    }
                }


                if (Succeeded(hr))
                {
                    Debug.Assert(pPresentationDescriptor != null);  // Checked this earlier.

                    // Validate the caller's presentation descriptor.
                    hr = ValidatePresentationDescriptor(pPresentationDescriptor);
                }

                if (Succeeded(hr))
                {
                    // Sends the MENewStream or MEUpdatedStream event.
                    hr = QueueNewStreamEvent(pPresentationDescriptor);
                }

                if (Succeeded(hr))
                {
                    // Notify the stream of the new start time.
                    //hr = m_pStream.SetPosition(llStartOffset);
                }

                if (Succeeded(hr))
                {
                    // Send Started or Seeked events. We will send them
                    // 1. from the media source
                    // 2. from each stream

                    //var.vt = VT_I8;
                    //var.hVal.QuadPart = llStartOffset;
                    var = llStartOffset;

                    // (1) Send the source event.
                    if (bIsSeek)
                    {
                        hr = QueueEvent(MediaEventType.MESourceSeeked, Guid.Empty, hr, var);
                    }
                    else
                    {
                        // For starting, if we are RESTARTING from the current position and our
                        // previous state was running/paused, then we need to add the
                        // MF_EVENT_SOURCE_ACTUAL_START attribute to the event. This requires
                        // creating the event object first.

                        IMFMediaEvent pEvent = null;

                        // Create the event.
                        hr = MFDll.MFCreateMediaEvent(
                            MediaEventType.MESourceStarted,
                            Guid.Empty,
                            hr,
                            var,
                            out pEvent
                            );

                        // For restarts, set the actual start time as an attribute.
                        if (Succeeded(hr))
                        {
                            if (bIsRestartFromCurrentPosition)
                            {
                                hr = pEvent.SetUINT64(MFAttributesClsid.MF_EVENT_SOURCE_ACTUAL_START, llStartOffset);
                            }
                        }

                        // Now  queue the event.
                        if (Succeeded(hr))
                        {
                            hr = m_pEventQueue.QueueEvent(pEvent);
                        }

                        //SAFE_RELEASE(pEvent);
                    }
                }

                if (Succeeded(hr))
                {
                    // 2. Send the stream event.
                    if (m_pStream != null)
                    {
                        if (bIsSeek)
                        {
                            hr = m_pStream.QueueEvent(MediaEventType.MEStreamSeeked, Guid.Empty, hr, var);
                        }
                        else
                        {
                            hr = m_pStream.QueueEvent(MediaEventType.MEStreamStarted, Guid.Empty, hr, var);
                        }
                    }
                }

                if (Succeeded(hr))
                {
                    // Update our state.
                    m_state = State.Started;
                }


                // NOTE: If this method were implemented as an asynchronous operation
                // and the operation Failed asynchronously, the media source would need
                // to send an MESourceStarted event with the failure code. For this
                // sample, all operations are synchronous (which is allowed), so any
                // failures are also synchronous.


                //PropVariantClear(var);
            }

            return hr;
        }

        //-------------------------------------------------------------------
        // Name: Pause
        // Description: Switches to paused state.
        //-------------------------------------------------------------------

        public int Pause()
        {
            int hr;
            m_Log.WriteLine("-Pause");

            lock (this)
            {

                hr = CheckShutdown();

                // Pause is only allowed from started state.
                if (Succeeded(hr))
                {
                    if (m_state != State.Started)
                    {
                        hr = MFError.MF_E_INVALID_STATE_TRANSITION;
                    }
                }

                if (Succeeded(hr))
                {
                    // Send the appropriate events.
                    if (m_pStream != null)
                    {
                        object o = null;

                        hr = m_pStream.QueueEvent(MediaEventType.MEStreamPaused, Guid.Empty, S_Ok, o);
                    }
                }

                if (Succeeded(hr))
                {
                    object o = null;
                    hr = QueueEvent(MediaEventType.MESourcePaused, Guid.Empty, S_Ok, o);
                }

                if (Succeeded(hr))
                {
                    // Update our state.
                    m_state = State.Paused;
                }
            }

            // Nothing else for us to do.

            return hr;
        }

        //-------------------------------------------------------------------
        // Name: Stop
        // Description: Switches to stopped state.
        //-------------------------------------------------------------------

        public int Stop()
        {
            int hr;
            m_Log.WriteLine("-Stop");

            lock (this)
            {
                hr = CheckShutdown();

                if (Succeeded(hr))
                {
                    // Queue events.
                    if (m_pStream != null)
                    {
                        object o = null;
                        hr = m_pStream.QueueEvent(MediaEventType.MEStreamStopped, Guid.Empty, S_Ok, o);
                    }
                }

                if (Succeeded(hr))
                {
                    object o = null;
                    hr = QueueEvent(MediaEventType.MESourceStopped, Guid.Empty, S_Ok, o);
                }

                if (Succeeded(hr))
                {
                    // Update our state.
                    m_state = State.Stopped;
                }
            }

            return hr;
        }

        //-------------------------------------------------------------------
        // Name: Shutdown
        // Description: Releases resources.
        //
        // The source and stream objects hold reference counts on each other.
        // To avoid memory leaks caused by circular ref. counts, the Shutdown
        // method releases the pointer to the stream.
        //-------------------------------------------------------------------

        public int Shutdown()
        {
            int hr;
            m_Log.WriteLine("-Shutdown");

            lock (this)
            {

                hr = CheckShutdown();

                if (Succeeded(hr))
                {
                    // Shut down the stream object.
                    if (m_pStream != null)
                    {
                        hr = m_pStream.Shutdown();
                    }

                    // Shut down the event queue.
                    if (m_pEventQueue != null)
                    {
                        m_pEventQueue.Shutdown();
                    }

                    // Release objects. (Even if Shutdown Failed for some reason.)
                    Dispose();

                    // Set our shutdown flag.
                    m_IsShutdown = true;
                }
            }

            return hr;
        }

        #endregion

        #region Other public methods

        //-------------------------------------------------------------------
        // Name: Open
        // Description: Opens the source from a bytestream.
        //
        // The bytestream handler calls this method after it creates the
        // source.
        //
        // Note: This method is not a public API. It is a custom method on
        // for our bytestream class to use.
        //-------------------------------------------------------------------

        internal int Open(IMFByteStream pStream)
        {
            int hr;
            m_Log.WriteLine("Open");

            lock (this)
            {

                if (m_pRiff != null)
                {
                    // The media source has already been opened.
                    return MFError.MF_E_INVALIDREQUEST;
                }

                hr = S_Ok;

                // Create a new WAVE RIFF parser object to parse the stream.
                hr = CWavRiffParser.Create(pStream, out m_pRiff);

                // Parse the WAVE header. This fails if the header is not
                // well-formed.
                if (Succeeded(hr))
                {
                    hr = m_pRiff.ParseWAVEHeader();
                }

                // Validate the WAVEFORMATEX structure from the file header.
                if (Succeeded(hr))
                {
                    hr = ValidateWaveFormat(m_pRiff.Format(), m_pRiff.FormatSize());
                }

                if (Failed(hr))
                {
                    Shutdown();
                }
            }

            return hr;
        }

        internal State GetState()
        {
            m_Log.WriteLine("GetState");

            return m_state;
        }

        #endregion

        // NOTE: These private methods do not hold the source's critical
        // section. The caller must ensure the critical section is held.
        // Also, these methods do not check for shut-down.

        #region Private methods

        /// <summary>
        /// WaveFormat
        /// </summary>
        /// <remarks>
        /// Returns a pointer to the WAVEFORMATEX structure that describes the
        /// audio format. Returns NULL if no format is set.
        /// </remarks>
        /// <returns></returns>
        private WaveFormatEx WaveFormat()
        {
            m_Log.WriteLine("WaveFormat");

            if (m_pRiff != null)
            {
                return m_pRiff.Format();
            }
            else
            {
                return null;
            }
        }

        private int WaveFormatSize()
        {
            m_Log.WriteLine("WaveFormatSize");

            if (m_pRiff != null)
            {
                return m_pRiff.FormatSize();
            }
            else
            {
                return 0;
            }
        }

        private int CreatePresentationDescriptor()
        {
            int hr = S_Ok;

            m_Log.WriteLine("CreatePresentationDescriptor");

            IMFMediaType pMediaType = null;
            IMFStreamDescriptor pStreamDescriptor = null;
            IMFMediaTypeHandler pHandler = null;

            Debug.Assert(WaveFormat() != null);

            // Create an empty media type.
            hr = MFDll.MFCreateMediaType(out pMediaType);

            // Initialize the media type from the WAVEFORMATEX structure.
            if (Succeeded(hr))
            {
                hr = MFDll.MFInitMediaTypeFromWaveFormatEx(pMediaType, WaveFormat(), WaveFormatSize());
            }

            IMFMediaType[] mt = new IMFMediaType[1];
            mt[0] = pMediaType;

            // Create the stream descriptor.
            if (Succeeded(hr))
            {
                hr = MFDll.MFCreateStreamDescriptor(
                    0,          // stream identifier
                    1,          // Number of media types.
                    mt, // Array of media types
                    out pStreamDescriptor
                    );
            }

            // Set the default media type on the media type handler.
            if (Succeeded(hr))
            {
                hr = pStreamDescriptor.GetMediaTypeHandler(out pHandler);
            }

            if (Succeeded(hr))
            {
                hr = pHandler.SetCurrentMediaType(pMediaType);
            }

            IMFStreamDescriptor[] ms = new IMFStreamDescriptor[1];
            ms[0] = pStreamDescriptor;

            // Create the presentation descriptor.
            if (Succeeded(hr))
            {
                hr = MFDll.MFCreatePresentationDescriptor(
                    1,      // Number of stream descriptors
                    ms, // Array of stream descriptors
                    out m_pPresentationDescriptor
                    );
            }

            // Select the first stream
            if (Succeeded(hr))
            {
                hr = m_pPresentationDescriptor.SelectStream(0);
            }

            // Set the file duration as an attribute on the presentation descriptor.
            if (Succeeded(hr))
            {
                long duration = m_pRiff.FileDuration();
                hr = m_pPresentationDescriptor.SetUINT64(MFAttributesClsid.MF_PD_DURATION, (long)duration);
            }


            //SAFE_RELEASE(pMediaType);
            //SAFE_RELEASE(pStreamDescriptor);
            //SAFE_RELEASE(pHandler);

            return hr;

        }

        //-------------------------------------------------------------------
        // Name: ValidatePresentationDescriptor
        // Description: Validates the caller's presentation descriptor.
        //
        // This method is called when Start() is called with a non-NULL
        // presentation descriptor. The caller is supposed to give us back
        // the same PD that we gave out in CreatePresentationDescriptor().
        // This method performs a sanity check on the caller's PD to make
        // sure it matches ours.
        //
        // Note: Because this media source has one stream with single, fixed
        //       media type, there is not much for the caller to decide. In
        //       a more complicated source, the caller might select different
        //       streams, or select from a list of media types.
        //-------------------------------------------------------------------

        private int ValidatePresentationDescriptor(IMFPresentationDescriptor pPD)
        {
            int hr;

            m_Log.WriteLine("ValidatePresentationDescriptor");

            Debug.Assert(pPD != null);

            IMFStreamDescriptor pStreamDescriptor = null;
            IMFMediaTypeHandler pHandler = null;
            IMFMediaType pMediaType = null;
            IMFAudioMediaType pAudioType = null;
            WaveFormatEx pFormat = null;

            int cStreamDescriptors = 0;
            bool fSelected = false;

            // Make sure there is only one stream.
            hr = pPD.GetStreamDescriptorCount(out cStreamDescriptors);

            if (Succeeded(hr))
            {
                if (cStreamDescriptors != 1)
                {
                    hr = MFError.MF_E_UNSUPPORTED_REPRESENTATION;
                }
            }

            // Get the stream descriptor.
            if (Succeeded(hr))
            {
                hr = pPD.GetStreamDescriptorByIndex(0, out fSelected, out pStreamDescriptor);
            }

            // Make sure it's selected. (This media source has only one stream, so it
            // is not useful to deselect the only stream.)
            if (Succeeded(hr))
            {
                if (!fSelected)
                {
                    hr = MFError.MF_E_UNSUPPORTED_REPRESENTATION;
                }
            }


            // Get the media type handler, so that we can get the media type.
            if (Succeeded(hr))
            {
                hr = pStreamDescriptor.GetMediaTypeHandler(out pHandler);
            }
            if (Succeeded(hr))
            {
                try
                {
                    hr = pHandler.GetCurrentMediaType(out pMediaType);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message); // todo - remove this
                }
            }


            if (Succeeded(hr))
            {
                pAudioType = (IMFAudioMediaType)pMediaType;
            }

            if (Succeeded(hr))
            {
                int iSize;

                // Deprecated method, but it works
                //IntPtr ip = pAudioType.GetAudioFormat();
                //pFormat = new WaveFormatEx();
                //Marshal.PtrToStructure(ip, pFormat);

                hr = MFDll.MFCreateWaveFormatExFromMFMediaType(
                    pMediaType, 
                    out pFormat, 
                    out iSize, 
                    MFWaveFormatExConvertFlags.Normal);

                if (Failed(hr) || (pFormat == null) || (this.WaveFormat() == null))
                {
                    hr = MFError.MF_E_INVALIDMEDIATYPE;
                }
            }

            if (Succeeded(hr))
            {
                Debug.Assert(this.WaveFormat() != null);

                if (!pFormat.IsEqual(WaveFormat()))
                {
                    hr = MFError.MF_E_INVALIDMEDIATYPE;
                }
            }


            //SAFE_RELEASE(pStreamDescriptor);
            //SAFE_RELEASE(pHandler);
            //SAFE_RELEASE(pMediaType);
            //SAFE_RELEASE(pAudioType);

            return hr;

        }

        //-------------------------------------------------------------------
        // Name: QueueNewStreamEvent
        // Description:
        // Queues an MENewStream or MEUpdatedStream event during Start.
        //
        // pPD: The presentation descriptor.
        //
        // Precondition: The presentation descriptor is assumed to be valid.
        // Call ValidatePresentationDescriptor before calling this method.
        //-------------------------------------------------------------------

        private int QueueNewStreamEvent(IMFPresentationDescriptor pPD)
        {
            Debug.Assert(pPD != null);

            m_Log.WriteLine("QueueNewStreamEvent");

            int hr = S_Ok;
            IMFStreamDescriptor pSD = null;

            bool fSelected = false;
            hr = pPD.GetStreamDescriptorByIndex(0, out fSelected, out pSD);

            if (Succeeded(hr))
            {
                // The stream must be selected, because we don't allow the app
                // to de-select the stream. See ValidatePresentationDescriptor.
                Debug.Assert(fSelected);

                if (m_pStream != null)
                {
                    // The stream already exists, and is still selected.
                    // Send the MEUpdatedStream event.

                    hr = Utils.QueueEventWithIUnknown(this, MediaEventType.MEUpdatedStream, S_Ok, m_pStream);
                }
                else
                {
                    // The stream does not exist, and is now selected.
                    // Create a new stream.

                    hr = CreateWavStream(pSD);

                    // CreateWavStream creates the stream, so m_pStream is no longer null.
                    Debug.Assert(m_pStream != null);

                    if (Succeeded(hr))
                    {
                        // Send the MENewStream event.
                        hr = Utils.QueueEventWithIUnknown(this, MediaEventType.MENewStream, S_Ok, m_pStream);
                    }
                }
            }


            //SAFE_RELEASE(pSD);

            return hr;
        }

        private int CreateWavStream(IMFStreamDescriptor pSD)
        {
            m_Log.WriteLine("CreateWavStream");

            int hr = S_Ok;
            m_pStream = new WavStream(this, m_pRiff, pSD, hr);

            if (m_pStream == null)
            {
                hr = E_OutOfMemory;
            }

            if (Succeeded(hr))
            {
                //m_pStream.AddRef();
            }
            return hr;
        }

        private long GetCurrentPosition()
        {
            m_Log.WriteLine("GetCurrentPosition");

            if (m_pStream != null)
            {
                return m_pStream.GetCurrentPosition();
            }
            else
            {
                // If no stream is selected, we are at time 0 by definition.
                return 0;
            }
        }

        /// <summary>
        /// ValidateWaveFormat - Validates a WAVEFORMATEX structure.
        /// </summary>
        /// <remarks>
        /// This method is called when the byte stream handler opens the
        /// source. The WAVEFORMATEX structure is copied directly from the
        /// .wav file. Therefore the source should not trust any of the
        /// values in the format header.
        ///
        /// Just to keep the sample as simple as possible, we only accept
        /// uncompressed PCM formats in this media source.
        /// </remarks>
        /// <param name="pWav"></param>
        /// <param name="cbSize"></param>
        /// <returns></returns>
        private int ValidateWaveFormat(WaveFormatEx pWav, int cbSize)
        {
            m_Log.WriteLine("ValidateWaveFormat");

            if (pWav.wFormatTag != WAVE_FORMAT_PCM)
            {
                return MFError.MF_E_INVALIDMEDIATYPE;
            }

            if (pWav.nChannels != 1 && pWav.nChannels != 2)
            {
                return MFError.MF_E_INVALIDMEDIATYPE;
            }

            if (pWav.wBitsPerSample != 8 && pWav.wBitsPerSample != 16)
            {
                return MFError.MF_E_INVALIDMEDIATYPE;
            }

            if (pWav.cbSize != 0)
            {
                return MFError.MF_E_INVALIDMEDIATYPE;
            }

            // Make sure block alignment was calculated correctly.
            if (pWav.nBlockAlign != pWav.nChannels * (pWav.wBitsPerSample / 8))
            {
                return MFError.MF_E_INVALIDMEDIATYPE;
            }

            // Check possible overflow...
            if (pWav.nSamplesPerSec > (int)(int.MaxValue / pWav.nBlockAlign))        // Is (nSamplesPerSec * nBlockAlign > MAXDWORD) ?
            {
                return MFError.MF_E_INVALIDMEDIATYPE;
            }

            // Make sure average bytes per second was calculated correctly.
            if (pWav.nAvgBytesPerSec != pWav.nSamplesPerSec * pWav.nBlockAlign)
            {
                return MFError.MF_E_INVALIDMEDIATYPE;
            }

            // Everything checked out.
            return S_Ok;
        }

        private long BufferSizeFromAudioDuration(WaveFormatEx pWav, long duration)
        {

            long cbSize = duration * pWav.nAvgBytesPerSec / ONE_SECOND;

            int ulRemainder = (int)(cbSize % pWav.nBlockAlign);

            // Round up to the next block.
            if (ulRemainder > 0)
            {
                cbSize += pWav.nBlockAlign - ulRemainder;
            }

            return cbSize;
        }

        private int CheckShutdown()
        {
            m_Log.WriteLine("CheckShutdown");

            int hr;

            if (m_IsShutdown)
            {
                hr = MFError.MF_E_SHUTDOWN;
            }
            else
            {
                // If the calling thread is STA, we aren't going to work right.  In
                // theory I could change this object to re-make every call on an MTA
                // thread.  Instead, I'm just going to insist on being called on MTA
                // thread in the first place.
                if (Thread.CurrentThread.GetApartmentState() == ApartmentState.MTA)
                {
                    hr = S_Ok;
                }
                else
                {
                    hr = unchecked((int)0x80040156); //REGDB_E_BADTHREADINGMODEL;
                }
            }

            return hr;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (m_pRiff != null)
            {
                m_pRiff.Dispose();
                m_pRiff = null;
            }

            if (m_pStream != null)
            {
                m_pStream.Dispose();
                m_pStream = null;
            }

            if (m_pEventQueue != null)
            {
                Marshal.ReleaseComObject(m_pEventQueue);
                m_pEventQueue = null;
            }

            if (m_pPresentationDescriptor != null)
            {
                Marshal.ReleaseComObject(m_pPresentationDescriptor);
                m_pPresentationDescriptor = null;
            }

            if (m_Log != null)
            {
                m_Log.Dispose();
                m_Log = null;
            }
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    public class CWavRiffParser : CRiffParser
    {
        #region Member variables

        WaveFormatEx m_pWaveFormat;
        int m_cbWaveFormat;

        long m_rtDuration;               // File duration.

        public WaveFormatEx Format() { return m_pWaveFormat; }
        public int FormatSize() { return m_cbWaveFormat; }
        public long FileDuration() { return m_rtDuration; }

        #endregion

        // CWavRiffParser is a specialization of the generic RIFF parser object,
        // and is designed to parse .wav files.

        CWavRiffParser(IMFByteStream pStream, out int hr)
            : base(pStream, new FourCC("RIFF"), 0, out hr)
        {
        }

        ~CWavRiffParser()
        {
        }

        //-------------------------------------------------------------------
        // Name: Create
        // Description: Static creation function.
        //-------------------------------------------------------------------

        public static int Create(IMFByteStream pStream, out CWavRiffParser ppParser)
        {
            int hr;

            // Create a riff parser for the 'RIFF' container
            ppParser = new CWavRiffParser(pStream, out hr);

            // Check the RIFF file type.
            if (Succeeded(hr))
            {
                if (ppParser.RiffType() != new FourCC("WAVE"))
                {
                    hr = MFError.MF_E_INVALID_FILE_FORMAT;
                }
            }

            return hr;
        }


        //-------------------------------------------------------------------
        // Name: ParseWAVEHeader
        // Description: Parsers the RIFF WAVE header.
        //
        // Note:
        // .wav files should look like this:
        //
        // RIFF ('WAVE'
        //       'fmt ' = WAVEFORMATEX structure
        //       'data' = audio data
        //       )
        //-------------------------------------------------------------------

        public int ParseWAVEHeader()
        {
            int hr = S_Ok;
            bool bFoundData = false;
            FourCC fmt = new FourCC("fmt ");
            FourCC data = new FourCC("data");

            // Iterate through the RIFF chunks. Ignore chunks we don't recognize.
            while (Succeeded(hr))
            {
                if (Chunk().FourCC() == fmt)
                {
                    // Read the WAVEFORMATEX structure allegedly contained in this chunk.
                    // This method does NOT validate the contents of the structure.
                    hr = ReadFormatBlock();
                }
                else if (Chunk().FourCC() == data)
                {
                    // Found the start of the audio data. The format chunk should precede the
                    // data chunk. If we did not find the formt chunk yet, that is a failure
                    // case (see below)
                    bFoundData = true;
                    break;
                }

                hr = MoveToNextChunk();
            }

            if (Succeeded(hr))
            {
                // To be valid, the file must have a format chunk and a data chunk.
                // Fail if either of these conditions is not met.
                if (m_pWaveFormat == null || !bFoundData)
                {
                    hr = MFError.MF_E_INVALID_FILE_FORMAT;
                }
            }

            if (Succeeded(hr))
            {
                m_rtDuration = Utils.AudioDurationFromBufferSize(m_pWaveFormat, Chunk().DataSize());
            }

            return hr;

        }

        //-------------------------------------------------------------------
        // Name: ReadFormatBlock
        // Description: Reads the WAVEFORMATEX structure from the file header.
        //-------------------------------------------------------------------

        int ReadFormatBlock()
        {
            Debug.Assert(Chunk().FourCC() == new FourCC("fmt "));
            Debug.Assert(m_pWaveFormat == null);

            int iWaveFormatExSize = Marshal.SizeOf(typeof(WaveFormatEx));

            int hr = S_Ok;

            // Some .wav files do not include the cbSize field of the WAVEFORMATEX
            // structure. For uncompressed PCM audio, field is always zero.
            int cbMinFormatSize = iWaveFormatExSize - Marshal.SizeOf(typeof(short));

            int cbFormatSize = 0;		// Size of the actual format block in the file.

            // Validate the size
            if (Chunk().DataSize() < cbMinFormatSize)
            {
                hr = MFError.MF_E_INVALID_FILE_FORMAT;
            }

            // Allocate a buffer for the WAVEFORMAT structure.
            if (Succeeded(hr))
            {
                cbFormatSize = Chunk().DataSize();

                // We store a WAVEFORMATEX structure, so our format block must be at
                // least sizeof(WAVEFORMATEX) even if the format block in the file
                // is smaller. See note above about cbMinFormatSize.
                m_cbWaveFormat = Math.Max(cbFormatSize, iWaveFormatExSize);
            }

            if (Succeeded(hr))
            {
                IntPtr ip = Marshal.AllocCoTaskMem(m_cbWaveFormat);

                try
                {
                    // Zero our structure, in case cbFormatSize < m_cbWaveFormat.
                    for (int x = 0; x < m_cbWaveFormat; x++)
                    {
                        Marshal.WriteByte(ip, x, 0);
                    }

                    // Now read cbFormatSize bytes from the file.
                    hr = ReadDataFromChunk(ip, cbFormatSize);
                    if (Succeeded(hr))
                    {
                        m_pWaveFormat = new WaveFormatEx();

                        Marshal.PtrToStructure(ip, m_pWaveFormat);
                        Debug.Assert(m_pWaveFormat.cbSize == 0);
                    }
                }
                finally
                {
                    Marshal.FreeCoTaskMem(ip);
                }
            }

            if (Failed(hr))
            {
                m_pWaveFormat = null;
                m_cbWaveFormat = 0;
            }

            return hr;
        }
    }

}
