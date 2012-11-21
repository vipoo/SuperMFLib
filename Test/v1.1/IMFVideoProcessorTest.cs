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

            int hr = m_vp.GetVideoProcessorCaps(g, out c);
            MFError.ThrowExceptionForHR(hr);
        }

        private void TestProcMode()
        {
            int i;
            Guid g1;
            Guid[] g3;

            //((IMediaControl)m_pGraph).Run();

            int hr = m_vp.GetVideoProcessorMode(out g1);
            MFError.ThrowExceptionForHR(hr);
            hr = m_vp.GetAvailableVideoProcessorModes(out i, null);
            MFError.ThrowExceptionForHR(hr);
            g3 = new Guid[i];
            hr = m_vp.GetAvailableVideoProcessorModes(out i, g3);
            MFError.ThrowExceptionForHR(hr);

            hr = m_vp.SetVideoProcessorMode(g3[0]);
            MFError.ThrowExceptionForHR(hr);
        }

        private void TestBackClr()
        {
            int c;

            int hr = m_vp.SetBackgroundColor(33);
            MFError.ThrowExceptionForHR(hr);
            hr = m_vp.GetBackgroundColor(out c);

            Debug.Assert(c == 33, "GetBackgroundColor");
        }

        private void TestAmp()
        {
            DXVA2ValueRange pr;
            int hr = m_vp.GetProcAmpRange(DXVA2ProcAmp.Brightness, out pr);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(pr.MaxValue == 6553600);

            DXVA2ProcAmpValues pv, pv2;
            pv = new DXVA2ProcAmpValues();
            pv.Hue = 65536 * 4;
            hr = m_vp.SetProcAmpValues(DXVA2ProcAmp.Hue, pv);
            MFError.ThrowExceptionForHR(hr);

            pv2 = new DXVA2ProcAmpValues();
            hr = m_vp.GetProcAmpValues(DXVA2ProcAmp.Hue, pv2);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(pv.Hue == pv2.Hue);
        }

        private void TestFilt()
        {
            DXVA2ValueRange pr;
            int pv, pv2;
            DXVA2Filters f = DXVA2Filters.NoiseFilterChromaLevel;

            int hr = m_vp.GetFilteringRange(f, out pr);
            MFError.ThrowExceptionForHR(hr);
            pv = pr.MinValue+pr.StepSize;
            hr = m_vp.SetFilteringValue(f, ref pv);
            MFError.ThrowExceptionForHR(hr);

            hr = m_vp.GetFilteringValue(f, out pv2);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(pv == pv2);
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
            hr = gs.GetService(MFServices.MR_VIDEO_MIXER_SERVICE, typeof(IMFVideoProcessor).GUID, out o);
            MFError.ThrowExceptionForHR(hr);

            m_vp = o as IMFVideoProcessor;
        }
    }
}
