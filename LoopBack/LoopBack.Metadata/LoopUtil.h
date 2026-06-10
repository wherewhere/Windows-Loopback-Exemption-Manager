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
            if (firewallAPI)
            {
                FreeLibrary(firewallAPI);
                firewallAPI = nullptr;
            }
        }

        IVectorView<AppContainer> Apps()
        {
            if (apps.Size() == 0)
            {
                return GetAppContainers();
            }
            return apps.GetView();
        }

        IVectorView<AppContainer> GetAppContainers();
        const HRESULT SetLoopbackList(const IIterable<hstring>& list) const;
        const HRESULT SetLoopbackList(const IIterable<AppContainer>& list) const;
        const HRESULT AddLookback(const hstring& stringSid) const;
        const HRESULT AddLookback(const AppContainer& appContainer) const;
        const HRESULT AddLookbacks(const IIterable<hstring>& list) const;
        const HRESULT AddLookbacks(const IIterable<AppContainer>& list) const;
        const HRESULT RemoveLookback(const hstring& stringSid) const;
        const HRESULT RemoveLookback(const AppContainer& appContainer) const;
        const HRESULT RemoveLookbacks(const IIterable<hstring>& list) const;
        const HRESULT RemoveLookbacks(const IIterable<AppContainer>& list) const;
        void Close();

    private:
        const IVector<AppContainer> apps = single_threaded_vector<AppContainer>();
        IVector<hstring> appListConfig = nullptr;
        HINSTANCE firewallAPI = LoadLibrary(L"FirewallAPI.dll");

        const AppContainer CreateAppContainer(const INET_FIREWALL_APP_CONTAINER& PI_app, const bool loopUtil) const;
        const bool CheckLoopback(SID* intPtr) const;
        const IVector<hstring> GetBinaries(const INET_FIREWALL_AC_BINARIES& cap) const;
        const IVector<hstring> GetCapabilities(const INET_FIREWALL_AC_CAPABILITIES& cap) const;
        const IVector<hstring> PI_NetworkIsolationGetAppContainerConfig() const;
        const IVectorView<AppContainer> PI_NetworkIsolationEnumAppContainers(const IVector<AppContainer>& list) const;
        void PI_NetworkIsolationFreeAppContainers(const PINET_FIREWALL_APP_CONTAINER& point) const;
        const IVector<hstring> GetEnabledLoopList(const hstring& list, const bool isAdd = true) const;
        const IVector<hstring> GetEnabledLoopList(const IIterable<hstring>& list, const bool isAdd = true) const;
        const IVector<hstring> GetEnabledLoopList(const IIterable<AppContainer>& list, const bool isAdd = true) const;

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
