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

// This entire file only exists to work around bugs in Media Foundation.  The core problem 
// is that there are some objects in MF that don't correctly support QueryInterface.  In c++ 
// this isn't a problem, since if you tell c++ that something is a pointer to an interface, 
// it just believes you.  In fact, that's one of the places where c++ gets its performance:
// it doesn't check anything.

// In .Net, it checks.  And the way it checks is that every time it receives an interfaces
// from unmanaged code, it does a couple of QI calls on it.  First it does a QI for IUnknown.
// Second it does a QI for the specific interface it is supposed to be (ie IMFMediaSink or
// whatever).

// Since c++ *doesn't* check, oftentimes the first people to even try to call QI on some of 
// MF's objects are c# programmers.  And, not surprisingly, sometimes the first time code is 
// run, it doesn't work correctly.

// The only way you can work around it is to change the definition of the method from 
// IMFMediaSink (or whatever interface MF is trying to pass you) to IntPtr.  Of course, 
// that limits what you can do with it.  You can't call methods on an IntPtr.

// Something to keep in mind is that while the work-around involves changing the interface,
// the problem isn't in the interface, it is in the object that implements the inteface.
// This means that while the inteface may experience problems on one object, it may work
// correctly on another object.  If you are unclear on the differences between an interface
// and an object, it's time to hit the books.

// In W7, MS has fixed a few of these issues that were reported in Vista.  The problem 
// is that even if they are fixed in W7, if your program also needs to run on Vista, you 
// still have to use the work-arounds.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;

using MediaFoundation.Misc;
using MediaFoundation.EVR;

