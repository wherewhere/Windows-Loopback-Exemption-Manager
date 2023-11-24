#include "pch.h"
#include "main.h"

#pragma comment(linker, "/subsystem:windows /entry:mainCRTStartup")

using namespace winrt;
using namespace std::chrono;

int main()
{
    init_apartment();

    // Enable fast rundown of objects so that the server exits faster when clients go away.
    {
        com_ptr<IGlobalOptions> globalOptions;
        check_hresult(CoCreateInstance(CLSID_GlobalOptions, nullptr, CLSCTX_INPROC, IID_PPV_ARGS(&globalOptions)));
        check_hresult(globalOptions->Set(COMGLB_RO_SETTINGS, COMGLB_FAST_RUNDOWN));
    }

    _comServerExitEvent = CreateEvent(NULL, FALSE, FALSE, NULL);
    DWORD token = RegisterServerManager();

    CheckComRefAsync();
    WaitForSingleObject(_comServerExitEvent, INFINITE);

    check_hresult(CoRevokeClassObject(token));
    uninit_apartment();
}

IAsyncAction CheckComRefAsync()
{
    co_await resume_after(seconds(10));
    CoAddRefServerProcess();
    if (CoReleaseServerProcess() == 0)
    {
        _releaseNotifier();
    }
}

DWORD RegisterServerManager()
{
    DWORD registration = 0;

    check_hresult(CoRegisterClassObject(
        Factory::GetCLSID(),
        make<Factory>().as<::IUnknown>().get(),
        CLSCTX_LOCAL_SERVER,
        REGCLS_MULTIPLEUSE,
        &registration));

    return registration;
}