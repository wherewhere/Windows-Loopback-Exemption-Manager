using LoopBack.Common;
using LoopBack.Helpers;
using LoopBack.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace LoopBack.ViewModels
{
    public partial class ManageViewModel(CoreDispatcher dispatcher) : INotifyPropertyChanged
    {
        private bool isLoading;
        private LoopUtil loopUtil;

        public CoreDispatcher Dispatcher => dispatcher;

        public string CachedSortedColumn { get; set; }
        public VectorViewReader<AppContainer> AppContainers { get; private set; }

        private bool isDirty;
        public bool IsDirty
        {
            get => isDirty;
            set => SetProperty(ref isDirty, value);
        }

        private bool isFullTrust = true;
        public bool IsFullTrust
        {
            get => isFullTrust;
            set => SetProperty(ref isFullTrust, value);
        }

        private bool isRunAsAdministrator;
        public bool IsRunAsAdministrator
        {
            get => isRunAsAdministrator;
            set => SetProperty(ref isRunAsAdministrator, value);
        }

        private string message = string.Empty;
        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }

        private ObservableCollection<AppContainer> filteredAppContainers = [];
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
                await Dispatcher.ResumeForegroundAsync();
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
                if (isLoading) { return; }
                isLoading = true;
                ShowMessage("Loading...");
                await ThreadSwitcher.ResumeBackgroundAsync();
                loopUtil ??= isFullTrust ? new LoopUtil() : LoopBackProjectionFactory.ServerManager.GetLoopUtil();
                if (loopUtil != null)
                {
                    IsRunAsAdministrator = !isFullTrust && LoopBackProjectionFactory.ServerManager.IsRunAsAdministrator;
                    AppContainers = new(loopUtil.GetAppContainers());
                    await Dispatcher.AwaitableRunAsync(FilteredAppContainers.Clear);
                    await FilteredAppContainers.AddRangeAsync(AppContainers, Dispatcher);
                    ShowMessage("Loaded");
                }
                else
                {
                    ShowMessage("Load failed");
                }
            }
            catch (Exception ex) when (ex.HResult == -2147023174)
            {
                loopUtil = null;
                AppContainers = null;
                FilteredAppContainers = null;
                IsDirty = IsRunAsAdministrator = false;
                SettingsHelper.LogManager.GetLogger(nameof(ManageViewModel)).Warn(ex.ExceptionToMessage());
                ShowMessage(ex.Message);
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(ManageViewModel)).Error(ex.ExceptionToMessage());
                ShowMessage(ex.Message);
            }
            finally
            {
                isLoading = false;
            }
        }

        public async Task FilterDataAsync(string filter)
        {
            try
            {
                await ThreadSwitcher.ResumeBackgroundAsync();
                if (string.IsNullOrWhiteSpace(filter))
                {
                    await Dispatcher.AwaitableRunAsync(FilteredAppContainers.Clear);
                    await FilteredAppContainers.AddRangeAsync(AppContainers, Dispatcher);
                    return;
                }
                else
                {
                    ShowMessage("Filtering...");
                    string appsInFilter = filter;
                    await Dispatcher.AwaitableRunAsync(filteredAppContainers.Clear);
                    foreach (AppContainer app in AppContainers)
                    {
                        if (app != null)
                        {
                            string appName = app.DisplayName;
                            string packageFullName = app.PackageFullName;

                            if (appName.Contains(appsInFilter, StringComparison.OrdinalIgnoreCase)
                                || packageFullName.Contains(appsInFilter, StringComparison.OrdinalIgnoreCase))
                            {
                                await Dispatcher.AwaitableRunAsync(() => filteredAppContainers.Add(app));
                            }
                        }
                    }
                    ShowMessage("Filtered");
                }
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
                AppContainer[] temp = [.. filteredAppContainers];
                await Dispatcher.AwaitableRunAsync(filteredAppContainers.Clear);
                switch (sortBy)
                {
                    case "IsEnableLoop":
                        await FilteredAppContainers.AddRangeAsync(ascending
                            ? temp.OrderBy(item => item.IsEnableLoop)
                            : temp.OrderByDescending(item => item.IsEnableLoop), Dispatcher);
                        break;
                    case "DisplayName":
                        await FilteredAppContainers.AddRangeAsync(ascending
                            ? temp.OrderBy(item => item.DisplayName)
                            : temp.OrderByDescending(item => item.DisplayName), Dispatcher);
                        break;
                    case "AppContainerName":
                        await FilteredAppContainers.AddRangeAsync(ascending
                            ? temp.OrderBy(item => item.AppContainerName)
                            : temp.OrderByDescending(item => item.AppContainerName), Dispatcher);
                        break;
                    case "WorkingDirectory":
                        await FilteredAppContainers.AddRangeAsync(ascending
                            ? temp.OrderBy(item => item.WorkingDirectory)
                            : temp.OrderByDescending(item => item.WorkingDirectory), Dispatcher);
                        break;
                    case "PackageFullName":
                        await FilteredAppContainers.AddRangeAsync(ascending
                            ? temp.OrderBy(item => item.PackageFullName)
                            : temp.OrderByDescending(item => item.PackageFullName), Dispatcher);
                        break;
                    case "Range":
                        await FilteredAppContainers.AddRangeAsync(ascending
                            ? temp.OrderBy(item => item.AppContainerSid)
                            : temp.OrderByDescending(item => item.AppContainerSid), Dispatcher);
                        break;
                    case "UserSid":
                        await FilteredAppContainers.AddRangeAsync(ascending
                            ? temp.OrderBy(item => item.UserSid)
                            : temp.OrderByDescending(item => item.UserSid), Dispatcher);
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
                foreach (AppContainer app in FilteredAppContainers)
                {
                    if (app != null)
                    {
                        app.IsEnableLoop = isChecked;
                    }
                }
                FilteredAppContainers = [.. FilteredAppContainers];
                IsDirty = true;
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
                IEnumerable<AppContainer> enableList = AppContainers.Where(x => x.IsEnableLoop);
                if (loopUtil.SetLoopbackList(enableList) is Exception exception)
                {
                    SettingsHelper.LogManager.GetLogger(nameof(ManageViewModel)).Error(exception.ExceptionToMessage());
                    ShowMessage($"ERROR SAVING: {exception.Message}");
                }
                else
                {
                    ShowMessage("Saved loopback exemptions");
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(ManageViewModel)).Error(ex.ExceptionToMessage());
                ShowMessage(ex.Message);
            }
        }

        public async Task RunAsAdministratorAsync()
        {
            try
            {
                await ThreadSwitcher.ResumeBackgroundAsync();
                if (!isRunAsAdministrator)
                {
                    ShowMessage("Try to run as administrator");
                    try
                    {
                        LoopBackProjectionFactory.ServerManager.RunAsAdministrator();
                    }
                    catch (Exception ex) when (ex.HResult == -2147023170)
                    {
                        loopUtil = LoopBackProjectionFactory.ServerManager.GetLoopUtil();
                        IsRunAsAdministrator = LoopBackProjectionFactory.ServerManager.IsRunAsAdministrator;
                        AppContainers = new(loopUtil.GetAppContainers());
                        await Dispatcher.AwaitableRunAsync(FilteredAppContainers.Clear);
                        await FilteredAppContainers.AddRangeAsync(AppContainers, Dispatcher);
                        if (isRunAsAdministrator)
                        {
                            ShowMessage("Run as administrator now");
                        }
                        else
                        {
                            ShowMessage("Failed to run as administrator");
                        }
                        return;
                    }
                    ShowMessage("Failed to run as administrator");
                }
                else
                {
                    ShowMessage("Already run as administrator");
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(ManageViewModel)).Error(ex.ExceptionToMessage());
                ShowMessage(ex.Message);
            }
        }

        public void ShowMessage(string log) => Message = $"{DateTime.Now:hh:mm:ss.fff} {log}";
    }
}
