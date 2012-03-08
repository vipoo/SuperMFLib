using System;
using System.Text;

using MediaFoundation;
using MediaFoundation.Misc;
using System.Diagnostics;

namespace Testv21
{
    class IMFPluginControlTest
    {
        const int E_PROP_ID_UNSUPPORTED = unchecked((int)0x80070490);
        IMFPluginControl m_pic;

        public void DoTests()
        {
            GetInterface();

            TestPreferred();
            TestDisable();
        }

        private void TestPreferred()
        {
            int hr;
            Guid g, g2;
            string s;

            // Get one to play with
            hr = m_pic.GetPreferredClsidByIndex(MFPluginType.MediaSource, 0, out s, out g);
            MFError.ThrowExceptionForHR(hr);

            // See if the value looks reasonable
            Debug.Assert(g != Guid.Empty);
            Debug.Assert(s != null && s != "");

            // Delete it (process specific)
            hr = m_pic.SetPreferredClsid(MFPluginType.MediaSource, s, null);
            MFError.ThrowExceptionForHR(hr);

            // Read the value back
            hr = m_pic.GetPreferredClsid(MFPluginType.MediaSource, s, out g2);
            Debug.Assert(hr == E_PROP_ID_UNSUPPORTED);

            // Restore the value
            hr = m_pic.SetPreferredClsid(MFPluginType.MediaSource, s, g);
            MFError.ThrowExceptionForHR(hr);

            // Should be back where we started
            hr = m_pic.GetPreferredClsid(MFPluginType.MediaSource, s, out g2);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(g == g2);
        }

        private void TestDisable()
        {
            int hr;
            Guid g, g2;
            string s;

            // On my machine, nothing is disabled by default.  Get something to disable
            hr = m_pic.GetPreferredClsidByIndex(MFPluginType.MediaSource, 0, out s, out g);
            MFError.ThrowExceptionForHR(hr);

            // Disable it
            hr = m_pic.SetDisabled(MFPluginType.MediaSource, g, true);
            MFError.ThrowExceptionForHR(hr);

            // See if it's there
            hr = m_pic.GetDisabledByIndex(MFPluginType.MediaSource, 0, out g2);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(g == g2);

            // Is it disabled?
            hr = m_pic.IsDisabled(MFPluginType.MediaSource, g);
            Debug.Assert(hr == 0);

            // Put it back
            hr = m_pic.SetDisabled(MFPluginType.MediaSource, g, false);
            MFError.ThrowExceptionForHR(hr);

            // And?
            hr = m_pic.IsDisabled(MFPluginType.MediaSource, g);
            Debug.Assert(hr == E_PROP_ID_UNSUPPORTED);
        }

        private void GetInterface()
        {
            int hr = MFExtern.MFGetPluginControl(out m_pic);
            MFError.ThrowExceptionForHR(hr);
        }
    }
}
