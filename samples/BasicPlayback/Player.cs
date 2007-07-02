/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/

using System;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

using MediaFoundation;
using MediaFoundation.EVR;
using MediaFoundation.Misc;
using MediaFoundation.Utils;

class CPlayer : COMBase, IMFAsyncCallback
{
    #region externs

    [DllImport("user32", CharSet = CharSet.Auto)]
    private extern static int PostMessage(
        IntPtr handle, int msg, int wParam, IntPtr lParam);

    #endregion

    #region Declarations

    const int WM_APP = 0x8000;
    const int WM_APP_ERROR = WM_APP + 2;
    const int WM_APP_NOTIFY = WM_APP + 1;
    const int WAIT_TIMEOUT = 258;

    const int MF_VERSION = 0x10070;

    public enum PlayerState
    {
        Ready = 0,
        OpenPending,
        Started,
        PausePending,
        Paused,
        StartPending,
    }

    #endregion

    #region Public methods

    public int OpenURL(string sURL)
    {
	    TRACE("CPlayer::OpenURL");
	    TRACE("URL = " + sURL);

	    // 1. Create a new media session.
	    // 2. Create the media source.
	    // 3. Create the topology.
	    // 4. Queue the topology [asynchronous]
	    // 5. Start playback [asynchronous - does not happen in this method.]

	    int hr = S_OK;
	    IMFTopology pTopology = null;

	    // Create the media session.
	    hr = CreateSession();

	    // Create the media source.
	    if (SUCCEEDED(hr))
	    {
		    hr = CreateMediaSource(sURL);
	    }

	    // Create a partial topology.
	    if (SUCCEEDED(hr))
	    {
		    hr = CreateTopologyFromSource(out pTopology);
	    }

	    // Set the topology on the media session.
	    if (SUCCEEDED(hr))
	    {
		    hr = m_pSession.SetTopology(0, pTopology);
		    LOG_IF_FAILED("IMFMediaSession::SetTopology", hr);
	    }

	    if (SUCCEEDED(hr))
	    {
		    // Set our state to "open pending"
            m_state = PlayerState.OpenPending;
		    NotifyState();
	    }
	    else
	    {
		    NotifyError(hr);
            m_state = PlayerState.Ready;
	    }

        if (pTopology != null)
        {
            Marshal.ReleaseComObject(pTopology);
        }

	    // If SetTopology succeeded, the media session will queue an 
	    // MESessionTopologySet event.

	    return hr;
    }

    public int Play()
    {
	    TRACE("CPlayer::Pause");

        if (m_state != PlayerState.Paused)
	    {
		    return E_FAIL;
	    }
        if (m_pSession == null || m_pSource == null)
	    {
		    return E_UNEXPECTED;
	    }

        int hr = StartPlayback();

	    if (SUCCEEDED(hr))
	    {
            m_state = PlayerState.StartPending;
		    NotifyState();
	    }
	    else
	    {
		    NotifyError(hr);
	    }

	    return hr;
    }

    public int Pause()
    {
        TRACE("CPlayer::Pause");

        if (m_state != PlayerState.Started)
        {
            return E_FAIL;
        }
        if (m_pSession == null || m_pSource == null)
        {
            return E_UNEXPECTED;
        }

        int hr = m_pSession.Pause();
        LOG_IF_FAILED("IMFMediaSession::Pause", hr);

        if (SUCCEEDED(hr))
        {
            m_state = PlayerState.PausePending;
            NotifyState();
        }
        else
        {
            NotifyError(hr);
        }

        return hr;
    }

    public int Shutdown()
    {
	    TRACE("CPlayer::ShutDown");

	    int hr = S_OK;

        if (m_hCloseEvent != null)
        {
            // Close the session
            hr = CloseSession();

            // Shutdown the Media Foundation platform
            MFDll.MFShutdown();

            m_hCloseEvent.Close();
            m_hCloseEvent = null;
        }

	    return hr;
    }

	// Video functionality
    public int Repaint()
    {
        int hr = S_OK;

        if (m_pVideoDisplay != null)
        {
            hr = m_pVideoDisplay.RepaintVideo();
        }

        return hr;
    }

