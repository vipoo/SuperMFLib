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

            m_sc.GetStreamType(out g);
            Debug.Assert(g == MFMediaType.Video, "GetStreamType");
        }

        private void TestMT()
        {
            IMFMediaType mt;

            m_sc.GetMediaType(out mt);
            m_sc.SetMediaType(mt);
        }

        private void TestStreamNumber()
        {
            m_sc.SetStreamNumber(0);
            short s = m_sc.GetStreamNumber();

            Debug.Assert(s == 0, "GetStreamNumber");
        }

        private void TestClone()
        {
            IMFASFStreamConfig sc;

            m_sc.Clone(out sc);
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

            m_sc.AddPayloadExtension(MFASFSampleExtension.SampleDuration, 2, ip, iSize);
            m_sc.GetPayloadExtensionCount(out s);

            Debug.Assert(s == 1, "GetPayloadExtensionCount");

            es = iSize;
            m_sc.GetPayloadExtension(0, out g, out ds, ip2, ref es);

            Debug.Assert(Marshal.ReadInt64(ip2) == 1234567890 && es == 10 && g == MFASFSampleExtension.SampleDuration && ds == 2);

            m_sc.RemoveAllPayloadExtensions();
            m_sc.GetPayloadExtensionCount(out s);

            Debug.Assert(s == 0, "GetPayloadExtensionCount");
        }

        private void GetInterface()
        {
            FourCC cc4 = new FourCC("YUY2");
            IMFMediaType mt;

            IMFASFProfile ap;

            MFExtern.MFCreateASFProfile(out ap);
            MFExtern.MFCreateMediaType(out mt);

            mt.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, cc4.ToMediaSubtype());
            mt.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);

            ap.CreateStream(mt, out m_sc);
        }

    }
}
