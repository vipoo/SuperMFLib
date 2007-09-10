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

public class ContentProtectionManager : COMBase, IMFAsyncCallback, IMFContentProtectionManager, IDisposable
{
    #region externs

    [DllImport("user32", CharSet = CharSet.Auto)]
    private extern static int PostMessage(
        IntPtr handle, int msg, IntPtr wParam, IntPtr lParam);

    #endregion

    #region Declarations

    private const int WM_APP = 0x8000;
    public const int WM_APP_CONTENT_ENABLER = WM_APP + 3;
    public const int WM_APP_BROWSER_DONE = WM_APP + 4;
    public const int NS_E_DRM_LICENSE_NOTACQUIRED = unchecked((int)0xC00D2759);

    public enum EnablerFlags
    {
        SilentOrNonSilent = 0,  // Use silent if supported, otherwise use non-silent.
        ForceNonSilent = 1      // Use non-silent.
    }

    public enum Enabler
    {
        Ready,
        SilentInProgress,
        NonSilentInProgress,
        Complete
    }

    #endregion

    #region Members

    private Enabler m_state;
    private int m_hrStatus;         // Status code from the most recent event.

    private IntPtr m_hwnd;

    private IMFContentEnabler m_pEnabler;        // Content enabler.
    private IMFMediaEventGenerator m_pMEG;            // The content enabler's event generator interface.
    private IMFAsyncCallback m_pCallback;
    private object m_punkState;

    #endregion

    public ContentProtectionManager(IntPtr hNotify)
    {
        m_hwnd = hNotify;
        m_state = Enabler.Ready;
    }

    /////////////////////////////////////////////////////////////////////////
    //  ContentProtectionManager destructor
    /////////////////////////////////////////////////////////////////////////

#if DEBUG

    ~ContentProtectionManager()
    {
        Debug.WriteLine("~ContentProtectionManager");

        // Probably all got released in Dispose, but just in case
        m_pEnabler = null;
        m_pMEG = null;
        m_pCallback = null;
        m_punkState = null;
    }

#endif

    #region IMFContentProtectionManager methods

    ///////////////////////////////////////////////////////////////////////
    //  Name: BeginEnableContent
    //  Description:  Called by the PMP session to start the enable action.
    /////////////////////////////////////////////////////////////////////////

    public void BeginEnableContent(
        IMFActivate pEnablerActivate,
        IMFTopology pTopo,
        IMFAsyncCallback pCallback,
        object punkState
        )
    {
        Debug.WriteLine("ContentProtectionManager::BeginEnableContent");

        if (m_pEnabler != null)
        {
            throw new COMException("A previous call is still pending", E_Fail);
        }

        // Save so we can create an async result later
        m_pCallback = pCallback;
        m_punkState = punkState;

        // Create the enabler from the IMFActivate pointer.
        object o;
        pEnablerActivate.ActivateObject(typeof(IMFContentEnabler).GUID, out o);
        m_pEnabler = o as IMFContentEnabler;

        // Notify the application. The application will call DoEnable from the app thread.
        m_state = Enabler.Ready; // Reset the state.
        PostMessage(m_hwnd, WM_APP_CONTENT_ENABLER, IntPtr.Zero, IntPtr.Zero);
    }

    ///////////////////////////////////////////////////////////////////////
    //  Name: EndEnableContent
    //  Description:  Completes the enable action.
    /////////////////////////////////////////////////////////////////////////

    public void EndEnableContent(IMFAsyncResult pResult)
    {
        Debug.WriteLine("ContentProtectionManager::EndEnableContent");

        if (pResult == null)
        {
            throw new COMException("NULL IMFAsyncResult", E_Pointer);
        }

        // Release interfaces, so that we're ready to accept another call
        // to BeginEnableContent.
        SafeRelease(m_pEnabler);
        SafeRelease(m_pMEG);
        SafeRelease(m_punkState);
        SafeRelease(m_pCallback);

        m_pEnabler = null;
        m_pMEG = null;
        m_pCallback = null;
        m_punkState = null;

        if (m_hrStatus < 0)
        {
            throw new COMException("hrStatus", m_hrStatus);
        }
    }

    #endregion

    #region IMFAsyncCallback methods

    public void GetParameters(out MFASync a, out MFAsyncCallbackQueue b)
    {
        // Implementation of this method is optional.
        throw new COMException("GetParameters not implemented", COMBase.E_NotImplemented);
    }

    ///////////////////////////////////////////////////////////////////////
    //  Name: Invoke
    //  Description:  Callback for asynchronous BeginGetEvent method.
    //
    //  pAsyncResult: Pointer to the result.
    /////////////////////////////////////////////////////////////////////////

