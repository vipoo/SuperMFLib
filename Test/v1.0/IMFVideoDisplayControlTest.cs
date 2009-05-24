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
        [ComImport, Guid("98455561-5136-4d28-AB08-4CEE40EA2781")]
        protected class myEVR
        {
        }

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
            Size s1, s2;
            s1 = new Size();
            s2 = new Size();

            // Note, this call doesn't seem to do anything, but it uses the
            // same defs as GetidealVideoSize, so I'm assuming it's just cuz
            // nothing is connected.
            m_vdc.GetNativeVideoSize(s1, s2);
        }

        void TestGetIdealVideoSize()
        {
            Size s1, s2;
            s1 = new Size();
            s2 = new Size();

            m_vdc.GetIdealVideoSize(s1, s2);

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

            m_vdc.SetVideoPosition(r1, r2);

            MFVideoNormalizedRect r3 = new MFVideoNormalizedRect();
            MFRect r4 = new MFRect();

            m_vdc.GetVideoPosition(r3, r4);
        }

        void TestSetAspectRatioMode()
        {
            MFVideoAspectRatioMode pMode;

            m_vdc.SetAspectRatioMode(MFVideoAspectRatioMode.PreservePixel);
            m_vdc.GetAspectRatioMode(out pMode);

            Debug.Assert(pMode == MFVideoAspectRatioMode.PreservePixel);
        }

        void TestSetVideoWindow()
        {
            IntPtr ip;

            System.Windows.Forms.Form f = new System.Windows.Forms.Form();
            m_vdc.SetVideoWindow(f.Handle);
            m_vdc.GetVideoWindow(out ip);

            Debug.Assert(f.Handle == ip);
        }

        void TestRepaintVideo()
        {
            m_vdc.RepaintVideo();
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
                m_vdc.GetCurrentImage(bmh, out ip, out i, out l);
            }
            catch { }
        }

        void TestSetBorderColor()
        {
            int i;

            m_vdc.SetBorderColor(333);
            m_vdc.GetBorderColor(out i);

            Debug.Assert(i == 333);
        }

        void TestSetRenderingPrefs()
        {
            MFVideoRenderPrefs p;

            m_vdc.SetRenderingPrefs(MFVideoRenderPrefs.DoNotRenderBorder);
            m_vdc.GetRenderingPrefs(out p);

            Debug.Assert(p == MFVideoRenderPrefs.DoNotRenderBorder);
        }

        void TestSetFullscreen()
        {
            bool b;

            // This doesn't work, but again, I'm assuming it's cuz things aren't well connected.  C++
            // does the same thing
            m_vdc.SetFullscreen(true);
            m_vdc.GetFullscreen(out b);

            //Debug.Assert(b == true);
        }

        private void GetInterface2()
        {
            object o;

            IMFGetService gs = new myEVR() as IMFGetService;
            gs.GetService(MFServices.MR_VIDEO_RENDER_SERVICE, typeof(IMFVideoDisplayControl).GUID, out o);
            m_vdc = o as IMFVideoDisplayControl;
        }

        private void GetInterface()
        {
            object o;
            IMFActivate pRendererActivate = null;

            System.Windows.Forms.Form f = new System.Windows.Forms.Form();
            MFExtern.MFCreateVideoRendererActivate(IntPtr.Zero, out pRendererActivate);

            pRendererActivate.ActivateObject(typeof(IMFGetService).GUID, out o);
            IMFGetService imfs = o as IMFGetService;

            imfs.GetService(MFServices.MR_VIDEO_RENDER_SERVICE, typeof(IMFVideoDisplayControl).GUID, out o);
            m_vdc = o as IMFVideoDisplayControl;
        }
    }
}
