using System;
using System.Runtime.InteropServices;
using System.Text;

using MediaFoundation;
using MediaFoundation.Misc;

namespace Testv21
{
    class Program
    {
        [MTAThread]
        static void Main(string[] args)
        {
            try
            {
                MFExtern.MFStartup(0x20070, MFStartup.Full);
                MFExtern.MFLockPlatform();

                //IEVRFilterConfigExTest t1 = new IEVRFilterConfigExTest();
                //t1.DoTests();

                //IMFByteStreamCacheControlTest t2 = new IMFByteStreamCacheControlTest();
                //t2.DoTests();

                //IMFDLNASinkInitTest t3 = new IMFDLNASinkInitTest();
                //t3.DoTests();

                //IMFDRMNetHelperTest t4 = new IMFDRMNetHelperTest();
                //t4.DoTests();

                //IMFFieldOfUseMFTUnlockTest t5 = new IMFFieldOfUseMFTUnlockTest();
                //t5.DoTests();

                //IMFPluginControlTest t6 = new IMFPluginControlTest();
                //t6.DoTests();

                //IMFPMediaItemTest t7 = new IMFPMediaItemTest();
                //t7.DoTests();

                //IMFPMediaPlayerTest t8 = new IMFPMediaPlayerTest();
                //t8.DoTests();

                //IMFPMediaPlayerCallbackTest t9 = new IMFPMediaPlayerCallbackTest();
                //t9.DoTests();

                //IMFQualityAdvise2Test t10 = new IMFQualityAdvise2Test();
                //t10.DoTests();

                //IMFQualityAdviseLimitsTest t11 = new IMFQualityAdviseLimitsTest();
                //t11.DoTests();

                //IMFReadWriteClassFactoryTest t12 = new IMFReadWriteClassFactoryTest();
                //t12.DoTests();

                //IMFSampleGrabberSinkCallback2Test t13 = new IMFSampleGrabberSinkCallback2Test();
                //t13.DoTests();

                //IMFSinkWriterTest t14 = new IMFSinkWriterTest();
                //t14.DoTests();

                //IMFSinkWriterCallbackTest t15 = new IMFSinkWriterCallbackTest();
                //t15.DoTests();

                //IMFSourceReaderTest t16 = new IMFSourceReaderTest();
                //t16.DoTests();

                //IMFSourceReaderCallbackTest t17 = new IMFSourceReaderCallbackTest();
                //t17.DoTests();

                //IMFSSLCertificateManagerTest t18 = new IMFSSLCertificateManagerTest();
                //t18.DoTests();

                //IMFStreamingSinkConfigTest t19 = new IMFStreamingSinkConfigTest();
                //t19.DoTests();

                //IMFTimecodeTranslateTest t20 = new IMFTimecodeTranslateTest();
                //t20.DoTests();

                //IMFTranscodeProfileTest t21 = new IMFTranscodeProfileTest();
                //t21.DoTests();

                //IMFTranscodeSinkInfoProviderTest t22 = new IMFTranscodeSinkInfoProviderTest();
                //t22.DoTests();

                //IMFVideoMixerControl2Test t23 = new IMFVideoMixerControl2Test();
                //t23.DoTests();

                //IMFVideoSampleAllocatorCallbackTest t24 = new IMFVideoSampleAllocatorCallbackTest();
                //t24.DoTests();

                //IMFVideoSampleAllocatorNotifyTest t25 = new IMFVideoSampleAllocatorNotifyTest();
                //t25.DoTests();
            }
            catch (Exception e)
            {
                int hr = Marshal.GetHRForException(e);
                string s = MFError.GetErrorText(hr);

                if (s == null)
                {
                    s = e.Message;
                }
                else
                {
                    s = string.Format("{0} ({1})", s, e.Message);
                }

                System.Windows.Forms.MessageBox.Show(string.Format("0x{0:x}: {1}", hr, s), "Exception", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            finally
            {
                MFExtern.MFUnlockPlatform();
                MFExtern.MFShutdown();
            }
        }
    }
}
