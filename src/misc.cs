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
    #region Helper classes

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

    [StructLayout(LayoutKind.Explicit)]
    public class PropVariant : IDisposable
    {
        public enum VariantType
        {
            None = 0x0,
            Blob = 0x1011,
            Double = 0x5,
            Guid = 0x48,
            IUnknown = 13,
            String = 0x1f,
            Uint32 = 0x13,
            Uint64 = 0x15,
            StringArray = 0x1000 + 0x1f
        }

        #region Member variables

        [FieldOffset(0)]
        private VariantType type;

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

        #endregion

        public PropVariant()
        {
            type = VariantType.None;
        }

        public PropVariant(string value)
        {
            type = VariantType.String;
            ptr = Marshal.StringToCoTaskMemUni(value);
        }

        public PropVariant(string[] value)
        {
            type = VariantType.StringArray;
            ptr = Marshal.AllocCoTaskMem(4 + IntPtr.Size);
            Marshal.WriteInt32(ptr, value.Length);

            IntPtr ip = Marshal.AllocCoTaskMem(IntPtr.Size * value.Length);
            Marshal.WriteIntPtr(ptr, 4, ip);

            for (int x = 0; x < value.Length; x++)
            {
                Marshal.WriteIntPtr(ip, x * IntPtr.Size, Marshal.StringToCoTaskMemUni(value[x]));
            }
        }

        public PropVariant(int value)
        {
            type = VariantType.Uint32;
            intValue = value;
        }

        public PropVariant(double value)
        {
            type = VariantType.Double;
            doubleValue = value;
        }

        public PropVariant(long value)
        {
            type = VariantType.Uint64;
            longValue = value;
        }

        public PropVariant(Guid value)
        {
            type = VariantType.Guid;
            ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(value));
            Marshal.StructureToPtr(value, ptr, false);
        }

        public PropVariant(byte[] value)
        {
            type = VariantType.Blob;

            blobValue.cbSize = value.Length;
            blobValue.pBlobData = Marshal.AllocCoTaskMem(value.Length);
            Marshal.Copy(value, 0, blobValue.pBlobData, value.Length);
        }

        public PropVariant(object value)
        {
            type = VariantType.IUnknown;
            ptr = Marshal.GetIUnknownForObject(value);
        }

        public PropVariant(IntPtr value)
        {
            Marshal.PtrToStructure(value, this);
        }

        ~PropVariant()
        {
            Clear();
        }

        public static explicit operator string(PropVariant f)
        {
            return f.GetString();
        }

        public static explicit operator string[](PropVariant f)
        {
            return f.GetStringArray();
        }

        public static explicit operator int(PropVariant f)
        {
            return f.GetInt();
        }

        public static explicit operator double(PropVariant f)
        {
            return f.GetDouble();
        }

        public static explicit operator long(PropVariant f)
        {
            return f.GetLong();
        }

        public static explicit operator Guid(PropVariant f)
        {
            return f.GetGuid();
        }

        public static explicit operator byte[](PropVariant f)
        {
            return f.GetBlob();
        }

        // I decided not to do implicits since perf is likely to be
        // better recycling the PropVariant, and the only way I can
        // see to support Implicit is to create a new PropVariant.
        // Also, since I can't free the previous instance, IUnknowns
        // will linger until the GC cleans up.  Not what I think I
        // want.

        public MFAttributeType GetMFAttributeType()
        {
            if (type != VariantType.StringArray)
            {
                return (MFAttributeType)type;
            }
            throw new Exception("Type is not a MFAttributeType");
        }

        public VariantType GetAttributeType()
        {
            if (type != VariantType.StringArray)
            {
                return type;
            }
            throw new Exception("Type is not a MFAttributeType");
        }

        public void Clear()
        {
            if (type == VariantType.String || type == VariantType.Guid)
            {
                Marshal.FreeCoTaskMem(ptr);
            }
            else if (type == VariantType.IUnknown)
            {
                Marshal.Release(ptr);
            }
            else if (type == VariantType.Blob)
            {
                Marshal.FreeCoTaskMem(blobValue.pBlobData);
                blobValue.pBlobData = IntPtr.Zero;
            }
            else if (type == VariantType.StringArray)
            {
                int iCount = Marshal.ReadInt32(ptr);
                IntPtr ip = Marshal.ReadIntPtr(ptr, 4);

                for (int x = 0; x < iCount; x++)
                {
                    Marshal.FreeCoTaskMem(Marshal.ReadIntPtr(ip, x * IntPtr.Size));
                }
                Marshal.FreeCoTaskMem(ip);
                Marshal.FreeCoTaskMem(ptr);
            }

            ptr = IntPtr.Zero;
            type = 0;
        }

        public string[] GetStringArray()
        {
            if (type == VariantType.StringArray)
            {
                string[] sa;

                int iCount = Marshal.ReadInt32(ptr);
                sa = new string[iCount];

                IntPtr ip = Marshal.ReadIntPtr(ptr, 4);
                for (int x = 0; x < iCount; x++)
                {
                    sa[x] = Marshal.PtrToStringUni(Marshal.ReadIntPtr(ip, x * IntPtr.Size));
                }
            }
            throw new ArgumentException("PropVariant contents not a string");
        }

        public string GetString()
        {
            if (type == VariantType.String)
            {
                return Marshal.PtrToStringUni(ptr);
            }
            throw new ArgumentException("PropVariant contents not a string");
        }

        public int GetInt()
        {
            if (type == VariantType.Uint32)
            {
                return intValue;
            }
            throw new ArgumentException("PropVariant contents not an int32");
        }

        public long GetLong()
        {
            if (type == VariantType.Uint64)
            {
                return longValue;
            }
            throw new ArgumentException("PropVariant contents not an int64");
        }

        public double GetDouble()
        {
            if (type == VariantType.Double)
            {
                return doubleValue;
            }
            throw new ArgumentException("PropVariant contents not a double");
        }

        public Guid GetGuid()
        {
            if (type == VariantType.Guid)
            {
                return (Guid)Marshal.PtrToStructure(ptr, typeof(Guid));
            }
            throw new ArgumentException("PropVariant contents not a Guid");
        }

        public byte[] GetBlob()
        {
            if (type == VariantType.Blob)
            {
                byte[] b = new byte[blobValue.cbSize];

                Marshal.Copy(blobValue.pBlobData, b, 0, blobValue.cbSize);

                return b;
            }
            throw new ArgumentException("PropVariant contents are not a Blob");
        }

        public object GetIUnknown()
        {
            if (type == VariantType.IUnknown)
            {
                return Marshal.GetObjectForIUnknown(ptr);
            }
            throw new ArgumentException("PropVariant contents not an IUnknown");
        }

        #region IDisposable Members

        public void Dispose()
        {
            Clear();
            GC.SuppressFinalize(this);
        }

        #endregion
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

    #region Declarations

#if ALLOW_UNTESTED_INTERFACES

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

    [UnmanagedName("STREAM_SEEK")]
    public enum StreamSeek
    {
        Set = 0,
        Cur = 1,
        End = 2
    }

#endif

    [StructLayout(LayoutKind.Sequential), UnmanagedName("BLOB")]
    public struct Blob
    {
        public int cbSize;
        public IntPtr pBlobData;
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

    [StructLayout(LayoutKind.Sequential, Pack = 4), UnmanagedName("PROPERTYKEY")]
    public class PropertyKey
    {
        public Guid fmtid;
        public int pID;
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

#if ALLOW_UNTESTED_INTERFACES

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

#endif

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

    #endregion

}
