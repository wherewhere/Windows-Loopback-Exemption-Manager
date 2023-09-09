#pragma once

#include "AppContainer.g.h"
#include "ComClsids.h"

using namespace winrt;
using namespace Windows::Foundation::Collections;

namespace winrt::LoopBack::Metadata::implementation
{
    struct __declspec(uuid(OUTOFPROC_COM_CLSID_AppContainer)) AppContainer : AppContainerT<AppContainer>
    {
        bool IsEnableLoop() const { return isEnableLoop; }
        hstring DisplayName() { return displayName; }
        hstring AppContainerName() { return appContainerName; }
        hstring PackageFullName() { return packageFullName; }
        hstring WorkingDirectory() { return workingDirectory; }
        hstring AppContainerSid() { return appContainerSid; }
        hstring UserSid() { return userSid; }
        IIterable<hstring> Capabilities() { return capabilities; }
        IIterable<hstring> Binaries() { return binaries; }

        void IsEnableLoop(bool value) { isEnableLoop = value; }
        void DisplayName(hstring value) { displayName = value; }
        void AppContainerName(hstring value) { appContainerName = value; }
        void PackageFullName(hstring value) { packageFullName = value; }
        void WorkingDirectory(hstring value) { workingDirectory = value; }
        void AppContainerSid(hstring value) { appContainerSid = value; }
        void UserSid(hstring value) { userSid = value; }
        void Capabilities(IIterable<hstring> value) { capabilities = value; }
        void Binaries(IIterable<hstring> value) { binaries = value; }

        AppContainer() = default;

        hstring ToString();

    private:
        bool isEnableLoop = false;
        hstring displayName = L"";
        hstring appContainerName = L"";
        hstring packageFullName = L"";
        hstring workingDirectory = L"";
        hstring appContainerSid = L"";
        hstring userSid = L"";
        IIterable<hstring> capabilities = nullptr;
        IIterable<hstring> binaries = nullptr;
    };
}

namespace winrt::LoopBack::Metadata::factory_implementation
{
    struct AppContainer : AppContainerT<AppContainer, implementation::AppContainer>
    {
    };
}
