using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Loopback
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/archive/blogs/fiddler/revisiting-fiddler-and-win8-immersive-applications
    /// </summary>
    public class LoopUtil
    {
        //https://docs.microsoft.com/zh-cn/windows/win32/api/winnt/ns-winnt-sid_and_attributes
        [StructLayout(LayoutKind.Sequential)]
        internal struct SID_AND_ATTRIBUTES
        {
            public IntPtr Sid;
            public uint Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct INET_FIREWALL_AC_CAPABILITIES
        {
            public uint Count;
            public IntPtr Capabilities; //SID_AND_ATTRIBUTES
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct INET_FIREWALL_AC_BINARIES
        {
            public uint Count;
            public IntPtr Binaries;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct INET_FIREWALL_APP_CONTAINER
        {
            internal IntPtr AppContainerSid;
            internal IntPtr UserSid;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string AppContainerName;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string DisplayName;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string Description;

            internal INET_FIREWALL_AC_CAPABILITIES Capabilities;
            internal INET_FIREWALL_AC_BINARIES Binaries;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string WorkingDirectory;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string PackageFullName;
        }

        // Call this API to free the memory returned by the Enumeration API
        [DllImport("FirewallAPI.dll")]
        internal static extern void NetworkIsolationFreeAppContainers(IntPtr pACs);

        // Call this API to load the current list of LoopUtil-enabled AppContainers
        [DllImport("FirewallAPI.dll")]
        internal static extern uint NetworkIsolationGetAppContainerConfig(out uint pdwCntACs, out IntPtr appContainerSids);

        // Call this API to set the LoopUtil-exemption list
        [DllImport("FirewallAPI.dll")]
        private static extern uint NetworkIsolationSetAppContainerConfig(uint pdwCntACs, SID_AND_ATTRIBUTES[] appContainerSids);

        // Use this API to convert a string SID into an actual SID
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool ConvertStringSidToSid(string strSid, out IntPtr pSid);

        [DllImport("advapi32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ConvertSidToStringSid(
            [MarshalAs(UnmanagedType.LPArray)] byte[] pSID,
            out IntPtr ptrSid);

        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool ConvertSidToStringSid(IntPtr pSid, out string strSid);

        // Use this API to convert a string reference (e.g. "@{blah.pri?ms-resource://whatever}") into a plain string
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern int SHLoadIndirectString(string pszSource, StringBuilder pszOutBuf);

        // Call this API to enumerate all of the AppContainers on the system
        [DllImport("FirewallAPI.dll")]
        internal static extern uint NetworkIsolationEnumAppContainers(uint Flags, out uint pdwCntPublicACs, out IntPtr ppACs);

        //https://docs.microsoft.com/zh-cn/windows/win32/api/netfw/ne-netfw-netiso_flag
        private enum NETISO_FLAG
        {
            NETISO_FLAG_FORCE_COMPUTE_BINARIES = 0x1,
            NETISO_FLAG_MAX = 0x2
        }

        public class AppContainer
        {
            public string AppContainerName { get; set; }
            public string DisplayName { get; set; }
            public string WorkingDirectory { get; set; }
            public string StringSid { get; set; }
            public List<uint> Capabilities { get; set; }
            public bool LoopUtil { get; set; }

            public AppContainer(string _appContainerName, string _displayName, string _workingDirectory, IntPtr _sid)
            {
                AppContainerName = _appContainerName;
                DisplayName = _displayName;
                WorkingDirectory = _workingDirectory;
                ConvertSidToStringSid(_sid, out string tempSid);
                StringSid = tempSid;
            }
        }

        internal List<INET_FIREWALL_APP_CONTAINER> _AppList;
        internal List<SID_AND_ATTRIBUTES> _AppListConfig;
        public List<AppContainer> Apps = new List<AppContainer>();
        internal IntPtr _pACs;

        public LoopUtil()
        {
            LoadApps();
        }

        public void LoadApps()
        {
            Apps.Clear();
            _pACs = IntPtr.Zero;
            //Full List of Apps
            _AppList = PI_NetworkIsolationEnumAppContainers();
            //List of Apps that have LoopUtil enabled.
            _AppListConfig = PI_NetworkIsolationGetAppContainerConfig();
            foreach (INET_FIREWALL_APP_CONTAINER PI_app in _AppList)
            {
                AppContainer app = new AppContainer(PI_app.AppContainerName, PI_app.DisplayName, PI_app.WorkingDirectory, PI_app.AppContainerSid);

                List<SID_AND_ATTRIBUTES> app_capabilities = GetCapabilites(PI_app.Capabilities);
                if (app_capabilities.Count > 0)
                {
                    //var sid = new SecurityIdentifier(app_capabilities[0], 0);

                    IntPtr arrayValue = IntPtr.Zero;
                    //var b = LoopUtil.ConvertStringSidToSid(app_capabilities[0].Sid, out arrayValue);
                    //string mysid;
                    //var b = LoopUtil.ConvertSidToStringSid(app_capabilities[0].Sid, out mysid);
                }
                app.LoopUtil = CheckLoopback(PI_app.AppContainerSid);
                Apps.Add(app);
            }
        }

        private bool CheckLoopback(IntPtr intPtr)
        {
            foreach (SID_AND_ATTRIBUTES item in _AppListConfig)
            {
                ConvertSidToStringSid(item.Sid, out string left);
                ConvertSidToStringSid(intPtr, out string right);

                if (left == right)
                {
                    return true;
                }
            }
            return false;
        }

        private static List<SID_AND_ATTRIBUTES> GetCapabilites(INET_FIREWALL_AC_CAPABILITIES cap)
        {
            List<SID_AND_ATTRIBUTES> mycap = new List<SID_AND_ATTRIBUTES>();

            IntPtr arrayValue = cap.Capabilities;

            int structSize = Marshal.SizeOf(typeof(SID_AND_ATTRIBUTES));
            for (int i = 0; i < cap.Count; i++)
            {
                SID_AND_ATTRIBUTES cur = (SID_AND_ATTRIBUTES)Marshal.PtrToStructure(arrayValue, typeof(SID_AND_ATTRIBUTES));
                mycap.Add(cur);
                arrayValue = new IntPtr((long)arrayValue + structSize);
            }

            return mycap;
        }

        private static List<SID_AND_ATTRIBUTES> GetContainerSID(INET_FIREWALL_AC_CAPABILITIES cap)
        {
            List<SID_AND_ATTRIBUTES> mycap = new List<SID_AND_ATTRIBUTES>();

            IntPtr arrayValue = cap.Capabilities;

            int structSize = Marshal.SizeOf<SID_AND_ATTRIBUTES>();
            for (int i = 0; i < cap.Count; i++)
            {
                SID_AND_ATTRIBUTES cur = Marshal.PtrToStructure<SID_AND_ATTRIBUTES>(arrayValue);
                mycap.Add(cur);
                arrayValue = new IntPtr((long)arrayValue + structSize);
            }

            return mycap;
        }

        private static List<SID_AND_ATTRIBUTES> PI_NetworkIsolationGetAppContainerConfig()
        {
            IntPtr arrayValue = IntPtr.Zero;
            uint size = 0;
            List<SID_AND_ATTRIBUTES> list = new List<SID_AND_ATTRIBUTES>();

            // Pin down variables
            GCHandle handle_pdwCntPublicACs = GCHandle.Alloc(size, GCHandleType.Pinned);
            GCHandle handle_ppACs = GCHandle.Alloc(arrayValue, GCHandleType.Pinned);

            uint retval = NetworkIsolationGetAppContainerConfig(out size, out arrayValue);

            int structSize = Marshal.SizeOf<SID_AND_ATTRIBUTES>();
            for (int i = 0; i < size; i++)
            {
                SID_AND_ATTRIBUTES cur = Marshal.PtrToStructure<SID_AND_ATTRIBUTES>(arrayValue);
                list.Add(cur);
                arrayValue = new IntPtr((long)arrayValue + structSize);
            }

            //release pinned variables.
            handle_pdwCntPublicACs.Free();
            handle_ppACs.Free();

            return list;
        }

        private List<INET_FIREWALL_APP_CONTAINER> PI_NetworkIsolationEnumAppContainers()
        {
            IntPtr arrayValue = IntPtr.Zero;
            uint size = 0;
            List<INET_FIREWALL_APP_CONTAINER> list = new List<INET_FIREWALL_APP_CONTAINER>();

            // Pin down variables
            GCHandle handle_pdwCntPublicACs = GCHandle.Alloc(size, GCHandleType.Pinned);
            GCHandle handle_ppACs = GCHandle.Alloc(arrayValue, GCHandleType.Pinned);

            //uint retval2 = NetworkIsolationGetAppContainerConfig( out size, out arrayValue);

            uint retval = NetworkIsolationEnumAppContainers((int)NETISO_FLAG.NETISO_FLAG_MAX, out size, out arrayValue);
            _pACs = arrayValue; //store the pointer so it can be freed when we close the form

            int structSize = Marshal.SizeOf<INET_FIREWALL_APP_CONTAINER>();
            for (int i = 0; i < size; i++)
            {
                INET_FIREWALL_APP_CONTAINER cur = Marshal.PtrToStructure<INET_FIREWALL_APP_CONTAINER>(arrayValue);
                list.Add(cur);
                arrayValue = new IntPtr((long)arrayValue + structSize);
            }

            //release pinned variables.
            handle_pdwCntPublicACs.Free();
            handle_ppACs.Free();

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
                    ConvertStringSidToSid(Apps[i].StringSid, out IntPtr ptr);
                    arr[count].Sid = ptr;
                    count++;
                }
            }

            return NetworkIsolationSetAppContainerConfig((uint)countEnabled, arr) == 0;
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

        public void FreeResources()
        {
            NetworkIsolationFreeAppContainers(_pACs);
        }
    }
}