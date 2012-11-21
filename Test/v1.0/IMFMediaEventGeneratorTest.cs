using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.EVR;

namespace Testv10
{
    class IMFMediaEventGeneratorTest : COMBase, IMFAsyncCallback
    {
        AutoResetEvent m_are = new AutoResetEvent(false);
        IMFMediaEventGenerator m_meg;

        public void DoTests()
        {
            IMFMediaEvent pEvent;

            GetInterface();

            int hr = m_meg.QueueEvent(MediaEventType.MESourceStarted, Guid.NewGuid(), 323, new PropVariant("asdf"));
            MFError.ThrowExceptionForHR(hr);
            hr = m_meg.GetEvent(MFEventFlag.None, out pEvent);
            MFError.ThrowExceptionForHR(hr);
            hr = m_meg.QueueEvent(MediaEventType.MESourcePaused, Guid.NewGuid(), 333, new PropVariant("xasdf"));
            MFError.ThrowExceptionForHR(hr);
            hr = m_meg.BeginGetEvent(this, this);
            MFError.ThrowExceptionForHR(hr);
            m_are.WaitOne(-1, true);
        }

        private void GetInterface()
        {
            IMFSourceResolver pSourceResolver = null;
            MFObjectType ObjectType;
            object pSource = null;

            // Create the source resolver.
            int hr = MFExtern.MFCreateSourceResolver(out pSourceResolver);
            MFError.ThrowExceptionForHR(hr);

            hr = pSourceResolver.CreateObjectFromURL(
                    @"file://c:/sourceforge/mflib/test/media/AspectRatio4x3.wmv",
                    MFResolution.MediaSource,	// Create a source object.
                    null,						// Optional property store.
                    out ObjectType,				// Receives the created object type. 
                    out pSource					// Receives a pointer to the media source.
                );
            MFError.ThrowExceptionForHR(hr);

            // Get the IMFMediaSource interface from the media source.
            m_meg = (IMFMediaEventGenerator)pSource;
        }

        #region IMFAsyncCallback Members

        public int GetParameters(out MFASync pdwFlags, out MFAsyncCallbackQueue pdwQueue)
        {
            pdwFlags = 0;
            pdwQueue = 0;
            return E_NotImplemented;
        }

        public int Invoke(IMFAsyncResult pAsyncResult)
        {
            IMFMediaEvent pEvent;
            int hr = m_meg.EndGetEvent(pAsyncResult, out pEvent);
            MFError.ThrowExceptionForHR(hr);
            m_are.Set();

            return S_Ok;
        }

        #endregion
    }
}
