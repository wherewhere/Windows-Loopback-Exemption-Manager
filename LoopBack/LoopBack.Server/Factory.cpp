#include "pch.h"
#include "Factory.h"
#include "main.h"

const CLSID& Factory::GetCLSID()
{
    static const CLSID CLSID_ServerManager = { 0x50169480, 0x3fb8, 0x4a19, { 0xaa, 0xed, 0xed, 0x91, 0x70, 0x81, 0x1a, 0x3a } }; // 50169480-3FB8-4A19-AAED-ED9170811A3A
    static const CLSID CLSID_ServerManager_Admin = { 0xf745ac80, 0xd07e, 0x4f0f, { 0xb5, 0x41, 0xbd, 0xa6, 0x19, 0x07, 0xf2, 0x34 } }; // F745AC80-D07E-4F0F-B541-BDA61907F234
    return IsRunAsAdministrator() ? CLSID_ServerManager_Admin : CLSID_ServerManager;
}

winrt::Windows::Foundation::IInspectable Factory::ActivateInstance()
{
    ServerManager result = ServerManager::ServerManager();
    if (!result) { return nullptr; }
    CoAddRefServerProcess();
    result.ServerManagerDestructed(
        [](winrt::Windows::Foundation::IInspectable, bool)
        {
            if (CoReleaseServerProcess() == 0)
            {
                _releaseNotifier();
            }
        });
    return result.as<IInspectable>();
}

HRESULT STDMETHODCALLTYPE Factory::CreateInstance(::IUnknown* pUnkOuter, REFIID riid, void** ppvObject) try
{
    if (!ppvObject) { return E_POINTER; }
    *ppvObject = nullptr;
    if (pUnkOuter != nullptr) { return CLASS_E_NOAGGREGATION; }
    ServerManager result = ServerManager::ServerManager();
    if (!result) { return S_FALSE; }
    CoAddRefServerProcess();
    result.ServerManagerDestructed(
        [](winrt::Windows::Foundation::IInspectable, bool)
        {
            if (CoReleaseServerProcess() == 0)
            {
                _releaseNotifier();
            }
        });
    return result.as(riid, ppvObject);
}
catch (...)
{
    return to_hresult();
}

HRESULT STDMETHODCALLTYPE Factory::LockServer(BOOL fLock) try
{
    if (fLock)
    {
        CoAddRefServerProcess();
    }
    else if (CoReleaseServerProcess() == 0)
    {
        _releaseNotifier();
    }
    return S_OK;
}
catch (...)
{
    return to_hresult();
}

const bool Factory::IsRunAsAdministrator()
{
    SID_IDENTIFIER_AUTHORITY NtAuthority = SECURITY_NT_AUTHORITY;
    PSID AdministratorsGroup;
    BOOL result = AllocateAndInitializeSid(
        &NtAuthority,
        2,
        SECURITY_BUILTIN_DOMAIN_RID,
        DOMAIN_ALIAS_RID_ADMINS,
        0, 0, 0, 0, 0, 0,
        &AdministratorsGroup);

    if (result)
    {
        CheckTokenMembership(NULL, AdministratorsGroup, &result);
    }

    if (AdministratorsGroup)
    {
        FreeSid(AdministratorsGroup);
    }

    return result;
}
