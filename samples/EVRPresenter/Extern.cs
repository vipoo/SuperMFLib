/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released
as public domain.  It is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
*****************************************************************************/

using System;
using System.Collections;
using System.Text;
using System.Diagnostics;

using System.Runtime.InteropServices;

using MediaFoundation;
using MediaFoundation.Misc;

namespace MediaFoundation.Utility
{
    class Utils
    {
        public static void MFSetBlob(IMFAttributes p, Guid g, object o)
        {
            int hr;
            int iSize = Marshal.SizeOf(o);
            byte[] b = new byte[iSize];

            GCHandle h = GCHandle.Alloc(b, GCHandleType.Pinned);
            try
            {
                IntPtr ip = h.AddrOfPinnedObject();

                Marshal.StructureToPtr(o, ip, false);
            }
            finally
            {
                h.Free();
            }
            hr = p.SetBlob(g, b, iSize);
            MFError.ThrowExceptionForHR(hr);
        }

        public static void MFGetBlob(IMFAttributes p, Guid g, object obj)
        {
            int hr;
            int iSize;
            int i;

            // Get the blob into a byte array
            hr = p.GetBlobSize(g, out iSize);
            MFError.ThrowExceptionForHR(hr);

            byte[] b = new byte[iSize];
            hr = p.GetBlob(g, b, iSize, out i);
            MFError.ThrowExceptionForHR(hr);

            GCHandle h = GCHandle.Alloc(b, GCHandleType.Pinned);

            try
            {
                IntPtr ip = h.AddrOfPinnedObject();

                // Convert the byte array to an IntPtr
                Marshal.PtrToStructure(ip, obj);
            }
            finally
            {
                h.Free();
            }
        }

        public static void MFSetAttribute2UINT32asUINT64(IMFAttributes pAttributes, Guid g, int nNumerator, int nDenominator)
        {
            int hr;
            long ul = nNumerator;

            ul <<= 32;
            ul |= (UInt32)nDenominator;

            hr = pAttributes.SetUINT64(g, ul);
            MFError.ThrowExceptionForHR(hr);
        }

        public static void MFGetAttribute2UINT32asUINT64(IMFAttributes pAttributes, Guid g, out int nNumerator, out int nDenominator)
        {
            long ul;
            int hr;

            hr = pAttributes.GetUINT64(g, out ul);
            MFError.ThrowExceptionForHR(hr);

            nDenominator = (int)ul;
            nNumerator = (int)(ul >> 32);
        }

        public static bool AreMediaTypesEqual(IMFMediaType pType1, IMFMediaType pType2)
        {
            if ((pType1 == null) && (pType2 == null))
            {
                return true; // Both are NULL.
            }
            else if ((pType1 == null) || (pType2 == null))
            {
                return false; // One is NULL.
            }

            MFMediaEqual dwFlags;
            int hr = pType1.IsEqual(pType2, out dwFlags);

            return (hr == 0);
        }

        public static int MulDiv(long a, long b, long c)
        {
            // Max Rate = Refresh Rate / Frame Rate
            long l = a * b;

            return (int)(l / c);
        }

        public static int MFTimeToMsec(long time)
        {
            const long ONE_SECOND = 10000000; // One second in hns
            const int ONE_MSEC = 1000;       // One msec in hns

            return (int)(time / (ONE_SECOND / ONE_MSEC));
        }

        public static MFRatio GetFrameRate(IMFMediaType pMediaType)
        {
            int hr;
            long i64;
            MFRatio fps;

            hr = pMediaType.GetUINT64(MFAttributesClsid.MF_MT_FRAME_RATE, out i64);
            MFError.ThrowExceptionForHR(hr);
            fps.Numerator = (int)(i64 >> 32);
            fps.Denominator = (int)i64;

            return fps;
        }

        public static int
        MFGetAttributeUINT32(
            IMFAttributes pAttributes,
            Guid guidKey,
            int unDefault
            )
        {
            int hr;
            int unRet;

            try
            {
                hr = pAttributes.GetUINT32(guidKey, out unRet);
                MFError.ThrowExceptionForHR(hr);
            }
            catch
            {
                unRet = unDefault;
            }

            return unRet;
        }
    }

    class MediaTypeBuilder : IDisposable
    {
        #region Member variables

