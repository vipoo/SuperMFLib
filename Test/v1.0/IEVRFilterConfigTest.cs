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
    class IEVRFilterConfigTest
    {
        IEVRFilterConfig m_fc;

        [ComImport, Guid("FA10746C-9B63-4b6c-BC49-FC300EA5F256")]
        protected class myEVR
        {
        }

        public void DoTests()
        {
            int i;
            int hr;

            myEVR my = new myEVR();
            m_fc = my as IEVRFilterConfig;

            hr = m_fc.SetNumberOfStreams(3);
            MFError.ThrowExceptionForHR(hr);

            hr = m_fc.GetNumberOfStreams(out i);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(i == 3);
        }
    }
}
