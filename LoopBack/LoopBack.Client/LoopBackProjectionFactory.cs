using LoopBack.Metadata;
using System;
using System.Runtime.InteropServices;

namespace LoopBack.Client
{
    public static class LoopBackProjectionFactory
    {
        private const uint CLSCTX_ALL = 1 | 2 | 4 | 16;

        private static Guid CLSID_IUnknown = new("00000000-0000-0000-C000-000000000046");
        private static Guid CLSID_LoopUtil = new("50169480-3FB8-4A19-AAED-ED9170811A3A");

        public static LoopUtil CreateLoopUtil() => CreateInstance<LoopUtil>(CLSID_LoopUtil, CLSCTX_ALL);

        public static LoopUtil TryCreateLoopUtil() => TryCreateInstance<LoopUtil>(CLSID_LoopUtil, CLSCTX_ALL);

        public static T CreateInstance<T>(Guid rclsid, uint dwClsContext = 0x1)
        {
            Guid riid = CLSID_IUnknown;
            uint hresult = CoCreateInstance(ref rclsid, IntPtr.Zero, dwClsContext, ref riid, out IntPtr results);
            if (hresult != 0)
            {
                Marshal.ThrowExceptionForHR((int)hresult);
            }
            return (T)Marshal.GetObjectForIUnknown(results);
        }

        public static T TryCreateInstance<T>(Guid rclsid, uint dwClsContext = 0x1) where T : class
        {
            Guid riid = CLSID_IUnknown;
            _ = CoCreateInstance(ref rclsid, IntPtr.Zero, dwClsContext, ref riid, out IntPtr results);
            return results == IntPtr.Zero ? null : (T)Marshal.GetObjectForIUnknown(results);
        }

        [DllImport("ole32", EntryPoint = "CoCreateInstance", ExactSpelling = true)]
        private static extern uint CoCreateInstance(
            [In] ref Guid rclsid,
            [In] IntPtr pUnkOuter,
            [In] uint dwClsContext,
            [In] ref Guid riid,
            [Out] out IntPtr ppv);
    }
}
