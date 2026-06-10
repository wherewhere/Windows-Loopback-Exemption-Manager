#pragma once

#include "AppContainer.g.h"

using namespace winrt;
using namespace Windows::Foundation::Collections;

namespace winrt::LoopBack::Metadata::implementation
{
    struct AppContainer : AppContainerT<AppContainer>
    {
        AppContainer() = default;

        const bool IsEnableLoop() const { return isEnableLoop; }
        hstring DisplayName() const { return displayName; }
        hstring Description() const { return description; }
        hstring AppContainerName() const { return appContainerName; }
        hstring PackageFullName() const { return packageFullName; }
        hstring WorkingDirectory() const { return workingDirectory; }
        hstring AppContainerSid() const { return appContainerSid; }
        hstring UserSid() const { return userSid; }
        IVector<hstring> Capabilities() const { return capabilities; }
        IVector<hstring> Binaries() const { return binaries; }

        void IsEnableLoop(const bool value) { isEnableLoop = value; }
        void DisplayName(const hstring& value) { displayName = value; }
        void Description(const hstring& value) { description = value; }
        void AppContainerName(const hstring& value) { appContainerName = value; }
        void PackageFullName(const hstring& value) { packageFullName = value; }
        void WorkingDirectory(const hstring& value) { workingDirectory = value; }
        void AppContainerSid(const hstring& value) { appContainerSid = value; }
        void UserSid(const hstring& value) { userSid = value; }
        void Capabilities(const IVector<hstring>& value) { capabilities = value; }
        void Binaries(const IVector<hstring>& value) { binaries = value; }

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
