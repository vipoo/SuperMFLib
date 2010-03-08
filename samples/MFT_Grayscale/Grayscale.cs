/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released
as public domain.  It is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
*****************************************************************************/

// c:\Windows\Microsoft.NET\Framework64\v2.0.50727\regasm /tlb /codebase MFT_Grayscale.dll

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.Transform;

namespace MFT_Grayscale
{
    // This sample implements a Media Foundation transform (MFT) that 
    // converts YUV video frames to grayscale. The conversion is done
    // simply by setting all of the U and V bytes to zero (0x80).

    // NOTES:
    // 1-in, 1-out
    // Fixed streams
    // Formats: UYVY, YUY2, NV12

    // Assumptions:
    // 1. If the MFT is holding an input sample, SetInputType and SetOutputType 
    //    return MF_E_UNSUPPORTED_MEDIATYPE
    // 2. If the input type is set, the output type must match (and vice versa).
    // 3. If both types are set, no type can be set until the current type is 
    //    cleared.
    // 4. Preferred input types: 
    //    (a) If the output type is set, that's the preferred type.
    //    (b) Otherwise. the preferred types are partial types, constructed from 
    //        a list of supported video subtypes. 
    // 5. Preferred output types: As above.

    [ComVisible(true),
    Guid("69042198-8146-4735-90F0-BEFD5BFAEDB7"),
    ClassInterface(ClassInterfaceType.None)]
    public class Grayscale : COMBase, IMFTransform, IDisposable
    {
        #region COM registration

        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);

        [DllImport("kernel32.dll")]
        private static extern void FillMemory(IntPtr destination, int len, byte val);

        #endregion

        public delegate void TransformImage(IntPtr pDest, int lDeststride, IntPtr pSrc, int lSrcStride, int dwWidthInPixels, int dwHeightInPixels);

        #region Member variables

        IMFSample m_pSample;                    // Input sample.
        IMFMediaType m_pInputType;              // Input media type.
        IMFMediaType m_pOutputType;             // Output media type.

        // Fomat information
        FourCC m_videoFOURCC;
        int m_imageWidthInPixels;
        int m_imageHeightInPixels;
        int m_cbImageSize;              // Image size, in bytes.
        TransformImage m_pTransformFn;

        // Video FOURCC codes.
        FourCC FOURCC_YUY2 = new FourCC('Y', 'U', 'Y', '2');
        FourCC FOURCC_UYVY = new FourCC('U', 'Y', 'V', 'Y');
        FourCC FOURCC_NV12 = new FourCC('N', 'V', '1', '2');

        Guid[] g_MediaSubtypes;

        #endregion

        #region Registration methods

        [ComRegisterFunctionAttribute]
        static public void DllRegisterServer(Type t)
        {
            int hr = MFExtern.MFTRegister(
                typeof(Grayscale).GUID,         // CLSID
                MFTransformCategory.MFT_CATEGORY_VIDEO_EFFECT,  // Category
                "Grayscale Video Effect .NET",  // Friendly name
                0,                          // Reserved, must be zero.
                0,
                null,
                0,
                null,
                null
                );
            MFError.ThrowExceptionForHR(hr);

        }

        [ComUnregisterFunctionAttribute]
        static public void DllUnregisterServer(Type t)
        {
            int hr = MFExtern.MFTUnregister(typeof(Grayscale).GUID);
            MFError.ThrowExceptionForHR(hr);
        }

        #endregion

        public Grayscale()
        {
            TRACE("Constructor");

            m_pSample = null;
            m_pInputType = null;
            m_pOutputType = null;
            m_pTransformFn = null;

            g_MediaSubtypes = new Guid[] { FOURCC_NV12.ToMediaSubtype(), FOURCC_YUY2.ToMediaSubtype(), FOURCC_UYVY.ToMediaSubtype() };
        }

        ~Grayscale()
        {
            TRACE("Destructor");
            Dispose();
        }

        #region IMFTransform methods

        public int GetStreamLimits(
            MFInt pdwInputMinimum,
            MFInt pdwInputMaximum,
            MFInt pdwOutputMinimum,
            MFInt pdwOutputMaximum
        )
        {
            TRACE("GetStreamLimits");

            // Fixed stream limits.
            if (pdwInputMinimum != null)
            {
                pdwInputMinimum.Assign(1);
            }
            if (pdwInputMaximum != null)
            {
                pdwInputMaximum.Assign(1);
            }
            if (pdwOutputMinimum != null)
            {
                pdwOutputMinimum.Assign(1);
            }
            if (pdwOutputMaximum != null)
            {
                pdwOutputMaximum.Assign(1);
            }

            return S_Ok;
        }

