#pragma once

#include "AppContainer.g.h"

using namespace std;
using namespace winrt;
using namespace Windows::Foundation::Collections;

namespace winrt::LoopBack::Metadata::implementation
{
    struct AppContainer : AppContainerT<AppContainer>
    {
        bool LoopUtil() { return loopUtil; }
        hstring DisplayName() { return displayName; }
        hstring AppContainerName() { return appContainerName; }
        hstring WorkingDirectory() { return workingDirectory; }
        hstring StringSid() { return stringSid; }
        IIterable<hstring> Capabilities() { return capabilities; }

        void LoopUtil(bool value) { loopUtil = value; }
        void DisplayName(hstring value) { displayName = value; }
        void AppContainerName(hstring value) { appContainerName = value; }
        void WorkingDirectory(hstring value) { workingDirectory = value; }
        void StringSid(hstring value) { stringSid = value; }
        void Capabilities(IIterable<hstring> value) { capabilities = value; }

        AppContainer() = default;

    private:
        bool loopUtil = false;
        hstring displayName = L"";
        hstring appContainerName = L"";
        hstring workingDirectory = L"";
        hstring stringSid = L"";
        IIterable<hstring> capabilities = nullptr;

        vector<SID_AND_ATTRIBUTES> GetCapabilities(INET_FIREWALL_AC_CAPABILITIES cap);
    };
}

namespace winrt::LoopBack::Metadata::factory_implementation
{
    struct AppContainer : AppContainerT<AppContainer, implementation::AppContainer>
    {
    };
}
