/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released
as public domain.  It is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

using MediaFoundation;
using MediaFoundation.Misc;

namespace Playlist
{
    #region Declarations

    public enum PlayerEvent
    {
        Initialized = MediaEventType.MEReservedMax + 1,           // Player initialized.
        SegmentDeleted,      //Topology deleted from the sequencer
        SegmentAdded         //Topology added to the sequencer
    }

    public struct TopologyStatusInfo
    {
        public MFTopoStatus iTopologyStatusType;
        public int iSegmentId;
    }

    public enum PlayerState
    {
        PlayerCreated,
        Initialized,
        Playing,
        Paused,
        Stopped
    }

    #endregion

    class CPlayer : COMBase, IMFAsyncCallback
    {
        #region Declarations

        [DllImport("user32", CharSet = CharSet.Auto)]
        private extern static int PostMessage(
            IntPtr handle, int msg, IntPtr wParam, IntPtr lParam);

        private class TimePair
        {
            public long hnsStartPresentationTime;
            public long hnsPresentationTimeOffset;
            public TimePair pNextTimePair;
        }

        #endregion

        #region Members

        IMFSequencerSource m_pSequencerSource;
        IMFPresentationClock m_pPresentationClock;
        IMFMediaSession m_pMediaSession;
        IMFActivate m_pAudioRendererActivate;

        // Application's window handle used by PostMessage
        IntPtr m_hWnd;

        //Player State
        PlayerState m_State;

        // SegmentList object
        CSegmentList m_Segments = new CSegmentList();

        // Timing data from the MESessionNotifyPresentationTime event.
        long m_PresentationTimeOffset;

        TimePair m_phnsTimePairStart;
        TimePair m_phnsTimePairEnd;

        // Event to wait on while closing

        AutoResetEvent m_hCloseEvent;

        TopologyStatusInfo topostat;

        int m_ActiveSegment;

        #endregion

        // ----- Public Methods -----------------------------------------------
        //////////////////////////////////////////////////////////////////////////
        //  Name: CPlayer
        //  Description: Constructor
        //
        /////////////////////////////////////////////////////////////////////////

        public CPlayer(IntPtr hWnd)
        {
            m_hWnd = hWnd;
            m_pMediaSession = null;
            m_pSequencerSource = null;
            m_pAudioRendererActivate = null;
            m_pPresentationClock = null;
            m_PresentationTimeOffset = 0;
            m_phnsTimePairStart = null;
            m_phnsTimePairEnd = null;
            m_State = PlayerState.PlayerCreated;
            m_ActiveSegment = -1;
            m_hCloseEvent = new AutoResetEvent(false);
        }

        //////////////////////////////////////////////////////////////////////////
        //  Name: ~CPlayer
        //  Description: Destructor
        //
        //  -Calls Shutdown
        /////////////////////////////////////////////////////////////////////////

        ~CPlayer()
        {
            ShutDown();
        }

        #region Public methods

