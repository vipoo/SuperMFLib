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
            MFExtern.CreatePropertyStore(out pProp);

            PropVariant var = new PropVariant(this);

            MFExtern.MFCreateSourceResolver(out pSourceResolver);

            // Set the event source property value.
            pProp.SetValue(MFPKEY.MFPKEY_SourceOpenMonitor, var);

            MFObjectType ObjectType = MFObjectType.Invalid;

            try
            {
                pSourceResolver.CreateObjectFromURL(
                          @"http://moo.local",                    // URL of the source.
                          MFResolution.MediaSource,  // Create a source object.
                          pProp,       // Optional property store.
                          out ObjectType, // Receives the created object type. 
                          out ppSource);   // Receives a pointer to the media source.
            }
            catch
            {
                // Since there is no moo.local domain
            }

            Debug.Assert(m_iCount > 0);
        }

        #region IMFSourceOpenMonitor Members

        public void OnSourceEvent(IMFMediaEvent pEvent)
        {
            m_iCount++;

            MediaEventType typ;
            pEvent.GetType(out typ);
            Debug.WriteLine(typ);
        }

        #endregion
    }
}
