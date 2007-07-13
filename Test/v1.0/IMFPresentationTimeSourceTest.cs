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
    class IMFPresentationTimeSourceTest
    {
        IMFPresentationTimeSource m_pts;

        public void DoTests()
        {
            IMFClock pc;

            MFExtern.MFCreateSystemTimeSource(out m_pts);
            m_pts.GetUnderlyingClock(out pc);
        }
    }
}
