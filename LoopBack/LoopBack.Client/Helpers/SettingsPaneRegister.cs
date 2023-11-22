using LoopBack.Client.Pages;
using LoopBack.Metadata;
using Microsoft.Toolkit.Uwp.UI;
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

namespace LoopBack.Client.Helpers
{
#pragma warning disable CS0618
    public static class SettingsPaneRegister
    {
        public static bool IsSearchPaneSupported { get; } = ApiInformation.IsTypePresent("Windows.ApplicationModel.Search.SearchPane") && CheckSearchExtension();
        public static bool IsSettingsPaneSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.ApplicationSettings.SettingsPane");

        public static void Register(Window window)
        {
            if (IsSearchPaneSupported)
            {
                SearchPane searchPane = SearchPane.GetForCurrentView();
                searchPane.QuerySubmitted -= SearchPane_QuerySubmitted;
                searchPane.QuerySubmitted += SearchPane_QuerySubmitted;
                searchPane.SuggestionsRequested -= SearchPane_SuggestionsRequested;
                searchPane.SuggestionsRequested += SearchPane_SuggestionsRequested;
            }

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
            if (IsSearchPaneSupported)
            {
                SearchPane searchPane = SearchPane.GetForCurrentView();
                searchPane.QuerySubmitted -= SearchPane_QuerySubmitted;
                searchPane.QuerySubmitted += SearchPane_QuerySubmitted;
                searchPane.SuggestionsRequested -= SearchPane_SuggestionsRequested;
                searchPane.SuggestionsRequested += SearchPane_SuggestionsRequested;
            }

            if (IsSettingsPaneSupported)
            {
                SettingsPane.GetForCurrentView().CommandsRequested -= OnCommandsRequested;
                window.Dispatcher.AcceleratorKeyActivated -= Dispatcher_AcceleratorKeyActivated;
            }
        }

        private static async void SearchPane_SuggestionsRequested(SearchPane sender, SearchPaneSuggestionsRequestedEventArgs args)
        {
            SearchPaneSuggestionsRequestDeferral deferral = args.Request.GetDeferral();
            string keyWord = args.QueryText;
            IList<string> results = null;
            await Task.Run(() =>
            {
                results = new List<string>();
                foreach (AppContainer app in LoopBackProjectionFactory.ServerManager.GetLoopUtil().GetAppContainers())
                {
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
            args.Request.SearchSuggestionCollection.AppendQuerySuggestions(results ?? Array.Empty<string>());
            deferral.Complete();
        }

        private static void SearchPane_QuerySubmitted(SearchPane sender, SearchPaneQuerySubmittedEventArgs args)
        {
            if (args.QueryText is string keyWord && !string.IsNullOrEmpty(keyWord))
            {
                ManagePage page = Window.Current?.Content?.FindDescendant<ManagePage>();
                _ = page.Provider.FilterDataAsync(keyWord);
                page.ClearSort();
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
