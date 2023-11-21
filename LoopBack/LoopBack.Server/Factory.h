#pragma once

using namespace winrt;
using namespace LoopBack::Metadata;

struct Factory : winrt::implements<Factory, winrt::Windows::Foundation::IActivationFactory, IClassFactory>
{
    static const CLSID& GetCLSID();

    // IActivationFactory
    winrt::Windows::Foundation::IInspectable ActivateInstance();

    // IClassFactory
    HRESULT STDMETHODCALLTYPE CreateInstance(::IUnknown* pUnkOuter, REFIID riid, void** ppvObject);
    HRESULT STDMETHODCALLTYPE LockServer(BOOL fLock);

private:
    static const bool IsRunAsAdministrator();
};
