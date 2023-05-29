using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.NetworkManagement.WindowsFirewall;
using Windows.Win32.Security;

namespace LoopBack.Metadata
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/archive/blogs/fiddler/revisiting-fiddler-and-win8-immersive-applications
    /// </summary>
    public sealed class LoopUtil : IDisposable
    {
        internal List<INET_FIREWALL_APP_CONTAINER> _AppList;
        internal List<SID_AND_ATTRIBUTES> _AppListConfig;
        public IList<AppContainer> Apps { get; } = new List<AppContainer>();
        unsafe internal INET_FIREWALL_APP_CONTAINER* _pACs;

        public LoopUtil() => LoadApps();

        public unsafe void LoadApps()
        {
            Apps.Clear();
            _pACs = (INET_FIREWALL_APP_CONTAINER*)0;
            //Full List of Apps
            _AppList = PI_NetworkIsolationEnumAppContainers();
            //List of Apps that have LoopUtil enabled.
            _AppListConfig = PI_NetworkIsolationGetAppContainerConfig();
            foreach (INET_FIREWALL_APP_CONTAINER PI_app in _AppList)
            {
                AppContainer app = new(PI_app.appContainerName.ToString(), PI_app.displayName.ToString(), PI_app.workingDirectory.ToString(), PI_app.appContainerSid);

                List<SID_AND_ATTRIBUTES> app_capabilities = GetCapabilities(PI_app.capabilities);
                foreach (SID_AND_ATTRIBUTES app_capability in app_capabilities)
                {
                    _ = PInvoke.ConvertSidToStringSid(app_capability.Sid, out PWSTR mysid);
                    app.Capabilities.Add(mysid.ToString());
                }

                app.LoopUtil = CheckLoopback(PI_app.appContainerSid);
                Apps.Add(app);
            }
        }

        private unsafe bool CheckLoopback(SID* intPtr)
        {
            foreach (SID_AND_ATTRIBUTES item in _AppListConfig)
            {
                PInvoke.ConvertSidToStringSid(item.Sid, out PWSTR left);
                PInvoke.ConvertSidToStringSid(new PSID(intPtr), out PWSTR right);

                if (left.ToString() == right.ToString())
                {
                    return true;
                }
            }
            return false;
        }

        private static unsafe List<SID_AND_ATTRIBUTES> GetCapabilities(INET_FIREWALL_AC_CAPABILITIES cap)
        {
            List<SID_AND_ATTRIBUTES> myCap = new();

            SID_AND_ATTRIBUTES* arrayValue = cap.capabilities;

            for (int i = 0; i < cap.count; i++)
            {
                SID_AND_ATTRIBUTES cur = arrayValue[i];
                myCap.Add(cur);
            }

            return myCap;
        }

        private unsafe static List<SID_AND_ATTRIBUTES> GetContainerSID(INET_FIREWALL_AC_CAPABILITIES cap)
        {
            List<SID_AND_ATTRIBUTES> myCap = new();

            SID_AND_ATTRIBUTES* arrayValue = cap.capabilities;

            for (int i = 0; i < cap.count; i++)
            {
                SID_AND_ATTRIBUTES cur = arrayValue[i];
                myCap.Add(cur);
            }

            return myCap;
        }

        private unsafe static List<SID_AND_ATTRIBUTES> PI_NetworkIsolationGetAppContainerConfig()
        {
            uint size = 0;
            List<SID_AND_ATTRIBUTES> list = new();

            // Pin down variables
            GCHandle handle_pdwCntPublicACs = GCHandle.Alloc(size, GCHandleType.Pinned);

            _ = PInvoke.NetworkIsolationGetAppContainerConfig(out size, out SID_AND_ATTRIBUTES* arrayValue);

            for (int i = 0; i < size; i++)
            {
                SID_AND_ATTRIBUTES cur = arrayValue[i];
                list.Add(cur);
            }

            //release pinned variables.
            handle_pdwCntPublicACs.Free();

            return list;
        }

        private unsafe List<INET_FIREWALL_APP_CONTAINER> PI_NetworkIsolationEnumAppContainers()
        {
            uint size = 0;
            List<INET_FIREWALL_APP_CONTAINER> list = new();

            // Pin down variables
            GCHandle handle_pdwCntPublicACs = GCHandle.Alloc(size, GCHandleType.Pinned);

            //_ = PInvoke.NetworkIsolationGetAppContainerConfig(out size, out arrayValue);

            _ = PInvoke.NetworkIsolationEnumAppContainers((int)NETISO_FLAG.NETISO_FLAG_MAX, out size, out INET_FIREWALL_APP_CONTAINER* arrayValue);
            _pACs = arrayValue; //store the pointer so it can be freed when we close the form

            int structSize = Marshal.SizeOf<INET_FIREWALL_APP_CONTAINER>();
            for (int i = 0; i < size; i++)
            {
                INET_FIREWALL_APP_CONTAINER cur = arrayValue[i];
                list.Add(cur);
            }

            //release pinned variables.
            handle_pdwCntPublicACs.Free();

            return list;
        }

        public bool SaveLoopbackState()
        {
            int countEnabled = CountEnabledLoopUtil();
            SID_AND_ATTRIBUTES[] arr = new SID_AND_ATTRIBUTES[countEnabled];
            int count = 0;

            for (int i = 0; i < Apps.Count; i++)
            {
                if (Apps[i].LoopUtil)
                {
                    arr[count].Attributes = 0;
                    //TO DO:
                    PInvoke.ConvertStringSidToSid(Apps[i].StringSid, out PSID ptr);
                    arr[count].Sid = ptr;
                    count++;
                }
            }

            return PInvoke.NetworkIsolationSetAppContainerConfig(arr) == 0;
        }

        private int CountEnabledLoopUtil()
        {
            int count = 0;
            for (int i = 0; i < Apps.Count; i++)
            {
                if (Apps[i].LoopUtil)
                {
                    count++;
                }
            }
            return count;
        }

        public unsafe void FreeResources()
        {
            PInvoke.NetworkIsolationFreeAppContainers(_pACs);
            _pACs = (INET_FIREWALL_APP_CONTAINER*)0;
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                FreeResources();
                Apps.Clear();
                _AppList.Clear();
                _AppListConfig.Clear();
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}