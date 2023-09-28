#pragma once

#include "ComClsids.h"
#include "LoopUtil.g.h"

using namespace std;
using namespace winrt;
using namespace LoopBack::Metadata;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;

namespace winrt::LoopBack::Metadata::implementation
{
    struct __declspec(OUTOFPROC_COM_CLSID_LoopUtil) LoopUtil : LoopUtilT<LoopUtil>
    {
        IIterable<AppContainer> Apps()
        {
            if (apps.Size() == 0)
            {
                return GetAppContainers();
            }
            return apps;
        }

        ServerManager ServerManager() const
        {
            return serverManager;
        }

        LoopUtil() = default;

        IIterable<AppContainer> GetAppContainers();
        bool SetLoopbackList(IIterable<hstring> list);
        bool AddLookback(IIterable<hstring> list);
        bool RemoveLookback(IIterable<hstring> list);
        bool AddLookback(hstring stringSid);
        bool RemoveLookback(hstring stringSid);
        void Close();

        IAsyncAction StopServerAsync();

    private:
        IVector<AppContainer> apps = single_threaded_vector<AppContainer>();
        ::ServerManager serverManager = ServerManager::ServerManager();
        IVector<hstring> appListConfig = nullptr;
        HINSTANCE firewallAPI = nullptr;

        AppContainer CreateAppContainer(INET_FIREWALL_APP_CONTAINER PI_app, bool loopUtil);
        bool CheckLoopback(SID* intPtr);
        IVector<hstring> GetCapabilities(INET_FIREWALL_AC_CAPABILITIES cap);
        IVector<hstring> GetBinaries(INET_FIREWALL_AC_BINARIES cap);
        IVector<hstring> PI_NetworkIsolationGetAppContainerConfig();
        IVector<AppContainer> PI_NetworkIsolationEnumAppContainers(IVector<AppContainer> &list);
        void PI_NetworkIsolationFreeAppContainers(PINET_FIREWALL_APP_CONTAINER point);
        IVector<hstring> GetEnabledLoopList(hstring list, bool isAdd = true);
        IVector<hstring> GetEnabledLoopList(IIterable<hstring> list, bool isAdd = true);

        HINSTANCE GetFirewallAPI()
        {
            if (firewallAPI == nullptr)
            {
                firewallAPI = LoadLibrary(L"FirewallAPI.dll");
            }
            return firewallAPI;
        }

        decltype(NetworkIsolationGetAppContainerConfig)* GetNetworkIsolationGetAppContainerConfig()
        {
            HINSTANCE instance = GetFirewallAPI();
            decltype(NetworkIsolationGetAppContainerConfig)* func =
                (decltype(NetworkIsolationGetAppContainerConfig)*)GetProcAddress(
                    instance,
                    "NetworkIsolationGetAppContainerConfig");
            return func;
        }

        decltype(NetworkIsolationSetAppContainerConfig)* GetNetworkIsolationSetAppContainerConfig()
        {
            HINSTANCE instance = GetFirewallAPI();
            decltype(NetworkIsolationSetAppContainerConfig)* func =
                (decltype(NetworkIsolationSetAppContainerConfig)*)GetProcAddress(
                    instance,
                    "NetworkIsolationSetAppContainerConfig");
            return func;
        }

        decltype(NetworkIsolationEnumAppContainers)* GetNetworkIsolationEnumAppContainers()
        {
            HINSTANCE instance = GetFirewallAPI();
            decltype(NetworkIsolationEnumAppContainers)* func =
                (decltype(NetworkIsolationEnumAppContainers)*)GetProcAddress(
                    instance,
                    "NetworkIsolationEnumAppContainers");
            return func;
        }

        decltype(NetworkIsolationFreeAppContainers)* GetNetworkIsolationFreeAppContainers()
        {
            HINSTANCE instance = GetFirewallAPI();
            decltype(NetworkIsolationFreeAppContainers)* func =
                (decltype(NetworkIsolationFreeAppContainers)*)GetProcAddress(
                    instance,
                    "NetworkIsolationFreeAppContainers");
            return func;
        }
    };
}

namespace winrt::LoopBack::Metadata::factory_implementation
{
    struct LoopUtil : LoopUtilT<LoopUtil, implementation::LoopUtil>
    {
    };
}
