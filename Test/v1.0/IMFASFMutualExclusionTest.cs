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

            m_ame.Clone(out ame);
            Debug.Assert(ame != null);
        }

        private void TestStream()
        {
            int iRec, i;
            short[] wa;

            m_ame.AddRecord(out iRec);
            m_ame.AddStreamForRecord(iRec, 17);
            m_ame.AddStreamForRecord(iRec, 18);

            i = 0;
            m_ame.GetStreamsForRecord(iRec, null, ref i);

            wa = new short[i];
            m_ame.GetStreamsForRecord(iRec, wa, ref i);

            Debug.Assert(i == 2 && wa[0] == 17 && wa[1] == 18);

            m_ame.RemoveStreamFromRecord(iRec, 17);
            m_ame.GetStreamsForRecord(iRec, wa, ref i);
            Debug.Assert(i == 1 && wa[0] == 18);
        }

        private void TestType()
        {
            Guid g;

            m_ame.GetType(out g);
            m_ame.SetType(g);
        }

        private void TestRecord()
        {
            int iRec, iCnt;

            m_ame.AddRecord(out iRec);
            m_ame.GetRecordCount(out iCnt);
            Debug.Assert(iCnt == 1);

            m_ame.RemoveRecord(iRec);
            m_ame.GetRecordCount(out iCnt);
            Debug.Assert(iCnt == 0);
        }

        private void GetInterface()
        {
            IMFASFProfile m_p;

            MFExtern.MFCreateASFProfile(out m_p);
            
            m_p.CreateMutualExclusion(out m_ame);
        }
    }
}
