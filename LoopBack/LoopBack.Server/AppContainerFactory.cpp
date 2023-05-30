#include "pch.h"
#include "AppContainerFactory.h"

using namespace Microsoft::WRL;

IFACEMETHODIMP AppContainerFactory::CreateInstance(
    _In_opt_ IUnknown* outer,
    _In_ REFIID iid,
    _COM_Outptr_ void** result)
{
    *result = nullptr;

    if (outer)
    {
        return CLASS_E_NOAGGREGATION;
    }

    return AppContainer::AppContainer().as(iid, result);
}
