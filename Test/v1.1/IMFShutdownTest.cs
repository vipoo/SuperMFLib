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
    public class IMFShutdownTest
    {
        IMFShutdown m_shut;

        public void DoTests()
        {
            GetInterface();

            TestShut();
        }

        private void TestShut()
        {
            MFShutdownStatus st, st2;

            m_shut.GetShutdownStatus(out st);
            m_shut.Shutdown();
            System.Threading.Thread.Sleep(1000);
            m_shut.GetShutdownStatus(out st2);
        }

        private void GetInterface()
        {
            IMFPresentationClock pc;
            MFExtern.MFCreatePresentationClock(out pc);

            IMFPresentationTimeSource stc;
            MFExtern.MFCreateSystemTimeSource(out stc);

            pc.SetTimeSource(stc);

            m_shut = pc as IMFShutdown;
        }

    }
}
