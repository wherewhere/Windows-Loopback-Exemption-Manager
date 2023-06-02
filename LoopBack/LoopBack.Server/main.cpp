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

    ComPtr<LoopUtilFactory> loopUtilFactory;
    check_hresult(MakeAndInitialize<LoopUtilFactory>(&loopUtilFactory));

    ComPtr<IClassFactory> loopUtilFactoryAsClassFactory;
    check_hresult(loopUtilFactory.As<IClassFactory>(&loopUtilFactoryAsClassFactory));

    check_hresult(module.RegisterCOMObject(
		nullptr,
        &CLSID_LoopUtil,
        loopUtilFactoryAsClassFactory.GetAddressOf(),
		&registration,
		1));

	return registration;
}

DWORD RegisterAppContainer(DefaultModule<ModuleType::OutOfProc>& module)
{
    DWORD registration = 0;

    ComPtr<AppContainerFactory> appContainerFactory;
    check_hresult(MakeAndInitialize<AppContainerFactory>(&appContainerFactory));

    ComPtr<IClassFactory> appContainerFactoryAsClassFactory;
    check_hresult(appContainerFactory.As<IClassFactory>(&appContainerFactoryAsClassFactory));

    check_hresult(module.RegisterCOMObject(
        nullptr,
        &CLSID_AppContainer,
        appContainerFactoryAsClassFactory.GetAddressOf(),
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