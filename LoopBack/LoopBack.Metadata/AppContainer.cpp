#include "pch.h"
#include "AppContainer.h"
#include "AppContainer.g.cpp"

namespace winrt::LoopBack::Metadata::implementation
{
    hstring AppContainer::ToString() const
    {
        return displayName;
    }
}
