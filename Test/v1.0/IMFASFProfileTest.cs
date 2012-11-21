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
    class IMFASFProfileTest
    {
        private IMFASFProfile m_p;

        public void DoTests()
        {
            GetInterface();

            TestCreateStream();
            TestExclusion();
            TestPrior();
            TestMutex();
            TestMisc();
        }

        private void TestMisc()
        {
            IMFASFProfile ap;

            int hr = m_p.Clone(out ap);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(ap != null);
        }

        private void TestCreateStream()
        {
            short s;
            IMFMediaType mt;
            IMFASFStreamConfig pStream, pStream2, pStream3;
            FourCC cc4 = new FourCC("YUY2");

            int hr = MFExtern.MFCreateMediaType(out mt);
            MFError.ThrowExceptionForHR(hr);

            hr = mt.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, cc4.ToMediaSubtype());
            MFError.ThrowExceptionForHR(hr);
            hr = mt.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);
            MFError.ThrowExceptionForHR(hr);

            hr = m_p.CreateStream(mt, out pStream);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(pStream != null);

            hr = m_p.SetStream(pStream);
            MFError.ThrowExceptionForHR(hr);

            int i, i2;
            hr = m_p.GetStreamCount(out i);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(i == 1);

            hr = m_p.GetStream(0, out s, out pStream2);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(pStream2 != null);

            hr = m_p.GetStreamByNumber(0, out pStream3);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(pStream2 == pStream3);

            IMFASFProfile p2;

            hr = m_p.Clone(out p2);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(p2 != null);
            hr = p2.GetStreamCount(out i);
            MFError.ThrowExceptionForHR(hr);
            hr = p2.RemoveStream(0);
            MFError.ThrowExceptionForHR(hr);
            hr = p2.GetStreamCount(out i2);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(i2 == i - 1);

        }

        private void TestExclusion()
        {
            int i, i2;
            IMFASFMutualExclusion me, me2;

            int hr = m_p.CreateMutualExclusion(out me);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(me != null);

            hr = m_p.AddMutualExclusion(me);
            MFError.ThrowExceptionForHR(hr);
            hr = m_p.GetMutualExclusionCount(out i);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(i == 1);
            hr = m_p.GetMutualExclusion(0, out me2);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(me2 != null);

            hr = m_p.RemoveMutualExclusion(0);
            MFError.ThrowExceptionForHR(hr);
            hr = m_p.GetMutualExclusionCount(out i2);
            Debug.Assert(i2 == 0);

        }

        private void TestPrior()
        {
#if false
            // You cannot either create, not retrieve a prioritization object
            IMFASFStreamPrioritization sp = null;

            try
            {
                m_p.CreateStreamPrioritization(out sp);
            }
            catch
            {
                // E_NOTIMPL
            }

            Debug.Assert(sp == null);

            m_p.GetStreamPrioritization(out sp);

            IMFASFStreamPrioritization sp2;
            // There doesn't appear to be any way to *get* an IMFASFStreamPrioritization
            Debug.Assert(sp != null);

            sp.Clone(out sp2);

            m_p.RemoveMutualExclusion(0);

            m_p.AddStreamPrioritization(sp);
#endif
        }

        private void TestMutex()
        {
            IMFASFMutualExclusion pm, pm2;
            int i;

            int hr = m_p.GetMutualExclusionCount(out i);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(i == 0);

            hr = m_p.CreateMutualExclusion(out pm);
            MFError.ThrowExceptionForHR(hr);
            hr = m_p.AddMutualExclusion(pm);
            MFError.ThrowExceptionForHR(hr);
            hr = m_p.GetMutualExclusionCount(out i);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(i == 1);

            hr = m_p.GetMutualExclusion(0, out pm2);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(pm2 != null);

            hr = m_p.RemoveMutualExclusion(0);
            MFError.ThrowExceptionForHR(hr);
            hr = m_p.GetMutualExclusionCount(out i);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(i == 0);
        }

        private void GetInterface()
        {
            int hr = MFExtern.MFCreateASFProfile(out m_p);
            MFError.ThrowExceptionForHR(hr);
        }
    }
}
