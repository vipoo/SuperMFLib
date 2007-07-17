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
using System.Text;
using System.Runtime.InteropServices;

using MediaFoundation.Misc;

namespace MediaFoundation
{
    #region Declarations

#if ALLOW_UNTESTED_INTERFACES

    [UnmanagedName("ASF_SELECTION_STATUS")]
    public enum ASFSelectionStatus
    {	
        NotSelected	= 0,
	    CleanPointsOnly	= 1,
	    AllDataUnits	= 2
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("ASF_MUX_STATISTICS")]
    public struct ASFMuxStatistics
    {
        int cFramesWritten;
        int cFramesDropped;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("ASF_INDEX_IDENTIFIER")]
    public struct ASFIndexIdentifier
    {
        Guid guidIndexType;
        short wStreamNumber;
    }
#endif

    #endregion

    #region Interfaces

#if ALLOW_UNTESTED_INTERFACES

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("B1DCA5CD-D5DA-4451-8E9E-DB5C59914EAD"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFASFContentInfo
    {
        void GetHeaderSize( 
            /* [in] */ IMFMediaBuffer pIStartOfContent,
            /* [out] */ out long cbHeaderSize);
        
        void ParseHeader( 
            /* [in] */ IMFMediaBuffer pIHeaderBuffer,
            /* [in] */ long cbOffsetWithinHeader);
        
        void GenerateHeader( 
            /* [out][in] */ IMFMediaBuffer pIHeader,
            /* [out] */ out int pcbHeader);
        
        void GetProfile( 
            /* [out] */ out IMFASFProfile ppIProfile);
        
        void SetProfile( 
            /* [in] */ IMFASFProfile pIProfile);
        
        void GeneratePresentationDescriptor( 
            /* [out] */ out IMFPresentationDescriptor ppIPresentationDescriptor);

        void GetEncodingConfigurationPropertyStore(
            /* [in] */ short wStreamNumber,
            /* [out] */ out IPropertyStore ppIStore);
        
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("53590F48-DC3B-4297-813F-787761AD7B3E"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFASFIndexer
    {
        void SetFlags( 
            /* [in] */ int dwFlags);
        
        void GetFlags( 
            /* [out] */ out int pdwFlags);
        
        void Initialize( 
            /* [in] */ IMFASFContentInfo pIContentInfo);
        
        void GetIndexPosition( 
            /* [in] */ IMFASFContentInfo pIContentInfo,
            /* [out] */ out long pcbIndexOffset);
        
        void SetIndexByteStreams( 
            /* [in] */ IMFByteStream [] ppIByteStreams,
            /* [in] */ int cByteStreams);
        
        void GetIndexByteStreamCount( 
            /* [out] */ out int pcByteStreams);
        
        void GetIndexStatus( 
            /* [in] */ ASFIndexIdentifier pIndexIdentifier,
            /* [out] */ out bool pfIsIndexed,
            /* [out] */ out byte pbIndexDescriptor,
            /* [out][in] */ out int pcbIndexDescriptor);
        
        void SetIndexStatus( 
            /* [in] */ IntPtr pbIndexDescriptor,
            /* [in] */ int cbIndexDescriptor,
            /* [in] */ bool fGenerateIndex);
        
        void GetSeekPositionForValue( 
            /* [in] */ PropVariant pvarValue,
            /* [in] */ ASFIndexIdentifier pIndexIdentifier,
            /* [out] */ out long pcbOffsetWithinData,
            /* [optional][out] */ long phnsApproxTime,
            /* [optional][out] */ out int pdwPayloadNumberOfStreamWithinPacket);
        
        void GenerateIndexEntries( 
            /* [in] */ IMFSample pIASFPacketSample);
        
        void CommitIndex( 
            /* [in] */ IMFASFContentInfo pIContentInfo);
        
        void GetIndexWriteSpace( 
            /* [out] */ out long pcbIndexWriteSpace);
        
        void GetCompletedIndex( 
            /* [in] */ IMFMediaBuffer pIIndexBuffer,
            /* [in] */ long cbOffsetWithinIndex);
        
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("57BDD80A-9B38-4838-B737-C58F670D7D4F"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFASFMultiplexer
    {
        void Initialize( 
            /* [in] */ IMFASFContentInfo pIContentInfo);
        
        void SetFlags( 
            /* [in] */ int dwFlags);
        
        void GetFlags( 
            /* [out] */ out int pdwFlags);
        
        void ProcessSample( 
            /* [in] */ short wStreamNumber,
            /* [in] */ IMFSample pISample,
            /* [in] */ long hnsTimestampAdjust);
        
        void GetNextPacket( 
            /* [out] */ out int pdwStatusFlags,
            /* [out] */ out IMFSample ppIPacket);
        
        void Flush( );
        
        void End( 
            /* [out][in] */ IMFASFContentInfo pIContentInfo);
        
        void GetStatistics( 
            /* [in] */ short wStreamNumber,
            /* [out] */ ASFMuxStatistics pMuxStats);
        
        void SetSyncTolerance( 
            /* [in] */ int msSyncTolerance);
        
    }
    
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("12558291-E399-11D5-BC2A-00B0D0F3F4AB"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFASFMutualExclusion
    {
        void GetType( 
            /* [out] */ out Guid pguidType);
        
        void SetType( 
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidType);
        
        void GetRecordCount( 
            /* [out] */ out int pdwRecordCount);
        
        void GetStreamsForRecord( 
            /* [in] */ int dwRecordNumber,
            /* [out] */ out short pwStreamNumArray,
            /* [out][in] */ out int pcStreams);
        
        void AddStreamForRecord( 
            /* [in] */ int dwRecordNumber,
            /* [in] */ short wStreamNumber);
        
        void RemoveStreamFromRecord( 
            /* [in] */ int dwRecordNumber,
            /* [in] */ short wStreamNumber);
        
        void RemoveRecord( 
            /* [in] */ int dwRecordNumber);
        
        void AddRecord( 
            /* [out] */ out int pdwRecordNumber);

        void Clone( 
            /* [out] */ out IMFASFMutualExclusion ppIMutex);
        
    }

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
            [In, MarshalAs(UnmanagedType.LPStruct)] PropVariant Value,
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
            [In, MarshalAs(UnmanagedType.LPStruct)] PropVariant Value
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
            /* [out] */ out int pcStreams);
        
        void GetStream( 
            /* [in] */ int dwStreamIndex,
            /* [out] */ out short pwStreamNumber,
            /* [out] */ out IMFASFStreamConfig ppIStream);
        
        void GetStreamByNumber( 
            /* [in] */ short wStreamNumber,
            /* [out] */ out IMFASFStreamConfig ppIStream);
        
        void SetStream( 
            /* [in] */ IMFASFStreamConfig pIStream);
        
        void RemoveStream( 
            /* [in] */ short wStreamNumber);
        
        void CreateStream( 
            /* [in] */ IMFMediaType pIMediaType,
            /* [out] */ out IMFASFStreamConfig ppIStream);
        
        void GetMutualExclusionCount( 
            /* [out] */ out int pcMutexs);
        
        void GetMutualExclusion( 
            /* [in] */ int dwMutexIndex,
            /* [out] */ out IMFASFMutualExclusion ppIMutex);
        
        void AddMutualExclusion( 
            /* [in] */ IMFASFMutualExclusion pIMutex);
        
        void RemoveMutualExclusion( 
            /* [in] */ int dwMutexIndex);
        
        void CreateMutualExclusion( 
            /* [out] */ out IMFASFMutualExclusion ppIMutex);
        
        void GetStreamPrioritization( 
            /* [out] */ out IMFASFStreamPrioritization ppIStreamPrioritization);
        
        void AddStreamPrioritization( 
            /* [in] */ IMFASFStreamPrioritization pIStreamPrioritization);
        
        void RemoveStreamPrioritization( );
        
        void CreateStreamPrioritization( 
            /* [out] */ out IMFASFStreamPrioritization ppIStreamPrioritization);
        
        void Clone( 
            /* [out] */ out IMFASFProfile ppIProfile);
        
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("12558295-E399-11D5-BC2A-00B0D0F3F4AB"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFASFSplitter
    {
        void Initialize( 
            /* [in] */ IMFASFContentInfo pIContentInfo);
        
        void SetFlags( 
            /* [in] */ int dwFlags);
        
        void GetFlags( 
            /* [out] */ out int pdwFlags);
        
        void SelectStreams( 
            /* [in] */ short [] pwStreamNumbers,
            /* [in] */ short wNumStreams);
        
        void GetSelectedStreams( 
            /* [out] */ short [] pwStreamNumbers,
            /* [out][in] */ out short pwNumStreams);
        
        void ParseData( 
            /* [in] */ IMFMediaBuffer pIBuffer,
            /* [in] */ int cbBufferOffset,
            /* [in] */ int cbLength);
        
        void GetNextSample( 
            /* [out] */ out int pdwStatusFlags,
            /* [out] */ out short pwStreamNumber,
            /* [out] */ out IMFSample ppISample);
        
        void Flush( );
        
        void GetLastSendTime( 
            /* [out] */ out int pdwLastSendTime);
        
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
            [In, MarshalAs(UnmanagedType.LPStruct)] PropVariant Value,
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
            [In, MarshalAs(UnmanagedType.LPStruct)] PropVariant Value
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
            /* [out] */ out Guid pguidStreamType);
        
        [PreserveSig]
        short GetStreamNumber( );
        
        void SetStreamNumber( 
            /* [in] */ short wStreamNum);
        
        void GetMediaType( 
            /* [out] */ out IMFMediaType ppIMediaType);
        
        void SetMediaType( 
            /* [in] */ IMFMediaType pIMediaType);
        
        void GetPayloadExtensionCount( 
            /* [out] */ out short pcPayloadExtensions);
        
        void GetPayloadExtension( 
            /* [in] */ short wPayloadExtensionNumber,
            /* [out] */ out Guid pguidExtensionSystemID,
            /* [out] */ out short pcbExtensionDataSize,
            /* [size_is][optional][out] */ IntPtr pbExtensionSystemInfo,
            /* [optional][out][in] */ out int pcbExtensionSystemInfo);
        
        void AddPayloadExtension( 
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtensionSystemID,
            /* [in] */ short cbExtensionDataSize,
            /* [size_is][in] */ IntPtr pbExtensionSystemInfo,
            /* [in] */ int cbExtensionSystemInfo);
        
        void RemoveAllPayloadExtensions( );
        
        void Clone( 
            /* [out] */ out IMFASFStreamConfig ppIStreamConfig);
        
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("699bdc27-bbaf-49ff-8e38-9c39c9b5e088"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFASFStreamPrioritization
    {
        void GetStreamCount( 
            /* [out] */ out int pdwStreamCount);
        
        void GetStream( 
            /* [in] */ int dwStreamIndex,
            /* [out] */ out short pwStreamNumber,
            /* [out] */ out short pwStreamFlags);
        
        void AddStream( 
            /* [in] */ short wStreamNumber,
            /* [in] */ short wStreamFlags);
        
        void RemoveStream( 
            /* [in] */ int dwStreamIndex);

        void Clone( 
            /* [out] */ out IMFASFStreamPrioritization ppIStreamPrioritization);
        
    }
    
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("d01bad4a-4fa0-4a60-9349-c27e62da9d41"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFASFStreamSelector
    {
        void GetStreamCount( 
            /* [out] */ out int pcStreams);
        
        void GetOutputCount( 
            /* [out] */ out int pcOutputs);
        
        void GetOutputStreamCount( 
            /* [in] */ int dwOutputNum,
            /* [out] */ out int pcStreams);
        
        void GetOutputStreamNumbers( 
            /* [in] */ int dwOutputNum,
            /* [out] */ short [] rgwStreamNumbers);
        
        void GetOutputFromStream( 
            /* [in] */ short wStreamNum,
            /* [out] */ out int pdwOutput);
        
        void GetOutputOverride( 
            /* [in] */ int dwOutputNum,
            /* [out] */ ASFSelectionStatus pSelection);
        
        void SetOutputOverride( 
            /* [in] */ int dwOutputNum,
            /* [in] */ ASFSelectionStatus Selection);
        
        void GetOutputMutexCount( 
            /* [in] */ int dwOutputNum,
            /* [out] */ out int pcMutexes);
        
        void GetOutputMutex( 
            /* [in] */ int dwOutputNum,
            /* [in] */ int dwMutexNum,
            /* [out] */ [MarshalAs(UnmanagedType.IUnknown)] out object ppMutex);
        
        void SetOutputMutexSelection( 
            /* [in] */ int dwOutputNum,
            /* [in] */ int dwMutexNum,
            /* [in] */ short wSelectedRecord);
        
        void GetBandwidthStepCount( 
            /* [out] */ out int pcStepCount);
        
        void GetBandwidthStep( 
            /* [in] */ int dwStepNum,
            /* [out] */ out int pdwBitrate,
            /* [out] */ out short rgwStreamNumbers,
            /* [out] */ ASFSelectionStatus rgSelections);
        
        void BitrateToStepNumber( 
            /* [in] */ int dwBitrate,
            /* [out] */ out int pdwStepNum);
        
        void SetStreamSelectorFlags( 
            /* [in] */ int dwStreamSelectorFlags);

    }
#endif

    #endregion
}
