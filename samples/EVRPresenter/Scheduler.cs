/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released
as public domain.  It is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
*****************************************************************************/

using System;
using System.Collections;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;

using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.Utility;

namespace EVRPresenter
{
    public class Scheduler : COMBase
    {
        const int SCHEDULER_TIMEOUT = 5000;
        const int WM_USER = 0x0400;

        [DllImport("Winmm.dll", PreserveSig = false, ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        protected extern static int timeBeginPeriod(int uPeriod);

        [DllImport("Winmm.dll", PreserveSig = false, ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        protected extern static int timeEndPeriod(int uPeriod);

        protected enum ScheduleEvent
        {
            eTerminate = WM_USER,
            eSchedule = WM_USER + 1,
            eFlush = WM_USER + 2
        }

        #region Member variables

        private Queue m_ScheduledSamples;

        private IMFClock m_pClock;  // Presentation clock. Can be null.
        private D3DPresentEngine m_pCB;

        private Queue m_EventQueue;
        private bool m_bSchedulerThread;
        private AutoResetEvent m_hThreadReadyEvent;
        private AutoResetEvent m_hFlushEvent;
        private AutoResetEvent m_hMsgEvent;

        private float m_fRate;                // Playback rate.
        private long m_PerFrameInterval;     // Duration of each frame.
        private long m_PerFrame_1_4th;       // 1/4th of the frame duration.
        private long m_LastSampleTime;       // Most recent sample time.

        #endregion

        public Scheduler(int iMaxSamples, D3DPresentEngine pCB)
        {
            if (pCB == null)
            {
                throw new COMException("Null D3DPresentEngine", MFError.MF_E_NOT_INITIALIZED);
            }

            m_ScheduledSamples = new Queue(iMaxSamples);
            m_EventQueue = new Queue(iMaxSamples * 2);

            m_pCB = pCB;
            m_pClock = null;
            m_bSchedulerThread = false;
            m_hThreadReadyEvent = null;
            m_hFlushEvent = new AutoResetEvent(false);
            m_hMsgEvent = new AutoResetEvent(false);

            m_fRate = 1.0f;
            m_LastSampleTime = 0;
            m_PerFrameInterval = 0;
            m_PerFrame_1_4th = 0;
        }

        ~Scheduler()
        {
            Debug.Assert(m_bSchedulerThread == false);
            Debug.Assert(m_hThreadReadyEvent == null);

            m_pCB = null;
            m_pClock = null;
            //SafeRelease(m_pClock);  // Released elsewhere

            if (m_hFlushEvent != null)
            {
                m_hFlushEvent.Close();
                m_hFlushEvent = null;
            }

            if (m_hMsgEvent != null)
            {
                m_hMsgEvent.Close();
                m_hMsgEvent = null;
            }

            if (m_EventQueue != null)
            {
                m_EventQueue.Clear();
                m_EventQueue = null;
            }
            if (m_ScheduledSamples != null)
            {
                m_ScheduledSamples.Clear();
            }
        }

        public void SetFrameRate(MFRatio fps)
        {
            // Convert to a duration.
            MFExtern.MFFrameRateToAverageTimePerFrame(fps.Numerator, fps.Denominator, out m_PerFrameInterval);

            // Calculate 1/4th of this value, because we use it frequently.
            m_PerFrame_1_4th = m_PerFrameInterval / 4;
        }

        public void SetClockRate(float fRate)
        {
            m_fRate = fRate;
        }

        public long LastSampleTime()
        {
            return m_LastSampleTime;
        }

        public long FrameDuration()
        {
            return m_PerFrameInterval;
        }

        public void StartScheduler(IMFClock pClock)
        {
            if (m_bSchedulerThread != false)
            {
                throw new COMException("Scheduler already started", E_Unexpected);
            }

            m_pClock = pClock;

            // Set a high the timer resolution (ie, short timer period).
            timeBeginPeriod(1);

            // Create an event to wait for the thread to start.
            m_hThreadReadyEvent = new AutoResetEvent(false);

            try
            {
                // Use the c# threadpool to avoid creating a thread
                // when streaming begins
                ThreadPool.QueueUserWorkItem(new WaitCallback(SchedulerThreadProcPrivate));

                m_hThreadReadyEvent.WaitOne(SCHEDULER_TIMEOUT, false);
                m_bSchedulerThread = true;
            }
            finally
            {
                // Regardless success/failure, we are done using the "thread ready" event.
                m_hThreadReadyEvent.Close();
                m_hThreadReadyEvent = null;
            }
        }

        public void StopScheduler()
        {
            if (m_bSchedulerThread == false)
            {
                return;
            }

            // Ask the scheduler thread to exit.
            lock (m_ScheduledSamples)
            {
                m_EventQueue.Enqueue(ScheduleEvent.eTerminate);
            }
            m_hMsgEvent.Set();

            // Close handles.
            m_bSchedulerThread = false;

            // Discard samples.
            lock (m_ScheduledSamples)
            {
                m_ScheduledSamples.Clear();
            }

            // Restore the timer resolution.
            timeEndPeriod(1);
        }

        public void ScheduleSample(IMFSample pSample, bool bPresentNow)
        {
            if (m_bSchedulerThread == false)
            {
                throw new COMException("Scheduler thread not started", MFError.MF_E_NOT_INITIALIZED);
            }

            if (bPresentNow || (m_pClock == null))
            {
                // Present the sample immediately.
                m_pCB.PresentSample(pSample, 0);
                SafeRelease(pSample);
            }
            else
            {
                // Queue the sample and ask the scheduler thread to wake up.
                lock (m_ScheduledSamples)
                {
                    m_ScheduledSamples.Enqueue(pSample);
                }

                lock (m_ScheduledSamples)
                {
                    m_EventQueue.Enqueue(ScheduleEvent.eSchedule);
                }
                m_hMsgEvent.Set();
            }
        }

        public void ProcessSamplesInQueue(out int plNextSleep)
        {
            plNextSleep = 0;
            IMFSample pSample = null;

            // Process samples until the queue is empty or until the wait time > 0.
            while (m_ScheduledSamples.Count > 0)
            {
                lock (m_ScheduledSamples)
                {
                    pSample = (IMFSample)m_ScheduledSamples.Peek();
                }

                // Process the next sample in the queue. If the sample is not ready
                // for presentation. the value returned in lWait is > 0, which
                // means the scheduler should sleep for that amount of time.

                if (ProcessSample(pSample, out plNextSleep))
                {
                    object o;

                    lock (m_ScheduledSamples)
                    {
                        o = m_ScheduledSamples.Dequeue();
                    }
                    SafeRelease(o);
                }

                if (plNextSleep > 0)
                {
                    break;
                }
            }

            // If the wait time is zero, it means we stopped because the queue is
            // empty (or an error occurred). Set the wait time to infinite; this will
            // make the scheduler thread sleep until it gets another thread message.
            if (plNextSleep == 0)
            {
                plNextSleep = Timeout.Infinite;
            }
        }

        public bool ProcessSample(IMFSample pSample, out int plNextSleep)
        {
            long hnsPresentationTime = 0;
            long hnsTimeNow = 0;
            long hnsSystemTime = 0;

            bool bPresentNow = true;
            plNextSleep = 0;

            if (m_pClock != null)
            {
                // Get the sample's time stamp. It is valid for a sample to
                // have no time stamp.

                try
                {
                    pSample.GetSampleTime(out hnsPresentationTime);

                    // Get the clock time. (But if the sample does not have a time stamp,
                    // we don't need the clock time.)
                    m_pClock.GetCorrelatedTime(0, out hnsTimeNow, out hnsSystemTime);
                }
                catch { }

                // Calculate the time until the sample's presentation time.
                // A negative value means the sample is late.
                long hnsDelta = hnsPresentationTime - hnsTimeNow;
                if (m_fRate < 0)
                {
                    // For reverse playback, the clock runs backward. Therefore the delta is reversed.
                    hnsDelta = -hnsDelta;
                }

                if (hnsDelta < -m_PerFrame_1_4th)
                {
                    // This sample is late.
                    bPresentNow = true;
                }
                else if (hnsDelta > (3 * m_PerFrame_1_4th))
                {
                    // This sample is still too early. Go to sleep.
                    plNextSleep = Utils.MFTimeToMsec(hnsDelta - (3 * m_PerFrame_1_4th));

                    // Adjust the sleep time for the clock rate. (The presentation clock runs
                    // at m_fRate, but sleeping uses the system clock.)
                    plNextSleep = (int)(plNextSleep / Math.Abs(m_fRate));

                    // Don't present yet.
                    bPresentNow = false;
                }
            }

            if (bPresentNow)
            {
                m_pCB.PresentSample(pSample, hnsPresentationTime);
                // pSample released by caller along with DeQueue
            }

            return bPresentNow;
        }

        public void Flush()
        {
            TRACE(("Scheduler::Flush"));

            if (m_bSchedulerThread != false)
            {
                // Ask the scheduler thread to flush.
                lock (m_ScheduledSamples)
                {
                    m_EventQueue.Enqueue(ScheduleEvent.eFlush);
                }
                m_hMsgEvent.Set();

                // Wait for the scheduler thread to signal the flush event,
                m_hFlushEvent.WaitOne(SCHEDULER_TIMEOUT, false);

                TRACE(("Scheduler::Flush completed."));
            }
        }

        private void SchedulerThreadProcPrivate(object state)
        {
            const int INFINITE = -1;
            int lWait = INFINITE;
            bool bExitThread = false;

            // Signal to the scheduler that the thread is ready.
            m_hThreadReadyEvent.Set();

            while (!bExitThread)
            {
                m_hMsgEvent.WaitOne(lWait, false);

                if (m_EventQueue.Count == 0 && lWait != INFINITE)
                {
                    try
                    {
                        // We haven't received any new messages, but we
                        // were waiting for the right time to show existing
                        // samples to arrive.  It has.
                        ProcessSamplesInQueue(out lWait);
                    }
                    catch
                    {
                        bExitThread = true;
                    }
                }

                while (m_EventQueue.Count > 0)
                {
                    ScheduleEvent se;

                    lock (m_ScheduledSamples)
                    {
                        se = (ScheduleEvent)m_EventQueue.Dequeue();
                    }
                    bool bProcessSamples = true;

                    //Debug.WriteLine(se);

                    switch (se)
                    {
                        case ScheduleEvent.eTerminate:
                            bExitThread = true;
                            break;

                        case ScheduleEvent.eFlush:
                            // Flushing: Clear the sample queue and set the event.
                            lock (m_ScheduledSamples)
                            {
                                m_ScheduledSamples.Clear();
                            }
                            lWait = INFINITE;
                            m_hFlushEvent.Set();
                            break;

                        case ScheduleEvent.eSchedule:
                            // Process as many samples as we can.
                            if (bProcessSamples)
                            {
                                try
                                {
                                    ProcessSamplesInQueue(out lWait);
                                }
                                catch
                                {
                                    bExitThread = true;
                                }
                                bProcessSamples = (lWait != INFINITE);
                            }
                            break;
                    } // switch

                }

            }  // while (!bExitThread)

            TRACE(("Exit scheduler thread."));
        }
    }
}
