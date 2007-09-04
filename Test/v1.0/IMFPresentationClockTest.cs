using System;
using System.Collections.Generic;
using System.Text;

namespace Testv10
{
    class IMFPresentationClockTest
    {
        // Tested by adding this code to BasicPlayer's pause

#if false
            IMFPresentationClock m_pc;
            long lTime;
            IMFPresentationTimeSource pts;
            IMFClockStateSink pSink = new Test();
            IMFClock m_c;
            m_pSession.GetClock(out m_c);

            m_pc = m_c as IMFPresentationClock;
            m_pc.AddClockStateSink(pSink);
            m_pc.GetTime(out lTime);
            m_pc.GetTimeSource(out pts);
            m_pc.SetTimeSource(pts);
            m_pc.Start(0x7fffffffffffffff);
            m_pc.Pause();
            m_pc.Start(lTime);
            m_pc.Stop();
            m_pc.RemoveClockStateSink(pSink);

            m_state = PlayerState.PausePending;
            NotifyState();

class Test : COMBase, IMFClockStateSink
{
    public int iVal = 0;

        #region IMFClockStateSink Members

    public void OnClockStart(long hnsSystemTime, long llClockStartOffset)
    {
        iVal |= 1;
    }

    public void OnClockStop(long hnsSystemTime)
    {
        iVal |= 2;
    }

    public void OnClockPause(long hnsSystemTime)
    {
        iVal |= 4;
    }

    public void OnClockRestart(long hnsSystemTime)
    {
        iVal |= 8;
    }

    public void OnClockSetRate(long hnsSystemTime, float flRate)
    {
        iVal |= 16;
    }

        #endregion
}

#endif

    }
}
