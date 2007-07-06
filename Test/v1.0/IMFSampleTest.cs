using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

using MediaFoundation;
using MediaFoundation.Utils;

namespace Testv10
{
    class IMFSampleTest
    {
        IMFSample m_ps;

        public void DoTests()
        {
            GetInterface();

            TestGetSampleFlags();
            TestGetSampleTime();
            TestGetSampleDuration();
            TestAddBuffer();
            TestGetBufferCount();
            TestGetBufferByIndex();
            TestGetTotalLength();
            TestCopyToBuffer();
            TestConvertToContiguousBuffer();
            TestRemoveBufferByIndex();
            TestRemoveAllBuffers();
        }

        private void TestGetSampleFlags()
        {
            int i;
            m_ps.SetSampleFlags(3);
            m_ps.GetSampleFlags(out i);

            Debug.Assert(i == 3);
        }

        private void TestGetSampleTime()
        {
            long l;

            m_ps.SetSampleTime(4);
            m_ps.GetSampleTime(out l);

            Debug.Assert(l == 4);
        }

        private void TestGetSampleDuration()
        {
            long l;

            m_ps.SetSampleDuration(5);
            m_ps.GetSampleDuration(out l);

            Debug.Assert(l == 5);
        }

        private void TestGetBufferCount()
        {
            int i;

            m_ps.GetBufferCount(out i);
            Debug.Assert(i == 1);
        }

        private void TestGetBufferByIndex()
        {
            IMFMediaBuffer pBuff;

            m_ps.GetBufferByIndex(0, out pBuff);
            Debug.Assert(pBuff != null);
        }

        private void TestConvertToContiguousBuffer()
        {
            IMFMediaBuffer pBuffer;

            m_ps.ConvertToContiguousBuffer(out pBuffer);

            Debug.Assert(pBuffer != null);
        }

        private void TestAddBuffer()
        {
            IMFMediaBuffer pBuff;

            int hr = MFDll.MFCreateMemoryBuffer(100, out pBuff);
            MFError.ThrowExceptionForHR(hr);

            pBuff.SetCurrentLength(17);

            m_ps.AddBuffer(pBuff);
        }

        private void TestRemoveBufferByIndex()
        {
            m_ps.RemoveBufferByIndex(0);
        }

        private void TestRemoveAllBuffers()
        {
            m_ps.RemoveAllBuffers();
        }

        private void TestGetTotalLength()
        {
            int i;
            m_ps.GetTotalLength(out i);

            Debug.Assert(i == 17);
        }

        private void TestCopyToBuffer()
        {
            IMFMediaBuffer pBuff;

            int hr = MFDll.MFCreateMemoryBuffer(17, out pBuff);
            MFError.ThrowExceptionForHR(hr);

            m_ps.CopyToBuffer(pBuff);
        }

        private void GetInterface()
        {
            int hr = MFDll.MFCreateSample(out m_ps);
            MFError.ThrowExceptionForHR(hr);
        }
    }
}
