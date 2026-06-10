#include "pch.h"
#include "LoopUtil.h"
#include "LoopUtil.g.cpp"

using namespace std;

namespace winrt::LoopBack::Metadata::implementation
{
    IVectorView<AppContainer> LoopUtil::GetAppContainers()
    {
        apps.Clear();
        //List of Apps that have LoopUtil enabled.
        appListConfig = PI_NetworkIsolationGetAppContainerConfig();
        //Full List of Apps
        return PI_NetworkIsolationEnumAppContainers(apps);
    }

    const HRESULT LoopUtil::SetLoopbackList(const IIterable<hstring>& list) const try
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

        return NetworkIsolationSetAppContainerConfig(count, arr.data());
    }
    catch (...)
    {
        return to_hresult();
    }

    const HRESULT LoopUtil::SetLoopbackList(const IIterable<AppContainer>& list) const try
    {
        vector<SID_AND_ATTRIBUTES> arr;
        DWORD count = 0;

        for (AppContainer app : list)
        {
            SID_AND_ATTRIBUTES sid{};
            sid.Attributes = 0;
            //TO DO:
            PSID ptr = nullptr;
            ConvertStringSidToSid(app.AppContainerSid().c_str(), &ptr);
            sid.Sid = ptr;
            arr.push_back(sid);
            count++;
        }

        return NetworkIsolationSetAppContainerConfig(count, arr.data());
    }
    catch (...)
    {
        return to_hresult();
    }

    const HRESULT LoopUtil::AddLookback(const hstring& stringSid) const try
    {
        const IVector<hstring> enabledList = GetEnabledLoopList(stringSid, true);
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

        return NetworkIsolationSetAppContainerConfig(count, arr.data());
    }
    catch (...)
    {
        return to_hresult();
    }

    const HRESULT LoopUtil::AddLookback(const AppContainer& appContainer) const try
    {
        const IVector<hstring> enabledList = GetEnabledLoopList(appContainer.AppContainerSid(), true);
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

        return NetworkIsolationSetAppContainerConfig(count, arr.data());
    }
    catch (...)
    {
        return to_hresult();
    }

    const HRESULT LoopUtil::AddLookbacks(const IIterable<hstring>& list) const try
    {
        const IVector<hstring> enabledList = GetEnabledLoopList(list, true);
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

        return NetworkIsolationSetAppContainerConfig(count, arr.data());
    }
    catch (...)
    {
        return to_hresult();
    }

    const HRESULT LoopUtil::AddLookbacks(const IIterable<AppContainer>& list) const try
    {
        const IVector<hstring> enabledList = GetEnabledLoopList(list, true);
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

        return NetworkIsolationSetAppContainerConfig(count, arr.data());
    }
    catch (...)
    {
        return to_hresult();
    }

    const HRESULT LoopUtil::RemoveLookback(const hstring& stringSid) const try
    {
        const IVector<hstring> enabledList = GetEnabledLoopList(stringSid, false);
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

        return NetworkIsolationSetAppContainerConfig(count, arr.data());
    }
    catch (...)
    {
        return to_hresult();
    }

    const HRESULT LoopUtil::RemoveLookback(const AppContainer& appContainer) const try
    {
        const IVector<hstring> enabledList = GetEnabledLoopList(appContainer.AppContainerSid(), false);
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

        return NetworkIsolationSetAppContainerConfig(count, arr.data());
    }
    catch (...)
    {
        return to_hresult();
    }

    const HRESULT LoopUtil::RemoveLookbacks(const IIterable<hstring>& list) const try
    {
        const IVector<hstring> enabledList = GetEnabledLoopList(list, false);
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

        return NetworkIsolationSetAppContainerConfig(count, arr.data());
    }
    catch (...)
    {
        return to_hresult();
    }

    const HRESULT LoopUtil::RemoveLookbacks(const IIterable<AppContainer>& list) const try
    {
        const IVector<hstring> enabledList = GetEnabledLoopList(list, false);
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

        return NetworkIsolationSetAppContainerConfig(count, arr.data());
    }
    catch (...)
    {
        return to_hresult();
    }

    const AppContainer LoopUtil::CreateAppContainer(const INET_FIREWALL_APP_CONTAINER& PI_app, const bool loopUtil) const
    {
        AppContainer app = AppContainer::AppContainer();

        app.IsEnableLoop(loopUtil);
        if (PI_app.displayName) { app.DisplayName(PI_app.displayName); }
        if (PI_app.description) { app.DisplayName(PI_app.description); }
        if (PI_app.appContainerName) { app.AppContainerName(PI_app.appContainerName); }
        if (PI_app.packageFullName) { app.PackageFullName(PI_app.packageFullName); }
        if (PI_app.workingDirectory) { app.WorkingDirectory(PI_app.workingDirectory); }

        if (PI_app.appContainerSid)
        {
            LPWSTR tempSid;
            ConvertSidToStringSid(PI_app.appContainerSid, &tempSid);
            if (tempSid) { app.AppContainerSid(tempSid); }
        }

        if (PI_app.userSid)
        {
            LPWSTR tempSid;
            ConvertSidToStringSid(PI_app.userSid, &tempSid);
            if (tempSid) { app.UserSid(tempSid); }
        }

        const IVector<hstring> capabilities = GetCapabilities(PI_app.capabilities);
        app.Capabilities(capabilities);

        const IVector<hstring> app_binaries = GetBinaries(PI_app.binaries);
        app.Binaries(app_binaries);

        return app;
    }

    const bool LoopUtil::CheckLoopback(SID* intPtr) const
    {
        if (intPtr)
        {
            LPWSTR right;
            ConvertSidToStringSid(intPtr, &right);
            if (right)
            {
                for (hstring left : appListConfig)
                {
                    if (left == (hstring)right)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    const IVector<hstring> LoopUtil::GetBinaries(const INET_FIREWALL_AC_BINARIES& cap) const
    {
        const IVector<hstring> myCap = single_threaded_vector<hstring>();

        if (cap.binaries)
        {
            const LPWSTR* arrayValue = cap.binaries;

            for (DWORD i = 0; i < cap.count; i++)
            {
                const LPWSTR cur = arrayValue[i];
                if (cur)
                {
                    myCap.Append(cur);
                }
            }
        }

        return myCap;
    }

    const IVector<hstring> LoopUtil::GetCapabilities(const INET_FIREWALL_AC_CAPABILITIES& cap) const
    {
        const IVector<hstring> myCap = single_threaded_vector<hstring>();

        if (cap.capabilities)
        {
            const SID_AND_ATTRIBUTES* arrayValue = cap.capabilities;

            for (DWORD i = 0; i < cap.count; i++)
            {
                const SID_AND_ATTRIBUTES cur = arrayValue[i];
                if (cur.Sid)
                {
                    LPWSTR mysid;
                    ConvertSidToStringSid(cur.Sid, &mysid);
                    if (mysid)
                    {
                        myCap.Append(mysid);
                    }
                }
            }
        }

        return myCap;
    }

    const IVector<hstring> LoopUtil::PI_NetworkIsolationGetAppContainerConfig() const
    {
        DWORD size = 0;
        PSID_AND_ATTRIBUTES arrayValue = nullptr;
        const IVector<hstring> list = single_threaded_vector<hstring>();

        if (NetworkIsolationGetAppContainerConfig(&size, &arrayValue) == S_OK)
        {
            if (arrayValue)
            {
                for (DWORD i = 0; i < size; i++)
                {
                    LPWSTR sid;
                    const SID_AND_ATTRIBUTES cur = arrayValue[i];
                    if (cur.Sid)
                    {
                        ConvertSidToStringSid(cur.Sid, &sid);
                        if (sid)
                        {
                            list.Append(sid);
                        }
                    }
                }
            }
        }

        return list;
    }

    const IVectorView<AppContainer> LoopUtil::PI_NetworkIsolationEnumAppContainers(const IVector<AppContainer>& list) const
    {
        if (!list) { return nullptr; }
        list.Clear();

        DWORD size = 0;
        PINET_FIREWALL_APP_CONTAINER arrayValue = nullptr;

        if (NetworkIsolationEnumAppContainers(NETISO_FLAG::NETISO_FLAG_MAX, &size, &arrayValue) == S_OK)
        {
            if (arrayValue)
            {
                const PINET_FIREWALL_APP_CONTAINER _PACs = arrayValue; //store the pointer so it can be freed when we close the form

                for (DWORD i = 0; i < size; i++)
                {
                    const INET_FIREWALL_APP_CONTAINER cur = arrayValue[i];
                    const AppContainer app = CreateAppContainer(cur, CheckLoopback(cur.appContainerSid));
                    list.Append(app);
                }

                PI_NetworkIsolationFreeAppContainers(_PACs);
            }
        }

        return list.GetView();
    }

    void LoopUtil::PI_NetworkIsolationFreeAppContainers(const PINET_FIREWALL_APP_CONTAINER& point) const
    {
        if (point)
        {
            NetworkIsolationFreeAppContainers(point);
        }
    }

    const IVector<hstring> LoopUtil::GetEnabledLoopList(const hstring& sid, const bool isAdd) const
    {
        const IVector<hstring> enabledList = single_threaded_vector<hstring>();
        for (AppContainer app : apps)
        {
            if (app.IsEnableLoop())
            {
                const hstring stringSid = app.AppContainerSid();
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

    const IVector<hstring> LoopUtil::GetEnabledLoopList(const IIterable<hstring>& list, const bool isAdd) const
    {
        const IVector<hstring> enabledList = single_threaded_vector<hstring>();
        for (AppContainer app : apps)
        {
            if (app.IsEnableLoop())
            {
                bool found = false;
                const hstring stringSid = app.AppContainerSid();
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

    const IVector<hstring> LoopUtil::GetEnabledLoopList(const IIterable<AppContainer>& list, const bool isAdd) const
    {
        const IVector<hstring> enabledList = single_threaded_vector<hstring>();
        for (AppContainer app : apps)
        {
            if (app.IsEnableLoop())
            {
                bool found = false;
                const hstring stringSid = app.AppContainerSid();
                for (AppContainer container : list)
                {
                    if (stringSid == container.AppContainerSid())
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
            for (AppContainer container : list)
            {
                enabledList.Append(container.AppContainerSid());
            }
        }
        return enabledList;
    }

    void LoopUtil::Close()
    {
        if (firewallAPI)
        {
            FreeLibrary(firewallAPI);
            firewallAPI = nullptr;
        }
        apps.Clear();
        appListConfig.Clear();
        appListConfig = nullptr;
    }
}
