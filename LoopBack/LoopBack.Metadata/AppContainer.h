#pragma once

#include "AppContainer.g.h"

using namespace winrt;
using namespace Windows::Foundation::Collections;

namespace winrt::LoopBack::Metadata::implementation
{
    struct AppContainer : AppContainerT<AppContainer>
    {
        AppContainer() = default;

        bool IsEnableLoop() const { return isEnableLoop; }
        hstring DisplayName() const { return displayName; }
        hstring Description() const { return description; }
        hstring AppContainerName() const { return appContainerName; }
        hstring PackageFullName() const { return packageFullName; }
        hstring WorkingDirectory() const { return workingDirectory; }
        hstring AppContainerSid() const { return appContainerSid; }
        hstring UserSid() const { return userSid; }
        IVector<hstring> Capabilities() const { return capabilities; }
        IVector<hstring> Binaries() const { return binaries; }

        void IsEnableLoop(bool value) { isEnableLoop = value; }
        void DisplayName(hstring value) { displayName = value; }
        void Description(hstring value) { description = value; }
        void AppContainerName(hstring value) { appContainerName = value; }
        void PackageFullName(hstring value) { packageFullName = value; }
        void WorkingDirectory(hstring value) { workingDirectory = value; }
        void AppContainerSid(hstring value) { appContainerSid = value; }
        void UserSid(hstring value) { userSid = value; }
        void Capabilities(IVector<hstring> value) { capabilities = value; }
        void Binaries(IVector<hstring> value) { binaries = value; }

        hstring ToString() const;

    private:
        bool isEnableLoop = false;
        hstring displayName = L"";
        hstring description = L"";
        hstring appContainerName = L"";
        hstring packageFullName = L"";
        hstring workingDirectory = L"";
        hstring appContainerSid = L"";
        hstring userSid = L"";
        IVector<hstring> capabilities = nullptr;
        IVector<hstring> binaries = nullptr;
    };
}

namespace winrt::LoopBack::Metadata::factory_implementation
{
    struct AppContainer : AppContainerT<AppContainer, implementation::AppContainer>
    {
    };
}
