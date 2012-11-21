using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

using MediaFoundation;
using MediaFoundation.Misc;
using System.IO;

namespace Testv10
{
    [ComVisible(true)]
    class IMFByteStreamTest : COMBase, IMFAsyncCallback
    {
        const string path = @"C:\sourceforge\mflib\Test\v1.0\test.wmv";

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
            TestClose();

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        void TestGetCapabilities()
        {
            MFByteStreamCapabilities pcap;

            int hr = m_bs.GetCapabilities(out pcap);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(pcap == (MFByteStreamCapabilities.IsSeekable | MFByteStreamCapabilities.IsReadable));
        }

        void TestGetLength()
        {
            long l;

            int hr = m_bs.GetLength(out l);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(l == 2163028);
        }

        void TestSetLength()
        {
            MFObjectType pObjectType;
            object pSource;
            IMFSourceResolver sr;
            int hr;

            if (File.Exists(path))
            {
                File.Delete(path);
            }
            using (FileStream fs = File.Create(path, 1024))
            {
            }

            hr = MFExtern.MFCreateSourceResolver(out sr);
            MFError.ThrowExceptionForHR(hr);            

            hr = sr.CreateObjectFromURL(
                @"file://" + path,
                MFResolution.ByteStream | MFResolution.Write,
                null,
                out pObjectType,
                out pSource);
            MFError.ThrowExceptionForHR(hr);

            m_bs = (IMFByteStream)pSource;

            hr = m_bs.SetLength(100);
            MFError.ThrowExceptionForHR(hr);
            m_write = true;

        }

        void TestGetCurrentPosition()
        {
            long l;

            int hr = m_bs.SetCurrentPosition(1);
            MFError.ThrowExceptionForHR(hr);
            hr = m_bs.GetCurrentPosition(out l);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(l == 1);
        }

        void TestIsEndOfStream()
        {
            bool b;

            int hr = m_bs.IsEndOfStream(out b);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(b == false);
        }

        void TestRead()
        {
            int iReq = 32;
            int iRead;
            IntPtr b = Marshal.AllocCoTaskMem(iReq);

            int hr = m_bs.Read(b, iReq, out iRead);
            MFError.ThrowExceptionForHR(hr);

            Marshal.FreeCoTaskMem(b);
        }

        void TestBeginRead()
        {
            int iReq = 32;
            IntPtr b = Marshal.AllocCoTaskMem(iReq);

            int hr = m_bs.BeginRead(b, iReq, this, this);
            MFError.ThrowExceptionForHR(hr);
            m_mre.WaitOne(-1, true);

            Marshal.FreeCoTaskMem(b);
        }

        void TestWrite()
        {
            int iWrote;
            int iReq = 32;
            IntPtr b = Marshal.AllocCoTaskMem(iReq);
            int hr = m_bs.Write(b, iReq, out iWrote);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(iWrote == iReq);
            Marshal.FreeCoTaskMem(b);
        }

        void TestBeginWrite()
        {
            int iReq = 32;
            IntPtr b = Marshal.AllocCoTaskMem(iReq);
            int hr = m_bs.BeginWrite(b, iReq, this, this);
            MFError.ThrowExceptionForHR(hr);

            m_mre.WaitOne(-1, true);
            Marshal.FreeCoTaskMem(b);
        }

        void TestSeek()
        {
            long l;
            int hr = m_bs.Seek(MFByteStreamSeekOrigin.Current, -32, MFByteStreamSeekingFlags.None, out l);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(l == 1);
        }

        void TestFlush()
        {
            int hr = m_bs.Flush();
            MFError.ThrowExceptionForHR(hr);
        }

        void TestClose()
        {
            int hr = m_bs.Close();
            MFError.ThrowExceptionForHR(hr);
        }


        private void GetInterface()
        {
            MFObjectType pObjectType;
            object pSource;
            IMFSourceResolver sr;

            int hr = MFExtern.MFCreateSourceResolver(out sr);
            MFError.ThrowExceptionForHR(hr);

            hr = sr.CreateObjectFromURL(
                @"file://c:/sourceforge/mflib/test/media/AspectRatio4x3.wmv",
                MFResolution.ByteStream,
                null,
                out pObjectType,
                out pSource);
            MFError.ThrowExceptionForHR(hr);

            m_bs = pSource as IMFByteStream;

        }

        #region IMFAsyncCallback Members

        public int GetParameters(out MFASync pdwFlags, out MFAsyncCallbackQueue pdwQueue)
        {
            pdwFlags = 0;
            pdwQueue = 0;
            return E_NotImplemented;
        }

        public int Invoke(IMFAsyncResult pAsyncResult)
        {
            int i;
            object o;
            IntPtr ip = Marshal.AllocCoTaskMem(8);

            int hr = pAsyncResult.GetState(out o);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(o == this);

            ip = pAsyncResult.GetStateNoAddRef();
            o = Marshal.GetObjectForIUnknown(ip);
            Debug.Assert(o == this);

            hr = pAsyncResult.SetStatus(-1);
            MFError.ThrowExceptionForHR(hr);
            hr = pAsyncResult.GetStatus();
            Debug.Assert(hr == -1);

            try
            {
                // Since the IMFAsyncResult was created with no
                hr = pAsyncResult.GetObject(out o);
                Debug.Assert(hr == E_Pointer);
            }
            catch (Exception e)
            {
                Debug.Assert(e is NullReferenceException);
            }


            if (!m_write)
            {
                hr = m_bs.EndRead(pAsyncResult, out i);
                MFError.ThrowExceptionForHR(hr);

                Debug.Assert(i == 32);
            }
            else
            {
                hr = m_bs.EndWrite(pAsyncResult, out i);
                MFError.ThrowExceptionForHR(hr);

                Debug.Assert(i == 32);
            }

            m_mre.Set();

            return 0;
        }

        #endregion
    }
}
