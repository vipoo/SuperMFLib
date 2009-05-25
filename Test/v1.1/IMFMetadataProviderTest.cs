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

            IMFMetadata md;
            mdp.GetMFMetadata(pd, 0, 0, out md);

            Debug.Assert(md != null);

        }
    }
}
