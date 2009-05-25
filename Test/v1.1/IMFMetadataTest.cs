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

            TestLang();
            TestProps();
        }

        private void TestLang()
        {
            string sLang;
            PropVariant pv = new PropVariant();

            m_md.SetLanguage("asdf");

            m_md.GetLanguage(out sLang);

            Debug.Assert(sLang == "asdf");
            m_md.GetAllLanguages(pv);

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

            m_md.GetAllPropertyNames(pv);

            Debug.Assert(pv.GetVariantType() == ConstPropVariant.VariantType.StringArray);
            string[] sa = pv.GetStringArray();
            Debug.Assert(sa.Length > 0);

            // The sr I'm using is R/O
            try
            {
                m_md.SetProperty("foo", pv3);
            }
            catch { }
            try
            {
                m_md.DeleteProperty("foo");
            }
            catch { }


            m_md.GetProperty("Buffer Average", pv2);
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

            MFExtern.MFCreateSourceResolver(out sr);

            sr.CreateObjectFromURL(
                @"file://c:/sourceforge/mflib/test/media/AspectRatio4x3.wmv",
                MFResolution.MediaSource,
                null,
                out pObjectType,
                out pSource);

            IMFMetadataProvider mdp = pSource as IMFMetadataProvider;

            pSource1 = pSource as IMFMediaSource;

            pSource1.CreatePresentationDescriptor(out pd);

            mdp.GetMFMetadata(pd, 0, 0, out m_md);

            Debug.Assert(m_md != null);
        }
    }
}
