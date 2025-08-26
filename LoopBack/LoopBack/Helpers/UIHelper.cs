using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace LoopBack.Helpers
{
    public static class UIHelper
    {
        public static bool HasTitleBar => !CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar;

        public static object GetMessage(this Exception ex) => ex.Message is { Length: > 0 } message ? message : ex.GetType();

        /// <summary>
        /// Extension method for <see cref="CoreDispatcher"/>. Offering an actual awaitable <see cref="Task"/> with optional result that will be executed on the given dispatcher.
        /// </summary>
        /// <param name="dispatcher">Dispatcher of a thread to run <paramref name="function"/>.</param>
        /// <param name="function"> Function to be executed on the given dispatcher.</param>
        /// <param name="priority">Dispatcher execution priority, default is normal.</param>
        /// <returns>An awaitable <see cref="Task"/> for the operation.</returns>
        /// <remarks>If the current thread has UI access, <paramref name="function"/> will be invoked directly.</remarks>
        public static Task AwaitableRunAsync(this CoreDispatcher dispatcher, Action function, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            ArgumentNullException.ThrowIfNull(function);

            /* Run the function directly when we have thread access.
             * Also reuse Task.CompletedTask in case of success,
             * to skip an unnecessary heap allocation for every invocation. */
            if (dispatcher.HasThreadAccess)
            {
                try
                {
                    function();
                    return Task.CompletedTask;
                }
                catch (Exception e)
                {
                    return Task.FromException(e);
                }
            }

            TaskCompletionSource taskCompletionSource = new TaskCompletionSource();

            _ = dispatcher.RunAsync(priority, () =>
            {
                try
                {
                    function();
                    taskCompletionSource.SetResult();
                }
                catch (Exception e)
                {
                    taskCompletionSource.SetException(e);
                }
            });

            return taskCompletionSource.Task;
        }

        /// <summary>
        /// Extension method for <see cref="CoreDispatcher"/>. Offering an actual awaitable <see cref="Task{T}"/> with optional result that will be executed on the given dispatcher.
        /// </summary>
        /// <typeparam name="T">Returned data type of the function.</typeparam>
        /// <param name="dispatcher">Dispatcher of a thread to run <paramref name="function"/>.</param>
        /// <param name="function"> Function to be executed on the given dispatcher.</param>
        /// <param name="priority">Dispatcher execution priority, default is normal.</param>
        /// <returns>An awaitable <see cref="Task{T}"/> for the operation.</returns>
        /// <remarks>If the current thread has UI access, <paramref name="function"/> will be invoked directly.</remarks>
        public static Task<T> AwaitableRunAsync<T>(this CoreDispatcher dispatcher, Func<T> function, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            ArgumentNullException.ThrowIfNull(function);

            // Skip the dispatch, if possible
            if (dispatcher.HasThreadAccess)
            {
                try
                {
                    return Task.FromResult(function());
                }
                catch (Exception e)
                {
                    return Task.FromException<T>(e);
                }
            }

            TaskCompletionSource<T> taskCompletionSource = new();

            _ = dispatcher.RunAsync(priority, () =>
            {
                try
                {
                    taskCompletionSource.SetResult(function());
                }
                catch (Exception e)
                {
                    taskCompletionSource.SetException(e);
                }
            });

            return taskCompletionSource.Task;
        }
    }
}
