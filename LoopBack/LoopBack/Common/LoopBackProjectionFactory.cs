using LoopBack.Metadata;
using System;
using System.Runtime.InteropServices;
using WinRT;

namespace LoopBack.Common
{
    public static partial class LoopBackProjectionFactory
    {
        private const uint CLSCTX_ALL = 1 | 2 | 4 | 16;
        public static readonly Guid CLSID_IUnknown = new("00000000-0000-0000-C000-000000000046");
        public static readonly Guid CLSID_ServerManager = new(0x50169480, 0x3fb8, 0x4a19, 0xaa, 0xed, 0xed, 0x91, 0x70, 0x81, 0x1a, 0x3a);  // 50169480-3FB8-4A19-AAED-ED9170811A3A
        
        private static ServerManager m_serverManager;
        public static ServerManager ServerManager
        {
            get
            {
                try
                {
                    if (m_serverManager != null && m_serverManager.IsServerRunning)
                    {
                        return m_serverManager;
                    }
                }
                catch { }
                m_serverManager = TryCreateInstance<ServerManager>(CLSID_ServerManager, CLSCTX_ALL);
                return m_serverManager;
            }
        }

        public static T CreateInstance<T>(Guid rclsid, uint dwClsContext = CLSCTX_ALL)
        {
            int hresult = CoCreateInstance(rclsid, 0, dwClsContext, CLSID_IUnknown, out nint result);
            if (hresult < 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }
            return Marshaler<T>.FromAbi(result);
        }

        public static T TryCreateInstance<T>(Guid rclsid, uint dwClsContext = 0x1) where T : class
        {
            int hresult = CoCreateInstance(rclsid, 0, dwClsContext, CLSID_IUnknown, out nint result);
            return hresult < 0 ? null : Marshaler<T>.FromAbi(result);
        }

        [LibraryImport("api-ms-win-core-com-l1-1-0.dll")]
        private static partial int CoCreateInstance(in Guid rclsid, nint pUnkOuter, uint dwClsContext, in Guid riid, out nint ppv);
    }
}
