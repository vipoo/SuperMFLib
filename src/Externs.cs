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
Foundation = new Guid(Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

#endregion

using System;
using System.Text;
using System.Runtime.InteropServices;

using MediaFoundation.Misc;

namespace MediaFoundation
{
    public class MFPlatDll
    {
        [DllImport("MfPlat.dll", PreserveSig = false)]
        public static extern void MFShutdown();

        [DllImport("MfPlat.dll", PreserveSig = false)]
        public static extern void MFStartup(
            int Version, MFStartup dwFlags
        );

        [DllImport("MfPlat.dll", PreserveSig = false)]
        public static extern void MFCreateSystemTimeSource(
            out IMFPresentationTimeSource ppSystemTimeSource
        );

        [DllImport("MfPlat.dll", PreserveSig = false)]
        public static extern void MFCreateCollection(
            out IMFCollection ppIMFCollection
        );

        [DllImport("MfPlat.dll", PreserveSig = false)]
        public static extern void MFCreateStreamDescriptor(
            int dwStreamIdentifier,
            int cMediaTypes,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] IMFMediaType[] apMediaTypes,
            out IMFStreamDescriptor ppDescriptor
        );

        [DllImport("MfPlat.dll", PreserveSig = false)]
        public static extern void CreatePropertyStore(
            out IPropertyStore ppStore
        );

        [DllImport("MfPlat.dll", PreserveSig = false)]
        public static extern void MFCreateAttributes(
            out IMFAttributes ppMFAttributes,
            int cInitialSize
        );

        [DllImport("MfPlat.dll", PreserveSig = false)]
        public static extern void MFCreateWaveFormatExFromMFMediaType(
            IMFMediaType pMFType,
            [Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(WEMarshaler))] out WaveFormatEx ppWF,
            out int pcbSize,
            MFWaveFormatExConvertFlags Flags
        );

        [DllImport("MfPlat.dll", PreserveSig = false)]
        public static extern void MFCreateAsyncResult(
            [MarshalAs(UnmanagedType.IUnknown)] object punkObject,
            IMFAsyncCallback pCallback,
            [MarshalAs(UnmanagedType.IUnknown)] object punkState,
            out IMFAsyncResult ppAsyncResult
        );

        [DllImport("MfPlat.dll", PreserveSig = false)]
        public static extern void MFInvokeCallback(
            IMFAsyncResult pAsyncResult
        );

        [DllImport("MfPlat.dll", PreserveSig = false)]
        public static extern void MFCreatePresentationDescriptor(
            int cStreamDescriptors,
            [MarshalAs(UnmanagedType.LPArray)] IMFStreamDescriptor[] apStreamDescriptors,
            out IMFPresentationDescriptor ppPresentationDescriptor
        );

        [DllImport("MfPlat.dll", PreserveSig = false)]
        public static extern void MFInitMediaTypeFromWaveFormatEx(
            IMFMediaType pMFType,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(WEMarshaler))] WaveFormatEx ppWF,
            int cbBufSize
        );

        [DllImport("MfPlat.dll", PreserveSig = false)]
        public static extern void MFCreateEventQueue(
            out IMFMediaEventQueue ppMediaEventQueue
        );

        [DllImport("MfPlat.dll", PreserveSig = false)]
        public static extern void MFCreateMediaType(
            out IMFMediaType ppMFType
        );

        [DllImport("MfPlat.dll", PreserveSig = false)]
        public static extern void MFCreateMediaEvent(
            MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            int hrStatus,
            [In, MarshalAs(UnmanagedType.LPStruct)] PropVariant pvValue,
            out IMFMediaEvent ppEvent
        );

        [DllImport("MfPlat.dll", PreserveSig = false)]
        public static extern void MFCreateSample(
            out IMFSample ppIMFSample
        );

        [DllImport("MfPlat.dll", PreserveSig = false)]
        public static extern void MFCreateMemoryBuffer(
            int cbMaxLength,
            out IMFMediaBuffer ppBuffer
        );

    }

    public class MFDll
    {
        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFGetService(
            [In, MarshalAs(UnmanagedType.Interface)] object punkObject,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidService,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [Out, MarshalAs(UnmanagedType.Interface)] out object ppvObject
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateVideoRendererActivate(
            IntPtr hwndVideo,
            out IMFActivate ppActivate
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateTopologyNode(
            MFTopologyType NodeType,
            out IMFTopologyNode ppNode
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateSourceResolver(
            out IMFSourceResolver ppISourceResolver
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateMediaSession(
            IMFAttributes pConfiguration,
            out IMFMediaSession ppMediaSession
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateTopology(
            out IMFTopology ppTopo
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateAudioRendererActivate(
            out IMFActivate ppActivate
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreatePresentationClock(
            out IMFPresentationClock ppPresentationClock
        );

#if ALLOW_UNTESTED_INTERFACES

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFGetUncompressedVideoFormat(
            [In]    MFVideoFormat pVideoFormat
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void CreateNamedPropertyStore(
        out INamedPropertyStore ppStore
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFLockPlatform();

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFUnlockPlatform();

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFPutWorkItem(
            int dwQueue,
            IMFAsyncCallback pCallback,
            [MarshalAs(UnmanagedType.IUnknown)] object pState);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFPutWorkItemEx(
            int dwQueue,
            IMFAsyncResult pResult);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFScheduleWorkItem(
            IMFAsyncCallback pCallback,
            [MarshalAs(UnmanagedType.IUnknown)] object pState,
            long Timeout,
            long pKey);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFScheduleWorkItemEx(
            IMFAsyncResult pResult,
            long Timeout,
            long pKey);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCancelWorkItem(
            long Key);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFGetTimerPeriodicity(
            out int Periodicity);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFAddPeriodicCallback(
            IntPtr Callback,
            [MarshalAs(UnmanagedType.IUnknown)] object pContext,
            out int pdwKey);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFRemovePeriodicCallback(
            int dwKey);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFAllocateWorkQueue(
            out int pdwWorkQueue);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFLockWorkQueue(
            [In] int dwWorkQueue);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFUnlockWorkQueue(
            [In] int dwWorkQueue);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFBeginRegisterWorkQueueWithMMCSS(
            int dwWorkQueueId,
            [In] string wszClass,
            int dwTaskId,
            [In] IMFAsyncCallback pDoneCallback,
            [In] [MarshalAs(UnmanagedType.IUnknown)] object pDoneState);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFEndRegisterWorkQueueWithMMCSS(
            [In] IMFAsyncResult pResult,
            out int pdwTaskId);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFBeginUnregisterWorkQueueWithMMCSS(
            int dwWorkQueueId,
            [In] IMFAsyncCallback pDoneCallback,
            [In] [MarshalAs(UnmanagedType.IUnknown)] object pDoneState);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFEndUnregisterWorkQueueWithMMCSS(
            [In] IMFAsyncResult pResult);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFSetWorkQueueClass(
            int dwWorkQueueId,
            string szClass);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFGetWorkQueueMMCSSClass(
            int dwWorkQueueId,
            [MarshalAs(UnmanagedType.LPWStr)] out string pwszClass,
            out  int pcchClass);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFGetWorkQueueMMCSSTaskId(
            int dwWorkQueueId,
            out int pdwTaskId);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateFile(
            MFFileAccessMode AccessMode,
            MFFileOpenMode OpenMode,
            MFFileFlags fFlags,
            string pwszFileURL,
            out IMFByteStream ppIByteStream);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateTempFile(
            MFFileAccessMode AccessMode,
            MFFileOpenMode OpenMode,
            MFFileFlags fFlags,
            out IMFByteStream ppIByteStream);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFBeginCreateFile(
            [In]  MFFileAccessMode AccessMode,
            [In]  MFFileOpenMode OpenMode,
            [In]  MFFileFlags fFlags,
            [In]  string pwszFilePath,
            [In]  IMFAsyncCallback pCallback,
            [In]  [MarshalAs(UnmanagedType.IUnknown)] object pState,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppCancelCookie);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFEndCreateFile(
            [In] IMFAsyncResult pResult,
            out IMFByteStream ppFile);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCancelCreateFile(
            [In] [MarshalAs(UnmanagedType.IUnknown)] object pCancelCookie);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateMediaBufferWrapper(
            [In] IMFMediaBuffer pBuffer,
            [In] int cbOffset,
            [In] int dwLength,
            out  IMFMediaBuffer ppBuffer);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateLegacyMediaBufferOnMFMediaBuffer(
            [In] IMFSample pSample,
            [In] IMFMediaBuffer pMFMediaBuffer,
            [In] int cbOffset,
            out IntPtr ppMediaBuffer);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateDXSurfaceBuffer(
            [In] Guid riid,
            [In] [MarshalAs(UnmanagedType.IUnknown)] object punkSurface,
            [In] bool fBottomUpWhenLinear,
            out IMFMediaBuffer ppBuffer);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateAlignedMemoryBuffer(
            [In] int cbMaxLength,
            [In] int cbAligment,
            out IMFMediaBuffer ppBuffer);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFInitAttributesFromBlob(
            [In]                    IMFAttributes pAttributes,
            IntPtr pBuf,
            [In]                    int cbBufSize
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFGetAttributesAsBlobSize(
            [In]    IMFAttributes pAttributes,
            out   int pcbBufSize
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFGetAttributesAsBlob(
            [In]                    IMFAttributes pAttributes,
            IntPtr pBuf,
            [In]                    int cbBufSize
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFTRegister(
            [In]                            CLSID clsidMFT,
            [In]                            Guid guidCategory,
            [In]                            string pszName,
            [In]                            int Flags,
            [In]                            int cInputTypes,
            MFTRegisterTypeInfo pInputTypes,
            [In]                            int cOutputTypes,
            MFTRegisterTypeInfo pOutputTypes,
            [In]                        IMFAttributes pAttributes
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFTUnregister(
            [In]    CLSID clsidMFT
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFTEnum(
            [In]                    Guid guidCategory,
            [In]                    int Flags,
            [In]                MFTRegisterTypeInfo pInputType,
            [In]                MFTRegisterTypeInfo pOutputType,
            [In]                IMFAttributes pAttributes,
            out   Guid ppclsidMFT, // must be freed with CoTaskMemFree
            out                   int pcMFTs
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFTGetInfo(
            [In]                                CLSID clsidMFT,
            [MarshalAs(UnmanagedType.LPWStr)] out string pszName,
            out MFTRegisterTypeInfo ppInputTypes,
            out                           int pcInputTypes,
            out MFTRegisterTypeInfo ppOutputTypes,
            out                           int pcOutputTypes,
            out IMFAttributes ppAttributes
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFValidateMediaTypeSize(
            [In]                    Guid FormatType,
            IntPtr pBlock,
            [In]                    int cbSize
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateMFVideoFormatFromMFMediaType(
            [In]        IMFMediaType pMFType,
            out       MFVideoFormat ppMFVF, // must be deleted with CoTaskMemFree
            out   int pcbSize
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFInitMediaTypeFromVideoInfoHeader(
            [In]                    IMFMediaType pMFType,
            VideoInfoHeader pVIH,
            [In]                    int cbBufSize,
            [In]                Guid pSubtype
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFInitMediaTypeFromVideoInfoHeader2(
            [In]                    IMFMediaType pMFType,
            VideoInfoHeader2 pVIH2,
            [In]                    int cbBufSize,
            [In]                Guid pSubtype
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFInitMediaTypeFromMPEG1VideoInfo(
            [In]                    IMFMediaType pMFType,
            MPEG1VideoInfo pMP1VI,
            [In]                    int cbBufSize,
            [In]                Guid pSubtype
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFInitMediaTypeFromMPEG2VideoInfo(
            [In]                    IMFMediaType pMFType,
            Mpeg2VideoInfo pMP2VI,
            [In]                    int cbBufSize,
            [In]                Guid pSubtype
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCalculateBitmapImageSize(
            BitmapInfoHeader pBMIH,
            [In]                    int cbBufSize,
            out                   int pcbImageSize,
            out               bool pbKnown
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCalculateImageSize(
            [In]                    Guid guidSubtype,
            [In]                    int unWidth,
            [In]                    int unHeight,
            out                   int pcbImageSize
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFFrameRateToAverageTimePerFrame(
            [In]                    int unNumerator,
            [In]                    int unDenominator,
            out                   long punAverageTimePerFrame
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFAverageTimePerFrameToFrameRate(
            [In]                    long unAverageTimePerFrame,
            out                   int punNumerator,
            out                   int punDenominator
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFInitMediaTypeFromMFVideoFormat(
            [In]                    IMFMediaType pMFType,
            MFVideoFormat pMFVF,
            [In]                    int cbBufSize
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFInitMediaTypeFromAMMediaType(
            [In]    IMFMediaType pMFType,
            [In]    AMMediaType pAMType
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFInitAMMediaTypeFromMFMediaType(
            [In]    IMFMediaType pMFType,
            [In]    Guid guidFormatBlockType,
            AMMediaType pAMType
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateAMMediaTypeFromMFMediaType(
            [In]    IMFMediaType pMFType,
            [In]    Guid guidFormatBlockType,
            out AMMediaType ppAMType // delete with DeleteMediaType
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCompareFullToPartialMediaType(
            [In]    IMFMediaType pMFTypeFull,
            [In]    IMFMediaType pMFTypePartial
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFWrapMediaType(
            [In]    IMFMediaType pOrig,
            [In]    Guid MajorType,
            [In]    Guid SubType,
            out   IMFMediaType ppWrap
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFUnwrapMediaType(
            [In]    IMFMediaType pWrap,
            out   IMFMediaType ppOrig
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateVideoMediaTypeFromVideoInfoHeader(
            VideoInfoHeader pVideoInfoHeader,
            int cbVideoInfoHeader,
            int dwPixelAspectRatioX,
            int dwPixelAspectRatioY,
            MFVideoInterlaceMode InterlaceMode,
            long VideoFlags,
            Guid pSubtype,
            out IMFVideoMediaType ppIVideoMediaType
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateVideoMediaTypeFromVideoInfoHeader2(
            VideoInfoHeader2 pVideoInfoHeader,
            int cbVideoInfoHeader,
            long AdditionalVideoFlags,
            Guid pSubtype,
            out IMFVideoMediaType ppIVideoMediaType
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateVideoMediaType(
            MFVideoFormat pVideoFormat,
            out IMFVideoMediaType ppIVideoMediaType
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateVideoMediaTypeFromSubtype(
            [In] Guid pAMSubtype,
            out IMFVideoMediaType ppIVideoMediaType
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFIsFormatYUV(
            int Format
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateVideoMediaTypeFromBitMapInfoHeader(
            BitmapInfoHeader pbmihBitMapInfoHeader,
            int dwPixelAspectRatioX,
            int dwPixelAspectRatioY,
            MFVideoInterlaceMode InterlaceMode,
            long VideoFlags,
            long qwFramesPerSecondNumerator,
            long qwFramesPerSecondDenominator,
            int dwMaxBitRate,
            out IMFVideoMediaType ppIVideoMediaType
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFGetStrideForBitmapInfoHeader(
            int format,
            int dwWidth,
            out int pStride
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFGetPlaneSize(
            int format,
            int dwWidth,
            int dwHeight,
            out int pdwPlaneSize
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateAudioMediaType(
            [In]    WaveFormatEx pAudioFormat,
            out   IMFAudioMediaType ppIAudioMediaType
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFInitVideoFormat(
            [In]    MFVideoFormat pVideoFormat,
            [In]    MFStandardVideoFormat type
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFInitVideoFormat_RGB(
            [In]    MFVideoFormat pVideoFormat,
            [In]    int dwWidth,
            [In]    int dwHeight,
            [In]    int D3Dfmt /* 0 indicates sRGB */
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFConvertColorInfoToDXVA(
            out int pdwToDXVA,
            MFVideoFormat pFromFormat
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFConvertColorInfoFromDXVA(
            MFVideoFormat pToFormat,
            int dwFromDXVA
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCopyImage(
            IntPtr pDest,
            int lDestStride,
            IntPtr pSrc,
            int lSrcStride,
            int dwWidthInBytes,
            int dwLines
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFConvertFromFP16Array(
            float[] pDest,
            short[] pSrc,
            int dwCount
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFConvertToFP16Array(
            short[] pDest,
            float[] pSrc,
            int dwCount
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateMediaTypeFromRepresentation(
            Guid guidRepresentation,
            IntPtr pvRepresentation,
            out IMFMediaType ppIMediaType
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFGetSystemTime(
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFSerializeAttributesToStream(
            IMFAttributes pAttr,
            int dwOptions,
            IStream pStm);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFDeserializeAttributesFromStream(
            IMFAttributes pAttr,
            int dwOptions,
            IStream pStm);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreatePMPMediaSession(
            MFPMPSESSION_CREATION_FLAGS dwCreationFlags,
            IMFAttributes pConfiguration,
            out IMFMediaSession ppMediaSession,
            out IMFActivate ppEnablerActivate
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFGetSupportedSchemes(
            [MarshalAs(UnmanagedType.LPStruct)] out PropVariant pPropVarSchemeArray
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFGetSupportedMimeTypes(
            [MarshalAs(UnmanagedType.LPStruct)] out PropVariant pPropVarMimeTypeArray
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFRequireProtectedEnvironment(
            IMFPresentationDescriptor pPresentationDescriptor
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFSerializePresentationDescriptor(
            IMFPresentationDescriptor pPD,
            out int pcbData,
            IntPtr ppbData
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFDeserializePresentationDescriptor(
            int cbData,
            IntPtr pbData,
            out IMFPresentationDescriptor ppPD
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateSimpleTypeHandler(
            out IMFMediaTypeHandler ppHandler
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFShutdownObject(
            object pUnk
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateAudioRenderer(
            IMFAttributes pAudioAttributes,
            out IMFMediaSink ppSink
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateTopoLoader(
            out IMFTopoLoader ppObj
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateSampleGrabberSinkActivate(
            IMFMediaType pIMFMediaType,
            IMFSampleGrabberSinkCallback pIMFSampleGrabberSinkCallback,
            out IMFActivate ppIActivate
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateStandardQualityManager(
            out IMFQualityManager ppQualityManager
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateQualityManager(
            out IMFQualityManager ppQualityManager
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateSequencerSource(
            [MarshalAs(UnmanagedType.IUnknown)] object pReserved,
            out IMFSequencerSource ppSequencerSource
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateSequencerSegmentOffset(
            int dwId,
            long hnsOffset,
            [MarshalAs(UnmanagedType.LPStruct)] out PropVariant pvarSegmentOffset
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateCredentialCache(
            out IMFNetCredentialCache ppCache
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateProxyLocator(
            string pszProtocol,
            IPropertyStore pProxyConfig,
            out IMFNetProxyLocator ppProxyLocator
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateNetSchemePlugin(
            [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] object ppvHandler
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreatePMPServer(
            int dwCreationFlags,
            out IMFPMPServer ppPMPServer
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateRemoteDesktopPlugin(
            out IMFRemoteDesktopPlugin ppPlugin
        );

#endif

    }
}
