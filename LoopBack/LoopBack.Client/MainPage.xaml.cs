using LoopBack.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace LoopBack.Client
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private LoopUtil _loop;
        private readonly ObservableCollection<AppContainer> appFiltered = new();
        private bool isDirty = false;

        public MainPage()
        {
            InitializeComponent();
            dgLoopback.ItemsSource = appFiltered;
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequested;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Log("Loading...");
            await ThreadSwitcher.ResumeBackgroundAsync();
            _loop = LoopBackProjectionFactory.TryCreateLoopUtil();
            await Filter(string.Empty, false, null);
            Log("Loaded");
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Log("Refreshing...");
            await Filter(string.Empty, false, null);
            txtFilter.Text = "";
            Loopback_Enabled.IsChecked = false;
            Loopback_Disabled.IsChecked = false;
            isDirty = false;
            Log("Refreshed");
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!isDirty)
            {
                Log("Nothing to save");
                return;
            }

            isDirty = false;
            IEnumerable<string> enableList = appFiltered.Where(x => x.LoopUtil).Select(x => x.AppContainerSid);
            if (_loop.SetLoopbackList(enableList))
            {
                Log("Saved loopback excemptions");
            }
            else
            {
                Log("ERROR SAVING");
            }
        }

        private void DgcbLoop_Click(object sender, RoutedEventArgs e)
        {
            isDirty = true;
        }

        private async Task Filter(string filter, bool Ischecked, bool? IsEnabled)
        {
            await ThreadSwitcher.ResumeBackgroundAsync();
            string appsInFilter = filter.ToUpper();
            IEnumerable<AppContainer> apps = _loop.GetAppContainers();
            await Dispatcher.ResumeForegroundAsync();
            appFiltered.Clear();
            foreach (AppContainer app in apps)
            {
                string appName = app.DisplayName.ToString().ToUpper();

                if (string.IsNullOrEmpty(filter) || appName.Contains(appsInFilter))
                {
                    if (Ischecked == false || app.LoopUtil == IsEnabled)
                    {
                        appFiltered.Add(app);
                    }
                }
            }
        }

        private async void Log(string logtxt)
        {
            if (!Dispatcher.HasThreadAccess) { await Dispatcher.ResumeForegroundAsync(); }
            txtStatus.Text = DateTime.Now.ToString("hh:mm:ss.fff ") + logtxt;
        }

        private void Loopback_Click_Disabled(object sender, RoutedEventArgs e)
        {
            _ = Filter(txtFilter.Text, (bool)Loopback_Disabled.IsChecked, false);
            Loopback_Enabled.IsChecked = false;
        }

        private void Loopback_Click_Enabled(object sender, RoutedEventArgs e)
        {
            _ = Filter(txtFilter.Text, (bool)Loopback_Enabled.IsChecked, true);
            Loopback_Disabled.IsChecked = false;
        }

        private void TxtFilter_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            bool isEnabledChecked = (bool)Loopback_Enabled.IsChecked;
            _ = isEnabledChecked
                ? Filter(txtFilter.Text, (bool)Loopback_Enabled.IsChecked, isEnabledChecked)
                : Filter(txtFilter.Text, (bool)Loopback_Disabled.IsChecked, isEnabledChecked);
        }

        private void OnCloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            //_ = _loop.StopService();
        }
    }
}
