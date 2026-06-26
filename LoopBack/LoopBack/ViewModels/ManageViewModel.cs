using CommunityToolkit.WinUI.Controls;
using LoopBack.Common;
using LoopBack.Helpers;
using LoopBack.Metadata;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using Windows.Win32.UI.Shell;

namespace LoopBack.ViewModels
{
    public partial class ManageViewModel(CoreDispatcher dispatcher) : INotifyPropertyChanged
    {
        private static readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("ManagePage");

        private LoopUtil loopUtil;
        private TaskbarProgress taskbar;

        private bool IsLoading
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    taskbar?.SetProgressState(value ? TBPFLAG.TBPF_INDETERMINATE : TBPFLAG.TBPF_NOPROGRESS);
                }
            }
        }

        public const bool IsFullTrust = LoopBackProjectionFactory.IsFullTrust;
        public static DataGridRowDetailsVisibilityMode[] DataGridRowDetailsVisibilityModes => Enum.GetValues<DataGridRowDetailsVisibilityMode>();

        public CoreDispatcher Dispatcher => dispatcher;

        public string CachedSortedColumn { get; set; }
        public VectorViewReader<AppContainer> AppContainers { get; private set; }

        public bool IsDirty
        {
            get;
            set => SetProperty(ref field, value);
        }

        public bool IsRunAsAdministrator
        {
            get;
            set => SetProperty(ref field, value);
        }

        public string Message
        {
            get;
            set => SetProperty(ref field, value);
        } = string.Empty;

        public ObservableCollection<AppContainer> FilteredAppContainers
        {
            get;
            set => SetProperty(ref field, value);
        } = [];

        public bool IsExemptAll => FilteredAppContainers.Count > 0 && FilteredAppContainers.All(x => x.IsEnableLoop);

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
                if (IsLoading) { return; }
                IsLoading = true;
                ShowLocalizedMessage("Loading");
                await ThreadSwitcher.ResumeBackgroundAsync();
                if (loopUtil == null)
                {
                    if (IsFullTrust || LoopBackProjectionFactory.ServerManager is not ServerManager serverManager)
                    {
                        taskbar = await TaskbarProgress.GetForDispatcher(Dispatcher);
                        taskbar.SetProgressState(TBPFLAG.TBPF_INDETERMINATE);
                        loopUtil = new LoopUtil();
                    }
                    else
                    {
                        taskbar = await TaskbarProgress.GetForDispatcher(serverManager.GetTaskbarList(), Dispatcher);
                        taskbar.SetProgressState(TBPFLAG.TBPF_INDETERMINATE);
                        loopUtil = serverManager.GetLoopUtil();
                        IsRunAsAdministrator = serverManager.IsRunAsAdministrator;
                    }
                }
                if (loopUtil != null)
                {
                    AppContainers = new(loopUtil.GetAppContainers());
                    await Dispatcher.AwaitableRunAsync(FilteredAppContainers.Clear);
                    await FilteredAppContainers.AddRangeAsync(AppContainers, Dispatcher);
                    ShowLocalizedMessage("Loaded");
                }
                else
                {
                    ShowLocalizedMessage("LoadFailed");
                }
            }
            catch (Exception ex) when (ex.HResult == -2147023174)
            {
                taskbar = null;
                loopUtil = null;
                AppContainers = null;
                FilteredAppContainers = null;
                IsDirty = IsRunAsAdministrator = false;
                SettingsHelper.LoggerFactory.CreateLogger<ManageViewModel>().LogWarning(ex, "The loop utility is offline. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
                ShowMessage(ex.Message);
            }
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger<ManageViewModel>().LogError(ex, "Failed to refresh manage page. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
                ShowMessage(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task FilterDataAsync(string filter)
        {
            try
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
                        ShowLocalizedMessage("Filtering");
                        string appsInFilter = filter;
                        await Dispatcher.AwaitableRunAsync(FilteredAppContainers.Clear);
                        foreach (AppContainer app in AppContainers)
                        {
                            if (app != null)
                            {
                                string appName = app.DisplayName;
                                string packageFullName = app.PackageFullName;

                                if (appName.Contains(appsInFilter, StringComparison.OrdinalIgnoreCase)
                                    || packageFullName.Contains(appsInFilter, StringComparison.OrdinalIgnoreCase))
                                {
                                    await Dispatcher.AwaitableRunAsync(() => FilteredAppContainers.Add(app));
                                }
                            }
                        }
                        ShowLocalizedMessage("Filtered");
                    }
                }
                finally
                {
                    RaisePropertyChangedEvent(nameof(IsExemptAll));
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger<ManageViewModel>().LogError(ex, "Failed to filter data. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
                ShowMessage(ex.Message);
            }
        }

        public async Task SortDataAsync(string sortBy, bool ascending)
        {
            try
            {
                ShowLocalizedMessage("Sorting");
                await ThreadSwitcher.ResumeBackgroundAsync();
                CachedSortedColumn = sortBy;
                AppContainer[] temp = [.. FilteredAppContainers];
                await Dispatcher.AwaitableRunAsync(FilteredAppContainers.Clear);
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
                ShowLocalizedMessage("Sorted");
            }
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger<ManageViewModel>().LogError(ex, "Failed to sort data. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
                ShowMessage(ex.Message);
            }
        }

        public async Task ExemptAllAsync(bool isChecked)
        {
            try
            {
                ShowLocalizedMessage("Switching");
                await ThreadSwitcher.ResumeBackgroundAsync();
                foreach (AppContainer app in FilteredAppContainers)
                {
                    app?.IsEnableLoop = isChecked;
                }
                FilteredAppContainers = [.. FilteredAppContainers];
                IsDirty = true;
                ShowLocalizedMessage("Switched");
            }
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger<ManageViewModel>().LogError(ex, "Failed to exempt all. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
                ShowMessage(ex.Message);
            }
        }

        public async Task SaveConfigureAsync()
        {
            try
            {
                await ThreadSwitcher.ResumeBackgroundAsync();

                if (!IsDirty)
                {
                    ShowLocalizedMessage("NothingToSave");
                    return;
                }

                IsDirty = false;
                AppContainer[] enableList = [.. AppContainers.Where(x => x.IsEnableLoop)];
                if (loopUtil.SetLoopbackList(enableList) is Exception exception)
                {
                    SettingsHelper.LoggerFactory.CreateLogger<ManageViewModel>().LogError(exception, "Failed to saving data. {message} (0x{hResult:X})", exception.GetMessage(), exception.HResult);
                    ShowLocalizedMessage("ErrorSavingFormat", exception.Message);
                }
                else
                {
                    ShowLocalizedMessage("SavedLoopbackExemptions");
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger<ManageViewModel>().LogError(ex, "Failed to save configure. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
                ShowMessage(ex.Message);
            }
        }

        public async Task RunAsAdministratorAsync()
        {
            try
            {
                await ThreadSwitcher.ResumeBackgroundAsync();
                if (!IsRunAsAdministrator)
                {
                    ShowLocalizedMessage("TryRunAsAdministrator");
                    try
                    {
                        LoopBackProjectionFactory.ServerManager.RunAsAdministrator();
                    }
                    catch (Exception ex) when (ex.HResult == -2147023170)
                    {
                        ServerManager serverManager = LoopBackProjectionFactory.ServerManager;
                        loopUtil = serverManager.GetLoopUtil();
                        taskbar = await TaskbarProgress.GetForDispatcher(serverManager.GetTaskbarList(), Dispatcher);
                        IsRunAsAdministrator = serverManager.IsRunAsAdministrator;
                        AppContainers = new(loopUtil.GetAppContainers());
                        await Dispatcher.AwaitableRunAsync(FilteredAppContainers.Clear);
                        await FilteredAppContainers.AddRangeAsync(AppContainers, Dispatcher);
                        ShowLocalizedMessage(IsRunAsAdministrator ? "RunAsAdministratorNow" : "FailedRunAsAdministrator");
                        return;
                    }
                    ShowLocalizedMessage("FailedRunAsAdministrator");
                }
                else
                {
                    ShowLocalizedMessage("AlreadyRunAsAdministrator");
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger<ManageViewModel>().LogError(ex, "Failed to run as administrator. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
                ShowMessage(ex.Message);
            }
        }

        public void ShowMessage(string log) => Message = $"{DateTime.Now:hh:mm:ss.fff} {log}";

        public void ShowLocalizedMessage(string resourceKey) => ShowMessage(_loader.GetString(resourceKey));

        public void ShowLocalizedMessage([ConstantExpected] string resourceKey, string arg0) => ShowMessage(string.Format(_loader.GetString(resourceKey), arg0));

        public static string GetLocalizedString(DataGridRowDetailsVisibilityMode mode) => _loader.GetString(mode.ToString());
    }
}
