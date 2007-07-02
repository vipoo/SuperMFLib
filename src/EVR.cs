#region license

/*
MediaFoundationLib - Provide access to MediaFoundation interfaces via .NET
Copyright (C) 2007
http://sourceforge.net/projects/directshownet/

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

    /// <summary>
    /// From MFVideoRenderPrefs
    /// </summary>
    [Flags]
    public enum MFVideoRenderPrefs
    {
        None = 0,
        DoNotRenderBorder = 0x00000001,
        DoNotClipToDevice = 0x00000002,
        Mask = 0x00000003
    }

    /// <summary>
    /// From MFVideoAspectRatioMode
    /// </summary>
    [Flags]
    public enum MFVideoAspectRatioMode
    {
        None = 0x00000000,
        PreservePicture = 0x00000001,
        PreservePixel = 0x00000002,
        NonLinearStretch = 0x00000004,
        Mask = 0x00000007
    }

    /// <summary>
    /// From MFVideoNormalizedRect
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct MFVideoNormalizedRect
    {
        public float left;
        public float top;
        public float right;
        public float bottom;
    }

    /// <summary>
    /// From RemotableHandle
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct RemotableHandle
    {
        public int fContext;
        public Unnamed2 u;
    }

    /// <summary>
    /// From unnamed union
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct Unnamed2
    {
        // Fields
        [FieldOffset(0)]
        public int hInproc;
        [FieldOffset(0)]
        public int hRemote;
    }

    /// <summary>
    /// From MFVP_MESSAGE_TYPE
    /// </summary>
    public enum MFVP_MessageType
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

    /// <summary>
    /// From MF_SERVICE_LOOKUP_TYPE
    /// </summary>
    public enum MF_ServiceLookupType
    {
        Upstream,
        UpstreamDirect,
        Downstream,
        DownstreamDirect,
        All,
        Global
    }

    #endregion

    #region Interfaces

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("A38D9567-5A9C-4F3C-B293-8EB415B279BA")]
    public interface IMFVideoDeviceID
    {
        [PreserveSig]
        int GetDeviceID(
            out Guid pDeviceID
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("A490B1E4-AB84-4D31-A1B2-181E03B1077A"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFVideoDisplayControl
    {
        [PreserveSig]
        int GetNativeVideoSize(
            [In, Out] ref SIZE pszVideo,
            [In, Out] ref SIZE pszARVideo
            );

        [PreserveSig]
        int GetIdealVideoSize(
            [In, Out] ref SIZE pszMin,
            [In, Out] ref SIZE pszMax
            );

        [PreserveSig]
        int SetVideoPosition(
            [In] MFVideoNormalizedRect pnrcSource,
            [In] RECT prcDest
            );

        [PreserveSig]
        int GetVideoPosition(
            out MFVideoNormalizedRect pnrcSource,
            out RECT prcDest
            );

        [PreserveSig]
        int SetAspectRatioMode(
            [In] MFVideoAspectRatioMode dwAspectRatioMode
            );

        [PreserveSig]
        int GetAspectRatioMode(
            out MFVideoAspectRatioMode pdwAspectRatioMode
            );

        [PreserveSig]
        int SetVideoWindow(
            [In] ref RemotableHandle hwndVideo
            );

        [PreserveSig]
        int GetVideoWindow(
            [Out] IntPtr phwndVideo
            );

        [PreserveSig]
        int RepaintVideo();

        [PreserveSig]
        int GetCurrentImage(
            [In, Out] ref BitmapInfoHeader pBih,
            [Out] IntPtr pDib,
            out int pcbDib,
            [In, Out] ref long pTimeStamp
            );

        [PreserveSig]
        int SetBorderColor(
            [In] int Clr
            );

        [PreserveSig]
        int GetBorderColor(
            out int pClr
            );

        [PreserveSig]
        int SetRenderingPrefs(
            [In] MFVideoRenderPrefs dwRenderFlags
            );

        [PreserveSig]
        int GetRenderingPrefs(
            out MFVideoRenderPrefs pdwRenderFlags
            );

        [PreserveSig]
        int SetFullscreen(
            [In] int fFullscreen
            );

        [PreserveSig]
        int GetFullscreen(
            out int pfFullscreen
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("A5C6C53F-C202-4AA5-9695-175BA8C508A5")]
    public interface IMFVideoMixerControl
    {
        [PreserveSig]
        int SetStreamZOrder(
            [In] int dwStreamID,
            [In] int dwZ
            );

        [PreserveSig]
        int GetStreamZOrder(
            [In] int dwStreamID,
            out int pdwZ
            );

        [PreserveSig]
        int SetStreamOutputRect(
            [In] int dwStreamID,
            [In] ref MFVideoNormalizedRect pnrcOutput
            );

        [PreserveSig]
        int GetStreamOutputRect(
            [In] int dwStreamID,
            out MFVideoNormalizedRect pnrcOutput
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("1F6A9F17-E70B-4E24-8AE4-0B2C3BA7A4AE"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFVideoPositionMapper
    {
        [PreserveSig]
        int MapOutputCoordinateToInputStream(
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

        [PreserveSig]
        new int OnClockStart(
            [In] long hnsSystemTime,
            [In] long llClockStartOffset
            );

        [PreserveSig]
        new int OnClockStop(
            [In] long hnsSystemTime
            );

        [PreserveSig]
        new int OnClockPause(
            [In] long hnsSystemTime
            );

        [PreserveSig]
        new int OnClockRestart(
            [In] long hnsSystemTime
            );

        [PreserveSig]
        new int OnClockSetRate(
            [In] long hnsSystemTime,
            [In] float flRate
            );

        #endregion

        [PreserveSig]
        int ProcessMessage(
            MFVP_MessageType eMessage,
            int ulParam
            );

        [PreserveSig]
        int GetCurrentMediaType(
            [MarshalAs(UnmanagedType.Interface)] out IMFVideoMediaType ppMediaType
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("DFDFD197-A9CA-43D8-B341-6AF3503792CD"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFVideoRenderer
    {
        [PreserveSig]
        int InitializeRenderer(
            [In, MarshalAs(UnmanagedType.Interface)] IMFTransform pVideoMixer,
            [In, MarshalAs(UnmanagedType.Interface)] IMFVideoPresenter pVideoPresenter
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("56C294D0-753E-4260-8D61-A3D8820B1D54"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFDesiredSample
    {
        [PreserveSig]
        int GetDesiredSampleTimeAndDuration(
            out long phnsSampleTime,
            out long phnsSampleDuration
            );

        [PreserveSig]
        int SetDesiredSampleTimeAndDuration(
            [In] long hnsSampleTime,
            [In] long hnsSampleDuration
            );

        [PreserveSig]
        int Clear();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("FA993889-4383-415A-A930-DD472A8CF6F7"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFTopologyServiceLookup
    {
        [PreserveSig]
        int LookupService(
            [In] MF_ServiceLookupType type,
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
        [PreserveSig]
        int InitServicePointers(
            [In, MarshalAs(UnmanagedType.Interface)] IMFTopologyServiceLookup pLookup
            );

        [PreserveSig]
        int ReleaseServicePointers();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("245BF8E9-0755-40F7-88A5-AE0F18D55E17")]
    public interface IMFTrackedSample
    {
        [PreserveSig]
        int SetAllocator(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pSampleAllocator,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkState
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("83A4CE40-7710-494b-A893-A472049AF630")]
    public interface IEVRTrustedVideoPlugin
    {
        [PreserveSig]
        int IsInTrustedVideoMode(
            [MarshalAs(UnmanagedType.Bool)] out bool pYes
            );

        [PreserveSig]
        int CanConstrict(
            [MarshalAs(UnmanagedType.Bool)] out bool pYes
            );

        [PreserveSig]
        int SetConstriction(
            int dwKPix
            );

        [PreserveSig]
        int DisableImageExport(
            [MarshalAs(UnmanagedType.Bool)] bool bDisable
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("83E91E85-82C1-4ea7-801D-85DC50B75086")]
    public interface IEVRFilterConfig
    {
        [PreserveSig]
        int SetNumberOfStreams(
            int dwMaxStreams
            );

        [PreserveSig]
        int GetNumberOfStreams(
            out int pdwMaxStreams
            );
    }

    #endregion

}
