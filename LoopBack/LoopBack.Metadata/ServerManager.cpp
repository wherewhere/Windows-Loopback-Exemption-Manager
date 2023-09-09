#include "pch.h"
#include "ServerManager.h"
#include "ServerManager.g.cpp"

namespace winrt::LoopBack::Metadata::implementation
{
	IAsyncAction ServerManager::StopServerAsync()
	{
		co_await winrt::resume_after(std::chrono::milliseconds(50));
		ExitProcess(S_OK);
	}

	bool ServerManager::IsRunAsAdministrator()
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
}