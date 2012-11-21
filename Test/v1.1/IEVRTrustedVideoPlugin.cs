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

            int hr = m_tvp.IsInTrustedVideoMode(out b);
            MFError.ThrowExceptionForHR(hr);

            hr = m_tvp.CanConstrict(out b);
            MFError.ThrowExceptionForHR(hr);

            hr = m_tvp.DisableImageExport(true);
            MFError.ThrowExceptionForHR(hr);

            hr = m_tvp.SetConstriction(123);
            MFError.ThrowExceptionForHR(hr);
        }


        private void GetInterface()
        {
            int hr;
            IGraphBuilder pGraph;

            pGraph = (IGraphBuilder)new FilterGraph();
            IBaseFilter pSource;
            hr = pGraph.AddSourceFilter(@"C:\SourceForge\mflib\Test\Media\AspectRatio4x3.wmv", null, out pSource);
            MFError.ThrowExceptionForHR(hr);
            IBaseFilter pEVR = (IBaseFilter)new EnhancedVideoRenderer();

            IMFVideoRenderer pRenderer = (IMFVideoRenderer)pEVR;

            object oMixer = new MFVideoMixer9();
            hr = pRenderer.InitializeRenderer(oMixer as IMFTransform, null);
            MFError.ThrowExceptionForHR(hr);

            ICaptureGraphBuilder2 cgb;
            cgb = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();

            hr = pGraph.AddFilter(pEVR, "EVR");
            MFError.ThrowExceptionForHR(hr);
            hr = cgb.SetFiltergraph(pGraph);
            MFError.ThrowExceptionForHR(hr);
            hr = cgb.RenderStream(null, MediaType.Video, pSource, null, pEVR);
            MFError.ThrowExceptionForHR(hr);

            m_tvp = oMixer as IEVRTrustedVideoPlugin;
        }
    }
}
