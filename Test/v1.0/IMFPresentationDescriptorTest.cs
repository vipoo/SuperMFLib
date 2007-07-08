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
    class IMFPresentationDescriptorTest
    {
        IMFPresentationDescriptor m_pd;

        public void DoTests()
        {
            GetInterface();

            TestGetStreamDescriptorCount();
            TestGetStreamDescriptorByIndex();
            TestSelectStream();
            TestDeselectStream();
            TestClone();
        }

        void TestGetStreamDescriptorCount()
        {
            int i;

            m_pd.GetStreamDescriptorCount(out i);

            Debug.Assert(i == 2);
        }

        void TestGetStreamDescriptorByIndex()
        {
            bool b;
            IMFStreamDescriptor pd;

            m_pd.GetStreamDescriptorByIndex(0, out b, out pd);

            Debug.Assert(pd != null);
        }

        void TestSelectStream()
        {
            m_pd.SelectStream(1);
        }

        void TestDeselectStream()
        {
            m_pd.DeselectStream(1);
        }

        void TestClone()
        {
            IMFPresentationDescriptor pd;

            m_pd.Clone(out pd);

            Debug.Assert(pd != null);
        }

        private void GetInterface()
        {
            IMFMediaSource m_pSource;
            IMFSourceResolver m_sr;
            MFObjectType pObjectType;
            object pSource;

            int hr = MFDll.MFCreateSourceResolver(out m_sr);
            MFError.ThrowExceptionForHR(hr);

            m_sr.CreateObjectFromURL(
                @"file://c:/sourceforge/mflib/test/media/AspectRatio4x3.wmv",
                MFResolution.MediaSource,
                null,
                out pObjectType,
                out pSource);

            m_pSource = pSource as IMFMediaSource;

            m_pSource.CreatePresentationDescriptor(out m_pd);
        }
    }
}
