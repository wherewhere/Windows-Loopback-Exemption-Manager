#include "pch.h"
#include "TaskbarList.h"
#include "TaskbarList.g.cpp"

namespace winrt::LoopBack::Metadata::implementation
{
    TaskbarList::TaskbarList()
    {
        m_taskbarList = create_instance<ITaskbarList4>(CLSID_TaskbarList, CLSCTX_INPROC_SERVER);
    }
}
