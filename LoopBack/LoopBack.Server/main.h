#pragma once

#include "pch.h"
#include "LoopUtilFactory.h"

// Holds the main open until COM tells us there are no more server connections
inline wil::slim_event_manual_reset _comServerExitEvent;

// Routine Description:
// - Called back when COM says there is nothing left for our server to do and we can tear down.
inline void _releaseNotifier() noexcept
{
    _comServerExitEvent.SetEvent();
}

DWORD RegisterLoopUtil();