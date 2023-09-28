using System;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace LoopBack.Client.Helpers
{
#pragma warning disable CS0618
    public static class SettingsPaneRegister
    {
        public static bool IsSettingsPaneSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.ApplicationSettings.SettingsPane");

        public static void Register(Window window)
        {
            if (IsSettingsPaneSupported)
            {
                SettingsPane searchPane = SettingsPane.GetForCurrentView();
                searchPane.CommandsRequested -= OnCommandsRequested;
                searchPane.CommandsRequested += OnCommandsRequested;
                window.Dispatcher.AcceleratorKeyActivated -= Dispatcher_AcceleratorKeyActivated;
                window.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
            }
        }

        public static void Unregister(Window window)
        {
            if (IsSettingsPaneSupported)
            {
                SettingsPane.GetForCurrentView().CommandsRequested -= OnCommandsRequested;
                window.Dispatcher.AcceleratorKeyActivated -= Dispatcher_AcceleratorKeyActivated;
            }
        }

        private static void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            args.Request.ApplicationCommands.Add(
                new SettingsCommand(
                    "Feedback",
                    "Feedback",
                    (handler) => _ = Launcher.LaunchUriAsync(new Uri("https://github.com/wherewhere/Windows-Loopback-Exemption-Manager/issues"))));
            args.Request.ApplicationCommands.Add(
                new SettingsCommand(
                    "LogFolder",
                    "LogFolder",
                    async (handler) => _ = Launcher.LaunchFolderAsync(await ApplicationData.Current.LocalFolder.CreateFolderAsync("MetroLogs", CreationCollisionOption.OpenIfExists))));
            args.Request.ApplicationCommands.Add(
                new SettingsCommand(
                    "Repository",
                    "Repository",
                    (handler) => _ = Launcher.LaunchUriAsync(new Uri("https://github.com/wherewhere/Windows-Loopback-Exemption-Manager"))));
        }

        private static void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            if (args.EventType.HasFlag(CoreAcceleratorKeyEventType.KeyDown) || args.EventType.HasFlag(CoreAcceleratorKeyEventType.SystemKeyUp))
            {
                CoreVirtualKeyStates ctrl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
                if (ctrl.HasFlag(CoreVirtualKeyStates.Down))
                {
                    CoreVirtualKeyStates shift = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift);
                    if (shift.HasFlag(CoreVirtualKeyStates.Down))
                    {
                        switch (args.VirtualKey)
                        {
                            case VirtualKey.X when IsSettingsPaneSupported:
                                SettingsPane.Show();
                                args.Handled = true;
                                break;
                        }
                    }
                }
            }
        }
    }
}
