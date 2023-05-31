using LoopBack.Client.Helpers;
using LoopBack.Client.ViewModels;
using LoopBack.Metadata;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace LoopBack.Client.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ManagePage : Page
    {
        private readonly ManageViewModel Provider = new();

        public ManagePage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequested;
            if (DataContext is not ManageViewModel)
            {
                DataContext = Provider;
                _ = Provider.Refresh();
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            _ = Provider.SelectAll(checkBox.IsChecked.Value);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag?.ToString())
            {
                case "Save":
                    _ = Provider.SaveConfigure();
                    break;
                case "Refresh":
                    _ = Provider.Refresh();
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
                ObservableCollection<string> observableCollection = new();
                sender.ItemsSource = observableCollection;
                await ThreadSwitcher.ResumeBackgroundAsync();
                if (!string.IsNullOrEmpty(filter))
                {
                    string appsInFilter = filter.ToUpper();
                    foreach (AppContainer app in Provider.AppContainers)
                    {
                        if (app != null)
                        {
                            string appName = app.DisplayName.ToUpper();
                            if (appName.Contains(appsInFilter))
                            {
                                await Dispatcher.TryRunAsync(
                                    CoreDispatcherPriority.Normal,
                                    () => observableCollection.Add(app.ToString()));
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
                _ = (Provider?.Filter(word));
            }
            else if (args.ChosenSuggestion is null)
            {
                _ = (Provider?.Filter(sender.Text));
            }
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e) => Provider.IsDirty = true;

        private void OnCloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs e) => _ = Provider.StopService();
    }
}
