using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

using MediaFoundation;
using MediaFoundation.Misc;

namespace Testv10
{
    class IMFStreamDescriptorTest
    {
        IMFStreamDescriptor m_sd;

        public void DoTests()
        {
            GetInterface();

            TestGetStreamID();
            TestGetMediaTypeHandler();
        }

        private void TestGetStreamID()
        {
            int i;

            int hr = m_sd.GetStreamIdentifier(out i);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(i == 333);
        }

        private void TestGetMediaTypeHandler()
        {
            IMFMediaTypeHandler pHan;

            int hr = m_sd.GetMediaTypeHandler(out pHan);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(pHan != null);
        }

        private void GetInterface()
        {
            IMFMediaType[] pmt = new IMFMediaType[1];
            int hr = MFExtern.MFCreateMediaType(out pmt[0]);
            MFError.ThrowExceptionForHR(hr);

            hr = MFExtern.MFCreateStreamDescriptor(333, 1, pmt, out m_sd);
            MFError.ThrowExceptionForHR(hr);
        }
    }
}
