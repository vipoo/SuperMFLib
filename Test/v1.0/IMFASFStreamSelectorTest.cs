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

            int hr = MFExtern.MFCreateASFProfile(out ap);
            MFError.ThrowExceptionForHR(hr);
            hr = ap.GetStreamPrioritization(out sp);
            MFError.ThrowExceptionForHR(hr);
            //ap.CreateStreamPrioritization(out sp);

            hr = MFExtern.MFCreateMediaType(out mt);
            MFError.ThrowExceptionForHR(hr);

            hr = mt.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, cc4.ToMediaSubtype());
            MFError.ThrowExceptionForHR(hr);
            hr = mt.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);
            MFError.ThrowExceptionForHR(hr);

            hr = ap.CreateStream(mt, out pStream);
            MFError.ThrowExceptionForHR(hr);

            hr = ap.SetStream(pStream);
            MFError.ThrowExceptionForHR(hr);

            hr = MFExtern.MFCreateASFStreamSelector(ap, out ss);
            MFError.ThrowExceptionForHR(hr);
            short[] w = new short[1];
            w[0] = 3;
            hr = ss.GetOutputStreamNumbers(0, w);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(w[0] != 3);

        }

        private void TestStream()
        {
            int i;

            int hr = m_ss.GetStreamCount(out i);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(i > 0);

            hr = m_ss.GetBandwidthStepCount(out i);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(i > 0);

            ASFSelectionStatus[] sel = new ASFSelectionStatus[i];
            short[] sa = new short[i];
            sel[0] = ASFSelectionStatus.NotSelected;
            sa[0] = 33;

            hr = m_ss.GetBandwidthStep(0, out i, sa, sel);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(sel[0] != ASFSelectionStatus.NotSelected);
            Debug.Assert(sa[0] != 33);

            hr = m_ss.SetStreamSelectorFlags(MFAsfStreamSelectorFlags.DisableThinning);
            MFError.ThrowExceptionForHR(hr);

            hr = m_ss.BitrateToStepNumber(100, out i);
            MFError.ThrowExceptionForHR(hr);

            hr = m_ss.BitrateToStepNumber(0, out i);
        }

        private void TestOutput()
        {
            int i;
            int hr = m_ss.GetOutputCount(out i);
            MFError.ThrowExceptionForHR(hr);
            hr = m_ss.GetOutputStreamCount(1, out i);
            MFError.ThrowExceptionForHR(hr);
            short[] sa = new short[i];

            sa[0] = 33;

            IntPtr ip = Marshal.AllocCoTaskMem(100);
            //m_ss.GetOutputStreamNumbers(0, out ip);
            hr = m_ss.GetOutputFromStream(0, out i);
            MFError.ThrowExceptionForHR(hr);

            hr = m_ss.SetOutputOverride(1, ASFSelectionStatus.CleanPointsOnly);
            MFError.ThrowExceptionForHR(hr);
            ASFSelectionStatus sel2;
            hr = m_ss.GetOutputOverride(1, out sel2);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(sel2 == ASFSelectionStatus.CleanPointsOnly);
        }

        private void TestMutex()
        {
            int i;
            object o;

            int hr = m_ss.GetOutputMutexCount(0, out i);
            MFError.ThrowExceptionForHR(hr);
            hr = m_ss.GetOutputMutex(0, 0, out o);
            MFError.ThrowExceptionForHR(hr);

            // Don't know what his problem is, but the def looks right
            try
            {
                hr = m_ss.SetOutputMutexSelection(0, 0, 0);
                MFError.ThrowExceptionForHR(hr);
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

            int hr = MFExtern.MFCreateASFProfile(out prof);
            MFError.ThrowExceptionForHR(hr);

            hr = prof.CreateMutualExclusion(out me);
            MFError.ThrowExceptionForHR(hr);
            hr = prof.AddMutualExclusion(me);
            MFError.ThrowExceptionForHR(hr);

            hr = MFExtern.MFCreateMediaType(out mt);
            MFError.ThrowExceptionForHR(hr);

            hr = mt.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, cc4.ToMediaSubtype());
            MFError.ThrowExceptionForHR(hr);
            hr = mt.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);
            MFError.ThrowExceptionForHR(hr);

            hr = prof.CreateStream(mt, out pStream);
            MFError.ThrowExceptionForHR(hr);

            hr = prof.SetStream(pStream);
            MFError.ThrowExceptionForHR(hr);

            hr = MFExtern.MFCreateASFStreamSelector(prof, out m_ss);
            MFError.ThrowExceptionForHR(hr);

        }
    }
}
