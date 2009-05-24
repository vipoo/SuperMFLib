/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released
as public domain.  It is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
*****************************************************************************/

#include "stdafx.h"

#include "hack.h"

///////////////////////////////////////////////////////////////////////////
//
// IClassFactory implementation
//

/**************************************************************************

CClassFactory::CClassFactory

**************************************************************************/

CClassFactory::CClassFactory()
{
    m_ObjRefCount = 1;
    InterlockedIncrement(&m_serverLocks);
}

/**************************************************************************

CClassFactory::~CClassFactory

**************************************************************************/

CClassFactory::~CClassFactory()
{
    InterlockedDecrement(&m_serverLocks);
}

/**************************************************************************

CClassFactory::QueryInterface

**************************************************************************/

STDMETHODIMP CClassFactory::QueryInterface(  REFIID riid,
                                           LPVOID FAR * ppReturn)
{
    HRESULT hr = S_OK;

    if (riid == IID_IUnknown)
    {
        *ppReturn = static_cast<IUnknown*>(this);
        AddRef();
    }
    else if (riid == IID_IClassFactory)
    {
        *ppReturn = static_cast<IClassFactory*>(this);
        AddRef();
    }
    else
    {
        *ppReturn = NULL;

        hr = E_NOINTERFACE;
    }

    return hr;
}

/**************************************************************************

CClassFactory::AddRef

**************************************************************************/

STDMETHODIMP_(DWORD) CClassFactory::AddRef()
{
    return InterlockedIncrement(&m_ObjRefCount);
}

/**************************************************************************

CClassFactory::Release

**************************************************************************/

STDMETHODIMP_(DWORD) CClassFactory::Release()
{
    DWORD dwRet = InterlockedDecrement(&m_ObjRefCount);

    if(dwRet == 0)
    {
        delete this;
    }

    return dwRet;
}

/**************************************************************************

CClassFactory::CreateInstance

**************************************************************************/

STDMETHODIMP CClassFactory::CreateInstance(  LPUNKNOWN pUnknown,
                                           REFIID riid,
                                           LPVOID FAR * ppObject)
{
    *ppObject = NULL;

    if(pUnknown != NULL)
    {
        return CLASS_E_NOAGGREGATION;
    }

    //add implementation specific code here

    HRESULT hr = Hack::CreateInstance(NULL, riid, ppObject);

    return hr;
}

/**************************************************************************

CClassFactory::LockServer

**************************************************************************/

STDMETHODIMP CClassFactory::LockServer(BOOL lock)
{
    if (lock)
    {
        LockServer();
    }
    else
    {
        UnlockServer();
    }
    return S_OK;
}

// Static methods to lock and unlock the the server.
void CClassFactory::LockServer()
{
    InterlockedIncrement(&m_serverLocks);
}

void CClassFactory::UnlockServer()
{
    InterlockedDecrement(&m_serverLocks);
}
