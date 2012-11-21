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
    class IMFMediaBufferTest
    {
        IMFMediaBuffer m_mb;

        public void DoTests()
        {
            GetInterface();

            TestSetCurrentLength();
            TestGetMaxLength();

            TestLock();
            TestUnlock();
        }

        void TestLock()
        {
            IntPtr ip;
            int imax, iCur;
            int hr = m_mb.Lock(out ip, out imax, out iCur);
            MFError.ThrowExceptionForHR(hr);
        }

        void TestUnlock()
        {
            int hr = m_mb.Unlock();
            MFError.ThrowExceptionForHR(hr);
        }

        void TestSetCurrentLength()
        {
            int i;

            int hr = m_mb.SetCurrentLength(33);
            MFError.ThrowExceptionForHR(hr);
            hr = m_mb.GetCurrentLength(out i);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(i == 33);
        }

        void TestGetMaxLength()
        {
            int i;

            int hr = m_mb.GetMaxLength(out i);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(i == 100);
        }

        private void GetInterface()
        {
            int hr = MFExtern.MFCreateMemoryBuffer(100, out m_mb);
            MFError.ThrowExceptionForHR(hr);
        }
    }
}
