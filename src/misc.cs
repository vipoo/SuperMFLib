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
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace MediaFoundation.Misc
{
    #region Declarations

#if ALLOW_UNTESTED_INTERFACES

    [UnmanagedName("MPEG1VIDEOINFO"), StructLayout(LayoutKind.Sequential)]
    public struct MPEG1VideoInfo
    {
        public VideoInfoHeader hdr;
        public int dwStartTimeCode;
        public int cbSequenceHeader;
        public byte [] bSequenceHeader; // Needs marshaler
    }

#endif

    /// <summary>
    /// When you are done with an instance of this class,
    /// it should be released with FreeAMMediaType() to avoid leaking
    /// </summary>
    [UnmanagedName("AM_MEDIA_TYPE"), StructLayout(LayoutKind.Sequential)]
    public class AMMediaType
    {
        public Guid majorType;
        public Guid subType;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fixedSizeSamples;
        [MarshalAs(UnmanagedType.Bool)]
        public bool temporalCompression;
        public int sampleSize;
        public Guid formatType;
        public IntPtr unkPtr; // IUnknown Pointer
        public int formatSize;
        public IntPtr formatPtr; // Pointer to a buff determined by formatType
    }

    [UnmanagedName("VIDEOINFOHEADER"), StructLayout(LayoutKind.Sequential)]
    public class VideoInfoHeader
    {
        public RECT SrcRect;
        public RECT TargetRect;
        public int BitRate;
        public int BitErrorRate;
        public long AvgTimePerFrame;
        public BitmapInfoHeader BmiHeader;  // Custom marshaler?
    }

    [UnmanagedName("VIDEOINFOHEADER2"), StructLayout(LayoutKind.Sequential)]
    public class VideoInfoHeader2
    {
        public RECT SrcRect;
        public RECT TargetRect;
        public int BitRate;
        public int BitErrorRate;
        public long AvgTimePerFrame;
        public AMInterlace InterlaceFlags;
        public AMCopyProtect CopyProtectFlags;
        public int PictAspectRatioX;
        public int PictAspectRatioY;
        public AMControl ControlFlags;
        public int Reserved2;
        public BitmapInfoHeader BmiHeader;  // Custom marshaler?
    }

    [UnmanagedName("AMINTERLACE_*"), Flags]
    public enum AMInterlace
    {
        None = 0,
        IsInterlaced = 0x00000001,
        OneFieldPerSample = 0x00000002,
        Field1First = 0x00000004,
        Unused = 0x00000008,
        FieldPatternMask = 0x00000030,
        FieldPatField1Only = 0x00000000,
        FieldPatField2Only = 0x00000010,
        FieldPatBothRegular = 0x00000020,
        FieldPatBothIrregular = 0x00000030,
        DisplayModeMask = 0x000000c0,
        DisplayModeBobOnly = 0x00000000,
        DisplayModeWeaveOnly = 0x00000040,
        DisplayModeBobOrWeave = 0x00000080,
    }

    [UnmanagedName("AMCOPYPROTECT_*")]
    public enum AMCopyProtect
    {
        None = 0,
        RestrictDuplication = 0x00000001
    }

    [UnmanagedName("From AMCONTROL_*"), Flags]
    public enum AMControl
    {
        None = 0,
        Used = 0x00000001,
        PadTo4x3 = 0x00000002,
        PadTo16x9 = 0x00000004,
    }

    [Flags, UnmanagedName("SPEAKER_* defines")]
    public enum WaveMask
    {
        None = 0x0,
        FrontLeft = 0x1,
        FrontRight = 0x2,
        FrontCenter = 0x4,
        LowFrequency = 0x8,
        BackLeft = 0x10,
        BackRight = 0x20,
        FrontLeftOfCenter = 0x40,
        FrontRightOfCenter = 0x80,
        BackCenter = 0x100,
        SideLeft = 0x200,
        SideRight = 0x400,
        TopCenter = 0x800,
        TopFrontLeft = 0x1000,
        TopFrontCenter = 0x2000,
        TopFrontRight = 0x4000,
        TopBackLeft = 0x8000,
        TopBackCenter = 0x10000,
        TopBackRight = 0x20000
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("BLOB")]
    public struct Blob
    {
        public int cbSize;
        public IntPtr pBlobData;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("CALPWSTR")]
    public struct CALPWstr
    {
        public int cElems;
        public IntPtr pElems;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4), UnmanagedName("PROPERTYKEY")]
    public class PropertyKey
    {
        public Guid fmtid;
        public int pID;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4), UnmanagedName("SIZE")]
    public class SIZE
    {
        public int cx;
        public int cy;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("RECT")]
    public class RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    #endregion

    #region Generic Interfaces

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("71604b0f-97b0-4764-8577-2f13e98a1422")]
    public interface INamedPropertyStore
    {
        void GetNamedValue( 
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pValue
        );
        
        void SetNamedValue( 
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
            PropVariant propvar);
        
        void GetNameCount( 
            out int pdwCount);
        
        void GetNameAt( 
            int iProp,
            [MarshalAs(UnmanagedType.BStr)] out string pbstrName);
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
    public interface IPropertyStore
    {
        void GetCount(
            out int cProps
            );

        void GetAt(
            [In] int iProp,
            [Out] PropertyKey pkey
            );

        void GetValue(
            [In, MarshalAs(UnmanagedType.LPStruct)] PropertyKey key,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pv
            );

        void SetValue(
            [In, MarshalAs(UnmanagedType.LPStruct)] PropertyKey key,
            [In, MarshalAs(UnmanagedType.LPStruct)] PropVariant propvar
            );

        void Commit();
    }

    #endregion

}
