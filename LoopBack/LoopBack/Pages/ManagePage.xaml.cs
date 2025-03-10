using CommunityToolkit.WinUI.Controls;
using LoopBack.Common;
using LoopBack.Metadata;
using LoopBack.ViewModels;
using System;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace LoopBack.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ManagePage : Page
    {
        public readonly ManageViewModel Provider;

        public ManagePage()
        {
            InitializeComponent();
            Provider = new ManageViewModel(Dispatcher);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _ = Provider.Refresh();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            _ = Provider.ExemptAllAsync(checkBox.IsChecked.Value);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag?.ToString())
            {
                case "Save":
                    _ = Provider.SaveConfigureAsync();
                    if (SaveButton.Flyout is FlyoutBase flyout_logout)
                    {
                        flyout_logout.Hide();
                    }
                    break;
                case "Refresh":
                    _ = Provider.Refresh().ContinueWith((x) => Provider.IsDirty = false);
                    break;
                default:
                    break;
            }
        }

        private async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string filter = sender.Text;
                ObservableCollection<string> observableCollection = [];
                sender.ItemsSource = observableCollection;
                await ThreadSwitcher.ResumeBackgroundAsync();
                if (!string.IsNullOrEmpty(filter))
                {
                    string appsInFilter = filter;
                    foreach (AppContainer app in Provider.AppContainers)
                    {
                        if (app != null)
                        {
                            string appName = app.DisplayName;
                            if (appName.Contains(appsInFilter, StringComparison.OrdinalIgnoreCase))
                            {
                                await Dispatcher.TryRunAsync(
                                    CoreDispatcherPriority.Normal,
                                    () => observableCollection.Add(appName));
                            }
                            else
                            {
                                string packageFullName = app.PackageFullName;
                                if (packageFullName.Contains(appsInFilter, StringComparison.OrdinalIgnoreCase))
                                {
                                    await Dispatcher.TryRunAsync(
                                        CoreDispatcherPriority.Normal,
                                        () => observableCollection.Add(packageFullName));
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is string word)
            {
                _ = Provider.FilterDataAsync(word);
            }
            else if (args.ChosenSuggestion is null)
            {
                _ = Provider.FilterDataAsync(sender.Text);
            }
        }

        private void DataColumn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DataColumn dataGrid = sender as DataColumn;
            if (dataGrid.Tag != null)
            {
                _ = Provider.SortDataAsync(dataGrid.Tag.ToString(), true);
            }
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element) { return; }
            switch (element.Name)
            {
                case "Open" when element.Tag != null:
                    string tag = element.Tag.ToString();
                    if (Uri.TryCreate(tag, UriKind.RelativeOrAbsolute, out Uri url))
                    {
                        if (url.IsFile)
                        {
                            _ = Launcher.LaunchFolderPathAsync(tag);
                        }
                    }
                    break;
                case "RunAsAdmin":
                    _ = Provider.RunAsAdministratorAsync();
                    break;
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element) { return; }
            if (element.Tag == null) { return; }
            DataPackage dataPackage = new();
            dataPackage.SetText(element.Tag.ToString());
            Clipboard.SetContentWithOptions(dataPackage, null);
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e) => Provider.IsDirty = true;
    }
}
