/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released
as public domain.  It is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
*****************************************************************************/

#pragma once

// File generated from hack.idl
#include "hack_h.h"

extern volatile long m_serverLocks;

class CClassFactory : public IClassFactory
{
protected:
    LONG m_ObjRefCount;

public:
    CClassFactory();
    ~CClassFactory();

    //IUnknown methods
    STDMETHODIMP QueryInterface(REFIID, LPVOID FAR *);
    STDMETHODIMP_(DWORD) AddRef();
    STDMETHODIMP_(DWORD) Release();

    //IClassFactory methods
    STDMETHODIMP CreateInstance(LPUNKNOWN, REFIID, LPVOID FAR *);
    STDMETHODIMP LockServer(BOOL);

    static void LockServer();
    static void UnlockServer();
};


class Hack :
    public IHack
{

public:
    static HRESULT CreateInstance(IUnknown *pUnkOuter, REFIID iid, void **ppv);

    Hack();
    ~Hack();

    // IUnknown methods
    STDMETHOD(QueryInterface)(REFIID riid, void ** ppv);
    STDMETHOD_(ULONG, AddRef)();
    STDMETHOD_(ULONG, Release)();

    STDMETHOD(Set)(IUnknown *, REFIID, BOOL);

protected:
    static volatile LONG s_iCount;
    volatile long   m_refCount;
    IUnknown *m_pUnk;
    GUID m_Guid;
    int m_id;
};
