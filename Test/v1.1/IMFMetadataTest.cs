using System;
using System.Diagnostics;

using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.EVR;
using MediaFoundation.Transform;

namespace Testv11
{
    public class IMFMetadataTest
    {
        IMFMetadata m_md;

        public void DoTests()
        {
            GetInterface();

            TestProps();
            TestLang();
        }

        private void TestLang()
        {
            string sLang;
            PropVariant pv = new PropVariant();

            int hr = m_md.SetLanguage("en-gb");
            MFError.ThrowExceptionForHR(hr);

            hr = m_md.GetLanguage(out sLang);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(sLang == "en-gb");
            hr = m_md.GetAllLanguages(pv);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(pv.GetVariantType() == ConstPropVariant.VariantType.StringArray);
            string[] sa = pv.GetStringArray();
            Debug.Assert(sa.Length > 0);
            Debug.Assert(sa[0] == "en-us");
        }

        private void TestProps()
        {
            PropVariant pv = new PropVariant();
            PropVariant pv2 = new PropVariant();
            PropVariant pv3 = new PropVariant("moo");

            int hr = m_md.GetAllPropertyNames(pv);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(pv.GetVariantType() == ConstPropVariant.VariantType.StringArray);
            string[] sa = pv.GetStringArray();
            Debug.Assert(sa.Length > 0);

            // The sr I'm using is R/O
            try
            {
                hr = m_md.SetProperty("foo", pv3);
                MFError.ThrowExceptionForHR(hr);
            }
            catch { }
            try
            {
                hr = m_md.DeleteProperty("foo");
                MFError.ThrowExceptionForHR(hr);
            }
            catch { }


            hr = m_md.GetProperty("Buffer Average", pv2);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(pv2.GetVariantType() == ConstPropVariant.VariantType.UInt32);
            Debug.Assert(pv2.GetUInt() == 200);

        }

        private void GetInterface()
        {
            IMFMediaSource pSource1;
            IMFPresentationDescriptor pd;
            IMFSourceResolver sr;
            MFObjectType pObjectType;
            object pSource;

            int hr = MFExtern.MFCreateSourceResolver(out sr);
            MFError.ThrowExceptionForHR(hr);

            hr = sr.CreateObjectFromURL(
                @"file://c:/sourceforge/mflib/test/media/AspectRatio4x3.wmv",
                MFResolution.MediaSource,
                null,
                out pObjectType,
                out pSource);

            IMFMetadataProvider mdp = pSource as IMFMetadataProvider;

            pSource1 = pSource as IMFMediaSource;

            hr = pSource1.CreatePresentationDescriptor(out pd);
            MFError.ThrowExceptionForHR(hr);

            hr = mdp.GetMFMetadata(pd, 0, 0, out m_md);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(m_md != null);
        }
    }
}
