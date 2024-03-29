#pragma once

#include "pch.h"
#include "Factory.h"

using namespace winrt::Windows::Foundation;

// Holds the main open until COM tells us there are no more server connections
inline HANDLE _comServerExitEvent;

// Routine Description:
// - Called back when COM says there is nothing left for our server to do and we can tear down.
inline void _releaseNotifier() noexcept
{
    SetEvent(_comServerExitEvent);
}

IAsyncAction CheckComRefAsync();
DWORD RegisterServerManager();