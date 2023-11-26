#pragma once

#include "LoopUtil.g.h"

using namespace winrt;
using namespace LoopBack::Metadata;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;

namespace winrt::LoopBack::Metadata::implementation
{
    struct LoopUtil : LoopUtilT<LoopUtil>
    {
        LoopUtil() = default;
        ~LoopUtil()
        {
            if (firewallAPI != nullptr)
            {
                FreeLibrary(firewallAPI);
                firewallAPI = nullptr;
            }
        }

        IVector<AppContainer> Apps()
        {
            if (apps.Size() == 0)
            {
                return GetAppContainers();
            }
            return apps;
        }

        IVector<AppContainer> GetAppContainers();
        HRESULT SetLoopbackList(IIterable<hstring> list) const;
        HRESULT SetLoopbackList(IIterable<AppContainer> list) const;
        HRESULT AddLookback(hstring stringSid) const;
        HRESULT AddLookback(AppContainer appContainer) const;
        HRESULT AddLookbacks(IIterable<hstring> list) const;
        HRESULT AddLookbacks(IIterable<AppContainer> list) const;
        HRESULT RemoveLookback(hstring stringSid) const;
        HRESULT RemoveLookback(AppContainer appContainer) const;
        HRESULT RemoveLookbacks(IIterable<hstring> list) const;
        HRESULT RemoveLookbacks(IIterable<AppContainer> list) const;
        const void Close();

    private:
        const IVector<AppContainer> apps = single_threaded_vector<AppContainer>();
        IVector<hstring> appListConfig = nullptr;
        HINSTANCE firewallAPI = LoadLibrary(L"FirewallAPI.dll");

        const AppContainer CreateAppContainer(INET_FIREWALL_APP_CONTAINER PI_app, bool loopUtil) const;
        const bool CheckLoopback(SID* intPtr) const;
        const IVector<hstring> GetBinaries(INET_FIREWALL_AC_BINARIES cap) const;
        const IVector<hstring> GetCapabilities(INET_FIREWALL_AC_CAPABILITIES cap) const;
        const IVector<hstring> PI_NetworkIsolationGetAppContainerConfig() const;
        const IVector<AppContainer> PI_NetworkIsolationEnumAppContainers(IVector<AppContainer> list) const;
        const void PI_NetworkIsolationFreeAppContainers(PINET_FIREWALL_APP_CONTAINER point) const;
        const IVector<hstring> GetEnabledLoopList(hstring list, bool isAdd = true) const;
        const IVector<hstring> GetEnabledLoopList(IIterable<hstring> list, bool isAdd = true) const;
        const IVector<hstring> GetEnabledLoopList(IIterable<AppContainer> list, bool isAdd = true) const;

        const decltype(&NetworkIsolationGetAppContainerConfig) NetworkIsolationGetAppContainerConfig = GetNetworkIsolationGetAppContainerConfig();
        const decltype(&NetworkIsolationSetAppContainerConfig) NetworkIsolationSetAppContainerConfig = GetNetworkIsolationSetAppContainerConfig();
        const decltype(&NetworkIsolationEnumAppContainers) NetworkIsolationEnumAppContainers = GetNetworkIsolationEnumAppContainers();
        const decltype(&NetworkIsolationFreeAppContainers) NetworkIsolationFreeAppContainers = GetNetworkIsolationFreeAppContainers();

        const decltype(NetworkIsolationGetAppContainerConfig) GetNetworkIsolationGetAppContainerConfig() const
        {
            decltype(NetworkIsolationGetAppContainerConfig) func =
                (decltype(NetworkIsolationGetAppContainerConfig))GetProcAddress(
                    firewallAPI,
                    "NetworkIsolationGetAppContainerConfig");
            return func;
        }

        const decltype(NetworkIsolationSetAppContainerConfig) GetNetworkIsolationSetAppContainerConfig() const
        {
            decltype(NetworkIsolationSetAppContainerConfig) func =
                (decltype(NetworkIsolationSetAppContainerConfig))GetProcAddress(
                    firewallAPI,
                    "NetworkIsolationSetAppContainerConfig");
            return func;
        }

        const decltype(NetworkIsolationEnumAppContainers) GetNetworkIsolationEnumAppContainers() const
        {
            decltype(NetworkIsolationEnumAppContainers) func =
                (decltype(NetworkIsolationEnumAppContainers))GetProcAddress(
                    firewallAPI,
                    "NetworkIsolationEnumAppContainers");
            return func;
        }

        const decltype(NetworkIsolationFreeAppContainers) GetNetworkIsolationFreeAppContainers() const
        {
            decltype(NetworkIsolationFreeAppContainers) func =
                (decltype(NetworkIsolationFreeAppContainers))GetProcAddress(
                    firewallAPI,
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