        protected IMFMediaType m_pType;

        #endregion

        // Construct from an existing media type.
        public MediaTypeBuilder(IMFMediaType pType)
        {
            Debug.Assert(pType != null);

            if (pType != null)
            {
                m_pType = pType;
            }
            else
            {
                throw new Exception("E_POINTER");
            }
        }

        // Create a new media type.
        public MediaTypeBuilder()
        {
            int hr = MFExtern.MFCreateMediaType(out m_pType);
            MFError.ThrowExceptionForHR(hr);
        }

        // Direct wrappers of IMFMediaType methods.
        // (For these methods, we leave parameter validation to the IMFMediaType implementation.)

        // Retrieves the major type GUID.
        public void GetMajorType(out Guid pGuid)
        {
            int hr = GetMediaType().GetMajorType(out pGuid);
            MFError.ThrowExceptionForHR(hr);
        }

        // Specifies whether the media data is compressed
        public void IsCompressedFormat(out bool pbCompressed)
        {
            int hr = GetMediaType().IsCompressedFormat(out pbCompressed);
            MFError.ThrowExceptionForHR(hr);
        }

        // Compares two media types and determines whether they are identical.
        public int IsEqual(IMFMediaType pType, out MFMediaEqual pdwFlags)
        {
            return GetMediaType().IsEqual(pType, out pdwFlags);
        }

        // Retrieves an alternative representation of the media type.
        public void GetRepresentation(Guid guidRepresentation, out IntPtr ppvRepresentation)
        {
            int hr = GetMediaType().GetRepresentation(guidRepresentation, out ppvRepresentation);
            MFError.ThrowExceptionForHR(hr);
        }

        // Frees memory that was allocated by the GetRepresentation method.
        public void FreeRepresentation(Guid guidRepresentation, IntPtr pvRepresentation)
        {
            int hr = GetMediaType().FreeRepresentation(guidRepresentation, pvRepresentation);
            MFError.ThrowExceptionForHR(hr);
        }

        // Helper methods

        // CopyFrom: Copy all of the attributes from another media type into this type.
        public void CopyFrom(MediaTypeBuilder pType)
        {
            if (!pType.IsValid())
            {
                throw new Exception("E_UNEXPECTED");
            }
            if (pType == null)
            {
                throw new Exception("E_POINTER");
            }
            CopyFrom(pType.m_pType);
        }

        public void CopyFrom(IMFMediaType pType)
        {
            if (pType == null)
            {
                throw new Exception("E_POINTER");
            }
            int hr = pType.CopyAllItems(m_pType);
            MFError.ThrowExceptionForHR(hr);
        }

        // Returns the underlying IMFMediaType pointer.
        public void GetMediaType(out IMFMediaType ppType)
        {
            Debug.Assert(IsValid());
            ppType = m_pType;
        }

        // Sets the major type GUID.
        public void SetMajorType(Guid guid)
        {
            int hr = GetMediaType().SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, guid);
            MFError.ThrowExceptionForHR(hr);
        }

        // Retrieves the subtype GUID.
        public void GetSubType(out Guid pGuid)
        {
            int hr = GetMediaType().GetGUID(MFAttributesClsid.MF_MT_SUBTYPE, out pGuid);
            MFError.ThrowExceptionForHR(hr);
        }

