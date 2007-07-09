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
    class IMFMediaEventQueueTest : IMFAsyncCallback
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

            m_meq.GetEvent(MFEventFlag.NoWait, out pEvent);
        }

        void TestBeginGetEvent()
        {
            m_meq.BeginGetEvent(this, null);
            m_are.WaitOne(-1, true);
        }

        void TestQueueEvent()
        {
            IMFMediaEvent pEvent;

            MFPlatDll.MFCreateMediaEvent(
                MediaEventType.MESourceStarted,
                Guid.Empty,
                0,
                null,
                out pEvent
                );
            m_meq.QueueEvent(pEvent);
        }

        void TestQueueEventParamVar()
        {
            IMFMediaEvent pEvent;
            Guid g = Guid.NewGuid();
            PropVariant p = new PropVariant();

            m_meq.QueueEventParamVar(MediaEventType.MESessionClosed, g, 0, new PropVariant("asdf"));

            m_meq.GetEvent(MFEventFlag.None, out pEvent);

            pEvent.GetValue(p);

            Debug.Assert(p.GetString() == "asdf");
        }

        void TestQueueEventParamUnk()
        {
            m_meq.QueueEventParamUnk(MediaEventType.MESessionEnded, Guid.Empty, 0, this);
        }

        void TestShutdown()
        {
            m_meq.Shutdown();
        }

        private void GetInterface()
        {
            MFPlatDll.MFCreateEventQueue(out m_meq);
        }


        #region IMFAsyncCallback Members

        public void GetParameters(out MFASync pdwFlags, out MFAsyncCallbackQueue pdwQueue)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Invoke(IMFAsyncResult pAsyncResult)
        {
            IMFMediaEvent pEvent;

            m_meq.EndGetEvent(pAsyncResult, out pEvent);
            m_are.Set();
        }

        #endregion
    }
}
