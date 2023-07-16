using GetStoreApp.Extensions.DataType.Enums;
using GetStoreApp.Helpers.Root;
using GetStoreApp.Models.Controls.WinGet;
using GetStoreApp.Services.Root;
using GetStoreApp.UI.Dialogs.WinGet;
using GetStoreApp.UI.Notifications;
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
using System.Threading.Tasks;

namespace GetStoreApp.UI.Controls.WinGet
{
    /// <summary>
    /// WinGet �����ҳ�棺�Ѱ�װӦ�ÿؼ�
    /// </summary>
    public sealed partial class InstalledAppsControl : Grid, INotifyPropertyChanged
    {
        private PackageManager InstalledAppsManager { get; set; }
        private bool isInitialized = false;

        private bool _isLoadedCompleted = false;

        public bool IsLoadedCompleted
        {
            get { return _isLoadedCompleted; }

            set
            {
                _isLoadedCompleted = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoadedCompleted)));
            }
        }

        private bool _isInstalledAppsEmpty;

        public bool IsInstalledAppsEmpty
        {
            get { return _isInstalledAppsEmpty; }

            set
            {
                _isInstalledAppsEmpty = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsInstalledAppsEmpty)));
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

        private List<MatchResult> MatchResultList;

        // ж��Ӧ��
        public XamlUICommand UnInstallCommand { get; } = new XamlUICommand();

        // ����ж������
        public XamlUICommand CopyUnInstallTextCommand { get; } = new XamlUICommand();

        public ObservableCollection<InstalledAppsModel> InstalledAppsDataList { get; set; } = new ObservableCollection<InstalledAppsModel>();

        public event PropertyChangedEventHandler PropertyChanged;

        public InstalledAppsControl()
        {
            InitializeComponent();

            PropertyChanged += OnPropertyChanged;
            UnInstallCommand.ExecuteRequested += async (sender, args) =>
            {
                InstalledAppsModel installedApps = args.Parameter as InstalledAppsModel;
                if (installedApps is not null)
                {
                    try
                    {
                        UninstallOptions uninstallOptions = WinGetService.CreateUninstallOptions();

                        uninstallOptions.PackageUninstallMode = PackageUninstallMode.Interactive;
                        uninstallOptions.PackageUninstallScope = PackageUninstallScope.Any;

                        UninstallResult unInstallResult = await InstalledAppsManager.UninstallPackageAsync(MatchResultList.Find(item => item.CatalogPackage.InstalledVersion.Id == installedApps.AppID).CatalogPackage, uninstallOptions);

                        // ��ȡж�غ�Ľ����Ϣ
                        // ж�سɹ������б���ɾ����Ӧ��
                        if (unInstallResult.Status == UninstallResultStatus.Ok)
                        {
                            AppNotificationService.Show(NotificationArgs.UnInstallSuccessfully, installedApps.AppName);

                            // ����Ƿ���Ҫ�����豸���Ӧ�õ�ж�أ�����ǣ�ѯ���û��Ƿ���Ҫ�����豸
                            if (unInstallResult.RebootRequired)
                            {
                                ContentDialogResult Result = await new RebootDialog(WinGetOptionArgs.UnInstall, installedApps.AppName).ShowAsync();
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
                            AppNotificationService.Show(NotificationArgs.UnInstallFailed, installedApps.AppName);
                        }
                    }
                    // �������û���ȡ���쳣
                    catch (OperationCanceledException e)
                    {
                        LogService.WriteLog(LogType.INFO, "App uninstalling operation canceled.", e);
                        AppNotificationService.Show(NotificationArgs.UnInstallFailed, installedApps.AppName);
                    }
                    // �����쳣
                    catch (Exception e)
                    {
                        LogService.WriteLog(LogType.ERROR, "App uninstalling failed.", e);
                        AppNotificationService.Show(NotificationArgs.UnInstallFailed, installedApps.AppName);
                    }
                }
            };

            CopyUnInstallTextCommand.ExecuteRequested += (sender, args) =>
            {
                string appId = args.Parameter as string;
                if (appId is not null)
                {
                    string copyContent = string.Format("winget uninstall {0}", appId);
                    CopyPasteHelper.CopyToClipBoard(copyContent);

                    new WinGetCopyNotification(WinGetOptionArgs.UnInstall).Show();
                }
            };
        }

        ~InstalledAppsControl()
        {
            PropertyChanged -= OnPropertyChanged;
        }

        /// <summary>
        /// ���ػ�Ӧ������ͳ����Ϣ
        /// </summary>
        public string LocalizeInstalledAppsCountInfo(int count)
        {
            if (count is 0)
            {
                return ResourceService.GetLocalized("WinGet/InstalledAppsCountEmpty");
            }
            else
            {
                return string.Format(ResourceService.GetLocalized("WinGet/InstalledAppsCountInfo"), count);
            }
        }

        /// <summary>
        /// ��ʼ���Ѱ�װӦ����Ϣ
        /// </summary>
        public async void OnLoaded(object sender, RoutedEventArgs args)
        {
            if (!isInitialized)
            {
                try
                {
                    InstalledAppsManager = WinGetService.CreatePackageManager();
                }
                catch (Exception e)
                {
                    LogService.WriteLog(LogType.ERROR, "Installed apps information initialized failed.", e);
                    return;
                }
                await Task.Delay(500);
                await GetInstalledAppsAsync();
                InitializeData();
                IsInstalledAppsEmpty = MatchResultList.Count is 0;
                IsLoadedCompleted = true;
                isInitialized = true;
            }
        }

        /// <summary>
        /// �����Ѱ�װӦ������
        /// </summary>
        public async void OnRefreshClicked(object sender, RoutedEventArgs args)
        {
            MatchResultList = null;
            IsLoadedCompleted = false;
            SearchText = string.Empty;
            await Task.Delay(500);
            await GetInstalledAppsAsync();
            InitializeData();
            IsInstalledAppsEmpty = MatchResultList.Count is 0;
            IsLoadedCompleted = true;
        }

        /// <summary>
        /// ������������ݼ���Ӧ��
        /// </summary>
        public void OnQuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            InitializeData(true);
        }

        /// <summary>
        /// �ı����������Ϊ��ʱ����ԭԭ��������
        /// </summary>
        public void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(SearchText))
            {
                if (SearchText == string.Empty && MatchResultList is not null)
                {
                    InitializeData();
                }
            }
        }

        /// <summary>
        /// ����ϵͳ�Ѱ�װ��Ӧ����Ϣ
        /// </summary>
        private async Task GetInstalledAppsAsync()
        {
            try
            {
                await Task.Run(async () =>
                {
                    PackageCatalogReference searchCatalogReference = InstalledAppsManager.GetLocalPackageCatalog(LocalPackageCatalog.InstalledPackages);

                    ConnectResult connectResult = await searchCatalogReference.ConnectAsync();
                    PackageCatalog installedCatalog = connectResult.PackageCatalog;

                    if (installedCatalog is not null)
                    {
                        FindPackagesOptions findPackagesOptions = WinGetService.CreateFindPackagesOptions();
                        FindPackagesResult findResult = await installedCatalog.FindPackagesAsync(findPackagesOptions);

                        MatchResultList = findResult.Matches.ToList().Where(item => item.CatalogPackage.InstalledVersion.Publisher != string.Empty).ToList();
                    }
                });
            }
            catch (Exception e)
            {
                LogService.WriteLog(LogType.ERROR, "Get installed apps information failed.", e);
            }
        }

        /// <summary>
        /// ��ʼ���б�����
        /// </summary>
        private void InitializeData(bool hasSearchText = false)
        {
            InstalledAppsDataList.Clear();
            if (MatchResultList is not null)
            {
                if (hasSearchText)
                {
                    foreach (MatchResult matchItem in MatchResultList)
                    {
                        if (matchItem.CatalogPackage.InstalledVersion.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                        {
                            InstalledAppsDataList.Add(new InstalledAppsModel()
                            {
                                AppID = matchItem.CatalogPackage.InstalledVersion.Id,
                                AppName = string.IsNullOrEmpty(matchItem.CatalogPackage.InstalledVersion.DisplayName) ? ResourceService.GetLocalized("WinGet/Unknown") : matchItem.CatalogPackage.InstalledVersion.DisplayName,
                                AppPublisher = string.IsNullOrEmpty(matchItem.CatalogPackage.InstalledVersion.Publisher) ? ResourceService.GetLocalized("WinGet/Unknown") : matchItem.CatalogPackage.InstalledVersion.Publisher,
                                AppVersion = string.IsNullOrEmpty(matchItem.CatalogPackage.InstalledVersion.Version) ? ResourceService.GetLocalized("WinGet/Unknown") : matchItem.CatalogPackage.InstalledVersion.Version,
                            });
                        }
                    }
                }
                else
                {
                    foreach (MatchResult matchItem in MatchResultList)
                    {
                        InstalledAppsDataList.Add(new InstalledAppsModel()
                        {
                            AppID = matchItem.CatalogPackage.InstalledVersion.Id,
                            AppName = string.IsNullOrEmpty(matchItem.CatalogPackage.InstalledVersion.DisplayName) ? ResourceService.GetLocalized("WinGet/Unknown") : matchItem.CatalogPackage.InstalledVersion.DisplayName,
                            AppPublisher = string.IsNullOrEmpty(matchItem.CatalogPackage.InstalledVersion.Publisher) ? ResourceService.GetLocalized("WinGet/Unknown") : matchItem.CatalogPackage.InstalledVersion.Publisher,
                            AppVersion = string.IsNullOrEmpty(matchItem.CatalogPackage.InstalledVersion.Version) ? ResourceService.GetLocalized("WinGet/Unknown") : matchItem.CatalogPackage.InstalledVersion.Version,
                        });
                    }
                }
            }
        }
    }
}
