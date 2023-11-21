using LoopBack.Metadata;
using System;
using System.Runtime.InteropServices;

namespace LoopBack.Client.Helpers
{
    public static class LoopBackProjectionFactory
    {
        private const uint CLSCTX_ALL = 1 | 2 | 4 | 16;

        private static Guid CLSID_IUnknown = new("00000000-0000-0000-C000-000000000046");
        private static Guid CLSID_ServerManager = new("50169480-3FB8-4A19-AAED-ED9170811A3A");

        private static ServerManager serverManager;
        public static ServerManager ServerManager
        {
            get
            {
                try
                {
                    if (serverManager?.IsServerRunning == true) { return serverManager; }
                }
                catch (Exception ex) when (ex.HResult == -2147023174)
                {
                    SettingsHelper.LogManager.GetLogger(nameof(LoopBackProjectionFactory)).Warn(ex.ExceptionToMessage());
                }
                serverManager = TryCreateInstance<ServerManager>(CLSID_ServerManager, CLSCTX_ALL);
                return serverManager;
            }
        }

        public static T CreateInstance<T>(Guid rclsid, uint dwClsContext = 0x1)
        {
            Guid riid = CLSID_IUnknown;
            uint hresult = CoCreateInstance(in rclsid, IntPtr.Zero, dwClsContext, in riid, out nint results);
            if (hresult != 0)
            {
                Marshal.ThrowExceptionForHR((int)hresult);
            }
            return (T)Marshal.GetObjectForIUnknown(results);
        }

        public static T TryCreateInstance<T>(Guid rclsid, uint dwClsContext = 0x1) where T : class
        {
            Guid riid = CLSID_IUnknown;
            _ = CoCreateInstance(in rclsid, IntPtr.Zero, dwClsContext, in riid, out nint results);
            return results == IntPtr.Zero ? null : Marshal.GetObjectForIUnknown(results) as T;
        }

        [DllImport("ole32", EntryPoint = "CoCreateInstance", ExactSpelling = true)]
        private static extern uint CoCreateInstance(
            [In] in Guid rclsid,
            [In] nint pUnkOuter,
            [In] uint dwClsContext,
            [In] in Guid riid,
            [Out] out nint ppv);
    }
}
