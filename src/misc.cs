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

    public struct Blob
    {
        public int cbSize;
        public IntPtr pBlobData;
    }

    [StructLayout(LayoutKind.Explicit)]
    public class PropVariant
    {
        [FieldOffset(0)]
        private MFAttributeType type;
        [FieldOffset(2)]
        private short reserved1;
        [FieldOffset(4)]
        private short reserved2;
        [FieldOffset(6)]
        private short reserved3;
        [FieldOffset(8)]
        private int intValue;
        [FieldOffset(8)]
        private long longValue;
        [FieldOffset(8)]
        private double doubleValue;
        [FieldOffset(8)]
        private Blob blobValue;
        [FieldOffset(8)]
        private IntPtr ptr;

        public PropVariant()
        {
            type = MFAttributeType.None;
        }

        public PropVariant(string value)
        {
            type = MFAttributeType.String;
            ptr = Marshal.StringToCoTaskMemUni(value);
        }

        public PropVariant(int value)
        {
            type = MFAttributeType.Uint32;
            intValue = value;
        }

        public PropVariant(double value)
        {
            type = MFAttributeType.Double;
            doubleValue = value;
        }

        public PropVariant(long value)
        {
            type = MFAttributeType.Uint64;
            longValue = value;
        }

        public PropVariant(Guid value)
        {
            type = MFAttributeType.Guid;
            ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(value));
            Marshal.StructureToPtr(value, ptr, false);
        }

        public PropVariant(byte[] value)
        {
            type = MFAttributeType.Blob;

            blobValue.cbSize = value.Length;
            blobValue.pBlobData = Marshal.AllocCoTaskMem(value.Length);
            Marshal.Copy(value, 0, blobValue.pBlobData, value.Length);
        }

        public PropVariant(object value)
        {
            type = MFAttributeType.IUnknown;
            ptr = Marshal.GetIUnknownForObject(value);
        }

        ~PropVariant()
        {
            Clear();
        }

        public MFAttributeType GetAttribType()
        {
            return type;
        }

        public void Clear()
        {
            if (type == MFAttributeType.String || type == MFAttributeType.Guid)
            {
                Marshal.FreeCoTaskMem(ptr);
            }
            else if (type == MFAttributeType.IUnknown)
            {
                Marshal.Release(ptr);
            }
            else if (type == MFAttributeType.Blob)
            {
                Marshal.FreeCoTaskMem(blobValue.pBlobData);
                blobValue.pBlobData = IntPtr.Zero;
            }

            ptr = IntPtr.Zero;
            type = 0;
        }

        public string GetString()
        {
            if (type == MFAttributeType.String)
            {
                return Marshal.PtrToStringUni(ptr);
            }
            throw new ArgumentException("PropVariant contents not a string");
        }

        public int GetInt()
        {
            if (type == MFAttributeType.Uint32)
            {
                return intValue;
            }
            throw new ArgumentException("PropVariant contents not an int32");
        }

        public long GetLong()
        {
            if (type == MFAttributeType.Uint64)
            {
                return longValue;
            }
            throw new ArgumentException("PropVariant contents not an int64");
        }

        public double GetDouble()
        {
            if (type == MFAttributeType.Double)
            {
                return doubleValue;
            }
            throw new ArgumentException("PropVariant contents not a double");
        }

        public Guid GetGuid()
        {
            if (type == MFAttributeType.Guid)
            {
                return (Guid)Marshal.PtrToStructure(ptr, typeof(Guid));
            }
            throw new ArgumentException("PropVariant contents not a Guid");
        }

        public byte[] GetBlob()
        {
            if (type == MFAttributeType.Blob)
            {
                byte[] b = new byte[blobValue.cbSize];

                Marshal.Copy(blobValue.pBlobData, b, 0, blobValue.cbSize);

                return b;
            }
            throw new ArgumentException("PropVariant contents are not a Blob");
        }

        public object GetIUnknown()
        {
            if (type == MFAttributeType.IUnknown)
            {
                return Marshal.GetObjectForIUnknown(ptr);
            }
            throw new ArgumentException("PropVariant contents not an IUnknown");
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
    public class PropertyKey
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

    [ComVisible(true),
    Guid("00000000-0000-0000-C000-000000000046")]
    public interface IUnknown
    {
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
            [Out] PropVariant pv
            );

        void SetValue(
            [In, MarshalAs(UnmanagedType.LPStruct)] PropertyKey key,
            [In, MarshalAs(UnmanagedType.LPStruct)] PropVariant propvar
            );

        void Commit();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("0C733A30-2A1C-11CE-ADE5-00AA0044773D")]
    public interface ISequentialStream
    {
        void Read(
            IntPtr pv,
            [In] int cb,
            out int pcbRead
            );

        void Write(
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

        new void Read(
            IntPtr pv,
            [In] int cb,
            out int pcbRead
            );

        new void Write(
            [In] IntPtr pv,
            [In] int cb,
            IntPtr pcbWritten // out int
            );

        #endregion

        void Seek(
            [In] long dlibMove,
            [In] StreamSeek dwOrigin,
            IntPtr plibNewPosition // out int
            );

        void SetSize(
            [In] long libNewSize
            );

        void CopyTo(
            [In, MarshalAs(UnmanagedType.Interface)] IStream pstm,
            [In] long cb,
            IntPtr pcbRead, // out long
            IntPtr pcbWritten // out long
            );

        void Commit(
            [In] STGC grfCommitFlags
            );

        void Revert();

        void LockRegion(
            [In] long libOffset,
            [In] long cb,
            [In] LockType dwLockType
            );

        void UnlockRegion(
            [In] long libOffset,
            [In] long cb,
            [In] LockType dwLockType
            );

        void Stat(
            out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg,
            [In] StatFlag grfStatFlag
            );

        void Clone(
            [MarshalAs(UnmanagedType.Interface)] out IStream ppstm
            );
    }

    #endregion

}