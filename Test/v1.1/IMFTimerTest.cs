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
                int hr = m_time.SetTimer(MFTimeFlags.None, 10000, this, this, out pUnk);
                MFError.ThrowExceptionForHR(hr);
                System.Threading.Thread.Sleep(1000);
                Debug.Assert(m_Count == 1);
                hr = m_time.SetTimer(MFTimeFlags.Relative, 10000, this, this, out pUnk);
                MFError.ThrowExceptionForHR(hr);
                hr = m_time.CancelTimer(pUnk);
                MFError.ThrowExceptionForHR(hr);
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
            int hr = MFExtern.MFCreatePresentationClock(out pc);
            MFError.ThrowExceptionForHR(hr);

            IMFPresentationTimeSource stc;
            hr = MFExtern.MFCreateSystemTimeSource(out stc);
            MFError.ThrowExceptionForHR(hr);

            hr = pc.SetTimeSource(stc);
            MFError.ThrowExceptionForHR(hr);

            hr = pc.Start(0);
            MFError.ThrowExceptionForHR(hr);

            m_time = pc as IMFTimer;
        }

        #region IMFAsyncCallback Members

        public int GetParameters(out MFASync pdwFlags, out MFAsyncCallbackQueue pdwQueue)
        {
            throw new NotImplementedException();
        }

        public int Invoke(IMFAsyncResult pAsyncResult)
        {
            Debug.WriteLine("Here");
            m_Count++;

            return 0;
        }

        #endregion
    }
}
