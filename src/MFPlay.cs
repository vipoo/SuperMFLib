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
using System.Runtime.InteropServices.ComTypes;
using System.Security;

using MediaFoundation.Misc;
using System.Drawing;

using MediaFoundation.EVR;

namespace MediaFoundation.MFPlayer
{
    #region Declarations

#if ALLOW_UNTESTED_INTERFACES

    [Flags, UnmanagedName("MFP_CREATION_OPTIONS")]
    public enum MFP_CREATION_OPTIONS
    {
        None = 0x00000000,
        FreeThreadedCallback = 0x00000001,
        NoMMCSS = 0x00000002,
        NoRemoteDesktopOptimization = 0x00000004
    }

    [UnmanagedName("MFP_MEDIAPLAYER_STATE")]
    public enum MFP_MEDIAPLAYER_STATE
    {
        Empty = 0x00000000,
        Stopped = 0x00000001,
        Playing = 0x00000002,
        Paused = 0x00000003,
        Shutdown = 0x00000004
    }

    [Flags, UnmanagedName("MFP_MEDIAITEM_CHARACTERISTICS")]
    public enum MFP_MEDIAITEM_CHARACTERISTICS
    {
        None = 0x00000000,
        IsLive = 0x00000001,
        CanSeek = 0x00000002,
        CanPause = 0x00000004,
        HasSlowSeek = 0x00000008
    }

    [Flags, UnmanagedName("MFP_CREDENTIAL_FLAGS")]
    public enum MFP_CREDENTIAL_FLAGS
    {
        None = 0x00000000,
        Prompt = 0x00000001,
        Save = 0x00000002,
        DoNotCache = 0x00000004,
        ClearText = 0x00000008,
        Proxy = 0x00000010,
        LoggedOnUser = 0x00000020
    }

    [UnmanagedName("MFP_EVENT_TYPE")]
    public enum MFP_EVENT_TYPE
    {
        Play = 0,
        Pause = 1,
        Stop = 2,
        PositionSet = 3,
        RateSet = 4,
        MediaItemCreated = 5,
        MediaItemSet = 6,
        FrameStep = 7,
        MediaItemCleared = 8,
        MF = 9,
        Error = 10,
        PlaybackEnded = 11,
        AcquireUserCredential = 12
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_EVENT_HEADER")]
    public class MFP_EVENT_HEADER
    {
        public MFP_EVENT_TYPE eEventType;
        public int hrEvent;
        public IMFPMediaPlayer pMediaPlayer;
        public MFP_MEDIAPLAYER_STATE eState;
        public IPropertyStore pPropertyStore;

        public IntPtr GetPtr()
        {
            IntPtr ip;

            int iSize = Marshal.SizeOf(this);

            ip = Marshal.AllocCoTaskMem(iSize);
            Marshal.StructureToPtr(this, ip, false);

            return ip;
        }

