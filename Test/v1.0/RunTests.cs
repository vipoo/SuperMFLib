using System;
using System.Collections.Generic;
using System.Text;

using MediaFoundation;
using MediaFoundation.Utils;

namespace Testv10
{
    class Program
    {
        [MTAThread]
        static void Main(string[] args)
        {
            try
            {
                MFPlatDll.MFStartup(0x10070, MFStartup.Full);

                //IMFSourceResolverTest t01 = new IMFSourceResolverTest();
                //t01.DoTests();

                //IMFTopologyTest t02 = new IMFTopologyTest();
                //t02.DoTests();

                //IMFAttributesTest t03 = new IMFAttributesTest();
                //t03.DoTests();

                //IMFStreamDescriptorTest t04 = new IMFStreamDescriptorTest();
                //t04.DoTests();

                //IMFTopologyNodeTest t05 = new IMFTopologyNodeTest();
                //t05.DoTests();

                //IMFSampleTest t06 = new IMFSampleTest();
                //t06.DoTests();

                //IPropertyStoreTest t07 = new IPropertyStoreTest();
                //t07.DoTests();

                //IMFByteStreamTest t08 = new IMFByteStreamTest();
                //t08.DoTests();

                //IMFVideoDisplayControlTest t09 = new IMFVideoDisplayControlTest();
                //t09.DoTests();

                //IMFMediaEventQueueTest t10 = new IMFMediaEventQueueTest();
                //t10.DoTests();

                //IMFActivateTest t11 = new IMFActivateTest();
                //t11.DoTests();

                //IMFMediaBufferTest t12 = new IMFMediaBufferTest();
                //t12.DoTests();

                //IMFMediaEventTest t13 = new IMFMediaEventTest();
                //t13.DoTests();

                //IMFMediaTypeTest t14 = new IMFMediaTypeTest();
                //t14.DoTests();

                //IMFMediaEventGeneratorTest t15 = new IMFMediaEventGeneratorTest();
                //t15.DoTests();

                //IMFMediaStreamTest t16 = new IMFMediaStreamTest();
                //t16.DoTests();

                //IMFMediaTypeHandlerTest t17 = new IMFMediaTypeHandlerTest();
                //t17.DoTests();

                //IMFPresentationDescriptorTest t18 = new IMFPresentationDescriptorTest();
                //t18.DoTests();

                //IMFMediaSessionTest t19 = new IMFMediaSessionTest();
                //t19.DoTests();

                //IMFClockTest t20 = new IMFClockTest();
                //t20.DoTests();

                //IMFCollectionTest t21 = new IMFCollectionTest();
                //t21.DoTests();

                //IMFPresentationTimeSourceTest t22 = new IMFPresentationTimeSourceTest();
                //t22.DoTests();

            }
            catch (Exception e)
            {
                int hr = COMBase.ParseError(e);

                if (hr != COMBase.E_Fail)
                {
                    System.Windows.Forms.MessageBox.Show(string.Format("0x{0:x}: {1}", hr, MFError.GetErrorText(hr)), "Exception", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(e.Message, "Exception", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
        }
    }
}
