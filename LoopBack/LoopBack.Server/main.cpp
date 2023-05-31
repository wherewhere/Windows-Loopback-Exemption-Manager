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
    auto& module = Microsoft::WRL::Module<Microsoft::WRL::ModuleType::OutOfProc>::Create(&_releaseNotifier);
    auto loopUtil = RegisterLoopUtil(module);
    auto appContainer = RegisterAppContainer(module);
    _comServerExitEvent.wait();
    UnregisterCOMObject(module, loopUtil);
    UnregisterCOMObject(module, appContainer);
}

DWORD RegisterLoopUtil(Microsoft::WRL::Details::DefaultModule<Microsoft::WRL::ModuleType::OutOfProc>& module)
{
    DWORD registration = 0;

    Microsoft::WRL::ComPtr<LoopUtilFactory> loopUtilFactory;
    check_hresult(Microsoft::WRL::MakeAndInitialize<LoopUtilFactory>(&loopUtilFactory));

    Microsoft::WRL::ComPtr<IClassFactory> loopUtilFactoryAsClassFactory;
    check_hresult(loopUtilFactory.As<IClassFactory>(&loopUtilFactoryAsClassFactory));

    check_hresult(module.RegisterCOMObject(
		nullptr,
        &CLSID_LoopUtil,
        loopUtilFactoryAsClassFactory.GetAddressOf(),
		&registration,
		1));

	return registration;
}

DWORD RegisterAppContainer(Microsoft::WRL::Details::DefaultModule<Microsoft::WRL::ModuleType::OutOfProc>& module)
{
    DWORD registration = 0;

    Microsoft::WRL::ComPtr<AppContainerFactory> appContainerFactory;
    check_hresult(Microsoft::WRL::MakeAndInitialize<AppContainerFactory>(&appContainerFactory));

    Microsoft::WRL::ComPtr<IClassFactory> appContainerFactoryAsClassFactory;
    check_hresult(appContainerFactory.As<IClassFactory>(&appContainerFactoryAsClassFactory));

    check_hresult(module.RegisterCOMObject(
        nullptr,
        &CLSID_AppContainer,
        appContainerFactoryAsClassFactory.GetAddressOf(),
        &registration,
        1));

    return registration;
}

void UnregisterCOMObject(Microsoft::WRL::Details::DefaultModule<Microsoft::WRL::ModuleType::OutOfProc>& module, DWORD registration)
{
    check_hresult(module.UnregisterCOMObject(
        nullptr,
        &registration,
        1));
}