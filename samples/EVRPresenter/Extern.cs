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
using System.Windows.Forms;

namespace EVRPresenter
{
    public enum PeekFlags
    {
        NOREMOVE = 0x0000,
        REMOVE = 0x0001,
        NOYIELD = 0x0002
    }

    public enum WakeMask
    {
        KEY = 0x0001,
        MOUSEMOVE = 0x0002,
        MOUSEBUTTON = 0x0004,
        POSTMESSAGE = 0x0008,
        TIMER = 0x0010,
        PAINT = 0x0020,
        SENDMESSAGE = 0x0040,
        HOTKEY = 0x0080,
        ALLPOSTMESSAGE = 0x0100
    }

    public enum MonitorFlags
    {
        DEFAULTTONULL = 0x00000000,
        DEFAULTTOPRIMARY = 0x00000001,
        DEFAULTTONEAREST = 0x00000002
    }

    class Extern
    {
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject(IntPtr a);

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr a);

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(
          IntPtr hwnd,       // handle to a window 
          MonitorFlags dwFlags    // determine return value 
        );

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetClientRect(
          IntPtr hwnd,       // handle to a window 
          out RECT r    // determine return value 
        );

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll")]
        public static extern int ReleaseDC(
          IntPtr hwnd,       // handle to a window 
          IntPtr hdc    // determine return value 
        );

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("coredll.dll")]
        public static extern IntPtr CreateSolidBrush(
          int c
        );

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll")]
        public static extern int FillRect(
            IntPtr hDC,           // handle to DC
            RECT lprc,  // rectangle
            IntPtr hbr         // handle to brush
        );

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll")]
        public static extern int MsgWaitForMultipleObjects(uint nCount, IntPtr[] pHandles,
           bool bWaitAll, int dwMilliseconds, WakeMask dwWakeMask);

        [System.Security.SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool PeekMessage(out Message message, IntPtr handle, uint filterMin, uint filterMax, PeekFlags flags);


        [DllImport("user32.dll")]
        public extern static int PostThreadMessage(
            int handle,
            int msg,
            IntPtr wParam,
            IntPtr lParam
         );

        [DllImport("user32.dll")]
        public extern static bool IsWindow(IntPtr x);

    }

    class Utils
    {
        public static void MFSetBlob(IMFAttributes p, Guid g, object o)
        {
            int iSize = Marshal.SizeOf(o);
            IntPtr ip = Marshal.AllocCoTaskMem(iSize);
            byte[] b;
            try
            {
                Marshal.PtrToStructure(ip, o);
                b = new byte[iSize];
                Marshal.Copy(ip, b, 0, iSize);
            }
            finally
            {
                Marshal.FreeCoTaskMem(ip);
            }
            p.SetBlob(MFAttributesClsid.MF_MT_CUSTOM_VIDEO_PRIMARIES, b, iSize);
        }

        public static IntPtr MFGetBlob(IMFAttributes p, Guid g)
        {
            int iSize;
            int i;

            p.GetBlobSize(g, out iSize);
            byte[] b = new byte[iSize];
            p.GetBlob(MFAttributesClsid.MF_MT_CUSTOM_VIDEO_PRIMARIES, b, iSize, out i);

            IntPtr ip = Marshal.AllocCoTaskMem(iSize);
            Marshal.Copy(b, 0, ip, iSize);

            return ip;
        }

        public static void MFSetAttributeRatio(IMFAttributes pAttributes, Guid g, int nNumerator, int nDenominator)
        {
            pAttributes.SetUINT64(g, (nNumerator << 32) | nDenominator);
        }

        public static void MFGetAttributeRatio(IMFAttributes pAttributes, Guid g, out int nNumerator, out int nDenominator)
        {
            long l;
            pAttributes.GetUINT64(g, out l);
            nDenominator = (int)l;
            nNumerator = (int)(l >> 32);
        }

        public static void MFGetAttributeSize(IMFAttributes pAttributes, Guid g, out int width, out int height)
        {
            long l;
            pAttributes.GetUINT64(g, out l);
            height = (int)l;
            width = (int)(l >> 32);
        }

        public static void MFSetAttributeSize(IMFAttributes pAttributes, Guid g, int width, int height)
        {
            pAttributes.SetUINT64(g, (width << 32) | width);
        }

        public static MFOffset MakeOffset(float v)
        {
            MFOffset offset;
            offset.Value = (short)v;
            offset.fract = (short)(65536 * (v - offset.Value));
            return offset;
        }

        public static MFVideoArea MakeArea(float x, float y, int width, int height)
        {
            MFVideoArea area = new MFVideoArea();

            area.OffsetX = Utils.MakeOffset(x);
            area.OffsetY = Utils.MakeOffset(y);
            area.Area.cx = width;
            area.Area.cy = height;

            return area;
        }

        public static int GetOffset(MFOffset offset)
        {
            return (int)((float)offset.Value + offset.fract / 65536.0f);
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

            MFMediaEqual dwFlags = MFMediaEqual.None;
            int hr = pType1.IsEqual(pType2, out dwFlags);

            return (hr == 0);
        }

        public static int
        MFGetAttributeUINT32(
            IMFAttributes pAttributes,
            Guid guidKey,
            int unDefault
            )
        {
            int unRet;

            try
            {
                pAttributes.GetUINT32(guidKey, out unRet);
            }
            catch
            {
                unRet = unDefault;
            }
            return unRet;
        }

        public static int MulDiv(int a, int b, int c)
        {
            // Max Rate = Refresh Rate / Frame Rate
            long l = a * b;
            return (int)(l / c);
        }

        public static int RGB(int r, int g, int b)
        {
            return r | (g << 8) | (b << 16);
        }

        public static int D3DCOLOR_ARGB(int a, int r, int g, int b)
        {
            return ((((a) & 0xff) << 24) | (((r) & 0xff) << 16) | (((g) & 0xff) << 8) | ((b) & 0xff));
        }
        const Int64 ONE_SECOND = 10000000; // One second in hns
        const int ONE_MSEC = 1000;       // One msec in hns 

        public static int MFTimeToMsec(Int64 time)
        {
            return (int)(time / (ONE_SECOND / ONE_MSEC));
        }

        public static bool EqualRect(
            RECT lprc1,
            RECT lprc2
        )
        {
            return (
                lprc1.bottom == lprc2.bottom &&
                lprc1.left == lprc2.left &&
                lprc1.right == lprc2.right &&
                lprc1.top == lprc2.top);
        }

        public static bool IsRectEmpty(
            RECT lprc)
        {
            return (lprc.right <= lprc.left || lprc.bottom <= lprc.top);
        }

        public static MFRatio GetFrameRate(IMFMediaType pMediaType)
        {
            Int64 i64;
            MFRatio fps;

            pMediaType.GetUINT64(MFAttributesClsid.MF_MT_FRAME_RATE, out i64);
            fps.Numerator = (int)(i64 >> 32);
            fps.Denominator = (int)i64;

            return fps;
        }
        public static float MFOffsetToFloat(MFOffset offset)
        {
            return (float)offset.Value + (float)offset.Value / 65536.0f;
        }

    }

    public class AsyncCallback : COMBase, IMFAsyncCallback
    {
        private EVRCustomPresenter m_pParent;

        public AsyncCallback(EVRCustomPresenter pParent)
        {
            m_pParent = pParent;
        }

        #region IMFAsyncCallback Members

        public void GetParameters(out MFASync pdwFlags, out MFAsyncCallbackQueue pdwQueue)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Invoke(IMFAsyncResult pAsyncResult)
        {
            m_pParent.OnSampleFree(pAsyncResult);
        }

        #endregion
    }

    class MediaTypeBuilder
    {
        protected IMFMediaType m_pType;

        protected bool IsValid()
        {
            return m_pType != null;
        }

        protected IMFMediaType GetMediaType()
        {
            Debug.Assert(IsValid());
            return m_pType;
        }

        // Construct from an existing media type.
        protected MediaTypeBuilder(IMFMediaType pType)
        {
            Debug.Assert(pType != null);

            if (pType == null)
            {
                throw new Exception("E_POINTER");
            }
            else
            {
                m_pType = pType;
            }
        }

        // Create a new media type.
        protected MediaTypeBuilder()
        {
            MFExtern.MFCreateMediaType(out m_pType);
        }

#if false
        //~MediaTypeBuilder()
        //{
            //Marshal.ReleaseComObject(m_pType);
        //}

        // Static creation functions. 

        // Use this version to create a new media type.
        template <class T>
        public static HRESULT Create(T** ppTypeBuilder)
        {
            if (ppTypeBuilder == NULL)
            {
                return E_POINTER;
            }

            HRESULT hr = S_OK;

            T *pTypeBuilder = new T(hr);
            if (pTypeBuilder == NULL)
            {
                return E_OUTOFMEMORY;
            }

            if (SUCCEEDED(hr))
            {
                *ppTypeBuilder = pTypeBuilder;
                (*ppTypeBuilder).AddRef();
            }
            SAFE_RELEASE(pTypeBuilder);
            return hr;
        }

        // Use this version to initialize from an existing media type.
        template <class T>
        public static HRESULT Create(IMFMediaType *pType, T** ppTypeBuilder)
        {
            if (ppTypeBuilder == NULL)
            {
                return E_POINTER;
            }

            HRESULT hr = S_OK;

            T *pTypeBuilder = new T(pType, hr);
            if (pTypeBuilder == NULL)
            {
                return E_OUTOFMEMORY;
            }

            if (SUCCEEDED(hr))
            {
                *ppTypeBuilder = pTypeBuilder;
            }
            SAFE_RELEASE(pTypeBuilder);
            return hr;
        }
#endif
        public static void Create(out MediaTypeBuilder ppTypeBuilder)
        {
            ppTypeBuilder = new MediaTypeBuilder();
        }

        public static void Create(IMFMediaType pType, out MediaTypeBuilder ppTypeBuilder)
        {
            ppTypeBuilder = new MediaTypeBuilder(pType);
        }

        // Direct wrappers of IMFMediaType methods.
        // (For these methods, we leave parameter validation to the IMFMediaType implementation.)

        // Retrieves the major type GUID.
        public void GetMajorType(out Guid pGuid)
        {
            GetMediaType().GetMajorType(out pGuid);
        }

        // Specifies whether the media data is compressed
        public void IsCompressedFormat(out bool pbCompressed)
        {
            GetMediaType().IsCompressedFormat(out pbCompressed);
        }

        // Compares two media types and determines whether they are identical.
        public int IsEqual(IMFMediaType pType, out MFMediaEqual pdwFlags)
        {
            return GetMediaType().IsEqual(pType, out pdwFlags);
        }

        // Retrieves an alternative representation of the media type.
        public void GetRepresentation(Guid guidRepresentation, out IntPtr ppvRepresentation)
        {
            GetMediaType().GetRepresentation(guidRepresentation, out ppvRepresentation);
        }

        // Frees memory that was allocated by the GetRepresentation method.
        public void FreeRepresentation(Guid guidRepresentation, IntPtr pvRepresentation)
        {
            GetMediaType().FreeRepresentation(guidRepresentation, pvRepresentation);
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
            pType.CopyAllItems(m_pType);
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
            GetMediaType().SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, guid);
        }

        // Retrieves the subtype GUID.
        public void GetSubType(out Guid pGuid)
        {
            GetMediaType().GetGUID(MFAttributesClsid.MF_MT_SUBTYPE, out pGuid);
        }

        // Sets the subtype GUID.
        public void SetSubType(Guid guid)
        {
            GetMediaType().SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, guid);
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
            GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_ALL_SAMPLES_INDEPENDENT, out i);

            pbIndependent = i != 0;
        }

        //  Specifies whether each sample is independent of the other samples in the stream.
        public void SetAllSamplesIndependent(bool bIndependent)
        {
            int i = 0;
            if (bIndependent)
                i = 1;
            GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_ALL_SAMPLES_INDEPENDENT, i);
        }

        // Queries whether the samples have a fixed size.
        public void GetFixedSizeSamples(out bool pbFixed)
        {
            int i;
            GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_FIXED_SIZE_SAMPLES, out i);

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
            GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_SAMPLE_SIZE, out pnSize);
        }

        // Sets the size of each sample, in bytes. 
        public void SetSampleSize(int nSize)
        {
            GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_SAMPLE_SIZE, nSize);
        }

        // Retrieves a media type that was wrapped by the MFWrapMediaType function.
        public void Unwrap(out IMFMediaType ppOriginal)
        {
            MFExtern.MFUnwrapMediaType(GetMediaType(), out ppOriginal);
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
    }

    class VideoTypeBuilder : MediaTypeBuilder
    {

        public static void Create(out VideoTypeBuilder ppTypeBuilder)
        {
            ppTypeBuilder = new VideoTypeBuilder();
        }

        public static void Create(IMFMediaType pType, out VideoTypeBuilder ppTypeBuilder)
        {
            ppTypeBuilder = new VideoTypeBuilder(pType);
        }

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
            GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_INTERLACE_MODE, out i);

            pmode = (MFVideoInterlaceMode)i;
        }

        // Sets a description of how the frames are interlaced.
        public void SetInterlaceMode(MFVideoInterlaceMode mode)
        {
            GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_INTERLACE_MODE, (int)mode);
        }

        // This returns the default or attempts to compute it, in its absence.
        public void GetDefaultStride(out int pnStride)
        {
            int nStride = 0;
            bool bFailed = false;

            // First try to get it from the attribute.
            try
            {
                GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_DEFAULT_STRIDE, out nStride);
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
                MFExtern.MFGetStrideForBitmapInfoHeader(f.ToInt32(), width, out nStride);

                // Set the attribute for later reference.
                SetDefaultStride(nStride);
            }

            pnStride = nStride;
        }


        // Sets the default stride. Only appropriate for uncompressed data formats.
        public void SetDefaultStride(int nStride)
        {
            GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_DEFAULT_STRIDE, (int)nStride);
        }

        // Retrieves the width and height of the video frame.
        public void GetFrameDimensions(out int pdwWidthInPixels, out int pdwHeightInPixels)
        {
            Utils.MFGetAttributeSize(GetMediaType(), MFAttributesClsid.MF_MT_FRAME_SIZE, out pdwWidthInPixels, out pdwHeightInPixels);
        }

        // Sets the width and height of the video frame.
        public void SetFrameDimensions(int dwWidthInPixels, int dwHeightInPixels)
        {
            Utils.MFSetAttributeSize(GetMediaType(), MFAttributesClsid.MF_MT_FRAME_SIZE, dwWidthInPixels, dwHeightInPixels);
        }

        // Retrieves the data error rate in bit errors per second
        public void GetDataBitErrorRate(out int pRate)
        {
            GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_AVG_BIT_ERROR_RATE, out pRate);
        }

        // Sets the data error rate in bit errors per second
        public void SetDataBitErrorRate(int rate)
        {
            GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_AVG_BIT_ERROR_RATE, rate);
        }

        // Retrieves the approximate data rate of the video stream.
        public void GetAverageBitRate(out int pRate)
        {
            GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_AVG_BITRATE, out pRate);
        }

        // Sets the approximate data rate of the video stream.
        public void SetAvgerageBitRate(int rate)
        {
            GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_AVG_BITRATE, rate);
        }

        // Retrieves custom color primaries.
        public void GetCustomVideoPrimaries(out MT_CustomVideoPrimaries pPrimaries)
        {
            pPrimaries = new MT_CustomVideoPrimaries();
            IntPtr ip = Utils.MFGetBlob(GetMediaType(), MFAttributesClsid.MF_MT_CUSTOM_VIDEO_PRIMARIES);
            try
            {
                Marshal.PtrToStructure(ip, pPrimaries);
            }
            finally
            {
                Marshal.FreeCoTaskMem(ip);
            }
        }

        // Sets custom color primaries.
        public void SetCustomVideoPrimaries(MT_CustomVideoPrimaries primary)
        {
            Utils.MFSetBlob(GetMediaType(), MFAttributesClsid.MF_MT_CUSTOM_VIDEO_PRIMARIES, primary);
        }

        // Gets the number of frames per second.
        public void GetFrameRate(out int pnNumerator, out int pnDenominator)
        {
            Utils.MFGetAttributeRatio(GetMediaType(), MFAttributesClsid.MF_MT_FRAME_RATE, out pnNumerator, out pnDenominator);
        }

        // Gets the frames per second as a ratio.
        public void GetFrameRate(out MFRatio pRatio)
        {
            GetFrameRate(out pRatio.Numerator, out pRatio.Denominator);
        }

        // Sets the number of frames per second.
        public void SetFrameRate(int nNumerator, int nDenominator)
        {
            Utils.MFSetAttributeRatio(GetMediaType(), MFAttributesClsid.MF_MT_FRAME_RATE, nNumerator, nDenominator);
        }

        // Sets the number of frames per second, as a ratio.
        public void SetFrameRate(MFRatio ratio)
        {
            Utils.MFSetAttributeRatio(GetMediaType(), MFAttributesClsid.MF_MT_FRAME_RATE, ratio.Numerator, ratio.Denominator);
        }

        // Queries the geometric aperture.
        public void GetGeometricAperture(out MFVideoArea pArea)
        {
            pArea = new MFVideoArea();
            IntPtr ip = Utils.MFGetBlob(GetMediaType(), MFAttributesClsid.MF_MT_GEOMETRIC_APERTURE);
            try
            {
                Marshal.PtrToStructure(ip, pArea);
            }
            finally
            {
                Marshal.FreeCoTaskMem(ip);
            }
        }

        // Sets the geometric aperture.
        public void SetGeometricAperture(MFVideoArea area)
        {
            Utils.MFSetBlob(GetMediaType(), MFAttributesClsid.MF_MT_GEOMETRIC_APERTURE, area);
        }

        // Retrieves the maximum number of frames from one key frame to the next.
        public void GetMaxKeyframeSpacing(out int pnSpacing)
        {
            GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_MAX_KEYFRAME_SPACING, out pnSpacing);
        }

        // Sets the maximum number of frames from one key frame to the next.
        public void SetMaxKeyframeSpacing(int nSpacing)
        {
            GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_MAX_KEYFRAME_SPACING, nSpacing);
        }

        // Retrieves the region that contains the valid portion of the signal.
        public void GetMinDisplayAperture(out MFVideoArea pArea)
        {
            pArea = new MFVideoArea();
            IntPtr ip = Utils.MFGetBlob(GetMediaType(), MFAttributesClsid.MF_MT_MINIMUM_DISPLAY_APERTURE);
            try
            {
                Marshal.PtrToStructure(ip, pArea);
            }
            finally
            {
                Marshal.FreeCoTaskMem(ip);
            }
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
            GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_PAD_CONTROL_FLAGS, out i);
            pFlags = (MFVideoPadFlags)i;
        }

        // Sets the aspect ratio of the output rectangle for a video media type. 
        public void SetPadControlFlags(MFVideoPadFlags flags)
        {
            GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_PAD_CONTROL_FLAGS, (int)flags);
        }

        // Retrieves an array of palette entries for a video media type. 
        public void GetPaletteEntries(out MFPaletteEntry[] paEntries, int nEntries)
        {
            paEntries = new MFPaletteEntry[nEntries];
            IntPtr ip = Utils.MFGetBlob(GetMediaType(), MFAttributesClsid.MF_MT_PALETTE);
            try
            {
                Marshal.PtrToStructure(ip, paEntries);
            }
            finally
            {
                Marshal.FreeCoTaskMem(ip);
            }
        }

        // Sets an array of palette entries for a video media type. 
        public void SetPaletteEntries(MFPaletteEntry paEntries, int nEntries)
        {
            Utils.MFSetBlob(GetMediaType(), MFAttributesClsid.MF_MT_PALETTE, paEntries);
        }

        // Retrieves the number of palette entries.
        public void GetNumPaletteEntries(out int pnEntries)
        {
            int nBytes = 0;
            GetMediaType().GetBlobSize(MFAttributesClsid.MF_MT_PALETTE, out nBytes);
            if (nBytes % Marshal.SizeOf(typeof(MFPaletteEntry)) != 0)
            {
                throw new Exception("E_UNEXPECTED");
            }
            pnEntries = nBytes / Marshal.SizeOf(typeof(MFPaletteEntry));
        }

        // Queries the 4×3 region of video that should be displayed in pan/scan mode.
        public void GetPanScanAperture(out MFVideoArea pArea)
        {
            pArea = new MFVideoArea();
            IntPtr ip = Utils.MFGetBlob(GetMediaType(), MFAttributesClsid.MF_MT_PAN_SCAN_APERTURE);
            try
            {
                Marshal.PtrToStructure(ip, pArea);
            }
            finally
            {
                Marshal.FreeCoTaskMem(ip);
            }
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
            GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_PAN_SCAN_ENABLED, out i);

            pBool = i != 0;
        }

        // Sets whether pan/scan mode is enabled.
        public void SetPanScanEnabled(bool bEnabled)
        {
            int i = 0;
            if (bEnabled)
                i = 1;
            GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_PAN_SCAN_ENABLED, i);
        }

        // Queries the pixel aspect ratio
        public void GetPixelAspectRatio(out int pnNumerator, out int pnDenominator)
        {
            Utils.MFGetAttributeRatio(GetMediaType(), MFAttributesClsid.MF_MT_PIXEL_ASPECT_RATIO, out pnNumerator, out pnDenominator);
        }

        // Sets the pixel aspect ratio
        public void SetPixelAspectRatio(int nNumerator, int nDenominator)
        {
            Utils.MFSetAttributeRatio(GetMediaType(), MFAttributesClsid.MF_MT_PIXEL_ASPECT_RATIO, nNumerator, nDenominator);
        }

        public void SetPixelAspectRatio(MFRatio ratio)
        {
            Utils.MFSetAttributeRatio(GetMediaType(), MFAttributesClsid.MF_MT_PIXEL_ASPECT_RATIO, ratio.Numerator, ratio.Denominator);
        }

        // Queries the intended aspect ratio.
        public void GetSourceContentHint(out MFVideoSrcContentHintFlags pFlags)
        {
            int i;
            GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_SOURCE_CONTENT_HINT, out i);
            pFlags = (MFVideoSrcContentHintFlags)i;
        }

        // Sets the intended aspect ratio.
        public void SetSourceContentHint(MFVideoSrcContentHintFlags nFlags)
        {
            GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_SOURCE_CONTENT_HINT, (int)nFlags);
        }

        // Queries an enumeration which represents the conversion function from RGB to R'G'B'.
        public void GetTransferFunction(out MFVideoTransferFunction pnFxn)
        {
            int i;
            GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_TRANSFER_FUNCTION, out i);
            pnFxn = (MFVideoTransferFunction)i;
        }

        // Set an enumeration which represents the conversion function from RGB to R'G'B'.
        public void SetTransferFunction(MFVideoTransferFunction nFxn)
        {
            GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_TRANSFER_FUNCTION, (int)nFxn);
        }

        // Queries how chroma was sampled for a Y'Cb'Cr' video media type.
        public void GetChromaSiting(out MFVideoChromaSubsampling pSampling)
        {
            int i;
            GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_VIDEO_CHROMA_SITING, out i);
            pSampling = (MFVideoChromaSubsampling)i;
        }

        // Sets how chroma was sampled for a Y'Cb'Cr' video media type.
        public void SetChromaSiting(MFVideoChromaSubsampling nSampling)
        {
            GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_VIDEO_CHROMA_SITING, (int)nSampling);
        }

        // Queries the optimal lighting conditions for viewing.
        public void GetVideoLighting(out MFVideoLighting pLighting)
        {
            int i;
            GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_VIDEO_LIGHTING, out i);
            pLighting = (MFVideoLighting)i;
        }

        // Sets the optimal lighting conditions for viewing.
        public void SetVideoLighting(MFVideoLighting nLighting)
        {
            GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_VIDEO_LIGHTING, (int)nLighting);
        }

        // Queries the nominal range of the color information in a video media type. 
        public void GetVideoNominalRange(out MFNominalRange pRange)
        {
            int i;
            GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_VIDEO_NOMINAL_RANGE, out i);
            pRange = (MFNominalRange)i;
        }

        // Sets the nominal range of the color information in a video media type. 
        public void SetVideoNominalRange(MFNominalRange nRange)
        {
            GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_VIDEO_NOMINAL_RANGE, (int)nRange);
        }

        // Queries the color primaries for a video media type.
        public void GetVideoPrimaries(out MFVideoPrimaries pPrimaries)
        {
            int i;
            GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_VIDEO_PRIMARIES, out i);
            pPrimaries = (MFVideoPrimaries)i;
        }

        // Sets the color primaries for a video media type.
        public void SetVideoPrimaries(MFVideoPrimaries nPrimaries)
        {
            GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_VIDEO_PRIMARIES, (int)nPrimaries);
        }

        // Gets a enumeration representing the conversion matrix from the 
        // Y'Cb'Cr' color space to the R'G'B' color space.
        public void GetYUVMatrix(out MFVideoTransferMatrix pMatrix)
        {
            int i;
            GetMediaType().GetUINT32(MFAttributesClsid.MF_MT_YUV_MATRIX, out i);
            pMatrix = (MFVideoTransferMatrix)i;
        }

        // Sets an enumeration representing the conversion matrix from the 
        // Y'Cb'Cr' color space to the R'G'B' color space.
        public void SetYUVMatrix(MFVideoTransferMatrix nMatrix)
        {
            GetMediaType().SetUINT32(MFAttributesClsid.MF_MT_YUV_MATRIX, (int)nMatrix);
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
                Utils.MFGetAttributeRatio(GetMediaType(), MFAttributesClsid.MF_MT_PIXEL_ASPECT_RATIO, out PAR.Numerator, out PAR.Denominator);
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

            try
            {
                bPanScan = Utils.MFGetAttributeUINT32(GetMediaType(), MFAttributesClsid.MF_MT_PAN_SCAN_ENABLED, 0) != 0;
            }
            catch { }

            // In pan/scan mode, try to get the pan/scan region.
            if (bPanScan)
            {
                try
                {
                    IntPtr ip = Utils.MFGetBlob(GetMediaType(), MFAttributesClsid.MF_MT_PAN_SCAN_APERTURE);
                    try
                    {
                        Marshal.PtrToStructure(ip, pArea);
                    }
                    finally
                    {
                        Marshal.FreeCoTaskMem(ip);
                    }
                }
                catch (Exception e)
                {
                    hr = Marshal.GetHRForException(e);
                }
            }

            // If not in pan/scan mode, or there is not pan/scan region, get the geometric aperture.
            if (!bPanScan || hr == MFError.MF_E_ATTRIBUTENOTFOUND)
            {
                IntPtr ip = Utils.MFGetBlob(GetMediaType(), MFAttributesClsid.MF_MT_GEOMETRIC_APERTURE);
                try
                {
                    Marshal.PtrToStructure(ip, pArea);
                }
                finally
                {
                    Marshal.FreeCoTaskMem(ip);
                }
            }

            // Default: Use the entire video area.
            if (!bPanScan || hr == MFError.MF_E_ATTRIBUTENOTFOUND)
            {
                Utils.MFGetAttributeSize(GetMediaType(), MFAttributesClsid.MF_MT_FRAME_SIZE, out width, out height);
                pArea.MakeArea(0, 0, width, height);
            }

        }

    }
}
