using System;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

using MediaFoundation;
using MediaFoundation.Misc;

namespace Testv10
{
    class IMFSourceResolverTest : COMBase, IMFAsyncCallback
    {
        IMFSourceResolver m_sr;
        AutoResetEvent m_mre = new AutoResetEvent(false);
        bool m_IsURL = true;

        public void DoTests()
        {
            GetInterface();

            TestCreateObjectFromURL();
            TestCreateObjectFromByteStream();
            TestBeginCreateObjectFromURL();
            TestCancelObjectCreation();
            TestBeginCreateObjectFromByteStream();

            //TestEndCreateObjectFromURL(); // Tested in Invoke
            //TestEndCreateObjectFromByteStream(); // Tested in Invoke
        }

        void TestCreateObjectFromURL()
        {
            MFObjectType pObjectType;
            object pSource;

            int hr = m_sr.CreateObjectFromURL(
                @"file://c:/sourceforge/mflib/test/media/AspectRatio4x3.wmv",
                MFResolution.ByteStream,
                null,
                out pObjectType,
                out pSource);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(pObjectType == MFObjectType.ByteStream);
            Debug.Assert(pSource != null);
        }

        void TestBeginCreateObjectFromURL()
        {
            object pCookie;

            int hr = m_sr.BeginCreateObjectFromURL(
                @"http://216.186.32.32/sourceforge/mflib/test/media/AspectRatio4x3.wmv",
                MFResolution.ByteStream,
                null,
                out pCookie,
                this,
                null);
            MFError.ThrowExceptionForHR(hr);

            m_mre.WaitOne(-1, true);  // Timeout takes ~25 seconds
        }

        void TestCancelObjectCreation()
        {
            object pCookie;

            DateTime before = DateTime.Now;

            int hr = m_sr.BeginCreateObjectFromURL(
                @"http://216.186.32.32/sourceforge/mflib/test/media/AspectRatio4x3.wmv",
                MFResolution.ByteStream,
                null,
                out pCookie,
                this,
                null);
            MFError.ThrowExceptionForHR(hr);

            Thread.Sleep(2000);
            hr = m_sr.CancelObjectCreation(pCookie);
            MFError.ThrowExceptionForHR(hr);

            m_mre.WaitOne(-1, true);  // Timeout takes ~25 seconds, should be less due to cancel
            DateTime after = DateTime.Now;

            Debug.Assert( (after - before).TotalSeconds < 10);
        }

        void TestCreateObjectFromByteStream()
        {
            MFObjectType pObjectType, ptype;
            object pSource, pobj;

            int hr = m_sr.CreateObjectFromURL(
                @"file://c:/sourceforge/mflib/test/media/AspectRatio4x3.wmv",
                MFResolution.ByteStream,
                null,
                out pObjectType,
                out pSource);
            MFError.ThrowExceptionForHR(hr);

            hr = m_sr.CreateObjectFromByteStream(
                pSource as IMFByteStream,
                @"file://c:/sourceforge/mflib/test/media/AspectRatio4x3.wmv",
                MFResolution.MediaSource,
                null,
                out ptype,
                out pobj);
            MFError.ThrowExceptionForHR(hr);
        }

        void TestBeginCreateObjectFromByteStream()
        {
            MFObjectType pObjectType;
            object pSource, pobj, pCookie;

            pobj = null;
            m_IsURL = false;

            int hr = m_sr.CreateObjectFromURL(
                @"file://c:/sourceforge/mflib/test/media/AspectRatio4x3.wmv",
                MFResolution.ByteStream,
                null,
                out pObjectType,
                out pSource);
            MFError.ThrowExceptionForHR(hr);

            hr = m_sr.BeginCreateObjectFromByteStream(
                pSource as IMFByteStream,
                @"file://c:/sourceforge/mflib/test/media/AspectRatio4x3.wmv",
                MFResolution.MediaSource,
                null,
                out pCookie,
                this,
                pobj);
            MFError.ThrowExceptionForHR(hr);

            m_mre.WaitOne(-1, true);
        }

        private void GetInterface()
        {
            int hr = MFExtern.MFCreateSourceResolver(out m_sr);
            MFError.ThrowExceptionForHR(hr);
        }

        #region IMFAsyncCallback Members

        public int GetParameters(out MFASync pdwFlags, out MFAsyncCallbackQueue pdwQueue)
        {
            pdwFlags = MFASync.FastIOProcessingCallback;
            pdwQueue = MFAsyncCallbackQueue.Standard;
            //throw new Exception("The method or operation is not implemented.");

            return 0;
        }

        public int Invoke(IMFAsyncResult pAsyncResult)
        {
            int hr;
            if (m_IsURL)
            {
                object o;
                MFObjectType ot;

                try
                {
                    hr = m_sr.EndCreateObjectFromURL(pAsyncResult, out ot, out o);
                    MFError.ThrowExceptionForHR(hr);
                    Debug.WriteLine(ot);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
                finally
                {
                    m_mre.Set();
                }
            }
            else
            {
                object o;
                MFObjectType ot;

                try
                {
                    hr = m_sr.EndCreateObjectFromByteStream(pAsyncResult, out ot, out o);
                    MFError.ThrowExceptionForHR(hr);
                    Debug.WriteLine(ot);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
                finally
                {
                    m_mre.Set();
                }
            }
            return 0;
        }

        #endregion
    }
}
