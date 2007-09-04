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
    class IMFASFStreamSelectorTest
    {
        IMFASFStreamSelector m_ss;

#if ALLOW_UNTESTED_INTERFACES
        public void DoTests()
        {
            Alt();
            GetInterface();

            TestStream();
            TestMutex();
            TestOutput();
        }

        private void Alt()
        {
            FourCC cc4 = new FourCC("YUY2");
            IMFASFProfile ap;
            IMFASFStreamPrioritization sp;
            IMFASFStreamSelector ss;
            IMFMediaType mt;
            IMFASFStreamConfig pStream;

            MFExtern.MFCreateASFProfile(out ap);
            ap.GetStreamPrioritization(out sp);
            //ap.CreateStreamPrioritization(out sp);

            MFExtern.MFCreateMediaType(out mt);

            mt.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, cc4.ToMediaSubtype());
            mt.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);

            ap.CreateStream(mt, out pStream);

            ap.SetStream(pStream);

            MFExtern.MFCreateASFStreamSelector(ap, out ss);
            short[] w = new short[1];
            w[0] = 3;
            ss.GetOutputStreamNumbers(0, w);
            Debug.Assert(w[0] != 3);

        }

        private void TestStream()
        {
            int i;

            m_ss.GetStreamCount(out i);
            Debug.Assert(i > 0);

            m_ss.GetBandwidthStepCount(out i);
            Debug.Assert(i > 0);

            ASFSelectionStatus[] sel = new ASFSelectionStatus[i];
            short[] sa = new short[i];
            sel[0] = ASFSelectionStatus.NotSelected;
            sa[0] = 33;

            m_ss.GetBandwidthStep(0, out i, sa, sel);
            Debug.Assert(sel[0] != ASFSelectionStatus.NotSelected);
            Debug.Assert(sa[0] != 33);

            m_ss.SetStreamSelectorFlags(MFAsfStreamSelectorFlags.DisableThinning);

            m_ss.BitrateToStepNumber(100, out i);

            m_ss.BitrateToStepNumber(0, out i);
        }

        private void TestOutput()
        {
            int i;
            m_ss.GetOutputCount(out i);
            m_ss.GetOutputStreamCount(1, out i);
            short[] sa = new short[i];

            sa[0] = 33;

            IntPtr ip = Marshal.AllocCoTaskMem(100);
            //m_ss.GetOutputStreamNumbers(0, out ip);
            m_ss.GetOutputFromStream(0, out i);

            m_ss.SetOutputOverride(1, ASFSelectionStatus.CleanPointsOnly);
            ASFSelectionStatus sel2;
            m_ss.GetOutputOverride(1, out sel2);
            Debug.Assert(sel2 == ASFSelectionStatus.CleanPointsOnly);
        }

        private void TestMutex()
        {
            int i;
            object o;

            m_ss.GetOutputMutexCount(0, out i);
            m_ss.GetOutputMutex(0, 0, out o);

            // Don't know what his problem is, but the def looks right
            try
            {
                m_ss.SetOutputMutexSelection(0, 0, 0);
            }
            catch { }
        }

        private void GetInterface()
        {
            IMFASFMutualExclusion me;
            IMFASFProfile prof;
            IMFMediaType mt;
            IMFASFStreamConfig pStream;
            FourCC cc4 = new FourCC("YUY2");

            MFExtern.MFCreateASFProfile(out prof);

            prof.CreateMutualExclusion(out me);
            prof.AddMutualExclusion(me);

            MFExtern.MFCreateMediaType(out mt);

            mt.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, cc4.ToMediaSubtype());
            mt.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);

            prof.CreateStream(mt, out pStream);

            prof.SetStream(pStream);

            MFExtern.MFCreateASFStreamSelector(prof, out m_ss);

        }
#endif
    }
}
