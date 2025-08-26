using CommunityToolkit.WinUI;
using LoopBack.Helpers;
using LoopBack.Metadata;
using LoopBack.Pages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Search;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace LoopBack.Common
{
    public static class SettingsPaneRegister
    {
        public static bool IsSearchPaneSupported { get; } = ApiInformation.IsTypePresent("Windows.ApplicationModel.Search.SearchPane") && CheckSearchExtension();
        public static bool IsSettingsPaneSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.ApplicationSettings.SettingsPane");

        public static void Register(Window window)
        {
            try
            {
                if (IsSearchPaneSupported)
                {
                    SearchPane searchPane = SearchPane.GetForCurrentView();
                    searchPane.QuerySubmitted -= SearchPane_QuerySubmitted;
                    searchPane.QuerySubmitted += SearchPane_QuerySubmitted;
                    searchPane.SuggestionsRequested -= SearchPane_SuggestionsRequested;
                    searchPane.SuggestionsRequested += SearchPane_SuggestionsRequested;
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger(typeof(SettingsPaneRegister)).LogError(ex, "Failed to register search pane. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
            }

            try
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
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger(typeof(SettingsPaneRegister)).LogError(ex, "Failed to register settings pane. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
            }
        }

        public static void Unregister(Window window)
        {
            try
            {
                if (IsSearchPaneSupported)
                {
                    SearchPane searchPane = SearchPane.GetForCurrentView();
                    searchPane.QuerySubmitted -= SearchPane_QuerySubmitted;
                    searchPane.SuggestionsRequested -= SearchPane_SuggestionsRequested;
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger(typeof(SettingsPaneRegister)).LogError(ex, "Failed to unregister search pane. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
            }

            try
            {
                if (IsSettingsPaneSupported)
                {
                    SettingsPane.GetForCurrentView().CommandsRequested -= OnCommandsRequested;
                    window.Dispatcher.AcceleratorKeyActivated -= Dispatcher_AcceleratorKeyActivated;
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger(typeof(SettingsPaneRegister)).LogError(ex, "Failed to unregister settings pane. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
            }
        }

        private static async void SearchPane_SuggestionsRequested(SearchPane sender, SearchPaneSuggestionsRequestedEventArgs args)
        {
            SearchPaneSuggestionsRequestDeferral deferral = args.Request.GetDeferral();
            string keyWord = args.QueryText;
            List<string> results = null;
            await Task.Run(() =>
            {
                results = [];
                using LoopUtil loopUtil = LoopBackProjectionFactory.ServerManager.GetLoopUtil();
                IReadOnlyList<AppContainer> appContainers = loopUtil.Apps;
                for (int i = 0; i < appContainers.Count; i++)
                {
                    AppContainer app = appContainers[i];
                    if (app != null)
                    {
                        string appName = app.DisplayName;
                        if (appName.Contains(keyWord, StringComparison.OrdinalIgnoreCase))
                        {
                            results.Add(appName);
                        }
                        else
                        {
                            string packageFullName = app.PackageFullName;
                            if (packageFullName.Contains(keyWord, StringComparison.OrdinalIgnoreCase))
                            {
                                results.Add(packageFullName);
                            }
                        }
                    }
                }
            });
            args.Request.SearchSuggestionCollection.AppendQuerySuggestions(results ?? []);
            deferral.Complete();
        }

        private static void SearchPane_QuerySubmitted(SearchPane sender, SearchPaneQuerySubmittedEventArgs args)
        {
            if (args.QueryText is string keyWord && !string.IsNullOrEmpty(keyWord))
            {
                ManagePage page = Window.Current?.Content?.FindDescendant<ManagePage>();
                _ = page.Provider.FilterDataAsync(keyWord);
                //page.ClearSort();
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
                    async (handler) => _ = Launcher.LaunchFolderAsync(await ApplicationData.Current.LocalFolder.CreateFolderAsync("Logs", CreationCollisionOption.OpenIfExists))));
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
                CoreWindow window = CoreWindow.GetForCurrentThread();
                CoreVirtualKeyStates ctrl = window.GetKeyState(VirtualKey.Control);
                if (ctrl.HasFlag(CoreVirtualKeyStates.Down))
                {
                    CoreVirtualKeyStates shift = window.GetKeyState(VirtualKey.Shift);
                    if (shift.HasFlag(CoreVirtualKeyStates.Down))
                    {
                        switch (args.VirtualKey)
                        {
                            case VirtualKey.X when IsSettingsPaneSupported:
                                SettingsPane.Show();
                                args.Handled = true;
                                break;
                            case VirtualKey.Q when IsSearchPaneSupported:
                                SearchPane.GetForCurrentView().Show();
                                args.Handled = true;
                                break;
                        }
                    }
                }
            }
        }

        private static bool CheckSearchExtension()
        {
            XDocument doc = XDocument.Load(Path.Combine(Package.Current.InstalledLocation.Path, "AppxManifest.xml"));
            XNamespace ns = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/uap/windows10");
            IEnumerable<XElement> extensions = doc.Root.Descendants(ns + "Extension");
            if (extensions != null)
            {
                foreach (XElement extension in extensions)
                {
                    XAttribute category = extension.Attribute("Category");
                    if (category != null && category.Value == "windows.search")
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
