﻿#include "pch.h"
#include "LoopUtil.h"
#include "LoopUtil.g.cpp"

namespace winrt::LoopBack::Metadata::implementation
{
    IIterable<AppContainer> LoopUtil::GetAppContainers()
	{
        apps.Clear();
        //List of Apps that have LoopUtil enabled.
        _AppListConfig = PI_NetworkIsolationGetAppContainerConfig();
        //Full List of Apps
        return PI_NetworkIsolationEnumAppContainers(apps);
	}

    bool LoopUtil::SetLoopbackList(IIterable<hstring> list)
    {
        vector<SID_AND_ATTRIBUTES> arr;
        DWORD count = 0;

        for (hstring app : list)
        {
            SID_AND_ATTRIBUTES sid{};
            sid.Attributes = 0;
            //TO DO:
            PSID ptr = nullptr;
            ConvertStringSidToSid(app.c_str(), &ptr);
            sid.Sid = ptr;
            arr.push_back(sid);
            count++;
        }

        return GetNetworkIsolationSetAppContainerConfig()(count, arr.data()) == 0;
    }

    bool LoopUtil::AddLookback(IIterable<hstring> list)
    {
        IVector<hstring> enabledList = GetEnabledLoopList(list, true);
        vector<SID_AND_ATTRIBUTES> arr;
        DWORD count = 0;

        for (hstring app : enabledList)
        {
            SID_AND_ATTRIBUTES sid{};
            sid.Attributes = 0;
            //TO DO:
            PSID ptr = nullptr;
            ConvertStringSidToSid(app.c_str(), &ptr);
            sid.Sid = ptr;
            arr.push_back(sid);
            count++;
        }

        return GetNetworkIsolationSetAppContainerConfig()(count, arr.data()) == 0;
    }

    bool LoopUtil::RemoveLookback(IIterable<hstring> list)
    {
        IVector<hstring> enabledList = GetEnabledLoopList(list, false);
        vector<SID_AND_ATTRIBUTES> arr;
        DWORD count = 0;

        for (hstring app : enabledList)
        {
            SID_AND_ATTRIBUTES sid{};
            sid.Attributes = 0;
            //TO DO:
            PSID ptr = nullptr;
            ConvertStringSidToSid(app.c_str(), &ptr);
            sid.Sid = ptr;
            arr.push_back(sid);
            count++;
        }

        return GetNetworkIsolationSetAppContainerConfig()(count, arr.data()) == 0;
    }

    bool LoopUtil::AddLookback(hstring stringSid)
    {
        IVector<hstring> enabledList = GetEnabledLoopList(stringSid, true);
        vector<SID_AND_ATTRIBUTES> arr;
        DWORD count = 0;

        for (hstring app : enabledList)
        {
            SID_AND_ATTRIBUTES sid{};
            sid.Attributes = 0;
            //TO DO:
            PSID ptr = nullptr;
            ConvertStringSidToSid(app.c_str(), &ptr);
            sid.Sid = ptr;
            arr.push_back(sid);
            count++;
        }

        return GetNetworkIsolationSetAppContainerConfig()(count, arr.data()) == 0;
    }

    bool LoopUtil::RemoveLookback(hstring stringSid)
    {
        IVector<hstring> enabledList = GetEnabledLoopList(stringSid, false);
        vector<SID_AND_ATTRIBUTES> arr;
        DWORD count = 0;

        for (hstring app : enabledList)
        {
            SID_AND_ATTRIBUTES sid{};
            sid.Attributes = 0;
            //TO DO:
            PSID ptr = nullptr;
            ConvertStringSidToSid(app.c_str(), &ptr);
            sid.Sid = ptr;
            arr.push_back(sid);
            count++;
        }

        return GetNetworkIsolationSetAppContainerConfig()(count, arr.data()) == 0;
    }

    AppContainer LoopUtil::CreateAppContainer(INET_FIREWALL_APP_CONTAINER PI_app, bool loopUtil)
    {
        AppContainer app = AppContainer::AppContainer();

        app.IsEnableLoop(loopUtil);
        if (PI_app.displayName != nullptr) { app.DisplayName(PI_app.displayName); }
        if (PI_app.appContainerName != nullptr) { app.AppContainerName(PI_app.appContainerName); }
        if (PI_app.packageFullName != nullptr) { app.PackageFullName(PI_app.packageFullName); }
        if (PI_app.workingDirectory != nullptr) { app.WorkingDirectory(PI_app.workingDirectory); }

        if (PI_app.appContainerSid != nullptr)
        {
            LPWSTR tempSid;
            ConvertSidToStringSid(PI_app.appContainerSid, &tempSid);
            if (tempSid != nullptr) { app.AppContainerSid(tempSid); }
        }

        if (PI_app.userSid != nullptr)
        {
            LPWSTR tempSid;
            ConvertSidToStringSid(PI_app.userSid, &tempSid);
            if (tempSid != nullptr) { app.UserSid(tempSid); }
        }

        IVector<hstring> capabilities = single_threaded_vector<hstring>();
        if (PI_app.capabilities.count > 0)
        {
            vector<SID_AND_ATTRIBUTES> app_capabilities = GetCapabilities(PI_app.capabilities);
            for (SID_AND_ATTRIBUTES app_capability : app_capabilities)
            {
                if (app_capability.Sid != nullptr)
                {
                    LPWSTR mysid;
                    ConvertSidToStringSid(app_capability.Sid, &mysid);
                    if (mysid != nullptr) { capabilities.Append(mysid); }
                }
            }
        }
        app.Capabilities(capabilities);

        IVector<hstring> app_binaries = GetBinaries(PI_app.binaries);
        app.Binaries(app_binaries);

        return app;
    }

