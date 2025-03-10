using LoopBack.Metadata;
using System.Threading;
using Windows.System;
using Windows.UI.Xaml;

namespace LoopBack
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            if (args is ["-RegisterProcessAsComServer", ..])
            {
                ServerFactory.StartServer();
            }
            else
            {
                Application.Start(static p =>
                {
                    DispatcherQueueSynchronizationContext context = new(DispatcherQueue.GetForCurrentThread());
                    SynchronizationContext.SetSynchronizationContext(context);
                    _ = new App();
                });
            }
        }
    }
}