    public int ResizeVideo(short width, short height)
    {
        int hr = S_OK;
        TRACE(string.Format("ResizeVideo: {0}x{1}", width, height));

        if (m_pVideoDisplay != null)
        {
            RECT rcDest = new RECT();
            MFVideoNormalizedRect nRect = new MFVideoNormalizedRect();

            nRect.left = 0;
            nRect.right = 1;
            nRect.top = 0;
            nRect.bottom = 1;
            rcDest.left = 0;
            rcDest.top = 0;
            rcDest.right = width;
            rcDest.bottom = height;

            hr = m_pVideoDisplay.SetVideoPosition(nRect, rcDest);
        }
        return hr;
    }

	public PlayerState GetState() 
    { 
        return m_state; 
    }

    public bool HasVideo() 
    { 
        return (m_pVideoDisplay != null); 
    }

    #region IMFAsyncCallback Members

    int IMFAsyncCallback.GetParameters(out MFASync pdwFlags, out int pdwQueue)
    {
        pdwFlags = MFASync.None;
        pdwQueue = 0;

        return E_NOTIMPL;
    }

    int IMFAsyncCallback.Invoke(IMFAsyncResult pResult)
    {
        int hr = 0;
        IMFMediaEvent pEvent = null;
        MediaEventType meType = MediaEventType.MEUnknown;  // Event type
        int hrStatus = 0;	        // Event status

        MF_TopoStatus TopoStatus = MF_TopoStatus.Invalid; // Used with MESessionTopologyStatus event.    

        // Get the event from the event queue.
        hr = m_pSession.EndGetEvent(pResult, out pEvent);
        LOG_IF_FAILED("IMediaEventGenerator::EndGetEvent", hr);

        // Get the event type.
        if (SUCCEEDED(hr))
        {
            hr = pEvent.GetType(out meType);
            LOG_IF_FAILED("IMFMediaEvent::GetType", hr);
        }

        // Get the event status. If the operation that triggered the event did
        // not succeed, the status is a failure code.
        if (SUCCEEDED(hr))
        {
            hr = pEvent.GetStatus(out hrStatus);
            LOG_IF_FAILED("IMFMediaEvent::GetStatus", hr);
        }

        if (SUCCEEDED(hr))
        {
            TRACE(string.Format("Media event: "+ meType.ToString()));

            // Check if the async operation succeeded.
            if (SUCCEEDED(hrStatus))
            {
                // Switch on the event type. Update the internal state of the CPlayer as needed.
                switch (meType)
                {
                    case MediaEventType.MESessionTopologyStatus:
                        // Get the status code.
                        int i;
                        hr = pEvent.GetUINT32(MFAttributesClsid.MF_EVENT_TOPOLOGY_STATUS, out i);
                        TopoStatus = (MF_TopoStatus)i;
                        if (SUCCEEDED(hr))
                        {
                            switch (TopoStatus)
                            {
                                case MF_TopoStatus.Ready:
                                    hr = OnTopologyReady(pEvent);
                                    break;
                                default:
                                    // Nothing to do.
                                    break;
                            }
                        }
                        break;

                    case MediaEventType.MESessionStarted:
                        hr = OnSessionStarted(pEvent);
                        break;

                    case MediaEventType.MESessionPaused:
                        hr = OnSessionPaused(pEvent);
                        break;

                    case MediaEventType.MESessionClosed:
                        hr = OnSessionClosed(pEvent);
                        break;

                    case MediaEventType.MEEndOfPresentation:
                        hr = OnPresentationEnded(pEvent);
                        break;
                }

            }
            else
            {
                // The async operation failed. Notify the application
                NotifyError(hrStatus);
            }
        }

	    // Request another event.
        if (meType != MediaEventType.MESessionClosed)
	    {
		    hr = m_pSession.BeginGetEvent(this, null);
		    LOG_IF_FAILED("IMFMediaSession::BeginGetEvent", hr);
	    }

        if (pEvent != null)
        {
            Marshal.ReleaseComObject(pEvent);
        }

        return hr;
    }

    #endregion

