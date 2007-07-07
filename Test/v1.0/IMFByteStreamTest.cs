using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

using MediaFoundation;
using MediaFoundation.Utils;
using MediaFoundation.Misc;

namespace Testv10
{
    [ComVisible(true)]
    class IMFByteStreamTest : IMFAsyncCallback
    {
        IMFByteStream m_bs;
        AutoResetEvent m_mre = new AutoResetEvent(false);
        bool m_write = false;

        public void DoTests()
        {
            GetInterface();

            TestGetCapabilities();
            TestGetLength();
            TestGetCurrentPosition();
            TestIsEndOfStream();
            TestRead();
            TestSeek();
            TestBeginRead();
            TestClose();

            TestSetLength();
            TestWrite();
            TestBeginWrite();
            TestFlush();
        }

        void TestGetCapabilities()
        {
            MFByteStreamCapabilities pcap;

            m_bs.GetCapabilities(out pcap);

            Debug.Assert(pcap == (MFByteStreamCapabilities.IsSeekable | MFByteStreamCapabilities.IsReadable));
        }

        void TestGetLength()
        {
            long l;

            m_bs.GetLength(out l);

            Debug.Assert(l == 2163028);
        }

        void TestSetLength()
        {
            MFObjectType pObjectType;
            object pSource;
            IMFSourceResolver sr;

            int hr = MFDll.MFCreateSourceResolver(out sr);
            MFError.ThrowExceptionForHR(hr);

            sr.CreateObjectFromURL(
                @"file://C:\sourceforge\mflib\Test\v1.0\test.wmv",
                MFResolution.ByteStream | MFResolution.Write,
                null,
                out pObjectType,
                out pSource);

            m_bs = pSource as IMFByteStream;

            m_bs.SetLength(100);
            m_write = true;

        }

        void TestGetCurrentPosition()
        {
            long l;

            m_bs.SetCurrentPosition(1);
            m_bs.GetCurrentPosition(out l);

            Debug.Assert(l == 1);
        }

        void TestIsEndOfStream()
        {
            bool b;

            m_bs.IsEndOfStream(out b);

            Debug.Assert(b == false);
        }

        void TestRead()
        {
            int iReq = 32;
            int iRead;
            byte [] b = new byte[iReq];

            m_bs.Read(b, iReq, out iRead);
        }

        void TestBeginRead()
        {
            int iReq = 32;
            byte [] b = new byte[iReq];

            m_bs.BeginRead(b, iReq, this, this);
            m_mre.WaitOne(-1, true);
        }

        void TestWrite()
        {
            int iWrote;
            int iReq = 32;
            byte [] b = new byte[iReq];
            m_bs.Write(b, iReq, out iWrote);

            Debug.Assert(iWrote == iReq);
        }

        void TestBeginWrite()
        {
            int iReq = 32;
            byte[] b = new byte[iReq];
            m_bs.BeginWrite(b, iReq, this, this);

            m_mre.WaitOne(-1, true);
        }

        void TestSeek()
        {
            long l;
            m_bs.Seek(MFByteStreamSeekOrigin.Current, -32, MFByteStreamSeekingFlags.None, out l);

            Debug.Assert(l == 1);
        }

        void TestFlush()
        {
            m_bs.Flush();
        }

        void TestClose()
        {
            m_bs.Close();
        }


        private void GetInterface()
        {
            MFObjectType pObjectType;
            object pSource;
            IMFSourceResolver sr;

            int hr = MFDll.MFCreateSourceResolver(out sr);
            MFError.ThrowExceptionForHR(hr);

            sr.CreateObjectFromURL(
                @"file://c:/sourceforge/mflib/test/media/AspectRatio4x3.wmv",
                MFResolution.ByteStream,
                null,
                out pObjectType,
                out pSource);

            m_bs = pSource as IMFByteStream;

        }

        #region IMFAsyncCallback Members

        public void GetParameters(out MFASync pdwFlags, out MFAsyncCallbackQueue pdwQueue)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Invoke(IMFAsyncResult pAsyncResult)
        {
            int i;
            object o;
            IntPtr ip = Marshal.AllocCoTaskMem(8);

            pAsyncResult.GetState(out o);
            Debug.Assert(o == this);

            ip = pAsyncResult.GetStateNoAddRef();
            o = Marshal.GetObjectForIUnknown(ip);
            Debug.Assert(o == this);

            pAsyncResult.SetStatus(-1);
            int hr = pAsyncResult.GetStatus();
            Debug.Assert(hr == -1);

            try
            {
                // Since the IMFAsyncResult was created with no
                pAsyncResult.GetObject(out o);
            }
            catch (Exception e)
            {
                Debug.Assert(e is NullReferenceException);
            }


            if (!m_write)
            {
                m_bs.EndRead(pAsyncResult, out i);

                Debug.Assert(i == 32);
            }
            else
            {
                m_bs.EndWrite(pAsyncResult, out i);

                Debug.Assert(i == 32);
            }

            m_mre.Set();
        }

        #endregion
    }
}
