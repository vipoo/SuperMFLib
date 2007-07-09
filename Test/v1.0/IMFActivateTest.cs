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
    class IMFActivateTest
    {
        IMFActivate m_a;

        public void DoTests()
        {
            object o;
            GetInterface();

            m_a.ActivateObject(typeof(IMFGetService).GUID, out o);

            try
            {
                m_a.DetachObject(); // Not implemented
            }
            catch { }

            m_a.ShutdownObject();
        }

        private void GetInterface()
        {
            System.Windows.Forms.Form f = new System.Windows.Forms.Form();
            MFDll.MFCreateVideoRendererActivate(IntPtr.Zero, out m_a);
        }
    }
}
