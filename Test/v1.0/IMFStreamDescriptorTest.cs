using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

using MediaFoundation;
using MediaFoundation.Utils;

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

            m_sd.GetStreamIdentifier(out i);

            Debug.Assert(i == 333);
        }

        private void TestGetMediaTypeHandler()
        {
            IMFMediaTypeHandler pHan;

            m_sd.GetMediaTypeHandler(out pHan);
            Debug.Assert(pHan != null);
        }

        private void GetInterface()
        {
            IMFMediaType[] pmt = new IMFMediaType[1];
            MFPlatDll.MFCreateMediaType(out pmt[0]);

            MFPlatDll.MFCreateStreamDescriptor(333, 1, pmt, out m_sd);
        }
    }
}
