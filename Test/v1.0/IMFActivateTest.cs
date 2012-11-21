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
    class IMFActivateTest
    {
        IMFActivate m_a;

        public void DoTests()
        {
            int hr;
            object o;
            GetInterface();

            hr = m_a.ActivateObject(typeof(IMFGetService).GUID, out o);
            MFError.ThrowExceptionForHR(hr);

            try
            {
                hr = m_a.DetachObject(); // Not implemented
                MFError.ThrowExceptionForHR(hr);
            }
            catch { }

            hr = m_a.ShutdownObject();
            MFError.ThrowExceptionForHR(hr);
        }

        private void GetInterface()
        {
            int hr;
            System.Windows.Forms.Form f = new System.Windows.Forms.Form();
            hr = MFExtern.MFCreateVideoRendererActivate(IntPtr.Zero, out m_a);
            MFError.ThrowExceptionForHR(hr);
        }
    }
}
