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
    class IMFMediaSessionTest
    {
        IMFMediaSession m_ms;

        public void DoTests()
        {
            GetInterface();

            TestSetTopology();
            TestClearTopologies();
            TestStart();
            TestPause();
            TestStop();
            TestClose();
            TestShutdown();
            TestGetClock();
            TestGetSessionCapabilities();
            TestGetFullTopology();
        }

        void TestSetTopology()
        {
            IMFTopology pTop;

            int hr = MFExtern.MFCreateTopology(out pTop);
            MFError.ThrowExceptionForHR(hr);
            hr = m_ms.SetTopology(MFSessionSetTopologyFlags.None, pTop);
            MFError.ThrowExceptionForHR(hr);
        }

        void TestClearTopologies()
        {
            int hr = m_ms.ClearTopologies();
            MFError.ThrowExceptionForHR(hr);
        }

        void TestStart()
        {
            // Tested in BasicPlayer
        }

        void TestPause()
        {
            // Tested in BasicPlayer
        }

        void TestStop()
        {
            // Tested in BasicPlayer by changing Pause to Stop
        }

        void TestClose()
        {
            // Tested in BasicPlayer
        }

        void TestShutdown()
        {
            // Tested in BasicPlayer
        }

        void TestGetClock()
        {
            IMFClock pClock;
            int hr = m_ms.GetClock(out pClock);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(pClock != null);
        }

        void TestGetSessionCapabilities()
        {
            MFSessionCapabilities pCaps;

            int hr = m_ms.GetSessionCapabilities(out pCaps);
            MFError.ThrowExceptionForHR(hr);
        }

        void TestGetFullTopology()
        {
            // Tested in BasicPlayer by adding this to Pause()
            //MFSessionCapabilities pCaps;
            //IMFTopology pTop;
            //m_pSession.GetSessionCapabilities(out pCaps);
            //m_pSession.GetFullTopology(MFSessionGetFullTopologyFlags.Current, 0, out pTop);
        }

        private void GetInterface()
        {
            int hr = MFExtern.MFCreateMediaSession(null, out m_ms);
            MFError.ThrowExceptionForHR(hr);
        }
    }
}
