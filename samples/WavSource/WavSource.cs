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

        public WavStream(WavSource pSource, CWavRiffParser pRiff, IMFStreamDescriptor pSD)
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
            MFExternAlt.MFCreateEventQueue(out m_pEventQueue);
        }

#if DEBUG

        ~WavStream()
        {
            Debug.Assert(m_IsShutdown);
        }

#endif

        #region IMFMediaEventGenerator methods

        public void BeginGetEvent(
            //IMFAsyncCallback pCallback,
            IntPtr pCallback,
            object punkState
            )
        {
            m_Log.WriteLine("-BeginGetEvent");

            lock (this)
            {
                CheckShutdown();
                m_pEventQueue.BeginGetEvent(pCallback, punkState);
            }
        }

        public void EndGetEvent(
            //IMFAsyncResult pResult, 
            IntPtr pResult,
            out IMFMediaEvent ppEvent
            )
        {
            m_Log.WriteLine("-EndGetEvent");
            ppEvent = null;

            lock (this)
            {
                CheckShutdown();
                m_pEventQueue.EndGetEvent(pResult, out ppEvent);
            }
        }

        public void GetEvent(MFEventFlag dwFlags, out IMFMediaEvent ppEvent)
        {
            m_Log.WriteLine("-GetEvent");
            ppEvent = null;

            IMFMediaEventQueue pQueue = null;

            lock (this)
            {
                CheckShutdown();
                pQueue = (IMFMediaEventQueue)m_pEventQueue;
            }

            pQueue.GetEvent(dwFlags, out ppEvent);

            //not needed SAFE_RELEASE(pQueue);
        }

        public void QueueEvent(MediaEventType met, Guid guidExtendedType, int hrStatus, ConstPropVariant pvValue)
        {
            m_Log.WriteLine("-QueueEvent");

            lock (this)
            {
                CheckShutdown();

                m_pEventQueue.QueueEventParamVar(met, guidExtendedType, hrStatus, pvValue);
            }
        }

        #endregion

        #region IMFMediaStream methods.

        public void GetMediaSource(out IMFMediaSource ppMediaSource)
        {
            m_Log.WriteLine("-GetMediaSource");
            ppMediaSource = null;

            lock (this)
            {
                if (m_pSource == null)
                {
                    throw new COMException("null WavSource", E_Unexpected);
                }

                CheckShutdown();
                ppMediaSource = (IMFMediaSource)m_pSource;
            }
        }

        public void GetStreamDescriptor(out IMFStreamDescriptor ppStreamDescriptor)
        {
            m_Log.WriteLine("-GetStreamDescriptor");
            ppStreamDescriptor = null;

            lock (this)
            {
                if (m_pStreamDescriptor == null)
                {
                    throw new COMException("null stream descriptor", E_Unexpected);
                }

                CheckShutdown();
                ppStreamDescriptor = m_pStreamDescriptor;
            }
        }

        public void RequestSample(object pToken)
        {
            m_Log.WriteLine("-RequestSample");

            if (m_pSource == null)
            {
                throw new COMException("null wavsource", E_Unexpected);
            }

            IMFSample pSample = null;  // Sample to deliver.
            bool bReachedEOS = false;   // true if we hit end-of-stream during this method.

            lock (this)
            {
                // Check if we are shut down.
                CheckShutdown();

                // Check if we already reached the end of the stream.
                if (m_EOS)
                {
                    throw new COMException("at eos", MFError.MF_E_END_OF_STREAM);
                }

                // Check the source is stopped.
                // GetState does not hold the source's critical section. Safe to call.
                if (m_pSource.GetState() == WavSource.State.Stopped)
                {
                    throw new COMException("stopped", MFError.MF_E_INVALIDREQUEST);
                }

                // If we Succeeded to here, we are able to deliver a sample.

                // Create a new audio sample.
                CreateAudioSample(out pSample);

                // If the caller provided a token, attach it to the sample as
                // an attribute.

                // NOTE: If we processed sample requests asynchronously, we would
                // need to call AddRef on the token and put the token onto a FIFO
                // queue. See documenation for IMFMediaStream::RequestSample.
                if (pToken != null)
                {
                    object o = pToken;
                    pSample.SetUnknown(MFAttributesClsid.MFSampleExtension_Token, o);
                }

                // Send the MEMediaSample event with the new sample.
                QueueEvent(MediaEventType.MEMediaSample, Guid.Empty, 0, new PropVariant(pSample));

                // See if we reached the end of the stream.
                CheckEndOfStream();    // This method sends MEEndOfStream if needed.
                bReachedEOS = m_EOS;        // Cache this flag in a local variable.

                //probably not needed SAFE_RELEASE(pSample);
            }

            // We only have one stream, so the end of the stream is also the end of the
            // presentation. Therefore, when we reach the end of the stream, we need to
            // queue the end-of-presentation event from the source. Logically we would do
            // this inside the CheckEndOfStream method. However, we cannot hold the
            // source's critical section while holding the stream's critical section, at
            // risk of deadlock.

            if (bReachedEOS)
            {
                m_pSource.QueueEvent(MediaEventType.MEEndOfPresentation, Guid.Empty, S_Ok, null);
            }
        }

        #endregion

        #region Other Public methods

        internal long GetCurrentPosition()
        {
            m_Log.WriteLine("GetCurrentPosition");
            return m_rtCurrentPosition;
        }

        internal void SetPosition(long rtNewPosition)
        {
            m_Log.WriteLine("SetPosition");

            lock (this)
            {
                // Check if the requested position is beyond the end of the stream.
                long duration = Utils.AudioDurationFromBufferSize(m_Riff.Format(), m_Riff.Chunk().DataSize());

                if (rtNewPosition > duration)
                {
                    throw new COMException("past end of stream", E_InvalidArgument);
                }

                if (m_rtCurrentPosition != rtNewPosition)
                {
                    long offset = Utils.BufferSizeFromAudioDuration(m_Riff.Format(), rtNewPosition);

                    // The chunk size is a int. So if our calculations are correct, there is no
                    // way that the maximum valid seek position can be larger than a int.
                    Debug.Assert(offset <= int.MaxValue);

                    m_Riff.MoveToChunkOffset((int)offset);

                    m_rtCurrentPosition = rtNewPosition;
                    m_discontinuity = true;
                    m_EOS = false;
                }
            }
        }

        internal void Shutdown()
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
        }

        #endregion

        #region Private methods

        private void CheckEndOfStream()
        {
            m_Log.WriteLine("CheckEndOfStream");

            if (m_Riff.BytesRemainingInChunk() < m_Riff.Format().nBlockAlign)
            {
                // The remaining data is smaller than the audio block size. (In theory there shouldn't be
                // partial bits of data at the end, so we should reach an even zero bytes, but the file
                // might not be authored correctly.)
                m_EOS = true;

                // Send the end-of-stream event,
                QueueEvent(MediaEventType.MEEndOfStream, Guid.Empty, S_Ok, null);
            }
        }

        private void CheckShutdown()
        {
            m_Log.WriteLine("CheckShutdown");

            if (m_IsShutdown)
            {
                throw new COMException("Parser is shut down", MFError.MF_E_SHUTDOWN);
            }
        }

        private void CreateAudioSample(out IMFSample ppSample)
        {
            m_Log.WriteLine("CreateAudioSample");
            ppSample = null;

            IMFMediaBuffer pBuffer = null;
            IMFSample pSample = null;

            int cbBuffer = 0;
            IntPtr pData = IntPtr.Zero;
            long duration = 0;

            // Start with one second of data, rounded up to the nearest block.
            cbBuffer = Utils.AlignUp(m_Riff.Format().nAvgBytesPerSec, m_Riff.Format().nBlockAlign);

            // Don't request any more than what's left.
            cbBuffer = Math.Min(cbBuffer, m_Riff.BytesRemainingInChunk());

            // Create the buffer.
            MFExtern.MFCreateMemoryBuffer(cbBuffer, out pBuffer);

            int a, b;

            // Get a pointer to the buffer memory.
            pBuffer.Lock(out pData, out a, out b);

            try
            {
                // Fill the buffer
                m_Riff.ReadDataFromChunk(pData, cbBuffer);
            }
            finally
            {
                // Unlock the buffer.
                pBuffer.Unlock();
            }

            // Set the size of the valid data in the buffer.
            pBuffer.SetCurrentLength(cbBuffer);

            // Create a new sample and add the buffer to it.
            MFExtern.MFCreateSample(out pSample);

            pSample.AddBuffer(pBuffer);

            // Set the time stamps, duration, and sample flags.
            pSample.SetSampleTime(m_rtCurrentPosition);

            duration = Utils.AudioDurationFromBufferSize(m_Riff.Format(), cbBuffer);
            pSample.SetSampleDuration(duration);

            // Set the discontinuity flag.
            if (m_discontinuity)
            {
                pSample.SetUINT32(MFAttributesClsid.MFSampleExtension_Discontinuity, 1);
            }

            // Update our current position.
            m_rtCurrentPosition += duration;

            // Give the pointer to the caller.
            ppSample = pSample;

            //SAFE_RELEASE(pBuffer);
            //SAFE_RELEASE(pSample);
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
            MFExtern.MFCreateEventQueue(out m_pEventQueue);
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

        public void BeginGetEvent(IMFAsyncCallback pCallback, object punkState)
        {
            m_Log.WriteLine("-BeginGetEvent");

            lock (this)
            {
                CheckShutdown();
                m_pEventQueue.BeginGetEvent(pCallback, punkState);
            }
        }

        public void EndGetEvent(IMFAsyncResult pResult, out IMFMediaEvent ppEvent)
        {
            m_Log.WriteLine("-EndGetEvent");
            ppEvent = null;

            lock (this)
            {
                CheckShutdown();
                m_pEventQueue.EndGetEvent(pResult, out ppEvent);
            }
        }

        public void GetEvent(MFEventFlag dwFlags, out IMFMediaEvent ppEvent)
        {
            // NOTE: GetEvent can block indefinitely, so we don't hold the
            //       WavSource lock. This requires some juggling with the
            //       event queue pointer.

            m_Log.WriteLine("-GetEvent");
            ppEvent = null;

            IMFMediaEventQueue pQueue = null;

            lock (this)
            {
                // Check shutdown
                CheckShutdown();
                pQueue = m_pEventQueue;
            }

            pQueue.GetEvent(dwFlags, out ppEvent);

            // not needed SAFE_RELEASE(pQueue);
        }

        public void QueueEvent(MediaEventType met, Guid guidExtendedType, int hrStatus, ConstPropVariant pvValue)
        {
            m_Log.WriteLine("-QueueEvent");

            lock (this)
            {
                CheckShutdown();
                m_pEventQueue.QueueEventParamVar(met, guidExtendedType, hrStatus, pvValue);
            }
        }

        #endregion

        #region IMFMediaSource methods

        //-------------------------------------------------------------------
        // Name: CreatePresentationDescriptor
        // Description: Returns a copy of the default presentation descriptor.
        //-------------------------------------------------------------------

        public void CreatePresentationDescriptor(out IMFPresentationDescriptor ppPresentationDescriptor)
        {
            m_Log.WriteLine("-CreatePresentationDescriptor");
            ppPresentationDescriptor = null;

            lock (this)
            {
                CheckShutdown();
                if (m_pPresentationDescriptor == null)
                {
                    CreatePresentationDescriptor();
                }

                // Clone our default presentation descriptor.
                m_pPresentationDescriptor.Clone(out ppPresentationDescriptor);
            }
        }

        //-------------------------------------------------------------------
        // Name: GetCharacteristics
        // Description: Returns flags the describe the source.
        //-------------------------------------------------------------------

        public void GetCharacteristics(out MFMediaSourceCharacteristics pdwCharacteristics)
        {
            m_Log.WriteLine("-GetCharacteristics");
            pdwCharacteristics = MFMediaSourceCharacteristics.None;

            lock (this)
            {
                CheckShutdown();
                pdwCharacteristics = MFMediaSourceCharacteristics.CanPause | MFMediaSourceCharacteristics.CanSeek;
            }
        }

        //-------------------------------------------------------------------
        // Name: Start
        // Description: Switches to running state.
        //-------------------------------------------------------------------

        public void Start(
           IMFPresentationDescriptor pPresentationDescriptor,
           Guid pguidTimeFormat,
           ConstPropVariant pvarStartPosition
           )
        {
            m_Log.WriteLine("-Start");

            lock (this)
            {
                PropVariant var;
                long llStartOffset = 0;
                bool bIsSeek = false;
                bool bIsRestartFromCurrentPosition = false;

                // Fail if the source is shut down.
                CheckShutdown();

                // Check parameters.
                // Start position and presentation descriptor cannot be null.
                if (pPresentationDescriptor == null) // pvarStartPosition == null || 
                {
                    throw new COMException("null presentation descriptor", E_InvalidArgument);
                }

                // Check the time format. Must be "reference time" units.
                if ((pguidTimeFormat != null) && (pguidTimeFormat != Guid.Empty))
                {
                    // Unrecognized time format GUID.
                    throw new COMException("unrecognized time format guid", MFError.MF_E_UNSUPPORTED_TIME_FORMAT);
                }

                // Check the start position.
                if ((pvarStartPosition == null) || (pvarStartPosition.GetMFAttributeType() == MFAttributeType.None))
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
                else if (pvarStartPosition.GetMFAttributeType() == MFAttributeType.Uint64)
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
                    throw new COMException("We don't support this time format", MFError.MF_E_UNSUPPORTED_TIME_FORMAT);
                }

                // Validate the caller's presentation descriptor.
                ValidatePresentationDescriptor(pPresentationDescriptor);

                // Sends the MENewStream or MEUpdatedStream event.
                QueueNewStreamEvent(pPresentationDescriptor);

                // Notify the stream of the new start time.
                m_pStream.SetPosition(llStartOffset);

                // Send Started or Seeked events. We will send them
                // 1. from the media source
                // 2. from each stream

                var = new PropVariant(llStartOffset);

                // (1) Send the source event.
                if (bIsSeek)
                {
                    QueueEvent(MediaEventType.MESourceSeeked, Guid.Empty, S_Ok, var);
                }
                else
                {
                    // For starting, if we are RESTARTING from the current position and our
                    // previous state was running/paused, then we need to add the
                    // MF_EVENT_SOURCE_ACTUAL_START attribute to the event. This requires
                    // creating the event object first.

                    IMFMediaEvent pEvent = null;

                    // Create the event.
                    MFExtern.MFCreateMediaEvent(
                        MediaEventType.MESourceStarted,
                        Guid.Empty,
                        S_Ok,
                        var,
                        out pEvent
                        );

                    // For restarts, set the actual start time as an attribute.
                    if (bIsRestartFromCurrentPosition)
                    {
                        pEvent.SetUINT64(MFAttributesClsid.MF_EVENT_SOURCE_ACTUAL_START, llStartOffset);
                    }

                    // Now  queue the event.
                    m_pEventQueue.QueueEvent(pEvent);

                    //SAFE_RELEASE(pEvent);
                }

                // 2. Send the stream event.
                if (m_pStream != null)
                {
                    if (bIsSeek)
                    {
                        m_pStream.QueueEvent(MediaEventType.MEStreamSeeked, Guid.Empty, S_Ok, var);
                    }
                    else
                    {
                        m_pStream.QueueEvent(MediaEventType.MEStreamStarted, Guid.Empty, S_Ok, var);
                    }
                }

                // Update our state.
                m_state = State.Started;

                // NOTE: If this method were implemented as an asynchronous operation
                // and the operation Failed asynchronously, the media source would need
                // to send an MESourceStarted event with the failure code. For this
                // sample, all operations are synchronous (which is allowed), so any
                // failures are also synchronous.


                var.Clear();
            }
        }

        //-------------------------------------------------------------------
        // Name: Pause
        // Description: Switches to paused state.
        //-------------------------------------------------------------------

        public void Pause()
        {
            m_Log.WriteLine("-Pause");

            lock (this)
            {
                CheckShutdown();

                // Pause is only allowed from started state.
                if (m_state != State.Started)
                {
                    throw new COMException("Not started", MFError.MF_E_INVALID_STATE_TRANSITION);
                }

                // Send the appropriate events.
                if (m_pStream != null)
                {
                    m_pStream.QueueEvent(MediaEventType.MEStreamPaused, Guid.Empty, S_Ok, null);
                }

                QueueEvent(MediaEventType.MESourcePaused, Guid.Empty, S_Ok, null);

                // Update our state.
                m_state = State.Paused;
            }

            // Nothing else for us to do.
        }

        //-------------------------------------------------------------------
        // Name: Stop
        // Description: Switches to stopped state.
        //-------------------------------------------------------------------

        public void Stop()
        {
            m_Log.WriteLine("-Stop");

            lock (this)
            {
                CheckShutdown();

                // Queue events.
                if (m_pStream != null)
                {
                    m_pStream.QueueEvent(MediaEventType.MEStreamStopped, Guid.Empty, S_Ok, null);
                }

                QueueEvent(MediaEventType.MESourceStopped, Guid.Empty, S_Ok, null);

                // Update our state.
                m_state = State.Stopped;
            }
        }

        //-------------------------------------------------------------------
        // Name: Shutdown
        // Description: Releases resources.
        //
        // The source and stream objects hold reference counts on each other.
        // To avoid memory leaks caused by circular ref. counts, the Shutdown
        // method releases the pointer to the stream.
        //-------------------------------------------------------------------

        public void Shutdown()
        {
            m_Log.WriteLine("-Shutdown");

            lock (this)
            {
                CheckShutdown();

                // Shut down the stream object.
                if (m_pStream != null)
                {
                    m_pStream.Shutdown();
                }

                // Shut down the event queue.
                if (m_pEventQueue != null)
                {
                    try
                    {
                        m_pEventQueue.Shutdown();
                    }
                    catch { }
                }

                // Release objects. (Even if Shutdown Failed for some reason.)
                Dispose();

                // Set our shutdown flag.
                m_IsShutdown = true;
            }
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

        internal void Open(IMFByteStream pStream)
        {
            m_Log.WriteLine("Open");

            lock (this)
            {
                if (m_pRiff != null)
                {
                    // The media source has already been opened.
                    throw new COMException("The media source has already been opened", MFError.MF_E_INVALIDREQUEST);
                }

                try
                {
                    // Create a new WAVE RIFF parser object to parse the stream.
                    CWavRiffParser.Create(pStream, out m_pRiff);

                    // Parse the WAVE header. This fails if the header is not
                    // well-formed.
                    m_pRiff.ParseWAVEHeader();

                    // Validate the WAVEFORMATEX structure from the file header.
                    ValidateWaveFormat(m_pRiff.Format(), m_pRiff.FormatSize());
                }
                catch
                {
                    Shutdown();
                    throw;
                }
            }
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

        private void CreatePresentationDescriptor()
        {
            m_Log.WriteLine("CreatePresentationDescriptor");

            IMFMediaType pMediaType = null;
            IMFStreamDescriptor pStreamDescriptor = null;
            IMFMediaTypeHandler pHandler = null;

            Debug.Assert(WaveFormat() != null);

            // Create an empty media type.
            MFExtern.MFCreateMediaType(out pMediaType);

            // Initialize the media type from the WAVEFORMATEX structure.
            MFExtern.MFInitMediaTypeFromWaveFormatEx(pMediaType, WaveFormat(), WaveFormatSize());

            IMFMediaType[] mt = new IMFMediaType[1];
            mt[0] = pMediaType;

            // Create the stream descriptor.
            MFExtern.MFCreateStreamDescriptor(
                0,          // stream identifier
                mt.Length,          // Number of media types.
                mt, // Array of media types
                out pStreamDescriptor
                );

            // Set the default media type on the media type handler.
            pStreamDescriptor.GetMediaTypeHandler(out pHandler);
            pHandler.SetCurrentMediaType(pMediaType);

            IMFStreamDescriptor[] ms = new IMFStreamDescriptor[1];
            ms[0] = pStreamDescriptor;

            // Create the presentation descriptor.
            MFExtern.MFCreatePresentationDescriptor(
                ms.Length,      // Number of stream descriptors
                ms, // Array of stream descriptors
                out m_pPresentationDescriptor
                );

            // Select the first stream
            m_pPresentationDescriptor.SelectStream(0);

            // Set the file duration as an attribute on the presentation descriptor.
            long duration = m_pRiff.FileDuration();
            m_pPresentationDescriptor.SetUINT64(MFAttributesClsid.MF_PD_DURATION, (long)duration);

            //SAFE_RELEASE(pMediaType);
            //SAFE_RELEASE(pStreamDescriptor);
            //SAFE_RELEASE(pHandler);
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

        private void ValidatePresentationDescriptor(IMFPresentationDescriptor pPD)
        {
            m_Log.WriteLine("ValidatePresentationDescriptor");

            Debug.Assert(pPD != null);

            IMFStreamDescriptor pStreamDescriptor = null;
            IMFMediaTypeHandler pHandler = null;
            IMFMediaType pMediaType = null;
            WaveFormatEx pFormat = null;

            int cStreamDescriptors = 0;
            bool fSelected = false;

            // Make sure there is only one stream.
            pPD.GetStreamDescriptorCount(out cStreamDescriptors);

            if (cStreamDescriptors != 1)
            {
                throw new COMException("not just 1 stream", MFError.MF_E_UNSUPPORTED_REPRESENTATION);
            }

            // Get the stream descriptor.
            pPD.GetStreamDescriptorByIndex(0, out fSelected, out pStreamDescriptor);

            // Make sure it's selected. (This media source has only one stream, so it
            // is not useful to deselect the only stream.)
            if (!fSelected)
            {
                throw new COMException("not selected", MFError.MF_E_UNSUPPORTED_REPRESENTATION);
            }

            // Get the media type handler, so that we can get the media type.
            pStreamDescriptor.GetMediaTypeHandler(out pHandler);

            pHandler.GetCurrentMediaType(out pMediaType);

            int iSize;

            // Deprecated method, but it works
            //IntPtr ip = pAudioType.GetAudioFormat();
            //pFormat = new WaveFormatEx();
            //Marshal.PtrToStructure(ip, pFormat);

            MFExtern.MFCreateWaveFormatExFromMFMediaType(
                pMediaType,
                out pFormat,
                out iSize,
                MFWaveFormatExConvertFlags.Normal);

            if ((pFormat == null) || (this.WaveFormat() == null))
            {
                throw new COMException("bad format or waveformat", MFError.MF_E_INVALIDMEDIATYPE);
            }

            if (!pFormat.Equals(WaveFormat()))
            {
                throw new COMException("wave formats don't match", MFError.MF_E_INVALIDMEDIATYPE);
            }

            //SAFE_RELEASE(pStreamDescriptor);
            //SAFE_RELEASE(pHandler);
            //SAFE_RELEASE(pMediaType);
            //SAFE_RELEASE(pAudioType);
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

        private void QueueNewStreamEvent(IMFPresentationDescriptor pPD)
        {
            Debug.Assert(pPD != null);

            m_Log.WriteLine("QueueNewStreamEvent");

            IMFStreamDescriptor pSD = null;

            bool fSelected = false;
            pPD.GetStreamDescriptorByIndex(0, out fSelected, out pSD);

            // The stream must be selected, because we don't allow the app
            // to de-select the stream. See ValidatePresentationDescriptor.
            Debug.Assert(fSelected);

            if (m_pStream != null)
            {
                // The stream already exists, and is still selected.
                // Send the MEUpdatedStream event.

                QueueEvent(MediaEventType.MEUpdatedStream, Guid.Empty, S_Ok, new PropVariant(m_pStream));
            }
            else
            {
                // The stream does not exist, and is now selected.
                // Create a new stream.
                CreateWavStream(pSD);

                // CreateWavStream creates the stream, so m_pStream is no longer null.
                Debug.Assert(m_pStream != null);

                // Send the MENewStream event.
                QueueEvent(MediaEventType.MENewStream, Guid.Empty, S_Ok, new PropVariant(m_pStream));
            }

            //SAFE_RELEASE(pSD);
        }

        private void CreateWavStream(IMFStreamDescriptor pSD)
        {
            m_Log.WriteLine("CreateWavStream");

            m_pStream = new WavStream(this, m_pRiff, pSD);
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
        private void ValidateWaveFormat(WaveFormatEx pWav, int cbSize)
        {
            m_Log.WriteLine("ValidateWaveFormat");

            if (pWav.wFormatTag != WAVE_FORMAT_PCM)
            {
                throw new COMException("bad wFormatTag", MFError.MF_E_INVALIDMEDIATYPE);
            }

            if (pWav.nChannels != 1 && pWav.nChannels != 2)
            {
                throw new COMException("bad # channels", MFError.MF_E_INVALIDMEDIATYPE);
            }

            if (pWav.wBitsPerSample != 8 && pWav.wBitsPerSample != 16)
            {
                throw new COMException("bad bitspersample", MFError.MF_E_INVALIDMEDIATYPE);
            }

            if (pWav.cbSize != 0)
            {
                throw new COMException("bad cbsize", MFError.MF_E_INVALIDMEDIATYPE);
            }

            // Make sure block alignment was calculated correctly.
            if (pWav.nBlockAlign != pWav.nChannels * (pWav.wBitsPerSample / 8))
            {
                throw new COMException("bad align", MFError.MF_E_INVALIDMEDIATYPE);
            }

            // Check possible overflow...
            if (pWav.nSamplesPerSec > (int)(int.MaxValue / pWav.nBlockAlign))        // Is (nSamplesPerSec * nBlockAlign > MAXDWORD) ?
            {
                throw new COMException("overflow", MFError.MF_E_INVALIDMEDIATYPE);
            }

            // Make sure average bytes per second was calculated correctly.
            if (pWav.nAvgBytesPerSec != pWav.nSamplesPerSec * pWav.nBlockAlign)
            {
                throw new COMException("bad AvgBytesPerSec", MFError.MF_E_INVALIDMEDIATYPE);
            }
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

        private void CheckShutdown()
        {
            m_Log.WriteLine("CheckShutdown");

            if (m_IsShutdown)
            {
                throw new COMException("Is shut down", MFError.MF_E_SHUTDOWN);
            }
            else
            {
                // If the calling thread is STA, we aren't going to work right.  In
                // theory I could change this object to re-make every call on an MTA
                // thread.  Instead, I'm just going to insist on being called on MTA
                // thread in the first place.
                if (Thread.CurrentThread.GetApartmentState() != ApartmentState.MTA)
                {
                    throw new COMException("Must be called on MTA thread", unchecked((int)0x80040156)); //REGDB_E_BADTHREADINGMODEL;
                }
            }
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

        CWavRiffParser(IMFByteStream pStream)
            : base(pStream, new FourCC("RIFF"), 0)
        {
        }

        //-------------------------------------------------------------------
        // Name: Create
        // Description: Static creation function.
        //-------------------------------------------------------------------

        public static void Create(IMFByteStream pStream, out CWavRiffParser ppParser)
        {
            // Create a riff parser for the 'RIFF' container
            ppParser = new CWavRiffParser(pStream);

            // Check the RIFF file type.
            if (ppParser.RiffType() != new FourCC("WAVE"))
            {
                throw new COMException("not a WAVE file", MFError.MF_E_INVALID_FILE_FORMAT);
            }
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

        public void ParseWAVEHeader()
        {
            bool bFoundData = false;
            FourCC fmt = new FourCC("fmt ");
            FourCC data = new FourCC("data");

            // Iterate through the RIFF chunks. Ignore chunks we don't recognize.
            while (true)
            {
                if (Chunk().FourCC() == fmt)
                {
                    // Read the WAVEFORMATEX structure allegedly contained in this chunk.
                    // This method does NOT validate the contents of the structure.
                    ReadFormatBlock();
                }
                else if (Chunk().FourCC() == data)
                {
                    // Found the start of the audio data. The format chunk should precede the
                    // data chunk. If we did not find the formt chunk yet, that is a failure
                    // case (see below)
                    bFoundData = true;
                    break;
                }

                MoveToNextChunk();
            }

            // To be valid, the file must have a format chunk and a data chunk.
            // Fail if either of these conditions is not met.
            if (m_pWaveFormat == null || !bFoundData)
            {
                throw new COMException("no format/data chunk found", MFError.MF_E_INVALID_FILE_FORMAT);
            }

            m_rtDuration = Utils.AudioDurationFromBufferSize(m_pWaveFormat, Chunk().DataSize());
        }

        //-------------------------------------------------------------------
        // Name: ReadFormatBlock
        // Description: Reads the WAVEFORMATEX structure from the file header.
        //-------------------------------------------------------------------

        private void ReadFormatBlock()
        {
            Debug.Assert(Chunk().FourCC() == new FourCC("fmt "));
            Debug.Assert(m_pWaveFormat == null);

            try
            {
                int iWaveFormatExSize = Marshal.SizeOf(typeof(WaveFormatEx));

                // Some .wav files do not include the cbSize field of the WAVEFORMATEX
                // structure. For uncompressed PCM audio, field is always zero.
                int cbMinFormatSize = iWaveFormatExSize - Marshal.SizeOf(typeof(short));

                int cbFormatSize = 0;       // Size of the actual format block in the file.

                // Validate the size
                if (Chunk().DataSize() < cbMinFormatSize)
                {
                    throw new COMException("chunk too small", MFError.MF_E_INVALID_FILE_FORMAT);
                }

                // Allocate a buffer for the WAVEFORMAT structure.
                cbFormatSize = Chunk().DataSize();

                // We store a WAVEFORMATEX structure, so our format block must be at
                // least sizeof(WAVEFORMATEX) even if the format block in the file
                // is smaller. See note above about cbMinFormatSize.
                m_cbWaveFormat = Math.Max(cbFormatSize, iWaveFormatExSize);

                IntPtr ip = Marshal.AllocCoTaskMem(m_cbWaveFormat);

                try
                {
                    // Zero our structure, in case cbFormatSize < m_cbWaveFormat.
                    for (int x = 0; x < m_cbWaveFormat; x++)
                    {
                        Marshal.WriteByte(ip, x, 0);
                    }

                    // Now read cbFormatSize bytes from the file.
                    ReadDataFromChunk(ip, cbFormatSize);
                    m_pWaveFormat = new WaveFormatEx();

                    Marshal.PtrToStructure(ip, m_pWaveFormat);
                    Debug.Assert(m_pWaveFormat.cbSize == 0);
                }
                finally
                {
                    Marshal.FreeCoTaskMem(ip);
                }

            }
            catch
            {
                m_pWaveFormat = null;
                m_cbWaveFormat = 0;

                throw;
            }
        }
    }

}
