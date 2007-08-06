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

using MediaFoundation.Misc;

namespace MediaFoundation.Transform
{
    #region Declarations

    [UnmanagedName("_MFT_DRAIN_TYPE")]
    public enum MFTDrainType
    {
        ProduceTails = 0x00000000,
        NoTails = 0x00000001
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFT_REGISTER_TYPE_INFO")]
    public class MFTRegisterTypeInfo
    {
        public Guid guidMajorType;
        public Guid guidSubtype;
    }

    [Flags, UnmanagedName("_MFT_PROCESS_OUTPUT_FLAGS")]
    public enum MFTProcessOutputFlags
    {
        None = 0,
        DiscardWhenNoBuffer = 0x00000001
    }

    [Flags, UnmanagedName("_MFT_OUTPUT_STATUS_FLAGS")]
    public enum MFTOutputStatusFlags
    {
        None = 0,
        SampleReady = 0x00000001
    }

    [Flags, UnmanagedName("_MFT_INPUT_STATUS_FLAGS")]
    public enum MFTInputStatusFlags
    {
        None = 0,
        AcceptData = 0x00000001
    }

    [Flags, UnmanagedName("_MFT_SET_TYPE_FLAGS")]
    public enum MFTSetTypeFlags
    {
        None = 0,
        TestOnly = 0x00000001
    }

    [Flags, UnmanagedName("_MFT_OUTPUT_STREAM_INFO_FLAGS")]
    public enum MFTOutputStreamInfoFlags
    {
        None = 0,
        WholeSamples = 0x00000001,
        SingleSamplePerBuffer = 0x00000002,
        FixedSampleSize = 0x00000004,
        Discardable = 0x00000008,
        Optional = 0x00000010,
        ProvidesSamples = 0x00000100,
        CanProvideSamples = 0x00000200,
        LazyRead = 0x00000400,
        Removable = 0x00000800
    }

    [Flags, UnmanagedName("_MFT_INPUT_STREAM_INFO_FLAGS")]
    public enum MFTInputStreamInfoFlags
    {
        WholeSamples = 0x1,
        SingleSamplePerBuffer = 0x2,
        FixedSampleSize = 0x4,
        HoldsBuffers = 0x8,
        DoesNotAddRef = 0x100,
        Removable = 0x200,
        Optional = 0x400,
        ProcessesInPlace = 0x800
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8), UnmanagedName("MFT_INPUT_STREAM_INFO")]
    public struct MFTInputStreamInfo
    {
        public long hnsMaxLatency;
        public MFTInputStreamInfoFlags dwFlags;
        public int cbSize;
        public int cbMaxLookahead;
        public int cbAlignment;
    }

    [UnmanagedName("MFT_MESSAGE_TYPE")]
    public enum MFTMessageType
    {
        CommandDrain = 1,
        CommandFlush = 0,
        NotifyBeginStreaming = 0x10000000,
        NotifyEndOfStream = 0x10000002,
        NotifyEndStreaming = 0x10000001,
        NotifyStartOfStream = 0x10000003,
        SetD3DManager = 2
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFT_OUTPUT_DATA_BUFFER")]
    public struct MFTOutputDataBuffer
    {
        public int dwStreamID;
        public IntPtr pSample; // Doesn't release correctly when marshaled as IMFSample
        public MFTOutputDataBufferFlags dwStatus;
        [MarshalAs(UnmanagedType.Interface)] public IMFCollection pEvents;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4), UnmanagedName("MFT_OUTPUT_STREAM_INFO")]
    public struct MFTOutputStreamInfo
    {
        public MFTOutputStreamInfoFlags dwFlags;
        public int cbSize;
        public int cbAlignment;
    }

    [Flags, UnmanagedName("_MFT_OUTPUT_DATA_BUFFER_FLAGS")]
    public enum MFTOutputDataBufferFlags
    {
        None = 0,
        Incomplete = 0x01000000,
        FormatChange = 0x00000100,
        StreamEnd = 0x00000200,
        NoSample = 0x00000300
    };

    [Flags, UnmanagedName("_MFT_PROCESS_OUTPUT_STATUS")]
    public enum ProcessOutputStatus
    {
        None = 0,
        NewStreams = 0x00000100
    }

    #endregion

    #region Interfaces

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("BF94C121-5B05-4E6F-8000-BA598961414D")]
    public interface IMFTransform
    {
        void GetStreamLimits(
            [Out] MFInt pdwInputMinimum,
            [Out] MFInt pdwInputMaximum,
            [Out] MFInt pdwOutputMinimum,
            [Out] MFInt pdwOutputMaximum
            );

        void GetStreamCount(
            [Out] MFInt pcInputStreams,
            [Out] MFInt pcOutputStreams
            );

        void GetStreamIDs(
            int dwInputIDArraySize,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] int [] pdwInputIDs,
            int dwOutputIDArraySize,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] int[] pdwOutputIDs
            );

        void GetInputStreamInfo(
            int dwInputStreamID,
            out MFTInputStreamInfo pStreamInfo
            );

        void GetOutputStreamInfo(
            int dwOutputStreamID,
            out MFTOutputStreamInfo pStreamInfo
            );

        void GetAttributes(
            [MarshalAs(UnmanagedType.Interface)] out IMFAttributes pAttributes
            );

        void GetInputStreamAttributes(
            int dwInputStreamID,
            [MarshalAs(UnmanagedType.Interface)] out IMFAttributes pAttributes
            );

        void GetOutputStreamAttributes(
            int dwOutputStreamID,
            [MarshalAs(UnmanagedType.Interface)] out IMFAttributes pAttributes
            );

        void DeleteInputStream(
            int dwStreamID
            );

        void AddInputStreams(
            int cStreams,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] int[] adwStreamIDs
            );

        void GetInputAvailableType(
            int dwInputStreamID,
            int dwTypeIndex,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaType ppType
            );

        void GetOutputAvailableType(
            int dwOutputStreamID,
            int dwTypeIndex,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaType ppType
            );

        void SetInputType(
            int dwInputStreamID,
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaType pType,
            MFTSetTypeFlags dwFlags
            );

        void SetOutputType(
            int dwOutputStreamID,
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaType pType,
            MFTSetTypeFlags dwFlags
            );

        void GetInputCurrentType(
            int dwInputStreamID,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaType ppType
            );

        void GetOutputCurrentType(
            int dwOutputStreamID,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaType ppType
            );

        void GetInputStatus(
            int dwInputStreamID,
            out MFTInputStatusFlags pdwFlags
            );

        void GetOutputStatus(
            out MFTOutputStatusFlags pdwFlags
            );

        void SetOutputBounds(
            long hnsLowerBound,
            long hnsUpperBound
            );

        void ProcessEvent(
            int dwInputStreamID,
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaEvent pEvent
            );

        void ProcessMessage(
            MFTMessageType eMessage,
            IntPtr ulParam
            );

        void ProcessInput(
            int dwInputStreamID,
            [MarshalAs(UnmanagedType.Interface)] IMFSample pSample,
            int dwFlags // Must be zero
            );

        void ProcessOutput(
            MFTProcessOutputFlags dwFlags,
            int cOutputBufferCount,
            [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] MFTOutputDataBuffer [] pOutputSamples,
            out ProcessOutputStatus pdwStatus
            );
    }

    #endregion

}