        public static MFP_EVENT_HEADER PtrToEH(IntPtr pNativeData)
        {
            MFP_EVENT_TYPE met = (MFP_EVENT_TYPE)Marshal.ReadInt32(pNativeData);
            object mce;

            switch (met)
            {
                case MFP_EVENT_TYPE.Play:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_PLAY_EVENT));
                    break;
                case MFP_EVENT_TYPE.Pause:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_PAUSE_EVENT));
                    break;
                case MFP_EVENT_TYPE.Stop:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_STOP_EVENT));
                    break;
                case MFP_EVENT_TYPE.PositionSet:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_POSITION_SET_EVENT));
                    break;
                case MFP_EVENT_TYPE.RateSet:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_RATE_SET_EVENT));
                    break;
                case MFP_EVENT_TYPE.MediaItemCreated:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_MEDIAITEM_CREATED_EVENT));
                    break;
                case MFP_EVENT_TYPE.MediaItemSet:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_RATE_SET_EVENT));
                    break;
                case MFP_EVENT_TYPE.FrameStep:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_FRAME_STEP_EVENT));
                    break;
                case MFP_EVENT_TYPE.MediaItemCleared:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_MEDIAITEM_CLEARED_EVENT));
                    break;
                case MFP_EVENT_TYPE.MF:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_MF_EVENT));
                    break;
                case MFP_EVENT_TYPE.Error:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_ERROR_EVENT));
                    break;
                case MFP_EVENT_TYPE.PlaybackEnded:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_PLAYBACK_ENDED_EVENT));
                    break;
                case MFP_EVENT_TYPE.AcquireUserCredential:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_ACQUIRE_USER_CREDENTIAL_EVENT));
                    break;
                default:
                    // Don't know what it is.  Send back the header.
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_EVENT_HEADER));
                    break;
            }

            return mce as MFP_EVENT_HEADER;
        }
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_PLAY_EVENT")]
    public class MFP_PLAY_EVENT : MFP_EVENT_HEADER
    {
        public IMFPMediaItem pMediaItem;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_PAUSE_EVENT")]
    public class MFP_PAUSE_EVENT : MFP_EVENT_HEADER
    {
        public IMFPMediaItem pMediaItem;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_STOP_EVENT")]
    public class MFP_STOP_EVENT : MFP_EVENT_HEADER
    {
        public IMFPMediaItem pMediaItem;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_POSITION_SET_EVENT")]
    public class MFP_POSITION_SET_EVENT : MFP_EVENT_HEADER
    {
        public IMFPMediaItem pMediaItem;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_RATE_SET_EVENT")]
    public class MFP_RATE_SET_EVENT : MFP_EVENT_HEADER
    {
        public IMFPMediaItem pMediaItem;
        public float flRate;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_MEDIAITEM_CREATED_EVENT")]
    public class MFP_MEDIAITEM_CREATED_EVENT : MFP_EVENT_HEADER
    {
        public IMFPMediaItem pMediaItem;
        public IntPtr dwUserData;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_MEDIAITEM_SET_EVENT")]
    public class MFP_MEDIAITEM_SET_EVENT : MFP_EVENT_HEADER
    {
        public IMFPMediaItem pMediaItem;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_FRAME_STEP_EVENT")]
    public class MFP_FRAME_STEP_EVENT : MFP_EVENT_HEADER
    {
        public IMFPMediaItem pMediaItem;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_MEDIAITEM_CLEARED_EVENT")]
    public class MFP_MEDIAITEM_CLEARED_EVENT : MFP_EVENT_HEADER
    {
        public IMFPMediaItem pMediaItem;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_MF_EVENT")]
    public class MFP_MF_EVENT : MFP_EVENT_HEADER
    {
        public MediaEventType MFEventType;
        public IMFMediaEvent pMFMediaEvent;
        public IMFPMediaItem pMediaItem;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_ERROR_EVENT")]
    public class MFP_ERROR_EVENT : MFP_EVENT_HEADER
    {
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_PLAYBACK_ENDED_EVENT")]
    public class MFP_PLAYBACK_ENDED_EVENT : MFP_EVENT_HEADER
    {
        public IMFPMediaItem pMediaItem;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_ACQUIRE_USER_CREDENTIAL_EVENT")]
    public class MFP_ACQUIRE_USER_CREDENTIAL_EVENT : MFP_EVENT_HEADER
    {
        public IntPtr dwUserData;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fProceedWithAuthentication;
        public int hrAuthenticationStatus;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pwszURL;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pwszSite;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pwszRealm;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pwszPackage;
        public int nRetries;
        public MFP_CREDENTIAL_FLAGS flags;
        public IMFNetCredential pCredential;
    }

#endif

    #endregion

    #region Interfaces

#if ALLOW_UNTESTED_INTERFACES

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("A714590A-58AF-430a-85BF-44F5EC838D85")]
    public interface IMFPMediaPlayer
    {
        void Play();

        void Pause();

        void Stop();

        void FrameStep();

        void SetPosition(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidPositionType,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvPositionValue
        );

        void GetPosition(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidPositionType,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvPositionValue
        );

        void GetDuration(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidPositionType,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvPositionValue
        );

        void SetRate(
            float flRate
        );

        void GetRate(
            out float pflRate
        );

        void GetSupportedRates(
            [MarshalAs(UnmanagedType.Bool)] bool fForwardDirection,
            out float pflSlowestRate,
            out float pflFastestRate
        );

        void GetState(
            out MFP_MEDIAPLAYER_STATE peState
        );

        void CreateMediaItemFromURL(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszURL,
            [MarshalAs(UnmanagedType.Bool)] bool fSync,
            IntPtr dwUserData,
            out IMFPMediaItem ppMediaItem
        );

        void CreateMediaItemFromObject(
            [MarshalAs(UnmanagedType.IUnknown)] object pIUnknownObj,
            [MarshalAs(UnmanagedType.Bool)] bool fSync,
            IntPtr dwUserData,
            out IMFPMediaItem ppMediaItem
        );

        void SetMediaItem(
            IMFPMediaItem pIMFPMediaItem
        );

        void ClearMediaItem();

        void GetMediaItem(
            out IMFPMediaItem ppIMFPMediaItem
        );

        void GetVolume(
            out float pflVolume
        );

        void SetVolume(
            float flVolume
        );

        void GetBalance(
            out float pflBalance
        );

        void SetBalance(
            float flBalance
        );

        void GetMute(
            [MarshalAs(UnmanagedType.Bool)] out bool pfMute
        );

        void SetMute(
            [MarshalAs(UnmanagedType.Bool)] bool fMute
        );

        void GetNativeVideoSize(
            out Size pszVideo,
            out Size pszARVideo
        );

        void GetIdealVideoSize(
            out Size pszMin,
            out Size pszMax
        );

        void SetVideoSourceRect(
            [In] MFVideoNormalizedRect pnrcSource
        );

        void GetVideoSourceRect(
            out MFVideoNormalizedRect pnrcSource
        );

        void SetAspectRatioMode(
            MFVideoAspectRatioMode dwAspectRatioMode
        );

        void GetAspectRatioMode(
            out MFVideoAspectRatioMode pdwAspectRatioMode
        );

        void GetVideoWindow(
            out IntPtr phwndVideo
        );

        void UpdateVideo();

        void SetBorderColor(
            Color Clr
        );

        void GetBorderColor(
            out Color pClr
        );

        void InsertEffect(
            [MarshalAs(UnmanagedType.IUnknown)] object pEffect,
            [MarshalAs(UnmanagedType.Bool)] bool fOptional
        );

        void RemoveEffect(
            [MarshalAs(UnmanagedType.IUnknown)] object pEffect
        );

        void RemoveAllEffects();

        void Shutdown();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("90EB3E6B-ECBF-45cc-B1DA-C6FE3EA70D57")]
    public interface IMFPMediaItem
    {
        void GetMediaPlayer(
            out IMFPMediaPlayer ppMediaPlayer
        );

        void GetURL(
            [MarshalAs(UnmanagedType.LPWStr)] out string ppwszURL
        );

        void GetObject(
            [MarshalAs(UnmanagedType.IUnknown)] out object ppIUnknown
        );

        void GetUserData(
            out IntPtr pdwUserData
        );

        void SetUserData(
            IntPtr dwUserData
        );

        void GetStartStopPosition(
            out Guid pguidStartPositionType,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvStartValue,
            out Guid pguidStopPositionType,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvStopValue
        );

        void SetStartStopPosition(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pguidStartPositionType,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvStartValue,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pguidStopPositionType,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvStopValue
        );

        void HasVideo(
            [MarshalAs(UnmanagedType.Bool)] out bool pfHasVideo,
            [MarshalAs(UnmanagedType.Bool)] out bool pfSelected
        );

        void HasAudio(
            [MarshalAs(UnmanagedType.Bool)] out bool pfHasAudio,
            [MarshalAs(UnmanagedType.Bool)] out bool pfSelected
        );

        void IsProtected(
            [MarshalAs(UnmanagedType.Bool)] out bool pfProtected
        );

        void GetDuration(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidPositionType,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvDurationValue
        );

        void GetNumberOfStreams(
            out int pdwStreamCount
        );

        void GetStreamSelection(
            int dwStreamIndex,
            [MarshalAs(UnmanagedType.Bool)] out bool pfEnabled
        );

        void SetStreamSelection(
            int dwStreamIndex,
            [MarshalAs(UnmanagedType.Bool)] bool fEnabled
        );

        void GetStreamAttribute(
            int dwStreamIndex,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidMFAttribute,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvValue
        );

        void GetPresentationAttribute(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidMFAttribute,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvValue
        );

        void GetCharacteristics(
            out MFP_MEDIAITEM_CHARACTERISTICS pCharacteristics
        );

        void SetStreamSink(
            int dwStreamIndex,
            [MarshalAs(UnmanagedType.IUnknown)] object pMediaSink
        );

        void GetMetadata(
            out IPropertyStore ppMetadataStore
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("766C8FFB-5FDB-4fea-A28D-B912996F51BD")]
    public interface IMFPMediaPlayerCallback
    {
        void OnMediaPlayerEvent(
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(EHMarshaler))] MFP_EVENT_HEADER pEventHeader
            );
    }

    internal class EHMarshaler : ICustomMarshaler
    {
        public IntPtr MarshalManagedToNative(object managedObj)
        {
            MFP_EVENT_HEADER eh = managedObj as MFP_EVENT_HEADER;

            IntPtr ip = eh.GetPtr();

            return ip;
        }

        // Called just after invoking the COM method.  The IntPtr is the same one that just got returned
        // from MarshalManagedToNative.  The return value is unused.
        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            MFP_EVENT_HEADER eh = MFP_EVENT_HEADER.PtrToEH(pNativeData);

            return eh;
        }

        // It appears this routine is never called
        public void CleanUpManagedData(object ManagedObj)
        {
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            Marshal.FreeCoTaskMem(pNativeData);
        }

        // The number of bytes to marshal out - never called
        public int GetNativeDataSize()
        {
            return -1;
        }

        // This method is called by interop to create the custom marshaler.  The (optional)
        // cookie is the value specified in MarshalCookie="asdf", or "" is none is specified.
        public static ICustomMarshaler GetInstance(string cookie)
        {
            return new EHMarshaler();
        }
    }

#endif

    #endregion
}