        public int GetStreamCount(
            MFInt pcInputStreams,
            MFInt pcOutputStreams
        )
        {
            TRACE("GetStreamCount");

            // Fixed stream count.
            if (pcInputStreams != null)
            {
                pcInputStreams.Assign(1);
            }

            if (pcOutputStreams != null)
            {
                pcOutputStreams.Assign(1);
            }
            return S_Ok;
        }

        public int GetStreamIDs(
            int dwInputIDArraySize,
            int[] pdwInputIDs,
            int dwOutputIDArraySize,
            int[] pdwOutputIDs
        )
        {
            TRACE("GetStreamIDs");

            // Do not need to implement, because this MFT has a fixed number of 
            // streams and the stream IDs match the stream indexes.

            // However, I'm going to implement it anyway
            //throw new COMException("Fixed # of zero based streams", E_NotImplemented);

            pdwInputIDs[0] = 0;
            pdwOutputIDs[0] = 0;
            return S_Ok;
        }

        public int GetInputStreamInfo(
            int dwInputStreamID,
            out MFTInputStreamInfo pStreamInfo
        )
        {
            TRACE("GetInputStreamInfo");

            pStreamInfo = new MFTInputStreamInfo();

            lock (this)
            {
                CheckValidInputStream(dwInputStreamID);

                pStreamInfo.hnsMaxLatency = 0;
                pStreamInfo.dwFlags = MFTInputStreamInfoFlags.WholeSamples | MFTInputStreamInfoFlags.SingleSamplePerBuffer;
                pStreamInfo.cbSize = m_cbImageSize;
                pStreamInfo.cbMaxLookahead = 0;
                pStreamInfo.cbAlignment = 0;
            }
            return S_Ok;
        }

        public int GetOutputStreamInfo(
            int dwOutputStreamID,
            out MFTOutputStreamInfo pStreamInfo
        )
        {
            TRACE("GetOutputStreamInfo");

            lock (this)
            {
                CheckValidOutputStream(dwOutputStreamID);

                if (m_pOutputType == null)
                {
                    throw new COMException("No output type set", MFError.MF_E_TRANSFORM_TYPE_NOT_SET);
                }

                pStreamInfo.dwFlags = MFTOutputStreamInfoFlags.WholeSamples |
                     MFTOutputStreamInfoFlags.SingleSamplePerBuffer |
                     MFTOutputStreamInfoFlags.FixedSampleSize;
                pStreamInfo.cbSize = m_cbImageSize;
                pStreamInfo.cbAlignment = 0;
            }
            return S_Ok;
        }

        public int GetAttributes(out IMFAttributes pAttributes)
        {
            TRACE("GetAttributes");

            throw new COMException("No attributes supported", E_NotImplemented);
        }

        public int GetInputStreamAttributes(
            int dwInputStreamID,
            out IMFAttributes ppAttributes
        )
        {
            TRACE("GetInputStreamAttributes");

            throw new COMException("No input attributes supported", E_NotImplemented);
        }

        public int GetOutputStreamAttributes(
            int dwOutputStreamID,
            out IMFAttributes ppAttributes
        )
        {
            TRACE("GetOutputStreamAttributes");

            throw new COMException("No output attributes supported", E_NotImplemented);
        }

        public int DeleteInputStream(int dwStreamID)
        {
            TRACE("DeleteInputStream");

            throw new COMException("Removing streams not supported", E_NotImplemented);
        }

        public int AddInputStreams(
            int cStreams,
            int[] adwStreamIDs
        )
        {
            TRACE("AddInputStreams");

            throw new COMException("Adding streams not supported", E_NotImplemented);
        }

        public int GetInputAvailableType(
            int dwInputStreamID,
            int dwTypeIndex, // 0-based
            out IMFMediaType ppType
        )
        {
            TRACE(string.Format("GetInputAvailableType (stream = {0}, type index = {1})", dwInputStreamID, dwTypeIndex));

            lock (this)
            {
                CheckValidInputStream(dwInputStreamID);

                if (m_pOutputType != null)
                {
                    ppType = m_pOutputType;
                }
                else
                {
                    // Create a partial media type.
                    OnGetPartialType(dwTypeIndex, out ppType);
                }
            }
            return S_Ok;
        }

        public int GetOutputAvailableType(
            int dwOutputStreamID,
            int dwTypeIndex, // 0-based
            out IMFMediaType ppType
        )
        {
            TRACE(string.Format("GetOutputAvailableType (stream = {0}, type index = {1})", dwOutputStreamID, dwTypeIndex));

            lock (this)
            {
                CheckValidOutputStream(dwOutputStreamID);

                if (m_pInputType != null)
                {
                    ppType = m_pInputType;
                }
                else
                {
                    OnGetPartialType(dwTypeIndex, out ppType);
                }
            }
            return S_Ok;
        }

