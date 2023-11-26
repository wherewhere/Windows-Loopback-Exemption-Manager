#pragma once

#include "LoopBackProjectionFactory.g.h"
#include "winrt/LoopBack.Metadata.h"

using namespace winrt::LoopBack::Metadata;

namespace winrt::LoopBack::Projection::implementation
{
    static ServerManager m_serverManager = nullptr;

    struct LoopBackProjectionFactory : LoopBackProjectionFactoryT<LoopBackProjectionFactory>
    {
        static ServerManager ServerManager();
    };
}

namespace winrt::LoopBack::Projection::factory_implementation
{
    struct LoopBackProjectionFactory : LoopBackProjectionFactoryT<LoopBackProjectionFactory, implementation::LoopBackProjectionFactory>
    {
    };
}
