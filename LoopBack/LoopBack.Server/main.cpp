#include "pch.h"
#include "main.h"

#pragma comment(linker, "/subsystem:windows /entry:mainCRTStartup" )

int main()
{
    init_apartment();

    // Enable fast rundown of objects so that the server exits faster when clients go away.
    {
        wil::com_ptr<IGlobalOptions> globalOptions;
        check_hresult(CoCreateInstance(CLSID_GlobalOptions, nullptr, CLSCTX_INPROC, IID_PPV_ARGS(&globalOptions)));
        check_hresult(globalOptions->Set(COMGLB_RO_SETTINGS, COMGLB_FAST_RUNDOWN));
    }

    _comServerExitEvent.create();
    auto& module = Module<ModuleType::OutOfProc>::Create(&_releaseNotifier);
    auto loopUtil = RegisterLoopUtil(module);
    auto appContainer = RegisterAppContainer(module);
    _comServerExitEvent.wait();
    UnregisterCOMObject(module, loopUtil);
    UnregisterCOMObject(module, appContainer);
}

DWORD RegisterLoopUtil(DefaultModule<ModuleType::OutOfProc>& module)
{
    DWORD registration = 0;

    ComPtr<IUnknown> factory;
    unsigned int flags = OutOfProc;

    check_hresult(CreateClassFactory<LoopUtilFactory>(
        &flags,
        nullptr,
        guid_of<IUnknown>(),
        &factory));

    ComPtr<IClassFactory> factoryAsClassFactory;
    check_hresult(factory.As<IClassFactory>(&factoryAsClassFactory));

    check_hresult(module.RegisterCOMObject(
        nullptr,
        &CLSID_LoopUtil,
        factoryAsClassFactory.GetAddressOf(),
        &registration,
        1));

    return registration;
}

DWORD RegisterAppContainer(DefaultModule<ModuleType::OutOfProc>& module)
{
    DWORD registration = 0;

    ComPtr<IUnknown> factory;
    unsigned int flags = OutOfProc;

    check_hresult(CreateClassFactory<AppContainerFactory>(
        &flags,
        nullptr,
        guid_of<IUnknown>(),
        &factory));

    ComPtr<IClassFactory> factoryAsClassFactory;
    check_hresult(factory.As<IClassFactory>(&factoryAsClassFactory));

    check_hresult(module.RegisterCOMObject(
        nullptr,
        &CLSID_AppContainer,
        factoryAsClassFactory.GetAddressOf(),
        &registration,
        1));

    return registration;
}

DWORD RegisterServerManager(DefaultModule<ModuleType::OutOfProc>& module)
{
    DWORD registration = 0;

    ComPtr<IUnknown> factory;
    unsigned int flags = OutOfProc;

    check_hresult(CreateClassFactory<ServerManagerFactory>(
        &flags,
        nullptr,
        guid_of<IUnknown>(),
        &factory));

    ComPtr<IClassFactory> factoryAsClassFactory;
    check_hresult(factory.As<IClassFactory>(&factoryAsClassFactory));

    check_hresult(module.RegisterCOMObject(
        nullptr,
        &CLSID_ServerManager,
        factoryAsClassFactory.GetAddressOf(),
        &registration,
        1));

    return registration;
}

void UnregisterCOMObject(DefaultModule<ModuleType::OutOfProc>& module, DWORD registration)
{
    check_hresult(module.UnregisterCOMObject(
        nullptr,
        &registration,
        1));
}