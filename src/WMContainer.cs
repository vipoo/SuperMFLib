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
using System.Text;
using System.Runtime.InteropServices;

using MediaFoundation.Misc;

namespace MediaFoundation
{
    #region Declarations

#if ALLOW_UNTESTED_INTERFACES

    [Flags, UnmanagedName("MFASF_MULTIPLEXERFLAGS")]
    public enum MFASFMultiplexerFlags
    {
        None = 0,
        AutoAdjustBitrate = 0x00000001
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("ASF_MUX_STATISTICS")]
    public struct ASFMuxStatistics
    {
        public int cFramesWritten;
        public int cFramesDropped;
    }

#endif

    [Flags, UnmanagedName("MFASF_SPLITTERFLAGS")]
    public enum MFASFSplitterFlags
    {
        None = 0,
        Reverse = 0x00000001,
        WMDRM = 0x00000002
    }

    [Flags, UnmanagedName("ASF_STATUSFLAGS")]
    public enum ASFStatusFlags
    {	
        None = 0,
        Incomplete = 0x1
    }

    [Flags, UnmanagedName("MFASF_STREAMSELECTORFLAGS")]
    public enum MFAsfStreamSelectorFlags
    {
        None = 0x00000000,
        DisableThinning = 0x00000001,
        UseAverageBitrate = 0x00000002
    }

    [UnmanagedName("ASF_SELECTION_STATUS")]
    public enum ASFSelectionStatus
    {
        NotSelected = 0,
        CleanPointsOnly = 1,
        AllDataUnits = 2
    }

    [Flags, UnmanagedName("MFASF_INDEXERFLAGS")]
    public enum MFAsfIndexerFlags
    {
        None = 0x0,
        WriteNewIndex = 0x00000001,
        ReadForReversePlayback = 0x00000004,
        WriteForLiveRead = 0x00000008
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("ASF_INDEX_IDENTIFIER")]
    public class ASFIndexIdentifier
    {
        public Guid guidIndexType;
        public short wStreamNumber;
    }

    public sealed class MFASFSampleExtension
    {
        public static readonly Guid SampleDuration = new Guid(0xc6bd9450, 0x867f, 0x4907, 0x83, 0xa3, 0xc7, 0x79, 0x21, 0xb7, 0x33, 0xad);
        public static readonly Guid OutputCleanPoint = new Guid(0xf72a3c6f, 0x6eb4, 0x4ebc, 0xb1, 0x92, 0x9, 0xad, 0x97, 0x59, 0xe8, 0x28);
        public static readonly Guid SMPTE = new Guid(0x399595ec, 0x8667, 0x4e2d, 0x8f, 0xdb, 0x98, 0x81, 0x4c, 0xe7, 0x6c, 0x1e);
        public static readonly Guid FileName = new Guid(0xe165ec0e, 0x19ed, 0x45d7, 0xb4, 0xa7, 0x25, 0xcb, 0xd1, 0xe2, 0x8e, 0x9b);
        public static readonly Guid ContentType = new Guid(0xd590dc20, 0x07bc, 0x436c, 0x9c, 0xf7, 0xf3, 0xbb, 0xfb, 0xf1, 0xa4, 0xdc);
        public static readonly Guid PixelAspectRatio = new Guid(0x1b1ee554, 0xf9ea, 0x4bc8, 0x82, 0x1a, 0x37, 0x6b, 0x74, 0xe4, 0xc4, 0xb8);
        public static readonly Guid Encryption_SampleID = new Guid(0x6698B84E, 0x0AFA, 0x4330, 0xAE, 0xB2, 0x1C, 0x0A, 0x98, 0xD7, 0xA4, 0x4D);
        public static readonly Guid Encryption_KeyID = new Guid(0x76376591, 0x795f, 0x4da1, 0x86, 0xed, 0x9d, 0x46, 0xec, 0xa1, 0x09, 0xa9);
    }

    #endregion

