using LoopBack.Metadata;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.Win32.Foundation;
using Windows.Win32.System.WinRT;
using Windows.Win32.UI.Shell;
using WinRT;

namespace LoopBack.Common
{
    internal sealed class TaskbarProgress
    {
        private readonly ITaskbarList3 _taskbarList;
        private readonly HWND hwnd = CoreWindow.GetForCurrentThread().As<ICoreWindowInterop>().WindowHandle;

        public TaskbarProgress(TaskbarList taskbarList)
        {
            _taskbarList = taskbarList.As<ITaskbarList3>();
            _taskbarList.HrInit();
        }

        public static async Task<TaskbarProgress> GetForDispatcher(CoreDispatcher dispatcher)
        {
            await dispatcher.ResumeForegroundAsync();
            return new TaskbarProgress(new TaskbarList());
        }

        public static async Task<TaskbarProgress> GetForDispatcher(TaskbarList taskbar, CoreDispatcher dispatcher)
        {
            await dispatcher.ResumeForegroundAsync();
            return new TaskbarProgress(taskbar);
        }

        /// <summary>
        /// Allows to change the status of the progress bar in the task bar.
        /// </summary>
        /// <param name="state">State of the progress indicator.</param>
        public void SetProgressState(TBPFLAG state) => _taskbarList.SetProgressState(hwnd, state);

        /// <summary>
        /// Allows to change the fill of the task bar.
        /// </summary>
        /// <param name="current">Current value to display</param>
        /// <param name="max">Maximum number for division.</param>
        public void SetProgressValue(ulong current, ulong max) => _taskbarList.SetProgressValue(hwnd, current, max);
    }
}

namespace Windows.Win32.System.WinRT
{
    file static class Extensions
    {
        extension(ICoreWindowInterop interop)
        {
            public unsafe HWND WindowHandle
            {
                get
                {
                    HWND hwnd = default;
                    interop.get_WindowHandle(&hwnd);
                    return hwnd;
                }
            }
        }
    }
}
