using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.EVR;
using MediaFoundation.Transform;

namespace Testv11
{
    [ComVisible(true)]
    public class IMFTimerTest : IMFAsyncCallback
    {
        IMFTimer m_time;
        IMFPresentationClock pc;
        int m_Count;

        public void DoTests()
        {
            GetInterface();

            TestTimer();
        }

        private void TestTimer()
        {
            object pUnk;

            IMFAsyncCallback cb = this as IMFAsyncCallback;

            m_Count = 0;

            try
            {
                m_time.SetTimer(MFTimeFlags.None, 10000, this, this, out pUnk);
                System.Threading.Thread.Sleep(1000);
                Debug.Assert(m_Count == 1);
                m_time.SetTimer(MFTimeFlags.Relative, 10000, this, this, out pUnk);
                m_time.CancelTimer(pUnk);
                System.Threading.Thread.Sleep(2000);
            }
            catch
            {
                Debug.WriteLine("Busted");
            }

            Debug.Assert(m_Count == 1);
        }

        private void GetInterface()
        {
            MFExtern.MFCreatePresentationClock(out pc);

            IMFPresentationTimeSource stc;
            MFExtern.MFCreateSystemTimeSource(out stc);

            pc.SetTimeSource(stc);

            pc.Start(0);

            m_time = pc as IMFTimer;
        }

        #region IMFAsyncCallback Members

        public void GetParameters(out MFASync pdwFlags, out MFAsyncCallbackQueue pdwQueue)
        {
            throw new NotImplementedException();
        }

        public void Invoke(IMFAsyncResult pAsyncResult)
        {
            Debug.WriteLine("Here");
            m_Count++;
        }

        #endregion
    }
}
