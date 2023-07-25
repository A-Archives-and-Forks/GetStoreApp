using GetStoreApp.Extensions.DataType.Enums;
using GetStoreApp.Helpers.Controls.Extensions;
using GetStoreApp.Helpers.Root;
using GetStoreApp.Models.Controls.WinGet;
using GetStoreApp.Services.Controls.Settings.Common;
using GetStoreApp.Services.Root;
using GetStoreApp.UI.Dialogs.WinGet;
using GetStoreApp.UI.Notifications;
using GetStoreApp.Views.Pages;
using GetStoreApp.WindowsAPI.PInvoke.Kernel32;
using GetStoreApp.WindowsAPI.PInvoke.User32;
using Microsoft.Management.Deployment;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace GetStoreApp.UI.Controls.WinGet
{
    /// <summary>
    /// WinGet �����ҳ�棺����Ӧ�ÿؼ�
    /// </summary>
    public sealed partial class SearchAppsControl : Grid, INotifyPropertyChanged
    {
        private PackageManager SearchAppsManager { get; set; }

        internal WinGetPage WinGetInstance;

        private string cachedSearchText;

        private bool isInitialized = false;

        private bool _notSearched = true;

        public bool NotSearched
        {
            get { return _notSearched; }

            set
            {
                _notSearched = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NotSearched)));
            }
        }

        private bool _isSearchCompleted = false;

        public bool IsSearchCompleted
        {
            get { return _isSearchCompleted; }

            set
            {
                _isSearchCompleted = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSearchCompleted)));
            }
        }

        private string _searchText = string.Empty;

        public string SearchText
        {
            get { return _searchText; }

            set
            {
                _searchText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchText)));
            }
        }

        // ��װӦ��
        public XamlUICommand InstallCommand { get; } = new XamlUICommand();

        // ���ư�װ����
        public XamlUICommand CopyInstallTextCommand { get; } = new XamlUICommand();

        // ʹ�����װ
        public XamlUICommand InstallWithCmdCommand { get; } = new XamlUICommand();

        private List<MatchResult> MatchResultList;

        public ObservableCollection<SearchAppsModel> SearchAppsDataList { get; set; } = new ObservableCollection<SearchAppsModel>();

        public event PropertyChangedEventHandler PropertyChanged;

        public SearchAppsControl()
        {
            InitializeComponent();

            InstallCommand.ExecuteRequested += async (sender, args) =>
            {
                SearchAppsModel searchApps = args.Parameter as SearchAppsModel;
                if (searchApps is not null)
                {
                    try
                    {
                        // ���õ�ǰӦ�õĿɰ�װ״̬
                        foreach (SearchAppsModel searchAppsItem in SearchAppsDataList)
                        {
                            if (searchAppsItem.AppID == searchApps.AppID)
                            {
                                searchAppsItem.IsInstalling = true;
                                break;
                            }
                        }

                        InstallOptions installOptions = WinGetService.CreateInstallOptions();

                        installOptions.PackageInstallMode = (PackageInstallMode)Enum.Parse(typeof(PackageInstallMode), WinGetConfigService.WinGetInstallMode.InternalName);
                        installOptions.PackageInstallScope = PackageInstallScope.Any;

                        // ���°�װ����
                        Progress<InstallProgress> progressCallBack = new Progress<InstallProgress>((installProgress) =>
                        {
                            switch (installProgress.State)
                            {
                                // ���ڵȴ���״̬
                                case PackageInstallProgressState.Queued:
                                    {
                                        lock (WinGetInstance.InstallingAppsObject)
                                        {
                                            foreach (InstallingAppsModel installingItem in WinGetInstance.InstallingAppsList)
                                            {
                                                if (installingItem.AppID == searchApps.AppID)
                                                {
                                                    installingItem.InstallProgressState = PackageInstallProgressState.Queued;
                                                    break;
                                                }
                                            }
                                        }
                                        break;
                                    }
                                // ����������״̬
                                case PackageInstallProgressState.Downloading:
                                    {
                                        lock (WinGetInstance.InstallingAppsObject)
                                        {
                                            foreach (InstallingAppsModel installingItem in WinGetInstance.InstallingAppsList)
                                            {
                                                if (installingItem.AppID == searchApps.AppID)
                                                {
                                                    installingItem.InstallProgressState = PackageInstallProgressState.Downloading;
                                                    installingItem.DownloadProgress = Math.Round(installProgress.DownloadProgress * 100, 2); installingItem.DownloadedFileSize = Convert.ToString(FileSizeHelper.ConvertFileSizeToString(installProgress.BytesDownloaded));
                                                    installingItem.TotalFileSize = Convert.ToString(FileSizeHelper.ConvertFileSizeToString(installProgress.BytesRequired));
                                                    break;
                                                }
                                            }
                                        }
                                        break;
                                    }
                                // ���ڰ�װ��״̬
                                case PackageInstallProgressState.Installing:
                                    {
                                        lock (WinGetInstance.InstallingAppsObject)
                                        {
                                            foreach (InstallingAppsModel installingItem in WinGetInstance.InstallingAppsList)
                                            {
                                                if (installingItem.AppID == searchApps.AppID)
                                                {
                                                    installingItem.InstallProgressState = PackageInstallProgressState.Installing;
                                                    installingItem.DownloadProgress = 100;
                                                    break;
                                                }
                                            }
                                        }
                                        break;
                                    }
                                // ��װ��ɺ�ȴ���������״̬
                                case PackageInstallProgressState.PostInstall:
                                    {
                                        lock (WinGetInstance.InstallingAppsObject)
                                        {
                                            foreach (InstallingAppsModel installingItem in WinGetInstance.InstallingAppsList)
                                            {
                                                if (installingItem.AppID == searchApps.AppID)
                                                {
                                                    installingItem.InstallProgressState = PackageInstallProgressState.PostInstall;
                                                    break;
                                                }
                                            }
                                        }
                                        break;
                                    }
                                // ���ڰ�װ���״̬
                                case PackageInstallProgressState.Finished:
                                    {
                                        lock (WinGetInstance.InstallingAppsObject)
                                        {
                                            foreach (InstallingAppsModel installingItem in WinGetInstance.InstallingAppsList)
                                            {
                                                if (installingItem.AppID == searchApps.AppID)
                                                {
                                                    installingItem.InstallProgressState = PackageInstallProgressState.Finished;
                                                    installingItem.DownloadProgress = 100;
                                                    break;
                                                }
                                            }
                                        }
                                        break;
                                    }
                            }
                        });

                        // ����ȡ��ִ�в���
                        CancellationTokenSource installTokenSource = new CancellationTokenSource();

                        // �������
                        lock (WinGetInstance.InstallingAppsObject)
                        {
                            WinGetInstance.InstallingAppsList.Add(new InstallingAppsModel()
                            {
                                AppID = searchApps.AppID,
                                AppName = searchApps.AppName,
                                DownloadProgress = 0,
                                InstallProgressState = PackageInstallProgressState.Queued,
                                DownloadedFileSize = FileSizeHelper.ConvertFileSizeToString(0),
                                TotalFileSize = FileSizeHelper.ConvertFileSizeToString(0)
                            });
                            WinGetInstance.InstallingStateDict.Add(searchApps.AppID, installTokenSource);
                        }

                        InstallResult installResult = await SearchAppsManager.InstallPackageAsync(MatchResultList.Find(item => item.CatalogPackage.DefaultInstallVersion.Id == searchApps.AppID).CatalogPackage, installOptions).AsTask(installTokenSource.Token, progressCallBack);

                        // ��ȡ��װ��ɺ�Ľ����Ϣ
                        if (installResult.Status == InstallResultStatus.Ok)
                        {
                            AppNotificationService.Show(NotificationArgs.InstallSuccessfully, searchApps.AppName);

                            // ����Ƿ���Ҫ�����豸���Ӧ�õ�ж�أ�����ǣ�ѯ���û��Ƿ���Ҫ�����豸
                            if (installResult.RebootRequired)
                            {
                                ContentDialogResult Result = await ContentDialogHelper.ShowAsync(new RebootDialog(WinGetOptionArgs.UpgradeInstall, searchApps.AppName), this);
                                if (Result == ContentDialogResult.Primary)
                                {
                                    unsafe
                                    {
                                        Kernel32Library.GetStartupInfo(out STARTUPINFO RebootStartupInfo);
                                        RebootStartupInfo.lpReserved = null;
                                        RebootStartupInfo.lpDesktop = null;
                                        RebootStartupInfo.lpTitle = null;
                                        RebootStartupInfo.dwX = 0;
                                        RebootStartupInfo.dwY = 0;
                                        RebootStartupInfo.dwXSize = 0;
                                        RebootStartupInfo.dwYSize = 0;
                                        RebootStartupInfo.dwXCountChars = 500;
                                        RebootStartupInfo.dwYCountChars = 500;
                                        RebootStartupInfo.dwFlags = STARTF.STARTF_USESHOWWINDOW;
                                        RebootStartupInfo.wShowWindow = WindowShowStyle.SW_HIDE;
                                        RebootStartupInfo.cbReserved2 = 0;
                                        RebootStartupInfo.lpReserved2 = IntPtr.Zero;

                                        RebootStartupInfo.cb = Marshal.SizeOf(typeof(STARTUPINFO));
                                        bool createResult = Kernel32Library.CreateProcess(null, string.Format("{0} {1}", Path.Combine(InfoHelper.SystemDataPath.Windows, "System32", "Shutdown.exe"), "-r -t 120"), IntPtr.Zero, IntPtr.Zero, false, CreateProcessFlags.CREATE_NO_WINDOW, IntPtr.Zero, null, ref RebootStartupInfo, out PROCESS_INFORMATION RebootProcessInformation);

                                        if (createResult)
                                        {
                                            if (RebootProcessInformation.hProcess != IntPtr.Zero) Kernel32Library.CloseHandle(RebootProcessInformation.hProcess);
                                            if (RebootProcessInformation.hThread != IntPtr.Zero) Kernel32Library.CloseHandle(RebootProcessInformation.hThread);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            AppNotificationService.Show(NotificationArgs.InstallFailed, searchApps.AppName, searchApps.AppID);
                        }

                        // Ӧ�ð�װʧ�ܣ�����ǰ����״̬�޸�Ϊ�ɰ�װ״̬
                        foreach (SearchAppsModel searchAppsItem in SearchAppsDataList)
                        {
                            if (searchAppsItem.AppID == searchApps.AppID)
                            {
                                searchAppsItem.IsInstalling = false;
                                break;
                            }
                        }

                        // ������������������ɾ������
                        lock (WinGetInstance.InstallingAppsObject)
                        {
                            foreach (InstallingAppsModel installingAppsItem in WinGetInstance.InstallingAppsList)
                            {
                                if (installingAppsItem.AppID == searchApps.AppID)
                                {
                                    WinGetInstance.InstallingAppsList.Remove(installingAppsItem);
                                    break;
                                }
                            }
                            WinGetInstance.InstallingStateDict.Remove(searchApps.AppID);
                        }
                    }
                    // �������û���ȡ���쳣
                    catch (OperationCanceledException e)
                    {
                        LogService.WriteLog(LogType.INFO, "App installing operation canceled.", e);

                        // Ӧ�ð�װʧ�ܣ�����ǰ����״̬�޸�Ϊ�ɰ�װ״̬
                        foreach (SearchAppsModel searchAppsItem in SearchAppsDataList)
                        {
                            if (searchAppsItem.AppID == searchApps.AppID)
                            {
                                searchAppsItem.IsInstalling = false;
                                break;
                            }
                        }

                        // ������������������ɾ������
                        lock (WinGetInstance.InstallingAppsObject)
                        {
                            foreach (InstallingAppsModel installingAppsItem in WinGetInstance.InstallingAppsList)
                            {
                                if (installingAppsItem.AppID == searchApps.AppID)
                                {
                                    WinGetInstance.InstallingAppsList.Remove(installingAppsItem);
                                    break;
                                }
                            }
                            WinGetInstance.InstallingStateDict.Remove(searchApps.AppID);
                        }
                    }
                    // �����쳣
                    catch (Exception e)
                    {
                        LogService.WriteLog(LogType.ERROR, "App installing failed.", e);

                        // Ӧ�ð�װʧ�ܣ�����ǰ����״̬�޸�Ϊ�ɰ�װ״̬
                        foreach (SearchAppsModel searchAppsItem in SearchAppsDataList)
                        {
                            if (searchAppsItem.AppID == searchApps.AppID)
                            {
                                searchAppsItem.IsInstalling = false;
                                break;
                            }
                        }

                        // ������������������ɾ������
                        lock (WinGetInstance.InstallingAppsObject)
                        {
                            foreach (InstallingAppsModel installingAppsItem in WinGetInstance.InstallingAppsList)
                            {
                                if (installingAppsItem.AppID == searchApps.AppID)
                                {
                                    WinGetInstance.InstallingAppsList.Remove(installingAppsItem);
                                    break;
                                }
                            }
                            WinGetInstance.InstallingStateDict.Remove(searchApps.AppID);
                        }

                        AppNotificationService.Show(NotificationArgs.InstallFailed, searchApps.AppName, searchApps.AppID);
                    }
                }
            };

            CopyInstallTextCommand.ExecuteRequested += (sender, args) =>
            {
                string appId = args.Parameter as string;
                if (appId is not null)
                {
                    string copyContent = string.Format("winget install {0}", appId);
                    CopyPasteHelper.CopyToClipBoard(copyContent);

                    new WinGetCopyNotification(this, WinGetOptionArgs.SearchInstall).Show();
                }
            };

            InstallWithCmdCommand.ExecuteRequested += (sender, args) =>
            {
                string appId = args.Parameter as string;
                if (appId is not null)
                {
                    unsafe
                    {
                        Kernel32Library.GetStartupInfo(out STARTUPINFO WinGetProcessStartupInfo);
                        WinGetProcessStartupInfo.lpReserved = null;
                        WinGetProcessStartupInfo.lpDesktop = null;
                        WinGetProcessStartupInfo.lpTitle = null;
                        WinGetProcessStartupInfo.dwX = 0;
                        WinGetProcessStartupInfo.dwY = 0;
                        WinGetProcessStartupInfo.dwXSize = 0;
                        WinGetProcessStartupInfo.dwYSize = 0;
                        WinGetProcessStartupInfo.dwXCountChars = 500;
                        WinGetProcessStartupInfo.dwYCountChars = 500;
                        WinGetProcessStartupInfo.dwFlags = STARTF.STARTF_USESHOWWINDOW;
                        WinGetProcessStartupInfo.wShowWindow = WindowShowStyle.SW_SHOW;
                        WinGetProcessStartupInfo.cbReserved2 = 0;
                        WinGetProcessStartupInfo.lpReserved2 = IntPtr.Zero;
                        WinGetProcessStartupInfo.cb = Marshal.SizeOf(typeof(STARTUPINFO));

                        bool createResult = Kernel32Library.CreateProcess(null, string.Format("winget install {0}", appId), IntPtr.Zero, IntPtr.Zero, false, CreateProcessFlags.CREATE_NEW_CONSOLE, IntPtr.Zero, null, ref WinGetProcessStartupInfo, out PROCESS_INFORMATION WinGetProcessInformation);

                        if (createResult)
                        {
                            if (WinGetProcessInformation.hProcess != IntPtr.Zero) Kernel32Library.CloseHandle(WinGetProcessInformation.hProcess);
                            if (WinGetProcessInformation.hThread != IntPtr.Zero) Kernel32Library.CloseHandle(WinGetProcessInformation.hThread);
                        }
                    }
                }
            };
        }

        /// <summary>
        /// ���ػ�Ӧ������ͳ����Ϣ
        /// </summary>
        public string LocalizeSearchAppsCountInfo(int count)
        {
            if (count is 0)
            {
                return ResourceService.GetLocalized("WinGet/SearchedAppsCountEmpty");
            }
            else
            {
                return string.Format(ResourceService.GetLocalized("WinGet/SearchedAppsCountInfo"), count);
            }
        }

        public bool IsSearchBoxEnabled(bool notSearched, bool isSearchCompleted)
        {
            if (notSearched)
            {
                return true;
            }
            else
            {
                if (isSearchCompleted)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// ��ʼ������Ӧ������
        /// </summary>
        public void OnLoaded(object sender, RoutedEventArgs args)
        {
            if (!isInitialized)
            {
                try
                {
                    SearchAppsManager = WinGetService.CreatePackageManager();
                }
                catch (Exception e)
                {
                    LogService.WriteLog(LogType.ERROR, "Search apps information initialized failed.", e);
                    return;
                }
                finally
                {
                    isInitialized = true;
                }
            }
        }

        /// <summary>
        /// �����Ѱ�װӦ������
        /// </summary>
        public async void OnRefreshClicked(object sender, RoutedEventArgs args)
        {
            MatchResultList = null;
            IsSearchCompleted = false;
            await Task.Delay(500);
            if (string.IsNullOrEmpty(cachedSearchText))
            {
                IsSearchCompleted = true;
                return;
            }
            await GetSearchAppsAsync();
            InitializeData();
            IsSearchCompleted = true;
        }

        /// <summary>
        /// ������������ݼ���Ӧ��
        /// </summary>
        public async void OnQuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            cachedSearchText = SearchText;
            NotSearched = false;
            IsSearchCompleted = false;
            await Task.Delay(500);
            if (string.IsNullOrEmpty(cachedSearchText))
            {
                IsSearchCompleted = true;
                return;
            }
            await GetSearchAppsAsync();
            InitializeData();
            IsSearchCompleted = true;
        }

        /// <summary>
        /// ����Ӧ��
        /// </summary>
        public async Task GetSearchAppsAsync()
        {
            try
            {
                await Task.Run(async () =>
                {
                    List<PackageCatalogReference> packageCatalogReferences = SearchAppsManager.GetPackageCatalogs().ToList();
                    CreateCompositePackageCatalogOptions createCompositePackageCatalogOptions = WinGetService.CreateCreateCompositePackageCatalogOptions();
                    foreach (PackageCatalogReference catalogReference in packageCatalogReferences)
                    {
                        createCompositePackageCatalogOptions.Catalogs.Add(catalogReference);
                    }
                    PackageCatalogReference catalogRef = SearchAppsManager.CreateCompositePackageCatalog(createCompositePackageCatalogOptions);
                    ConnectResult connectResult = await catalogRef.ConnectAsync();
                    PackageCatalog searchCatalog = connectResult.PackageCatalog;

                    if (searchCatalog is not null)
                    {
                        FindPackagesOptions findPackagesOptions = WinGetService.CreateFindPackagesOptions();
                        PackageMatchFilter nameMatchFilter = WinGetService.CreatePacakgeMatchFilter();
                        // ����Ӧ�õ�����Ѱ�ҷ��������Ľ��
                        nameMatchFilter.Field = PackageMatchField.Name;
                        nameMatchFilter.Option = PackageFieldMatchOption.ContainsCaseInsensitive;
                        nameMatchFilter.Value = cachedSearchText;
                        findPackagesOptions.Filters.Add(nameMatchFilter);
                        FindPackagesResult findResult = await connectResult.PackageCatalog.FindPackagesAsync(findPackagesOptions);
                        MatchResultList = findResult.Matches.ToList();
                    }
                });
            }
            catch (Exception e)
            {
                LogService.WriteLog(LogType.WARNING, "Get search apps information failed.", e);
            }
        }

        /// <summary>
        /// ��ʼ���б�����
        /// </summary>
        public void InitializeData()
        {
            SearchAppsDataList.Clear();
            if (MatchResultList is not null)
            {
                foreach (MatchResult matchItem in MatchResultList)
                {
                    if (matchItem.CatalogPackage.DefaultInstallVersion is not null)
                    {
                        bool isInstalling = false;
                        foreach (InstallingAppsModel installingAppsItem in WinGetInstance.InstallingAppsList)
                        {
                            if (matchItem.CatalogPackage.DefaultInstallVersion.Id == installingAppsItem.AppID)
                            {
                                isInstalling = true;
                                break;
                            }
                        }
                        SearchAppsDataList.Add(new SearchAppsModel()
                        {
                            AppID = matchItem.CatalogPackage.DefaultInstallVersion.Id,
                            AppName = string.IsNullOrEmpty(matchItem.CatalogPackage.DefaultInstallVersion.DisplayName) || matchItem.CatalogPackage.DefaultInstallVersion.DisplayName.Equals("Unknown", StringComparison.OrdinalIgnoreCase) ? ResourceService.GetLocalized("WinGet/Unknown") : matchItem.CatalogPackage.DefaultInstallVersion.DisplayName,
                            AppPublisher = string.IsNullOrEmpty(matchItem.CatalogPackage.DefaultInstallVersion.Publisher) || matchItem.CatalogPackage.DefaultInstallVersion.Publisher.Equals("Unknown", StringComparison.OrdinalIgnoreCase) ? ResourceService.GetLocalized("WinGet/Unknown") : matchItem.CatalogPackage.DefaultInstallVersion.Publisher,
                            AppVersion = string.IsNullOrEmpty(matchItem.CatalogPackage.DefaultInstallVersion.Version) || matchItem.CatalogPackage.DefaultInstallVersion.Version.Equals("Unknown", StringComparison.OrdinalIgnoreCase) ? ResourceService.GetLocalized("WinGet/Unknown") : matchItem.CatalogPackage.DefaultInstallVersion.Version,
                            IsInstalling = isInstalling,
                        });
                    }
                }
            }
        }
    }
}