    public CPlayer(IntPtr hVideo, IntPtr hEvent)
    {
        TRACE(("CPlayer::CPlayer"));

        Debug.Assert(hVideo != IntPtr.Zero);
        Debug.Assert(hEvent != IntPtr.Zero);

        m_pSession = null;
	    m_pSource = null;
	    m_pVideoDisplay = null;
	    m_hwndVideo = hVideo;
	    m_hwndEvent = hEvent;
        m_state = PlayerState.Ready;

        m_hCloseEvent = new AutoResetEvent(false);

        int hr = MFDll.MFStartup(0x10070, MFStartup.Full);
        LOG_IF_FAILED("MFStartup", hr);
        if (hr < 0)
        {
            throw new COMException("MFStartup", hr);
        }

    }

    // Destructor is private. Caller should call Release.
    ~CPlayer()
    {
        Debug.Assert(m_pSession == null);  // If FALSE, the app did not call Shutdown().
    }

    #endregion

    #region Protected methods

    // NotifyState: Notifies the application when the state changes.
	protected void NotifyState()
	{
		PostMessage(m_hwndEvent, WM_APP_NOTIFY, (int)m_state, IntPtr.Zero);
	}

	// NotifyState: Notifies the application when an error occurs.
    protected void NotifyError(int hr)
	{
		TRACE("NotifyError: " + hr.ToString());
        m_state = PlayerState.Ready;
		PostMessage(m_hwndEvent, WM_APP_ERROR, hr, IntPtr.Zero);
	}

    protected int CreateSession()
    {
	    // Close the old session, if any.
        int hr = CloseSession();

	    // Create the media session.
	    if (SUCCEEDED(hr))
	    {
            hr = MFDll.MFCreateMediaSession(null, out m_pSession);
    	    LOG_IF_FAILED("MFCreateMediaSession", hr);
	    }

	    // Start pulling events from the media session
	    if (SUCCEEDED(hr))
	    {
            hr = m_pSession.BeginGetEvent(this, null);
	    }

	    return hr;
    }

    protected int CloseSession()
    {
	    int hr = S_OK;

        if (m_pVideoDisplay != null)
        {
            Marshal.ReleaseComObject(m_pVideoDisplay);
            m_pVideoDisplay = null;
        }

        if (m_pSession != null)
        {
		    hr = m_pSession.Close();
		    LOG_IF_FAILED("IMFMediaSession::Close", hr);

		    // Wait for the close operation to complete
		    if (SUCCEEDED(hr))
		    {
			    bool res = m_hCloseEvent.WaitOne(5000, true);
			    if (!res)
			    {
				    TRACE(("WaitForSingleObject timed out!"));
			    }
		    }
        }

	    // Complete shutdown operations

	    // 1. Shut down the media source
	    if (m_pSource != null)
	    {
		    m_pSource.Shutdown();
            SAFE_RELEASE(m_pSource);
            m_pSource = null;
        }

	    // 2. Shut down the media session. (Synchronous operation, no events.)
	    if (m_pSession != null)
	    {
		    m_pSession.Shutdown();
            Marshal.ReleaseComObject(m_pSession);
            m_pSession = null;
	    }

	    return hr;
    }

    protected int StartPlayback()
    {
	    TRACE("CPlayer::StartPlayback");

        Debug.Assert(m_pSession != null);

        int hr = S_OK;

	    hr = m_pSession.Start(Guid.Empty, null);
	    LOG_IF_FAILED("IMFMediaSession::Start", hr);

        return hr;
    }

