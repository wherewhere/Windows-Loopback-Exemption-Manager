#include "pch.h"
#include "ServerManager.h"
#include "ServerManager.g.cpp"

using namespace std::chrono;

namespace winrt::LoopBack::Metadata::implementation
{
    ServerManager::~ServerManager()
    {
        if (m_isDisposed) { return; }
        m_serverManagerDestructedEvent(*this, true);
        m_isDisposed = true;
    }

    bool ServerManager::IsRunAsAdministrator() const
    {
        SID_IDENTIFIER_AUTHORITY NtAuthority = SECURITY_NT_AUTHORITY;
        PSID AdministratorsGroup;
        BOOL result = AllocateAndInitializeSid(
            &NtAuthority,
            2,
            SECURITY_BUILTIN_DOMAIN_RID,
            DOMAIN_ALIAS_RID_ADMINS,
            0, 0, 0, 0, 0, 0,
            &AdministratorsGroup);

        if (result)
        {
            CheckTokenMembership(NULL, AdministratorsGroup, &result);
        }

        if (AdministratorsGroup)
        {
            FreeSid(AdministratorsGroup);
        }

        return result;
    }

    event_token ServerManager::ServerManagerDestructed(EventHandler<bool> const& handler)
    {
        return m_serverManagerDestructedEvent.add(handler);
    }

    void ServerManager::ServerManagerDestructed(winrt::event_token const& token)
    {
        m_serverManagerDestructedEvent.remove(token);
    }

    LoopUtil ServerManager::GetLoopUtil() const
    {
        return LoopUtil::LoopUtil();
    }

    void ServerManager::RunAsAdministrator() const
    {
        if (!IsRunAsAdministrator())
        {
            WCHAR szPath[MAX_PATH];
            if (GetModuleFileName(NULL, szPath, ARRAYSIZE(szPath)))
            {
                SHELLEXECUTEINFO sei = { sizeof(sei) };

                sei.lpVerb = L"runas";
                sei.lpFile = szPath;
                sei.hwnd = NULL;
                sei.nShow = SW_SHOWDEFAULT;

                if (ShellExecuteEx(&sei))
                {
                    ExitProcess(S_OK);
                }
            }
        }
    }

    IAsyncAction ServerManager::StopServerAsync() const
    {
        co_await resume_after(milliseconds(50));
        ExitProcess(S_OK);
    }

    IAsyncOperation<LoopBack::Metadata::ServerManager> ServerManager::GetAdminServerManagerAsync()
    {
        try
        {
            if (m_adminServerManager && m_adminServerManager.IsServerRunning())
            {
                co_return m_adminServerManager;
            }
        }
        catch (...)
        {
            m_adminServerManager = nullptr;
        }

        static const CLSID CLSID_ServerManager = { 0xf745ac80, 0xd07e, 0x4f0f, { 0xb5, 0x41, 0xbd, 0xa6, 0x19, 0x7, 0xf2, 0x34 } }; // F745AC80-D07E-4F0F-B541-BDA61907F234
        m_adminServerManager = try_create_instance<LoopBack::Metadata::ServerManager>(CLSID_ServerManager, CLSCTX_ALL);
        if (m_adminServerManager)
        {
            co_return m_adminServerManager;
        }
        else
        {
            WCHAR szPath[MAX_PATH];
            if (GetModuleFileName(NULL, szPath, ARRAYSIZE(szPath)))
            {
                SHELLEXECUTEINFO sei = { sizeof(sei) };

                sei.lpVerb = L"runas";
                sei.lpFile = szPath;
                sei.hwnd = NULL;
                sei.nShow = SW_SHOWDEFAULT;

                if (ShellExecuteEx(&sei))
                {
                    co_await resume_after(milliseconds(50));
                    m_adminServerManager = create_instance<LoopBack::Metadata::ServerManager>(CLSID_ServerManager, CLSCTX_ALL);
                    co_return m_adminServerManager;
                }
            }
        }
    }

    const void ServerManager::Close()
    {
        if (m_isDisposed) { return; }
        m_serverManagerDestructedEvent(*this, true);
        m_isDisposed = true;
    }
}