namespace MediaFoundation.Alt
{
    #region Bugs in Vista and W7

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("FA993888-4383-415A-A930-DD472A8CF6F7")]
    public interface IMFGetServiceAlt
    {
        void GetService(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidService,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            out IntPtr ppvObject
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("FA993889-4383-415A-A930-DD472A8CF6F7"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFTopologyServiceLookupAlt
    {
        void LookupService(
            [In] MFServiceLookupType type,
            [In] int dwIndex,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidService,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.SysInt)] IntPtr[] ppvObjects,
            [In, Out] ref int pnObjects
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("FA99388A-4383-415A-A930-DD472A8CF6F7"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFTopologyServiceLookupClientAlt
    {
        void InitServicePointers(
            IntPtr pLookup
            );

        void ReleaseServicePointers();
    }

    #endregion

    #region Bugs in Vista that appear to be fixed in W7

    public class MFExternAlt
    {
        [DllImport("MFPlat.dll", ExactSpelling = true, PreserveSig = false), SuppressUnmanagedCodeSecurity]
        public static extern void MFCreateEventQueue(
            out IMFMediaEventQueueAlt ppMediaEventQueue
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("2CD0BD52-BCD5-4B89-B62C-EADC0C031E7D"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaEventGeneratorAlt
    {
        void GetEvent(
            [In] MFEventFlag dwFlags,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            );

        void BeginGetEvent(
            //[In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            IntPtr pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object o
            );

        void EndGetEvent(
            //IMFAsyncResult pResult,
            IntPtr pResult,
            out IMFMediaEvent ppEvent
            );

        void QueueEvent(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In] int hrStatus,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvValue
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("D182108F-4EC6-443F-AA42-A71106EC825F")]
    public interface IMFMediaStreamAlt : IMFMediaEventGeneratorAlt
    {
        #region IMFMediaEventGeneratorAlt methods

        new void GetEvent(
            [In] MFEventFlag dwFlags,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            );

        new void BeginGetEvent(
            //[In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            IntPtr p1,
            [In, MarshalAs(UnmanagedType.IUnknown)] object o
            );

        new void EndGetEvent(
            //IMFAsyncResult pResult,
            IntPtr pResult,
            out IMFMediaEvent ppEvent
            );

        new void QueueEvent(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In] int hrStatus,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvValue
            );

        #endregion

        void GetMediaSource(
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaSource ppMediaSource
            );

        void GetStreamDescriptor(
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamDescriptor ppStreamDescriptor
            );

        void RequestSample(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pToken
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("36F846FC-2256-48B6-B58E-E2B638316581"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaEventQueueAlt
    {
        void GetEvent(
            [In] MFEventFlag dwFlags,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            );

        void BeginGetEvent(
            IntPtr pCallBack,
            //[In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkState
            );

        void EndGetEvent(
            IntPtr p1,
            //[In, MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pResult,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            );

        void QueueEvent(
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaEvent pEvent
            );

        void QueueEventParamVar(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In, MarshalAs(UnmanagedType.Error)] int hrStatus,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvValue
            );

        void QueueEventParamUnk(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In, MarshalAs(UnmanagedType.Error)] int hrStatus,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnk
            );

        void Shutdown();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("279A808D-AEC7-40C8-9C6B-A6B492C78A66")]
    public interface IMFMediaSourceAlt : IMFMediaEventGeneratorAlt
    {
        #region IMFMediaEventGeneratorAlt methods

        new void GetEvent(
            [In] MFEventFlag dwFlags,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            );

        new void BeginGetEvent(
            //[In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            IntPtr pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object o
            );

        new void EndGetEvent(
            //IMFAsyncResult pResult,
            IntPtr pResult,
            out IMFMediaEvent ppEvent
            );

        new void QueueEvent(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In] int hrStatus,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvValue
            );

        #endregion

        void GetCharacteristics(
            out MFMediaSourceCharacteristics pdwCharacteristics
            );

        void CreatePresentationDescriptor(
            out IMFPresentationDescriptor ppPresentationDescriptor
            );

        void Start(
            [In, MarshalAs(UnmanagedType.Interface)] IMFPresentationDescriptor pPresentationDescriptor,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pguidTimeFormat,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvarStartPosition
            );

        void Stop();

        void Pause();

        void Shutdown();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("0A97B3CF-8E7C-4A3D-8F8C-0C843DC247FB"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFStreamSinkAlt : IMFMediaEventGeneratorAlt
    {
        #region IMFMediaEventGeneratorAlt methods

        new void GetEvent(
            [In] MFEventFlag dwFlags,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            );

        new void BeginGetEvent(
            //[In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            IntPtr pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object o);

        new void EndGetEvent(
            //IMFAsyncResult pResult,
            IntPtr pResult,
            out IMFMediaEvent ppEvent);

        new void QueueEvent(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In] int hrStatus,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvValue
            );

        #endregion

        void GetMediaSink(
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaSinkAlt ppMediaSink
            );

        void GetIdentifier(
            out int pdwIdentifier
            );

        void GetMediaTypeHandler(
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaTypeHandler ppHandler
            );

        void ProcessSample(
            [In, MarshalAs(UnmanagedType.Interface)] IMFSample pSample
            );

        void PlaceMarker(
            [In] MFStreamSinkMarkerType eMarkerType,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvarMarkerValue,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvarContextValue
            );

        void Flush();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("6EF2A660-47C0-4666-B13D-CBB717F2FA2C"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaSinkAlt
    {
        void GetCharacteristics(
            out MFMediaSinkCharacteristics pdwCharacteristics
            );

        void AddStreamSink(
            [In] int dwStreamSinkIdentifier,
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaType pMediaType,
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamSinkAlt ppStreamSink
            );

        void RemoveStreamSink(
            [In] int dwStreamSinkIdentifier
            );

        void GetStreamSinkCount(
            out int pcStreamSinkCount
            );

        void GetStreamSinkByIndex(
            [In] int dwIndex,
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamSinkAlt ppStreamSink
            );

        void GetStreamSinkById(
            [In] int dwStreamSinkIdentifier,
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamSinkAlt ppStreamSink
            );

        void SetPresentationClock(
            [In, MarshalAs(UnmanagedType.Interface)] IMFPresentationClock pPresentationClock
            );

        void GetPresentationClock(
            [MarshalAs(UnmanagedType.Interface)] out IMFPresentationClock ppPresentationClock
            );

        void Shutdown();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("EAECB74A-9A50-42CE-9541-6A7F57AA4AD7"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFFinalizableMediaSinkAlt : IMFMediaSinkAlt
    {
        #region IMFMediaSinkAlt methods

        new void GetCharacteristics(
            out MFMediaSinkCharacteristics pdwCharacteristics);

        new void AddStreamSink(
            [In] int dwStreamSinkIdentifier,
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaType pMediaType,
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamSinkAlt ppStreamSink
            );

        new void RemoveStreamSink(
            [In] int dwStreamSinkIdentifier
            );

        new void GetStreamSinkCount(
            out int pcStreamSinkCount
            );

        new void GetStreamSinkByIndex(
            [In] int dwIndex,
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamSinkAlt ppStreamSink
            );

        new void GetStreamSinkById(
            [In] int dwStreamSinkIdentifier,
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamSinkAlt ppStreamSink
            );

        new void SetPresentationClock(
            [In, MarshalAs(UnmanagedType.Interface)] IMFPresentationClock pPresentationClock
            );

        new void GetPresentationClock(
            [MarshalAs(UnmanagedType.Interface)] out IMFPresentationClock ppPresentationClock
            );

        new void Shutdown();

        #endregion

        void BeginFinalize(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkState
            );

        void EndFinalize(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pResult
            );
    }

    #endregion

}
