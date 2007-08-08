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
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

using MediaFoundation.Misc;
using MediaFoundation.Transform;

namespace MediaFoundation
{
    public class MFExtern
    {
        [DllImport("MfPlat.dll", PreserveSig = false)]
        public static extern void MFShutdown();

        [DllImport("MfPlat.dll", PreserveSig = false)]
        public static extern void MFStartup(
            int Version,
            MFStartup dwFlags
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
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] IMFMediaType[] apMediaTypes,
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
            [In, MarshalAs(UnmanagedType.LPArray)] IMFStreamDescriptor[] apStreamDescriptors,
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

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFTRegister(
            [In, MarshalAs(UnmanagedType.Struct)] Guid clsidMFT,
            [In, MarshalAs(UnmanagedType.Struct)] Guid guidCategory,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
            [In] int Flags, // Must be zero
            [In] int cInputTypes,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(RTAMarshaler))] 
            object pInputTypes, // should be MFTRegisterTypeInfo[], but .Net bug prevents in x64
            [In] int cOutputTypes,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(RTAMarshaler))] 
            object pOutputTypes, // should be MFTRegisterTypeInfo[], but .Net bug prevents in x64
            [In] IMFAttributes pAttributes
            );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFTUnregister(
            [In, MarshalAs(UnmanagedType.Struct)] Guid clsidMFT
            );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFTGetInfo(
            [In, MarshalAs(UnmanagedType.Struct)] Guid clsidMFT,
            [MarshalAs(UnmanagedType.LPWStr)] out string pszName,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "0", MarshalTypeRef = typeof(RTIMarshaler))] 
            ArrayList ppInputTypes,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "0", MarshalTypeRef = typeof(RTIMarshaler))] 
            MFInt pcInputTypes,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "1", MarshalTypeRef = typeof(RTIMarshaler))] 
            ArrayList ppOutputTypes,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "1", MarshalTypeRef = typeof(RTIMarshaler))] 
            MFInt pcOutputTypes,
            IntPtr ip // Must be IntPtr.Zero due to MF bug, but should be out IMFAttributes ppAttributes
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void CreateNamedPropertyStore(
            out INamedPropertyStore ppStore
        );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFLockPlatform();

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFUnlockPlatform();

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFGetTimerPeriodicity(
            out int Periodicity);

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFCreateFile(
            MFFileAccessMode AccessMode,
            MFFileOpenMode OpenMode,
            MFFileFlags fFlags,
            [MarshalAs(UnmanagedType.LPWStr)] string pwszFileURL,
            out IMFByteStream ppIByteStream);

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFCreateTempFile(
            MFFileAccessMode AccessMode,
            MFFileOpenMode OpenMode,
            MFFileFlags fFlags,
            out IMFByteStream ppIByteStream);

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFBeginCreateFile(
            [In] MFFileAccessMode AccessMode,
            [In] MFFileOpenMode OpenMode,
            [In] MFFileFlags fFlags,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszFilePath,
            [In] IMFAsyncCallback pCallback,
            [In] [MarshalAs(UnmanagedType.IUnknown)] object pState,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppCancelCookie);

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFEndCreateFile(
            [In] IMFAsyncResult pResult,
            out IMFByteStream ppFile);

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFCancelCreateFile(
            [In] [MarshalAs(UnmanagedType.IUnknown)] object pCancelCookie);

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFCreateAlignedMemoryBuffer(
            [In] int cbMaxLength,
            [In] int cbAligment,
            out IMFMediaBuffer ppBuffer);

        [DllImport("mfplat.dll", PreserveSig = true)]
        public static extern long MFGetSystemTime(
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFGetSupportedSchemes(
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pPropVarSchemeArray
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFGetSupportedMimeTypes(
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pPropVarSchemeArray
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateSimpleTypeHandler(
            out IMFMediaTypeHandler ppHandler
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateSequencerSegmentOffset(
            int dwId,
            long hnsOffset,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvarSegmentOffset
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateVideoRenderer(
            [MarshalAs(UnmanagedType.LPStruct)] Guid riidRenderer,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppVideoRenderer
            );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFCreateMediaBufferWrapper(
            [In] IMFMediaBuffer pBuffer,
            [In] int cbOffset,
            [In] int dwLength,
            out IMFMediaBuffer ppBuffer);

        // Technically, the last param should be an IMediaBuffer.  However, that interface is
        // beyond the scope of this library.  If you are using DirectShowNet (where this *is*
        // defined), you can cast from the object to the IMediaBuffer.
        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFCreateLegacyMediaBufferOnMFMediaBuffer(
            [In] IMFSample pSample,
            [In] IMFMediaBuffer pMFMediaBuffer,
            [In] int cbOffset,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppMediaBuffer);

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFInitAttributesFromBlob(
            [In] IMFAttributes pAttributes,
            IntPtr pBuf,
            [In] int cbBufSize
            );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFGetAttributesAsBlobSize(
            [In] IMFAttributes pAttributes,
            out int pcbBufSize
            );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFGetAttributesAsBlob(
            [In] IMFAttributes pAttributes,
            IntPtr pBuf,
            [In] int cbBufSize
            );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFSerializeAttributesToStream(
            IMFAttributes pAttr,
            MFAttributeSerializeOptions dwOptions,
            IStream pStm);

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFDeserializeAttributesFromStream(
            IMFAttributes pAttr,
            MFAttributeSerializeOptions dwOptions,
            IStream pStm);

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFCreateMFVideoFormatFromMFMediaType(
            [In] IMFMediaType pMFType,
            out MFVideoFormat ppMFVF,
            out int pcbSize
            );

        [DllImport("evr.dll", PreserveSig = true)]
        public static extern int MFGetUncompressedVideoFormat(
            [In, MarshalAs(UnmanagedType.LPStruct)] MFVideoFormat pVideoFormat
        );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFInitMediaTypeFromMFVideoFormat(
            [In] IMFMediaType pMFType,
            MFVideoFormat pMFVF,
            [In] int cbBufSize
            );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFInitAMMediaTypeFromMFMediaType(
            [In] IMFMediaType pMFType,
            [In, MarshalAs(UnmanagedType.Struct)] Guid guidFormatBlockType,
            [Out] AMMediaType pAMType
            );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFCreateAMMediaTypeFromMFMediaType(
            [In] IMFMediaType pMFType,
            [In, MarshalAs(UnmanagedType.Struct)] Guid guidFormatBlockType,
            out AMMediaType ppAMType // delete with DeleteMediaType
            );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFInitMediaTypeFromAMMediaType(
            [In] IMFMediaType pMFType,
            [In] AMMediaType pAMType
            );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFInitMediaTypeFromVideoInfoHeader(
            [In] IMFMediaType pMFType,
            VideoInfoHeader pVIH,
            [In] int cbBufSize,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pSubtype
            );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFInitMediaTypeFromVideoInfoHeader2(
            [In] IMFMediaType pMFType,
            VideoInfoHeader2 pVIH2,
            [In] int cbBufSize,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pSubtype
            );

        [DllImport("evr.dll", PreserveSig = false)]
        public static extern void MFCreateVideoMediaType(
            MFVideoFormat pVideoFormat,
            out IMFVideoMediaType ppIVideoMediaType
            );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFTEnum(
            [In, MarshalAs(UnmanagedType.Struct)] Guid guidCategory,
            [In] int Flags, // Must be zero
            [In, MarshalAs(UnmanagedType.LPStruct)] MFTRegisterTypeInfo pInputType,
            [In, MarshalAs(UnmanagedType.LPStruct)] MFTRegisterTypeInfo pOutputType,
            [In] IMFAttributes pAttributes,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "0", MarshalTypeRef = typeof(GAMarshaler))]             
            ArrayList ppclsidMFT,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "0", MarshalTypeRef = typeof(GAMarshaler))]             
            MFInt pcMFTs
            );

#if ALLOW_UNTESTED_INTERFACES

        #region Tested
        // While these methods are tested, the interfaces they use are not

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
        public static extern void MFCreateStandardQualityManager(
            out IMFQualityManager ppQualityManager
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateSequencerSource(
            [MarshalAs(UnmanagedType.IUnknown)] object pReserved,
            out IMFSequencerSource ppSequencerSource
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreatePMPServer(
            MFPMPSessionCreationFlags dwCreationFlags,
            out IMFPMPServer ppPMPServer
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateASFContentInfo(
            out IMFASFContentInfo ppIContentInfo);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateASFProfile(
            out IMFASFProfile ppIProfile);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateASFSplitter(
            out IMFASFSplitter ppISplitter);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateASFMultiplexer(
            out IMFASFMultiplexer ppIMultiplexer);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateASFIndexer(
            out IMFASFIndexer ppIIndexer);

        #endregion

        #region Work Queue

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFPutWorkItem(
            int dwQueue,
            IMFAsyncCallback pCallback,
            [MarshalAs(UnmanagedType.IUnknown)] object pState);

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFPutWorkItemEx(
            int dwQueue,
            IMFAsyncResult pResult);

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFScheduleWorkItem(
            IMFAsyncCallback pCallback,
            [MarshalAs(UnmanagedType.IUnknown)] object pState,
            long Timeout,
            long pKey);

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFScheduleWorkItemEx(
            IMFAsyncResult pResult,
            long Timeout,
            long pKey);

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFCancelWorkItem(
            long Key);

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFAddPeriodicCallback(
            IntPtr Callback,
            [MarshalAs(UnmanagedType.IUnknown)] object pContext,
            out int pdwKey);

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFRemovePeriodicCallback(
            int dwKey);

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFAllocateWorkQueue(
            out int pdwWorkQueue);

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFLockWorkQueue(
            [In] int dwWorkQueue);

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFUnlockWorkQueue(
            [In] int dwWorkQueue);

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFBeginRegisterWorkQueueWithMMCSS(
            int dwWorkQueueId,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszClass,
            int dwTaskId,
            [In] IMFAsyncCallback pDoneCallback,
            [In] [MarshalAs(UnmanagedType.IUnknown)] object pDoneState);

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFEndRegisterWorkQueueWithMMCSS(
            [In] IMFAsyncResult pResult,
            out int pdwTaskId);

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFBeginUnregisterWorkQueueWithMMCSS(
            int dwWorkQueueId,
            [In] IMFAsyncCallback pDoneCallback,
            [In] [MarshalAs(UnmanagedType.IUnknown)] object pDoneState);

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFEndUnregisterWorkQueueWithMMCSS(
            [In] IMFAsyncResult pResult);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFSetWorkQueueClass(
            int dwWorkQueueId,
            [MarshalAs(UnmanagedType.LPWStr)] string szClass);

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFGetWorkQueueMMCSSClass(
            int dwWorkQueueId,
            [MarshalAs(UnmanagedType.LPWStr)] out string pwszClass,
            out int pcchClass);

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFGetWorkQueueMMCSSTaskId(
            int dwWorkQueueId,
            out int pdwTaskId);

        #endregion

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateRemoteDesktopPlugin(
            out IMFRemoteDesktopPlugin ppPlugin
        );

        [DllImport("evr.dll", PreserveSig = false)]
        public static extern void MFCreateDXSurfaceBuffer(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [In] [MarshalAs(UnmanagedType.IUnknown)] object punkSurface,
            [In] bool fBottomUpWhenLinear,
            out IMFMediaBuffer ppBuffer);

        // --------------------------------------------------

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFValidateMediaTypeSize(
            [In, MarshalAs(UnmanagedType.Struct)] Guid FormatType,
            IntPtr pBlock,
            [In] int cbSize
            );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFInitMediaTypeFromMPEG1VideoInfo(
            [In] IMFMediaType pMFType,
            MPEG1VideoInfo pMP1VI,
            [In] int cbBufSize,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pSubtype
            );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFInitMediaTypeFromMPEG2VideoInfo(
            [In] IMFMediaType pMFType,
            Mpeg2VideoInfo pMP2VI,
            [In] int cbBufSize,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pSubtype
            );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFCalculateBitmapImageSize(
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(BMMarshaler))] BitmapInfoHeader pBMIH,
            [In] int cbBufSize,
            out int pcbImageSize,
            out bool pbKnown
            );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFCalculateImageSize(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidSubtype,
            [In] int unWidth,
            [In] int unHeight,
            out int pcbImageSize
            );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFFrameRateToAverageTimePerFrame(
            [In] int unNumerator,
            [In] int unDenominator,
            out long punAverageTimePerFrame
            );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFAverageTimePerFrameToFrameRate(
            [In] long unAverageTimePerFrame,
            out int punNumerator,
            out int punDenominator
            );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFCompareFullToPartialMediaType(
            [In] IMFMediaType pMFTypeFull,
            [In] IMFMediaType pMFTypePartial
            );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFWrapMediaType(
            [In] IMFMediaType pOrig,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid MajorType,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid SubType,
            out IMFMediaType ppWrap
            );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFUnwrapMediaType(
            [In] IMFMediaType pWrap,
            out IMFMediaType ppOrig
            );

        [DllImport("evr.dll", PreserveSig = false)]
        public static extern void MFCreateVideoMediaTypeFromSubtype(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pAMSubtype,
            out IMFVideoMediaType ppIVideoMediaType
            );

        [DllImport("evr.dll", PreserveSig = false)]
        public static extern void MFIsFormatYUV(
            int Format
            );

        [DllImport("evr.dll", PreserveSig = false)]
        public static extern void MFGetStrideForBitmapInfoHeader(
            int format,
            int dwWidth,
            out int pStride
            );

        [DllImport("evr.dll", PreserveSig = false)]
        public static extern void MFGetPlaneSize(
            int format,
            int dwWidth,
            int dwHeight,
            out int pdwPlaneSize
            );

        [DllImport("evr.dll", PreserveSig = false)]
        public static extern void MFInitVideoFormat(
            [In] MFVideoFormat pVideoFormat,
            [In] MFStandardVideoFormat type
            );

        [DllImport("evr.dll", PreserveSig = false)]
        public static extern void MFInitVideoFormat_RGB(
            [In] MFVideoFormat pVideoFormat,
            [In] int dwWidth,
            [In] int dwHeight,
            [In] int D3Dfmt
            );

        [DllImport("evr.dll", PreserveSig = false)]
        public static extern void MFConvertColorInfoToDXVA(
            out int pdwToDXVA,
            MFVideoFormat pFromFormat
            );

        [DllImport("evr.dll", PreserveSig = false)]
        public static extern void MFConvertColorInfoFromDXVA(
            MFVideoFormat pToFormat,
            int dwFromDXVA
            );

        [DllImport("evr.dll", PreserveSig = false)]
        public static extern void MFCopyImage(
            IntPtr pDest,
            int lDestStride,
            IntPtr pSrc,
            int lSrcStride,
            int dwWidthInBytes,
            int dwLines
            );

        [DllImport("evr.dll", PreserveSig = false)]
        public static extern void MFConvertFromFP16Array(
            float[] pDest,
            short[] pSrc,
            int dwCount
            );

        [DllImport("evr.dll", PreserveSig = false)]
        public static extern void MFConvertToFP16Array(
            short[] pDest,
            float[] pSrc,
            int dwCount
            );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFCreateMediaTypeFromRepresentation(
            [MarshalAs(UnmanagedType.Struct)] Guid guidRepresentation,
            IntPtr pvRepresentation,
            out IMFMediaType ppIMediaType
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreatePMPMediaSession(
            MFPMPSessionCreationFlags dwCreationFlags,
            IMFAttributes pConfiguration,
            out IMFMediaSession ppMediaSession,
            out IMFActivate ppEnablerActivate
        );

        [DllImport("mf.dll", PreserveSig = true)]
        public static extern int MFRequireProtectedEnvironment(
            IMFPresentationDescriptor pPresentationDescriptor
        );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFSerializePresentationDescriptor(
            IMFPresentationDescriptor pPD,
            out int pcbData,
            IntPtr ppbData
        );

        [DllImport("mfplat.dll", PreserveSig = false)]
        public static extern void MFDeserializePresentationDescriptor(
            int cbData,
            IntPtr pbData,
            out IMFPresentationDescriptor ppPD
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFShutdownObject(
            object pUnk
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateSampleGrabberSinkActivate(
            IMFMediaType pIMFMediaType,
            IMFSampleGrabberSinkCallback pIMFSampleGrabberSinkCallback,
            out IMFActivate ppIActivate
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateProxyLocator(
            [MarshalAs(UnmanagedType.LPWStr)] string pszProtocol,
            IPropertyStore pProxyConfig,
            out IMFNetProxyLocator ppProxyLocator
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateNetSchemePlugin(
            [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] object ppvHandler
        );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateASFProfileFromPresentationDescriptor(
            [In] IMFPresentationDescriptor pIPD,
            out IMFASFProfile ppIProfile);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateASFIndexerByteStream(
            [In] IMFByteStream pIContentByteStream,
            [In] long cbIndexStartOffset,
            out IMFByteStream pIIndexByteStream);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateASFStreamSelector(
            [In] IMFASFProfile pIASFProfile,
            out IMFASFStreamSelector ppSelector);

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateASFMediaSink(
            IMFByteStream pIByteStream,
            out IMFMediaSink ppIMediaSink
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateASFMediaSinkActivate(
            [MarshalAs(UnmanagedType.LPWStr)] string pwszFileName,
            IMFASFContentInfo pContentInfo,
            out IMFActivate ppIActivate
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateWMVEncoderActivate(
            IMFMediaType pMediaType,
            IPropertyStore pEncodingConfigurationProperties,
            out IMFActivate ppActivate
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreateWMAEncoderActivate(
            IMFMediaType pMediaType,
            IPropertyStore pEncodingConfigurationProperties,
            out IMFActivate ppActivate
            );

        [DllImport("mf.dll", PreserveSig = false)]
        public static extern void MFCreatePresentationDescriptorFromASFProfile(
            [In] IMFASFProfile pIProfile,
            out IMFPresentationDescriptor ppIPD);

        [DllImport("evr.dll", PreserveSig = false)]
        public static extern void MFCreateVideoPresenter(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pOwner,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riidDevice,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            out IntPtr ppVideoPresenter
            );

        [DllImport("evr.dll", PreserveSig = false)]
        public static extern void MFCreateVideoMixer(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pOwner,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riidDevice,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            out IntPtr ppVideoMixer
            );

        [DllImport("evr.dll", PreserveSig = false)]
        public static extern void MFCreateVideoMixerAndPresenter(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pMixerOwner,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pPresenterOwner,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riidMixer,
            out IntPtr ppvVideoMixer,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riidPresenter,
            out IntPtr ppvVideoPresenter
            );

        [DllImport("evr.dll", PreserveSig = false)]
        public static extern void MFCreateVideoSampleFromSurface(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkSurface,
            out IMFSample ppSample
            );

        #region Untestable

        [DllImport("mfplat.dll", PreserveSig = false), Obsolete("This function is deprecated")]
        public static extern void MFCreateAudioMediaType(
            [In] WaveFormatEx pAudioFormat,
            out IMFAudioMediaType ppIAudioMediaType
            );

        [DllImport("mf.dll", PreserveSig = false), Obsolete("The returned object doesn't support QI")]
        public static extern void MFCreateCredentialCache(
            out IMFNetCredentialCache ppCache
        );

        [DllImport("evr.dll", PreserveSig = false), Obsolete("Not implemented")]
        public static extern void MFCreateVideoMediaTypeFromBitMapInfoHeader(
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(BMMarshaler))] BitmapInfoHeader pbmihBitMapInfoHeader,
            int dwPixelAspectRatioX,
            int dwPixelAspectRatioY,
            MFVideoInterlaceMode InterlaceMode,
            long VideoFlags,
            long qwFramesPerSecondNumerator,
            long qwFramesPerSecondDenominator,
            int dwMaxBitRate,
            out IMFVideoMediaType ppIVideoMediaType
            );

        [DllImport("mf.dll", PreserveSig = false), Obsolete("Interface doesn't exist")]
        public static extern void MFCreateQualityManager(
            out IMFQualityManager ppQualityManager
        );

        [DllImport("evr.dll", PreserveSig = false), Obsolete("Undoc'ed")]
        public static extern void MFCreateVideoMediaTypeFromVideoInfoHeader(
            VideoInfoHeader pVideoInfoHeader,
            int cbVideoInfoHeader,
            int dwPixelAspectRatioX,
            int dwPixelAspectRatioY,
            MFVideoInterlaceMode InterlaceMode,
            long VideoFlags,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pSubtype,
            out IMFVideoMediaType ppIVideoMediaType
            );

        [DllImport("evr.dll", PreserveSig = false), Obsolete("Undoc'ed")]
        public static extern void MFCreateVideoMediaTypeFromVideoInfoHeader2(
            VideoInfoHeader2 pVideoInfoHeader,
            int cbVideoInfoHeader,
            long AdditionalVideoFlags,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pSubtype,
            out IMFVideoMediaType ppIVideoMediaType
            );

        #endregion
#endif
    }
}
