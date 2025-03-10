#pragma once

#include "ServerFactory.g.h"

using namespace winrt::Windows::Foundation;

namespace winrt::LoopBack::Metadata::implementation
{
    // Holds the main open until COM tells us there are no more server connections
    inline HANDLE _comServerExitEvent;

    // Routine Description:
    // - Called back when COM says there is nothing left for our server to do and we can tear down.
    inline void _releaseNotifier() noexcept
    {
        SetEvent(_comServerExitEvent);
    }

    struct ServerFactory : ServerFactoryT<ServerFactory>
    {
        static void StartServer();

    private:
        static IAsyncAction CheckComRefAsync();
        static DWORD RegisterServerManager();
    };

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
}

namespace winrt::LoopBack::Metadata::factory_implementation
{
    struct ServerFactory : ServerFactoryT<ServerFactory, implementation::ServerFactory>
    {
    };
}