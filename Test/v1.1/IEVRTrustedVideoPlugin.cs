using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using DirectShowLib;
using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.EVR;
using MediaFoundation.Transform;

namespace Testv11
{
    [ComVisible(true)]
    public class IEVRTrustedVideoPluginTest
    {
        IEVRTrustedVideoPlugin m_tvp;

        public void DoTests()
        {
            GetInterface();

            TestIt();
        }

        private void TestIt()
        {
            bool b;

            m_tvp.IsInTrustedVideoMode(out b);
            m_tvp.CanConstrict(out b);
            m_tvp.DisableImageExport(true);
            m_tvp.SetConstriction(123);
        }


        private void GetInterface()
        {
            int hr;
            IGraphBuilder pGraph;

            pGraph = (IGraphBuilder)new FilterGraph();
            IBaseFilter pSource;
            hr = pGraph.AddSourceFilter(@"C:\SourceForge\mflib\Test\Media\AspectRatio4x3.wmv", null, out pSource);
            IBaseFilter pEVR = (IBaseFilter)new EnhancedVideoRenderer();

            IMFVideoRenderer pRenderer = (IMFVideoRenderer)pEVR;

            object oMixer = new MFVideoMixer9();
            pRenderer.InitializeRenderer(oMixer as IMFTransform, null);

            ICaptureGraphBuilder2 cgb;
            cgb = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();

            hr = pGraph.AddFilter(pEVR, "EVR");
            hr = cgb.SetFiltergraph(pGraph);
            hr = cgb.RenderStream(null, MediaType.Video, pSource, null, pEVR);

            m_tvp = oMixer as IEVRTrustedVideoPlugin;
        }
    }
}