    protected int CreateMediaSource(string sURL)
    {
        TRACE("CPlayer::CreateMediaSource");

        int hr = S_OK;

        IMFSourceResolver pSourceResolver = null;
        object pSource = null;

	    // Create the source resolver.
	    if (SUCCEEDED(hr))
	    {
            hr = MFDll.MFCreateSourceResolver(out pSourceResolver);
	        LOG_IF_FAILED("MFCreateSourceResolver", hr);
        }

	    // Use the source resolver to create the media source.
	    if (SUCCEEDED(hr))
	    {
            MF_ObjectType ObjectType = MF_ObjectType.Invalid;

            hr = pSourceResolver.CreateObjectFromURL(
				    sURL,						// URL of the source.
                    MF_Resolution.MediaSource,	// Create a source object.
                    null,						// Optional property store.
				    out ObjectType,				// Receives the created object type. 
				    out pSource					// Receives a pointer to the media source.
			    );

            LOG_IF_FAILED("IMFSourceResolver::CreateObjectFromURL", hr);
	    }

	    // Get the IMFMediaSource interface from the media source.
	    if (SUCCEEDED(hr))
	    {
            m_pSource = pSource as IMFMediaSource;
	    }

	    // Clean up
        if (pSourceResolver != null)
        {
            Marshal.ReleaseComObject(pSourceResolver);
        }

        return hr;
    }

    protected int CreateTopologyFromSource(out IMFTopology ppTopology)
    {
	    TRACE("CPlayer::CreateTopologyFromSource");

        Debug.Assert(m_pSession != null);
        Debug.Assert(m_pSource != null);

        int hr = S_OK;

        IMFTopology pTopology = null;
        IMFPresentationDescriptor pSourcePD = null;
	    int cSourceStreams = 0;

	    // Create a new topology.
        hr = MFDll.MFCreateTopology(out pTopology);
	    LOG_IF_FAILED("MFCreateTopology", hr);

	    // Create the presentation descriptor for the media source.
	    if (SUCCEEDED(hr))
	    {
		    hr = m_pSource.CreatePresentationDescriptor(out pSourcePD);
		    LOG_IF_FAILED("IMFMediaSource::CreatePresentationDescriptor", hr);
	    }

	    // Get the number of streams in the media source.
	    if (SUCCEEDED(hr))
	    {
		    hr = pSourcePD.GetStreamDescriptorCount(out cSourceStreams);
		    LOG_IF_FAILED("IMFPresentationDescriptor::GetStreamDescriptorCount", hr);
	    }

	    TRACE(string.Format("Stream count: {0}", cSourceStreams));

	    // For each stream, create the topology nodes and add them to the topology.
	    if (SUCCEEDED(hr))
	    {
		    for (int i = 0; i < cSourceStreams; i++)
		    {
			    hr = AddBranchToPartialTopology(pTopology, pSourcePD, i);
			    if (FAILED(hr))
			    {
				    break;
			    }
		    }
	    }

	    // Return the IMFTopology pointer to the caller.
        if (SUCCEEDED(hr))
        {
            ppTopology = pTopology;
        }
        else
        {
            ppTopology = null;
            if (pTopology != null)
            {
                Marshal.ReleaseComObject(pTopology);
            }
        }

        if (pSourcePD != null)
        {
            Marshal.ReleaseComObject(pSourcePD);
        }

	    return hr;
    }

    protected int AddBranchToPartialTopology(
        IMFTopology pTopology,
        IMFPresentationDescriptor pSourcePD,
        int iStream
        )
    {
	    TRACE("CPlayer::AddBranchToPartialTopology");

        Debug.Assert(pTopology != null);

        IMFStreamDescriptor pSourceSD = null;
        IMFTopologyNode pSourceNode = null;
        IMFTopologyNode pOutputNode = null;
        bool fSelected = false;

        int hr = S_OK;

	    // Get the stream descriptor for this stream.
        hr = pSourcePD.GetStreamDescriptorByIndex(iStream, out fSelected, out pSourceSD);
	    LOG_IF_FAILED("IMFPresentationDescriptor::GetStreamDescriptorByIndex", hr);

        if (SUCCEEDED(hr))
        {
            // Create the topology branch only if the stream is selected.
            // Otherwise, do nothing.
            if (fSelected)
            {
                // Create a source node for this stream.
                hr = CreateSourceStreamNode(pSourcePD, pSourceSD, out pSourceNode);

                // Create the output node for the renderer.
                if (SUCCEEDED(hr))
                {
                    hr = CreateOutputNode(pSourceSD, out pOutputNode);
                }

                // Add both nodes to the topology.
                if (SUCCEEDED(hr))
                {
                    hr = pTopology.AddNode(pSourceNode);
                    LOG_IF_FAILED("IMFTopology::AddNode", hr);
                }

                if (SUCCEEDED(hr))
                {
                    hr = pTopology.AddNode(pOutputNode);
                    LOG_IF_FAILED("IMFTopology::AddNode", hr);
                }

                // Connect the source node to the output node.
                if (SUCCEEDED(hr))
                {
                    hr = pSourceNode.ConnectOutput(0, pOutputNode, 0);
                    LOG_IF_FAILED("IMFTopologyNode::ConnectOutput", hr);
                }
            }
        }

	    // Clean up.
        if (pSourceSD != null)
        {
            Marshal.ReleaseComObject(pSourceSD);
        }
        if (pSourceNode != null)
        {
            Marshal.ReleaseComObject(pSourceNode);
        }
        if (pOutputNode != null)
        {
            Marshal.ReleaseComObject(pOutputNode);
        }

	    return hr;

    }

