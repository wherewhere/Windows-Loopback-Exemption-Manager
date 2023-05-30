#pragma once

#include "wrl.h"

using namespace winrt;
using namespace LoopBack::Metadata;

struct AppContainerFactory : Microsoft::WRL::ClassFactory<>
{
    IFACEMETHODIMP CreateInstance(
        _In_opt_ IUnknown* outer,
        _In_ REFIID iid,
        _COM_Outptr_ void** result) override;
};
