#pragma once

#include "pch.h"
#include "Factory.h"

using namespace winrt;

// {50169480-3FB8-4A19-AAED-ED9170811A3A}
static constexpr GUID CLSID_LoopUtil =
{
    0x50169480, 0x3fb8, 0x4a19, { 0xaa, 0xed, 0xed, 0x91, 0x70, 0x81, 0x1a, 0x3a }
};

// {F45FCBCC-E727-411D-880B-3EF2DB8752B9}
static constexpr GUID CLSID_AppContainer =
{
    0xf45fcbcc, 0xe727, 0x411d, { 0x88, 0xb, 0x3e, 0xf2, 0xdb, 0x87, 0x52, 0xb9 }
};

DWORD RegisterLoopUtil();
DWORD RegisterAppContainer();