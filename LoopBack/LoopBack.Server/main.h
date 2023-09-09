#pragma once

#include "pch.h"
#include "LoopUtilFactory.h"
#include "AppContainerFactory.h"
#include "ServerManagerFactory.h"
#include "wil/cppwinrt_wrl.h"
#include "wrl/module.h"

using namespace wil;
using namespace winrt;
using namespace Microsoft::WRL;
using namespace Microsoft::WRL::Details;

// Holds the main open until COM tells us there are no more server connections
unique_event _comServerExitEvent;

// Routine Description:
// - Called back when COM says there is nothing left for our server to do and we can tear down.
static void _releaseNotifier() noexcept
{
    _comServerExitEvent.SetEvent();
}

// {50169480-3FB8-4A19-AAED-ED9170811A3A}
static GUID CLSID_LoopUtil =
{
    0x50169480, 0x3fb8, 0x4a19, { 0xaa, 0xed, 0xed, 0x91, 0x70, 0x81, 0x1a, 0x3a }
};

// {F45FCBCC-E727-411D-880B-3EF2DB8752B9}
static GUID CLSID_AppContainer =
{
    0xf45fcbcc, 0xe727, 0x411d, { 0x88, 0xb, 0x3e, 0xf2, 0xdb, 0x87, 0x52, 0xb9 }
};

// {06FDF320-A6F5-45B4-B553-ADF15C927F51}
static GUID CLSID_ServerManager =
{
    0x6fdf320, 0xa6f5, 0x45b4, { 0xb5, 0x53, 0xad, 0xf1, 0x5c, 0x92, 0x7f, 0x51 }
};

DWORD RegisterLoopUtil(DefaultModule<ModuleType::OutOfProc>& module);
DWORD RegisterAppContainer(DefaultModule<ModuleType::OutOfProc>& module);
DWORD RegisterServerManager(DefaultModule<ModuleType::OutOfProc>& module);
void UnregisterCOMObject(DefaultModule<ModuleType::OutOfProc>& module, DWORD registration);