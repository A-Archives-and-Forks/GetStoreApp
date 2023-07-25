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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace GetStoreApp.UI.Controls.WinGet
{
    public sealed partial class UpgradableAppsControl : Grid, INotifyPropertyChanged
    {
        private PackageManager UpgradableAppsManager { get; set; }

        internal WinGetPage WinGetInstance;

        private bool isInitialized = false;

        private bool _isLoadedCompleted = false;

        public bool IsLoadedCompleted
        {
            get { return _isLoadedCompleted; }

            set
            {
                _isLoadedCompleted = value;
                OnPropertyChanged();
            }
        }

        private bool _isUpgradableAppsEmpty;

        public bool IsUpgradableAppsEmpty
        {
            get { return _isUpgradableAppsEmpty; }

            set
            {
                _isUpgradableAppsEmpty = value;
                OnPropertyChanged();
            }
        }

        private List<MatchResult> MatchResultList;

        // Ӧ������
        public XamlUICommand UpdateCommand { get; } = new XamlUICommand();

        // ������������
        public XamlUICommand CopyUpgradeTextCommand { get; } = new XamlUICommand();

        // ʹ�������а�װ
        public XamlUICommand InstallWithCmdCommand { get; } = new XamlUICommand();

        public ObservableCollection<UpgradableAppsModel> UpgradableAppsDataList { get; } = new ObservableCollection<UpgradableAppsModel>();

        public event PropertyChangedEventHandler PropertyChanged;

        public UpgradableAppsControl()
        {
            InitializeComponent();

            UpdateCommand.ExecuteRequested += async (sender, args) =>
            {
                UpgradableAppsModel upgradableApps = args.Parameter as UpgradableAppsModel;
                if (upgradableApps is not null)
                {
                    try
                    {
                        // ���õ�ǰӦ�õĿ�����״̬
                        foreach (UpgradableAppsModel upgradableAppsItem in UpgradableAppsDataList)
                        {
                            if (upgradableAppsItem.AppID == upgradableApps.AppID)
                            {
                                upgradableAppsItem.IsUpgrading = true;
                                break;
                            }
                        }

                        InstallOptions installOptions = WinGetService.CreateInstallOptions();

                        installOptions.PackageInstallMode = (PackageInstallMode)Enum.Parse(typeof(PackageInstallMode), WinGetConfigService.WinGetInstallMode.InternalName);
                        installOptions.PackageInstallScope = PackageInstallScope.Any;

                        // ������������
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
                                                if (installingItem.AppID == upgradableApps.AppID)
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
                                                if (installingItem.AppID == upgradableApps.AppID)
                                                {
                                                    installingItem.InstallProgressState = PackageInstallProgressState.Downloading;
                                                    installingItem.DownloadProgress = Math.Round(installProgress.DownloadProgress * 100, 2);
                                                    installingItem.DownloadedFileSize = Convert.ToString(FileSizeHelper.ConvertFileSizeToString(installProgress.BytesDownloaded));
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
                                                if (installingItem.AppID == upgradableApps.AppID)
                                                {
                                                    installingItem.InstallProgressState = PackageInstallProgressState.Installing;
                                                    installingItem.DownloadProgress = 100;
                                                    break;
                                                }
                                            }
                                        }
                                        break;
                                    }
                                // ����״̬
                                case PackageInstallProgressState.PostInstall:
                                    {
                                        lock (WinGetInstance.InstallingAppsObject)
                                        {
                                            foreach (InstallingAppsModel installingItem in WinGetInstance.InstallingAppsList)
                                            {
                                                if (installingItem.AppID == upgradableApps.AppID)
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
                                                if (installingItem.AppID == upgradableApps.AppID)
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
                        CancellationTokenSource upgradeTokenSource = new CancellationTokenSource();

                        // �������
                        lock (WinGetInstance.InstallingAppsObject)
                        {
                            WinGetInstance.InstallingAppsList.Add(new InstallingAppsModel()
                            {
                                AppID = upgradableApps.AppID,
                                AppName = upgradableApps.AppName,
                                DownloadProgress = 0,
                                InstallProgressState = PackageInstallProgressState.Queued,
                                DownloadedFileSize = FileSizeHelper.ConvertFileSizeToString(0),
                                TotalFileSize = FileSizeHelper.ConvertFileSizeToString(0)
                            });
                            WinGetInstance.InstallingStateDict.Add(upgradableApps.AppID, upgradeTokenSource);
                        }

                        InstallResult installResult = await UpgradableAppsManager.UpgradePackageAsync(MatchResultList.Find(item => item.CatalogPackage.DefaultInstallVersion.Id == upgradableApps.AppID).CatalogPackage, installOptions).AsTask(upgradeTokenSource.Token, progressCallBack);

                        // ��ȡ������ɺ�Ľ����Ϣ
                        // ������ɣ����б���ɾ����Ӧ��
                        if (installResult.Status == InstallResultStatus.Ok)
                        {
                            AppNotificationService.Show(NotificationArgs.UpgradeSuccessfully, upgradableApps.AppName);

                            // ����Ƿ���Ҫ�����豸���Ӧ�õ�ж�أ�����ǣ�ѯ���û��Ƿ���Ҫ�����豸
                            if (installResult.RebootRequired)
                            {
                                ContentDialogResult Result = await ContentDialogHelper.ShowAsync(new RebootDialog(WinGetOptionArgs.UpgradeInstall, upgradableApps.AppName), this);
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
                                        bool createResult = Kernel32Library.CreateProcess(null, string.Format("{0} {1}", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "Shutdown.exe"), "-r -t 120"), IntPtr.Zero, IntPtr.Zero, false, CreateProcessFlags.CREATE_NO_WINDOW, IntPtr.Zero, null, ref RebootStartupInfo, out PROCESS_INFORMATION RebootProcessInformation);

                                        if (createResult)
                                        {
                                            if (RebootProcessInformation.hProcess != IntPtr.Zero) Kernel32Library.CloseHandle(RebootProcessInformation.hProcess);
                                            if (RebootProcessInformation.hThread != IntPtr.Zero) Kernel32Library.CloseHandle(RebootProcessInformation.hThread);
                                        }
                                    }
                                }
                            }

                            // ������������������ɾ������
                            lock (WinGetInstance.InstallingAppsObject)
                            {
                                foreach (InstallingAppsModel installingAppsItem in WinGetInstance.InstallingAppsList)
                                {
                                    if (installingAppsItem.AppID == upgradableApps.AppID)
                                    {
                                        WinGetInstance.InstallingAppsList.Remove(installingAppsItem);
                                        break;
                                    }
                                }
                                WinGetInstance.InstallingStateDict.Remove(upgradableApps.AppID);
                            }

                            // �������б����Ƴ���������ɵ�����
                            foreach (UpgradableAppsModel upgradableAppsItem in UpgradableAppsDataList)
                            {
                                if (upgradableAppsItem.AppID == upgradableApps.AppID)
                                {
                                    UpgradableAppsDataList.Remove(upgradableAppsItem);
                                    IsUpgradableAppsEmpty = UpgradableAppsDataList.Count is 0;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // Ӧ������ʧ�ܣ�����ǰ����״̬�޸�Ϊ������״̬
                            foreach (UpgradableAppsModel upgradableAppsItem in UpgradableAppsDataList)
                            {
                                if (upgradableAppsItem.AppID == upgradableApps.AppID)
                                {
                                    upgradableAppsItem.IsUpgrading = false;
                                }
                            }

                            // Ӧ������ʧ�ܣ�����ǰ����״̬�޸�Ϊ������״̬
                            lock (WinGetInstance.InstallingAppsObject)
                            {
                                foreach (InstallingAppsModel installingAppsItem in WinGetInstance.InstallingAppsList)
                                {
                                    if (installingAppsItem.AppID == upgradableApps.AppID)
                                    {
                                        WinGetInstance.InstallingAppsList.Remove(installingAppsItem);
                                        break;
                                    }
                                }
                                WinGetInstance.InstallingStateDict.Remove(upgradableApps.AppID);
                            }

                            AppNotificationService.Show(NotificationArgs.UpgradeFailed, upgradableApps.AppName, upgradableApps.AppID);
                        }
                    }
                    // �������û���ȡ���쳣
                    catch (OperationCanceledException e)
                    {
                        LogService.WriteLog(LogType.INFO, "App installing operation canceled.", e);

                        // Ӧ������ʧ�ܣ�����ǰ����״̬�޸�Ϊ������״̬
                        foreach (UpgradableAppsModel upgradableAppsItem in UpgradableAppsDataList)
                        {
                            if (upgradableAppsItem.AppID == upgradableApps.AppID)
                            {
                                upgradableAppsItem.IsUpgrading = false;
                                break;
                            }
                        }

                        // Ӧ������ʧ�ܣ�����ǰ����״̬�޸�Ϊ������״̬
                        lock (WinGetInstance.InstallingAppsObject)
                        {
                            foreach (InstallingAppsModel installingAppsItem in WinGetInstance.InstallingAppsList)
                            {
                                if (installingAppsItem.AppID == upgradableApps.AppID)
                                {
                                    WinGetInstance.InstallingAppsList.Remove(installingAppsItem);
                                    break;
                                }
                            }
                            WinGetInstance.InstallingStateDict.Remove(upgradableApps.AppID);
                        }
                    }
                    // �����쳣
                    catch (Exception e)
                    {
                        LogService.WriteLog(LogType.ERROR, "App installing failed.", e);

                        // Ӧ������ʧ�ܣ�����������б����Ƴ���ǰ����
                        foreach (UpgradableAppsModel upgradableAppsItem in UpgradableAppsDataList)
                        {
                            if (upgradableAppsItem.AppID == upgradableApps.AppID)
                            {
                                upgradableAppsItem.IsUpgrading = false;
                                break;
                            }
                        }

                        // Ӧ������ʧ�ܣ�����������б����Ƴ���ǰ����
                        lock (WinGetInstance.InstallingAppsObject)
                        {
                            foreach (InstallingAppsModel installingAppsItem in WinGetInstance.InstallingAppsList)
                            {
                                if (installingAppsItem.AppID == upgradableApps.AppID)
                                {
                                    WinGetInstance.InstallingAppsList.Remove(installingAppsItem);
                                    break;
                                }
                            }
                            WinGetInstance.InstallingStateDict.Remove(upgradableApps.AppID);
                        }

                        AppNotificationService.Show(NotificationArgs.UpgradeFailed, upgradableApps.AppName, upgradableApps.AppID);
                    }
                }
            };

            CopyUpgradeTextCommand.ExecuteRequested += (sender, args) =>
            {
                string appId = args.Parameter as string;
                if (appId is not null)
                {
                    string copyContent = string.Format("winget install {0}", appId);
                    CopyPasteHelper.CopyToClipBoard(copyContent);

                    new WinGetCopyNotification(this, WinGetOptionArgs.UpgradeInstall).Show();
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
        public string LocalizeUpgradableAppsCountInfo(int count)
        {
            if (count is 0)
            {
                return ResourceService.GetLocalized("WinGet/UpgradableAppsCountEmpty");
            }
            else
            {
                return string.Format(ResourceService.GetLocalized("WinGet/UpgradableAppsCountInfo"), count);
            }
        }

        /// <summary>
        /// ��ʼ��������Ӧ����Ϣ
        /// </summary>
        public async void OnLoaded(object sender, RoutedEventArgs args)
        {
            if (!isInitialized)
            {
                try
                {
                    UpgradableAppsManager = WinGetService.CreatePackageManager();
                }
                catch (Exception e)
                {
                    LogService.WriteLog(LogType.ERROR, "Upgradable apps information initialized failed.", e);
                    return;
                }
                await Task.Delay(500);
                await GetUpgradableAppsAsync();
                InitializeData();
                if (MatchResultList is null || MatchResultList.Count is 0)
                {
                    IsUpgradableAppsEmpty = true;
                }
                else
                {
                    IsUpgradableAppsEmpty = false;
                }
                IsLoadedCompleted = true;
                isInitialized = true;
            }
        }

        /// <summary>
        /// ���¿�����Ӧ������
        /// </summary>
        public async void OnRefreshClicked(object sender, RoutedEventArgs args)
        {
            MatchResultList = null;
            IsLoadedCompleted = false;
            await Task.Delay(500);
            await GetUpgradableAppsAsync();
            InitializeData();
            if (MatchResultList is null || MatchResultList.Count is 0)
            {
                IsUpgradableAppsEmpty = true;
            }
            else
            {
                IsUpgradableAppsEmpty = false;
            }
            IsLoadedCompleted = true;
        }

        /// <summary>
        /// ����ֵ�����仯ʱ֪ͨ����
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// ����ϵͳ��������Ӧ����Ϣ
        /// </summary>
        private async Task GetUpgradableAppsAsync()
        {
            try
            {
                await Task.Run(async () =>
                {
                    List<PackageCatalogReference> packageCatalogReferences = UpgradableAppsManager.GetPackageCatalogs().ToList();
                    CreateCompositePackageCatalogOptions createCompositePackageCatalogOptions = WinGetService.CreateCreateCompositePackageCatalogOptions();
                    PackageCatalogReference searchCatalogReference = UpgradableAppsManager.GetLocalPackageCatalog(LocalPackageCatalog.InstalledPackages);
                    foreach (PackageCatalogReference catalogReference in packageCatalogReferences)
                    {
                        createCompositePackageCatalogOptions.Catalogs.Add(catalogReference);
                    }
                    createCompositePackageCatalogOptions.CompositeSearchBehavior = CompositeSearchBehavior.LocalCatalogs;
                    PackageCatalogReference packageCatalogReference = UpgradableAppsManager.CreateCompositePackageCatalog(createCompositePackageCatalogOptions);
                    ConnectResult connectResult = await packageCatalogReference.ConnectAsync();
                    PackageCatalog upgradableCatalog = connectResult.PackageCatalog;

                    if (upgradableCatalog is not null)
                    {
                        FindPackagesOptions findPackagesOptions = WinGetService.CreateFindPackagesOptions();
                        FindPackagesResult findResult = await upgradableCatalog.FindPackagesAsync(findPackagesOptions);
                        var result = findResult.Matches.ToList();

                        MatchResultList = findResult.Matches.ToList().Where(item => item.CatalogPackage.IsUpdateAvailable == true).ToList();
                    }
                });
            }
            catch (Exception e)
            {
                LogService.WriteLog(LogType.WARNING, "Get upgradable apps information failed.", e);
            }
        }

        private void InitializeData()
        {
            UpgradableAppsDataList.Clear();
            if (MatchResultList is not null)
            {
                foreach (MatchResult matchItem in MatchResultList)
                {
                    bool isUpgrading = false;
                    foreach (InstallingAppsModel installingAppsItem in WinGetInstance.InstallingAppsList)
                    {
                        if (matchItem.CatalogPackage.DefaultInstallVersion.Id == installingAppsItem.AppID)
                        {
                            isUpgrading = true;
                            break;
                        }
                    }
                    UpgradableAppsDataList.Add(new UpgradableAppsModel()
                    {
                        AppID = matchItem.CatalogPackage.DefaultInstallVersion.Id,
                        AppName = string.IsNullOrEmpty(matchItem.CatalogPackage.DefaultInstallVersion.DisplayName) ? ResourceService.GetLocalized("WinGet/Unknown") : matchItem.CatalogPackage.DefaultInstallVersion.DisplayName,
                        AppPublisher = string.IsNullOrEmpty(matchItem.CatalogPackage.InstalledVersion.Publisher) ? ResourceService.GetLocalized("WinGet/Unknown") : matchItem.CatalogPackage.InstalledVersion.Publisher,
                        AppCurrentVersion = string.IsNullOrEmpty(matchItem.CatalogPackage.InstalledVersion.Version) ? ResourceService.GetLocalized("WinGet/Unknown") : matchItem.CatalogPackage.InstalledVersion.Version,
                        AppNewestVersion = string.IsNullOrEmpty(matchItem.CatalogPackage.DefaultInstallVersion.Version) ? ResourceService.GetLocalized("WinGet/Unknown") : matchItem.CatalogPackage.DefaultInstallVersion.Version,
                        IsUpgrading = isUpgrading
                    });
                }
            }
        }
    }
}
