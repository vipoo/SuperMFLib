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
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class)]
    public class UnmanagedNameAttribute : System.Attribute
    {
        private string m_Name;

        public UnmanagedNameAttribute(string s)
        {
            m_Name = s;
        }

        public override string ToString()
        {
            return m_Name;
        }
    }

    #region Declarations

    [Flags, UnmanagedName("STATFLAG")]
    public enum StatFlag
    {
        Default = 0,
        NoName = 1,
        NoOpen = 2
    }
    
    [Flags, UnmanagedName("STGC")]
    public enum STGC
    {
        Default = 0,
        Overwrite = 1,
        OnlyIfCurrent = 2,
        DangerouslyCommitMerelyToToDiskCache = 4,
        Consolidate = 8
    }

    [Flags, UnmanagedName("LOCKTYPE")]
    public enum LockType
    {
        None = 0,
        Write = 1,
        Exclusive = 2,
        OnlyOnce = 4
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1), UnmanagedName("WAVEFORMATEX")]
    public class WaveFormatEx
    {
        public short wFormatTag;
        public short nChannels;
        public int nSamplesPerSec;
        public int nAvgBytesPerSec;
        public short nBlockAlign;
        public short wBitsPerSample;
        public short cbSize;

        public bool IsEqual(WaveFormatEx b)
        {
            bool bRet = false;

            if (b == null)
            {
                bRet = false;
            }
            else
            {
                if (wFormatTag == b.wFormatTag &&
                    nChannels == b.nChannels &&
                    nSamplesPerSec == b.nSamplesPerSec &&
                    nAvgBytesPerSec == b.nAvgBytesPerSec &&
                    nBlockAlign == b.nBlockAlign &&
                    wBitsPerSample == b.wBitsPerSample &&
                    cbSize == b.cbSize)
                {
                    if (cbSize > 0)
                    {
                        Debug.Assert(false);
#if false
                        for (int x = 0; x < cbSize; x++)
                        {
                            if (Marshal.ReadByte(pExtraBytes, x) != Marshal.ReadByte(b.pExtraBytes, x))
                            {
                                bRet = false;
                                break;
                            }
                        }
#endif
                    }
                    else
                    {
                        bRet = true;
                    }
                }
            }

            return bRet;
        }

    }

    [StructLayout(LayoutKind.Sequential, Pack = 4), UnmanagedName("WAVEFORMATEX")]
    public struct BitmapInfoHeader
    {
        public int biSize;
        public int biWidth;
        public int biHeight;
        public short biPlanes;
        public short biBitCount;
        public int biCompression;
        public int biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public int biClrUsed;
        public int biClrImportant;
    }

    [UnmanagedName("STREAM_SEEK")]
    public enum StreamSeek
    {
        Set = 0,
        Cur = 1,
        End = 2
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4), UnmanagedName("PROPERTYKEY")]
    public struct PropertyKey
    {
        public Guid fmtid;
        public int pID;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4), UnmanagedName("SIZE")]
    public struct SIZE
    {
        public int cx;
        public int cy;
    }

    [UnmanagedName("RECT")]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public class FourCC
    {
        private int m_fourCC = 0;

        public FourCC(string fcc)
        {
            if (fcc.Length != 4)
            {
                throw new ArgumentException(fcc + " is not a valid FourCC");
            }

            byte[] asc = Encoding.ASCII.GetBytes(fcc);

            this.m_fourCC = asc[0];
            this.m_fourCC |= asc[1] << 8;
            this.m_fourCC |= asc[2] << 16;
            this.m_fourCC |= asc[3] << 24;
        }

        public FourCC(char a, char b, char c, char d)
            : this(new string(new char[] { a, b, c, d }))
        { }

        public FourCC(int fcc)
        {
            this.m_fourCC = fcc;
        }

        public int ToInt32()
        {
            return this.m_fourCC;
        }

        public Guid ToMediaSubtype()
        {
            return new Guid(this.m_fourCC.ToString("X") + "-0000-0010-8000-00AA00389B71");
        }

        public static bool operator ==(FourCC fcc1, FourCC fcc2)
        {
            return fcc1.m_fourCC == fcc2.m_fourCC;
        }

        public static bool operator !=(FourCC fcc1, FourCC fcc2)
        {
            return fcc1.m_fourCC != fcc2.m_fourCC;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FourCC))
                return false;

            return (obj as FourCC).m_fourCC == this.m_fourCC;
        }

        public override int GetHashCode()
        {
            return this.m_fourCC.GetHashCode();
        }

        public override string ToString()
        {
            char[] ca = new char[] { 
                Convert.ToChar((m_fourCC & 255)), 
                Convert.ToChar((m_fourCC >> 8) & 255), 
                Convert.ToChar((m_fourCC >> 16) & 255), 
                Convert.ToChar((m_fourCC >> 24) & 255)
            };

            string s = new string(ca);

            return s;
        }
    }

    #endregion

    #region Generic Interfaces

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
    public interface IPropertyStore
    {
        [PreserveSig]
        int GetCount(
            out int cProps
            );

        [PreserveSig]
        int GetAt(
            [In] int iProp,
            out PropertyKey pkey
            );

        [PreserveSig]
        int GetValue(
            [In, MarshalAs(UnmanagedType.LPStruct)] PropertyKey key,
            out object pv
            );

        [PreserveSig]
        int SetValue(
            [In, MarshalAs(UnmanagedType.LPStruct)] PropertyKey key,
            [In] ref object propvar
            );

        [PreserveSig]
        int Commit();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("0C733A30-2A1C-11CE-ADE5-00AA0044773D")]
    public interface ISequentialStream
    {
        [PreserveSig]
        int Read(
            IntPtr pv,
            [In] int cb,
            out int pcbRead
            );

        [PreserveSig]
        int Write(
            IntPtr pv,
            [In] int cb,
            IntPtr pcbWritten // out int
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("0000000C-0000-0000-C000-000000000046")]
    public interface IStream : ISequentialStream
    {
        #region ISequentialStream Methods

        [PreserveSig]
        new int Read(
            IntPtr pv,
            [In] int cb,
            out int pcbRead
            );

        [PreserveSig]
        new int Write(
            [In] IntPtr pv,
            [In] int cb,
            IntPtr pcbWritten // out int
            );

        #endregion

        [PreserveSig]
        int Seek(
            [In] long dlibMove,
            [In] StreamSeek dwOrigin,
            IntPtr plibNewPosition // out int
            );

        [PreserveSig]
        int SetSize(
            [In] long libNewSize
            );

        [PreserveSig]
        int CopyTo(
            [In, MarshalAs(UnmanagedType.Interface)] IStream pstm,
            [In] long cb,
            IntPtr pcbRead, // out long
            IntPtr pcbWritten // out long
            );

        [PreserveSig]
        int Commit(
            [In] STGC grfCommitFlags
            );

        [PreserveSig]
        int Revert();

        [PreserveSig]
        int LockRegion(
            [In] long libOffset,
            [In] long cb,
            [In] LockType dwLockType
            );

        [PreserveSig]
        int UnlockRegion(
            [In] long libOffset,
            [In] long cb,
            [In] LockType dwLockType
            );

        [PreserveSig]
        int Stat(
            out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg,
            [In] StatFlag grfStatFlag
            );

        [PreserveSig]
        int Clone(
            [MarshalAs(UnmanagedType.Interface)] out IStream ppstm
            );
    }

    #endregion

}