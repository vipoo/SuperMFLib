#region license

/*
MediaFoundationLib - Provide access to MediaFoundation interfaces via .NET
Copyright (C) 2007
http://mfnet.sourceforge.net

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

#endregion

using System;
using System.Runtime.InteropServices;

using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.Transform;

namespace MediaFoundation.EVR
{
    #region Declarations

#if ALLOW_UNTESTED_INTERFACES

    [UnmanagedName("Unnamed enum")]
    public enum DXVA2Filters
    {
        NoiseFilterLumaLevel = 1,
        NoiseFilterLumaThreshold = 2,
        NoiseFilterLumaRadius = 3,
        NoiseFilterChromaLevel = 4,
        NoiseFilterChromaThreshold = 5,
        NoiseFilterChromaRadius = 6,
        DetailFilterLumaLevel = 7,
        DetailFilterLumaThreshold = 8,
        DetailFilterLumaRadius = 9,
        DetailFilterChromaLevel = 10,
        DetailFilterChromaThreshold = 11,
        DetailFilterChromaRadius = 12
    }

    [UnmanagedName("MFVP_MESSAGE_TYPE")]
    public enum MFVPMessageType
    {
        Flush,
        InvalidateMediaType,
        ProcessInputNotify,
        BeginStreaming,
        EndStreaming,
        EndOfStream,
        Step,
        CancelStep
    }

    [UnmanagedName("MF_SERVICE_LOOKUP_TYPE")]
    public enum MFServiceLookupType
    {
        Upstream,
        UpstreamDirect,
        Downstream,
        DownstreamDirect,
        All,
        Global
    }

    [Flags, UnmanagedName("MFVideoAlphaBitmapFlags")]
    public enum MFVideoAlphaBitmapFlags
    {
        None = 0,
        EntireDDS = 0x00000001,
        SrcColorKey = 0x00000002,
        SrcRect = 0x00000004,
        DestRect = 0x00000008,
        FilterMode = 0x00000010,
        Alpha = 0x00000020,
        BitMask = 0x0000003f
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVA2_ProcAmpValues")]
    public struct DXVA2ProcAmpValues
    {
        int Brightness;
        int Contrast;
        int Hue;
        int Saturation;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVA2_ValueRange")]
    public struct DXVA2ValueRange
    {
        int MinValue;
        int MaxValue;
        int DefaultValue;
        int StepSize;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVA2_VideoProcessorCaps")]
    public struct DXVA2VideoProcessorCaps
    {
        int DeviceCaps;
        int InputPool;
        int NumForwardRefSamples;
        int NumBackwardRefSamples;
        int Reserved;
        int DeinterlaceTechnology;
        int ProcAmpControlCaps;
        int VideoProcessorOperations;
        int NoiseFilterTechnology;
        int DetailFilterTechnology;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFVideoAlphaBitmapParams")]
    public struct MFVideoAlphaBitmapParams
    {
        MFVideoAlphaBitmapFlags dwFlags;
        int clrSrcKey;
        RECT rcSrc;
        MFVideoNormalizedRect nrcDest;
        float fAlpha;
        int dwFilterMode;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFVideoAlphaBitmap")]
    public struct MFVideoAlphaBitmap
    {
        bool GetBitmapFromDC;
        IntPtr stru;
        MFVideoAlphaBitmapParams paras;
    }

    [Flags, UnmanagedName("DXVA2_ProcAmp_* defines")]
    public enum DXVA2ProcAmp
    {
        None = 0,
        Brightness = 0x0001,
        Contrast = 0x0002,
        Hue = 0x0004,
        Saturation = 0x0008
    }

#endif

    [Flags, UnmanagedName("MFVideoRenderPrefs")]
    public enum MFVideoRenderPrefs
    {
        None = 0,
        DoNotRenderBorder = 0x00000001,
        DoNotClipToDevice = 0x00000002,
        Mask = 0x00000003
    }

    [Flags, UnmanagedName("MFVideoAspectRatioMode")]
    public enum MFVideoAspectRatioMode
    {
        None = 0x00000000,
        PreservePicture = 0x00000001,
        PreservePixel = 0x00000002,
        NonLinearStretch = 0x00000004,
        Mask = 0x00000007
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4), UnmanagedName("MFVideoNormalizedRect")]
    public class MFVideoNormalizedRect
    {
        public float left;
        public float top;
        public float right;
        public float bottom;
    }

    #endregion

    #region Interfaces

#if ALLOW_UNTESTED_INTERFACES

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("814C7B20-0FDB-4eec-AF8F-F957C8F69EDC")]
    public interface IMFVideoMixerBitmap
    {
        void SetAlphaBitmap( 
            [In] MFVideoAlphaBitmap pBmpParms);
        
        void ClearAlphaBitmap( );
        
        void UpdateAlphaBitmapParameters( 
            [In] MFVideoAlphaBitmapParams pBmpParms);
        
        void GetAlphaBitmapParameters( 
            out MFVideoAlphaBitmapParams pBmpParms);
        
    }
    
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("6AB0000C-FECE-4d1f-A2AC-A9573530656E")]
    public interface IMFVideoProcessor
    {
        void GetAvailableVideoProcessorModes( 
            out int lpdwNumProcessingModes,
            out IntPtr ppVideoProcessingModes);
        
        void GetVideoProcessorCaps( 
            [In] Guid lpVideoProcessorMode,
            out DXVA2VideoProcessorCaps lpVideoProcessorCaps);
        
        void GetVideoProcessorMode( 
            out Guid lpMode);
        
        void SetVideoProcessorMode(
            [In] Guid lpMode);
        
        void GetProcAmpRange( 
            int dwProperty,
            out DXVA2ValueRange pPropRange);
        
        void GetProcAmpValues(
            DXVA2ProcAmp dwFlags,
            out DXVA2ProcAmpValues Values);
        
        void SetProcAmpValues(
            DXVA2ProcAmp dwFlags,
            [In] DXVA2ProcAmpValues pValues);
        
        void GetFilteringRange( 
            DXVA2Filters dwProperty,
            out DXVA2ValueRange pPropRange);
        
        void GetFilteringValue(
            DXVA2Filters dwProperty,
            out int pValue);
        
        void SetFilteringValue(
            DXVA2Filters dwProperty,
            [In] int pValue);
        
        void GetBackgroundColor( 
            out int lpClrBkg);
        
        void SetBackgroundColor( 
            int ClrBkg);        
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("A38D9567-5A9C-4F3C-B293-8EB415B279BA")]
    public interface IMFVideoDeviceID
    {
        void GetDeviceID(
            out Guid pDeviceID
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("A5C6C53F-C202-4AA5-9695-175BA8C508A5")]
    public interface IMFVideoMixerControl
    {
        void SetStreamZOrder(
            [In] int dwStreamID,
            [In] int dwZ
            );

        void GetStreamZOrder(
            [In] int dwStreamID,
            out int pdwZ
            );

        void SetStreamOutputRect(
            [In] int dwStreamID,
            [In] ref MFVideoNormalizedRect pnrcOutput
            );

        void GetStreamOutputRect(
            [In] int dwStreamID,
            out MFVideoNormalizedRect pnrcOutput
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("1F6A9F17-E70B-4E24-8AE4-0B2C3BA7A4AE"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFVideoPositionMapper
    {
        void MapOutputCoordinateToInputStream(
            [In] float xOut,
            [In] float yOut,
            [In] int dwOutputStreamIndex,
            [In] int dwInputStreamIndex,
            out float pxIn,
            out float pyIn
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("29AFF080-182A-4A5D-AF3B-448F3A6346CB")]
    public interface IMFVideoPresenter : IMFClockStateSink
    {
        #region IMFClockStateSink

        new void OnClockStart(
            [In] long hnsSystemTime,
            [In] long llClockStartOffset
            );

        new void OnClockStop(
            [In] long hnsSystemTime
            );

        new void OnClockPause(
            [In] long hnsSystemTime
            );

        new void OnClockRestart(
            [In] long hnsSystemTime
            );

        new void OnClockSetRate(
            [In] long hnsSystemTime,
            [In] float flRate
            );

        #endregion

        void ProcessMessage(
            MFVPMessageType eMessage,
            IntPtr ulParam
            );

        void GetCurrentMediaType(
            [MarshalAs(UnmanagedType.Interface)] out IMFVideoMediaType ppMediaType
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("DFDFD197-A9CA-43D8-B341-6AF3503792CD"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFVideoRenderer
    {
        void InitializeRenderer(
            [In, MarshalAs(UnmanagedType.Interface)] IMFTransform pVideoMixer,
            [In, MarshalAs(UnmanagedType.Interface)] IMFVideoPresenter pVideoPresenter
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("56C294D0-753E-4260-8D61-A3D8820B1D54"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFDesiredSample
    {
        void GetDesiredSampleTimeAndDuration(
            out long phnsSampleTime,
            out long phnsSampleDuration
            );

        void SetDesiredSampleTimeAndDuration(
            [In] long hnsSampleTime,
            [In] long hnsSampleDuration
            );

        void Clear();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("FA993889-4383-415A-A930-DD472A8CF6F7"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFTopologyServiceLookup
    {
        void LookupService(
            [In] MFServiceLookupType type,
            [In] int dwIndex,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidService,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Interface)] out object[] ppvObjects,
            [In, Out] ref int pnObjects
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("FA99388A-4383-415A-A930-DD472A8CF6F7"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFTopologyServiceLookupClient
    {
        void InitServicePointers(
            [In, MarshalAs(UnmanagedType.Interface)] IMFTopologyServiceLookup pLookup
            );

        void ReleaseServicePointers();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("245BF8E9-0755-40F7-88A5-AE0F18D55E17")]
    public interface IMFTrackedSample
    {
        void SetAllocator(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pSampleAllocator,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkState
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("83A4CE40-7710-494b-A893-A472049AF630")]
    public interface IEVRTrustedVideoPlugin
    {
        void IsInTrustedVideoMode(
            [MarshalAs(UnmanagedType.Bool)] out bool pYes
            );

        void CanConstrict(
            [MarshalAs(UnmanagedType.Bool)] out bool pYes
            );

        void SetConstriction(
            int dwKPix
            );

        void DisableImageExport(
            [MarshalAs(UnmanagedType.Bool)] bool bDisable
            );
    }

#endif

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("A490B1E4-AB84-4D31-A1B2-181E03B1077A"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFVideoDisplayControl
    {
        void GetNativeVideoSize(
            [Out] SIZE pszVideo,
            [Out] SIZE pszARVideo
            );

        void GetIdealVideoSize(
            [Out] SIZE pszMin,
            [Out] SIZE pszMax
            );

        void SetVideoPosition(
            [In] MFVideoNormalizedRect pnrcSource,
            [In] RECT prcDest
            );

        void GetVideoPosition(
            [Out] MFVideoNormalizedRect pnrcSource,
            [Out] RECT prcDest
            );

        void SetAspectRatioMode(
            [In] MFVideoAspectRatioMode dwAspectRatioMode
            );

        void GetAspectRatioMode(
            out MFVideoAspectRatioMode pdwAspectRatioMode
            );

        void SetVideoWindow(
            [In] IntPtr hwndVideo
            );

        void GetVideoWindow(
            out IntPtr phwndVideo
            );

        void RepaintVideo();

        void GetCurrentImage(
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(BMMarshaler))] BitmapInfoHeader pBih,
            out IntPtr pDib,
            out int pcbDib,
            out long pTimeStamp
            );

        void SetBorderColor(
            [In] int Clr
            );

        void GetBorderColor(
            out int pClr
            );

        void SetRenderingPrefs(
            [In] MFVideoRenderPrefs dwRenderFlags
            );

        void GetRenderingPrefs(
            out MFVideoRenderPrefs pdwRenderFlags
            );

        void SetFullscreen(
            [In, MarshalAs(UnmanagedType.Bool)] bool fFullscreen
            );

        void GetFullscreen(
            [MarshalAs(UnmanagedType.Bool)] out bool pfFullscreen
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("83E91E85-82C1-4ea7-801D-85DC50B75086")]
    public interface IEVRFilterConfig
    {
        void SetNumberOfStreams(
            int dwMaxStreams
            );

        void GetNumberOfStreams(
            out int pdwMaxStreams
            );
    }

    #endregion

}
