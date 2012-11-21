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

            int hr = MFExtern.MFCreateMediaType(out mt);
            MFError.ThrowExceptionForHR(hr);

            hr = mt.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);
            MFError.ThrowExceptionForHR(hr);

            IntPtr ip = Marshal.AllocCoTaskMem(IntPtr.Size);

            try
            {
                hr = m_mth.IsMediaTypeSupported(mt, ip);
                MFError.ThrowExceptionForHR(hr);

                if (Marshal.ReadIntPtr(ip) != IntPtr.Zero)
                {
                    mt2 = (IMFMediaType)Marshal.GetObjectForIUnknown(ip);
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem(ip);
            }
        }

        void TestGetMediaTypeCount()
        {
            int i;
            int hr = m_mth.GetMediaTypeCount(out i);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(i == 1);
        }

        void TestGetMediaTypeByIndex()
        {
            IMFMediaType pType;
            int hr = m_mth.GetMediaTypeByIndex(0, out pType);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(pType != null);
        }

        void TestGetCurrentMediaType()
        {
            IMFMediaType pType, pType2;

            int hr = m_mth.GetMediaTypeByIndex(0, out pType);
            MFError.ThrowExceptionForHR(hr);

            hr = m_mth.SetCurrentMediaType(pType);
            MFError.ThrowExceptionForHR(hr);
            hr = m_mth.GetCurrentMediaType(out pType2);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(pType == pType2);
        }

        void TestGetMajorType()
        {
            Guid g;
            int hr = m_mth.GetMajorType(out g);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(g == MFMediaType.Video);
        }

        private void GetInterface()
        {
            IMFStreamDescriptor m_sd;
            IMFMediaType[] pmt = new IMFMediaType[1];
            int hr = MFExtern.MFCreateMediaType(out pmt[0]);
            MFError.ThrowExceptionForHR(hr);

            pmt[0].SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);

            hr = MFExtern.MFCreateStreamDescriptor(333, 1, pmt, out m_sd);
            MFError.ThrowExceptionForHR(hr);

            hr = m_sd.GetMediaTypeHandler(out m_mth);
            MFError.ThrowExceptionForHR(hr);
        }
    }
}
