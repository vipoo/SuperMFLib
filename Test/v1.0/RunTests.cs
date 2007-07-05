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
            }
            catch (Exception e)
            {
                int hr = COMBase.ParseError(e);

                System.Windows.Forms.MessageBox.Show(MFError.GetErrorText(hr), "Exception", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }
}
