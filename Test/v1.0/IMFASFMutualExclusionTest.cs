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
    class IMFASFMutualExclusionTest
    {
        private IMFASFMutualExclusion m_ame;

        public void DoTests()
        {
            GetInterface();

            TestType();
            TestRecord();
            TestStream();
            TestMisc();
        }

        private void TestMisc()
        {
            IMFASFMutualExclusion ame;

            int hr = m_ame.Clone(out ame);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(ame != null);
        }

        private void TestStream()
        {
            int iRec, i;
            short[] wa;

            int hr = m_ame.AddRecord(out iRec);
            MFError.ThrowExceptionForHR(hr);
            hr = m_ame.AddStreamForRecord(iRec, 17);
            MFError.ThrowExceptionForHR(hr);
            hr = m_ame.AddStreamForRecord(iRec, 18);

            i = 0;
            hr = m_ame.GetStreamsForRecord(iRec, null, ref i);
            MFError.ThrowExceptionForHR(hr);

            wa = new short[i];
            hr = m_ame.GetStreamsForRecord(iRec, wa, ref i);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(i == 2 && wa[0] == 17 && wa[1] == 18);

            hr = m_ame.RemoveStreamFromRecord(iRec, 17);
            MFError.ThrowExceptionForHR(hr);
            hr = m_ame.GetStreamsForRecord(iRec, wa, ref i);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(i == 1 && wa[0] == 18);
        }

        private void TestType()
        {
            Guid g;

            int hr = m_ame.GetType(out g);
            MFError.ThrowExceptionForHR(hr);
            hr = m_ame.SetType(g);
            MFError.ThrowExceptionForHR(hr);
        }

        private void TestRecord()
        {
            int iRec, iCnt;

            int hr = m_ame.AddRecord(out iRec);
            MFError.ThrowExceptionForHR(hr);
            hr = m_ame.GetRecordCount(out iCnt);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(iCnt == 1);

            hr = m_ame.RemoveRecord(iRec);
            MFError.ThrowExceptionForHR(hr);
            hr = m_ame.GetRecordCount(out iCnt);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(iCnt == 0);
        }

        private void GetInterface()
        {
            IMFASFProfile m_p;

            int hr = MFExtern.MFCreateASFProfile(out m_p);
            MFError.ThrowExceptionForHR(hr);
            
            hr = m_p.CreateMutualExclusion(out m_ame);
            MFError.ThrowExceptionForHR(hr);
        }
    }
}
