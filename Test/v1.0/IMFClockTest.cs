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
    class IMFClockTest
    {
        IMFClock m_c;

        public void DoTests()
        {
            GetInterface();

#if false
            // Tested by adding this code to BasicPlayer's pause
            IMFClock m_c;
            MFClockCharacteristicsFlags c;
            long l, l2;
            int iKey;
            MFClockState pState;
            MFClockProperties pProp;

            m_pSession.GetClock(out m_c);
            m_c.GetClockCharacteristics(out c);
            m_c.GetCorrelatedTime(0, out l, out l2);
            m_c.GetContinuityKey(out iKey);
            m_c.GetState(0, out pState);
            m_c.GetProperties(out pProp);

            TestGetClockCharacteristics();
            TestGetCorrelatedTime();
            TestGetContinuityKey();
            TestGetState();
            TestGetProperties();
#endif
        }

        void TestGetClockCharacteristics()
        {
            MFClockCharacteristicsFlags c;

            int hr = m_c.GetClockCharacteristics(out c);
            MFError.ThrowExceptionForHR(hr);
        }

        void TestGetCorrelatedTime()
        {
            long l, l2;
            int hr = m_c.GetCorrelatedTime(0, out l, out l2);
            MFError.ThrowExceptionForHR(hr);
        }

        void TestGetContinuityKey()
        {
            int iKey;
            int hr = m_c.GetContinuityKey(out iKey);
            MFError.ThrowExceptionForHR(hr);
        }

        void TestGetState()
        {
            MFClockState pState;
            int hr = m_c.GetState(0, out pState);
            MFError.ThrowExceptionForHR(hr);
        }

        void TestGetProperties()
        {
            MFClockProperties pProp;

            int hr = m_c.GetProperties(out pProp);
            MFError.ThrowExceptionForHR(hr);
        }

        private void GetInterface()
        {
            IMFPresentationClock pc;

            int hr = MFExtern.MFCreatePresentationClock(out pc);
            MFError.ThrowExceptionForHR(hr);
            m_c = pc as IMFClock;

            IMFMediaSession ms;

            hr = MFExtern.MFCreateMediaSession(null, out ms);
            MFError.ThrowExceptionForHR(hr);
            hr = ms.GetClock(out m_c);
            MFError.ThrowExceptionForHR(hr);
        }
    }
}