    protected int CreateSourceStreamNode(
        IMFPresentationDescriptor pSourcePD,
        IMFStreamDescriptor pSourceSD,
        out IMFTopologyNode ppNode
        )
    {
        Debug.Assert(m_pSource != null);

        IMFTopologyNode pNode = null;
        int hr = S_OK;

	    // Create the source-stream node. 
        hr = MFDll.MFCreateTopologyNode(MF_TopologyType.SourcestreamNode, out pNode);
	    LOG_IF_FAILED("MFCreateTopologyNode", hr);

	    // Set attribute: Pointer to the media source.
	    if (SUCCEEDED(hr))
	    {
            hr = pNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_SOURCE, m_pSource);
	    }

	    // Set attribute: Pointer to the presentation descriptor.
	    if (SUCCEEDED(hr))
	    {
            hr = pNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_PRESENTATION_DESCRIPTOR, pSourcePD);
	    }

	    // Set attribute: Pointer to the stream descriptor.
	    if (SUCCEEDED(hr))
	    {
            hr = pNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_STREAM_DESCRIPTOR, pSourceSD);
	    }

	    // Return the IMFTopologyNode pointer to the caller.
        if (SUCCEEDED(hr))
        {
            ppNode = pNode;
        }
        else
        {
            ppNode = null;
            if (pNode != null)
            {
                Marshal.ReleaseComObject(pNode);
            }
        }

	    return hr;
    }

    protected int CreateOutputNode(
        IMFStreamDescriptor pSourceSD,
        out IMFTopologyNode ppNode
        )
    {
        IMFTopologyNode pNode = null;
        IMFMediaTypeHandler pHandler = null;
        IMFActivate pRendererActivate = null;

	    Guid guidMajorType = Guid.Empty;
        int hr = S_OK;

	    // Get the stream ID.
	    int streamID = 0;
	    pSourceSD.GetStreamIdentifier(out streamID); // Just for debugging, ignore any failures.
	    LOG_IF_FAILED("IMFStreamDescriptor::GetStreamIdentifier", hr);

	    // Get the media type handler for the stream.
	    hr = pSourceSD.GetMediaTypeHandler(out pHandler);
	    LOG_IF_FAILED("IMFStreamDescriptor::GetMediaTypeHandler", hr);
    	
	    // Get the major media type.
	    if (SUCCEEDED(hr))
	    {
		    hr = pHandler.GetMajorType(out guidMajorType);
		    LOG_IF_FAILED("IMFMediaTypeHandler::GetMajorType", hr);
	    }

	    // Create a downstream node.
	    if (SUCCEEDED(hr))
	    {
            hr = MFDll.MFCreateTopologyNode(MF_TopologyType.OutputNode, out pNode);
		    LOG_IF_FAILED("MFCreateTopologyNode", hr);
	    }

	    // Create an IMFActivate object for the renderer, based on the media type.
	    if (SUCCEEDED(hr))
	    {
            if (CLSID.MFMediaType_Audio == guidMajorType)
		    {
			    // Create the audio renderer.
			    TRACE(string.Format("Stream {0}: audio stream", streamID));
                hr = MFDll.MFCreateAudioRendererActivate(out pRendererActivate);
			    LOG_IF_FAILED("MFCreateAudioRendererActivate", hr);
		    }
            else if (CLSID.MFMediaType_Video == guidMajorType)
		    {
			    // Create the video renderer.
                TRACE(string.Format("Stream {0}: video stream", streamID));
                hr = MFDll.MFCreateVideoRendererActivate(m_hwndVideo, out pRendererActivate);
			    LOG_IF_FAILED("MFCreateVideoRendererActivate", hr);
		    }
		    else
		    {
                TRACE(string.Format("Stream {0}: Unknown format", streamID));
			    hr = E_FAIL;
		    }
	    }

	    // Set the IActivate object on the output node.
	    if (SUCCEEDED(hr))
	    {
		    hr = pNode.SetObject(pRendererActivate);
		    LOG_IF_FAILED("IMFTopologyNode::SetObject", hr);
	    }

	    // Return the IMFTopologyNode pointer to the caller.
        if (SUCCEEDED(hr))
        {
            ppNode = pNode;
        }
        else
        {
            if (pNode != null)
            {
                Marshal.ReleaseComObject(pNode);
            }
            ppNode = null;
        }

	    // Clean up.
        if (pHandler != null)
        {
            Marshal.ReleaseComObject(pHandler);
        }
        if (pRendererActivate != null)
        {
            Marshal.ReleaseComObject(pRendererActivate);
        }

	    return hr;
    }

	// Media event handlers
    protected int OnTopologyReady(IMFMediaEvent pEvent)
    {
        object o;
        TRACE("CPlayer::OnTopologyReady");

	    // Ask for the IMFVideoDisplayControl interface.
	    // This interface is implemented by the EVR and is
	    // exposed by the media session as a service.

	    // Note: This call is expected to fail if the source
	    // does not have video.

        MFDll.MFGetService(
		    m_pSession,
            MFServices.MR_VIDEO_RENDER_SERVICE,
		    typeof(IMFVideoDisplayControl).GUID,
		    out o
		    );

        m_pVideoDisplay = o as IMFVideoDisplayControl;

        int hr = StartPlayback();
	    if (FAILED(hr))
	    {
		    NotifyError(hr);
	    }

	    // If we succeeded, the Start call is pending. Don't notify the app yet.

	    return S_OK;
    }

    protected int OnSessionStarted(IMFMediaEvent pEvent)
    {
	    TRACE("CPlayer::OnSessionStarted");

        m_state = PlayerState.Started;
	    NotifyState();

        return S_OK;
    }

    protected int OnSessionPaused(IMFMediaEvent pEvent)
    {
	    TRACE("CPlayer::OnSessionPaused");

        m_state = PlayerState.Paused;
	    NotifyState();

        return S_OK;
    }

    protected int OnSessionClosed(IMFMediaEvent pEvent)
    {
	    TRACE("CPlayer::OnSessionClosed");

	    // The application thread is waiting on this event, inside the 
	    // CPlayer::CloseSession method. 
        m_hCloseEvent.Set();
	    return S_OK;
    }

    protected int OnPresentationEnded(IMFMediaEvent pEvent)
    {
	    TRACE("CPlayer::OnPresentationEnded");

        // The session puts itself into the stopped state autmoatically.

        m_state = PlayerState.Ready;
	    NotifyState();

	    return S_OK;
    }

    #endregion

    #region Private Methods

    private static void TRACE(string s)
    {
        Debug.WriteLine(s);
    }

    private void LOG_IF_FAILED(string s, int hr)
    {
        if (FAILED(hr))
        {
            TRACE(s);
        }
    }

    #endregion

	protected IMFMediaSession			m_pSession;
	protected IMFMediaSource			m_pSource;
	protected IMFVideoDisplayControl	m_pVideoDisplay;

    protected IntPtr m_hwndVideo;		// Video window.
    protected IntPtr m_hwndEvent;		// App window to receive events.
    protected PlayerState m_state;			// Current state of the media session.
    protected AutoResetEvent m_hCloseEvent;		// Event to wait on while closing
};