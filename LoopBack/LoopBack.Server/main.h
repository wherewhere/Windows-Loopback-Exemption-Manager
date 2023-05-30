#pragma once

#include "pch.h"
#include "LoopUtilFactory.h"
#include "AppContainerFactory.h"
#include "wil/resource.h"
#include "wil/com.h"
#include "wrl/module.h"

using namespace winrt;

// Holds the main open until COM tells us there are no more server connections
wil::unique_event _comServerExitEvent;

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

DWORD RegisterLoopUtil(Microsoft::WRL::Details::DefaultModule<Microsoft::WRL::ModuleType::OutOfProc>& module);
DWORD RegisterAppContainer(Microsoft::WRL::Details::DefaultModule<Microsoft::WRL::ModuleType::OutOfProc>& module);