        // Sets the subtype GUID.
        public void SetSubType(Guid guid)
        {
            int hr = GetMediaType().SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, guid);
            MFError.ThrowExceptionForHR(hr);
        }

        // Extracts the FOURCC code from the subtype.
        // Not all subtypes follow this pattern.
        public void GetFourCC(out int pFourCC)
        {
            Debug.Assert(IsValid());
            Guid guidSubType;

            GetSubType(out guidSubType);

            FourCC f = new FourCC(guidSubType);

            pFourCC = f.ToInt32();
        }

        //  Queries whether each sample is independent of the other samples in the stream.
        public void GetAllSamplesIndependent(out bool pbIndependent)
        {
            int i;

            int hr = GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_ALL_SAMPLES_INDEPENDENT, out i);
            MFError.ThrowExceptionForHR(hr);

            pbIndependent = i != 0;
        }

        //  Specifies whether each sample is independent of the other samples in the stream.
        public void SetAllSamplesIndependent(bool bIndependent)
        {
            int i = 0;
            if (bIndependent)
                i = 1;
            int hr = GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_ALL_SAMPLES_INDEPENDENT, i);
            MFError.ThrowExceptionForHR(hr);
        }

        // Queries whether the samples have a fixed size.
        public void GetFixedSizeSamples(out bool pbFixed)
        {
            int i;
            int hr = GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_FIXED_SIZE_SAMPLES, out i);
            MFError.ThrowExceptionForHR(hr);

            pbFixed = i != 0;
        }

        // Specifies whether the samples have a fixed size.
        public void SetFixedSizeSamples(bool bFixed)
        {
            int i = 0;
            if (bFixed)
                i = 1;
            GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_FIXED_SIZE_SAMPLES, i);
        }

        // Retrieves the size of each sample, in bytes.
        public void GetSampleSize(out int pnSize)
        {
            int hr = GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_SAMPLE_SIZE, out pnSize);
            MFError.ThrowExceptionForHR(hr);
        }

        // Sets the size of each sample, in bytes.
        public void SetSampleSize(int nSize)
        {
            int hr = GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_SAMPLE_SIZE, nSize);
            MFError.ThrowExceptionForHR(hr);
        }

        // Retrieves a media type that was wrapped by the MFWrapMediaType function.
        public void Unwrap(out IMFMediaType ppOriginal)
        {
            int hr = MFExtern.MFUnwrapMediaType(GetMediaType(), out ppOriginal);
            MFError.ThrowExceptionForHR(hr);
        }

        // The following versions return reasonable defaults if the relevant attribute is not present (zero/FALSE).
        // This is useful for making quick comparisons betweeen media types.

        public bool AllSamplesIndependent()
        {
            return Utils.MFGetAttributeUINT32(GetMediaType(), MFAttributesClsid.MF_MT_ALL_SAMPLES_INDEPENDENT, 0) != 0;
        }

        public bool FixedSizeSamples()
        {
            return Utils.MFGetAttributeUINT32(GetMediaType(), MFAttributesClsid.MF_MT_FIXED_SIZE_SAMPLES, 0) != 0;
        }

        public int SampleSize()
        {
            return Utils.MFGetAttributeUINT32(GetMediaType(), MFAttributesClsid.MF_MT_SAMPLE_SIZE, 0);
        }

        protected bool IsValid()
        {
            return m_pType != null;
        }

        protected IMFMediaType GetMediaType()
        {
            Debug.Assert(IsValid());
            return m_pType;
        }

        #region IDisposable Members

        public void Dispose()
        {
            m_pType = null;
        }

        #endregion
    }

    class VideoTypeBuilder : MediaTypeBuilder
    {

        public VideoTypeBuilder(IMFMediaType pType)
            : base(pType)
        {
            Guid guidMajorType;

            GetMajorType(out guidMajorType);

            if (guidMajorType != MFMediaType.Video)
            {
                throw new Exception("MF_E_INVALIDTYPE");
            }
        }

        public VideoTypeBuilder()
            : base()
        {
            SetMajorType(MFMediaType.Video);
        }

        // Retrieves a description of how the frames are interlaced.
        public void GetInterlaceMode(out MFVideoInterlaceMode pmode)
        {
            int i;
            int hr = GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_INTERLACE_MODE, out i);
            MFError.ThrowExceptionForHR(hr);

            pmode = (MFVideoInterlaceMode)i;
        }

        // Sets a description of how the frames are interlaced.
        public void SetInterlaceMode(MFVideoInterlaceMode mode)
        {
            int hr = GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_INTERLACE_MODE, (int)mode);
            MFError.ThrowExceptionForHR(hr);
        }

        // This returns the default or attempts to compute it, in its absence.
        public void GetDefaultStride(out int pnStride)
        {
            int nStride = 0;
            bool bFailed = false;

            // First try to get it from the attribute.
            try
            {
                int hr = GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_DEFAULT_STRIDE, out nStride);
                MFError.ThrowExceptionForHR(hr);
            }
            catch
            {
                bFailed = true;
            }
            if (bFailed)
            {
                // Attribute not set. See if we can calculate the default stride.
                Guid subtype;

                int width = 0;
                int height = 0;

                // First we need the subtype .
                GetSubType(out subtype);

                // Now we need the image width and height.
                GetFrameDimensions(out width, out height);

                // Now compute the stride for a particular bitmap type
                FourCC f = new FourCC(subtype);
                int hr = MFExtern.MFGetStrideForBitmapInfoHeader(f.ToInt32(), width, out nStride);
                MFError.ThrowExceptionForHR(hr);

                // Set the attribute for later reference.
                SetDefaultStride(nStride);
            }

            pnStride = nStride;
        }

        // Sets the default stride. Only appropriate for uncompressed data formats.
        public void SetDefaultStride(int nStride)
        {
            int hr = GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_DEFAULT_STRIDE, nStride);
            MFError.ThrowExceptionForHR(hr);
        }

        // Retrieves the width and height of the video frame.
        public void GetFrameDimensions(out int pdwWidthInPixels, out int pdwHeightInPixels)
        {
            Utils.MFGetAttribute2UINT32asUINT64(GetMediaType(), MFAttributesClsid.MF_MT_FRAME_SIZE, out pdwWidthInPixels, out pdwHeightInPixels);
        }

        // Sets the width and height of the video frame.
        public void SetFrameDimensions(int dwWidthInPixels, int dwHeightInPixels)
        {
            Utils.MFSetAttribute2UINT32asUINT64(GetMediaType(), MFAttributesClsid.MF_MT_FRAME_SIZE, dwWidthInPixels, dwHeightInPixels);
        }

        // Retrieves the data error rate in bit errors per second
        public void GetDataBitErrorRate(out int pRate)
        {
            int hr = GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_AVG_BIT_ERROR_RATE, out pRate);
            MFError.ThrowExceptionForHR(hr);
        }

        // Sets the data error rate in bit errors per second
        public void SetDataBitErrorRate(int rate)
        {
            int hr = GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_AVG_BIT_ERROR_RATE, rate);
            MFError.ThrowExceptionForHR(hr);
        }

        // Retrieves the approximate data rate of the video stream.
        public void GetAverageBitRate(out int pRate)
        {
            int hr = GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_AVG_BITRATE, out pRate);
            MFError.ThrowExceptionForHR(hr);
        }

        // Sets the approximate data rate of the video stream.
        public void SetAvgerageBitRate(int rate)
        {
            int hr = GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_AVG_BITRATE, rate);
            MFError.ThrowExceptionForHR(hr);
        }

        // Retrieves custom color primaries.
        public void GetCustomVideoPrimaries(out MT_CustomVideoPrimaries pPrimaries)
        {
            pPrimaries = new MT_CustomVideoPrimaries();
            Utils.MFGetBlob(GetMediaType(), MFAttributesClsid.MF_MT_CUSTOM_VIDEO_PRIMARIES, pPrimaries);
        }

        // Sets custom color primaries.
        public void SetCustomVideoPrimaries(MT_CustomVideoPrimaries primary)
        {
            Utils.MFSetBlob(GetMediaType(), MFAttributesClsid.MF_MT_CUSTOM_VIDEO_PRIMARIES, primary);
        }

        // Gets the number of frames per second.
        public void GetFrameRate(out int pnNumerator, out int pnDenominator)
        {
            Utils.MFGetAttribute2UINT32asUINT64(GetMediaType(), MFAttributesClsid.MF_MT_FRAME_RATE, out pnNumerator, out pnDenominator);
        }

        // Gets the frames per second as a ratio.
        public void GetFrameRate(out MFRatio pRatio)
        {
            GetFrameRate(out pRatio.Numerator, out pRatio.Denominator);
        }

        // Sets the number of frames per second.
        public void SetFrameRate(int nNumerator, int nDenominator)
        {
            Utils.MFSetAttribute2UINT32asUINT64(GetMediaType(), MFAttributesClsid.MF_MT_FRAME_RATE, nNumerator, nDenominator);
        }

        // Sets the number of frames per second, as a ratio.
        public void SetFrameRate(MFRatio ratio)
        {
            Utils.MFSetAttribute2UINT32asUINT64(GetMediaType(), MFAttributesClsid.MF_MT_FRAME_RATE, ratio.Numerator, ratio.Denominator);
        }

        // Queries the geometric aperture.
        public void GetGeometricAperture(out MFVideoArea pArea)
        {
            pArea = new MFVideoArea();
            Utils.MFGetBlob(GetMediaType(), MFAttributesClsid.MF_MT_GEOMETRIC_APERTURE, pArea);
        }

        // Sets the geometric aperture.
        public void SetGeometricAperture(MFVideoArea area)
        {
            Utils.MFSetBlob(GetMediaType(), MFAttributesClsid.MF_MT_GEOMETRIC_APERTURE, area);
        }

        // Retrieves the maximum number of frames from one key frame to the next.
        public void GetMaxKeyframeSpacing(out int pnSpacing)
        {
            int hr = GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_MAX_KEYFRAME_SPACING, out pnSpacing);
            MFError.ThrowExceptionForHR(hr);
        }

        // Sets the maximum number of frames from one key frame to the next.
        public void SetMaxKeyframeSpacing(int nSpacing)
        {
            int hr = GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_MAX_KEYFRAME_SPACING, nSpacing);
            MFError.ThrowExceptionForHR(hr);
        }

        // Retrieves the region that contains the valid portion of the signal.
        public void GetMinDisplayAperture(out MFVideoArea pArea)
        {
            pArea = new MFVideoArea();
            Utils.MFGetBlob(GetMediaType(), MFAttributesClsid.MF_MT_MINIMUM_DISPLAY_APERTURE, pArea);
        }

        // Sets the the region that contains the valid portion of the signal.
        public void SetMinDisplayAperture(MFVideoArea area)
        {
            Utils.MFSetBlob(GetMediaType(), MFAttributesClsid.MF_MT_MINIMUM_DISPLAY_APERTURE, area);
        }

        // Retrieves the aspect ratio of the output rectangle for a video media type.
        public void GetPadControlFlags(out MFVideoPadFlags pFlags)
        {
            int i;
            int hr = GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_PAD_CONTROL_FLAGS, out i);
            MFError.ThrowExceptionForHR(hr);
            pFlags = (MFVideoPadFlags)i;
        }

        // Sets the aspect ratio of the output rectangle for a video media type.
        public void SetPadControlFlags(MFVideoPadFlags flags)
        {
            int hr = GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_PAD_CONTROL_FLAGS, (int)flags);
            MFError.ThrowExceptionForHR(hr);
        }

        // Retrieves an array of palette entries for a video media type.
        public void GetPaletteEntries(out MFPaletteEntry[] paEntries, int nEntries)
        {
            paEntries = new MFPaletteEntry[nEntries];
            Utils.MFGetBlob(GetMediaType(), MFAttributesClsid.MF_MT_PALETTE, paEntries);
        }

        // Sets an array of palette entries for a video media type.
        public void SetPaletteEntries(MFPaletteEntry[] paEntries, int nEntries)
        {
            Utils.MFSetBlob(GetMediaType(), MFAttributesClsid.MF_MT_PALETTE, paEntries);
        }

        // Retrieves the number of palette entries.
        public void GetNumPaletteEntries(out int pnEntries)
        {
            int iSize = Marshal.SizeOf(typeof(MFPaletteEntry));
            int nBytes = 0;
            GetMediaType().GetBlobSize(MFAttributesClsid.MF_MT_PALETTE, out nBytes);
            if (nBytes % iSize != 0)
            {
                throw new Exception("E_UNEXPECTED");
            }
            pnEntries = nBytes / iSize;
        }

        // Queries the 4×3 region of video that should be displayed in pan/scan mode.
        public void GetPanScanAperture(out MFVideoArea pArea)
        {
            pArea = new MFVideoArea();
            Utils.MFGetBlob(GetMediaType(), MFAttributesClsid.MF_MT_PAN_SCAN_APERTURE, pArea);
        }

        // Sets the 4×3 region of video that should be displayed in pan/scan mode.
        public void SetPanScanAperture(MFVideoArea area)
        {
            Utils.MFSetBlob(GetMediaType(), MFAttributesClsid.MF_MT_PAN_SCAN_APERTURE, area);
        }

        // Queries whether pan/scan mode is enabled.
        public void IsPanScanEnabled(out bool pBool)
        {
            int i;
            int hr = GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_PAN_SCAN_ENABLED, out i);
            MFError.ThrowExceptionForHR(hr);

            pBool = i != 0;
        }

        // Sets whether pan/scan mode is enabled.
        public void SetPanScanEnabled(bool bEnabled)
        {
            int i = 0;
            if (bEnabled)
            {
                i = 1;
            }
            int hr = GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_PAN_SCAN_ENABLED, i);
            MFError.ThrowExceptionForHR(hr);
        }

        // Queries the pixel aspect ratio
        public void GetPixelAspectRatio(out int pnNumerator, out int pnDenominator)
        {
            Utils.MFGetAttribute2UINT32asUINT64(GetMediaType(), MFAttributesClsid.MF_MT_PIXEL_ASPECT_RATIO, out pnNumerator, out pnDenominator);
        }

        // Sets the pixel aspect ratio
        public void SetPixelAspectRatio(int nNumerator, int nDenominator)
        {
            Utils.MFSetAttribute2UINT32asUINT64(GetMediaType(), MFAttributesClsid.MF_MT_PIXEL_ASPECT_RATIO, nNumerator, nDenominator);
        }

        public void SetPixelAspectRatio(MFRatio ratio)
        {
            Utils.MFSetAttribute2UINT32asUINT64(GetMediaType(), MFAttributesClsid.MF_MT_PIXEL_ASPECT_RATIO, ratio.Numerator, ratio.Denominator);
        }

        // Queries the intended aspect ratio.
        public void GetSourceContentHint(out MFVideoSrcContentHintFlags pFlags)
        {
            int i;
            int hr = GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_SOURCE_CONTENT_HINT, out i);
            MFError.ThrowExceptionForHR(hr);
            pFlags = (MFVideoSrcContentHintFlags)i;
        }

        // Sets the intended aspect ratio.
        public void SetSourceContentHint(MFVideoSrcContentHintFlags nFlags)
        {
            int hr = GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_SOURCE_CONTENT_HINT, (int)nFlags);
            MFError.ThrowExceptionForHR(hr);
        }

        // Queries an enumeration which represents the conversion function from RGB to R'G'B'.
        public void GetTransferFunction(out MFVideoTransferFunction pnFxn)
        {
            int i;
            int hr = GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_TRANSFER_FUNCTION, out i);
            MFError.ThrowExceptionForHR(hr);
            pnFxn = (MFVideoTransferFunction)i;
        }

        // Set an enumeration which represents the conversion function from RGB to R'G'B'.
        public void SetTransferFunction(MFVideoTransferFunction nFxn)
        {
            int hr = GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_TRANSFER_FUNCTION, (int)nFxn);
            MFError.ThrowExceptionForHR(hr);
        }

        // Queries how chroma was sampled for a Y'Cb'Cr' video media type.
        public void GetChromaSiting(out MFVideoChromaSubsampling pSampling)
        {
            int i;
            int hr = GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_VIDEO_CHROMA_SITING, out i);
            MFError.ThrowExceptionForHR(hr);
            pSampling = (MFVideoChromaSubsampling)i;
        }

        // Sets how chroma was sampled for a Y'Cb'Cr' video media type.
        public void SetChromaSiting(MFVideoChromaSubsampling nSampling)
        {
            int hr = GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_VIDEO_CHROMA_SITING, (int)nSampling);
            MFError.ThrowExceptionForHR(hr);
        }

        // Queries the optimal lighting conditions for viewing.
        public void GetVideoLighting(out MFVideoLighting pLighting)
        {
            int i;
            int hr = GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_VIDEO_LIGHTING, out i);
            MFError.ThrowExceptionForHR(hr);
            pLighting = (MFVideoLighting)i;
        }

        // Sets the optimal lighting conditions for viewing.
        public void SetVideoLighting(MFVideoLighting nLighting)
        {
            int hr = GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_VIDEO_LIGHTING, (int)nLighting);
            MFError.ThrowExceptionForHR(hr);
        }

        // Queries the nominal range of the color information in a video media type.
        public void GetVideoNominalRange(out MFNominalRange pRange)
        {
            int i;
            int hr = GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_VIDEO_NOMINAL_RANGE, out i);
            MFError.ThrowExceptionForHR(hr);
            pRange = (MFNominalRange)i;
        }

        // Sets the nominal range of the color information in a video media type.
        public void SetVideoNominalRange(MFNominalRange nRange)
        {
            int hr = GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_VIDEO_NOMINAL_RANGE, (int)nRange);
            MFError.ThrowExceptionForHR(hr);
        }

        // Queries the color primaries for a video media type.
        public void GetVideoPrimaries(out MFVideoPrimaries pPrimaries)
        {
            int i;
            int hr = GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_VIDEO_PRIMARIES, out i);
            MFError.ThrowExceptionForHR(hr);
            pPrimaries = (MFVideoPrimaries)i;
        }

        // Sets the color primaries for a video media type.
        public void SetVideoPrimaries(MFVideoPrimaries nPrimaries)
        {
            int hr = GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_VIDEO_PRIMARIES, (int)nPrimaries);
            MFError.ThrowExceptionForHR(hr);
        }

        // Gets a enumeration representing the conversion matrix from the
        // Y'Cb'Cr' color space to the R'G'B' color space.
        public void GetYUVMatrix(out MFVideoTransferMatrix pMatrix)
        {
            int i;
            int hr = GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_YUV_MATRIX, out i);
            MFError.ThrowExceptionForHR(hr);
            pMatrix = (MFVideoTransferMatrix)i;
        }

        // Sets an enumeration representing the conversion matrix from the
        // Y'Cb'Cr' color space to the R'G'B' color space.
        public void SetYUVMatrix(MFVideoTransferMatrix nMatrix)
        {
            int hr = GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_YUV_MATRIX, (int)nMatrix);
            MFError.ThrowExceptionForHR(hr);
        }

        //
        // The following versions return reasonable defaults if the relevant attribute is not present (zero/FALSE).
        // This is useful for making quick comparisons betweeen media types.
        //

        public MFRatio GetPixelAspectRatio() // Defaults to 1:1 (square pixels)
        {
            MFRatio PAR = new MFRatio();

            try
            {
                Utils.MFGetAttribute2UINT32asUINT64(GetMediaType(), MFAttributesClsid.MF_MT_PIXEL_ASPECT_RATIO, out PAR.Numerator, out PAR.Denominator);
            }
            catch
            {
                PAR.Numerator = 1;
                PAR.Denominator = 1;
            }
            return PAR;
        }

        public bool IsPanScanEnabled() // Defaults to FALSE
        {
            return Utils.MFGetAttributeUINT32(GetMediaType(), MFAttributesClsid.MF_MT_PAN_SCAN_ENABLED, 0) != 0;
        }

        // Returns (in this order)
        // 1. The pan/scan region, only if pan/scan mode is enabled.
        // 2. The geometric aperture.
        // 3. The entire video area.
        public void GetVideoDisplayArea(out MFVideoArea pArea)
        {
            int hr = 0;
            bool bPanScan = false;
            int width = 0, height = 0;
            pArea = new MFVideoArea();

            bPanScan = Utils.MFGetAttributeUINT32(GetMediaType(), MFAttributesClsid.MF_MT_PAN_SCAN_ENABLED, 0) != 0;

            // In pan/scan mode, try to get the pan/scan region.
            if (bPanScan)
            {
                try
                {
                    Utils.MFGetBlob(GetMediaType(), MFAttributesClsid.MF_MT_PAN_SCAN_APERTURE, pArea);
                }
                catch (Exception e)
                {
                    hr = Marshal.GetHRForException(e);
                }
            }

            // If not in pan/scan mode, or there is not pan/scan region, get the geometric aperture.
            if (!bPanScan || hr == MFError.MF_E_ATTRIBUTENOTFOUND)
            {
                try
                {
                    Utils.MFGetBlob(GetMediaType(), MFAttributesClsid.MF_MT_GEOMETRIC_APERTURE, pArea);
                    hr = 0;
                }
                catch (Exception e)
                {
                    hr = Marshal.GetHRForException(e);
                }

                // Default: Use the entire video area.
                if (hr == MFError.MF_E_ATTRIBUTENOTFOUND)
                {
                    Utils.MFGetAttribute2UINT32asUINT64(GetMediaType(), MFAttributesClsid.MF_MT_FRAME_SIZE, out width, out height);
                    pArea.MakeArea(0, 0, width, height);
                }
            }

        }

    }
}
