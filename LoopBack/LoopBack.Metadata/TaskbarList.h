#pragma once

#include "TaskbarList.g.h"

using namespace winrt;

namespace winrt::LoopBack::Metadata::implementation
{
    struct TaskbarList : TaskbarListT<TaskbarList, ::ITaskbarList, ITaskbarList2, ITaskbarList3, ITaskbarList4>
    {
        TaskbarList();

#pragma region ITaskbarList
        HRESULT STDMETHODCALLTYPE HrInit(void) override {
            return m_taskbarList->HrInit();
        }

        HRESULT STDMETHODCALLTYPE AddTab(
            /* [in] */ __RPC__in HWND hwnd) override {
            return m_taskbarList->AddTab(hwnd);
        }

        HRESULT STDMETHODCALLTYPE DeleteTab(
            /* [in] */ __RPC__in HWND hwnd) override {
            return m_taskbarList->DeleteTab(hwnd);
        }

        HRESULT STDMETHODCALLTYPE ActivateTab(
            /* [in] */ __RPC__in HWND hwnd) override {
            return m_taskbarList->ActivateTab(hwnd);
        }

        HRESULT STDMETHODCALLTYPE SetActiveAlt(
            /* [in] */ __RPC__in HWND hwnd) override {
            return m_taskbarList->SetActiveAlt(hwnd);
        }
#pragma endregion

#pragma region ITaskbarList2
        HRESULT STDMETHODCALLTYPE MarkFullscreenWindow(
            /* [in] */ __RPC__in HWND hwnd,
            /* [in] */ BOOL fFullscreen) override {
            return m_taskbarList->MarkFullscreenWindow(hwnd, fFullscreen);
        }
#pragma endregion

#pragma region ITaskbarList3
        HRESULT STDMETHODCALLTYPE SetProgressValue(
            /* [in] */ __RPC__in HWND hwnd,
            /* [in] */ ULONGLONG ullCompleted,
            /* [in] */ ULONGLONG ullTotal) override {
            return m_taskbarList->SetProgressValue(hwnd, ullCompleted, ullTotal);
        }

        HRESULT STDMETHODCALLTYPE SetProgressState(
            /* [in] */ __RPC__in HWND hwnd,
            /* [in] */ TBPFLAG tbpFlags) override {
            return m_taskbarList->SetProgressState(hwnd, tbpFlags);
        }

        HRESULT STDMETHODCALLTYPE RegisterTab(
            /* [in] */ __RPC__in HWND hwndTab,
            /* [in] */ __RPC__in HWND hwndMDI) override {
            return m_taskbarList->RegisterTab(hwndTab, hwndMDI);
        }

        HRESULT STDMETHODCALLTYPE UnregisterTab(
            /* [in] */ __RPC__in HWND hwndTab) override {
            return m_taskbarList->UnregisterTab(hwndTab);
        }

        HRESULT STDMETHODCALLTYPE SetTabOrder(
            /* [in] */ __RPC__in HWND hwndTab,
            /* [in] */ __RPC__in HWND hwndInsertBefore) override {
            return m_taskbarList->SetTabOrder(hwndTab, hwndInsertBefore);
        }

        HRESULT STDMETHODCALLTYPE SetTabActive(
            /* [in] */ __RPC__in HWND hwndTab,
            /* [in] */ __RPC__in HWND hwndMDI,
            /* [in] */ DWORD dwReserved) override {
            return m_taskbarList->SetTabActive(hwndTab, hwndMDI, dwReserved);
        }

        HRESULT STDMETHODCALLTYPE ThumbBarAddButtons(
            /* [in] */ __RPC__in HWND hwnd,
            /* [in] */ UINT cButtons,
            /* [size_is][in] */ __RPC__in_ecount_full(cButtons) LPTHUMBBUTTON pButton) override {
            return m_taskbarList->ThumbBarAddButtons(hwnd, cButtons, pButton);
        }

        HRESULT STDMETHODCALLTYPE ThumbBarUpdateButtons(
            /* [in] */ __RPC__in HWND hwnd,
            /* [in] */ UINT cButtons,
            /* [size_is][in] */ __RPC__in_ecount_full(cButtons) LPTHUMBBUTTON pButton) override {
            return m_taskbarList->ThumbBarUpdateButtons(hwnd, cButtons, pButton);
        }

        HRESULT STDMETHODCALLTYPE ThumbBarSetImageList(
            /* [in] */ __RPC__in HWND hwnd,
            /* [in] */ __RPC__in_opt HIMAGELIST himl) override {
            return m_taskbarList->ThumbBarSetImageList(hwnd, himl);
        }

        HRESULT STDMETHODCALLTYPE SetOverlayIcon(
            /* [in] */ __RPC__in HWND hwnd,
            /* [in] */ __RPC__in HICON hIcon,
            /* [string][unique][in] */ __RPC__in_opt_string LPCWSTR pszDescription) override {
            return m_taskbarList->SetOverlayIcon(hwnd, hIcon, pszDescription);
        }

        HRESULT STDMETHODCALLTYPE SetThumbnailTooltip(
            /* [in] */ __RPC__in HWND hwnd,
            /* [string][unique][in] */ __RPC__in_opt_string LPCWSTR pszTip) override {
            return m_taskbarList->SetThumbnailTooltip(hwnd, pszTip);
        }

        HRESULT STDMETHODCALLTYPE SetThumbnailClip(
            /* [in] */ __RPC__in HWND hwnd,
            /* [in] */ __RPC__in RECT* prcClip) override {
            return m_taskbarList->SetThumbnailClip(hwnd, prcClip);
        }
#pragma endregion

#pragma region ITaskbarList4
        HRESULT STDMETHODCALLTYPE SetTabProperties(
            /* [in] */ __RPC__in HWND hwndTab,
            /* [in] */ STPFLAG stpFlags) override {
            return m_taskbarList->SetTabProperties(hwndTab, stpFlags);
        }
#pragma endregion

    private:
        com_ptr<ITaskbarList4> m_taskbarList;
    };
}
namespace winrt::LoopBack::Metadata::factory_implementation
{
    struct TaskbarList : TaskbarListT<TaskbarList, implementation::TaskbarList>
    {
    };
}
