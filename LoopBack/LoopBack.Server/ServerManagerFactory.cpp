#include "pch.h"
#include "ServerManagerFactory.h"

using namespace Microsoft::WRL;

IFACEMETHODIMP ServerManagerFactory::CreateInstance(
    _In_opt_ IUnknown* outer,
    _In_ REFIID iid,
    _COM_Outptr_ void** result)
{
    *result = nullptr;

    if (outer)
    {
        return CLASS_E_NOAGGREGATION;
    }

    return ServerManager::ServerManager().as(iid, result);
}
