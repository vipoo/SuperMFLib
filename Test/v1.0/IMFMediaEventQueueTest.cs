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
    class IMFMediaEventQueueTest : COMBase, IMFAsyncCallback
    {
        IMFMediaEventQueue m_meq;
        AutoResetEvent m_are = new AutoResetEvent(false);

        public void DoTests()
        {
            GetInterface();

            TestQueueEvent();
            TestGetEvent();
            TestQueueEventParamUnk();
            TestBeginGetEvent();

            TestQueueEventParamVar();
            TestShutdown();
        }

        void TestGetEvent()
        {
            IMFMediaEvent pEvent;

            int hr = m_meq.GetEvent(MFEventFlag.NoWait, out pEvent);
            MFError.ThrowExceptionForHR(hr);
        }

        void TestBeginGetEvent()
        {
            int hr = m_meq.BeginGetEvent(this, null);
            MFError.ThrowExceptionForHR(hr);
            m_are.WaitOne(-1, true);
        }

        void TestQueueEvent()
        {
            IMFMediaEvent pEvent;

            int hr = MFExtern.MFCreateMediaEvent(
                MediaEventType.MESourceStarted,
                Guid.Empty,
                0,
                null,
                out pEvent
                );
            MFError.ThrowExceptionForHR(hr);
            hr = m_meq.QueueEvent(pEvent);
            MFError.ThrowExceptionForHR(hr);
        }

        void TestQueueEventParamVar()
        {
            IMFMediaEvent pEvent;
            Guid g = Guid.NewGuid();
            PropVariant p = new PropVariant();

            int hr = m_meq.QueueEventParamVar(MediaEventType.MESessionClosed, g, 0, new PropVariant("asdf"));
            MFError.ThrowExceptionForHR(hr);

            hr = m_meq.GetEvent(MFEventFlag.None, out pEvent);
            MFError.ThrowExceptionForHR(hr);

            hr = pEvent.GetValue(p);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(p.GetString() == "asdf");
        }

        void TestQueueEventParamUnk()
        {
            int hr = m_meq.QueueEventParamUnk(MediaEventType.MESessionEnded, Guid.Empty, 0, this);
            MFError.ThrowExceptionForHR(hr);
        }

        void TestShutdown()
        {
            int hr = m_meq.Shutdown();
            MFError.ThrowExceptionForHR(hr);
        }

        private void GetInterface()
        {
            int hr = MFExtern.MFCreateEventQueue(out m_meq);
            MFError.ThrowExceptionForHR(hr);
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

            int hr = m_meq.EndGetEvent(pAsyncResult, out pEvent);
            MFError.ThrowExceptionForHR(hr);
            m_are.Set();

            return S_Ok;
        }

        #endregion
    }
}
