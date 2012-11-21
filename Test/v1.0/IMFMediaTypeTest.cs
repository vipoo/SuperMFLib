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
    class IMFMediaTypeTest
    {
        IMFMediaType m_mt;

        public void DoTests()
        {
            GetInterface();

            TestGetMajorType();
            TestIsCompressedFormat();
            TestIsEqual();
            TestGetRepresentation();
        }

        void TestGetMajorType()
        {
            Guid g1 = Guid.NewGuid();
            Guid g2;

            int hr = m_mt.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, g1);
            MFError.ThrowExceptionForHR(hr);
            hr = m_mt.GetMajorType(out g2);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(g1 == g2);
        }

        void TestIsCompressedFormat()
        {
            bool b;

            int hr = m_mt.SetUINT32(MFAttributesClsid.MF_MT_ALL_SAMPLES_INDEPENDENT, 0);
            MFError.ThrowExceptionForHR(hr);
            hr = m_mt.IsCompressedFormat(out b);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(b == true);
        }

        void TestIsEqual()
        {
            IMFMediaType mt;
            MFMediaEqual f;

            int hr = MFExtern.MFCreateMediaType(out mt);
            MFError.ThrowExceptionForHR(hr);

            Guid g1 = Guid.NewGuid();

            hr = mt.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, g1);
            MFError.ThrowExceptionForHR(hr);

            hr = m_mt.IsEqual(mt, out f);
            Debug.Assert(hr == 1);
        }

        [StructLayout(LayoutKind.Sequential)]
        private class AMMediaType
        {
            public Guid majorType;
            public Guid subType;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fixedSizeSamples;
            [MarshalAs(UnmanagedType.Bool)]
            public bool temporalCompression;
            public int sampleSize;
            public Guid formatType;
            public IntPtr unkPtr; // IUnknown Pointer
            public int formatSize;
            public IntPtr formatPtr; // Pointer to a buff determined by formatType
        }

        void TestGetRepresentation()
        {
            IntPtr ip = IntPtr.Zero;
            MFVideoFormat vf = new MFVideoFormat();
            AMMediaType a = new AMMediaType();

            try
            {
                int hr = m_mt.GetRepresentation(MFRepresentation.MFVideoFormat, out ip);
                MFError.ThrowExceptionForHR(hr);
                Marshal.PtrToStructure(ip, a);

                if (a.formatType == MFRepresentation.MFVideoFormat)
                {
                    Marshal.PtrToStructure(a.formatPtr, vf);
                }

                hr = m_mt.FreeRepresentation(MFRepresentation.MFVideoFormat, ip);
                MFError.ThrowExceptionForHR(hr);

            }
            catch { } // fails because it's not fully formed.  Tested in player.cs

        }

        private void GetInterface()
        {
            int hr = MFExtern.MFCreateMediaType(out m_mt);
            MFError.ThrowExceptionForHR(hr);
        }
    }
}
