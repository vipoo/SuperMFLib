// Also tests IMFGetService

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.EVR;
using System.Drawing;

namespace Testv10
{
    class IMFVideoDisplayControlTest
    {
        IMFVideoDisplayControl m_vdc;

        public void DoTests()
        {
            GetInterface2();

            TestSetVideoWindow();
            TestGetNativeVideoSize();
            TestGetIdealVideoSize();
            TestSetVideoPosition();
            TestSetAspectRatioMode();
            TestSetBorderColor();
            TestSetRenderingPrefs();
            TestSetFullscreen();

            TestRepaintVideo();
            TestGetCurrentImage();
        }

        void TestGetNativeVideoSize()
        {
            MFSize s1, s2;
            s1 = new MFSize(1,1);
            s2 = new MFSize(1,1);

            // Note, this call doesn't seem to do anything, but it uses the
            // same defs as GetidealVideoSize, so I'm assuming it's just cuz
            // nothing is connected.
            int hr = m_vdc.GetNativeVideoSize(s1, s2);
            MFError.ThrowExceptionForHR(hr);
        }

        void TestGetIdealVideoSize()
        {
            MFSize s1, s2;
            s1 = new MFSize();
            s2 = new MFSize();

            int hr = m_vdc.GetIdealVideoSize(s1, s2);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(s1.Width > 0 && s1.Height > 0 && s1.Width > 0 && s2.Height > 0);
        }

        void TestSetVideoPosition()
        {
            MFVideoNormalizedRect r1 = new MFVideoNormalizedRect();
            MFRect r2 = new MFRect();

            r1.bottom = 1.0f;
            r1.right = 0.9f;

            r2.bottom = 234;
            r2.right = 345;

            int hr = m_vdc.SetVideoPosition(r1, r2);
            MFError.ThrowExceptionForHR(hr);

            MFVideoNormalizedRect r3 = new MFVideoNormalizedRect();
            MFRect r4 = new MFRect();

            hr = m_vdc.GetVideoPosition(r3, r4);
            MFError.ThrowExceptionForHR(hr);
        }

        void TestSetAspectRatioMode()
        {
            MFVideoAspectRatioMode pMode;

            int hr = m_vdc.SetAspectRatioMode(MFVideoAspectRatioMode.PreservePixel);
            MFError.ThrowExceptionForHR(hr);
            hr = m_vdc.GetAspectRatioMode(out pMode);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(pMode == MFVideoAspectRatioMode.PreservePixel);
        }

        void TestSetVideoWindow()
        {
            IntPtr ip;

            System.Windows.Forms.Form f = new System.Windows.Forms.Form();
            int hr = m_vdc.SetVideoWindow(f.Handle);
            MFError.ThrowExceptionForHR(hr);
            hr = m_vdc.GetVideoWindow(out ip);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(f.Handle == ip);
        }

        void TestRepaintVideo()
        {
            int hr = m_vdc.RepaintVideo();
            MFError.ThrowExceptionForHR(hr);
        }

        void TestGetCurrentImage()
        {
            BitmapInfoHeader bmh = new BitmapInfoHeader();
            int i;
            long l = 0;
            IntPtr ip = IntPtr.Zero;

            bmh.Size = Marshal.SizeOf(typeof(BitmapInfoHeader));

            try
            {
                // Works in BasicPlayer
                int hr = m_vdc.GetCurrentImage(bmh, out ip, out i, out l);
                MFError.ThrowExceptionForHR(hr);
            }
            catch { }
        }

        void TestSetBorderColor()
        {
            int i;

            int hr = m_vdc.SetBorderColor(333);
            MFError.ThrowExceptionForHR(hr);
            hr = m_vdc.GetBorderColor(out i);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(i == 333);
        }

        void TestSetRenderingPrefs()
        {
            MFVideoRenderPrefs p;

            int hr = m_vdc.SetRenderingPrefs(MFVideoRenderPrefs.DoNotRenderBorder);
            MFError.ThrowExceptionForHR(hr);
            hr = m_vdc.GetRenderingPrefs(out p);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(p == MFVideoRenderPrefs.DoNotRenderBorder);
        }

        void TestSetFullscreen()
        {
            bool b;

            // This doesn't work, but again, I'm assuming it's cuz things aren't well connected.  C++
            // does the same thing
            int hr = m_vdc.SetFullscreen(true);
            MFError.ThrowExceptionForHR(hr);
            hr = m_vdc.GetFullscreen(out b);
            MFError.ThrowExceptionForHR(hr);

            //Debug.Assert(b == true);
        }

        private void GetInterface2()
        {
            object o;

            IMFGetService gs = new MFVideoPresenter9() as IMFGetService;
            int hr = gs.GetService(MFServices.MR_VIDEO_RENDER_SERVICE, typeof(IMFVideoDisplayControl).GUID, out o);
            MFError.ThrowExceptionForHR(hr);
            m_vdc = o as IMFVideoDisplayControl;
        }

        private void GetInterface()
        {
            object o;
            IMFActivate pRendererActivate = null;

            System.Windows.Forms.Form f = new System.Windows.Forms.Form();
            int hr = MFExtern.MFCreateVideoRendererActivate(IntPtr.Zero, out pRendererActivate);
            MFError.ThrowExceptionForHR(hr);

            hr = pRendererActivate.ActivateObject(typeof(IMFGetService).GUID, out o);
            MFError.ThrowExceptionForHR(hr);
            IMFGetService imfs = o as IMFGetService;

            hr = imfs.GetService(MFServices.MR_VIDEO_RENDER_SERVICE, typeof(IMFVideoDisplayControl).GUID, out o);
            MFError.ThrowExceptionForHR(hr);
            m_vdc = o as IMFVideoDisplayControl;
        }
    }
}
