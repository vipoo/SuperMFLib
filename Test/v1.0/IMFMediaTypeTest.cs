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

            m_mt.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, g1);
            m_mt.GetMajorType(out g2);

            Debug.Assert(g1 == g2);
        }

        void TestIsCompressedFormat()
        {
            bool b;

            m_mt.SetUINT32(MFAttributesClsid.MF_MT_ALL_SAMPLES_INDEPENDENT, 0);
            m_mt.IsCompressedFormat(out b);

            Debug.Assert(b == true);
        }

        void TestIsEqual()
        {
            IMFMediaType mt;
            MFMediaEqual f;

            MFExtern.MFCreateMediaType(out mt);

            Guid g1 = Guid.NewGuid();

            mt.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, g1);

            int hr = m_mt.IsEqual(mt, out f);
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
                m_mt.GetRepresentation(MFRepresentation.MFVideoFormat, out ip);
                Marshal.PtrToStructure(ip, a);

                if (a.formatType == MFRepresentation.MFVideoFormat)
                {
                    Marshal.PtrToStructure(a.formatPtr, vf);
                }

                m_mt.FreeRepresentation(MFRepresentation.MFVideoFormat, ip);

            }
            catch { } // fails because it's not fully formed.  Tested in player.cs

        }

        private void GetInterface()
        {
            MFExtern.MFCreateMediaType(out m_mt);
        }
    }
}
