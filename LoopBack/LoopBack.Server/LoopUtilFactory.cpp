#include "pch.h"
#include "LoopUtilFactory.h"

using namespace Microsoft::WRL;

IFACEMETHODIMP LoopUtilFactory::CreateInstance(
    _In_opt_ IUnknown* outer,
    _In_ REFIID iid,
    _COM_Outptr_ void** result)
{
    *result = nullptr;

    if (outer)
    {
        return CLASS_E_NOAGGREGATION;
    }

    return LoopUtil::LoopUtil().as(iid, result);
}
