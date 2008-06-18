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
using System.Runtime.InteropServices;
using System.Windows.Forms;

using MediaFoundation;
using MediaFoundation.Misc;

namespace EVRPresenter
{
    public class Scheduler : COMBase
    {
        const int WM_USER = 0x0400;

        enum ScheduleEvent
        {
            eTerminate = WM_USER,
            eSchedule = WM_USER + 1,
            eFlush = WM_USER + 2
        };

        const int SCHEDULER_TIMEOUT = 5000;

        public Scheduler()
        {
            m_pCB = null;
            m_pClock = null;
            m_dwThreadID = 0;
            m_hSchedulerThread = null;
            m_hThreadReadyEvent = null;
            m_hFlushEvent = null;
            m_fRate = 1.0f;
            m_LastSampleTime = 0;
            m_PerFrameInterval = 0;
            m_PerFrame_1_4th = 0;
        }

        ~Scheduler()
        {
            SafeRelease(m_pClock);
        }

        public void SetCallback(D3DPresentEngine pCB)
        {
            m_pCB = pCB;
        }

        public void SetFrameRate(MFRatio fps)
        {
            Int64 AvgTimePerFrame = 0;

            // Convert to a duration.
            MFExtern.MFFrameRateToAverageTimePerFrame(fps.Numerator, fps.Denominator, out AvgTimePerFrame);

            m_PerFrameInterval = (Int64)AvgTimePerFrame;

            // Calculate 1/4th of this value, because we use it frequently.
            m_PerFrame_1_4th = m_PerFrameInterval / 4;
        }

        public void SetClockRate(float fRate)
        {
            m_fRate = fRate;
        }

        public Int64 LastSampleTime()
        {
            return m_LastSampleTime;
        }

        public Int64 FrameDuration()
        {
            return m_PerFrameInterval;
        }

        public void StartScheduler(IMFClock pClock)
        {
            if (m_hSchedulerThread != null)
            {
                throw new COMException("Scheduler::StartScheduler", E_Unexpected);
            }

            int dwID = 0;

            m_pClock = pClock;

            // Set a high the timer resolution (ie, short timer period).
            Winmm.timeBeginPeriod(1);

            // Create an event to wait for the thread to start.
            m_hThreadReadyEvent = new AutoResetEvent(false);

            // Create an event to wait for flush commands to complete.
            m_hFlushEvent = new AutoResetEvent(false);

            // Create the scheduler thread.
            //m_hSchedulerThread = CreateThread(null, 0, SchedulerThreadProc, (LPVOID)this, 0, &dwID);
            m_hSchedulerThread = new Thread(new ThreadStart(this.SchedulerThreadProcPrivate));
            m_dwThreadID = m_hSchedulerThread.ManagedThreadId;

            do
            {
            } while (!m_hThreadReadyEvent.WaitOne(5000, false) && m_hSchedulerThread.IsAlive);

            if (!m_hSchedulerThread.IsAlive)
            {
                m_hSchedulerThread = null;
                throw new COMException("StartScheduler", E_Unexpected);
            }

            m_dwThreadID = dwID;

            // Regardless success/failure, we are done using the "thread ready" event.
            if (m_hThreadReadyEvent != null)
            {
                m_hThreadReadyEvent.Close();
                m_hThreadReadyEvent = null;
            }
        }

        public void StopScheduler()
        {
            if (m_hSchedulerThread == null)
            {
                return;
            }

            // Ask the scheduler thread to exit.
            Extern.PostThreadMessage(m_dwThreadID, (int)ScheduleEvent.eTerminate, IntPtr.Zero, IntPtr.Zero);

            // Wait for the thread to exit.
            while (m_hSchedulerThread.IsAlive)
            {
                System.Threading.Thread.Sleep(1);
            }

            // Close handles.
            //CloseHandle(m_hSchedulerThread);
            m_hSchedulerThread = null;

            m_hFlushEvent.Close();
            m_hFlushEvent = null;

            // Discard samples.
            m_ScheduledSamples.Clear();

            // Restore the timer resolution.
            Winmm.timeEndPeriod(1);
        }

        public void ScheduleSample(IMFSample pSample, bool bPresentNow)
        {
            if (m_pCB == null)
            {
                throw new COMException("Scheduler::ScheduleSample", MFError.MF_E_NOT_INITIALIZED);
            }

            if (m_hSchedulerThread == null)
            {
                throw new COMException("Scheduler::ScheduleSample 2", MFError.MF_E_NOT_INITIALIZED);
            }

            if (!m_hSchedulerThread.IsAlive)
            {
                throw new COMException("Scheduler::ScheduleSample", E_Fail);
            }

            if (bPresentNow || (m_pClock == null))
            {
                // Present the sample immediately.
                m_pCB.PresentSample(pSample, 0);
            }
            else
            {
                // Queue the sample and ask the scheduler thread to wake up.
                m_ScheduledSamples.Enqueue(pSample);

                Extern.PostThreadMessage(m_dwThreadID, (int)ScheduleEvent.eSchedule, IntPtr.Zero, IntPtr.Zero);
            }
        }

