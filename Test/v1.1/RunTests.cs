using System;
using System.Runtime.InteropServices;
using System.Text;

using MediaFoundation;
using MediaFoundation.Misc;

namespace Testv11
{
    class Program
    {
        [MTAThread]
        static void Main(string[] args)
        {
            try
            {
                MFExtern.MFStartup(0x10070, MFStartup.Full);
                MFExtern.MFLockPlatform();

                //IMFASFMultiplexerTest t01 = new IMFASFMultiplexerTest();
                //t01.DoTests();

                //IMFAudioStreamVolumeTest t02 = new IMFAudioStreamVolumeTest();
                //t02.DoTests();

                //IMFMetadataTest t03 = new IMFMetadataTest();
                //t03.DoTests();

                //IMFMetadataProviderTest t04 = new IMFMetadataProviderTest();
                //t04.DoTests();

                //IMFQualityAdviseTest t05 = new IMFQualityAdviseTest();
                //t05.DoTests();

                //IMFQualityManagerTest t06 = new IMFQualityManagerTest();
                //t06.DoTests();

                //IMFRateControlTest t07 = new IMFRateControlTest();
                //t07.DoTests();

                //IMFShutdownTest t08 = new IMFShutdownTest();
                //t08.DoTests();

                //IMFSimpleAudioVolumeTest t09 = new IMFSimpleAudioVolumeTest();
                //t09.DoTests();

                //IMFVideoMixerBitmapTest t10 = new IMFVideoMixerBitmapTest();
                //t10.DoTests();

                //IMFVideoMixerControlTest t11 = new IMFVideoMixerControlTest();
                //t11.DoTests();

                //IMFVideoPositionMapperTest t12 = new IMFVideoPositionMapperTest();
                //t12.DoTests();

                //IMFVideoProcessorTest t13 = new IMFVideoProcessorTest();
                //t13.DoTests();

                //IMFTimerTest t14 = new IMFTimerTest();
                //t14.DoTests();

                //IMFSourceOpenMonitorTest t15 = new IMFSourceOpenMonitorTest();
                //t15.DoTests();

                //IEVRTrustedVideoPluginTest t16 = new IEVRTrustedVideoPluginTest();
                //t16.DoTests();

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
