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
            m_vp.MapOutputCoordinateToInputStream(.5f, .5f, 0, 0, out x, out y);
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
            gs.GetService(MFServices.MR_VIDEO_RENDER_SERVICE, typeof(IMFVideoPositionMapper).GUID, out o);

            m_vp = o as IMFVideoPositionMapper;
        }
    }
}
