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

            m_c.GetClockCharacteristics(out c);
        }

        void TestGetCorrelatedTime()
        {
            long l, l2;
            m_c.GetCorrelatedTime(0, out l, out l2);
        }

        void TestGetContinuityKey()
        {
            int iKey;
            m_c.GetContinuityKey(out iKey);
        }

        void TestGetState()
        {
            MFClockState pState;
            m_c.GetState(0, out pState);
        }

        void TestGetProperties()
        {
            MFClockProperties pProp;

            m_c.GetProperties(out pProp);
        }

        private void GetInterface()
        {
            IMFPresentationClock pc;

            MFDll.MFCreatePresentationClock(out pc);
            m_c = pc as IMFClock;

            IMFMediaSession ms;

            MFDll.MFCreateMediaSession(null, out ms);
            ms.GetClock(out m_c);
        }
    }
}
