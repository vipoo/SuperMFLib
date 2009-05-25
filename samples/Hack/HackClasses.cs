/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released
as public domain.  It is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
*****************************************************************************/

// This file has the c# declarations needed to call the Hack COM object

using System;
using System.Runtime.InteropServices;

namespace NSHack
{
    [ComImport, Guid("4B0B6227-8B08-4b45-8BA9-02944B25DDD9")]
    public class Hack
    {
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("9F7AF24D-C1F0-4b88-8444-AB695F4A29A2"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IHack
    {
        void Set(IntPtr lpInterface,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.Bool)] bool bAddRef
            );
    }
}
