#pragma once

using namespace winrt;
using namespace LoopBack::Metadata;

struct LoopUtilFactory : winrt::implements<LoopUtilFactory, winrt::Windows::Foundation::IActivationFactory, IClassFactory>
{
    static const CLSID& GetLoopUtilCLSID();

    // IActivationFactory
    winrt::Windows::Foundation::IInspectable ActivateInstance();

    // IClassFactory
    HRESULT STDMETHODCALLTYPE CreateInstance(::IUnknown* pUnkOuter, REFIID riid, void** ppvObject);
    HRESULT STDMETHODCALLTYPE LockServer(BOOL fLock);
};