    void IMFAsyncCallback.Invoke(IMFAsyncResult pAsyncResult)
    {
        IMFMediaEvent pEvent;
        MediaEventType meType = MediaEventType.MEUnknown;  // Event type
        PropVariant varEventData = new PropVariant();	        // Event data

        // Get the event from the event queue.
        m_pMEG.EndGetEvent(pAsyncResult, out pEvent);

        // Get the event type.
        pEvent.GetType(out meType);

        // Get the event status. If the operation that triggered the event did
        // not succeed, the status is a failure code.
        pEvent.GetStatus(out m_hrStatus);

        if (m_hrStatus == 862022) // NS_S_DRM_MONITOR_CANCELLED
        {
            m_hrStatus = MFError.MF_E_OPERATION_CANCELLED;
            m_state = Enabler.Complete;
        }

        // Get the event data.
        pEvent.GetValue(varEventData);

        // For the MEEnablerCompleted action, notify the application.
        // Otherwise, request another event.
        Debug.WriteLine(string.Format("Content enabler event: {0}", meType.ToString()));

        if (meType == MediaEventType.MEEnablerCompleted)
        {
            PostMessage(m_hwnd, WM_APP_CONTENT_ENABLER, IntPtr.Zero, IntPtr.Zero);
        }
        else
        {
            if (meType == MediaEventType.MEEnablerProgress)
            {
                if (varEventData.GetAttributeType() == PropVariant.VariantType.String)
                {
                    Debug.WriteLine(string.Format("Progress: {0}", varEventData.GetString()));
                }
            }
            m_pMEG.BeginGetEvent(this, null);
        }

        // Clean up.
        varEventData.Clear();
        SafeRelease(pEvent);
    }

    #endregion

    #region Public methods

    public Enabler GetState()
    {
        return m_state;
    }

    public int GetStatus()
    {
        return m_hrStatus;
    }

    ///////////////////////////////////////////////////////////////////////
    //  Name: DoEnable
    //  Description:  Does the enabler action.
    //
    //  flags: If ForceNonSilent, then always use non-silent enable.
    //         Otherwise, use silent enable if possible.
    ////////////////////////////////////////////////////////////////////////

    public void DoEnable(EnablerFlags flags)
    {
        Debug.WriteLine(string.Format("ContentProtectionManager::DoEnable (flags ={0})", flags.ToString()));

        bool bAutomatic = false;
        Guid guidEnableType;

        try
        {
            // Get the enable type. (Just for logging. We don't use it.)
            m_pEnabler.GetEnableType(out guidEnableType);

            LogEnableType(guidEnableType);

            // Query for the IMFMediaEventGenerator interface so that we can get the
            // enabler events.
            m_pMEG = (IMFMediaEventGenerator)m_pEnabler;

            // Ask for the first event.
            m_pMEG.BeginGetEvent(this, null);

            // Decide whether to use silent or non-silent enabling. If flags is ForceNonSilent,
            // then we use non-silent. Otherwise, we query whether the enabler object supports
            // silent enabling (also called "automatic" enabling).
            if (flags == EnablerFlags.ForceNonSilent)
            {
                Debug.WriteLine(("Forcing non-silent enable."));
                bAutomatic = false;
            }
            else
            {
                m_pEnabler.IsAutomaticSupported(out bAutomatic);
                Debug.WriteLine(string.Format("IsAutomatic: auto = {0}", bAutomatic));
            }

            // Start automatic or non-silent, depending.
            if (bAutomatic)
            {
                m_state = Enabler.SilentInProgress;
                Debug.WriteLine("Content enabler: Automatic is supported");
                m_pEnabler.AutomaticEnable();
            }
            else
            {
                m_state = Enabler.NonSilentInProgress;
                Debug.WriteLine("Content enabler: Using non-silent enabling");
                DoNonSilentEnable();
            }
        }
        catch (Exception e)
        {
            m_hrStatus = Marshal.GetHRForException(e);
            throw;
        }
    }

    ///////////////////////////////////////////////////////////////////////
    //  Name: CancelEnable
    //  Description:  Cancels the current action.
    //
    //  During silent enable, this cancels the enable action in progress.
    //  During non-silent enable, this cancels the MonitorEnable thread.
    /////////////////////////////////////////////////////////////////////////

