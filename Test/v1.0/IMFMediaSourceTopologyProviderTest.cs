// http://msdn2.microsoft.com/en-us/library/ms697285.aspx

using System;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

using MediaFoundation;
using MediaFoundation.Misc;

namespace Testv10
{
    class IMFMediaSourceTopologyProviderTest
    {
        public void DoTests()
        {
            Again();
        }

        private void Again()
        {
            IMFSourceResolver sr;
            MFObjectType pObjectType;
            object pSource;
            IMFMediaSession pMediaSession;
            IMFMediaSource ms;
            IMFTopology pt;

            int hr = MFExtern.MFCreateMediaSession(null, out pMediaSession);
            MFError.ThrowExceptionForHR(hr);

            IMFSequencerSource pSequencerSource;
            hr = MFExtern.MFCreateSequencerSource(null, out pSequencerSource);
            MFError.ThrowExceptionForHR(hr);

            hr = MFExtern.MFCreateSourceResolver(out sr);
            MFError.ThrowExceptionForHR(hr);

            hr = sr.CreateObjectFromURL(
                @"file://c:/sourceforge/mflib/test/media/AspectRatio4x3.wmv",
                MFResolution.MediaSource,
                null,
                out pObjectType,
                out pSource);
            MFError.ThrowExceptionForHR(hr);

            ms = pSource as IMFMediaSource;

            hr = MFExtern.MFCreateTopology(out pt);
            MFError.ThrowExceptionForHR(hr);

            // http://msdn2.microsoft.com/en-us/library/ms701605.aspx

            int sid;
            hr = pSequencerSource.AppendTopology(pt, MFSequencerTopologyFlags.Last, out sid);
            MFError.ThrowExceptionForHR(hr);

            SetFirstTopology(pSequencerSource, pMediaSession);
        }

        private void SetFirstTopology(IMFSequencerSource pSequencerSource, IMFMediaSession pMediaSession)
        {

            IMFMediaSource pMediaSource;
            IMFPresentationDescriptor pPresentationDescriptor;
            IMFMediaSourceTopologyProvider pMediaSourceTopologyProvider;
            IMFTopology pTopology;

            pMediaSource = pSequencerSource as IMFMediaSource;

            // Create the presentation descriptor for the media source.
            int hr = pMediaSource.CreatePresentationDescriptor(out pPresentationDescriptor);
            MFError.ThrowExceptionForHR(hr);

            // Get the topology provider from the sequencer source.
            pMediaSourceTopologyProvider = pSequencerSource as IMFMediaSourceTopologyProvider;
            // Get the first topology from the topology provider.
            hr = pMediaSourceTopologyProvider.GetMediaSourceTopology(pPresentationDescriptor, out pTopology);
            MFError.ThrowExceptionForHR(hr);

            // Set the topology on the media session.
            hr = pMediaSession.SetTopology(0, pTopology);
            MFError.ThrowExceptionForHR(hr);
        }
    }
}
