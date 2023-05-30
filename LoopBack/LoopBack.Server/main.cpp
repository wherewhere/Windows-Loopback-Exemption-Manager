#include "pch.h"
#include "main.h"

#pragma comment(linker, "/subsystem:windows /entry:mainCRTStartup" )

int main()
{
    init_apartment();

    RegisterLoopUtil();
    RegisterAppContainer();

    handle event{ CreateEvent(nullptr, false, false, nullptr) };
    WaitForSingleObject(event.get(), INFINITE);
}

DWORD RegisterLoopUtil()
{
	DWORD registration{};

    check_hresult(CoRegisterClassObject(
		CLSID_LoopUtil,
		make<LoopUtilFactory>().get(),
		CLSCTX_LOCAL_SERVER,
		REGCLS_SINGLEUSE,
		&registration));

	return registration;
}

DWORD RegisterAppContainer()
{
    DWORD registration{};

    check_hresult(CoRegisterClassObject(
        CLSID_AppContainer,
        make<AppContainerFactory>().get(),
        CLSCTX_LOCAL_SERVER,
        REGCLS_SINGLEUSE,
        &registration));

    return registration;
}
