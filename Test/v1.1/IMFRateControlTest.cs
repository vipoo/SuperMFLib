using System;
using System.Diagnostics;

using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.EVR;
using MediaFoundation.Transform;

namespace Testv11
{
    public class IMFRateControlTest
    {
        IMFRateControl m_rc;

        public void DoTests()
        {
            GetInterface();

            TestRate();
        }

        private void TestRate()
        {
            bool b = true;
            float pf;
            m_rc.GetRate(ref b, out pf);

            m_rc.SetRate(true, 0.5f);
        }

        private void GetInterface()
        {
            IMFMediaSession ms;
            MFExtern.MFCreateMediaSession(null, out ms);
            m_rc = ms as IMFRateControl;
        }
    }
}
