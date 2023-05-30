#pragma once

#include "LoopUtil.g.h"

using namespace std;
using namespace winrt;
using namespace LoopBack::Metadata;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;

namespace winrt::LoopBack::Metadata::implementation
{
    struct LoopUtil : LoopUtilT<LoopUtil>
    {
        PINET_FIREWALL_APP_CONTAINER _PACs = nullptr;
        vector<SID_AND_ATTRIBUTES> _AppListConfig;

        LoopUtil() = default;

        IIterable<AppContainer> GetAppContainers();
        bool SetLoopbackList(IIterable<hstring> list);
        bool AddLookback(IIterable<hstring> list);
        bool RemoveLookback(IIterable<hstring> list);
        bool AddLookback(hstring stringSid);
        bool RemoveLookback(hstring stringSid);
        void FreeResources();
        IAsyncAction StopService();

        IIterable<AppContainer> Apps()
        {
            if (apps.Size() == 0)
            {
                return GetAppContainers();
            }
            return apps;
        }

    private:
        IVector<AppContainer> apps = single_threaded_vector<AppContainer>();
        HINSTANCE FirewallAPI = nullptr;

        vector<SID_AND_ATTRIBUTES> GetCapabilities(INET_FIREWALL_AC_CAPABILITIES cap);
        AppContainer CreateAppContainer(INET_FIREWALL_APP_CONTAINER PI_app, bool loopUtil);
        bool CheckLoopback(SID* intPtr);
        vector<SID_AND_ATTRIBUTES> GetContainerSID(INET_FIREWALL_AC_CAPABILITIES cap);
        vector<SID_AND_ATTRIBUTES> PI_NetworkIsolationGetAppContainerConfig();
        vector<INET_FIREWALL_APP_CONTAINER> PI_NetworkIsolationEnumAppContainers();
        IVector<hstring> GetEnabledLoopList(hstring list, bool isAdd = true);
        IVector<hstring> GetEnabledLoopList(IIterable<hstring> list, bool isAdd = true);

        HINSTANCE GetFirewallAPI()
        {
            if (FirewallAPI == nullptr)
            {
                FirewallAPI = LoadLibrary(L"FirewallAPI.dll");
            }
            return FirewallAPI;
        }

        decltype(NetworkIsolationGetAppContainerConfig)* GetNetworkIsolationGetAppContainerConfig()
        {
            HINSTANCE FirewallAPI = LoadLibrary(L"FirewallAPI.dll");
            decltype(NetworkIsolationGetAppContainerConfig)* func =
                (decltype(NetworkIsolationGetAppContainerConfig)*)GetProcAddress(
                    FirewallAPI,
                    "NetworkIsolationGetAppContainerConfig");
            return func;
        }

        decltype(NetworkIsolationSetAppContainerConfig)* GetNetworkIsolationSetAppContainerConfig()
        {
            HINSTANCE FirewallAPI = LoadLibrary(L"FirewallAPI.dll");
            decltype(NetworkIsolationSetAppContainerConfig)* func =
                (decltype(NetworkIsolationSetAppContainerConfig)*)GetProcAddress(
                    FirewallAPI,
                    "NetworkIsolationSetAppContainerConfig");
            return func;
        }

        decltype(NetworkIsolationEnumAppContainers)* GetNetworkIsolationEnumAppContainers()
        {
            HINSTANCE FirewallAPI = LoadLibrary(L"FirewallAPI.dll");
            decltype(NetworkIsolationEnumAppContainers)* func =
                (decltype(NetworkIsolationEnumAppContainers)*)GetProcAddress(
                    FirewallAPI,
                    "NetworkIsolationEnumAppContainers");
            return func;
        }

        decltype(NetworkIsolationFreeAppContainers)* GetNetworkIsolationFreeAppContainers()
        {
            HINSTANCE FirewallAPI = LoadLibrary(L"FirewallAPI.dll");
            decltype(NetworkIsolationFreeAppContainers)* func =
                (decltype(NetworkIsolationFreeAppContainers)*)GetProcAddress(
                    FirewallAPI,
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
