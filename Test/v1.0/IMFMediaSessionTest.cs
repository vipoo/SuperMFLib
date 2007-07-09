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

            MFDll.MFCreateTopology(out pTop);
            m_ms.SetTopology(MFSessionSetTopologyFlags.None, pTop);
        }

        void TestClearTopologies()
        {
            m_ms.ClearTopologies();
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
            m_ms.GetClock(out pClock);

            Debug.Assert(pClock != null);
        }

        void TestGetSessionCapabilities()
        {
            MFSessionCapabilities pCaps;

            m_ms.GetSessionCapabilities(out pCaps);
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
            MFDll.MFCreateMediaSession(null, out m_ms);
        }
    }
}