    public void CancelEnable()
    {
        int hr = 0;

        if (m_state != Enabler.Complete)
        {
            try
            {
                m_pEnabler.Cancel();
            }
            catch (Exception e)
            {
                hr = Marshal.GetHRForException(e);
            }

            if (hr < 0)
            {
                // If Cancel fails for some reason, queue the MEEnablerCompleted
                // event ourselves. This will cause the current action to fail.
                m_pMEG.QueueEvent(MediaEventType.MEEnablerCompleted, Guid.Empty, hr, null);
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////
    //  Name: CompleteEnable
    //  Description:  Completes the current action.
    //
    //  This method invokes the PMP session's callback.
    /////////////////////////////////////////////////////////////////////////

    public void CompleteEnable()
    {
        m_state = Enabler.Complete;

        // m_pCallback can be NULL if the BeginEnableContent was not called.
        // This is the case when the application initiates the enable action, eg
        // when MFCreatePMPMediaSession fails and returns an IMFActivate pointer.
        if (m_pCallback != null)
        {
            Debug.WriteLine(string.Format("ContentProtectionManager: Invoking the pipeline's callback. (status = 0x{0})", m_hrStatus));
            IMFAsyncResult pResult;

            MFExtern.MFCreateAsyncResult(null, m_pCallback, m_punkState, out pResult);
            pResult.SetStatus(m_hrStatus);

            MFExtern.MFInvokeCallback(pResult);
        }
    }

    #endregion

    #region Private methods

    ///////////////////////////////////////////////////////////////////////
    //  Name: DoNonSilentEnable
    //  Description:  Performs non-silent enable.
    /////////////////////////////////////////////////////////////////////////

    private void DoNonSilentEnable()
    {
        // Trust status for the URL.
        MFURLTrustStatus trustStatus = MFURLTrustStatus.Untrusted;

        string sURL;	            // Enable URL
        int cchURL = 0;             // Size of enable URL in characters.

        IntPtr pPostData;        // Buffer to hold HTTP POST data.
        int cbPostDataSize = 0;   // Size of buffer, in bytes.

        // Get the enable URL. This is where we get the enable data for non-silent enabling.
        m_pEnabler.GetEnableURL(out sURL, out cchURL, out trustStatus);

        Debug.WriteLine(string.Format("Content enabler: URL = {0}", sURL));
        LogTrustStatus(trustStatus);

        if (trustStatus != MFURLTrustStatus.Trusted)
        {
            throw new COMException("The enabler URL is not trusted. Failing.", E_Fail);
        }

        // Start the thread that monitors the non-silent enable action.
        m_pEnabler.MonitorEnable();

        // Get the HTTP POST data
        m_pEnabler.GetEnableData(out pPostData, out cbPostDataSize);

        try
        {
            WebHelper m_webHelper = new WebHelper();

            // Open the URL and send the HTTP POST data.
            m_webHelper.OpenURLWithData(sURL, pPostData, cbPostDataSize, m_hwnd);
        }
        finally
        {
            Marshal.FreeCoTaskMem(pPostData);
        }
    }

    private void LogEnableType(Guid guidEnableType)
    {
        if (guidEnableType == MFEnabletype.MFENABLETYPE_WMDRMV1_LicenseAcquisition)
        {
            Debug.WriteLine("MFENABLETYPE_WMDRMV1_LicenseAcquisition");
        }
        else if (guidEnableType == MFEnabletype.MFENABLETYPE_WMDRMV7_LicenseAcquisition)
        {
            Debug.WriteLine("MFENABLETYPE_WMDRMV7_LicenseAcquisition");
        }
        else if (guidEnableType == MFEnabletype.MFENABLETYPE_WMDRMV7_Individualization)
        {
            Debug.WriteLine("MFENABLETYPE_WMDRMV7_Individualization");
        }
        else if (guidEnableType == MFEnabletype.MFENABLETYPE_MF_UpdateRevocationInformation)
        {
            Debug.WriteLine("MFENABLETYPE_MF_UpdateRevocationInformation");
        }
        else if (guidEnableType == MFEnabletype.MFENABLETYPE_MF_UpdateUntrustedComponent)
        {
            Debug.WriteLine("MFENABLETYPE_MF_UpdateUntrustedComponent");
        }
        else
        {
            Debug.WriteLine("Unknown content enabler type.");
        }
    }

    private void LogTrustStatus(MFURLTrustStatus status)
    {
        Debug.WriteLine(status.ToString());
    }

    #endregion

    #region IDisposable Members

    public void  Dispose()
    {
        Debug.WriteLine("ContentProtectionManager::Dispose");
 	    SafeRelease(m_pEnabler);
 	    SafeRelease(m_pMEG);
 	    SafeRelease(m_pCallback);
 	    SafeRelease(m_punkState);

 	    m_pEnabler = null;
 	    m_pMEG = null;
 	    m_pCallback = null;
 	    m_punkState = null;
    }

    #endregion
}
