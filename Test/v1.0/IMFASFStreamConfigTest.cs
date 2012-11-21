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
    class IMFASFStreamConfigTest
    {
        private IMFASFStreamConfig m_sc;

        public void DoTests()
        {
            GetInterface();

            TestMT();
            TestStreamNumber();
            TestType();
            TestPayload();
        }

        private void TestType()
        {
            Guid g;

            int hr = m_sc.GetStreamType(out g);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(g == MFMediaType.Video, "GetStreamType");
        }

        private void TestMT()
        {
            IMFMediaType mt;

            int hr = m_sc.GetMediaType(out mt);
            MFError.ThrowExceptionForHR(hr);
            hr = m_sc.SetMediaType(mt);
            MFError.ThrowExceptionForHR(hr);
        }

        private void TestStreamNumber()
        {
            int hr = m_sc.SetStreamNumber(0);
            MFError.ThrowExceptionForHR(hr);
            short s = m_sc.GetStreamNumber();

            Debug.Assert(s == 0, "GetStreamNumber");
        }

        private void TestClone()
        {
            IMFASFStreamConfig sc;

            int hr = m_sc.Clone(out sc);
            MFError.ThrowExceptionForHR(hr);
        }

        private void TestPayload()
        {
            const int iSize = 10;
            short s, ds;
            int es;
            IntPtr ip = Marshal.AllocCoTaskMem(iSize);
            IntPtr ip2 = Marshal.AllocCoTaskMem(iSize);
            Guid g;

            Marshal.WriteInt64(ip, 1234567890);

            int hr = m_sc.AddPayloadExtension(MFASFSampleExtension.SampleDuration, 2, ip, iSize);
            MFError.ThrowExceptionForHR(hr);
            hr = m_sc.GetPayloadExtensionCount(out s);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(s == 1, "GetPayloadExtensionCount");

            es = iSize;
            hr = m_sc.GetPayloadExtension(0, out g, out ds, ip2, ref es);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(Marshal.ReadInt64(ip2) == 1234567890 && es == 10 && g == MFASFSampleExtension.SampleDuration && ds == 2);

            hr = m_sc.RemoveAllPayloadExtensions();
            MFError.ThrowExceptionForHR(hr);
            hr = m_sc.GetPayloadExtensionCount(out s);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(s == 0, "GetPayloadExtensionCount");
        }

        private void GetInterface()
        {
            FourCC cc4 = new FourCC("YUY2");
            IMFMediaType mt;

            IMFASFProfile ap;

            int hr = MFExtern.MFCreateASFProfile(out ap);
            MFError.ThrowExceptionForHR(hr);
            hr = MFExtern.MFCreateMediaType(out mt);
            MFError.ThrowExceptionForHR(hr);

            hr = mt.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, cc4.ToMediaSubtype());
            MFError.ThrowExceptionForHR(hr);
            hr = mt.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);
            MFError.ThrowExceptionForHR(hr);

            hr = ap.CreateStream(mt, out m_sc);
            MFError.ThrowExceptionForHR(hr);
        }

    }
}
