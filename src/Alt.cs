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

// This entire file only exists to work around a bug in Media Foundation.  The core problem is
// that there are a couple of interfaces that don't support QueryInterface.  AAR, .net won't marshal
// parameters that contain instances of those classes.  To work around this, it is necessary to
// change the definition to IntPtr for any method that touches those instances.
//
// Hopefully this will be fixed soon.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace MediaFoundation
{
    public class MFDllAlt
    {
        [DllImport("MFPlat.dll")]
        public static extern int MFCreateEventQueue(
            out IMFMediaEventQueueAlt ppMediaEventQueue
        );

    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("2CD0BD52-BCD5-4B89-B62C-EADC0C031E7D"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaEventGeneratorAlt
    {
        [PreserveSig]
        int GetEvent(
            [In] MFEventFlag dwFlags,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        int BeginGetEvent(
            //[In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            IntPtr pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object o
            //IntPtr o
            );

        [PreserveSig]
        int EndGetEvent(
            //IMFAsyncResult pResult,
            IntPtr pResult,
            out IMFMediaEvent ppEvent
            //IntPtr ppEvent
            );

        [PreserveSig]
        int QueueEvent(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In] int hrStatus,
            [In] object pvValue);
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("D182108F-4EC6-443F-AA42-A71106EC825F")]
    public interface IMFMediaStreamAlt : IMFMediaEventGeneratorAlt
    {
        #region IMFMediaEventGenerator methods

        [PreserveSig]
        new int GetEvent(
            [In] MFEventFlag dwFlags,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        new int BeginGetEvent(
            //[In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback, 
            IntPtr p1,
            [In, MarshalAs(UnmanagedType.IUnknown)] object o
            //IntPtr p2
            );

        [PreserveSig]
        new int EndGetEvent(
            //IMFAsyncResult pResult,
            IntPtr pResult,
            out IMFMediaEvent ppEvent
            //IntPtr ppEvent
            );

        [PreserveSig]
        new int QueueEvent(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In] int hrStatus,
            [In] object pvValue
            );

        #endregion

        [PreserveSig]
        int GetMediaSource(
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaSource ppMediaSource
            );

        [PreserveSig]
        int GetStreamDescriptor(
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamDescriptor ppStreamDescriptor
            );

        [PreserveSig]
        int RequestSample(
            [In] IUnknown pToken
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("00000000-0000-0000-C000-000000000046")]
    public interface IUnknown
    {
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("36F846FC-2256-48B6-B58E-E2B638316581"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaEventQueueAlt
    {
        [PreserveSig]
        int GetEvent(
            [In] MFEventFlag dwFlags,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        int BeginGetEvent(
            IntPtr pCallBack,
            //[In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkState
            );

        [PreserveSig]
        int EndGetEvent(
            //[In, MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pResult, 
            IntPtr p1,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            //out IntPtr p2
            );

        [PreserveSig]
        int QueueEvent([In, MarshalAs(UnmanagedType.Interface)] IMFMediaEvent pEvent);

        [PreserveSig]
        int QueueEventParamVar(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In, MarshalAs(UnmanagedType.Error)] int hrStatus,
            [In] IntPtr pvValue
            );

        [PreserveSig]
        int QueueEventParamUnk(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In, MarshalAs(UnmanagedType.Error)] int hrStatus,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnk
            );

        [PreserveSig]
        int Shutdown();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("279A808D-AEC7-40C8-9C6B-A6B492C78A66")]
    public interface IMFMediaSourceAlt : IMFMediaEventGeneratorAlt
    {
        #region IMFMediaEventGenerator methods

        [PreserveSig]
        new int GetEvent(
            [In] MFEventFlag dwFlags,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        new int BeginGetEvent(
            //[In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            IntPtr pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object o
            //IntPtr o
            );

        [PreserveSig]
        new int EndGetEvent(
            //IMFAsyncResult pResult,
            IntPtr pResult,
            out IMFMediaEvent ppEvent
            //IntPtr ppEvent
            );

        [PreserveSig]
        new int QueueEvent(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In] int hrStatus,
            [In] object pvValue
            );

        #endregion

        [PreserveSig]
        int GetCharacteristics(
            out MFMediaSourceCharacteristics pdwCharacteristics
            );

        [PreserveSig]
        int CreatePresentationDescriptor(
            out IMFPresentationDescriptor ppPresentationDescriptor
            );

        [PreserveSig]
        int Start(
            [In, MarshalAs(UnmanagedType.Interface)] IMFPresentationDescriptor pPresentationDescriptor,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pguidTimeFormat,
            [In] ref object pvarStartPosition
            );

        [PreserveSig]
        int Stop();

        [PreserveSig]
        int Pause();

        [PreserveSig]
        int Shutdown();
    }


}