        public int SetInputType(
            int dwInputStreamID,
            IMFMediaType pType,
            MFTSetTypeFlags dwFlags
        )
        {
            TRACE("SetInputType");

            lock (this)
            {
                CheckValidInputStream(dwInputStreamID);

                // Does the caller want us to set the type, or just test it?
                bool bReallySet = ((dwFlags & MFTSetTypeFlags.TestOnly) == 0);

                // If we have an input sample, the client cannot change the type now.
                if (HasPendingOutput())
                {
                    throw new COMException("Can't change type while samples are pending", MFError.MF_E_INVALIDMEDIATYPE);
                }

                // Validate the type.
                OnCheckInputType(pType);

                // The type is OK. 
                // Set the type, unless the caller was just testing.
                if (bReallySet)
                {
                    OnSetInputType(pType);
                }
            }
            return S_Ok;
        }

        public int SetOutputType(
            int dwOutputStreamID,
            IMFMediaType pType,
            MFTSetTypeFlags dwFlags
        )
        {
            TRACE("SetOutputType");

            lock (this)
            {
                CheckValidOutputStream(dwOutputStreamID);

                // Does the caller want us to set the type, or just test it?
                bool bReallySet = ((dwFlags & MFTSetTypeFlags.TestOnly) == 0);

                // If we have an input sample, the client cannot change the type now.
                if (HasPendingOutput())
                {
                    throw new COMException("Cannot change type while samples are pending", MFError.MF_E_INVALIDMEDIATYPE);
                }

                // Validate the type.
                OnCheckOutputType(pType);
                if (bReallySet)
                {
                    // The type is OK. 
                    // Set the type, unless the caller was just testing.
                    OnSetOutputType(pType);
                }
            }
            return S_Ok;
        }

        public int GetInputCurrentType(
            int dwInputStreamID,
            out IMFMediaType ppType
        )
        {
            TRACE("GetInputCurrentType");

            lock (this)
            {
                CheckValidInputStream(dwInputStreamID);

                if (m_pInputType == null)
                {
                    throw new COMException("Type is not set", MFError.MF_E_TRANSFORM_TYPE_NOT_SET);
                }

                ppType = m_pInputType;
            }
            return S_Ok;
        }

        public int GetOutputCurrentType(
            int dwOutputStreamID,
            out IMFMediaType ppType
        )
        {
            TRACE("GetOutputCurrentType");

            lock (this)
            {
                CheckValidOutputStream(dwOutputStreamID);

                if (m_pOutputType == null)
                {
                    throw new COMException("No output type set", MFError.MF_E_TRANSFORM_TYPE_NOT_SET);
                }

                ppType = m_pOutputType;
            }
            return S_Ok;
        }

        public int GetInputStatus(
            int dwInputStreamID,
            out MFTInputStatusFlags pdwFlags
        )
        {
            TRACE("GetInputStatus");

            lock (this)
            {
                CheckValidInputStream(dwInputStreamID);

                // If we already have an input sample, we don't accept
                // another one until the client calls ProcessOutput or Flush.
                if (m_pSample == null)
                {
                    pdwFlags = MFTInputStatusFlags.AcceptData;
                }
                else
                {
                    pdwFlags = MFTInputStatusFlags.None;
                }
            }
            return S_Ok;
        }

        public int GetOutputStatus(
            out MFTOutputStatusFlags pdwFlags)
        {
            TRACE("GetOutputStatus");

            lock (this)
            {
                // We can produce an output sample if (and only if)
                // we have an input sample.
                if (m_pSample != null)
                {
                    pdwFlags = MFTOutputStatusFlags.SampleReady;
                }
                else
                {
                    pdwFlags = MFTOutputStatusFlags.None;
                }
            }
            return S_Ok;
        }

        public int SetOutputBounds(
            long hnsLowerBound,
            long hnsUpperBound
        )
        {
            TRACE("SetOutputBounds");

            throw new COMException("Output bounds not supported", E_NotImplemented);
        }

        public int ProcessEvent(
            int dwInputStreamID,
            IMFMediaEvent pEvent
        )
        {
            TRACE("ProcessEvent");

            throw new COMException("Events not support", E_NotImplemented);
        }

