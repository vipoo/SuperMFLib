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

            int hr = m_pd.GetStreamDescriptorCount(out i);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(i == 2);
        }

        void TestGetStreamDescriptorByIndex()
        {
            bool b;
            IMFStreamDescriptor pd;

            int hr = m_pd.GetStreamDescriptorByIndex(0, out b, out pd);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(pd != null);
        }

        void TestSelectStream()
        {
            int hr = m_pd.SelectStream(1);
            MFError.ThrowExceptionForHR(hr);
        }

        void TestDeselectStream()
        {
            int hr = m_pd.DeselectStream(1);
            MFError.ThrowExceptionForHR(hr);
        }

        void TestClone()
        {
            IMFPresentationDescriptor pd;

            int hr = m_pd.Clone(out pd);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(pd != null);
        }

        private void GetInterface()
        {
            IMFMediaSource m_pSource;
            IMFSourceResolver m_sr;
            MFObjectType pObjectType;
            object pSource;

            int hr = MFExtern.MFCreateSourceResolver(out m_sr);
            MFError.ThrowExceptionForHR(hr);

            hr = m_sr.CreateObjectFromURL(
                @"file://c:/sourceforge/mflib/test/media/AspectRatio4x3.wmv",
                MFResolution.MediaSource,
                null,
                out pObjectType,
                out pSource);
            MFError.ThrowExceptionForHR(hr);

            m_pSource = pSource as IMFMediaSource;

            hr = m_pSource.CreatePresentationDescriptor(out m_pd);
            MFError.ThrowExceptionForHR(hr);
        }
    }
}
