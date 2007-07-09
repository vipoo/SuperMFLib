using System;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

using MediaFoundation;
using MediaFoundation.Utils;

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

            m_sr.CreateObjectFromURL(
                @"file://c:/sourceforge/mflib/test/media/AspectRatio4x3.wmv",
                MFResolution.ByteStream,
                null,
                out pObjectType,
                out pSource);

            Debug.Assert(pObjectType == MFObjectType.ByteStream);
            Debug.Assert(pSource != null);
        }

        void TestBeginCreateObjectFromURL()
        {
            object pCookie;

            m_sr.BeginCreateObjectFromURL(
                @"http://216.186.32.32/sourceforge/mflib/test/media/AspectRatio4x3.wmv",
                MFResolution.ByteStream,
                null,
                out pCookie,
                this,
                null);

            m_mre.WaitOne(-1, true);  // Timeout takes ~25 seconds
        }

        void TestCancelObjectCreation()
        {
            object pCookie;

            DateTime before = DateTime.Now;

            m_sr.BeginCreateObjectFromURL(
                @"http://216.186.32.32/sourceforge/mflib/test/media/AspectRatio4x3.wmv",
                MFResolution.ByteStream,
                null,
                out pCookie,
                this,
                null);

            Thread.Sleep(2000);
            m_sr.CancelObjectCreation(pCookie);

            m_mre.WaitOne(-1, true);  // Timeout takes ~25 seconds, should be less due to cancel
            DateTime after = DateTime.Now;

            Debug.Assert( (after - before).TotalSeconds < 10);
        }

        void TestCreateObjectFromByteStream()
        {
            MFObjectType pObjectType, ptype;
            object pSource, pobj;

            m_sr.CreateObjectFromURL(
                @"file://c:/sourceforge/mflib/test/media/AspectRatio4x3.wmv",
                MFResolution.ByteStream,
                null,
                out pObjectType,
                out pSource);

            try
            {
                m_sr.CreateObjectFromByteStream(
                    pSource as IMFByteStream,
                    @"file://c:/sourceforge/mflib/test/media/AspectRatio4x3.wmv",
                    MFResolution.MediaSource,
                    null,
                    out ptype,
                    out pobj);
            }
            catch (Exception ce)
            {
                int hr = ParseError(ce);
                throw new COMException(MFError.GetErrorText(hr), hr);
            }
        }

        void TestBeginCreateObjectFromByteStream()
        {
            MFObjectType pObjectType;
            object pSource, pobj, pCookie;

            pobj = null;
            m_IsURL = false;

            m_sr.CreateObjectFromURL(
                @"file://c:/sourceforge/mflib/test/media/AspectRatio4x3.wmv",
                MFResolution.ByteStream,
                null,
                out pObjectType,
                out pSource);

            try
            {
                m_sr.BeginCreateObjectFromByteStream(
                    pSource as IMFByteStream,
                    @"file://c:/sourceforge/mflib/test/media/AspectRatio4x3.wmv",
                    MFResolution.MediaSource,
                    null,
                    out pCookie,
                    this,
                    pobj);

                m_mre.WaitOne(-1, true);
            }
            catch (Exception ce)
            {
                int hr = ParseError(ce);
                throw new COMException(MFError.GetErrorText(hr), hr);
            }
        }

        private void GetInterface()
        {
            MFDll.MFCreateSourceResolver(out m_sr);
        }

        #region IMFAsyncCallback Members

        public void GetParameters(out MFASync pdwFlags, out MFAsyncCallbackQueue pdwQueue)
        {
            pdwFlags = MFASync.FastIOProcessingCallback;
            pdwQueue = MFAsyncCallbackQueue.Standard;
            //throw new Exception("The method or operation is not implemented.");
        }

        public void Invoke(IMFAsyncResult pAsyncResult)
        {
            if (m_IsURL)
            {
                object o;
                MFObjectType ot;

                try
                {
                    m_sr.EndCreateObjectFromURL(pAsyncResult, out ot, out o);
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
                    m_sr.EndCreateObjectFromByteStream(pAsyncResult, out ot, out o);
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
        }

        #endregion
    }
}
