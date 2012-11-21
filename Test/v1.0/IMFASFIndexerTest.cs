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
            int hr1;
            bool bHasVideo = false;

            IMFByteStream pStream = null;
            IMFASFContentInfo pContentInfo = null;
            IMFASFSplitter pSplitter = null;
            IMFASFIndexer ai = null;

            Console.WriteLine(string.Format("Opening {0}.", m_sFileName));

            try
            {
                // Start the Media Foundation platform.
                hr1 = MFExtern.MFStartup(0x10070, MFStartup.Full);
                MFError.ThrowExceptionForHR(hr1);

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
            int hr;

            hr = MFExtern.MFCreateASFIndexer(out ai);
            MFError.ThrowExceptionForHR(hr);
            hr = MFExtern.MFCreateMemoryBuffer(1000, out mb);
            MFError.ThrowExceptionForHR(hr);

            hr = ai.Initialize(pContentInfo);
            MFError.ThrowExceptionForHR(hr);

            hr = ai.GetIndexPosition(pContentInfo, out l);
            MFError.ThrowExceptionForHR(hr);

            hr = ai.GetFlags(out f);
            MFError.ThrowExceptionForHR(hr);

            hr = ai.SetFlags(f);
            MFError.ThrowExceptionForHR(hr);

            hr = ai.GetIndexByteStreamCount(out i);
            MFError.ThrowExceptionForHR(hr);

            hr = ai.GetCompletedIndex(mb, 0);
            MFError.ThrowExceptionForHR(hr);

            hr = ai.GetIndexWriteSpace(out l);
            MFError.ThrowExceptionForHR(hr);
        }

        void OpenFile(string sFileName, out IMFByteStream ppStream)
        {
            // Open a byte stream for the file.
            int hr = MFExtern.MFCreateFile(MFFileAccessMode.Read, MFFileOpenMode.FailIfNotExist, MFFileFlags.None, sFileName, out ppStream);
            MFError.ThrowExceptionForHR(hr);
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
            int hr = MFExtern.MFCreateMemoryBuffer(cbToRead, out ppBuffer);
            MFError.ThrowExceptionForHR(hr);

            // Access the buffer.
            hr = ppBuffer.Lock(out pData, out iMax, out iCur);
            MFError.ThrowExceptionForHR(hr);

            try
            {
                // Read the data from the byte stream.
                hr = pStream.Read(pData, cbToRead, out cbRead);
                MFError.ThrowExceptionForHR(hr);
            }
            finally
            {
                hr = ppBuffer.Unlock();
                MFError.ThrowExceptionForHR(hr);
                pData = IntPtr.Zero;
            }

            // Update the size of the valid data.
            hr = ppBuffer.SetCurrentLength(cbRead);
            MFError.ThrowExceptionForHR(hr);
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
            int hr = MFExtern.MFCreateASFContentInfo(out ppContentInfo);
            MFError.ThrowExceptionForHR(hr);

            // Read the first 30 bytes to find the total header size.
            ReadDataIntoBuffer(pStream, MIN_ASF_HEADER_SIZE, out pBuffer);

            try
            {
                hr = ppContentInfo.GetHeaderSize(pBuffer, out cbHeader);
                MFError.ThrowExceptionForHR(hr);

                // Pass the first 30 bytes to the content information object.
                hr = ppContentInfo.ParseHeader(pBuffer, 0);
                MFError.ThrowExceptionForHR(hr);
            }
            finally
            {
                SafeRelease(pBuffer);
            }

            // Read the rest of the header and finish parsing the header.
            ReadDataIntoBuffer(pStream, (int)(cbHeader - MIN_ASF_HEADER_SIZE), out pBuffer);

            hr = ppContentInfo.ParseHeader(pBuffer, MIN_ASF_HEADER_SIZE);
            MFError.ThrowExceptionForHR(hr);
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

            int hr = MFExtern.MFCreateASFSplitter(out ppSplitter);
            MFError.ThrowExceptionForHR(hr);
            hr = ppSplitter.Initialize(pContentInfo);
            MFError.ThrowExceptionForHR(hr);

            hr = ppSplitter.GetFlags(out f);
            MFError.ThrowExceptionForHR(hr);
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
            int hr = pContentInfo.GetProfile(out pProfile);
            MFError.ThrowExceptionForHR(hr);

            try
            {
                // Loop through all of the streams in the profile.
                hr = pProfile.GetStreamCount(out cStreams);
                MFError.ThrowExceptionForHR(hr);

                for (int i = 0; i < cStreams; i++)
                {
                    // Get the stream type and stream identifier.
                    hr = pProfile.GetStream(i, out wStreamID, out pStream);
                    MFError.ThrowExceptionForHR(hr);

                    try
                    {
                        hr = pStream.GetStreamType(out streamType);
                        MFError.ThrowExceptionForHR(hr);

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
                hr = pSplitter.SelectStreams(wStreamIDs, 1);
                MFError.ThrowExceptionForHR(hr);
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

            int hr = ai.SetIndexByteStreams(aia, 1);
            MFError.ThrowExceptionForHR(hr);

            ASFIndexIdentifier ii = new ASFIndexIdentifier();
            ii.guidIndexType = Guid.Empty;
            ii.wStreamNumber = 2;
            bool b;
            int i1 = 100;
            IntPtr ip = Marshal.AllocCoTaskMem(i1);
            ai.GetIndexStatus(ii, out b, ip, ref i1);
            long l;
            PropVariant pv = new PropVariant(50000000L);
            hr = ai.GetSeekPositionForValue(pv, ii, out l, IntPtr.Zero, out i1);
            MFError.ThrowExceptionForHR(hr);

            while (true)
            {
                // Read data into the buffer.
                ReadDataIntoBuffer(pStream, cbReadSize, out pBuffer);

                try
                {
                    hr = pBuffer.GetCurrentLength(out cbData);
                    MFError.ThrowExceptionForHR(hr);

                    if (cbData == 0)
                    {
                        break; // End of file.
                    }

                    // Send the data to the ASF splitter.
                    hr = pSplitter.ParseData(pBuffer, 0, 0);
                    MFError.ThrowExceptionForHR(hr);

                    // Pull samples from the splitter.
                    do
                    {
                        hr = pSplitter.GetNextSample(out dwStatus, out wStreamID, out pSample);
                        MFError.ThrowExceptionForHR(hr);

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
                                hr = pSample.GetUINT32(MFAttributesClsid.MFSampleExtension_CleanPoint, out i);
                                MFError.ThrowExceptionForHR(hr);
                                bIsKeyFrame = i != 0;
                            }
                            catch
                            {
                                bIsKeyFrame = false;
                            }

                            if (bIsKeyFrame)
                            {
                                // Print various information about the key frame.
                                hr = pSample.GetBufferCount(out cBuffers);
                                MFError.ThrowExceptionForHR(hr);
                                hr = pSample.GetTotalLength(out cbTotalLength);
                                MFError.ThrowExceptionForHR(hr);

                                Console.WriteLine(string.Format("Buffer count: {0}", cBuffers));
                                Console.WriteLine(string.Format("Length: {0} bytes", cbTotalLength));

                                hr = pSample.GetSampleTime(out hnsTime);
                                MFError.ThrowExceptionForHR(hr);

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
