#pragma once

#include "ComClsids.h"
#include "ServerManager.g.h"

using namespace std;
using namespace winrt;
using namespace LoopBack::Metadata;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;

namespace winrt::LoopBack::Metadata::implementation
{
    struct __declspec(OUTOFPROC_COM_CLSID_ServerManager)ServerManager : ServerManagerT<ServerManager>
    {
        bool IsRunAsAdministrator();

        ServerManager() = default;

        IAsyncAction StopServerAsync();

        void RunAsAdministrator()
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
    };
}

namespace winrt::LoopBack::Metadata::factory_implementation
{
    struct ServerManager : ServerManagerT<ServerManager, implementation::ServerManager>
    {
    };
}
