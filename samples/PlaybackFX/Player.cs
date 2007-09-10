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

class CPlayer : COMBase, IMFAsyncCallback
{
    #region externs

    [DllImport("user32", CharSet = CharSet.Auto)]
    private extern static int PostMessage(
        IntPtr handle, int msg, IntPtr wParam, IntPtr lParam);

    #endregion

    #region Declarations

    const int WM_APP = 0x8000;
    const int WM_APP_ERROR = WM_APP + 2;
    const int WM_APP_NOTIFY = WM_APP + 1;
    const int WAIT_TIMEOUT = 258;

    // Pick the video effect to use.  This Guid is the grayscale mft.  That
    // mft needs to be compiled and registered before this will work.
    Guid m_VideoEffect = new Guid("{69042198-8146-4735-90F0-BEFD5BFAEDB7}");

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

        MFExtern.MFStartup(0x10070, MFStartup.Full);
    }

    // Destructor is private. Caller should call Release.
    ~CPlayer()
    {
        Debug.Assert(m_pSession == null);  // If FALSE, the app did not call Shutdown().
    }

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

        int hr = S_Ok;
        try
        {
            IMFTopology pTopology = null;

            // Create the media session.
            CreateSession();

            // Create the media source.
            CreateMediaSource(sURL);

            // Create a partial topology.
            CreateTopologyFromSource(out pTopology);

            // Set the topology on the media session.
            m_pSession.SetTopology(0, pTopology);

            // Set our state to "open pending"
            m_state = PlayerState.OpenPending;
            NotifyState();

            SafeRelease(pTopology);

            // If SetTopology succeeded, the media session will queue an
            // MESessionTopologySet event.
        }
        catch (Exception ce)
        {
            hr = Marshal.GetHRForException(ce);
            NotifyError(hr);
            m_state = PlayerState.Ready;
        }

        return hr;
    }

    public int Play()
    {
        TRACE("CPlayer::Play");

        if (m_state != PlayerState.Paused)
        {
            return E_Fail;
        }
        if (m_pSession == null || m_pSource == null)
        {
            return E_Unexpected;
        }

        int hr = S_Ok;

        try
        {
            StartPlayback();

            m_state = PlayerState.StartPending;
            NotifyState();
        }
        catch (Exception ce)
        {
            hr = Marshal.GetHRForException(ce);
            NotifyError(hr);
        }

        return hr;
    }

    public int Pause()
    {
        TRACE("CPlayer::Pause");

        if (m_state != PlayerState.Started)
        {
            return E_Fail;
        }
        if (m_pSession == null || m_pSource == null)
        {
            return E_Unexpected;
        }

        int hr = S_Ok;

        try
        {
            m_pSession.Pause();

            m_state = PlayerState.PausePending;
            NotifyState();
        }
        catch (Exception ce)
        {
            hr = Marshal.GetHRForException(ce);
            NotifyError(hr);
        }

        return hr;
    }

    public int Shutdown()
    {
        TRACE("CPlayer::ShutDown");

        int hr = S_Ok;

        try
        {
            if (m_hCloseEvent != null)
            {
                // Close the session
                CloseSession();

                // Shutdown the Media Foundation platform
                MFExtern.MFShutdown();

                m_hCloseEvent.Close();
                m_hCloseEvent = null;
            }
        }
        catch (Exception ce)
        {
            hr = Marshal.GetHRForException(ce);
        }

        return hr;
    }

    // Video functionality
    public int Repaint()
    {
        int hr = S_Ok;

        if (m_pVideoDisplay != null)
        {
            try
            {
                m_pVideoDisplay.RepaintVideo();
            }
            catch (Exception ce)
            {
                hr = Marshal.GetHRForException(ce);
            }
        }

        return hr;
    }

    public int ResizeVideo(short width, short height)
    {
        int hr = S_Ok;
        TRACE(string.Format("ResizeVideo: {0}x{1}", width, height));

        if (m_pVideoDisplay != null)
        {
            try
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

                m_pVideoDisplay.SetVideoPosition(nRect, rcDest);
            }
            catch (Exception ce)
            {
                hr = Marshal.GetHRForException(ce);
            }
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

    #endregion

    #region IMFAsyncCallback Members

    void IMFAsyncCallback.GetParameters(out MFASync pdwFlags, out MFAsyncCallbackQueue pdwQueue)
    {
        pdwFlags = MFASync.FastIOProcessingCallback;
        pdwQueue = MFAsyncCallbackQueue.Standard;
        //throw new COMException("IMFAsyncCallback.GetParameters not implemented in Player", E_NotImplemented);
    }

    void IMFAsyncCallback.Invoke(IMFAsyncResult pResult)
    {
        IMFMediaEvent pEvent = null;
        MediaEventType meType = MediaEventType.MEUnknown;  // Event type
        int hrStatus = 0;           // Event status
        MFTopoStatus TopoStatus = MFTopoStatus.Invalid; // Used with MESessionTopologyStatus event.

        try
        {
            // Get the event from the event queue.
            m_pSession.EndGetEvent(pResult, out pEvent);

            // Get the event type.
            pEvent.GetType(out meType);

            // Get the event status. If the operation that triggered the event did
            // not succeed, the status is a failure code.
            pEvent.GetStatus(out hrStatus);

            TRACE(string.Format("Media event: " + meType.ToString()));

            // Check if the async operation succeeded.
            if (Succeeded(hrStatus))
            {
                // Switch on the event type. Update the internal state of the CPlayer as needed.
                switch (meType)
                {
                    case MediaEventType.MESessionTopologyStatus:
                        // Get the status code.
                        int i;
                        pEvent.GetUINT32(MFAttributesClsid.MF_EVENT_TOPOLOGY_STATUS, out i);
                        TopoStatus = (MFTopoStatus)i;
                        switch (TopoStatus)
                        {
                            case MFTopoStatus.Ready:
                                OnTopologyReady(pEvent);
                                break;
                            default:
                                // Nothing to do.
                                break;
                        }
                        break;

                    case MediaEventType.MESessionStarted:
                        OnSessionStarted(pEvent);
                        break;

                    case MediaEventType.MESessionPaused:
                        OnSessionPaused(pEvent);
                        break;

                    case MediaEventType.MESessionClosed:
                        OnSessionClosed(pEvent);
                        break;

                    case MediaEventType.MEEndOfPresentation:
                        OnPresentationEnded(pEvent);
                        break;
                }
            }
            else
            {
                // The async operation failed. Notify the application
                NotifyError(hrStatus);
            }
        }
        finally
        {
            // Request another event.
            if (meType != MediaEventType.MESessionClosed)
            {
                m_pSession.BeginGetEvent(this, null);
            }

            SafeRelease(pEvent);
        }
    }

    #endregion

    #region Protected methods

    // NotifyState: Notifies the application when the state changes.
    protected void NotifyState()
    {
        PostMessage(m_hwndEvent, WM_APP_NOTIFY, new IntPtr((int)m_state), IntPtr.Zero);
    }

    // NotifyState: Notifies the application when an error occurs.
    protected void NotifyError(int hr)
    {
        TRACE("NotifyError: 0x" + hr.ToString("X"));
        m_state = PlayerState.Ready;
        PostMessage(m_hwndEvent, WM_APP_ERROR, new IntPtr(hr), IntPtr.Zero);
    }

    protected void CreateSession()
    {
        // Close the old session, if any.
        CloseSession();

        // Create the media session.
        MFExtern.MFCreateMediaSession(null, out m_pSession);

        // Start pulling events from the media session
        m_pSession.BeginGetEvent(this, null);
    }

    protected void CloseSession()
    {
        if (m_pVideoDisplay != null)
        {
            Marshal.ReleaseComObject(m_pVideoDisplay);
            m_pVideoDisplay = null;
        }

        if (m_pSession != null)
        {
            m_pSession.Close();

            // Wait for the close operation to complete
            bool res = m_hCloseEvent.WaitOne(5000, true);
            if (!res)
            {
                TRACE(("WaitForSingleObject timed out!"));
            }
        }

        // Complete shutdown operations

        // 1. Shut down the media source
        if (m_pSource != null)
        {
            m_pSource.Shutdown();
            SafeRelease(m_pSource);
            m_pSource = null;
        }

        // 2. Shut down the media session. (Synchronous operation, no events.)
        if (m_pSession != null)
        {
            m_pSession.Shutdown();
            Marshal.ReleaseComObject(m_pSession);
            m_pSession = null;
        }
    }

    protected void StartPlayback()
    {
        TRACE("CPlayer::StartPlayback");

        Debug.Assert(m_pSession != null);

        m_pSession.Start(Guid.Empty, new PropVariant());
    }

    protected void CreateMediaSource(string sURL)
    {
        TRACE("CPlayer::CreateMediaSource");

        IMFSourceResolver pSourceResolver;
        object pSource;

        // Create the source resolver.
        MFExtern.MFCreateSourceResolver(out pSourceResolver);

        try
        {
            // Use the source resolver to create the media source.
            MFObjectType ObjectType = MFObjectType.Invalid;

            pSourceResolver.CreateObjectFromURL(
                    sURL,                       // URL of the source.
                    MFResolution.MediaSource,   // Create a source object.
                    null,                       // Optional property store.
                    out ObjectType,             // Receives the created object type.
                    out pSource                 // Receives a pointer to the media source.
                );

            // Get the IMFMediaSource interface from the media source.
            m_pSource = (IMFMediaSource)pSource;
        }
        finally
        {
            // Clean up
            Marshal.ReleaseComObject(pSourceResolver);
        }
    }

    protected void CreateTopologyFromSource(out IMFTopology ppTopology)
    {
        TRACE("CPlayer::CreateTopologyFromSource");

        Debug.Assert(m_pSession != null);
        Debug.Assert(m_pSource != null);

        IMFTopology pTopology = null;
        IMFPresentationDescriptor pSourcePD = null;
        int cSourceStreams = 0;

        try
        {
            // Create a new topology.
            MFExtern.MFCreateTopology(out pTopology);

            // Create the presentation descriptor for the media source.
            m_pSource.CreatePresentationDescriptor(out pSourcePD);

            // Get the number of streams in the media source.
            pSourcePD.GetStreamDescriptorCount(out cSourceStreams);

            TRACE(string.Format("Stream count: {0}", cSourceStreams));

            // For each stream, create the topology nodes and add them to the topology.
            for (int i = 0; i < cSourceStreams; i++)
            {
                AddBranchToPartialTopology(pTopology, m_hwndVideo, m_pSource, pSourcePD, i);
            }

            // Return the IMFTopology pointer to the caller.
            ppTopology = pTopology;
        }
        catch
        {
            // If we failed, release the topology
            SafeRelease(pTopology);
            throw;
        }
        finally
        {
            SafeRelease(pSourcePD);
        }
    }

    protected void CreateSourceStreamNode(
        IMFPresentationDescriptor pSourcePD,
        IMFStreamDescriptor pSourceSD,
        out IMFTopologyNode ppNode
        )
    {
        Debug.Assert(m_pSource != null);

        IMFTopologyNode pNode = null;

        try
        {
            // Create the source-stream node.
            MFExtern.MFCreateTopologyNode(MFTopologyType.SourcestreamNode, out pNode);

            // Set attribute: Pointer to the media source.
            pNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_SOURCE, m_pSource);

            // Set attribute: Pointer to the presentation descriptor.
            pNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_PRESENTATION_DESCRIPTOR, pSourcePD);

            // Set attribute: Pointer to the stream descriptor.
            pNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_STREAM_DESCRIPTOR, pSourceSD);

            // Return the IMFTopologyNode pointer to the caller.
            ppNode = pNode;
        }
        catch
        {
            // If we failed, release the pnode
            SafeRelease(pNode);
            throw;
        }
    }

    protected void CreateOutputNode(
        IMFStreamDescriptor pSourceSD,
        out IMFTopologyNode ppNode
        )
    {
        IMFTopologyNode pNode = null;
        IMFMediaTypeHandler pHandler = null;
        IMFActivate pRendererActivate = null;

        Guid guidMajorType = Guid.Empty;
        int hr = S_Ok;

        // Get the stream ID.
        int streamID = 0;

        try
        {
            try
            {
                pSourceSD.GetStreamIdentifier(out streamID); // Just for debugging, ignore any failures.
            }
            catch
            {
                TRACE("IMFStreamDescriptor::GetStreamIdentifier" + hr.ToString());
            }

            // Get the media type handler for the stream.
            pSourceSD.GetMediaTypeHandler(out pHandler);

            // Get the major media type.
            pHandler.GetMajorType(out guidMajorType);

            // Create a downstream node.
            MFExtern.MFCreateTopologyNode(MFTopologyType.OutputNode, out pNode);

            // Create an IMFActivate object for the renderer, based on the media type.
            if (MFMediaType.Audio == guidMajorType)
            {
                // Create the audio renderer.
                TRACE(string.Format("Stream {0}: audio stream", streamID));
                MFExtern.MFCreateAudioRendererActivate(out pRendererActivate);
            }
            else if (MFMediaType.Video == guidMajorType)
            {
                // Create the video renderer.
                TRACE(string.Format("Stream {0}: video stream", streamID));
                MFExtern.MFCreateVideoRendererActivate(m_hwndVideo, out pRendererActivate);
            }
            else
            {
                TRACE(string.Format("Stream {0}: Unknown format", streamID));
                throw new COMException("Unknown format", E_Fail);
            }

            // Set the IActivate object on the output node.
            pNode.SetObject(pRendererActivate);

            // Return the IMFTopologyNode pointer to the caller.
            ppNode = pNode;
        }
        catch
        {
            // If we failed, release the pNode
            SafeRelease(pNode);
            throw;
        }
        finally
        {
            // Clean up.
            SafeRelease(pHandler);
            SafeRelease(pRendererActivate);
        }
    }

    // Media event handlers
    protected void OnTopologyReady(IMFMediaEvent pEvent)
    {
        object o;
        TRACE("CPlayer::OnTopologyReady");

        // Ask for the IMFVideoDisplayControl interface.
        // This interface is implemented by the EVR and is
        // exposed by the media session as a service.

        // Note: This call is expected to fail if the source
        // does not have video.

        try
        {
            MFExtern.MFGetService(
                m_pSession,
                MFServices.MR_VIDEO_RENDER_SERVICE,
                typeof(IMFVideoDisplayControl).GUID,
                out o
                );

            m_pVideoDisplay = o as IMFVideoDisplayControl;
        }
        catch (InvalidCastException)
        {
            m_pVideoDisplay = null;
        }

        try
        {
            StartPlayback();
        }
        catch (Exception ce)
        {
            int hr = Marshal.GetHRForException(ce);
            NotifyError(hr);
        }

        // If we succeeded, the Start call is pending. Don't notify the app yet.
    }

    protected void OnSessionStarted(IMFMediaEvent pEvent)
    {
        TRACE("CPlayer::OnSessionStarted");

        m_state = PlayerState.Started;
        NotifyState();
    }

    protected void OnSessionPaused(IMFMediaEvent pEvent)
    {
        TRACE("CPlayer::OnSessionPaused");

        m_state = PlayerState.Paused;
        NotifyState();
    }

    protected void OnSessionClosed(IMFMediaEvent pEvent)
    {
        TRACE("CPlayer::OnSessionClosed");

        // The application thread is waiting on this event, inside the
        // CPlayer::CloseSession method.
        m_hCloseEvent.Set();
    }

    protected void OnPresentationEnded(IMFMediaEvent pEvent)
    {
        TRACE("CPlayer::OnPresentationEnded");

        // The session puts itself into the stopped state autmoatically.

        m_state = PlayerState.Ready;
        NotifyState();
    }

    #endregion

    #region Private Methods

    ///////////////////////////////////////////////////////////////////////
    //  Name:  AddBranchToPartialTopology
    //  Description:  Adds a topology branch for one stream.
    //
    //  pTopology: Pointer to the topology object.
    //  hVideoWindow: Handle to the video window (for video streams).
    //  pSource: Media source.
    //  pSourcePD: The source's presentation descriptor.
    //  iStream: Index of the stream to render.
    //
    //  Pre-conditions: The topology must be created already.
    //
    //  Notes: For each stream, we must do the following:
    //    1. Create a source node associated with the stream. 
    //    2. Create an output node for the renderer. 
    //    3. Connect the two nodes.
    //
    //  Optionally we can also add an effect transform between the source
    //  and output nodes.
    //
    //  The media session will resolve the topology, so we do not have
    //  to worry about decoders or color converters.
    /////////////////////////////////////////////////////////////////////////

    void AddBranchToPartialTopology(
        IMFTopology pTopology,
        IntPtr hVideoWindow,
        IMFMediaSource pSource,
        IMFPresentationDescriptor pSourcePD,
        int iStream)
    {
        TRACE("Player::RenderStream");

        IMFStreamDescriptor pSourceSD = null;
        IMFTopologyNode pSourceNode = null;

        Guid majorType;
        bool fSelected = false;

        // Get the stream descriptor for this stream.
        pSourcePD.GetStreamDescriptorByIndex(iStream, out fSelected, out pSourceSD);

        // First check if the stream is selected by default. If not, ignore it.
        // More sophisticated applications can change the default selections.
        if (fSelected)
        {
            try
            {
                // Create a source node for this stream.
                CreateSourceStreamNode(pSource, pSourcePD, pSourceSD, out pSourceNode);

                // Add the source node to the topology.
                pTopology.AddNode(pSourceNode);

                // Get the major media type for the stream.
                GetStreamType(pSourceSD, out majorType);

                if (majorType == MFMediaType.Video)
                {
                    // For video, use the grayscale transform.
                    CreateVideoBranch(pTopology, pSourceNode, hVideoWindow, m_VideoEffect);
                }
                else if (majorType == MFMediaType.Audio)
                {
                    CreateAudioBranch(pTopology, pSourceNode, Guid.Empty);
                }
            }
            finally
            {
                // Clean up.
                SafeRelease(pSourceSD);
                SafeRelease(pSourceNode);
            }
        }
    }



    ///////////////////////////////////////////////////////////////////////
    //  Name: CreateSourceStreamNode
    //  Description:  Creates a source-stream node for a stream.
    // 
    //  pSource: Media source.
    //  pSourcePD: Presentation descriptor for the media source.
    //  pSourceSD: Stream descriptor for the stream.
    //  ppNode: Receives a pointer to the new node.
    //
    //  Pre-conditions: Create the media source.
    /////////////////////////////////////////////////////////////////////////

    void CreateSourceStreamNode(
        IMFMediaSource pSource,
        IMFPresentationDescriptor pSourcePD,
        IMFStreamDescriptor pSourceSD,
        out IMFTopologyNode ppNode
        )
    {
        // Create the source-stream node. 
        MFExtern.MFCreateTopologyNode(MFTopologyType.SourcestreamNode, out ppNode);

        // Set attribute: Pointer to the media source.
        ppNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_SOURCE, pSource);

        // Set attribute: Pointer to the presentation descriptor.
        ppNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_PRESENTATION_DESCRIPTOR, pSourcePD);

        // Set attribute: Pointer to the stream descriptor.
        ppNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_STREAM_DESCRIPTOR, pSourceSD);
    }




    ///////////////////////////////////////////////////////////////////////
    //  Name: CreateVideoBranch
    //  Description:  
    //  Adds and connects the nodes downstream from a video source node.
    //
    //  pTopology:      Pointer to the topology.
    //  pSourceNode:    Pointer to the source node.
    //  hVideoWindow:   Handle to the video window.
    //  clsidTransform: CLSID of an effect transform. 
    /////////////////////////////////////////////////////////////////////////

    void CreateVideoBranch(
        IMFTopology pTopology,
        IMFTopologyNode pSourceNode,
        IntPtr hVideoWindow,
        Guid clsidTransform   // GUID_NULL = No effect transform.
        )
    {
        TRACE("CreateVideoBranch");

        IMFTopologyNode pOutputNode = null;
        IMFActivate pRendererActivate = null;

        // Create a downstream node.
        MFExtern.MFCreateTopologyNode(MFTopologyType.OutputNode, out pOutputNode);

        try
        {
            // Create an IMFActivate object for the video renderer.
            MFExtern.MFCreateVideoRendererActivate(hVideoWindow, out pRendererActivate);

            // Set the IActivate object on the output node.
            pOutputNode.SetObject(pRendererActivate);

            // Add the output node to the topology.
            pTopology.AddNode(pOutputNode);

            // Connect the source to the output.
            ConnectSourceToOutput(pTopology, pSourceNode, pOutputNode, clsidTransform);
        }
        finally
        {
            SafeRelease(pOutputNode);
            SafeRelease(pRendererActivate);
        }
    }


    ///////////////////////////////////////////////////////////////////////
    //  Name: CreateAudioBranch
    //  Description:  
    //  Adds and connects the nodes downstream from an audio source node.
    //
    //  pTopology:      Pointer to the topology.
    //  pSourceNode:    Pointer to the source node.
    //  clsidTransform: CLSID of an effect transform. 
    /////////////////////////////////////////////////////////////////////////

    void CreateAudioBranch(
        IMFTopology pTopology,
        IMFTopologyNode pSourceNode,
        Guid clsidTransform  // GUID_NULL = No transform.
        )
    {
        TRACE("CreateAudioBranch");

        IMFTopologyNode pOutputNode = null;
        IMFActivate pRendererActivate = null;

        // Create a downstream node.
        MFExtern.MFCreateTopologyNode(MFTopologyType.OutputNode, out pOutputNode);

        try
        {
            // Create an IMFActivate object for the audio renderer.
            MFExtern.MFCreateAudioRendererActivate(out pRendererActivate);

            // Set the IActivate object on the output node.
            pOutputNode.SetObject(pRendererActivate);

            // Add the output node to the topology.
            pTopology.AddNode(pOutputNode);
            ConnectSourceToOutput(pTopology, pSourceNode, pOutputNode, clsidTransform);
        }
        finally
        {
            // Connect the source to the output.
            SafeRelease(pOutputNode);
            SafeRelease(pRendererActivate);
        }
    }

    ///////////////////////////////////////////////////////////////////////
    //  Name: ConnectSourceToOutput
    //  Description:  
    //  Connects the source node to the output node.
    //
    //  pTopology:      Pointer to the topology.
    //  pSourceNode:    Pointer to the source node.
    //  pOutputNode:    Pointer to the output node.
    //  clsidTransform: CLSID of an effect transform. 
    /////////////////////////////////////////////////////////////////////////

    void ConnectSourceToOutput(
        IMFTopology pTopology,
        IMFTopologyNode pSourceNode,
        IMFTopologyNode pOutputNode,
        Guid clsidTransform
        )
    {
        if (clsidTransform == Guid.Empty)
        {
            // There is no effect specified, so connect the source node
            // directly to the output node.
            pSourceNode.ConnectOutput(0, pOutputNode, 0);
        }
        else
        {
            IMFTopologyNode pTransformNode = null;
            object pTransformUnk = null;

            // Create a transform node.
            MFExtern.MFCreateTopologyNode(MFTopologyType.TransformNode, out pTransformNode);

            try
            {
                Type type = Type.GetTypeFromCLSID(clsidTransform);

                pTransformUnk = Activator.CreateInstance(type);
                pTransformNode.SetObject(pTransformUnk);

                //// Set the CLSID of the transform.
                //if (SUCCEEDED(hr))
                //{
                //    hr = pTransformNode->SetGUID(MF_TOPONODE_TRANSFORM_OBJECTID, clsidTransform);
                //}

                // Add the transform node to the topology.
                pTopology.AddNode(pTransformNode);

                // Connect the source node to the transform node.
                pSourceNode.ConnectOutput(0, pTransformNode, 0);

                // Connect the transform node to the output node.
                pTransformNode.ConnectOutput(0, pOutputNode, 0);
            }
            finally
            {
                SafeRelease(pTransformNode);
                //SafeRelease(pTransformUnk);
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////
    //  Name: GetStreamType
    //  Description:  
    //  Returns the major media type from a stream descriptor.
    //
    //  pTopology:      Pointer to the topology.
    //  pSourceNode:    Pointer to the source node.
    //  pOutputNode:    Pointer to the output node.
    //  clsidTrnasform: CLSID of an effect transform. 
    /////////////////////////////////////////////////////////////////////////

    void GetStreamType(IMFStreamDescriptor pSourceSD, out Guid pMajorType)
    {
        Debug.Assert(pSourceSD != null);
        //Debug.Assert(pMajorType != null);

        IMFMediaTypeHandler pHandler;

        // Get the media type handler for the stream.
        pSourceSD.GetMediaTypeHandler(out pHandler);

        try
        {
            // Get the major media type.
            pHandler.GetMajorType(out pMajorType);
        }
        finally
        {
            SafeRelease(pHandler);
        }
    }

    #endregion

    #region Member Variables

    protected IMFMediaSession m_pSession;
    protected IMFMediaSource m_pSource;
    protected IMFVideoDisplayControl m_pVideoDisplay;

    protected IntPtr m_hwndVideo;       // Video window.
    protected IntPtr m_hwndEvent;       // App window to receive events.
    protected PlayerState m_state;          // Current state of the media session.
    protected AutoResetEvent m_hCloseEvent;     // Event to wait on while closing

    #endregion
}
