using System;
using System.Runtime.InteropServices;
using System.Text;
using MediaFoundation;
using MediaFoundation.Misc;
using System.Diagnostics;

namespace Testv21
{
    class IMFTranscodeProfileTest
    {

        public void DoTests()
        {
            GetAudioAttributesNullTest();
            SetAudioAttributesTest();

            GetVideoAttributesNullTest();
            SetVideoAttributesTest();

            GetContainerAttributesNullTest();
            SetContainerAttributesTest();
            
        }


        private void GetAudioAttributesNullTest()
        {
            IMFTranscodeProfile tp;
            int hr = MFExtern.MFCreateTranscodeProfile(out tp);
            MFError.ThrowExceptionForHR(hr);
            IMFAttributes attrib;

            hr = tp.GetAudioAttributes(out attrib);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(attrib == null);

            Marshal.ReleaseComObject(tp);
        }

        private void SetAudioAttributesTest()
        {
            IMFTranscodeProfile tp;
            IMFAttributes attrib;
            IMFAttributes attribNull = null;

            int hr = MFExtern.MFCreateTranscodeProfile(out tp);
            MFError.ThrowExceptionForHR(hr);
            
            hr = MFExtern.MFCreateAttributes(out attrib, 5);            
            MFError.ThrowExceptionForHR(hr);

            hr = tp.SetAudioAttributes(attrib);
            MFError.ThrowExceptionForHR(hr);

            hr = tp.GetAudioAttributes(out attribNull);
            MFError.ThrowExceptionForHR(hr);
            
            Debug.Assert(attrib.Equals(attribNull));

            Marshal.ReleaseComObject(tp);
            Marshal.ReleaseComObject(attrib);
            Marshal.ReleaseComObject(attribNull);
        }


        private void GetVideoAttributesNullTest()
        {
            IMFTranscodeProfile tp;
            int hr = MFExtern.MFCreateTranscodeProfile(out tp);
            MFError.ThrowExceptionForHR(hr);
            IMFAttributes attrib;

            hr = tp.GetVideoAttributes(out attrib);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(attrib == null);

            Marshal.ReleaseComObject(tp);
        }

        private void SetVideoAttributesTest()
        {
            IMFTranscodeProfile tp;
            IMFAttributes attrib;
            IMFAttributes attribNull = null;

            int hr = MFExtern.MFCreateTranscodeProfile(out tp);
            MFError.ThrowExceptionForHR(hr);

            hr = MFExtern.MFCreateAttributes(out attrib, 5);
            MFError.ThrowExceptionForHR(hr);

            hr = tp.SetVideoAttributes(attrib);
            MFError.ThrowExceptionForHR(hr);

            hr = tp.GetVideoAttributes(out attribNull);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(attrib.Equals(attribNull));

            Marshal.ReleaseComObject(tp);
            Marshal.ReleaseComObject(attrib);
            Marshal.ReleaseComObject(attribNull);
        }


        private void GetContainerAttributesNullTest()
        {
            IMFTranscodeProfile tp;
            int hr = MFExtern.MFCreateTranscodeProfile(out tp);
            MFError.ThrowExceptionForHR(hr);
            IMFAttributes attrib;

            hr = tp.GetContainerAttributes(out attrib);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(attrib == null);

            Marshal.ReleaseComObject(tp);
        }

        private void SetContainerAttributesTest()
        {
            IMFTranscodeProfile tp;
            IMFAttributes attrib;
            IMFAttributes attribNull = null;

            int hr = MFExtern.MFCreateTranscodeProfile(out tp);
            MFError.ThrowExceptionForHR(hr);

            hr = MFExtern.MFCreateAttributes(out attrib, 5);
            MFError.ThrowExceptionForHR(hr);

            hr = tp.SetContainerAttributes(attrib);
            MFError.ThrowExceptionForHR(hr);

            hr = tp.GetContainerAttributes(out attribNull);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(attrib.Equals(attribNull));

            Marshal.ReleaseComObject(tp);
            Marshal.ReleaseComObject(attrib);
            Marshal.ReleaseComObject(attribNull);
        }

    }
}
