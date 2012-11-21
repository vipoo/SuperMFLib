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

            int hr = m_shut.GetShutdownStatus(out st);
            Debug.Assert(hr == unchecked((int)0xc00d36b2));
            hr = m_shut.Shutdown();
            MFError.ThrowExceptionForHR(hr);
            System.Threading.Thread.Sleep(1000);
            hr = m_shut.GetShutdownStatus(out st2);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(st2 == MFShutdownStatus.Completed);
        }

        private void GetInterface()
        {
            IMFPresentationClock pc;
            int hr = MFExtern.MFCreatePresentationClock(out pc);
            MFError.ThrowExceptionForHR(hr);

            IMFPresentationTimeSource stc;
            hr = MFExtern.MFCreateSystemTimeSource(out stc);
            MFError.ThrowExceptionForHR(hr);

            hr = pc.SetTimeSource(stc);
            MFError.ThrowExceptionForHR(hr);

            m_shut = pc as IMFShutdown;
        }

    }
}
