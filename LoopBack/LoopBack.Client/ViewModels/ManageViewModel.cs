﻿using LoopBack.Client.Helpers;
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

        public DispatcherQueue Dispatcher { get; } = DispatcherQueue.GetForCurrentThread();

        public string CachedSortedColumn { get; set; }
        public IEnumerable<AppContainer> AppContainers { get; private set; }

        private bool isDirty;
        public bool IsDirty
        {
            get => isDirty;
            set => SetProperty(ref isDirty, value);
        }

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

        protected async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                if (Dispatcher is DispatcherQueue dispatcher
                    && !(ThreadSwitcher.IsHasThreadAccessPropertyAvailable
                    && (dispatcher?.HasThreadAccess) != false))
                {
                    await Dispatcher.ResumeForegroundAsync();
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        protected void SetProperty<TProperty>(ref TProperty property, TProperty value, [CallerMemberName] string name = null)
        {
            if (property == null ? value != null : !property.Equals(value))
            {
                property = value;
                RaisePropertyChangedEvent(name);
            }
        }

        public async Task Refresh()
        {
            try
            {
                ShowMessage("Loading...");
                await ThreadSwitcher.ResumeBackgroundAsync();
                loopUtil ??= LoopBackProjectionFactory.TryCreateLoopUtil();
                AppContainers = loopUtil.GetAppContainers();
                FilteredAppContainers = new(AppContainers);
                ShowMessage("Loaded");
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(ManageViewModel)).Error(ex.ExceptionToMessage());
                ShowMessage(ex.Message);
            }
        }

        public async Task FilterDataAsync(string filter)
        {
            try
            {
                await ThreadSwitcher.ResumeBackgroundAsync();
                bool notFiltering = string.IsNullOrWhiteSpace(filter);
                if (!notFiltering) { ShowMessage("Filtering..."); }
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

        public async Task SortDataAsync(string sortBy, bool ascending)
        {
            try
            {
                ShowMessage("Sorting...");
                await ThreadSwitcher.ResumeBackgroundAsync();
                CachedSortedColumn = sortBy;
                switch (sortBy)
                {
                    case "IsEnableLoop":
                        FilteredAppContainers = ascending
                            ? new(filteredAppContainers.OrderBy(item => item.IsEnableLoop))
                            : new(filteredAppContainers.OrderByDescending(item => item.IsEnableLoop));
                        break;
                    case "DisplayName":
                        FilteredAppContainers = ascending
                            ? new(filteredAppContainers.OrderBy(item => item.DisplayName))
                            : new(filteredAppContainers.OrderByDescending(item => item.DisplayName));
                        break;
                    case "AppContainerName":
                        FilteredAppContainers = ascending
                            ? new(filteredAppContainers.OrderBy(item => item.AppContainerName))
                            : new(filteredAppContainers.OrderByDescending(item => item.AppContainerName));
                        break;
                    case "WorkingDirectory":
                        FilteredAppContainers = ascending
                            ? new(filteredAppContainers.OrderBy(item => item.WorkingDirectory))
                            : new(filteredAppContainers.OrderByDescending(item => item.WorkingDirectory));
                        break;
                    case "PackageFullName":
                        FilteredAppContainers = ascending
                            ? new(filteredAppContainers.OrderBy(item => item.PackageFullName))
                            : new(filteredAppContainers.OrderByDescending(item => item.PackageFullName));
                        break;
                    case "Range":
                        FilteredAppContainers = ascending
                            ? new(filteredAppContainers.OrderBy(item => item.AppContainerSid))
                            : new(filteredAppContainers.OrderByDescending(item => item.AppContainerSid));
                        break;
                    case "UserSid":
                        FilteredAppContainers = ascending
                            ? new(filteredAppContainers.OrderBy(item => item.UserSid))
                            : new(filteredAppContainers.OrderByDescending(item => item.UserSid));
                        break;
                    default:
                        break;
                }
                ShowMessage("Sorted...");
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(ManageViewModel)).Error(ex.ExceptionToMessage());
                ShowMessage(ex.Message);
            }
        }

        public async Task ExemptAllAsync(bool isChecked)
        {
            try
            {
                ShowMessage("Switching...");
                await ThreadSwitcher.ResumeBackgroundAsync();
                foreach (AppContainer app in AppContainers)
                {
                    if (app != null)
                    {
                        app.IsEnableLoop = isChecked;
                    }
                }
                FilteredAppContainers = new(FilteredAppContainers);
                ShowMessage("Switched");
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(ManageViewModel)).Error(ex.ExceptionToMessage());
                ShowMessage(ex.Message);
            }
        }

        public async Task SaveConfigureAsync()
        {
            try
            {
                await ThreadSwitcher.ResumeBackgroundAsync();

                if (!isDirty)
                {
                    ShowMessage("Nothing to save");
                    return;
                }

                IsDirty = false;
                IEnumerable<string> enableList = AppContainers.Where(x => x.IsEnableLoop).Select(x => x.AppContainerSid);
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

        public IAsyncAction StopService() => loopUtil.StopServiceAsync();
    }
}
