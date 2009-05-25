// Also tests IMFGetService

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

using DirectShowLib;
using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.EVR;
using MediaFoundation.Transform;

namespace Testv11
{
    class IMFVideoProcessorTest
    {
        IGraphBuilder m_pGraph;
        IMFVideoProcessor m_vp;

        public void DoTests()
        {
            GetInterface();

            TestBackClr();
            TestCaps();
            TestProcMode();
            TestAmp();
            TestFilt();
        }

        private void TestCaps()
        {
            Guid g = Guid.Empty;
            DXVA2VideoProcessorCaps c;

            m_vp.GetVideoProcessorCaps(g, out c);
        }

        private void TestProcMode()
        {
            int i;
            Guid g1;
            Guid[] g = null;

            //((IMediaControl)m_pGraph).Run();

            m_vp.GetVideoProcessorMode(out g1);
            m_vp.GetAvailableVideoProcessorModes(out i, g);
            g = new Guid[i];
            m_vp.GetAvailableVideoProcessorModes(out i, g);

            m_vp.SetVideoProcessorMode(g[0]);
        }

        private void TestBackClr()
        {
            int c;

            m_vp.SetBackgroundColor(33);
            m_vp.GetBackgroundColor(out c);

            Debug.Assert(c == 33, "GetBackgroundColor");
        }

        private void TestAmp()
        {
            DXVA2ValueRange pr;
            m_vp.GetProcAmpRange(DXVA2ProcAmp.Brightness, out pr);
            Debug.Assert(pr.MaxValue == 6553600);

            DXVA2ProcAmpValues pv, pv2;
            pv = new DXVA2ProcAmpValues();
            pv.Hue = 65536 * 4;
            m_vp.SetProcAmpValues(DXVA2ProcAmp.Hue, pv);

            m_vp.GetProcAmpValues(DXVA2ProcAmp.Hue, out pv2);
            Debug.Assert(pv.Hue == pv2.Hue);
        }

        private void TestFilt()
        {
            DXVA2ValueRange pr;
            int pv, pv2;
            DXVA2Filters f = DXVA2Filters.DetailFilterLumaLevel;

            m_vp.GetFilteringRange(f, out pr);
            pv = 655;
            m_vp.SetFilteringValue(f, ref pv);

            m_vp.GetFilteringValue(f, out pv2);
            Debug.Assert(pv == pv2);
        }

        private void GetInterface()
        {
            object o;
            int hr;

            m_pGraph = (IGraphBuilder)new FilterGraph();
            IBaseFilter pSource;
            hr = m_pGraph.AddSourceFilter(@"C:\SourceForge\mflib\Test\Media\AspectRatio4x3.wmv", null, out pSource);
            IBaseFilter pEVR = (IBaseFilter)new EnhancedVideoRenderer();
            hr = m_pGraph.AddFilter(pEVR, "EVR");

            ICaptureGraphBuilder2 cgb;
            cgb = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();

            hr = cgb.SetFiltergraph(m_pGraph);
            hr = cgb.RenderStream(null, MediaType.Video, pSource, null, pEVR);

            IMFGetService gs = pEVR as IMFGetService;
            gs.GetService(MFServices.MR_VIDEO_MIXER_SERVICE, typeof(IMFVideoProcessor).GUID, out o);

            m_vp = o as IMFVideoProcessor;
        }
    }
}
