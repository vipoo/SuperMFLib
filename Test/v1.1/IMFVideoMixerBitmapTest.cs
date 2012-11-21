// Also tests IMFGetService

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Runtime.InteropServices;

using DirectShowLib;
using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.EVR;
using MediaFoundation.Transform;

namespace Testv11
{
    class IMFVideoMixerBitmapTest
    {
        IGraphBuilder m_pGraph;
        IMFVideoMixerBitmap m_vmb;

        public void DoTests()
        {
            GetInterface();

            Test();
        }

        private void Test()
        {
            MFVideoAlphaBitmap ab = new MFVideoAlphaBitmap();
            MFVideoAlphaBitmapParams abp = new MFVideoAlphaBitmapParams();
            MFVideoAlphaBitmapParams abp2 = new MFVideoAlphaBitmapParams();
            Bitmap bm = new Bitmap(@"C:\Windows\Web\Wallpaper\Windows\img0.jpg");
            Graphics g = Graphics.FromImage(bm);            

            ab.GetBitmapFromDC = true;
            ab.stru = g.GetHdc();
            ab.paras = new MFVideoAlphaBitmapParams();
            ab.paras.dwFlags = MFVideoAlphaBitmapFlags.Alpha | MFVideoAlphaBitmapFlags.DestRect;

            ab.paras.fAlpha =  0.5f;

            ab.paras.nrcDest = new MFVideoNormalizedRect(0.5f, 0.5f, 1.0f, 1.0f);

            ab.paras.rcSrc = new MFRect(0, 0, bm.Width, bm.Height);

            int hr = m_vmb.SetAlphaBitmap(ab);
            MFError.ThrowExceptionForHR(hr);

            hr = m_vmb.GetAlphaBitmapParameters(abp);
            MFError.ThrowExceptionForHR(hr);

            // According the the docs, the graph must be running in order to call update
            hr = ((IMediaControl)m_pGraph).Run();
            MFError.ThrowExceptionForHR(hr);
            System.Threading.Thread.Sleep(1000);

            abp.fAlpha = .6f;
            abp.dwFlags |= MFVideoAlphaBitmapFlags.Alpha;

            hr = m_vmb.UpdateAlphaBitmapParameters(abp);
            MFError.ThrowExceptionForHR(hr);
            hr = m_vmb.GetAlphaBitmapParameters(abp2);
            MFError.ThrowExceptionForHR(hr);

            hr = m_vmb.ClearAlphaBitmap();
            MFError.ThrowExceptionForHR(hr);
        }

        private void GetInterface()
        {
            object o;
            int hr;

            m_pGraph = (IGraphBuilder)new FilterGraph();
            IBaseFilter pSource;
            hr = m_pGraph.AddSourceFilter(@"C:\SourceForge\mflib\Test\Media\AspectRatio4x3.wmv", null, out pSource);
            DsError.ThrowExceptionForHR(hr);
            IBaseFilter pEVR = (IBaseFilter)new EnhancedVideoRenderer();
            hr = m_pGraph.AddFilter(pEVR, "EVR");
            DsError.ThrowExceptionForHR(hr);

            ICaptureGraphBuilder2 cgb;
            cgb = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();

            hr = cgb.SetFiltergraph(m_pGraph);
            DsError.ThrowExceptionForHR(hr);
            hr = cgb.RenderStream(null, MediaType.Video, pSource, null, pEVR);
            DsError.ThrowExceptionForHR(hr);

            IMFGetService gs = pEVR as IMFGetService;
            hr = gs.GetService(MFServices.MR_VIDEO_MIXER_SERVICE, typeof(IMFVideoMixerBitmap).GUID, out o);
            MFError.ThrowExceptionForHR(hr);

            m_vmb = o as IMFVideoMixerBitmap;
        }
    }
}