        public void ProcessSamplesInQueue(out int plNextSleep)
        {
            int lWait = 0;
            IMFSample pSample = null;

            // Process samples until the queue is empty or until the wait time > 0.

            // Note: Dequeue returns S_FALSE when the queue is empty.
            while (m_ScheduledSamples.Count > 0)
            {
                pSample = (IMFSample)m_ScheduledSamples.Dequeue();
                // Process the next sample in the queue. If the sample is not ready
                // for presentation. the value returned in lWait is > 0, which
                // means the scheduler should sleep for that amount of time.

                ProcessSample(pSample, out lWait);
                SafeRelease(pSample);

                if (lWait > 0)
                {
                    break;
                }
            }

            // If the wait time is zero, it means we stopped because the queue is
            // empty (or an error occurred). Set the wait time to infinite; this will
            // make the scheduler thread sleep until it gets another thread message.
            if (lWait == 0)
            {
                lWait = Timeout.Infinite;
            }

            plNextSleep = lWait;
        }
        public void ProcessSample(IMFSample pSample, out int plNextSleep)
        {
            Int64 hnsPresentationTime = 0;
            Int64 hnsTimeNow = 0;
            Int64 hnsSystemTime = 0;

            bool bPresentNow = true;
            int lNextSleep = 0;

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
                Int64 hnsDelta = hnsPresentationTime - hnsTimeNow;
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
                    lNextSleep = Utils.MFTimeToMsec(hnsDelta - (3 * m_PerFrame_1_4th));

                    // Adjust the sleep time for the clock rate. (The presentation clock runs
                    // at m_fRate, but sleeping uses the system clock.)
                    lNextSleep = (int)(lNextSleep / Math.Abs(m_fRate));

                    // Don't present yet.
                    bPresentNow = false;
                }
            }

            if (bPresentNow)
            {
                m_pCB.PresentSample(pSample, hnsPresentationTime);
            }
            else
            {
                // The sample is not ready yet. Return it to the queue.
                m_ScheduledSamples.Enqueue(pSample);
            }

            plNextSleep = lNextSleep;
        }

        public void Flush()
        {
            TRACE(("Scheduler::Flush"));

            if (m_hSchedulerThread == null)
            {
                TRACE(("No scheduler thread!"));
            }

            if (m_hSchedulerThread != null)
            {
                // Ask the scheduler thread to flush.
                Extern.PostThreadMessage(m_dwThreadID, (int)ScheduleEvent.eFlush, IntPtr.Zero, IntPtr.Zero);

                // Wait for the scheduler thread to signal the flush event,
                // OR for the thread to terminate.

                do
                {
                } while (!m_hFlushEvent.WaitOne(5000, false) && (m_hSchedulerThread.IsAlive));

                TRACE(("Scheduler::Flush completed."));
            }
        }

        // non-static version of SchedulerThreadProc.
        private void SchedulerThreadProcPrivate()
        {
            const int INFINITE = -1;
            Message msg;
            int lWait = INFINITE;
            bool bExitThread = false;

            // Force the system to create a message queue for this thread.
            // (See MSDN documentation for PostThreadMessage.)
            Extern.PeekMessage(out msg, IntPtr.Zero, WM_USER, WM_USER, PeekFlags.NOREMOVE);

            // Signal to the scheduler that the thread is ready.
            m_hThreadReadyEvent.Set();

            while (!bExitThread)
            {
                // Wait for a thread message OR until the wait time expires.
                int dwResult = Extern.MsgWaitForMultipleObjects(0, null, false, lWait, WakeMask.POSTMESSAGE);

                if (dwResult == 258)
                {
                    // If we timed out, then process the samples in the queue
                    try
                    {
                        ProcessSamplesInQueue(out lWait);
                    }
                    catch
                    {
                        bExitThread = true;
                    }
                }

                while (Extern.PeekMessage(out msg, IntPtr.Zero, 0, 0, PeekFlags.REMOVE))
                {
                    bool bProcessSamples = true;

                    switch (msg.Msg)
                    {
                        case (int)ScheduleEvent.eTerminate:
                            TRACE(("eTerminate"));
                            bExitThread = true;
                            break;

                        case (int)ScheduleEvent.eFlush:
                            // Flushing: Clear the sample queue and set the event.
                            m_ScheduledSamples.Clear();
                            lWait = INFINITE;
                            m_hFlushEvent.Set();
                            break;

                        case (int)ScheduleEvent.eSchedule:
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

                } // while PeekMessage

            }  // while (!bExitThread)

            TRACE(("Exit scheduler thread."));
            //return (SUCCEEDED(hr) ? 0 : 1);
        }


        //private ThreadSafeQueue<IMFSample> m_ScheduledSamples;		// Samples waiting to be presented.
        Queue m_ScheduledSamples = new Queue();

        private IMFClock m_pClock;  // Presentation clock. Can be null.
        private D3DPresentEngine m_pCB;     // Weak reference; do not delete.

        private int m_dwThreadID;
        private Thread m_hSchedulerThread;
        private AutoResetEvent m_hThreadReadyEvent;
        private AutoResetEvent m_hFlushEvent;

        private float m_fRate;                // Playback rate.
        private Int64 m_PerFrameInterval;     // Duration of each frame.
        private Int64 m_PerFrame_1_4th;       // 1/4th of the frame duration.
        private Int64 m_LastSampleTime;       // Most recent sample time.
    }
}
