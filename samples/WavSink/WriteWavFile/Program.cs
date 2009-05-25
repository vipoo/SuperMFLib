/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released
as public domain.  It is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
*****************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.Alt;

namespace WriteWavFile
{
    class Program
    {
        ///////////////////////////////////////////////////////////////////////
        //  Name: wmain
        //  Description:  Entry point to the application.
        //  
        //  Usage: writewavfile.exe inputfile outputfile
        ///////////////////////////////////////////////////////////////////////

        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                MFExtern.MFStartup(0x10070, MFStartup.Full);

                try
                {
                    CreateWavFile(args[0], args[1]);
                    Console.WriteLine("Complete!");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    MFExtern.MFShutdown();
                }
            }
            else
            {
                Console.WriteLine("Usage: WriteWavFile.exe InputFile OuputFile");
            }
        }

        ///////////////////////////////////////////////////////////////////////
        //  Name: CreateWavFile
        //  Description:  Creates a .wav file from an input file.
        ///////////////////////////////////////////////////////////////////////

        static void CreateWavFile(string sURL, string sOutputFile)
        {
            IMFByteStream pStream = null;
            IMFMediaSinkAlt pSink = null;
            IMFMediaSource pSource = null;
            IMFTopology pTopology = null;
            WavSinkNS.CWavSink pObj = null;

            MFExtern.MFCreateFile(MFFileAccessMode.Write, MFFileOpenMode.DeleteIfExist, MFFileFlags.None, sOutputFile, out pStream);

            try
            {
                pObj = new WavSinkNS.CWavSink(pStream);
                pSink = pObj as IMFMediaSinkAlt;

                // Create the media source from the URL.
                CreateMediaSource(sURL, out pSource);

                // Create the topology.
                CreateTopology(pSource, pSink, out pTopology);

                // Run the media session.
                RunMediaSession(pTopology);

                pSource.Shutdown();
            }
            finally
            {
                if (pStream != null)
                {
                    Marshal.ReleaseComObject(pStream);
                }
                if (pSource != null)
                {
                    Marshal.ReleaseComObject(pSource);
                }
                if (pTopology != null)
                {
                    Marshal.ReleaseComObject(pTopology);
                }
                //pObj.Dispose();
            }
        }

        ///////////////////////////////////////////////////////////////////////
        //  Name: RunMediaSession
        //  Description:  
        //  Queues the specified topology on the media session and runs the
        //  media session until the MESessionEnded event is received.
        ///////////////////////////////////////////////////////////////////////

        static void RunMediaSession(IMFTopology pTopology)
        {
            IMFMediaSession pSession;

            bool bGetAnotherEvent = true;
            PropVariant varStartPosition = new PropVariant();

            MFExtern.MFCreateMediaSession(null, out pSession);

            try
            {
                pSession.SetTopology(0, pTopology);

                while (bGetAnotherEvent)
                {
                    int hrStatus = 0;
                    IMFMediaEvent pEvent;
                    MediaEventType meType = MediaEventType.MEUnknown;

                    int TopoStatus = (int)MFTopoStatus.Invalid; // Used with MESessionTopologyStatus event.    

                    pSession.GetEvent(MFEventFlag.None, out pEvent);

                    try
                    {
                        pEvent.GetStatus(out hrStatus);
                        pEvent.GetType(out meType);

                        if (hrStatus >= 0)
                        {
                            switch (meType)
                            {
                                case MediaEventType.MESessionTopologySet:
                                    Debug.WriteLine("MESessionTopologySet");
                                    break;

                                case MediaEventType.MESessionTopologyStatus:
                                    // Get the status code.
                                    pEvent.GetUINT32(MFAttributesClsid.MF_EVENT_TOPOLOGY_STATUS, out TopoStatus);
                                    switch ((MFTopoStatus)TopoStatus)
                                    {
                                        case MFTopoStatus.Ready:
                                            Debug.WriteLine("MESessionTopologyStatus: MF_TOPOSTATUS_READY");
                                            pSession.Start(Guid.Empty, varStartPosition);
                                            break;

                                        case MFTopoStatus.Ended:
                                            Debug.WriteLine("MESessionTopologyStatus: MF_TOPOSTATUS_ENDED");
                                            break;
                                    }
                                    break;

                                case MediaEventType.MESessionStarted:
                                    Debug.WriteLine("MESessionStarted");
                                    break;

                                case MediaEventType.MESessionEnded:
                                    Debug.WriteLine("MESessionEnded");
                                    pSession.Stop();
                                    break;

                                case MediaEventType.MESessionStopped:
                                    Debug.WriteLine("MESessionStopped");
                                    Console.WriteLine("Attempting to close the media session.");
                                    pSession.Close();
                                    break;

                                case MediaEventType.MESessionClosed:
                                    Debug.WriteLine("MESessionClosed");
                                    bGetAnotherEvent = false;
                                    break;

                                default:
                                    Debug.WriteLine(string.Format("Media session event: {0}", meType));
                                    break;
                            }
                        }
                        else
                        {
                            Debug.WriteLine(string.Format("HRStatus: {0}", hrStatus));
                            bGetAnotherEvent = false;
                        }
                    }
                    finally
                    {
                        if (pEvent != null)
                        {
                            Marshal.ReleaseComObject(pEvent);
                        }
                    }
                }

                Debug.WriteLine("Shutting down the media session.");
                pSession.Shutdown();
            }
            finally
            {
                if (pSession != null)
                {
                    Marshal.ReleaseComObject(pSession);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////
        //  Name: CreateMediaSource
        //  Description:  Create a media source from a URL.
        //
        //  sURL: The URL to open.
        //  ppSource: Receives a pointer to the media source.
        ///////////////////////////////////////////////////////////////////////

        static void CreateMediaSource(string sURL, out IMFMediaSource ppSource)
        {
            IMFSourceResolver pSourceResolver;
            object pSourceUnk;

            // Create the source resolver.
            MFExtern.MFCreateSourceResolver(out pSourceResolver);

            try
            {
                // Use the source resolver to create the media source.
                MFObjectType ObjectType = MFObjectType.Invalid;
                pSourceResolver.CreateObjectFromURL(
                        sURL,                      // URL of the source.
                        MFResolution.MediaSource, // Create a source object.
                        null,                      // Optional property store.
                        out ObjectType,               // Receives the object type. 
                        out pSourceUnk   // Receives a pointer to the source.
                    );

                // Get the IMFMediaSource interface from the media source.
                ppSource = (IMFMediaSource)pSourceUnk;
            }
            finally
            {
                // Clean up.
                if (pSourceResolver != null)
                {
                    Marshal.ReleaseComObject(pSourceResolver);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////
        //  Name: CreateTopology
        //  Description:  Creates the topology.
        // 
        //  Note: The first audio stream is conntected to the media sink.
        //        Other streams are deselected.
        ///////////////////////////////////////////////////////////////////////

        static void CreateTopology(IMFMediaSource pSource, IMFMediaSinkAlt pSink, out IMFTopology ppTopology)
        {
            IMFPresentationDescriptor pPD = null;
            IMFStreamDescriptor pSD = null;

            int cStreams = 0;
            bool fConnected = false;

            MFExtern.MFCreateTopology(out ppTopology);

            pSource.CreatePresentationDescriptor(out pPD);

            try
            {
                pPD.GetStreamDescriptorCount(out cStreams);

                Guid majorType;
                bool fSelected = false;

                for (int iStream = 0; iStream < cStreams; iStream++)
                {
                    pPD.GetStreamDescriptorByIndex(iStream, out fSelected, out pSD);

                    try
                    {
                        // If the stream is not selected by default, ignore it.
                        if (!fSelected)
                        {
                            continue;
                        }

                        // Get the major media type.
                        GetStreamMajorType(pSD, out majorType);

                        // If it's not audio, deselect it and continue.
                        if (majorType != MFMediaType.Audio)
                        {
                            // Deselect this stream
                            pPD.DeselectStream(iStream);

                            continue;
                        }

                        // It's an audio stream, so try to create the topology branch.
                        CreateTopologyBranch(ppTopology, pSource, pPD, pSD, pSink);
                    }
                    finally
                    {
                        if (pSD != null)
                        {
                            Marshal.ReleaseComObject(pSD);
                        }
                    }

                    // Set our status flag. 
                    fConnected = true;

                    // At this point we have reached the first audio stream in the
                    // source, so we can stop looking (whether we succeeded or failed).
                    break;
                }
            }
            finally
            {
                if (pPD != null)
                {
                    Marshal.ReleaseComObject(pPD);
                }
            }

            // Even if we succeeded, if we didn't connect any streams, it's a failure.
            // (For example, it might be a video-only source.
            if (!fConnected)
            {
                throw new Exception("No audio streams");
            }
        }

        //////////////////////////////////////////////////////////////////////
        //  Name: CreateSourceNode
        //  Creates a source node for a media stream. 
        //
        //  pSource:   Pointer to the media source.
        //  pSourcePD: Pointer to the source's presentation descriptor.
        //  pSourceSD: Pointer to the stream descriptor.
        //  ppNode:    Receives the IMFTopologyNode pointer.
        ///////////////////////////////////////////////////////////////////////

        static void CreateSourceNode(
            IMFMediaSource pSource,          // Media source.
            IMFPresentationDescriptor pPD,   // Presentation descriptor.
            IMFStreamDescriptor pSD,         // Stream descriptor.
            out IMFTopologyNode ppNode          // Receives the node pointer.
            )
        {

            // Create the node.
            MFExtern.MFCreateTopologyNode(
                MFTopologyType.SourcestreamNode,
                out ppNode);

            // Set the attributes.
            ppNode.SetUnknown(
                MFAttributesClsid.MF_TOPONODE_SOURCE,
                pSource);

            ppNode.SetUnknown(
                MFAttributesClsid.MF_TOPONODE_PRESENTATION_DESCRIPTOR,
                pPD);

            ppNode.SetUnknown(
                MFAttributesClsid.MF_TOPONODE_STREAM_DESCRIPTOR,
                pSD);
        }

        ///////////////////////////////////////////////////////////////////////
        //  Name: CreateTopologyBranch
        //  Description:  Adds a source and sink to the topology and
        //                connects them.
        //
        //  pTopology: The topology.
        //  pSource:   The media source.
        //  pPD:       The source's presentation descriptor.
        //  pSD:       The stream descriptor for the stream.
        //  pSink:     The media sink.
        //
        ///////////////////////////////////////////////////////////////////////

        static void CreateTopologyBranch(
            IMFTopology pTopology,
            IMFMediaSource pSource,          // Media source.
            IMFPresentationDescriptor pPD,   // Presentation descriptor.
            IMFStreamDescriptor pSD,         // Stream descriptor.
            IMFMediaSinkAlt pSink
            )
        {
            IMFTopologyNode pSourceNode = null;
            IMFTopologyNode pOutputNode = null;

            CreateSourceNode(pSource, pPD, pSD, out pSourceNode);

            try
            {
                CreateOutputNode(pSink, 0, out pOutputNode);

                try
                {
                    pTopology.AddNode(pSourceNode);
                    pTopology.AddNode(pOutputNode);

                    pSourceNode.ConnectOutput(0, pOutputNode, 0);
                }
                finally
                {
                    if (pOutputNode != null)
                    {
                        Marshal.ReleaseComObject(pOutputNode);
                    }
                }
            }
            finally
            {
                if (pSourceNode != null)
                {
                    Marshal.ReleaseComObject(pSourceNode);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////
        //  Name: CreateOutputNode
        //  Description:  Creates an output node for a stream sink.
        //
        //  pSink:     The media sink.
        //  iStream:   Index of the stream sink on the media sink.
        //  ppNode:    Receives a pointer to the topology node.
        ///////////////////////////////////////////////////////////////////////

        static void CreateOutputNode(IMFMediaSinkAlt pSink, int iStream, out IMFTopologyNode ppNode)
        {
            IMFStreamSinkAlt pStream = null;

            pSink.GetStreamSinkByIndex(iStream, out pStream);

            MFExtern.MFCreateTopologyNode(MFTopologyType.OutputNode, out ppNode);
            ppNode.SetObject(pStream);

            //Marshal.ReleaseComObject(pStream);
        }

        ///////////////////////////////////////////////////////////////////////
        //  Name: GetStreamMajorType
        //  Description:  Returns the major media type for a stream.
        ///////////////////////////////////////////////////////////////////////

        static void GetStreamMajorType(IMFStreamDescriptor pSD, out Guid pMajorType)
        {
            IMFMediaTypeHandler pHandler;

            pSD.GetMediaTypeHandler(out pHandler);

            try
            {
                pHandler.GetMajorType(out pMajorType);
            }
            finally
            {
                if (pHandler != null)
                {
                    Marshal.ReleaseComObject(pHandler);
                }
            }
        }
    }
}
