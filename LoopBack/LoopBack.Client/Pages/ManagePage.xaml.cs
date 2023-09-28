using LoopBack.Client.Helpers;
using LoopBack.Client.ViewModels;
using LoopBack.Metadata;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace LoopBack.Client.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ManagePage : Page
    {
        private ManageViewModel Provider { get; } = new();

        public ManagePage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequested;
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
                    ClearSort();
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
                ClearSort();
            }
            else if (args.ChosenSuggestion is null)
            {
                _ = Provider.FilterDataAsync(sender.Text);
                ClearSort();
            }
        }

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            // Clear previous sorted column if we start sorting a different column
            string previousSortedColumn = Provider.CachedSortedColumn;
            if (previousSortedColumn != string.Empty)
            {
                foreach (DataGridColumn dataGridColumn in dataGrid.Columns)
                {
                    if (dataGridColumn.Tag != null && dataGridColumn.Tag.ToString() == previousSortedColumn &&
                        (e.Column.Tag == null || previousSortedColumn != e.Column.Tag.ToString()))
                    {
                        dataGridColumn.SortDirection = null;
                    }
                }
            }

            // Toggle clicked column's sorting method
            if (e.Column.Tag != null)
            {
                if (e.Column.SortDirection == null)
                {
                    _ = Provider.SortDataAsync(e.Column.Tag.ToString(), true);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
                else if (e.Column.SortDirection == DataGridSortDirection.Ascending)
                {
                    _ = Provider.SortDataAsync(e.Column.Tag.ToString(), false);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
                else
                {
                    _ = Provider.SortDataAsync(e.Column.Tag.ToString(), true);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
            }
        }

        private void ClearSort()
        {
            // Clear previous sorted column if we start sorting a different column
            string previousSortedColumn = Provider.CachedSortedColumn;
            if (previousSortedColumn != string.Empty)
            {
                foreach (DataGridColumn dataGridColumn in DataGrid.Columns)
                {
                    dataGridColumn.SortDirection = null;
                }
            }
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element) { return; }
            switch (element.Name)
            {
                case "Copy":
                    DataPackage dataPackage = new();
                    dataPackage.SetText(element.Tag.ToString());
                    Clipboard.SetContentWithOptions(dataPackage, null);
                    break;
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
                    _ = Provider.RunAsAdministrator();
                    break;
            }
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e) => Provider.IsDirty = true;

        private void OnCloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs e) => _ = Provider.StopServerAsync();
    }
}
