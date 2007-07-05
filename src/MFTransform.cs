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

using MediaFoundation.Misc;

namespace MediaFoundation.Transform
{
    #region Declarations

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
            out int pdwInputMinimum,
            out int pdwInputMaximum,
            out int pdwOutputMinimum,
            out int pdwOutputMaximum
            );

        void GetStreamCount(
            out int pcInputStreams,
            out int pcOutputStreams
            );

        void GetStreamIDs(
            int dwInputIDArraySize,
            out int pdwInputIDs,
            int dwOutputIDArraySize,
            out int pdwOutputIDs
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
            [In] ref int adwStreamIDs
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
            int ulParam
            );

        void ProcessInput(
            int dwInputStreamID,
            [MarshalAs(UnmanagedType.Interface)] IMFSample pSample,
            int dwFlags // Must be zero
            );

        void ProcessOutput(
            MFTProcessOutputFlags dwFlags,
            int cOutputBufferCount,
            [In, Out] ref MFTOutputDataBuffer pOutputSamples,
            out ProcessOutputStatus pdwStatus
            );
    }

    #endregion

}