        public int ProcessMessage(
            MFTMessageType eMessage,
            IntPtr ulParam
        )
        {
            TRACE("ProcessMessage");

            lock (this)
            {
                switch (eMessage)
                {
                    case MFTMessageType.CommandFlush:
                        // Flush the MFT.
                        OnFlush();
                        break;

                    // The remaining messages do not require any action from this MFT.

                    case MFTMessageType.CommandDrain:
                        // Drain: Tells the MFT not to accept any more input until 
                        // all of the pending output has been processed. That is our 
                        // default behevior already, so there is nothing to do.

                        //MFTDrainType dt = (MFTDrainType)ulParam.ToInt32();
                        break;

                    case MFTMessageType.SetD3DManager:
                        //object o = Marshal.GetUniqueObjectForIUnknown(ulParam);
                        break;

                    case MFTMessageType.NotifyBeginStreaming:
                        break;

                    case MFTMessageType.NotifyEndStreaming:
                        break;

                    case MFTMessageType.NotifyEndOfStream:
                        //int i = ulParam.ToInt32();
                        break;

                    case MFTMessageType.NotifyStartOfStream:
                        break;
                }
            }
            return S_Ok;
        }

        public int ProcessInput(
            int dwInputStreamID,
            IMFSample pSample,
            int dwFlags
        )
        {
            TRACE("ProcessInput");

            lock (this)
            {
                if (pSample == null)
                {
                    throw new COMException("No input sample provided", E_Pointer);
                }

                CheckValidInputStream(dwInputStreamID);

                if (dwFlags != 0)
                {
                    throw new COMException("Invalid flags", E_InvalidArgument);
                }

                if (m_pInputType == null || m_pOutputType == null)
                {
                    throw new COMException("No input or output type specified", MFError.MF_E_NOTACCEPTING);
                }

                if (m_pSample != null)
                {
                    throw new COMException("Already have input sample", MFError.MF_E_NOTACCEPTING);
                }

                // Cache the sample. We do the actual work in ProcessOutput.
                m_pSample = pSample;
            }
            return S_Ok;
        }

        public int ProcessOutput(
            MFTProcessOutputFlags dwFlags,
            int cOutputBufferCount,
            MFTOutputDataBuffer [] pOutputSamples, // one per stream
            out ProcessOutputStatus pdwStatus
        )
        {
            TRACE("ProcessOutput");

            int hr;

            lock (this)
            {
                // If we don't have an input sample, we need some input before
                // we can generate any output.  Check this first for perf
                if (m_pSample == null)
                {
                    throw new COMException("No input sample", MFError.MF_E_TRANSFORM_NEED_MORE_INPUT);
                }

                // Check input parameters...

                // There are no flags that we accept in this MFT.
                // The only defined flag is MFT_PROCESS_OUTPUT_DISCARD_WHEN_NO_BUFFER. This 
                // flag only applies when the MFT marks an output stream as lazy or optional.
                // However there are no lazy or optional streams on this MFT, so the flag is
                // not valid.
                if (dwFlags != MFTProcessOutputFlags.None)
                {
                    throw new COMException("Invalid flag", E_InvalidArgument);
                }

                if (pOutputSamples == null)
                {
                    throw new COMException("No output sample stream buffer provided", E_Pointer);
                }

                // Must be exactly one output buffer.
                if (cOutputBufferCount != 1)
                {
                    throw new COMException("Incorrect # of buffers", E_InvalidArgument);
                }

                // It must contain a sample.
                if (pOutputSamples[0].pSample == IntPtr.Zero)
                {
                    throw new COMException("Output buffer contains no sample", E_InvalidArgument);
                }

                IMFMediaBuffer pInput = null;
                IMFMediaBuffer pOutput = null;
                IMFSample mypSample = null;

                try
                {
                    // Get the input buffer.
                    hr = m_pSample.GetBufferByIndex(0, out pInput);
                    MFError.ThrowExceptionForHR(hr);

                    // Get the output buffer.
                    mypSample = Marshal.GetUniqueObjectForIUnknown(pOutputSamples[0].pSample) as IMFSample;
                    hr = mypSample.GetBufferByIndex(0, out pOutput);
                    MFError.ThrowExceptionForHR(hr);

                    OnProcessOutput(pInput, pOutput);

                    // Set status flags.
                    pOutputSamples[0].dwStatus = MFTOutputDataBufferFlags.None;
                    pdwStatus = ProcessOutputStatus.None;

                    // Copy the duration and time stamp from the input sample,
                    // if present.

                    try
                    {
                        long hnsDuration;

                        hr = m_pSample.GetSampleDuration(out hnsDuration);
                        MFError.ThrowExceptionForHR(hr);

                        hr = mypSample.SetSampleDuration(hnsDuration);
                        MFError.ThrowExceptionForHR(hr);
                    }
                    catch { }

                    try
                    {
                        long hnsTime;

                        hr = m_pSample.GetSampleTime(out hnsTime);
                        MFError.ThrowExceptionForHR(hr);

                        hr = mypSample.SetSampleTime(hnsTime);
                        MFError.ThrowExceptionForHR(hr);
                    }
                    catch { }

                    SafeRelease(m_pSample);
                    m_pSample = null;
                }
                finally
                {
                    // Release our input sample.
                    SafeRelease(pInput);
                    SafeRelease(pOutput);
                    SafeRelease(mypSample);
                }

            }
            return S_Ok;
        }

