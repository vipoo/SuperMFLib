// Tested in wavsource.cs

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
    class IMFMediaStreamTest
    {
        IMFMediaStream m_ms;

        public void DoTests()
        {
            GetInterface();
        }

        private void GetInterface()
        {
            IMFSourceResolver pSourceResolver = null;
            MFObjectType ObjectType;
            object pSource = null;

            // Create the source resolver.
            int hr = MFDll.MFCreateSourceResolver(out pSourceResolver);
            MFError.ThrowExceptionForHR(hr);

            pSourceResolver.CreateObjectFromURL(
                    @"file://c:/sourceforge/mflib/test/media/AspectRatio4x3.wmv",
                    MFResolution.ByteStream,	// Create a source object.
                    null,						// Optional property store.
                    out ObjectType,				// Receives the created object type. 
                    out pSource					// Receives a pointer to the media source.
                );

            // Get the IMFMediaSource interface from the media source.
            m_ms = (IMFMediaStream)pSource;
        }
    }
}
