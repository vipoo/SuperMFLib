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
            m_mb.Lock(out ip, out imax, out iCur);
        }

        void TestUnlock()
        {
            m_mb.Unlock();
        }

        void TestSetCurrentLength()
        {
            int i;

            m_mb.SetCurrentLength(33);
            m_mb.GetCurrentLength(out i);

            Debug.Assert(i == 33);
        }

        void TestGetMaxLength()
        {
            int i;

            m_mb.GetMaxLength(out i);

            Debug.Assert(i == 100);
        }

        private void GetInterface()
        {
            MFExtern.MFCreateMemoryBuffer(100, out m_mb);
        }
    }
}
