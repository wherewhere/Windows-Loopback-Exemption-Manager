#pragma once

#include "LoopUtil.g.h"
#include "ComClsids.h"

using namespace std;
using namespace winrt;
using namespace LoopBack::Metadata;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;

namespace winrt::LoopBack::Metadata::implementation
{
    [uuid(OUTOFPROC_COM_CLSID_LoopUtil)]
    struct LoopUtil : LoopUtilT<LoopUtil>
    {
        IVector<hstring> _AppListConfig;

        LoopUtil() = default;

        IIterable<AppContainer> GetAppContainers();
        bool SetLoopbackList(IIterable<hstring> list);
        bool AddLookback(IIterable<hstring> list);
        bool RemoveLookback(IIterable<hstring> list);
        bool AddLookback(hstring stringSid);
        bool RemoveLookback(hstring stringSid);
        void Close();
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
