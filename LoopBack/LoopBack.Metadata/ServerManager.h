#pragma once

#include "ServerManager.g.h"

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

        bool IsServerRunning() const { return true; }
        bool IsRunAsAdministrator() const;

        event_token ServerManagerDestructed(EventHandler<bool> const& handler);
        void ServerManagerDestructed(winrt::event_token const& token);

        LoopUtil GetLoopUtil() const;
        void RunAsAdministrator() const;
        IAsyncAction StopServerAsync() const;
        IAsyncOperation<LoopBack::Metadata::ServerManager> GetAdminServerManagerAsync();
        const void Close();

    private:
        event<EventHandler<bool>> m_serverManagerDestructedEvent;
        LoopBack::Metadata::ServerManager m_adminServerManager = nullptr;
        bool m_isDisposed = false;
    };
}

namespace winrt::LoopBack::Metadata::factory_implementation
{
    struct ServerManager : ServerManagerT<ServerManager, implementation::ServerManager>
    {
    };
}
