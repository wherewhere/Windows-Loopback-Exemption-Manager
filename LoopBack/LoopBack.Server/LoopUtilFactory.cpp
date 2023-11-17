#include "pch.h"
#include "LoopUtilFactory.h"
#include "main.h"

const CLSID& LoopUtilFactory::GetLoopUtilCLSID()
{
    static const CLSID CLSID_LoopUtil = { 0x50169480, 0x3fb8, 0x4a19, { 0xaa, 0xed, 0xed, 0x91, 0x70, 0x81, 0x1a, 0x3a } };  // 50169480-3FB8-4A19-AAED-ED9170811A3A
    return CLSID_LoopUtil;
}

winrt::Windows::Foundation::IInspectable LoopUtilFactory::ActivateInstance()
{
    return LoopUtil::LoopUtil().as<winrt::Windows::Foundation::IInspectable>();
}

HRESULT STDMETHODCALLTYPE LoopUtilFactory::CreateInstance(::IUnknown* pUnkOuter, REFIID riid, void** ppvObject) try
{
    RETURN_HR_IF(E_POINTER, !ppvObject);
    *ppvObject = nullptr;
    RETURN_HR_IF(CLASS_E_NOAGGREGATION, pUnkOuter != nullptr);

    return LoopUtil::LoopUtil().as(riid, ppvObject);
}
catch (...)
{
    return to_hresult();
}

HRESULT STDMETHODCALLTYPE LoopUtilFactory::LockServer(BOOL fLock)
{
    if (fLock)
    {
        CoAddRefServerProcess();
    }
    else
    {
        if (CoReleaseServerProcess() == 0)
        {
            _releaseNotifier();
        }
    }
    return S_OK;
}
