#pragma once

#include "ServerManager.g.h"

using namespace std;
using namespace winrt;
using namespace LoopBack::Metadata;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;

namespace winrt::LoopBack::Metadata::implementation
{
    struct ServerManager : ServerManagerT<ServerManager>
    {
        ServerManager() = default;
        ~ServerManager();

        const bool IsServerRunning() { return true; }
        const bool IsRunAsAdministrator();

        event_token ServerManagerDestructed(EventHandler<bool> const& handler);
        void ServerManagerDestructed(winrt::event_token const& token);

        LoopUtil GetLoopUtil();
        void RunAsAdministrator();
        IAsyncAction StopServerAsync();
        IAsyncOperation<LoopUtil> GetLoopUtilAdminAsync();

    private:
        event<EventHandler<bool>> m_serverManagerDestructedEvent;
    };
}

namespace winrt::LoopBack::Metadata::factory_implementation
{
    struct ServerManager : ServerManagerT<ServerManager, implementation::ServerManager>
    {
    };
}
