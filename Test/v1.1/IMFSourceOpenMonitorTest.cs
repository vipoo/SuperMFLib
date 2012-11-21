using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.EVR;
using MediaFoundation.Transform;

namespace Testv11
{
    [ComVisible(true)]
    public class IMFSourceOpenMonitorTest : IMFSourceOpenMonitor
    {
        int m_iCount;

        public void DoTests()
        {
            GetInterface();
        }

        private void GetInterface()
        {
            m_iCount = 0;
            IMFSourceResolver pSourceResolver;
            object ppSource;

            IPropertyStore pProp;
            int hr = MFExtern.CreatePropertyStore(out pProp);
            MFError.ThrowExceptionForHR(hr);

            PropVariant var = new PropVariant(this);

            hr = MFExtern.MFCreateSourceResolver(out pSourceResolver);
            MFError.ThrowExceptionForHR(hr);

            // Set the event source property value.
            hr = pProp.SetValue(MFPKEY.SourceOpenMonitor, var);
            MFError.ThrowExceptionForHR(hr);

            MFObjectType ObjectType = MFObjectType.Invalid;

            try
            {
                hr = pSourceResolver.CreateObjectFromURL(
                          @"http://moo.local",                    // URL of the source.
                          MFResolution.MediaSource,  // Create a source object.
                          pProp,       // Optional property store.
                          out ObjectType, // Receives the created object type. 
                          out ppSource);   // Receives a pointer to the media source.
                MFError.ThrowExceptionForHR(hr);
            }
            catch
            {
                // Since there is no moo.local domain
            }

            Debug.Assert(m_iCount > 0);
        }

        #region IMFSourceOpenMonitor Members

        public int OnSourceEvent(IMFMediaEvent pEvent)
        {
            m_iCount++;

            MediaEventType typ;
            int hr = pEvent.GetType(out typ);
            MFError.ThrowExceptionForHR(hr);
            Debug.WriteLine(typ);

            return 0;
        }

        #endregion
    }
}
