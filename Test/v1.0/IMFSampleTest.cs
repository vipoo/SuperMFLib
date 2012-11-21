using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

using MediaFoundation;
using MediaFoundation.Misc;

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
            int hr = m_ps.SetSampleFlags(3);
            MFError.ThrowExceptionForHR(hr);
            hr = m_ps.GetSampleFlags(out i);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(i == 3);
        }

        private void TestGetSampleTime()
        {
            long l;

            int hr = m_ps.SetSampleTime(4);
            MFError.ThrowExceptionForHR(hr);
            hr = m_ps.GetSampleTime(out l);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(l == 4);
        }

        private void TestGetSampleDuration()
        {
            long l;

            int hr = m_ps.SetSampleDuration(5);
            MFError.ThrowExceptionForHR(hr);
            hr = m_ps.GetSampleDuration(out l);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(l == 5);
        }

        private void TestGetBufferCount()
        {
            int i;

            int hr = m_ps.GetBufferCount(out i);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(i == 1);
        }

        private void TestGetBufferByIndex()
        {
            IMFMediaBuffer pBuff;

            int hr = m_ps.GetBufferByIndex(0, out pBuff);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(pBuff != null);
        }

        private void TestConvertToContiguousBuffer()
        {
            IMFMediaBuffer pBuffer;

            int hr = m_ps.ConvertToContiguousBuffer(out pBuffer);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(pBuffer != null);
        }

        private void TestAddBuffer()
        {
            IMFMediaBuffer pBuff;

            int hr = MFExtern.MFCreateMemoryBuffer(100, out pBuff);
            MFError.ThrowExceptionForHR(hr);

            hr = pBuff.SetCurrentLength(17);
            MFError.ThrowExceptionForHR(hr);

            hr = m_ps.AddBuffer(pBuff);
            MFError.ThrowExceptionForHR(hr);
        }

        private void TestRemoveBufferByIndex()
        {
            int hr = m_ps.RemoveBufferByIndex(0);
            MFError.ThrowExceptionForHR(hr);
        }

        private void TestRemoveAllBuffers()
        {
            int hr = m_ps.RemoveAllBuffers();
            MFError.ThrowExceptionForHR(hr);
        }

        private void TestGetTotalLength()
        {
            int i;
            int hr = m_ps.GetTotalLength(out i);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(i == 17);
        }

        private void TestCopyToBuffer()
        {
            IMFMediaBuffer pBuff;

            int hr = MFExtern.MFCreateMemoryBuffer(17, out pBuff);
            MFError.ThrowExceptionForHR(hr);

            hr = m_ps.CopyToBuffer(pBuff);
            MFError.ThrowExceptionForHR(hr);
        }

        private void GetInterface()
        {
            int hr = MFExtern.MFCreateSample(out m_ps);
            MFError.ThrowExceptionForHR(hr);
        }
    }
}
