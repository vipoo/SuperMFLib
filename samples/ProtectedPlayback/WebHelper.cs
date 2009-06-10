/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released
as public domain.  It is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
*****************************************************************************/

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using MF_ProtectedPlayback;

using MediaFoundation.Misc;

public class WebHelper
{
    //-----------------------------------------------------------------------------
    // Name: OpenURLWithData
    // Desc: Navigates to a URL and POSTs the license acquisition data.
    //
    // wszURL: The license acquisition URL
    // pbPostData: The license acquisition data.
    // cbData: Size of the data, in bytes.
    //
    // (Get the values of these parameters from WM_GET_LICENSE_DATA structure.)
    //-----------------------------------------------------------------------------

    private string m_szUrl;
    private byte[] m_byteArray;
    private IntPtr m_hwnd;

    public void OpenURLWithData(string wszURL, IntPtr pbPostData, int cbData, IntPtr hwnd)
    {
        Thread InvokeThread;

        // Create POST data and convert it to a byte array.
        m_byteArray = new byte[cbData];
        Marshal.Copy(pbPostData, m_byteArray, 0, cbData);

        m_szUrl = wszURL;
        m_hwnd = hwnd;

        InvokeThread = new Thread(new ThreadStart(InvokeMethod));
        InvokeThread.SetApartmentState(ApartmentState.STA);
        InvokeThread.Start();
    }

    // The thread entry point
    private void InvokeMethod()
    {
        Form3 f = new Form3(m_szUrl, m_byteArray, m_hwnd);

        // Keep this thread open until the form closes
        f.ShowDialog();
    }
}
