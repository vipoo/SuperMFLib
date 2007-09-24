/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/

using System;
using System.Runtime.InteropServices;

using MediaFoundation;
using MediaFoundation.Misc;

// Test with c:\Program Files\Microsoft SDKs\Windows\v6.0\Samples\Multimedia\WMP_11\media\smooth.wmv

namespace Testv10
{
    class IMFAsfIndexerTest
    {
        public void DoTests()
        {
            xMain(new string[] { @"c:\Program Files\Microsoft SDKs\Windows\v6.0\Samples\Multimedia\WMP_11\media\smooth.wmv" });
        }

        static void xMain(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: splitter <filename>");
                return;
            }

            ASFSplit sp = new ASFSplit(args[0]);
            sp.DoSplit();
        }
    }

    class ASFSplit : COMBase
    {
        private string m_sFileName;

        public ASFSplit(string s)
        {
            m_sFileName = s;
        }

        public void DoSplit()
        {
            bool bHasVideo = false;

            IMFByteStream pStream = null;
            IMFASFContentInfo pContentInfo = null;
            IMFASFSplitter pSplitter = null;
            IMFASFIndexer ai = null;

            Console.WriteLine(string.Format("Opening {0}.", m_sFileName));

            try
            {
                // Start the Media Foundation platform.
                MFExtern.MFStartup(0x10070, MFStartup.Full);

                // Open the file.
                OpenFile(m_sFileName, out pStream);

                // Read the ASF header.
                CreateContentInfo(pStream, out pContentInfo);

                // Create the ASF splitter.
                CreateASFSplitter(pContentInfo, out pSplitter);

                // Select the first video stream.
                SelectVideoStream(pContentInfo, pSplitter, out bHasVideo);

                CreateIndexer(pContentInfo, out ai);

                // Parse the ASF file.
                if (bHasVideo)
                {
                    DisplayKeyFrames(ai, pStream, pSplitter);
                }
                else
                {
                    Console.WriteLine("No video stream.");
                }
            }
            catch (Exception e)
            {
                int hr = Marshal.GetHRForException(e);
                string s = MFError.GetErrorText(hr);

                if (s == null)
                {
                    s = e.Message;
                }
                else
                {
                    s = string.Format("{0} ({1})", s, e.Message);
                }

                Console.WriteLine(string.Format("Exception 0x{0:x}: {1}", hr, s));
            }
            finally
            {
                // Clean up.
                SafeRelease(pSplitter);
                SafeRelease(pContentInfo);
                SafeRelease(pStream);
            }

            // Shut down the Media Foundation platform.
            MFExtern.MFShutdown();
        }

        void CreateIndexer(IMFASFContentInfo pContentInfo, out IMFASFIndexer ai)
        {
            MFAsfIndexerFlags f;
            long l;
            int i;
            IMFMediaBuffer mb;

            MFExtern.MFCreateASFIndexer(out ai);
            MFExtern.MFCreateMemoryBuffer(1000, out mb);

            ai.Initialize(pContentInfo);
            ai.GetIndexPosition(pContentInfo, out l);
            ai.GetFlags(out f);
            ai.SetFlags(f);
            ai.GetIndexByteStreamCount(out i);
            ai.GetCompletedIndex(mb, 0);
            ai.GetIndexWriteSpace(out l);
        }

        void OpenFile(string sFileName, out IMFByteStream ppStream)
        {
            // Open a byte stream for the file.
            MFExtern.MFCreateFile(MFFileAccessMode.Read, MFFileOpenMode.FailIfNotExist, MFFileFlags.None, sFileName, out ppStream);
        }

        /////////////////////////////////////////////////////////////////////
        // Name: ReadDataIntoBuffer
        //
        // Reads data from a byte stream and returns a media buffer that
        // contains the data.
        //
        // pStream: Pointer to the byte stream
        // cbToRead: Number of bytes to read
        // ppBuffer: Receives a pointer to the buffer.
        /////////////////////////////////////////////////////////////////////

        void ReadDataIntoBuffer(
            IMFByteStream pStream,     // Pointer to the byte stream.
            int cbToRead,             // Number of bytes to read
            out IMFMediaBuffer ppBuffer   // Receives a pointer to the buffer.
            )
        {
            IntPtr pData;
            int cbRead;   // Actual amount of data read
            int iMax, iCur;

            // Create the media buffer. This function allocates the memory.
            MFExtern.MFCreateMemoryBuffer(cbToRead, out ppBuffer);

            // Access the buffer.
            ppBuffer.Lock(out pData, out iMax, out iCur);

            try
            {
                // Read the data from the byte stream.
                pStream.Read(pData, cbToRead, out cbRead);
            }
            finally
            {
                ppBuffer.Unlock();
                pData = IntPtr.Zero;
            }

            // Update the size of the valid data.
            ppBuffer.SetCurrentLength(cbRead);
        }

        /////////////////////////////////////////////////////////////////////
        // Name: CreateContentInfo
        //
        // Reads the ASF Header Object from a byte stream and returns a
        // pointer to the ASF content information object.
        //
        // pStream:       Pointer to the byte stream. The byte stream's 
        //                current read position must be at the start of the
        //                ASF Header Object.
        // ppContentInfo: Receives a pointer to the ASF content information
        //                object.
        /////////////////////////////////////////////////////////////////////

        void CreateContentInfo(
            IMFByteStream pStream,
            out IMFASFContentInfo ppContentInfo
            )
        {
            long cbHeader = 0;

            const int MIN_ASF_HEADER_SIZE = 30;

            IMFMediaBuffer pBuffer;

            // Create the ASF content information object.
            MFExtern.MFCreateASFContentInfo(out ppContentInfo);

            // Read the first 30 bytes to find the total header size.
            ReadDataIntoBuffer(pStream, MIN_ASF_HEADER_SIZE, out pBuffer);

            try
            {
                ppContentInfo.GetHeaderSize(pBuffer, out cbHeader);

                // Pass the first 30 bytes to the content information object.
                ppContentInfo.ParseHeader(pBuffer, 0);
            }
            finally
            {
                SafeRelease(pBuffer);
            }

            // Read the rest of the header and finish parsing the header.
            ReadDataIntoBuffer(pStream, (int)(cbHeader - MIN_ASF_HEADER_SIZE), out pBuffer);

            ppContentInfo.ParseHeader(pBuffer, MIN_ASF_HEADER_SIZE);
        }

        /////////////////////////////////////////////////////////////////////
        // Name: CreateASFSplitter
        //
        // Creates the ASF splitter.
        //
        // pContentInfo: Pointer to an initialized instance of the ASF 
        //               content information object.
        // ppSplitter:   Receives a pointer to the ASF splitter.
        /////////////////////////////////////////////////////////////////////

        void CreateASFSplitter(IMFASFContentInfo pContentInfo, out IMFASFSplitter ppSplitter)
        {
            MFASFSplitterFlags f;

            MFExtern.MFCreateASFSplitter(out ppSplitter);
            ppSplitter.Initialize(pContentInfo);

            ppSplitter.GetFlags(out f);
            Console.WriteLine(string.Format("Splitter flags: {0}", f));
        }

        /////////////////////////////////////////////////////////////////////
        // Name: SelectVideoStream
        //
        // Selects the first video stream for parsing with the ASF splitter.
        //
        // pContentInfo: Pointer to an initialized instance of the ASF 
        //               content information object.
        // pSplitter:    Pointer to the ASF splitter.
        // pbHasVideo:   Receives TRUE if there is a video stream, or FALSE
        //               otherwise.
        /////////////////////////////////////////////////////////////////////

        void SelectVideoStream(
            IMFASFContentInfo pContentInfo,
            IMFASFSplitter pSplitter,
            out bool pbHasVideo
            )
        {
            int cStreams;
            short wStreamID = 33;
            short[] wStreamIDs = new short[1];
            Guid streamType;
            bool bFoundVideo = false;

            IMFASFProfile pProfile;
            IMFASFStreamConfig pStream;

            // Get the ASF profile from the content information object.
            pContentInfo.GetProfile(out pProfile);

            try
            {
                // Loop through all of the streams in the profile.
                pProfile.GetStreamCount(out cStreams);

                for (int i = 0; i < cStreams; i++)
                {
                    // Get the stream type and stream identifier.
                    pProfile.GetStream(i, out wStreamID, out pStream);

                    try
                    {
                        pStream.GetStreamType(out streamType);

                        if (streamType == MFMediaType.Video)
                        {
                            bFoundVideo = true;
                            break;
                        }
                    }
                    finally
                    {
                        SafeRelease(pStream);
                    }
                }
            }
            finally
            {
                SafeRelease(pProfile);
            }

            // Select the video stream, if found.
            if (bFoundVideo)
            {
                // SelectStreams takes an array of stream identifiers.
                wStreamIDs[0] = wStreamID;
                pSplitter.SelectStreams(wStreamIDs, 1);
            }

            pbHasVideo = bFoundVideo;
        }

        /////////////////////////////////////////////////////////////////////
        // Name: DisplayKeyFrames
        //
        // Parses the video stream and displays information about the
        // samples that contain key frames.
        //
        // pStream:   Pointer to a byte stream. The byte stream's current 
        //            read position must be at the start of the ASF Data 
        //            Object.
        // pSplitter: Pointer to the ASF splitter.
        /////////////////////////////////////////////////////////////////////

        void DisplayKeyFrames(
            IMFASFIndexer ai,
            IMFByteStream pStream,
            IMFASFSplitter pSplitter
            )
        {
            const int cbReadSize = 2048;  // Read size (arbitrary value)

            int cbData;             // Amount of data read
            ASFStatusFlags dwStatus;           // Parsing status
            short wStreamID;          // Stream identifier
            bool bIsKeyFrame = false;    // Is the sample a key frame?
            int cBuffers;           // Buffer count
            int cbTotalLength;      // Buffer length
            long hnsTime;            // Time stamp

            IMFMediaBuffer pBuffer;
            IMFSample pSample;

            IMFByteStream[] aia = new IMFByteStream[1];
            aia[0] = pStream;

            ai.SetIndexByteStreams(aia, 1);

            ASFIndexIdentifier ii = new ASFIndexIdentifier();
            ii.guidIndexType = Guid.Empty;
            ii.wStreamNumber = 2;
            bool b;
            int i1 = 100;
            IntPtr ip = Marshal.AllocCoTaskMem(i1);
            ai.GetIndexStatus(ii, out b, ip, ref i1);
            long l;
            PropVariant pv = new PropVariant(50000000L);
            ai.GetSeekPositionForValue(pv, ii, out l, IntPtr.Zero, out i1);

            while (true)
            {
                // Read data into the buffer.
                ReadDataIntoBuffer(pStream, cbReadSize, out pBuffer);

                try
                {
                    pBuffer.GetCurrentLength(out cbData);

                    if (cbData == 0)
                    {
                        break; // End of file.
                    }

                    // Send the data to the ASF splitter.
                    pSplitter.ParseData(pBuffer, 0, 0);

                    // Pull samples from the splitter.
                    do
                    {
                        pSplitter.GetNextSample(out dwStatus, out wStreamID, out pSample);

                        if (pSample == null)
                        {
                            // No samples yet. Parse more data.
                            break;
                        }

                        try
                        {
                            // We received a sample from the splitter. Check to see
                            // if it's a key frame. The default is FALSE.
                            try
                            {
                                int i;
                                pSample.GetUINT32(MFAttributesClsid.MFSampleExtension_CleanPoint, out i);
                                bIsKeyFrame = i != 0;
                            }
                            catch
                            {
                                bIsKeyFrame = false;
                            }

                            if (bIsKeyFrame)
                            {
                                // Print various information about the key frame.
                                pSample.GetBufferCount(out cBuffers);
                                pSample.GetTotalLength(out cbTotalLength);

                                Console.WriteLine(string.Format("Buffer count: {0}", cBuffers));
                                Console.WriteLine(string.Format("Length: {0} bytes", cbTotalLength));

                                pSample.GetSampleTime(out hnsTime);

                                // Convert the time stamp to seconds.
                                double sec = (double)(hnsTime / 10000) / 1000;

                                Console.WriteLine(string.Format("Time stamp: {0} sec.", sec));
                            }
                        }
                        finally
                        {
                            SafeRelease(pSample);
                        }

                    } while ((dwStatus & ASFStatusFlags.Incomplete) > 0);
                }
                finally
                {
                    SafeRelease(pBuffer);
                }
            }

            Console.WriteLine("Done");
        }
    }
}
