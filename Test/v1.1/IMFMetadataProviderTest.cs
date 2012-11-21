using System;
using System.Diagnostics;

using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.EVR;
using MediaFoundation.Transform;

namespace Testv11
{
    public class IMFMetadataProviderTest
    {
        public void DoTests()
        {
            GetInterface();
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
            MFError.ThrowExceptionForHR(hr);

            IMFMetadataProvider mdp = pSource as IMFMetadataProvider;

            pSource1 = pSource as IMFMediaSource;

            hr = pSource1.CreatePresentationDescriptor(out pd);
            MFError.ThrowExceptionForHR(hr);

            IMFMetadata md;
            hr = mdp.GetMFMetadata(pd, 0, 0, out md);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(md != null);

        }
    }
}
