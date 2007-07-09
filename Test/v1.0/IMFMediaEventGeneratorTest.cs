using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

using MediaFoundation;
using MediaFoundation.Utils;
using MediaFoundation.Misc;
using MediaFoundation.EVR;

namespace Testv10
{
    class IMFMediaEventGeneratorTest : IMFAsyncCallback
    {
        AutoResetEvent m_are = new AutoResetEvent(false);
        IMFMediaEventGenerator m_meg;

        public void DoTests()
        {
            IMFMediaEvent pEvent;

            GetInterface();

            m_meg.QueueEvent(MediaEventType.MESourceStarted, Guid.NewGuid(), 323, new PropVariant("asdf"));
            m_meg.GetEvent(MFEventFlag.None, out pEvent);
            m_meg.QueueEvent(MediaEventType.MESourcePaused, Guid.NewGuid(), 333, new PropVariant("xasdf"));
            m_meg.BeginGetEvent(this, this);
            m_are.WaitOne(-1, true);
        }

        private void GetInterface()
        {
            IMFSourceResolver pSourceResolver = null;
            MFObjectType ObjectType;
            object pSource = null;

            // Create the source resolver.
            MFDll.MFCreateSourceResolver(out pSourceResolver);

            pSourceResolver.CreateObjectFromURL(
                    @"file://c:/sourceforge/mflib/test/media/AspectRatio4x3.wmv",
                    MFResolution.MediaSource,	// Create a source object.
                    null,						// Optional property store.
                    out ObjectType,				// Receives the created object type. 
                    out pSource					// Receives a pointer to the media source.
                );

            // Get the IMFMediaSource interface from the media source.
            m_meg = (IMFMediaEventGenerator)pSource;
        }

        #region IMFAsyncCallback Members

        public void GetParameters(out MFASync pdwFlags, out MFAsyncCallbackQueue pdwQueue)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Invoke(IMFAsyncResult pAsyncResult)
        {
            IMFMediaEvent pEvent;
            m_meg.EndGetEvent(pAsyncResult, out pEvent);
            m_are.Set();
        }

        #endregion
    }
}
