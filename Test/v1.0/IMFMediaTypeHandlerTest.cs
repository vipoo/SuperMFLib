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
    class IMFMediaTypeHandlerTest
    {
        IMFMediaTypeHandler m_mth;

        public void DoTests()
        {
            GetInterface();

            TestIsMediaTypeSupported();
            TestGetMediaTypeCount();
            TestGetMediaTypeByIndex();
            TestGetCurrentMediaType();
            TestGetMajorType();
        }

        void TestIsMediaTypeSupported()
        {
            IMFMediaType mt, mt2;

            MFExtern.MFCreateMediaType(out mt);

            mt.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);

            m_mth.IsMediaTypeSupported(mt, out mt2);
        }

        void TestGetMediaTypeCount()
        {
            int i;
            m_mth.GetMediaTypeCount(out i);

            Debug.Assert(i == 1);
        }

        void TestGetMediaTypeByIndex()
        {
            IMFMediaType pType;
            m_mth.GetMediaTypeByIndex(0, out pType);

            Debug.Assert(pType != null);
        }

        void TestGetCurrentMediaType()
        {
            IMFMediaType pType, pType2;

            m_mth.GetMediaTypeByIndex(0, out pType);

            m_mth.SetCurrentMediaType(pType);
            m_mth.GetCurrentMediaType(out pType2);

            Debug.Assert(pType == pType2);
        }

        void TestGetMajorType()
        {
            Guid g;
            m_mth.GetMajorType(out g);

            Debug.Assert(g == MFMediaType.Video);
        }

        private void GetInterface()
        {
            IMFStreamDescriptor m_sd;
            IMFMediaType[] pmt = new IMFMediaType[1];
            MFExtern.MFCreateMediaType(out pmt[0]);

            pmt[0].SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);

            MFExtern.MFCreateStreamDescriptor(333, 1, pmt, out m_sd);

            m_sd.GetMediaTypeHandler(out m_mth);
        }
    }
}
