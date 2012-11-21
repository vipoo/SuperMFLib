using System;
using System.Text;
using System.Threading;
using MediaFoundation;
using MediaFoundation.Misc;
using System.Diagnostics;

namespace Testv21
{
    public class IMFTimecodeTranslateTest : IMFAsyncCallback
    {
        /*
         * 
         * BeginConvertHNSToTimecode Starts an asynchronous call to convert time in 100-nanosecond units to SMPTE time code.
 
            BeginConvertTimecodeToHNS Starts an asynchronous call to convert SMPTE time code to 100-nanosecond units.
 
            EndConvertHNSToTimecode Completes an asynchronous request to convert time in 100-nanosecond units to SMPTE time code.
 
            EndConvertTimecodeToHNS Completes an asynchronous request to convert time in SMPTE time code to 100-nanosecond units.
 

         * */

        public void DoTests()
        {
            TestBeginConvertHNSToTimecode(@"C:\Users\Public\Videos\Sample Videos\Wildlife.wmv");
        }

        public void TestBeginConvertHNSToTimecode(string sFileName)
        {
            IMFSourceResolver source;
            int hr = MFExtern.MFCreateSourceResolver(out source);
            MFError.ThrowExceptionForHR(hr);

            MFObjectType objType;

            object obj;
            hr = source.CreateObjectFromURL(sFileName, MFResolution.MediaSource, null, out objType, out obj);
            MFError.ThrowExceptionForHR(hr);

            IMFMediaSource src = obj as IMFMediaSource;
            object timecode;
            hr = MFExtern.MFGetService(src, MFServices.MF_TIMECODE_SERVICE, typeof(IMFTimecodeTranslate).GUID, out timecode);
            MFError.ThrowExceptionForHR(hr);

            translate = (IMFTimecodeTranslate)timecode;
            bDone = false;
            hr = translate.BeginConvertHNSToTimecode(0, this, null);
            MFError.ThrowExceptionForHR(hr);

            while (!bDone)
                Thread.Sleep(100);
        }

        private IMFTimecodeTranslate translate;
        private bool bDone;

        int IMFAsyncCallback.GetParameters(out MFASync pdwFlags, out MFAsyncCallbackQueue pdwQueue)
        {
            //throw new NotImplementedException();
            pdwFlags = MFASync.None;
            pdwQueue = MFAsyncCallbackQueue.Standard;
            return 0;
        }

        int IMFAsyncCallback.Invoke(IMFAsyncResult pAsyncResult)
        {
            PropVariant var = new PropVariant();
            int hr = translate.EndConvertHNSToTimecode(pAsyncResult, var);
            string s = MFError.GetErrorText(hr);

            if (hr != MFError.MF_E_NO_INDEX)
                Debug.WriteLine("Works!");

            return hr;
        }
    }
}
