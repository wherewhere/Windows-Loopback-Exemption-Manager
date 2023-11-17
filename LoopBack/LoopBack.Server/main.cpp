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

    _comServerExitEvent.ResetEvent();
    auto loopUtil = RegisterLoopUtil();

    _comServerExitEvent.wait();
    CoRevokeClassObject(loopUtil);
}

DWORD RegisterLoopUtil()
{
    DWORD registration = 0;

    CoRegisterClassObject(
        LoopUtilFactory::GetLoopUtilCLSID(),
        winrt::make<LoopUtilFactory>().as<IUnknown>().get(),
        CLSCTX_LOCAL_SERVER,
        REGCLS_MULTIPLEUSE,
        &registration);

    return registration;
}