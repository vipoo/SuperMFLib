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
    class IMFVideoPositionMapperTest
    {
        IGraphBuilder m_pGraph;
        IMFVideoPositionMapper m_vp;

        public void DoTests()
        {
            GetInterface();

            TestPos();
        }

        private void TestPos()
        {
            float x, y;
            int hr = m_vp.MapOutputCoordinateToInputStream(.5f, .5f, 0, 0, out x, out y);
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
            hr = gs.GetService(MFServices.MR_VIDEO_RENDER_SERVICE, typeof(IMFVideoPositionMapper).GUID, out o);
            MFError.ThrowExceptionForHR(hr);

            m_vp = o as IMFVideoPositionMapper;
        }
    }
}
