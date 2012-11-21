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
    class IMFMediaEventTest
    {
        IMFMediaEvent m_me;

        public void DoTests()
        {
            GetInterface();

            TestGetType();
            TestGetExtendedType();
            TestGetStatus();
            TestGetValue();
        }

        void TestGetType()
        {
            MediaEventType m;

            int hr = m_me.GetType(out m);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(MediaEventType.MESourceStarted == m);
        }

        void TestGetExtendedType()
        {
            Guid g;
            int hr = m_me.GetExtendedType(out g);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(g != Guid.Empty);
        }

        void TestGetStatus()
        {
            int i;
            int hr = m_me.GetStatus(out i);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(i == 313);
        }

        void TestGetValue()
        {
            PropVariant p = new PropVariant("FDSA");
            int hr = m_me.GetValue(p);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(p.GetString() == "asdf");
        }

        private void GetInterface()
        {
            int hr = MFExtern.MFCreateMediaEvent(
                MediaEventType.MESourceStarted,
                Guid.NewGuid(),
                313,
                new PropVariant("asdf"),
                out m_me
                );
            MFError.ThrowExceptionForHR(hr);
        }
    }
}
