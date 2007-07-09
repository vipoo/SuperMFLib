/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Win32;

using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.Utils;

using Utils;

namespace WavSourceFilter
{
    [ComVisible(true), 
    Guid("B2C8B1AF-A0CC-4a47-9F4C-9764CF1CBF6E"),
    ClassInterface(ClassInterfaceType.None)]
    public class CWavByteStreamHandler : COMBase, IMFByteStreamHandler
    {
        xLog m_Log;

        public CWavByteStreamHandler()
        {
            m_Log = new xLog("CWavByteStreamHandler");
        }

        ~CWavByteStreamHandler()
        {
            m_Log.Dispose();
        }

        #region IMFByteStreamHandler methods

        public void BeginCreateObject(
            IMFByteStream pByteStream,
            string pwszURL,
            MFResolution dwFlags,
            IPropertyStore pProps,              // Can be NULL.
            out object ppIUnknownCancelCookie,  // Can be NULL.
            IMFAsyncCallback pCallback,
            object punkState                  // Can be NULL
        )
        {
            m_Log.WriteLine("BeginCreateObject");

            ppIUnknownCancelCookie = null; // We don't return a cancellation cookie.

            if ((pByteStream == null) || (pwszURL == null) || (pCallback == null))
            {
                throw new COMException("bad stream, url, or callback", E_InvalidArgument);
            }

            IMFAsyncResult pResult = null;

            WavSource pSource = new WavSource();

            pSource.Open(pByteStream);

            MFPlatDll.MFCreateAsyncResult(pSource as IMFMediaSource, pCallback, punkState, out pResult);

            MFPlatDll.MFInvokeCallback(pResult);

            if (pResult != null)
            {
                Marshal.ReleaseComObject(pResult);
            }
        }

        public void EndCreateObject(
            IMFAsyncResult pResult,
            out MFObjectType pObjectType,
            out object ppObject
        )
        {
            pObjectType = MFObjectType.Invalid;
            ppObject = null;

            m_Log.WriteLine("EndCreateObject");

            if (pResult == null)
            {
                throw new COMException("invalid IMFAsyncResult", E_InvalidArgument);
            }

            pResult.GetObject(out ppObject);

            // Minimal sanity check - is it really a media source?
            IMFMediaSource pSource = (IMFMediaSource)ppObject;

            pObjectType = MFObjectType.MediaSource;

            // unneeded SAFE_RELEASE(pSource);
            // unneeded SAFE_RELEASE(ppObject);
        }

        public void CancelObjectCreation(object pIUnknownCancelCookie)
        {
            m_Log.WriteLine("CancelObjectCreation");

            throw new COMException("Not implemented", E_NotImplemented);
        }

        public void GetMaxNumberOfBytesRequiredForResolution(out long pqwBytes)
        {
            m_Log.WriteLine("GetMaxNumberOfBytesRequiredForResolution");

            // In a canonical PCM .wav file, the start of the 'data' chunk is at byte offset 44.
            pqwBytes = 44;
        }

        #endregion

        #region COM Registration methods

        const string sByteStreamHandlerDescription = ".NET WAVE Source ByteStreamHandler";
        const string sWavFileExtension = ".wav";
        const string REGKEY_MF_BYTESTREAM_HANDLERS = "Software\\Microsoft\\Windows Media Foundation\\ByteStreamHandlers";

        [ComRegisterFunctionAttribute]
        static public void DllRegisterServer(Type t)
        {
            // Register the bytestream handler as a Media Foundation bytestream handler for the ".wav"
            // file extension.
            RegisterByteStreamHandler(
                typeof(CWavByteStreamHandler).GUID,
                sWavFileExtension,
                sByteStreamHandlerDescription
                );
        }

        [ComUnregisterFunctionAttribute]
        static public void DllUnregisterServer(Type t)
        {
            UnregisterByteStreamHandler(typeof(CWavByteStreamHandler).GUID, sWavFileExtension);
        }

        ///////////////////////////////////////////////////////////////////////
        // Name: RegisterByteStreamHandler
        // Desc: Register a bytestream handler for the Media Foundation
        //       source resolver.
        //
        // guid:            CLSID of the bytestream handler.
        // sFileExtension:  File extension.
        // sDescription:    Description.
        //
        // Note: sFileExtension can also be a MIME type although that is not
        //       illustrated in this sample.
        ///////////////////////////////////////////////////////////////////////

        static public int RegisterByteStreamHandler(Guid guid, string sFileExtension, string sDescription)
        {
            int hr = S_Ok;

            RegistryKey hKey;
            RegistryKey hSubKey;

            try
            {
                hKey = Registry.LocalMachine.CreateSubKey(REGKEY_MF_BYTESTREAM_HANDLERS);
            }
            catch { }

            hKey = Registry.LocalMachine.OpenSubKey(REGKEY_MF_BYTESTREAM_HANDLERS, true);

            hSubKey = hKey.CreateSubKey(sFileExtension);

            hSubKey.SetValue(guid.ToString("B"), sDescription);

            return hr;
        }

        static public int UnregisterByteStreamHandler(Guid guid, string sFileExtension)
        {
            string sKey = string.Format("{0}\\{1}", REGKEY_MF_BYTESTREAM_HANDLERS, sFileExtension);

            RegistryKey hKey = Registry.LocalMachine.OpenSubKey(sKey, true);
            hKey.DeleteValue(guid.ToString("B"));

            return S_Ok;
        }

        #endregion
    }
}
