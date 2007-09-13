// This interface isn't implemented by anything in vista sp0

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
    class IMFASFStreamPrioritizationTest
    {
        private IMFASFStreamPrioritization m_sp;

        public void DoTests()
        {
            GetInterface();
        }

        private void GetInterface()
        {
            IMFASFProfile ap;
            IntPtr o;

            MFExtern.MFCreateASFProfile(out ap);

            ap.CreateStreamPrioritization(out o);
            //m_sp = o as IMFASFStreamPrioritization;
        }
    }
}