    IVector<hstring> LoopUtil::GetBinaries(INET_FIREWALL_AC_BINARIES cap)
    {
        IVector<hstring> myCap = single_threaded_vector<hstring>();

        LPWSTR* arrayValue = cap.binaries;

        for (DWORD i = 0; i < cap.count; i++)
        {
            LPWSTR cur = arrayValue[i];
            myCap.Append(cur);
        }

        return myCap;
    }

    vector<SID_AND_ATTRIBUTES> LoopUtil::GetCapabilities(INET_FIREWALL_AC_CAPABILITIES cap)
    {
        vector<SID_AND_ATTRIBUTES> myCap;

        SID_AND_ATTRIBUTES* arrayValue = cap.capabilities;

        for (DWORD i = 0; i < cap.count; i++)
        {
            SID_AND_ATTRIBUTES cur = arrayValue[i];
            myCap.push_back(cur);
        }

        return myCap;
    }

    bool LoopUtil::CheckLoopback(SID* intPtr)
    {
        if (intPtr != nullptr)
        {
            LPWSTR right;
            ConvertSidToStringSid(intPtr, &right);
            if (right != nullptr)
            {
                for (hstring left : _AppListConfig)
                {
                    if ((hstring)left == (hstring)right)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    IVector<hstring> LoopUtil::PI_NetworkIsolationGetAppContainerConfig()
    {
        DWORD size = 0;
        PSID_AND_ATTRIBUTES arrayValue = nullptr;
        IVector<hstring> list = single_threaded_vector<hstring>();

        if (GetNetworkIsolationGetAppContainerConfig()(&size, &arrayValue) == S_OK)
        {
            for (DWORD i = 0; i < size; i++)
            {
                LPWSTR sid;
                SID_AND_ATTRIBUTES cur = arrayValue[i];
                if (cur.Sid != nullptr)
                {
                    ConvertSidToStringSid(cur.Sid, &sid);
                    list.Append(sid);
                }
            }
        }

        return list;
    }

    IVector<AppContainer> LoopUtil::PI_NetworkIsolationEnumAppContainers(IVector<AppContainer> &list)
    {
        list.Clear();

        DWORD size = 0;
        PINET_FIREWALL_APP_CONTAINER arrayValue = nullptr;

        GetNetworkIsolationEnumAppContainers()(NETISO_FLAG::NETISO_FLAG_MAX, &size, &arrayValue);
        PINET_FIREWALL_APP_CONTAINER _PACs = arrayValue; //store the pointer so it can be freed when we close the form

        for (DWORD i = 0; i < size; i++)
        {
            INET_FIREWALL_APP_CONTAINER cur = arrayValue[i];
            AppContainer app = CreateAppContainer(cur, CheckLoopback(cur.appContainerSid));
            list.Append(app);
        }

        PI_NetworkIsolationFreeAppContainers(_PACs);
         
        return list;
    }

    void LoopUtil::PI_NetworkIsolationFreeAppContainers(PINET_FIREWALL_APP_CONTAINER point)
    {
        if (point != nullptr)
        {
            GetNetworkIsolationFreeAppContainers()(point);
        }
    }

    IVector<hstring> LoopUtil::GetEnabledLoopList(hstring sid, bool isAdd)
    {
        IVector<hstring> enabledList = single_threaded_vector<hstring>();
        for (AppContainer app : apps)
        {
            if (app.IsEnableLoop())
            {
                hstring stringSid = app.AppContainerSid();
                if (stringSid != sid)
                {
                    enabledList.Append(app.AppContainerSid());
                }
            }
        }
        if (isAdd)
        {
            enabledList.Append(sid);
        }
        return enabledList;
    }

    IVector<hstring> LoopUtil::GetEnabledLoopList(IIterable<hstring> list, bool isAdd)
    {
        IVector<hstring> enabledList = single_threaded_vector<hstring>();
        for (AppContainer app : apps)
        {
            if (app.IsEnableLoop())
            {
                bool found = false;
                hstring stringSid = app.AppContainerSid();
                for (hstring sid : list)
                {
                    if (stringSid == sid)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    enabledList.Append(app.AppContainerSid());
                }
            }
        }
        if (isAdd)
        {
            for (hstring sid : list)
            {
                enabledList.Append(sid);
            }
        }
        return enabledList;
    }

    void LoopUtil::Close()
    {
        if (FirewallAPI != nullptr)
        {
            FreeLibrary(FirewallAPI);
            FirewallAPI = nullptr;
        }
        apps.Clear();
        _AppListConfig.Clear();
    }

    IAsyncAction LoopUtil::StopService()
    {
        co_await winrt::resume_after(std::chrono::seconds(1));
        ExitProcess(S_OK);
    }
}
