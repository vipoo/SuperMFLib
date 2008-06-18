/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/

using System;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;

using MediaFoundation;
using MediaFoundation.Misc;

namespace EVRPresenter
{
    public class SamplePool
    {
        Queue m_VideoSampleQueue;			// Available queue

        bool m_bInitialized;
        int m_cPending;

        public SamplePool()
        {
            m_VideoSampleQueue = new Queue();
            m_bInitialized = false;
            m_cPending = 0;
        }

        //-----------------------------------------------------------------------------
        // GetSample
        //
        // Gets a sample from the pool. If no samples are available, the method
        // returns MF_E_SAMPLEALLOCATOR_EMPTY.
        //-----------------------------------------------------------------------------

        public void GetSample(out IMFSample ppSample)
        {
            lock (this)
            {
                if (!m_bInitialized)
                {
                    throw new COMException("SamplePool::GetSample", MFError.MF_E_NOT_INITIALIZED);
                }

                if (m_VideoSampleQueue.Count == 0)
                {
                    throw new COMException("SamplePool::GetSample", MFError.MF_E_SAMPLEALLOCATOR_EMPTY);
                }

                // Get a sample from the allocated queue.

                // It doesn't matter if we pull them from the head or tail of the list,
                // but when we get it back, we want to re-insert it onto the opposite end.
                // (see ReturnSample)

                ppSample = (IMFSample)m_VideoSampleQueue.Dequeue();

                m_cPending++;
            }
        }

        //-----------------------------------------------------------------------------
        // ReturnSample
        //
        // Returns a sample to the pool.
        //-----------------------------------------------------------------------------

        public void ReturnSample(IMFSample pSample)
        {
            lock (this)
            {
                if (!m_bInitialized)
                {
                    throw new COMException("SamplePool::ReturnSample", MFError.MF_E_NOT_INITIALIZED);
                }

                m_VideoSampleQueue.Enqueue(pSample);

                m_cPending--;
            }
        }

        //-----------------------------------------------------------------------------
        // AreSamplesPending
        //
        // Returns TRUE if any samples are in use.
        //-----------------------------------------------------------------------------

        public bool AreSamplesPending()
        {
            bool bRet;

            lock (this)
            {
                if (!m_bInitialized)
                {
                    throw new COMException("SamplePool::AreSamplesPending", MFError.MF_E_NOT_INITIALIZED);
                }
                bRet = (m_cPending > 0);
            }

            return bRet;
        }


        //-----------------------------------------------------------------------------
        // Initialize
        //
        // Initializes the pool with a list of samples.
        //-----------------------------------------------------------------------------

        public void Initialize(Queue samples)
        {
            lock (this)
            {
                if (m_bInitialized)
                {
                    throw new COMException("SamplePool::Initialize", MFError.MF_E_INVALIDREQUEST);
                }

                // Move these samples into our allocated queue.
                while (samples.Count > 0)
                {
                    m_VideoSampleQueue.Enqueue(samples.Dequeue());
                }

                m_bInitialized = true;
            }
        }


        //-----------------------------------------------------------------------------
        // Clear
        //
        // Releases all samples.
        //-----------------------------------------------------------------------------

        public void Clear()
        {
            lock (this)
            {
                m_VideoSampleQueue.Clear();
                m_bInitialized = false;
                m_cPending = 0;
            }
        }

    }
}