    #region Interfaces

#if ALLOW_UNTESTED_INTERFACES

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("699bdc27-bbaf-49ff-8e38-9c39c9b5e088"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFASFStreamPrioritization
    {
        void GetStreamCount(
            out int pdwStreamCount);

        void GetStream(
            [In] int dwStreamIndex,
            out short pwStreamNumber,
            out short pwStreamFlags); // bool

        void AddStream(
            [In] short wStreamNumber,
            [In] short wStreamFlags); // bool

        void RemoveStream(
            [In] int dwStreamIndex);

        void Clone(
            out IMFASFStreamPrioritization ppIStreamPrioritization);
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("57BDD80A-9B38-4838-B737-C58F670D7D4F"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFASFMultiplexer
    {
        void Initialize(
            [In] IMFASFContentInfo pIContentInfo);

        void SetFlags(
            [In] MFASFMultiplexerFlags dwFlags);

        void GetFlags(
            out MFASFMultiplexerFlags pdwFlags);

        void ProcessSample(
            [In] short wStreamNumber,
            [In] IMFSample pISample,
            [In] long hnsTimestampAdjust);

        void GetNextPacket(
            out ASFStatusFlags pdwStatusFlags,
            out IMFSample ppIPacket);

        void Flush( );

        void End(
            [In] IMFASFContentInfo pIContentInfo);

        void GetStatistics(
            [In] short wStreamNumber,
            out ASFMuxStatistics pMuxStats);

        void SetSyncTolerance(
            [In] int msSyncTolerance);
    }

#endif

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("D267BF6A-028B-4e0d-903D-43F0EF82D0D4"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFASFProfile : IMFAttributes
    {
        #region IMFAttributes methods

        new void GetItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pValue
            );

        new void GetItemType(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out MFAttributeType pType
            );

        new void CompareItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value,
            [MarshalAs(UnmanagedType.Bool)] out bool pbResult
            );

        new void Compare(
            [MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
            MFAttributesMatchType MatchType,
            [MarshalAs(UnmanagedType.Bool)] out bool pbResult
            );

        new void GetUINT32(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int punValue
            );

        new void GetUINT64(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out long punValue
            );

        new void GetDouble(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out double pfValue
            );

        new void GetGUID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out Guid pguidValue
            );

        new void GetStringLength(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int pcchLength
            );

        new void GetString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszValue,
            int cchBufSize,
            out int pcchLength
            );

        new void GetAllocatedString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
            out int pcchLength
            );

        new void GetBlobSize(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int pcbBlobSize
            );

        new void GetBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pBuf,
            int cbBufSize,
            out int pcbBlobSize
            );

        // Use GetBlob instead of this
        new void GetAllocatedBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
            out int pcbSize
            );

        new void GetUnknown(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppv
            );

        new void SetItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value
            );

        new void DeleteItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
            );

        new void DeleteAllItems();

        new void SetUINT32(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            int unValue
            );

        new void SetUINT64(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            long unValue
            );

        new void SetDouble(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            double fValue
            );

        new void SetGUID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidValue
            );

        new void SetString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszValue
            );

        new void SetBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pBuf,
            int cbBufSize
            );

        new void SetUnknown(
            [MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnknown
            );

        new void LockStore();

        new void UnlockStore();

        new void GetCount(
            out int pcItems
            );

        new void GetItemByIndex(
            int unIndex,
            out Guid pguidKey,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pValue
            );

        new void CopyAllItems(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
            );

        #endregion

        void GetStreamCount(
            out int pcStreams);

        void GetStream(
            [In] int dwStreamIndex,
            out short pwStreamNumber,
            out IMFASFStreamConfig ppIStream);

        void GetStreamByNumber(
            [In] short wStreamNumber,
            out IMFASFStreamConfig ppIStream);

        void SetStream(
            [In] IMFASFStreamConfig pIStream);

        void RemoveStream(
            [In] short wStreamNumber);

        void CreateStream(
            [In] IMFMediaType pIMediaType,
            out IMFASFStreamConfig ppIStream);

        void GetMutualExclusionCount(
            out int pcMutexs);

        void GetMutualExclusion(
            [In] int dwMutexIndex,
            out IMFASFMutualExclusion ppIMutex);

        void AddMutualExclusion(
            [In] IMFASFMutualExclusion pIMutex);

        void RemoveMutualExclusion(
            [In] int dwMutexIndex);

        void CreateMutualExclusion(
            out IMFASFMutualExclusion ppIMutex);

        void GetStreamPrioritization(
#if ALLOW_UNTESTED_INTERFACES
            out IMFASFStreamPrioritization ppIStreamPrioritization);
#else
            [MarshalAs(UnmanagedType.IUnknown)] out object ppIStreamPrioritization);
#endif

        void AddStreamPrioritization(
#if ALLOW_UNTESTED_INTERFACES
            [In] IMFASFStreamPrioritization pIStreamPrioritization);
#else
            [MarshalAs(UnmanagedType.IUnknown)] object pIStreamPrioritization);
#endif

        void RemoveStreamPrioritization();

        void CreateStreamPrioritization(
#if ALLOW_UNTESTED_INTERFACES
            out IMFASFStreamPrioritization ppIStreamPrioritization);
#else
            [MarshalAs(UnmanagedType.IUnknown)] out object ppIStreamPrioritization);
