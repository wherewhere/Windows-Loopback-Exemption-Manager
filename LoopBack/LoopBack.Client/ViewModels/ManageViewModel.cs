using LoopBack.Client.Helpers;
using LoopBack.Metadata;
using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;

namespace LoopBack.Client.ViewModels
{
    public class ManageViewModel : INotifyPropertyChanged
    {
        private LoopUtil loopUtil;

        private readonly DispatcherQueue Dispatcher = DispatcherQueue.GetForCurrentThread();

        public bool IsDirty { get; set; }
        public IEnumerable<AppContainer> AppContainers { get; private set; }

        private string message = string.Empty;
        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }

        private ObservableCollection<AppContainer> filteredAppContainers = new();
        public ObservableCollection<AppContainer> FilteredAppContainers
        {
            get => filteredAppContainers;
            set => SetProperty(ref filteredAppContainers, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>(ref T property, T value, [CallerMemberName] string name = null)
        {
            if (name == null || property is null ? value is null : property.Equals(value)) { return; }
            property = value;
            _ = Dispatcher.EnqueueAsync(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)));
        }

        public async Task Refresh()
        {
            try
            {
                ShowMessage("Loading...");
                await ThreadSwitcher.ResumeBackgroundAsync();
                loopUtil ??= LoopBackProjectionFactory.TryCreateLoopUtil();
                AppContainers = loopUtil.GetAppContainers();
                await Filter(string.Empty);
                ShowMessage("Loaded");
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(ManageViewModel)).Error(ex.ExceptionToMessage());
                ShowMessage(ex.Message);
            }
        }

        public async Task Filter(string filter)
        {
            try
            {
                bool notFiltering = string.IsNullOrWhiteSpace(filter);
                if (!notFiltering) { ShowMessage("Filtering..."); }
                await ThreadSwitcher.ResumeBackgroundAsync();
                string appsInFilter = filter.ToUpper();
                await Dispatcher.EnqueueAsync(filteredAppContainers.Clear);
                foreach (AppContainer app in AppContainers)
                {
                    if (app != null)
                    {
                        string appName = app.DisplayName.ToUpper();

                        if (notFiltering || appName.Contains(appsInFilter))
                        {
                            await Dispatcher.EnqueueAsync(() => filteredAppContainers.Add(app));
                        }
                    }
                }
                if (!notFiltering) { ShowMessage("Filtered"); }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(ManageViewModel)).Error(ex.ExceptionToMessage());
                ShowMessage(ex.Message);
            }
        }

        public async Task SelectAll(bool isChecked)
        {
            try
            {
                ShowMessage("Switching...");
                await ThreadSwitcher.ResumeBackgroundAsync();
                foreach (AppContainer app in AppContainers)
                {
                    if (app != null)
                    {
                        app.LoopUtil = isChecked;
                    }
                }
                await Dispatcher.EnqueueAsync(() => FilteredAppContainers = new(FilteredAppContainers));
                ShowMessage("Switched");
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(ManageViewModel)).Error(ex.ExceptionToMessage());
                ShowMessage(ex.Message);
            }
        }

        public async Task SaveConfigure()
        {
            try
            {
                await ThreadSwitcher.ResumeBackgroundAsync();

                if (!IsDirty)
                {
                    ShowMessage("Nothing to save");
                    return;
                }

                IsDirty = false;
                IEnumerable<string> enableList = AppContainers.Where(x => x.LoopUtil).Select(x => x.AppContainerSid);
                if (loopUtil.SetLoopbackList(enableList))
                {
                    ShowMessage("Saved loopback exemptions");
                }
                else
                {
                    ShowMessage("ERROR SAVING");
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(ManageViewModel)).Error(ex.ExceptionToMessage());
                ShowMessage(ex.Message);
            }
        }

        public void ShowMessage(string log) => Message = $"{DateTime.Now:hh:mm:ss.fff} {log}";

        public IAsyncAction StopService() => loopUtil.StopService();
    }
}
