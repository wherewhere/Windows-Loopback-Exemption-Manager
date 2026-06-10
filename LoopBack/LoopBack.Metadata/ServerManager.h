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

        const bool IsServerRunning() const { return true; }
        const bool IsRunAsAdministrator() const;

        event_token ServerManagerDestructed(const EventHandler<bool>& handler);
        void ServerManagerDestructed(const winrt::event_token& token);

        LoopUtil GetLoopUtil() const;
        void RunAsAdministrator() const;
        IAsyncAction StopServerAsync() const;
        IAsyncOperation<LoopBack::Metadata::ServerManager> GetAdminServerManagerAsync();
        void Close();

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
