/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released
as public domain.  It is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
*****************************************************************************/

#include "stdafx.h"
#include "hack.h"

HMODULE g_hModule;                  // DLL module handle

volatile LONG m_serverLocks = 0;

// Friendly name for COM registration.
WCHAR* g_sFriendlyName =  L"Hack";

BOOL APIENTRY DllMain( HANDLE hModule,
                                              DWORD  ul_reason_for_call,
                                              LPVOID lpReserved
                                              )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        g_hModule = (HMODULE)hModule;
        DisableThreadLibraryCalls(g_hModule);
        break;

    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
        break;

    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

STDAPI DllCanUnloadNow()
{
    if (m_serverLocks == 0)
    {
        return S_OK;
    }
    else
    {
        return S_FALSE;
    }
}


inline HRESULT SetKeyValue(HKEY hKey, const WCHAR *sName, const WCHAR *sValue)
{
    HRESULT hr;
    size_t cch = 0;

    cch = wcslen(sValue);

    // Size must include NULL terminator, which is not counted in StringCchLength
    DWORD  cbData = ((DWORD)cch + 1) * sizeof(WCHAR);

    // set description string
    LONG ret = RegSetValueEx(hKey, sName, 0, REG_SZ, (BYTE*)sValue, cbData);
    if (ret == ERROR_SUCCESS)
    {
        hr = S_OK;
    }
    else
    {
        hr = HRESULT_FROM_WIN32(ret);
    }

    return hr;
}

inline HRESULT CreateObjectKeyName(const GUID& guid, WCHAR *sName, DWORD cchMax)
{
    // convert CLSID uuid to string
    OLECHAR szCLSID[39];
    HRESULT hr = StringFromGUID2(guid, szCLSID, 39);
    if (FAILED(hr))
    {
        return hr;
    }

    // Create a string of the form "CLSID\{clsid}"
    wcscpy_s(sName, cchMax, L"CLSID\\");
    wcscat_s(sName, cchMax, szCLSID);

    return S_OK;
}

inline HRESULT RegisterObject(HMODULE hModule, const GUID& guid, const WCHAR *sDescription, const WCHAR *sThreadingModel)
{
    HKEY hKey = NULL;
    HKEY hSubkey = NULL;

    WCHAR achTemp[MAX_PATH];

    // Create the name of the key from the object's CLSID
    HRESULT hr = CreateObjectKeyName(guid, achTemp, MAX_PATH);

    // Create the new key.
    if (SUCCEEDED(hr))
    {
        LONG lreturn = RegCreateKeyEx(
            HKEY_CLASSES_ROOT,
            (LPCWSTR)achTemp,     // subkey
            0,                    // reserved
            NULL,                 // class string (can be NULL)
            REG_OPTION_NON_VOLATILE,
            KEY_ALL_ACCESS,
            NULL,                 // security attributes
            &hKey,
            NULL                  // receives the "disposition" (is it a new or existing key)
            );

        hr = __HRESULT_FROM_WIN32(lreturn);
    }

    // The default key value is a description of the object.
    if (SUCCEEDED(hr))
    {
        hr = SetKeyValue(hKey, NULL, sDescription);
    }

    // Create the "InprocServer32" subkey
    if (SUCCEEDED(hr))
    {
        const WCHAR *sServer = L"InprocServer32";

        LONG lreturn = RegCreateKeyEx(hKey, sServer, 0, NULL,
            REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, NULL, &hSubkey, NULL);

        hr = __HRESULT_FROM_WIN32(lreturn);
    }

    // The default value for this subkey is the path to the DLL.
    // Get the name of the module ...
    if (SUCCEEDED(hr))
    {
        DWORD res = GetModuleFileName(hModule, achTemp, MAX_PATH);
        if (res == 0)
        {
            hr = __HRESULT_FROM_WIN32(GetLastError());
        }
        if (res == MAX_PATH)
        {
            hr = E_FAIL; // buffer too small
        }
    }

    // ... and set the default key value.
    if (SUCCEEDED(hr))
    {
        hr = SetKeyValue(hSubkey, NULL, achTemp);
    }

    // Add a new value to the subkey, for "ThreadingModel" = <threading model>
    if (SUCCEEDED(hr))
    {
        hr = SetKeyValue(hSubkey, L"ThreadingModel", sThreadingModel);
    }

    // close hkeys

    if (hSubkey != NULL)
    {
        RegCloseKey( hSubkey );
    }

    if (hKey != NULL)
    {
        RegCloseKey( hKey );
    }

    return hr;
}

inline HRESULT UnregisterObject(const GUID& guid)
{
    WCHAR achTemp[MAX_PATH];

    HRESULT hr = CreateObjectKeyName(guid, achTemp, MAX_PATH);

    if (SUCCEEDED(hr))
    {
        // Delete the key recursively.
        DWORD res = RegDeleteTree(HKEY_CLASSES_ROOT, achTemp);

        if (res == ERROR_SUCCESS)
        {
            hr = S_OK;
        }
        else
        {
            hr = __HRESULT_FROM_WIN32(res);
        }
    }

    return hr;
}


STDAPI DllRegisterServer()
{
    HRESULT hr;

    // Register the MFT's CLSID as a COM object.
    hr = RegisterObject(g_hModule, CLSID_Hack, g_sFriendlyName, L"Both");

    return hr;
}

STDAPI DllUnregisterServer()
{
    // Unregister the CLSID
    UnregisterObject(CLSID_Hack);

    return S_OK;
}

STDAPI DllGetClassObject(REFCLSID clsid, REFIID riid, void** ppv)
{
    CClassFactory *pFactory = NULL;
    HRESULT hr = CLASS_E_CLASSNOTAVAILABLE; // Default to failure

    if (CLSID_Hack == clsid)
    {
        // Found an entry. Create a new class factory object.
        pFactory = new CClassFactory();

        hr = pFactory->QueryInterface(riid, ppv);
        if (SUCCEEDED(hr))
        {
            pFactory->Release();
        }
    }

    return hr;
}


