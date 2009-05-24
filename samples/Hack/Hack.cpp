/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released
as public domain.  It is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
*****************************************************************************/

#include "stdafx.h"
#include <initguid.h>
#include "hack.h"

//-----------------------------------------------------------------------------
// CreateInstance
//
// Static method to create an instance of the object.
// Used by the class factory.
//-----------------------------------------------------------------------------

volatile LONG Hack::s_iCount = 0;

HRESULT Hack::CreateInstance(IUnknown *pUnkOuter, REFIID iid, void **ppv)
{
    if (ppv == NULL)
    {
        return E_POINTER;
    }

    // This object does not support aggregation.
    if (pUnkOuter != NULL)
    {
        return CLASS_E_NOAGGREGATION;
    }

    HRESULT hr = S_OK;

    Hack *pObject = new Hack();

    if (pObject == NULL)
    {
        hr = E_OUTOFMEMORY;
    }
    else
    {
        hr = pObject->QueryInterface(iid, ppv);
    }

    return hr;
}

Hack::Hack()
{
    m_id = InterlockedIncrement(&s_iCount);
    m_pUnk = NULL;
    m_refCount = 0;

    CClassFactory::LockServer();
}

Hack::~Hack()
{
    if (m_pUnk != NULL)
    {
        ULONG i = m_pUnk->Release();
        i = i;
    }

    CClassFactory::UnlockServer();
}

void WriteString(int id, REFGUID g, int i, WCHAR *pTerm)
{
#if 0
    WCHAR buff[255];
    WCHAR buff2[20];
    OLECHAR szCLSID[39];

    StringFromGUID2(g, szCLSID, sizeof(szCLSID) / 2);
    _itow_s(id, buff, sizeof(buff2) / 2, 10);

    wcscat_s(buff, sizeof(buff) / 2, L" ");
    wcscat_s(buff, sizeof(buff) / 2, szCLSID);
    wcscat_s(buff, sizeof(buff) / 2, L" ");
    _itow_s(i, buff2, sizeof(buff2) / 2, 10);
    wcscat_s(buff, sizeof(buff) / 2, buff2);
    wcscat_s(buff, sizeof(buff) / 2, pTerm);

    OutputDebugStringW(buff);
#endif
}

HRESULT Hack::QueryInterface(REFIID riid, void ** ppv)
{
    if (ppv == NULL)
    {
        return E_POINTER;
    }

    HRESULT hr = S_OK;

    // First check our interfaces
    if (riid == __uuidof(IUnknown))
    {
        *ppv = static_cast<IUnknown*>( static_cast<IHack*>(this) );
        AddRef();
    }
    else if (riid == __uuidof(IHack))
    {
        *ppv = static_cast<IHack*>(this);
        AddRef();
    }
    else
    {
        // Do we have an embedded object?
        if (m_pUnk != NULL)
        {
            // Check the specified guid
            if (riid == m_Guid)
            {
                *ppv = m_pUnk;
                m_pUnk->AddRef();
            }
            else
            {
                // If all else fails, see if the embedded obj supports it
                hr = m_pUnk->QueryInterface(riid, ppv);
            }
        }
    }

    WriteString(m_id, riid, m_refCount, L" Q\n");

    return hr;
}

ULONG Hack::AddRef()
{
    WriteString(m_id, m_Guid, m_refCount + 1, L" +\n");

    return InterlockedIncrement(&m_refCount);
}

ULONG Hack::Release()
{
    WriteString(m_id, m_Guid, m_refCount - 1, L" -\n");

    assert(m_refCount > 0);
    ULONG uCount = InterlockedDecrement(&m_refCount);
    if (uCount == 0)
    {
        delete this;
    }
    return uCount;
}

HRESULT Hack::Set(IUnknown *pUnk, REFIID iid, BOOL bAddRef)
{
    HRESULT hr;

    if (pUnk != NULL)
    {
        m_pUnk = pUnk;

        if (bAddRef)
        {
            m_pUnk->AddRef();
        }

        m_Guid = iid;

        hr = S_OK;
    }
    else
    {
        hr = E_FAIL;
    }

    return hr;
}
