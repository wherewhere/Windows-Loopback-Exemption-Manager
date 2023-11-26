#include "pch.h"
#include "LoopBackProjectionFactory.h"
#include "LoopBackProjectionFactory.g.cpp"

using namespace winrt;

namespace winrt::LoopBack::Projection::implementation
{
    static const CLSID CLSID_ServerManager = { 0x50169480, 0x3fb8, 0x4a19, { 0xaa, 0xed, 0xed, 0x91, 0x70, 0x81, 0x1a, 0x3a } }; // 50169480-3FB8-4A19-AAED-ED9170811A3A

    ServerManager LoopBackProjectionFactory::ServerManager()
    {
        try
        {
            if (m_serverManager != nullptr && m_serverManager.IsServerRunning())
            {
                return m_serverManager;
            }
        }
        catch (...) {}
        m_serverManager = try_create_instance<::ServerManager>(CLSID_ServerManager, CLSCTX_ALL);
        return m_serverManager;
    }
}