#endif

        void Clone(
            out IMFASFProfile ppIProfile);
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("9E8AE8D2-DBBD-4200-9ACA-06E6DF484913"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFASFStreamConfig : IMFAttributes
    {
        #region IMFAttributes methods

        new void GetItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pValue
            );

        new void GetItemType(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out MFAttributeType pType
            );

        new void CompareItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value,
            [MarshalAs(UnmanagedType.Bool)] out bool pbResult
            );

        new void Compare(
            [MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
            MFAttributesMatchType MatchType,
            [MarshalAs(UnmanagedType.Bool)] out bool pbResult
            );

        new void GetUINT32(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int punValue
            );

        new void GetUINT64(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out long punValue
            );

        new void GetDouble(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out double pfValue
            );

        new void GetGUID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out Guid pguidValue
            );

        new void GetStringLength(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int pcchLength
            );

        new void GetString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszValue,
            int cchBufSize,
            out int pcchLength
            );

        new void GetAllocatedString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
            out int pcchLength
            );

        new void GetBlobSize(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int pcbBlobSize
            );

        new void GetBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pBuf,
            int cbBufSize,
            out int pcbBlobSize
            );

        // Use GetBlob instead of this
        new void GetAllocatedBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
            out int pcbSize
            );

        new void GetUnknown(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppv
            );

        new void SetItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value
            );

        new void DeleteItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
            );

        new void DeleteAllItems();

        new void SetUINT32(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            int unValue
            );

        new void SetUINT64(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            long unValue
            );

        new void SetDouble(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            double fValue
            );

        new void SetGUID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidValue
            );

        new void SetString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszValue
            );

        new void SetBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pBuf,
            int cbBufSize
            );

        new void SetUnknown(
            [MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnknown
            );

        new void LockStore();

        new void UnlockStore();

        new void GetCount(
            out int pcItems
            );

        new void GetItemByIndex(
            int unIndex,
            out Guid pguidKey,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pValue
            );

        new void CopyAllItems(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
            );

        #endregion

        void GetStreamType(
            out Guid pguidStreamType);

        [PreserveSig]
        short GetStreamNumber();

        void SetStreamNumber(
            [In] short wStreamNum);

        void GetMediaType(
            out IMFMediaType ppIMediaType);

        void SetMediaType(
            [In] IMFMediaType pIMediaType);

        void GetPayloadExtensionCount(
            out short pcPayloadExtensions);

        void GetPayloadExtension(
            [In] short wPayloadExtensionNumber,
            out Guid pguidExtensionSystemID,
            out short pcbExtensionDataSize,
            IntPtr pbExtensionSystemInfo,
            ref int pcbExtensionSystemInfo);

        void AddPayloadExtension(
            [In, MarshalAs(UnmanagedType.Struct)] Guid guidExtensionSystemID,
            [In] short cbExtensionDataSize,
            IntPtr pbExtensionSystemInfo,
            [In] int cbExtensionSystemInfo);

        void RemoveAllPayloadExtensions();

        void Clone(
            out IMFASFStreamConfig ppIStreamConfig);
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("d01bad4a-4fa0-4a60-9349-c27e62da9d41"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFASFStreamSelector
    {
        void GetStreamCount(
            out int pcStreams);

        void GetOutputCount(
            out int pcOutputs);

        void GetOutputStreamCount(
            [In] int dwOutputNum,
            out int pcStreams);

        void GetOutputStreamNumbers(
            [In] int dwOutputNum,
            [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I2)] short[] rgwStreamNumbers);

        void GetOutputFromStream(
            [In] short wStreamNum,
            out int pdwOutput);

        void GetOutputOverride(
            [In] int dwOutputNum,
            out ASFSelectionStatus pSelection);

        void SetOutputOverride(
            [In] int dwOutputNum,
            [In] ASFSelectionStatus Selection);

        void GetOutputMutexCount(
            [In] int dwOutputNum,
            out int pcMutexes);

        void GetOutputMutex(
            [In] int dwOutputNum,
            [In] int dwMutexNum,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppMutex);

        void SetOutputMutexSelection(
            [In] int dwOutputNum,
            [In] int dwMutexNum,
            [In] short wSelectedRecord);

        void GetBandwidthStepCount(
            out int pcStepCount);

        void GetBandwidthStep(
            [In] int dwStepNum,
            out int pdwBitrate,
            [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I2)] short[] rgwStreamNumbers,
            [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)] ASFSelectionStatus[] rgSelections);

        void BitrateToStepNumber(
            [In] int dwBitrate,
            out int pdwStepNum);

        void SetStreamSelectorFlags(
            [In] MFAsfStreamSelectorFlags dwStreamSelectorFlags);
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("12558291-E399-11D5-BC2A-00B0D0F3F4AB"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFASFMutualExclusion
    {
        void GetType(
            out Guid pguidType);

        void SetType(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidType);

        void GetRecordCount(
            out int pdwRecordCount);

        void GetStreamsForRecord(
            [In] int dwRecordNumber,
            [In, Out, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.I4)] short [] pwStreamNumArray,
            ref int pcStreams);

        void AddStreamForRecord(
            [In] int dwRecordNumber,
            [In] short wStreamNumber);

        void RemoveStreamFromRecord(
            [In] int dwRecordNumber,
            [In] short wStreamNumber);

        void RemoveRecord(
            [In] int dwRecordNumber);

        void AddRecord(
            out int pdwRecordNumber);

        void Clone(
            out IMFASFMutualExclusion ppIMutex);
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("12558295-E399-11D5-BC2A-00B0D0F3F4AB"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFASFSplitter
    {
        void Initialize(
            [In] IMFASFContentInfo pIContentInfo);

        void SetFlags(
            [In] MFASFSplitterFlags dwFlags);

        void GetFlags(
            out MFASFSplitterFlags pdwFlags);

        void SelectStreams(
            [In] short[] pwStreamNumbers,
            [In] short wNumStreams);

        void GetSelectedStreams(
            [In, Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I2)] short[] pwStreamNumbers,
            ref short pwNumStreams);

        void ParseData(
            [In] IMFMediaBuffer pIBuffer,
            [In] int cbBufferOffset,
            [In] int cbLength);

        void GetNextSample(
            out ASFStatusFlags pdwStatusFlags,
            out short pwStreamNumber,
            out IMFSample ppISample);

        void Flush();

        void GetLastSendTime(
            out int pdwLastSendTime);
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("B1DCA5CD-D5DA-4451-8E9E-DB5C59914EAD"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFASFContentInfo
    {
        void GetHeaderSize(
            [In] IMFMediaBuffer pIStartOfContent,
            out long cbHeaderSize);

        void ParseHeader(
            [In] IMFMediaBuffer pIHeaderBuffer,
            [In] long cbOffsetWithinHeader);

        void GenerateHeader(
            [In] IMFMediaBuffer pIHeader,
            out int pcbHeader);

        void GetProfile(
            out IMFASFProfile ppIProfile);

        void SetProfile(
            [In] IMFASFProfile pIProfile);

        void GeneratePresentationDescriptor(
            out IMFPresentationDescriptor ppIPresentationDescriptor);

        void GetEncodingConfigurationPropertyStore(
            [In] short wStreamNumber,
            out IPropertyStore ppIStore);
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("53590F48-DC3B-4297-813F-787761AD7B3E"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFASFIndexer
    {
        void SetFlags(
            [In] MFAsfIndexerFlags dwFlags);

        void GetFlags(
            out MFAsfIndexerFlags pdwFlags);

        void Initialize(
            [In] IMFASFContentInfo pIContentInfo);

        void GetIndexPosition(
            [In] IMFASFContentInfo pIContentInfo,
            out long pcbIndexOffset);

        void SetIndexByteStreams(
            [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.IUnknown)] IMFByteStream[] ppIByteStreams,
            [In] int cByteStreams);

        void GetIndexByteStreamCount(
            out int pcByteStreams);

        void GetIndexStatus(
            [In, MarshalAs(UnmanagedType.LPStruct)] ASFIndexIdentifier pIndexIdentifier,
            out bool pfIsIndexed,
            IntPtr pbIndexDescriptor,
            ref int pcbIndexDescriptor);

        void SetIndexStatus(
            [In] IntPtr pbIndexDescriptor,
            [In] int cbIndexDescriptor,
            [In] bool fGenerateIndex);

        void GetSeekPositionForValue(
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvarValue,
            [In, MarshalAs(UnmanagedType.LPStruct)] ASFIndexIdentifier pIndexIdentifier,
            out long pcbOffsetWithinData,
            IntPtr phnsApproxTime,
            out int pdwPayloadNumberOfStreamWithinPacket);

        void GenerateIndexEntries(
            [In] IMFSample pIASFPacketSample);

        void CommitIndex(
            [In] IMFASFContentInfo pIContentInfo);

        void GetIndexWriteSpace(
            out long pcbIndexWriteSpace);

        void GetCompletedIndex(
            [In] IMFMediaBuffer pIIndexBuffer,
            [In] long cbOffsetWithinIndex);
    }

    #endregion
}
