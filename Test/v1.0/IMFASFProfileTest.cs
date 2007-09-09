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

            m_p.Clone(out ap);
            Debug.Assert(ap != null);
        }

        private void TestCreateStream()
        {
            short s;
            IMFMediaType mt;
            IMFASFStreamConfig pStream, pStream2, pStream3;
            FourCC cc4 = new FourCC("YUY2");

            MFExtern.MFCreateMediaType(out mt);

            mt.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, cc4.ToMediaSubtype());
            mt.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);

            m_p.CreateStream(mt, out pStream);

            Debug.Assert(pStream != null);

            m_p.SetStream(pStream);

            int i, i2;
            m_p.GetStreamCount(out i);

            Debug.Assert(i == 1);

            m_p.GetStream(0, out s, out pStream2);

            Debug.Assert(pStream2 != null);

            m_p.GetStreamByNumber(0, out pStream3);
            Debug.Assert(pStream2 == pStream3);

            IMFASFProfile p2;

            m_p.Clone(out p2);

            Debug.Assert(p2 != null);
            p2.GetStreamCount(out i);
            p2.RemoveStream(0);
            p2.GetStreamCount(out i2);

            Debug.Assert(i2 == i - 1);

        }

        private void TestExclusion()
        {
            int i, i2;
            IMFASFMutualExclusion me, me2;

            m_p.CreateMutualExclusion(out me);

            Debug.Assert(me != null);

            m_p.AddMutualExclusion(me);
            m_p.GetMutualExclusionCount(out i);
            Debug.Assert(i == 1);
            m_p.GetMutualExclusion(0, out me2);
            Debug.Assert(me2 != null);

            m_p.RemoveMutualExclusion(0);
            m_p.GetMutualExclusionCount(out i2);
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

            m_p.GetMutualExclusionCount(out i);
            Debug.Assert(i == 0);

            m_p.CreateMutualExclusion(out pm);
            m_p.AddMutualExclusion(pm);
            m_p.GetMutualExclusionCount(out i);
            Debug.Assert(i == 1);

            m_p.GetMutualExclusion(0, out pm2);
            Debug.Assert(pm2 != null);

            m_p.RemoveMutualExclusion(0);
            m_p.GetMutualExclusionCount(out i);
            Debug.Assert(i == 0);
        }

        private void GetInterface()
        {
            MFExtern.MFCreateASFProfile(out m_p);
        }
    }
}