        //////////////////////////////////////////////////////////////////////////
        //  Name: CPlayer::Initialize
        //  Description:
        //      Intializes Media Foundation
        //      Creates a media session
        //      Creates a sequencer source
        //      Creates a presentation clock
        //      Creates an audio renderer
        //      Starts the event queue
        //
        /////////////////////////////////////////////////////////////////////////
        public int Initialize()
        {
            Debug.WriteLine("\nCPlayer::Initialize");

            int hr = 0;

            try
            {
                IMFClock pClock;

                // Initialize Media Foundation.
                hr = MFExtern.MFStartup(0x10070, MFStartup.Full);
                MFError.ThrowExceptionForHR(hr);

                // Create the media session.
                hr = MFExtern.MFCreateMediaSession(null, out m_pMediaSession);
                MFError.ThrowExceptionForHR(hr);

                // Start the event queue.
                hr = m_pMediaSession.BeginGetEvent(this, null);
                MFError.ThrowExceptionForHR(hr);

                // Create a sequencer Source.
                hr = MFExtern.MFCreateSequencerSource(null, out m_pSequencerSource);
                MFError.ThrowExceptionForHR(hr);

                //setup clock
                hr = m_pMediaSession.GetClock(out pClock);
                MFError.ThrowExceptionForHR(hr);

                m_pPresentationClock = (IMFPresentationClock)pClock;

                // Create an IMFActivate object for the audio renderer.
                hr = MFExtern.MFCreateAudioRendererActivate(out m_pAudioRendererActivate);
                MFError.ThrowExceptionForHR(hr);

                //Set the player state to Initialized
                m_State = PlayerState.Initialized;

                // Notify the app that the player is initialized.
                PostMessage(m_hWnd, Form1.WM_NOTIFY_APP, new IntPtr((int)PlayerEvent.Initialized), new IntPtr((int)m_State));
            }
            catch (Exception e)
            {
                hr = Marshal.GetHRForException(e);
            }

            //Clean up.
            return hr;
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        //  Name: CPlayer::AddToPlaylist (Public)
        //  Description:
        //      Adds a new segment to the playlist.
        //      If the new segment is the first one to be added to the sequencer:
        //          Queues it on the media session.
        //      Otherwise:
        //          Resets the last topology flag and sets it to the newly added segment
        //
        //  Parameter:
        //      sURL: [in] File URL
        /////////////////////////////////////////////////////////////////////////////////////////

        public int AddToPlaylist(string sURL)
        {
            Debug.WriteLine("\nCPlayer::AddToPlaylist");

            int hr = 0;

            try
            {
                if (sURL == null)
                {
                    throw new COMException("Null url", E_Pointer);
                }

                IMFPresentationDescriptor pPresentationDescriptor;
                IMFMediaSource pMediaSource;

                int SegmentId = 0;

                if (m_Segments.GetCount() != 0)
                {
                    //Get the last segment id
                    m_Segments.GetLastSegmentId(out SegmentId);

                    //reset the last topology in the sequencer
                    hr = m_pSequencerSource.UpdateTopologyFlags(SegmentId, 0);
                    MFError.ThrowExceptionForHR(hr);
                }

                //Create media source and topology, and add it to the sequencer
                AddSegment(sURL, out SegmentId);

                //Set the last topology
                hr = m_pSequencerSource.UpdateTopologyFlags(SegmentId, MFSequencerTopologyFlags.Last);
                MFError.ThrowExceptionForHR(hr);

                //If this is the first segment in the sequencer, queue it on the session
                if (m_Segments.GetCount() == 1)
                {
                    pMediaSource = (IMFMediaSource)m_pSequencerSource;

                    hr = pMediaSource.CreatePresentationDescriptor(out pPresentationDescriptor);
                    MFError.ThrowExceptionForHR(hr);

                    try
                    {
                        QueueNextSegment(pPresentationDescriptor, out SegmentId);
                    }
                    finally
                    {
                        SafeRelease(pPresentationDescriptor);
                    }
                }

                //Notify the app
                PostMessage(m_hWnd, Form1.WM_NOTIFY_APP, new IntPtr((int)PlayerEvent.SegmentAdded), new IntPtr(SegmentId));
            }
            catch (Exception e)
            {
                hr = Marshal.GetHRForException(e);
            }

            return hr;
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        //  Name: CPlayer::DeleteSegment (Public)
        //  Description:
        //      Deletes the corresponding topology from the sequencer source
        //  Parameter:
        //      SegmentID: [in] The segment identifier
        ///////////////////////////////////////////////////////////////////////////////////////////

        public int DeleteSegment(int SegmentID)
        {
            Debug.WriteLine(string.Format("\nCPlayer::DeleteSegment: {0}", SegmentID));

            int hr = 0;

            try
            {
                if (m_ActiveSegment == SegmentID)
                {
                    throw new COMException("Can't delete active segment", E_InvalidArgument);
                }

                int SegId = 0;

                hr = m_pSequencerSource.DeleteTopology(SegmentID);
                MFError.ThrowExceptionForHR(hr);

                m_Segments.GetLastSegmentId(out SegId);

                //Delete the segment entry from the list.
                m_Segments.DeleteSegmentEntry(SegmentID);

                //Is the deleted topology the last one?
                if (SegId == SegmentID)
                {
                    //Get the new last segment id

                    try
                    {
                        m_Segments.GetLastSegmentId(out SegId);

                        //set this topology as the last in the sequencer
                        hr = m_pSequencerSource.UpdateTopologyFlags(SegId, MFSequencerTopologyFlags.Last);
                        MFError.ThrowExceptionForHR(hr);
                    }
                    catch { }
                }

                PostMessage(m_hWnd, Form1.WM_NOTIFY_APP, new IntPtr((int)PlayerEvent.SegmentDeleted), new IntPtr(SegmentID));
            }
            catch (Exception e)
            {
                hr = Marshal.GetHRForException(e);
            }

            return hr;
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        //  Name: CPlayer::Play (Public)
        //  Description:
        //      Starts the media session with the current topology
        ///////////////////////////////////////////////////////////////////////////////////////////

        public int Play()
        {
            Debug.WriteLine("\nCPlayer::Play");

            int hr = 0;

            try
            {
                // Create the starting position parameter
                PropVariant var = new PropVariant();

                hr = m_pMediaSession.Start(Guid.Empty, var);
                MFError.ThrowExceptionForHR(hr);
            }
            catch (Exception e)
            {
                hr = Marshal.GetHRForException(e);
            }

            return hr;
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        //  Name: CPlayer::Pause (Public)
        //  Description:
        //      Pauses the media session with the current topology
        ///////////////////////////////////////////////////////////////////////////////////////////

        public int Pause()
        {
            Debug.WriteLine("\nCPlayer::Pause");

            int hr = 0;

            try
            {
                // pause the media session.
                hr = m_pMediaSession.Pause();
                MFError.ThrowExceptionForHR(hr);
            }
            catch (Exception e)
            {
                hr = Marshal.GetHRForException(e);
            }

            return hr;
        }

        public PlayerState GetState()
        {
            return m_State;
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        //  Name: CPlayer::Stop (Public)
        //  Description:
        //      Stops the media session with the current topology
        ///////////////////////////////////////////////////////////////////////////////////////////

        public int Stop()
        {
            Debug.WriteLine("CPlayer::Stop");

            int hr = 0;

            try
            {
                hr = m_pMediaSession.Stop();
                MFError.ThrowExceptionForHR(hr);
            }
            catch (Exception e)
            {
                hr = Marshal.GetHRForException(e);
            }

            return hr;
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        //  Name: CPlayer::Skip (Public)
        //  Description:
        //      Skips to the specified segment in the sequencer source
        //  Parameter:
        //      SegmentID: [in] The segment identifier
        ///////////////////////////////////////////////////////////////////////////////////////////

        public int Skip(int SegmentID)
        {
            Debug.WriteLine("\nCPlayer::Skip");

            int hr = 0;

            PropVariant var = new PropVariant();

            try
            {
                hr = m_pMediaSession.Stop();
                MFError.ThrowExceptionForHR(hr);

                hr = MFExtern.MFCreateSequencerSegmentOffset(SegmentID, 0, var);
                MFError.ThrowExceptionForHR(hr);

                hr = m_pMediaSession.Start(CLSID.MF_TIME_FORMAT_SEGMENT_OFFSET, var);
                MFError.ThrowExceptionForHR(hr);
            }
            catch (Exception e)
            {
                hr = Marshal.GetHRForException(e);
            }
            finally
            {
                var.Clear();
            }

            return hr;
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        //  Name: CPlayer::GetCurrentSegmentTime (Public)
        //  Description:
        //      Gets the lastest play time of the current segment
        //  Parameter:
        //      phnsCurrentTime: [out] Playtime of the current segment
        ///////////////////////////////////////////////////////////////////////////////////////////

        public int GetCurrentSegmentTime(out long phnsCurrentTime)
        {
            int hr = 0;

            try
            {
                hr = m_pPresentationClock.GetTime(out phnsCurrentTime);
                MFError.ThrowExceptionForHR(hr);

                if (m_phnsTimePairStart != null)
                {
                    if (phnsCurrentTime >= m_phnsTimePairStart.hnsStartPresentationTime)
                    {
                        //update m_PresentationTimeOffset
                        m_PresentationTimeOffset = m_phnsTimePairStart.hnsPresentationTimeOffset;

                        TimePair temp = m_phnsTimePairStart;

                        m_phnsTimePairStart = m_phnsTimePairStart.pNextTimePair;
                    }
                }

                phnsCurrentTime -= m_PresentationTimeOffset;
            }
            catch (Exception e)
            {
                hr = Marshal.GetHRForException(e);
                phnsCurrentTime = 0;
            }

            return hr;
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        //  Name: CPlayer::GetSegmentInfo (Public)
        //  Description:
        //      Gets the segment info based on segment identifier: Duration, segment name.
        //  Parameter:
        //      SegmentID: [in] Segment identifier
        //      phnsSegmentDuration: [out] Receives the segment duration
        //      szSegmentURL: [out] Receives segment name
        //      dwSize: [in] Size of szSegmentURL
        /////////////////////////////////////////////////////////////////////////////////////////

        public int GetSegmentInfo(
                            int SegmentID,
                            out long phnsSegmentDuration,
                            out string szSegmentURL)
        {
            int hr = 0;

            try
            {
                m_Segments.GetSegmentEntryInfo(SegmentID, out phnsSegmentDuration, out szSegmentURL);
            }
            catch (Exception e)
            {
                hr = Marshal.GetHRForException(e);
                phnsSegmentDuration = 0;
                szSegmentURL = null;
            }

            return hr;
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        //  Name: CPlayer::ShutDown (Public)
        //  Description:
        //      Releases all resources and shuts down Media Foundation
        ///////////////////////////////////////////////////////////////////////////////////////////

        int ShutDown()
        {
            Debug.WriteLine("\nCPlayer::ShutDown");

            int hr = 0;

            IMFMediaSource pMediaSource;

            try
            {
                //Call shutdown on the sequencer source
                pMediaSource = (IMFMediaSource)m_pSequencerSource;

                hr = pMediaSource.Shutdown();
                MFError.ThrowExceptionForHR(hr);

                //Close media session
                if (m_pMediaSession != null)
                {
                    hr = m_pMediaSession.Close();
                    MFError.ThrowExceptionForHR(hr);

                    // Wait for the close operation to complete
                    bool res = m_hCloseEvent.WaitOne(5000, false);
                    if (!res)
                    {
                        Debug.WriteLine("WaitForSingleObject timed out!");
                    }

                    m_hCloseEvent.Close();
                    m_hCloseEvent = null;
                }

                //Shutdown media session
                hr = m_pMediaSession.Shutdown();
                MFError.ThrowExceptionForHR(hr);

                // Shut down Media Foundation.
                hr = MFExtern.MFShutdown();
                MFError.ThrowExceptionForHR(hr);
            }
            catch (Exception e)
            {
                hr = Marshal.GetHRForException(e);
            }

            SafeRelease(m_pMediaSession);
            SafeRelease(m_pSequencerSource);
            SafeRelease(m_pPresentationClock);
            SafeRelease(m_pAudioRendererActivate);

            return hr;
        }

        #region IMFAsyncCallback methods

        int IMFAsyncCallback.GetParameters(out MFASync pdwFlags, out MFAsyncCallbackQueue pdwQueue)
        {
            pdwFlags = MFASync.None;
            pdwQueue = 0;

            return S_Ok;
        }

        //////////////////////////////////////////////////////////////////////////
        //  Name: CPlayer::Invoke
        //  Description:
        //      Implementation of CAsyncCallback::Invoke.
        //      Callback for asynchronous BeginGetEvent method.
        //  Parameter:
        //      pAsyncResult: Pointer to the result.
        //
        /////////////////////////////////////////////////////////////////////////

        int IMFAsyncCallback.Invoke(IMFAsyncResult pAsyncResult)
        {
            MediaEventType eventType = MediaEventType.MEUnknown;
            IMFMediaEvent pEvent;
            PropVariant eventData = null;
            Exception excpt = null;
            int hr;

            try
            {
                int eventStatus = 0;             // Event status
                eventData = new PropVariant();                  // Event data

                // Get the event from the event queue.
                hr = m_pMediaSession.EndGetEvent(pAsyncResult, out pEvent);
                MFError.ThrowExceptionForHR(hr);

                // Get the event type.
                hr = pEvent.GetType(out eventType);
                MFError.ThrowExceptionForHR(hr);

                // Get the event data
                hr = pEvent.GetValue(eventData);
                MFError.ThrowExceptionForHR(hr);

                // Get the event status. If the operation that triggered the event
                // did not succeed, the status is a failure code.
                hr = pEvent.GetStatus(out eventStatus);
                MFError.ThrowExceptionForHR(hr);

                // Switch on the event type. Update the internal state of the CPlayer
                // as needed.

                switch (eventType)
                {
                    // Session events
                    case MediaEventType.MESessionStarted:
                        {
                            Debug.WriteLine(string.Format("{0}: MESessionStarted, Status: 0x{1:x}", eventType.ToString(), eventStatus));

                            m_State = PlayerState.Playing;
                            PostMessage(m_hWnd, Form1.WM_NOTIFY_APP, new IntPtr((int)eventType), new IntPtr((int)m_State));

                            break;
                        }
                    case MediaEventType.MESessionPaused:
                        {
                            Debug.WriteLine(string.Format("{0}: MESessionPaused, Status: 0x{1:x}", eventType.ToString(), eventStatus));

                            m_State = PlayerState.Paused;
                            PostMessage(m_hWnd, Form1.WM_NOTIFY_APP, new IntPtr((int)eventType), new IntPtr((int)m_State));

                            break;
                        }
                    case MediaEventType.MESessionStopped:
                        {
                            Debug.WriteLine(string.Format("{0}: MESessionStopped, Status: 0x{1:x}", eventType.ToString(), eventStatus));

                            m_State = PlayerState.Stopped;
                            PostMessage(m_hWnd, Form1.WM_NOTIFY_APP, new IntPtr((int)eventType), new IntPtr((int)m_State));

                            break;
                        }

                    case MediaEventType.MESessionTopologyStatus:
                        {
                            Debug.WriteLine(string.Format("{0}: MESessionTopologyStatus, Status: 0x{1:x}", eventType.ToString(), eventStatus));

                            int value = 0;

                            hr = pEvent.GetUINT32(MFAttributesClsid.MF_EVENT_TOPOLOGY_STATUS, out value);
                            MFError.ThrowExceptionForHR(hr);
                            int SegmentID = 0;
                            long ID;

                            //Get information about the new segment
                            IMFTopology pTopology;

                            pTopology = (IMFTopology)eventData.GetIUnknown();

                            try
                            {
                                hr = pTopology.GetTopologyID(out ID);
                                MFError.ThrowExceptionForHR(hr);
                                m_Segments.GetSegmentIDByTopoID(ID, out SegmentID);

                                topostat.iTopologyStatusType = (MFTopoStatus)value;
                                topostat.iSegmentId = SegmentID;

                                switch (topostat.iTopologyStatusType)
                                {
                                    case MFTopoStatus.StartedSource:
                                        m_ActiveSegment = SegmentID;
                                        break;

                                    case MFTopoStatus.Ended:
                                        m_ActiveSegment = -1;
                                        break;
                                }

                                GCHandle gc = GCHandle.Alloc(topostat);

                                PostMessage(m_hWnd, Form1.WM_NOTIFY_APP, new IntPtr((int)MediaEventType.MESessionTopologyStatus), GCHandle.ToIntPtr(gc));
                            }
                            finally
                            {
                                SafeRelease(pTopology);
                            }

                            break;
                        }
                    case MediaEventType.MENewPresentation:
                        {
                            Debug.WriteLine(string.Format("{0}: MENewPresentation, Status: 0x{1:x}", eventType.ToString(), eventStatus));

                            IMFPresentationDescriptor pPresentationDescriptor;

                            int SegmentId = 0;

                            pPresentationDescriptor = (IMFPresentationDescriptor)eventData.GetIUnknown();

                            try
                            {
                                //Queue the next segment on the media session
                                QueueNextSegment(pPresentationDescriptor, out SegmentId);
                            }
                            finally
                            {
                                SafeRelease(pPresentationDescriptor);
                            }

                            PostMessage(m_hWnd, Form1.WM_NOTIFY_APP, new IntPtr((int)eventType), new IntPtr(SegmentId));

                            break;
                        }

                    case MediaEventType.MEEndOfPresentation:
                        {
                            Debug.WriteLine(string.Format("{0}: MEEndOfPresentation, Status: 0x{1:x}", eventType.ToString(), eventStatus));

                            int value = 0;

                            try
                            {
                                hr = pEvent.GetUINT32(MFAttributesClsid.MF_EVENT_SOURCE_TOPOLOGY_CANCELED, out value);
                                MFError.ThrowExceptionForHR(hr);
                            }
                            catch { }

                            PostMessage(m_hWnd, Form1.WM_NOTIFY_APP, new IntPtr((int)eventType), new IntPtr(value));

                            break;
                        }

                    case MediaEventType.MEEndOfPresentationSegment:
                        {
                            Debug.WriteLine(string.Format("{0}: MEEndOfPresentationSegment, Status: 0x{1:x}", eventType.ToString(), eventStatus));

                            int value = 0;

                            try
                            {
                                hr = pEvent.GetUINT32(MFAttributesClsid.MF_EVENT_SOURCE_TOPOLOGY_CANCELED, out value);
                                MFError.ThrowExceptionForHR(hr);
                            }
                            catch { }

                            PostMessage(m_hWnd, Form1.WM_NOTIFY_APP, new IntPtr((int)eventType), new IntPtr(value));

                            break;
                        }

                    case MediaEventType.MESessionNotifyPresentationTime:
                        {
                            Debug.WriteLine(string.Format("{0}: MESessionNotifyPresentationTime, Status: 0x{1:x}", eventType.ToString(), eventStatus));

                            HandleNotifyPresentationTime(pEvent);
                            break;
                        }
                    case MediaEventType.MESessionClosed:
                        {
                            Debug.WriteLine(string.Format("{0}: MESessionClosed, Status: 0x{1:x}", eventType.ToString(), eventStatus));

                            m_hCloseEvent.Set();

                            break;
                        }

                    default:
                        Debug.WriteLine(string.Format("{0}: Event", eventType.ToString()));
                        break;
                }

            }
            catch (Exception e)
            {
                excpt = e;
            }
            finally
            {
                if (eventData != null)
                {
                    eventData.Clear();
                }
            }

            // Request another event.
            if (eventType != MediaEventType.MESessionClosed)
            {
                hr = m_pMediaSession.BeginGetEvent(this, null);
                MFError.ThrowExceptionForHR(hr);
            }

            if (excpt != null)
            {
                throw excpt;
            }

            return S_Ok;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////
        //  Name: CPlayer::GetPresentationTime (Public)
        //  Description:
        //      Gets the unadjusted presentation time
        //  Parameter:
        //      phnsPresentationTime: [out] presentation time
        ///////////////////////////////////////////////////////////////////////////////////////////

        public int GetPresentationTime(out long phnsPresentationTime)
        {
            int hr = 0;

            try
            {
                hr = m_pPresentationClock.GetTime(out phnsPresentationTime);
                MFError.ThrowExceptionForHR(hr);
            }
            catch (Exception e)
            {
                hr = Marshal.GetHRForException(e);
                phnsPresentationTime = 0;
            }

            return hr;
        }

        #endregion

        #region Private methods

        ////////////////////////////////////////////////////////////////////////////////////////
        //  Name: CPlayer::CreateNodesForStream (Private)
        //  Description:
        //      Creates the source and output nodes for a stream and
        //      Adds them to the topology
        //      Connects the source node to the output node
        //
        //  Parameter:
        //      pPresentationDescriptor: [in] Pointer to the presentation descriptor for the media source
        //      pMediaSource: [in] Pointer to the media source
        //      pTopology: [in] Pointer to the topology
        //
        //  Notes: For each stream, the app must:
        //      1. Create a source node associated with the stream.
        //      2. Create an output node for the renderer.
        //      3. Connect the two nodes.
        //      The media session will resolve the topology, transform nodes are not required
        /////////////////////////////////////////////////////////////////////////////////////////

        private void CreateNodesForStream(
                            IMFPresentationDescriptor pPresentationDescriptor,
                            IMFMediaSource pMediaSource,
                            IMFTopology pTopology)
        {
            if (pPresentationDescriptor == null || pMediaSource == null || pTopology == null)
            {
                throw new COMException("null pointer", E_Pointer);
            }

            int hr;
            IMFStreamDescriptor pStreamDescriptor;
            IMFTopologyNode pSourceNode;
            IMFTopologyNode pOutputNode;

            bool fSelected = false;

            // Get the stream descriptor for the only stream index =0.
            hr = pPresentationDescriptor.GetStreamDescriptorByIndex(0, out fSelected, out pStreamDescriptor);
            MFError.ThrowExceptionForHR(hr);

            try
            {
                if (fSelected)
                {
                    // Create a source node for this stream and add it to the topology.
                    CreateSourceNode(pPresentationDescriptor, pStreamDescriptor, pMediaSource, out pSourceNode);

                    try
                    {
                        hr = pTopology.AddNode(pSourceNode);
                        MFError.ThrowExceptionForHR(hr);

                        // Create the output node for the renderer and add it to the topology.
                        CreateOutputNode(pStreamDescriptor, out pOutputNode);

                        try
                        {
                            hr = pTopology.AddNode(pOutputNode);
                            MFError.ThrowExceptionForHR(hr);

                            // Connect the source node to the output node.
                            hr = pSourceNode.ConnectOutput(0, pOutputNode, 0);
                            MFError.ThrowExceptionForHR(hr);
                        }
                        finally
                        {
                            SafeRelease(pOutputNode);
                        }
                    }
                    finally
                    {
                        SafeRelease(pSourceNode);
                    }
                }
            }
            finally
            {
                //clean up
                SafeRelease(pStreamDescriptor);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        //  Name: CPlayer::CreateSourceNode (Private)
        //  Description:
        //      Creates the source node for a stream
        //  Parameter:
        //      pPresentationDescriptor: [in] Pointer to the presentation descriptor for the media source
        //      pStreamDescriptor: [in] Stream descriptor for the stream
        //      pMediaSource: [in] Pointer to the media source
        //      ppSourceNode: [out] Receives a pointer to the new node
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void CreateSourceNode(
            IMFPresentationDescriptor pPresentationDescriptor,
            IMFStreamDescriptor pStreamDescriptor,
            IMFMediaSource pMediaSource,
            out IMFTopologyNode ppSourceNode)
        {
            if (pPresentationDescriptor == null || pMediaSource == null || pStreamDescriptor == null)
            {
                throw new COMException("null pointer", E_Pointer);
            }

            int hr;
            // Create the source-stream node.
            hr = MFExtern.MFCreateTopologyNode(MFTopologyType.SourcestreamNode, out ppSourceNode);
            MFError.ThrowExceptionForHR(hr);

            // Set attribute: Pointer to the media source. Necessary.
            hr = ppSourceNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_SOURCE, pMediaSource);
            MFError.ThrowExceptionForHR(hr);

            // Set attribute: Pointer to the presentation descriptor. Necessary.
            hr = ppSourceNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_PRESENTATION_DESCRIPTOR, pPresentationDescriptor);
            MFError.ThrowExceptionForHR(hr);

            // Set attribute: Pointer to the stream descriptor. Necessary.
            hr = ppSourceNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_STREAM_DESCRIPTOR, pStreamDescriptor);
            MFError.ThrowExceptionForHR(hr);
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        //  Name: CPlayer::CreateOutputNode (Private)
        //  Description:
        //      Creates an output node for a stream
        //      Sets the IActivate pointer on the node
        //  Parameter:
        //      pStreamDescriptor: [in] Stream descriptor for the stream
        //      ppSourceNode: [out] Receives a pointer to the new node
        ////////////////////////////////////////////////////////////////////////////////////////

        private void CreateOutputNode(
                            IMFStreamDescriptor pStreamDescriptor,
                            out IMFTopologyNode ppOutputNode)
        {
            if (pStreamDescriptor == null)
            {
                throw new COMException("null pointer", E_Pointer);
            }

            IMFMediaTypeHandler pHandler;

            Guid guidMajorType = Guid.Empty;

            // Create a downstream node.
            int hr = MFExtern.MFCreateTopologyNode(MFTopologyType.OutputNode, out ppOutputNode);
            MFError.ThrowExceptionForHR(hr);

            // Get the media type handler for the stream.
            hr = pStreamDescriptor.GetMediaTypeHandler(out pHandler);
            MFError.ThrowExceptionForHR(hr);

            try
            {
                // Get the major media type.
                pHandler.GetMajorType(out guidMajorType);

                // Set the IActivate object on the output node.
                if (MFMediaType.Audio == guidMajorType)
                {
                    hr = ppOutputNode.SetObject(m_pAudioRendererActivate);
                    MFError.ThrowExceptionForHR(hr);
                    Debug.WriteLine(("Audio stream"));
                }
                //Only audio is implemented, if guidMajorType is any other type, return E_NOTIMPL
                else
                {
                    Debug.WriteLine(("Unsupported stream"));
                    throw new COMException("Unsupported stream", E_NotImplemented);
                }
            }
            finally
            {
                // Clean up.
                SafeRelease(pHandler);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        //  Name: CPlayer::HandleNotifyPresentationTime (Private)
        //  Description:
        //      Handles the media session's MESessionNotifyPresentationTime event
        //  Parameter:
        //      pEvent: [in] MESessionNotifyPresentationTime event
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void HandleNotifyPresentationTime(IMFMediaEvent pEvent)
        {
            if (pEvent == null)
            {
                throw new COMException("null pointer", E_Pointer);
            }

            if (m_phnsTimePairStart == null)
            {
                m_phnsTimePairStart = new TimePair();
                m_phnsTimePairStart.pNextTimePair = null;
                m_phnsTimePairEnd = m_phnsTimePairStart;
            }
            else
            {
                m_phnsTimePairEnd.pNextTimePair = new TimePair();
                m_phnsTimePairEnd = m_phnsTimePairEnd.pNextTimePair;
                m_phnsTimePairEnd.pNextTimePair = null;
            }

            int hr = pEvent.GetUINT64(
                        MFAttributesClsid.MF_EVENT_START_PRESENTATION_TIME,
                        out m_phnsTimePairEnd.hnsStartPresentationTime);
            MFError.ThrowExceptionForHR(hr);

            hr = pEvent.GetUINT64(
                        MFAttributesClsid.MF_EVENT_PRESENTATION_TIME_OFFSET,
                        out m_phnsTimePairEnd.hnsPresentationTimeOffset);
            MFError.ThrowExceptionForHR(hr);
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        //  Name: CPlayer::CreateMediaSource (Private)
        //  Description:
        //      Creates a media source from URL
        //  Parameter:
        //      sURL: [in] File URL
        //      ppMediaSource: [out] Receives the media source
        /////////////////////////////////////////////////////////////////////////////////////////

        private void CreateMediaSource(
                            string sURL,
                            out IMFMediaSource ppMediaSource)
        {
            Debug.WriteLine("CPlayer::CreateMediaSource");

            if (sURL == null)
            {
                throw new COMException("null pointer", E_Pointer);
            }

            int hr;
            IMFSourceResolver pSourceResolver;
            object pSourceUnk;

            hr = MFExtern.MFCreateSourceResolver(out pSourceResolver);
            MFError.ThrowExceptionForHR(hr);

            try
            {
                // Use the source resolver to create the media source.
                MFObjectType ObjectType = MFObjectType.Invalid;

                hr = pSourceResolver.CreateObjectFromURL(
                        sURL,                       // URL of the source.
                        MFResolution.MediaSource,  // Create a source object.
                        null,                       // Optional property store.
                        out ObjectType,                // Receives the created object type.
                        out pSourceUnk                 // Receives a pointer to the media source.
                    );
                MFError.ThrowExceptionForHR(hr);

                // Get the IMFMediaSource interface from the media source.
                ppMediaSource = (IMFMediaSource)pSourceUnk;
            }
            finally
            {
                SafeRelease(pSourceResolver);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        //  Name: CPlayer::CreateTopology (Private)
        //  Description:
        //      Creates a topology for the media source
        //  Parameter:
        //      pMediaSource: [in] Pointer to the media source
        //      pTopology: [in] Receives the partial topology
        /////////////////////////////////////////////////////////////////////////////////////////

        private void CreateTopology(
                            IMFMediaSource pMediaSource,
                            IMFTopology pTopology)
        {
            Debug.WriteLine("CPlayer::CreateTopology");

            //The caller needs to pass a valid media source
            //We need the media source because to set the source node attribute, media source is needed

            if (pMediaSource == null || pTopology == null)
            {
                throw new COMException("null pointer", E_Pointer);
            }

            IMFPresentationDescriptor pPresentationDescriptor;

            //Create Presentation Descriptor for the media source
            int hr = pMediaSource.CreatePresentationDescriptor(out pPresentationDescriptor);
            MFError.ThrowExceptionForHR(hr);

            try
            {
                CreateNodesForStream(pPresentationDescriptor, pMediaSource, pTopology);
            }
            finally
            {
                SafeRelease(pPresentationDescriptor);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        //  Name: CPlayer::AddTopologyToSequencer (Private)
        //  Description:
        //      Adds the topology to the sequencer
        //  Parameter:
        //      sURL: [in] File URL
        //      pMediaSource: [in] Pointer to the media source
        //      pTopology: [in] Pointer to the topology
        //      pSegmentId: [out] Receives the segment id returned by the sequencer source
        /////////////////////////////////////////////////////////////////////////////////////////

        private void AddTopologyToSequencer(
                            string sURL,
                            IMFMediaSource pMediaSource,
                            IMFTopology pTopology,
                            out int pSegmentId)
        {
            Debug.WriteLine("CPlayer::AddTopologyToSequencer");

            if (sURL == null || pMediaSource == null || pTopology == null)
            {
                throw new COMException("null pointer", E_Pointer);
            }

            long hnsSegmentDuration = 0;
            long TopologyID = 0;
            int hr;

            IMFPresentationDescriptor pPresentationDescriptor;

            hr = m_pSequencerSource.AppendTopology(pTopology, 0, out pSegmentId);
            MFError.ThrowExceptionForHR(hr);

            hr = pTopology.GetTopologyID(out TopologyID);
            MFError.ThrowExceptionForHR(hr);

            //create a presentation descriptor
            hr = pMediaSource.CreatePresentationDescriptor(out pPresentationDescriptor);
            MFError.ThrowExceptionForHR(hr);

            //get the segment duration
            hr = pPresentationDescriptor.GetUINT64(MFAttributesClsid.MF_PD_DURATION, out hnsSegmentDuration);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(hnsSegmentDuration > 0);

            //store the segment info: SegmentId, SegmentDuration, TopoID in the linked list.
            m_Segments.AddNewSegmentEntry(pSegmentId, hnsSegmentDuration, TopologyID, sURL);
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        //  Name: CPlayer::QueueNextSegment (Private)
        //  Description:
        //      Queues the next topology on the session.
        //
        //  Parameter:
        //      pPresentationDescriptor: [in] Presentation descriptor for the next topology
        //      pSegmentId: [out] Receives the corresponding segment identifier for the topology
        //
        //  Note: The presentation descriptor is received from the MENewPresentation event.
        //          This event tells the session about the next topology in the sequencer.
        //          If NULL is passed, this method queues the first topology on the media session.
        /////////////////////////////////////////////////////////////////////////////////////////

        private void QueueNextSegment(
                             IMFPresentationDescriptor pPresentationDescriptor,
                             out int pSegmentId)
        {
            int hr;
            IMFMediaSourceTopologyProvider pMediaSourceTopologyProvider;
            IMFTopology pTopology;

            int SegId = 0;

            // Get the Segment ID.
            hr = m_pSequencerSource.GetPresentationContext(
                pPresentationDescriptor, 
                out SegId,
                out pTopology);
            MFError.ThrowExceptionForHR(hr);

            SafeRelease(pTopology);

            Debug.WriteLine(string.Format("CPlayer::QueueNextSegment: {0}", SegId));

            //Get the topology for the presentation descriptor
            pMediaSourceTopologyProvider = (IMFMediaSourceTopologyProvider)m_pSequencerSource;

            hr = pMediaSourceTopologyProvider.GetMediaSourceTopology(
                                pPresentationDescriptor,
                                out pTopology);
            MFError.ThrowExceptionForHR(hr);

            try
            {
                //Set the topology on the media session
                hr = m_pMediaSession.SetTopology(MFSessionSetTopologyFlags.None, pTopology);
                MFError.ThrowExceptionForHR(hr);

                pSegmentId = SegId;
            }
            finally
            {
                //clean up
                SafeRelease(pTopology);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        //  Name: CPlayer::AddSegment (Private)
        //  Description:
        //      Adds a segment to the sequencer.
        //  Parameter:
        //      sURL: [in]File URL
        //      pSegmentId: [out] receives the segment identifier of the segment returned by AppendTopology
        /////////////////////////////////////////////////////////////////////////////////////////

        private void AddSegment(string sURL, out int pSegmentId)
        {
            Debug.WriteLine("CPlayer::AddSegment");
            Debug.WriteLine(string.Format("URL = {0}", sURL));

            if (sURL == null)
            {
                throw new COMException("null pointer", E_Pointer);
            }

            IMFMediaSource pMediaSource;
            IMFTopology pTopology;

            CreateMediaSource(sURL, out pMediaSource);

            try
            {
                int hr = MFExtern.MFCreateTopology(out pTopology);
                MFError.ThrowExceptionForHR(hr);
                try
                {
                    CreateTopology(pMediaSource, pTopology);

                    AddTopologyToSequencer(sURL, pMediaSource, pTopology, out pSegmentId);
                }
                finally
                {
                    SafeRelease(pTopology);
                }
            }
            finally
            {
                SafeRelease(pMediaSource);
            }
        }

        #endregion
    }
}
