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
                int hr = MFDll.MFStartup(0x10070, MFStartup.Full);
                MFError.ThrowExceptionForHR(hr);

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

            }
            catch (Exception e)
            {
                int hr = COMBase.ParseError(e);

                if (hr != COMBase.E_Fail)
                {
                    System.Windows.Forms.MessageBox.Show(MFError.GetErrorText(hr), "Exception", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(e.Message, "Exception", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
        }
    }
}