        #endregion

        #region Private Methods

        //-------------------------------------------------------------------
        // Name: OnGetPartialType
        // Description: Returns a partial media type from our list.
        //
        // dwTypeIndex: Index into the list of peferred media types.
        // ppmt: Receives a pointer to the media type.
        //-------------------------------------------------------------------

        private void OnGetPartialType(int dwTypeIndex, out IMFMediaType ppmt)
        {
            if (dwTypeIndex >= g_MediaSubtypes.Length)
            {
                throw new COMException("Index out of range", MFError.MF_E_NO_MORE_TYPES);
            }

            int hr = MFExtern.MFCreateMediaType(out ppmt);
            MFError.ThrowExceptionForHR(hr);

            hr = ppmt.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);
            MFError.ThrowExceptionForHR(hr);

            hr = ppmt.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, g_MediaSubtypes[dwTypeIndex]);
            MFError.ThrowExceptionForHR(hr);
        }

        //-------------------------------------------------------------------
        // Name: OnCheckInputType
        // Description: Validate an input media type.
        //-------------------------------------------------------------------

        private void OnCheckInputType(IMFMediaType pmt)
        {
            TRACE("OnCheckInputType");

            // If the output type is set, see if they match.
            if (m_pOutputType != null)
            {
                MFMediaEqual flags;
                int hr = pmt.IsEqual(m_pOutputType, out flags);

                // IsEqual can return S_FALSE. Treat this as failure.
                if (hr != S_Ok)
                {
                    throw new COMException("Output type != input type", MFError.MF_E_INVALIDTYPE);
                }
            }
            else
            {
                // Output type is not set. Just check this type.
                OnCheckMediaType(pmt);
            }
        }

        //-------------------------------------------------------------------
        // Name: OnCheckOutputType
        // Description: Validate an output media type.
        //-------------------------------------------------------------------

        private void OnCheckOutputType(IMFMediaType pmt)
        {
            TRACE("OnCheckOutputType");

            // If the input type is set, see if they match.
            if (m_pInputType != null)
            {
                MFMediaEqual flags;
                int hr = pmt.IsEqual(m_pInputType, out flags);

                // IsEqual can return S_FALSE. Treat this as failure.
                if (hr != S_Ok)
                {
                    throw new COMException("Output type != input type", MFError.MF_E_INVALIDTYPE);
                }
            }
            else
            {
                // Input type is not set. Just check this type.
                OnCheckMediaType(pmt);
            }
        }

        //-------------------------------------------------------------------
        // Name: OnCheckMediaType
        // Description: Validates a media type for this transform.
        //-------------------------------------------------------------------

        private void OnCheckMediaType(IMFMediaType pmt)
        {
            Guid major_type;
            Guid subtype;
            int interlace;

            // Major type must be video.
            int hr = pmt.GetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, out major_type);
            MFError.ThrowExceptionForHR(hr);

            if (major_type != MFMediaType.Video)
            {
                throw new COMException("Type not video", MFError.MF_E_INVALIDTYPE);
            }

            // Subtype must be one of the subtypes in our global list.
            // Get the subtype GUID.
            hr = pmt.GetGUID(MFAttributesClsid.MF_MT_SUBTYPE, out subtype);
            MFError.ThrowExceptionForHR(hr);

            TraceSubtype(subtype);

            // Look for the subtype in our list of accepted types.
            bool bFoundMatchingSubtype = false;
            for (int i = 0; i < g_MediaSubtypes.Length; i++)
            {
                if (subtype == g_MediaSubtypes[i])
                {
                    bFoundMatchingSubtype = true;
                    break;
                }
            }

            if (!bFoundMatchingSubtype)
            {
                throw new COMException("No match on subtype", MFError.MF_E_INVALIDTYPE);
            }

            // Video must be progressive frames.
            hr = pmt.GetUINT32(MFAttributesClsid.MF_MT_INTERLACE_MODE, out interlace);
            MFError.ThrowExceptionForHR(hr);

            if ((MFVideoInterlaceMode)interlace != MFVideoInterlaceMode.Progressive)
            {
                throw new COMException("Video not progressive", MFError.MF_E_INVALIDTYPE);
            }
        }

        //-------------------------------------------------------------------
        // Name: OnSetInputType
        // Description: Sets the input media type.
        //
        // Prerequisite:
        // The input type has already been validated.
        //-------------------------------------------------------------------

        private void OnSetInputType(IMFMediaType pmt)
        {
            TRACE("OnSetInputType");

            // Release the old type
            SafeRelease(m_pInputType);

            // Set the type.
            m_pInputType = pmt;

            // Update the format information.
            UpdateFormatInfo();
        }

        //-------------------------------------------------------------------
        // Name: OnSetOutputType
        // Description: Sets the output media type.
        //
        // Prerequisite:
        // The output type has already been validated.
        //-------------------------------------------------------------------

        private void OnSetOutputType(IMFMediaType pmt)
        {
            TRACE("OnSetOutputType");

            // Release the old type
            SafeRelease(m_pOutputType);

            // Set the type.
            m_pOutputType = pmt;
        }

        //-------------------------------------------------------------------
        // Name: OnProcessOutput
        // Description: Generates output data.
        //-------------------------------------------------------------------

        private void OnProcessOutput(IMFMediaBuffer pIn, IMFMediaBuffer pOut)
        {
            int hr;
            int cb;

            IntPtr pDest;			// Destination buffer.
            int lDestStride = 0;	// Destination stride.

            IntPtr pSrc;			// Source buffer.
            int lSrcStride = 0;		// Source stride.

            bool bLockedInputBuffer = false;
            bool bLockedOutputBuffer = false;

            IMF2DBuffer pOut2D;
            IMF2DBuffer pIn2D;

            // Stride if the buffer does not support IMF2DBuffer
            int lStrideIfContiguous;

            hr = m_pInputType.GetUINT32(MFAttributesClsid.MF_MT_DEFAULT_STRIDE, out lStrideIfContiguous);
            MFError.ThrowExceptionForHR(hr);

            // Lock the output buffer. Use IMF2DBuffer if available.
            pOut2D = pOut as IMF2DBuffer;
            if (pOut2D != null)
            {
                TRACE("output buffer: 2D");
                hr = pOut2D.Lock2D(out pDest, out lDestStride);
                MFError.ThrowExceptionForHR(hr);
            }
            else
            {
                int ml;
                TRACE("output buffer: Lock");
                hr = pOut.Lock(out pDest, out ml, out cb);
                MFError.ThrowExceptionForHR(hr);
                lDestStride = lStrideIfContiguous;
            }
            bLockedOutputBuffer = true;

            // Lock the input buffer. Use IMF2DBuffer if available.
            pIn2D = pIn as IMF2DBuffer;
            if (pIn2D != null)
            {
                TRACE("input buffer: 2D");
                hr = pIn2D.Lock2D(out pSrc, out lSrcStride);
                MFError.ThrowExceptionForHR(hr);
            }
            else
            {
                int ml;
                TRACE("Input buffer: lock");
                hr = pIn.Lock(out pSrc, out ml, out cb);
                MFError.ThrowExceptionForHR(hr);
                lSrcStride = lStrideIfContiguous;
            }
            bLockedInputBuffer = true;

            // Invoke the image transform function.
            if (m_pTransformFn != null)
            {
                m_pTransformFn(pDest, lDestStride, pSrc, lSrcStride,
                    m_imageWidthInPixels, m_imageHeightInPixels);
            }
            else
            {
                throw new COMException("Transform type not set", E_Unexpected);
            }

            // Unlock the buffers.
            if (bLockedOutputBuffer)
            {
                TRACE("Output buffer: unlock");
                if (pOut2D != null)
                {
                    hr = pOut2D.Unlock2D();
                    MFError.ThrowExceptionForHR(hr);
                }
                else
                {
                    hr = pOut.Unlock();
                    MFError.ThrowExceptionForHR(hr);
                }
            }

            if (bLockedInputBuffer)
            {
                TRACE("Input buffer: unlock");
                if (pIn2D != null)
                {
                    hr = pIn2D.Unlock2D();
                    MFError.ThrowExceptionForHR(hr);
                }
                else
                {
                    hr = pIn.Unlock();
                    MFError.ThrowExceptionForHR(hr);
                }
            }

            // Set the data size on the output buffer.
            hr = pOut.SetCurrentLength(m_cbImageSize);
            MFError.ThrowExceptionForHR(hr);

            //SafeRelease(pOut2D);   // This gets released when pOut does
            //SafeRelease(pIn2D);   // This gets released when pIn does
        }

        //-------------------------------------------------------------------
        // Name: OnFlush
        // Description: Flush the MFT.
        //-------------------------------------------------------------------

        private void OnFlush()
        {
            // For this MFT, flushing just means releasing the input sample.
            SafeRelease(m_pSample);
            m_pSample = null;
        }

        //-------------------------------------------------------------------
        // Name: UpdateFormatInfo
        // Description: After the input type is set, update our format 
        //              information.
        //-------------------------------------------------------------------

        private void UpdateFormatInfo()
        {
            int hr;
            Guid subtype;

            m_imageWidthInPixels = 0;
            m_imageHeightInPixels = 0;
            m_videoFOURCC = new FourCC(0);
            m_cbImageSize = 0;

            m_pTransformFn = null;

            if (m_pInputType != null)
            {
                hr = m_pInputType.GetGUID(MFAttributesClsid.MF_MT_SUBTYPE, out subtype);
                MFError.ThrowExceptionForHR(hr);

                TraceSubtype(subtype);
                m_videoFOURCC = new FourCC(subtype);

                if (m_videoFOURCC == FOURCC_YUY2)
                {
                    m_pTransformFn = TransformImage_YUY2;
                }
                else if (m_videoFOURCC == FOURCC_UYVY)
                {
                    m_pTransformFn = TransformImage_UYVY;
                }
                else if (m_videoFOURCC == FOURCC_NV12)
                {
                    m_pTransformFn = TransformImage_NV12;
                }
                else
                {
                    throw new COMException("Unrecognized type", E_Unexpected);
                }

                long lPacked;
                hr = m_pInputType.GetUINT64(MFAttributesClsid.MF_MT_FRAME_SIZE, out lPacked);
                MFError.ThrowExceptionForHR(hr);

                m_imageHeightInPixels = (int)(lPacked & int.MaxValue);
                m_imageWidthInPixels = (int)(lPacked >> 32);

                TRACE(string.Format("Frame size: {0} x {1}", m_imageWidthInPixels, m_imageHeightInPixels));

                // Calculate the image size (not including padding)
                GetImageSize(m_videoFOURCC, m_imageWidthInPixels, m_imageHeightInPixels, out m_cbImageSize);
            }
        }

        //-------------------------------------------------------------------
        // Name: GetImageSize
        // Description: 
        // Calculates the buffer size needed, based on the video format.
        //-------------------------------------------------------------------

        private void GetImageSize(FourCC fcc, int width, int height, out int pcbImage)
        {
            if ( (fcc == FOURCC_YUY2) || (fcc == FOURCC_UYVY) )
            {
                // check overflow
                if ((width > int.MaxValue / 2) ||
                    (width * 2 > int.MaxValue / height))
                {
                    throw new COMException("Overflow on width/height", E_InvalidArgument);
                }
                else
                {
                    // 16 bpp
                    pcbImage = width * height * 2;
                }
            }
            else if (fcc == FOURCC_NV12)
            {
                // check overflow
                if ((height / 2 > int.MaxValue - height) ||
                    ((height + height / 2) > int.MaxValue / width))
                {
                    throw new COMException("Overflow on width/height", E_InvalidArgument);
                }
                else
                {
                    // 12 bpp
                    pcbImage = width * (height + (height / 2));
                }
            }
            else
            {
                throw new COMException("Unsupported type", E_Fail);    // Unsupported type.
            }
        }

        private void CheckValidInputStream(int dwInputStreamID)
        {
            if (dwInputStreamID != 0)
            {
                throw new COMException("Invalid input stream ID", MFError.MF_E_INVALIDSTREAMNUMBER);
            }
        }

        private void CheckValidOutputStream(int dwOutputStreamID)
        {
            if (dwOutputStreamID != 0)
            {
                throw new COMException("Invalid output stream ID", MFError.MF_E_INVALIDSTREAMNUMBER);
            }
        }

        // HasPendingOutput: Returns TRUE if the MFT is holding an input sample.
        private bool HasPendingOutput() { return m_pSample != null; }

        private void TraceSubtype(Guid g)
        {
#if DEBUG
            FourCC fc = new FourCC(g);
            TRACE(string.Format("Subtype: {0}", fc.ToString()));
#endif
        }

        //-------------------------------------------------------------------
        // Name: TransformImage_UYVY
        // Description: Converts an image in UYVY format to grayscale.
        //
        // The image conversion functions take the following parameters:
        //
        // pDest:            Pointer to the destination buffer.
        // lDestStride:      Stride of the destination buffer, in bytes.
        // pSrc:             Pointer to the source buffer.
        // lSrcStride:       Stride of the source buffer, in bytes.
        // dwWidthInPixels:  Frame width in pixels.
        // dwHeightInPixels: Frame height, in pixels.
        //-------------------------------------------------------------------
        unsafe private void TransformImage_UYVY(
            IntPtr pDest,
            int lDestStride,
            IntPtr pSrc,
            int lSrcStride,
            int dwWidthInPixels,
            int dwHeightInPixels
            )
        {
            // This routine uses unsafe pointers only for perf reasons.  Note you'll
            // need to mark the routine as unsafe, and change the project settings to
            // allow unsafe code

            ushort* pSrc_Pixel = (ushort*)pSrc;
            ushort* pDest_Pixel = (ushort*)pDest;
            int lMySrcStride = (lSrcStride / 2);  // lSrcStride is in bytes and we need words
            int lMyDestStride = (lDestStride / 2); // lSrcStride is in bytes and we need words

            for (int y = 0; y < dwHeightInPixels; y++)
            {
                for (int x = 0; x < dwWidthInPixels; x++)
                {
                    // Byte order is U0 Y0 V0 Y1
                    // Each WORD is a byte pair (U/V, Y)
                    // Windows is little-endian so the order appears reversed.

                    //short pixel = (short)(pSrc_Pixel[x] & 0xFF00);
                    //pixel |= 0x0080;
                    //pDest_Pixel[x] = pixel;

                    pDest_Pixel[x] = (ushort)((pSrc_Pixel[x] & 0xFF00) | 0x0080);
                }

                pSrc_Pixel += lMySrcStride;
                pDest_Pixel += lMyDestStride;
            }
        }

        //-------------------------------------------------------------------
        // Name: TransformImage_YUY2
        // Description: Converts an image in YUY2 format to grayscale.
        //-------------------------------------------------------------------

        unsafe private void TransformImage_YUY2(
            IntPtr pDest,
            int lDestStride,
            IntPtr pSrc,
            int lSrcStride,
            int dwWidthInPixels,
            int dwHeightInPixels
            )
        {
            // This routine uses unsafe pointers only for perf reasons.  Note you'll
            // need to mark the routine as unsafe, and change the project settings to
            // allow unsafe code

            ushort* pSrc_Pixel = (ushort*)pSrc;
            ushort* pDest_Pixel = (ushort*)pDest;
            int lMySrcStride = (lSrcStride / 2);  // lSrcStride is in bytes and we need words
            int lMyDestStride = (lDestStride / 2); // lSrcStride is in bytes and we need words

            for (int y = 0; y < dwHeightInPixels; y++)
            {
                for (int x = 0; x < dwWidthInPixels; x++)
                {
                    // Byte order is Y0 U0 Y1 V0 
                    // Each WORD is a byte pair (Y, U/V)
                    // Windows is little-endian so the order appears reversed.

                    //ushort pixel = (ushort)(pSrc_Pixel[x] & 0x00FF);
                    //pixel |= (ushort)0x8000;
                    //pDest_Pixel[x] = pixel;

                    pDest_Pixel[x] = (ushort)((pSrc_Pixel[x] & 0x00FF) | 0x8000);
                }

                pSrc_Pixel += lMySrcStride;
                pDest_Pixel += lMyDestStride;
            }
        }

        //-------------------------------------------------------------------
        // Name: TransformImage_NV12
        // Description: Converts an image in NV12 format to grayscale.
        //-------------------------------------------------------------------

        private void TransformImage_NV12(
            IntPtr pDest,
            int lDestStride,
            IntPtr pSrc,
            int lSrcStride,
            int dwWidthInPixels,
            int dwHeightInPixels
            )
        {
            // NV12 is planar: Y plane, followed by packed U-V plane.

            // Y plane
            for (int y = 0; y < dwHeightInPixels; y++)
            {
                CopyMemory(pDest, pSrc, dwWidthInPixels);
                pDest = new IntPtr(pDest.ToInt64() + lDestStride);
                pSrc = new IntPtr(pSrc.ToInt64() + lSrcStride);
            }

            // U-V plane
            for (int y = 0; y < dwHeightInPixels / 2; y++)
            {
                FillMemory(pDest, dwWidthInPixels, 0x80);
                pDest = new IntPtr(pDest.ToInt64() + lDestStride);
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            TRACE("Dispose");

            SafeRelease(m_pSample);
            m_pSample = null;

            if (m_pInputType == m_pOutputType)
            {
                SafeRelease(m_pInputType);
            }
            else
            {
                SafeRelease(m_pInputType);
                SafeRelease(m_pOutputType);
            }

            m_pInputType = null;
            m_pOutputType = null;
            g_MediaSubtypes = null;

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
