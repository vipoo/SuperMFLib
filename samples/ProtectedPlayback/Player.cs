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

        int hr;

        m_pSession = null;
        m_pSource = null;
        m_pVideoDisplay = null;
        m_hwndVideo = hVideo;
        m_hwndEvent = hEvent;
        m_state = PlayerState.Ready;
        m_pContentProtectionManager = null;

        m_hCloseEvent = new AutoResetEvent(false);

        hr = MFExtern.MFStartup(0x10070, MFStartup.Full);
        MFError.ThrowExceptionForHR(hr);
    }

#if DEBUG

    // Destructor is private. Caller should call Release.
    ~CPlayer()
    {
        Debug.Assert(m_pSession == null);  // If FALSE, the app did not call Shutdown().
    }

#endif

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
            hr = m_pSession.SetTopology(0, pTopology);
            MFError.ThrowExceptionForHR(hr);

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
            hr = m_pSession.Pause();
            MFError.ThrowExceptionForHR(hr);

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
            if (m_pContentProtectionManager != null)
            {
                m_pContentProtectionManager.Dispose();
                m_pContentProtectionManager = null;
            }

            if (m_hCloseEvent != null)
            {
                // Close the session
                CloseSession();

                // Shutdown the Media Foundation platform
                hr = MFExtern.MFShutdown();
                MFError.ThrowExceptionForHR(hr);

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
                hr = m_pVideoDisplay.RepaintVideo();
                MFError.ThrowExceptionForHR(hr);
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
                MFRect rcDest = new MFRect();
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
                MFError.ThrowExceptionForHR(hr);
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

    ///////////////////////////////////////////////////////////////////////
    //  Name: GetContentProtectionManager
    //  Description:  Returns the content protection manager object.
    //
    //  This is a helper object for handling IMFContentEnabler operations.
    /////////////////////////////////////////////////////////////////////////
    public int GetContentProtectionManager(out ContentProtectionManager ppManager)
    {
        int hr;

        ppManager = m_pContentProtectionManager;

        if (m_pContentProtectionManager == null)
        {
            hr = unchecked((int)0x80004005); // Session wasn't created yet. No helper object;
        }
        else
        {
            hr = 0;
        }

        return hr;
    }

    #endregion

    #region IMFAsyncCallback Members

    int IMFAsyncCallback.GetParameters(out MFASync pdwFlags, out MFAsyncCallbackQueue pdwQueue)
    {
        // Make sure we *never* leave this entry point with an exception
        try
        {
            pdwFlags = MFASync.FastIOProcessingCallback;
            pdwQueue = MFAsyncCallbackQueue.Standard;
            //throw new COMException("IMFAsyncCallback.GetParameters not implemented in Player", E_NotImplemented);
            return S_Ok;
        }
        catch (Exception e)
        {
            pdwQueue = MFAsyncCallbackQueue.Undefined;
            pdwFlags = MFASync.None;
            return Marshal.GetHRForException(e);
        }
    }

    int IMFAsyncCallback.Invoke(IMFAsyncResult pResult)
    {
        // Make sure we *never* leave this entry point with an exception
        try
        {
            int hr;
            IMFMediaEvent pEvent = null;
            MediaEventType meType = MediaEventType.MEUnknown;  // Event type
            int hrStatus = 0;           // Event status
            MFTopoStatus TopoStatus = MFTopoStatus.Invalid; // Used with MESessionTopologyStatus event.

            try
            {
                // Get the event from the event queue.
                hr = m_pSession.EndGetEvent(pResult, out pEvent);
                MFError.ThrowExceptionForHR(hr);

                // Get the event type.
                hr = pEvent.GetType(out meType);
                MFError.ThrowExceptionForHR(hr);

                // Get the event status. If the operation that triggered the event did
                // not succeed, the status is a failure code.
                hr = pEvent.GetStatus(out hrStatus);
                MFError.ThrowExceptionForHR(hr);

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
                            hr = pEvent.GetUINT32(MFAttributesClsid.MF_EVENT_TOPOLOGY_STATUS, out i);
                            MFError.ThrowExceptionForHR(hr);
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

                        default:
                            Debug.WriteLine(meType);
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
                    hr = m_pSession.BeginGetEvent(this, null);
                    MFError.ThrowExceptionForHR(hr);
                }

                SafeRelease(pEvent);
            }
            return S_Ok;
        }
        catch (Exception e)
        {
            return Marshal.GetHRForException(e);
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
        TRACE("CPlayer::CreateSession");

        int hr;
        IMFAttributes pAttributes;
        IMFActivate pEnablerActivate;

        // Close the old session, if any.
        CloseSession();

        // Create a new attribute store.
        hr = MFExtern.MFCreateAttributes(out pAttributes, 1);
        MFError.ThrowExceptionForHR(hr);

        // Create the content protection manager.
        m_pContentProtectionManager = new ContentProtectionManager(m_hwndEvent);

        // Set the MF_SESSION_CONTENT_PROTECTION_MANAGER attribute with a pointer
        // to the content protection manager.
        hr = pAttributes.SetUnknown(
            MFAttributesClsid.MF_SESSION_CONTENT_PROTECTION_MANAGER,
            m_pContentProtectionManager
            );
        MFError.ThrowExceptionForHR(hr);

        // Create the PMP media session.
        try
        {
            hr = MFExtern.MFCreatePMPMediaSession(
                MFPMPSessionCreationFlags.None,
                pAttributes,
                out m_pSession,
                out pEnablerActivate
                );
            MFError.ThrowExceptionForHR(hr);
        }
        catch
        {
            // TODO:

            // If MFCreatePMPMediaSession fails it might return an IMFActivate pointer.
            // This indicates that a trusted binary failed to load in the protected process.
            // An application can use the IMFActivate pointer to create an enabler object, which
            // provides revocation and renewal information for the component that failed to
            // load.

            // This sample does not demonstrate that feature. Instead, we simply treat this
            // case as a playback failure.
            throw;
        }

        hr = m_pSession.BeginGetEvent(this, null);
        MFError.ThrowExceptionForHR(hr);

        SafeRelease(pAttributes);
        SafeRelease(pEnablerActivate);
    }

    protected void CloseSession()
    {
        if (m_pVideoDisplay != null)
        {
            Marshal.ReleaseComObject(m_pVideoDisplay);
            m_pVideoDisplay = null;
        }

        int hr;

        if (m_pSession != null)
        {
            hr = m_pSession.Close();
            MFError.ThrowExceptionForHR(hr);

            // Wait for the close operation to complete
            bool res = m_hCloseEvent.WaitOne(5000, false);
            if (!res)
            {
                TRACE(("WaitForSingleObject timed out!"));
            }
        }

        // Complete shutdown operations

        // 1. Shut down the media source
        if (m_pSource != null)
        {
            hr = m_pSource.Shutdown();
            MFError.ThrowExceptionForHR(hr);
            SafeRelease(m_pSource);
            m_pSource = null;
        }

        // 2. Shut down the media session. (Synchronous operation, no events.)
        if (m_pSession != null)
        {
            hr = m_pSession.Shutdown();
            MFError.ThrowExceptionForHR(hr);
            Marshal.ReleaseComObject(m_pSession);
            m_pSession = null;
        }
    }

    protected void StartPlayback()
    {
        TRACE("CPlayer::StartPlayback");

        Debug.Assert(m_pSession != null);

        int hr;

        hr = m_pSession.Start(Guid.Empty, new PropVariant());
        MFError.ThrowExceptionForHR(hr);
    }

    protected void CreateMediaSource(string sURL)
    {
        TRACE("CPlayer::CreateMediaSource");

        int hr;
        IMFSourceResolver pSourceResolver;
        object pSource;

        // Create the source resolver.
        hr = MFExtern.MFCreateSourceResolver(out pSourceResolver);
        MFError.ThrowExceptionForHR(hr);

        try
        {
            // Use the source resolver to create the media source.
            MFObjectType ObjectType = MFObjectType.Invalid;

            hr = pSourceResolver.CreateObjectFromURL(
                    sURL,                       // URL of the source.
                    MFResolution.MediaSource,   // Create a source object.
                    null,                       // Optional property store.
                    out ObjectType,             // Receives the created object type.
                    out pSource                 // Receives a pointer to the media source.
                );
            MFError.ThrowExceptionForHR(hr);

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

        int hr;
        IMFTopology pTopology = null;
        IMFPresentationDescriptor pSourcePD = null;
        int cSourceStreams = 0;

        try
        {
            // Create a new topology.
            hr = MFExtern.MFCreateTopology(out pTopology);
            MFError.ThrowExceptionForHR(hr);

            // Create the presentation descriptor for the media source.
            hr = m_pSource.CreatePresentationDescriptor(out pSourcePD);
            MFError.ThrowExceptionForHR(hr);

            // Get the number of streams in the media source.
            hr = pSourcePD.GetStreamDescriptorCount(out cSourceStreams);
            MFError.ThrowExceptionForHR(hr);

            TRACE(string.Format("Stream count: {0}", cSourceStreams));

            // For each stream, create the topology nodes and add them to the topology.
            for (int i = 0; i < cSourceStreams; i++)
            {
                AddBranchToPartialTopology(pTopology, pSourcePD, i);
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

    protected void AddBranchToPartialTopology(
        IMFTopology pTopology,
        IMFPresentationDescriptor pSourcePD,
        int iStream
        )
    {
        TRACE("CPlayer::AddBranchToPartialTopology");

        Debug.Assert(pTopology != null);

        int hr;
        IMFStreamDescriptor pSourceSD = null;
        IMFTopologyNode pSourceNode = null;
        IMFTopologyNode pOutputNode = null;
        bool fSelected = false;

        try
        {
            // Get the stream descriptor for this stream.
            hr = pSourcePD.GetStreamDescriptorByIndex(iStream, out fSelected, out pSourceSD);
            MFError.ThrowExceptionForHR(hr);

            // Create the topology branch only if the stream is selected.
            // Otherwise, do nothing.
            if (fSelected)
            {
                // Create a source node for this stream.
                CreateSourceStreamNode(pSourcePD, pSourceSD, out pSourceNode);

                // Create the output node for the renderer.
                CreateOutputNode(pSourceSD, out pOutputNode);

                // Add both nodes to the topology.
                hr = pTopology.AddNode(pSourceNode);
                MFError.ThrowExceptionForHR(hr);

                hr = pTopology.AddNode(pOutputNode);
                MFError.ThrowExceptionForHR(hr);

                // Connect the source node to the output node.
                hr = pSourceNode.ConnectOutput(0, pOutputNode, 0);
                MFError.ThrowExceptionForHR(hr);
            }
        }
        finally
        {
            // Clean up.
            SafeRelease(pSourceSD);
            SafeRelease(pSourceNode);
            SafeRelease(pOutputNode);
        }
    }

    protected void CreateSourceStreamNode(
        IMFPresentationDescriptor pSourcePD,
        IMFStreamDescriptor pSourceSD,
        out IMFTopologyNode ppNode
        )
    {
        Debug.Assert(m_pSource != null);

        int hr;
        IMFTopologyNode pNode = null;

        try
        {
            // Create the source-stream node.
            hr = MFExtern.MFCreateTopologyNode(MFTopologyType.SourcestreamNode, out pNode);
            MFError.ThrowExceptionForHR(hr);

            // Set attribute: Pointer to the media source.
            hr = pNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_SOURCE, m_pSource);
            MFError.ThrowExceptionForHR(hr);

            // Set attribute: Pointer to the presentation descriptor.
            hr = pNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_PRESENTATION_DESCRIPTOR, pSourcePD);
            MFError.ThrowExceptionForHR(hr);

            // Set attribute: Pointer to the stream descriptor.
            hr = pNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_STREAM_DESCRIPTOR, pSourceSD);
            MFError.ThrowExceptionForHR(hr);

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
                hr = pSourceSD.GetStreamIdentifier(out streamID); // Just for debugging, ignore any failures.
                MFError.ThrowExceptionForHR(hr);
            }
            catch
            {
                TRACE("IMFStreamDescriptor::GetStreamIdentifier" + hr.ToString());
            }

            // Get the media type handler for the stream.
            hr = pSourceSD.GetMediaTypeHandler(out pHandler);
            MFError.ThrowExceptionForHR(hr);

            // Get the major media type.
            hr = pHandler.GetMajorType(out guidMajorType);
            MFError.ThrowExceptionForHR(hr);

            // Create a downstream node.
            hr = MFExtern.MFCreateTopologyNode(MFTopologyType.OutputNode, out pNode);
            MFError.ThrowExceptionForHR(hr);

            // Create an IMFActivate object for the renderer, based on the media type.
            if (MFMediaType.Audio == guidMajorType)
            {
                // Create the audio renderer.
                TRACE(string.Format("Stream {0}: audio stream", streamID));
                hr = MFExtern.MFCreateAudioRendererActivate(out pRendererActivate);
                MFError.ThrowExceptionForHR(hr);
            }
            else if (MFMediaType.Video == guidMajorType)
            {
                // Create the video renderer.
                TRACE(string.Format("Stream {0}: video stream", streamID));
                hr = MFExtern.MFCreateVideoRendererActivate(m_hwndVideo, out pRendererActivate);
                MFError.ThrowExceptionForHR(hr);
            }
            else
            {
                TRACE(string.Format("Stream {0}: Unknown format", streamID));
                throw new COMException("Unknown format", E_Fail);
            }

            // Set the IActivate object on the output node.
            hr = pNode.SetObject(pRendererActivate);
            MFError.ThrowExceptionForHR(hr);

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
        int hr;
        object o;
        TRACE("CPlayer::OnTopologyReady");

        // Ask for the IMFVideoDisplayControl interface.
        // This interface is implemented by the EVR and is
        // exposed by the media session as a service.

        // Note: This call is expected to fail if the source
        // does not have video.

        try
        {
            hr = MFExtern.MFGetService(
                m_pSession,
                MFServices.MR_VIDEO_RENDER_SERVICE,
                typeof(IMFVideoDisplayControl).GUID,
                out o
                );
            MFError.ThrowExceptionForHR(hr);

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
            hr = Marshal.GetHRForException(ce);
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

    #region Member Variables

    protected IMFMediaSession m_pSession;
    protected IMFMediaSource m_pSource;
    protected IMFVideoDisplayControl m_pVideoDisplay;

    protected IntPtr m_hwndVideo;       // Video window.
    protected IntPtr m_hwndEvent;       // App window to receive events.
    protected PlayerState m_state;          // Current state of the media session.
    protected AutoResetEvent m_hCloseEvent;     // Event to wait on while closing

    protected ContentProtectionManager m_pContentProtectionManager;

    #endregion
}
