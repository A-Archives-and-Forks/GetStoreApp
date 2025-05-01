using GetStoreApp.Extensions.DataType.Classes;
using GetStoreApp.Extensions.DataType.Enums;
using GetStoreApp.Models.Controls.AppManager;
using GetStoreApp.Services.Root;
using GetStoreApp.Views.Windows;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Diagnostics;
using Windows.Management.Core;
using Windows.Management.Deployment;
using Windows.Storage;
using Windows.System;

// ���� CA1822��IDE0060 ����
#pragma warning disable CA1822,IDE0060

namespace GetStoreApp.Views.Pages
{
    /// <summary>
    /// Ӧ�ù����б�ҳ
    /// </summary>
    public sealed partial class AppListPage : Page, INotifyPropertyChanged
    {
        private readonly string PackageEmptyDescription = ResourceService.GetLocalized("AppManager/PackageEmptyDescription");
        private readonly string PackageEmptyWithConditionDescription = ResourceService.GetLocalized("AppManager/PackageEmptyWithConditionDescription");
        private readonly string Unknown = ResourceService.GetLocalized("AppManager/Unknown");
        private readonly string Yes = ResourceService.GetLocalized("AppManager/Yes");
        private readonly string No = ResourceService.GetLocalized("AppManager/No");
        private readonly string PackageCountInfo = ResourceService.GetLocalized("AppManager/PackageCountInfo");
        private bool isInitialized;
        private bool needToRefreshData;
        private readonly PackageManager packageManager = new();

        private string _searchText = string.Empty;

        public string SearchText
        {
            get { return _searchText; }

            set
            {
                if (!Equals(_searchText, value))
                {
                    _searchText = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchText)));
                }
            }
        }

        private bool _isIncrease = true;

        public bool IsIncrease
        {
            get { return _isIncrease; }

            set
            {
                if (!Equals(_isIncrease, value))
                {
                    _isIncrease = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsIncrease)));
                }
            }
        }

        private AppManagerResultKind _appManagerResultKind;

        public AppManagerResultKind AppManagerResultKind
        {
            get { return _appManagerResultKind; }

            set
            {
                if (!Equals(_appManagerResultKind, value))
                {
                    _appManagerResultKind = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AppManagerResultKind)));
                }
            }
        }

        private bool _isAppFramework;

        public bool IsAppFramework
        {
            get { return _isAppFramework; }

            set
            {
                if (!Equals(_isAppFramework, value))
                {
                    _isAppFramework = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAppFramework)));
                }
            }
        }

        private AppSortRuleKind _selectedAppSortRuleKind = AppSortRuleKind.DisplayName;

        public AppSortRuleKind SelectedAppSortRuleKind
        {
            get { return _selectedAppSortRuleKind; }

            set
            {
                if (!Equals(_selectedAppSortRuleKind, value))
                {
                    _selectedAppSortRuleKind = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedAppSortRuleKind)));
                }
            }
        }

        private bool _isStoreSignatureSelected = true;

        public bool IsStoreSignatureSelected
        {
            get { return _isStoreSignatureSelected; }

            set
            {
                if (!Equals(_isStoreSignatureSelected, value))
                {
                    _isStoreSignatureSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsStoreSignatureSelected)));
                }
            }
        }

        private bool _isSystemSignatureSelected;

        public bool IsSystemSignatureSelected
        {
            get { return _isSystemSignatureSelected; }

            set
            {
                if (!Equals(_isSystemSignatureSelected, value))
                {
                    _isSystemSignatureSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSystemSignatureSelected)));
                }
            }
        }

        private bool _isEnterpriseSignatureSelected;

        public bool IsEnterpriseSignatureSelected
        {
            get { return _isEnterpriseSignatureSelected; }

            set
            {
                if (!Equals(_isEnterpriseSignatureSelected, value))
                {
                    _isEnterpriseSignatureSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnterpriseSignatureSelected)));
                }
            }
        }

        private bool _isDeveloperSignatureSelected;

        public bool IsDeveloperSignatureSelected
        {
            get { return _isDeveloperSignatureSelected; }

            set
            {
                if (!Equals(_isDeveloperSignatureSelected, value))
                {
                    _isDeveloperSignatureSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDeveloperSignatureSelected)));
                }
            }
        }

        private bool _isNoneSignatureSelected;

        public bool IsNoneSignatureSelected
        {
            get { return _isNoneSignatureSelected; }

            set
            {
                if (!Equals(_isNoneSignatureSelected, value))
                {
                    _isNoneSignatureSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsNoneSignatureSelected)));
                }
            }
        }

        private string _appManagerFailedContent;

        public string AppManagerFailedContent
        {
            get { return _appManagerFailedContent; }

            set
            {
                if (!Equals(_appManagerFailedContent, value))
                {
                    _appManagerFailedContent = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AppManagerFailedContent)));
                }
            }
        }

        private List<PackageModel> AppManagerList { get; } = [];

        private ObservableCollection<PackageModel> AppManagerCollection { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public AppListPage()
        {
            InitializeComponent();
        }

        #region ��һ���֣���д�����¼�

        /// <summary>
        /// ��������ҳ�津�����¼�
        /// </summary>
        protected override async void OnNavigatedTo(NavigationEventArgs args)
        {
            base.OnNavigatedTo(args);

            if (!isInitialized)
            {
                isInitialized = true;

                AppManagerResultKind = AppManagerResultKind.Loading;
                AppManagerList.Clear();
                AppManagerCollection.Clear();

                List<PackageModel> packageList = await Task.Run(() =>
                {
                    List<PackageModel> packageList = [];

                    try
                    {
                        foreach (Package packageItem in packageManager.FindPackagesForUser(string.Empty))
                        {
                            packageList.Add(new PackageModel()
                            {
                                LogoImage = packageItem.Logo,
                                IsFramework = GetIsFramework(packageItem),
                                AppListEntryCount = GetAppListEntriesCount(packageItem),
                                DisplayName = GetDisplayName(packageItem),
                                InstallDate = GetInstallDate(packageItem),
                                PublisherDisplayName = GetPublisherDisplayName(packageItem),
                                Version = GetVersion(packageItem),
                                SignatureKind = GetSignatureKind(packageItem),
                                InstalledDate = GetInstalledDate(packageItem),
                                Package = packageItem,
                                IsUninstalling = false
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(LoggingLevel.Error, "Find current user packages failed", e);
                    }

                    return packageList;
                });

                AppManagerList.AddRange(packageList);

                if (AppManagerList.Count is 0)
                {
                    AppManagerResultKind = AppManagerResultKind.Failed;
                    AppManagerFailedContent = PackageEmptyDescription;
                }
                else
                {
                    List<PackageModel> filterSortPackageList = await Task.Run(() =>
                    {
                        List<PackageModel> filterSortPackageList = [];

                        try
                        {
                            List<PackageModel> conditionWithFrameworkList = [];

                            // ����ѡ���Ƿ�ɸѡ������ܰ�������
                            if (IsAppFramework)
                            {
                                foreach (PackageModel packageItem in AppManagerList)
                                {
                                    if (packageItem.IsFramework.Equals(IsAppFramework))
                                    {
                                        conditionWithFrameworkList.Add(packageItem);
                                    }
                                }
                            }
                            else
                            {
                                conditionWithFrameworkList.AddRange(AppManagerList);
                            }

                            // ����ѡ���Ƿ�ɸѡ�����ض�ǩ�����͵�����
                            List<PackageModel> conditionWithSignatureKindList = [];
                            foreach (PackageModel packageItem in conditionWithFrameworkList)
                            {
                                if (packageItem.SignatureKind.Equals(PackageSignatureKind.Store) && IsStoreSignatureSelected)
                                {
                                    conditionWithSignatureKindList.Add(packageItem);
                                }
                                else if (packageItem.SignatureKind.Equals(PackageSignatureKind.System) && IsSystemSignatureSelected)
                                {
                                    conditionWithSignatureKindList.Add(packageItem);
                                }
                                else if (packageItem.SignatureKind.Equals(PackageSignatureKind.Enterprise) && IsEnterpriseSignatureSelected)
                                {
                                    conditionWithSignatureKindList.Add(packageItem);
                                }
                                else if (packageItem.SignatureKind.Equals(PackageSignatureKind.Developer) && IsDeveloperSignatureSelected)
                                {
                                    conditionWithSignatureKindList.Add(packageItem);
                                }
                                else if (packageItem.SignatureKind.Equals(PackageSignatureKind.None) && IsNoneSignatureSelected)
                                {
                                    conditionWithSignatureKindList.Add(packageItem);
                                }
                            }

                            List<PackageModel> searchedList = [];

                            // ������������ɸѡ�����ض�ǩ�����͵�����
                            if (string.IsNullOrEmpty(SearchText))
                            {
                                searchedList.AddRange(conditionWithSignatureKindList);
                            }
                            else
                            {
                                foreach (PackageModel packageItem in conditionWithSignatureKindList)
                                {
                                    if (packageItem.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || packageItem.PublisherDisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                                    {
                                        searchedList.Add(packageItem);
                                    }
                                }
                            }

                            // �Թ��˺���б����ݽ�������
                            switch (SelectedAppSortRuleKind)
                            {
                                case AppSortRuleKind.DisplayName:
                                    {
                                        if (IsIncrease)
                                        {
                                            searchedList.Sort((item1, item2) => item1.DisplayName.CompareTo(item2.DisplayName));
                                        }
                                        else
                                        {
                                            searchedList.Sort((item1, item2) => item2.DisplayName.CompareTo(item1.DisplayName));
                                        }
                                        break;
                                    }
                                case AppSortRuleKind.PublisherName:
                                    {
                                        if (IsIncrease)
                                        {
                                            searchedList.Sort((item1, item2) => item1.PublisherDisplayName.CompareTo(item2.PublisherDisplayName));
                                        }
                                        else
                                        {
                                            searchedList.Sort((item1, item2) => item2.PublisherDisplayName.CompareTo(item1.PublisherDisplayName));
                                        }
                                        break;
                                    }
                                case AppSortRuleKind.InstallDate:
                                    {
                                        if (IsIncrease)
                                        {
                                            searchedList.Sort((item1, item2) => item1.InstalledDate.CompareTo(item2.InstalledDate));
                                        }
                                        else
                                        {
                                            searchedList.Sort((item1, item2) => item2.InstalledDate.CompareTo(item1.InstalledDate));
                                        }
                                        break;
                                    }
                            }

                            filterSortPackageList.AddRange(searchedList);
                        }
                        catch (Exception e)
                        {
                            LogService.WriteLog(LoggingLevel.Error, "Filter and sort package list failed", e);
                        }

                        return filterSortPackageList;
                    });

                    foreach (PackageModel packageItem in filterSortPackageList)
                    {
                        AppManagerCollection.Add(packageItem);
                    }

                    AppManagerResultKind = AppManagerCollection.Count is 0 ? AppManagerResultKind.Failed : AppManagerResultKind.Successfully;
                    AppManagerFailedContent = AppManagerCollection.Count is 0 ? PackageEmptyWithConditionDescription : string.Empty;
                }
            }
        }

        #endregion ��һ���֣���д�����¼�

        #region ��һ���֣�XamlUICommand �������ʱ���ص��¼�

        /// <summary>
        /// ��Ӧ��
        /// </summary>
        private void OnOpenAppExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (args.Parameter is Package package)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await package.GetAppListEntries()[0].LaunchAsync();
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(LoggingLevel.Error, string.Format("Open app {0} failed", package.DisplayName), e);
                    }
                });
            }
        }

        /// <summary>
        /// ��Ӧ�û���Ŀ¼
        /// </summary>
        private void OnOpenCacheFolderExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (args.Parameter is Package package)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        if (ApplicationDataManager.CreateForPackageFamily(package.Id.FamilyName) is ApplicationData applicationData)
                        {
                            await Launcher.LaunchFolderAsync(applicationData.LocalFolder);
                        }
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(LoggingLevel.Information, "Open app cache folder failed.", e);
                    }
                });
            }
        }

        /// <summary>
        /// ��Ӧ�ð�װĿ¼
        /// </summary>
        private void OnOpenInstalledFolderExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (args.Parameter is Package package)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await Launcher.LaunchFolderPathAsync(package.InstalledPath);
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(LoggingLevel.Warning, string.Format("{0} app installed folder open failed", package.DisplayName), e);
                    }
                });
            }
        }

        /// <summary>
        /// ��Ӧ���嵥�ļ�
        /// </summary>
        private void OnOpenManifestExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (args.Parameter is Package package)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        if (await StorageFile.GetFileFromPathAsync(Path.Combine(package.InstalledPath, "AppxManifest.xml")) is StorageFile file)
                        {
                            await Launcher.LaunchFileAsync(file);
                        }
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(LoggingLevel.Error, string.Format("{0}'s AppxManifest.xml file open failed", package.DisplayName), e);
                    }
                });
            }
        }

        /// <summary>
        /// ���̵�
        /// </summary>
        private void OnOpenStoreExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (args.Parameter is Package package)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await Launcher.LaunchUriAsync(new Uri($"ms-windows-store://pdp/?PFN={package.Id.FamilyName}"));
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(LoggingLevel.Error, string.Format("Open microsoft store {0} failed", package.DisplayName), e);
                    }
                });
            }
        }

        /// <summary>
        /// ж��Ӧ��
        /// </summary>
        private async void OnUninstallExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (args.Parameter is Package package)
            {
                foreach (PackageModel packageItem in AppManagerList)
                {
                    if (packageItem.Package.Id.FullName.Equals(package.Id.FullName))
                    {
                        packageItem.IsUninstalling = true;
                        break;
                    }
                }

                try
                {
                    DeploymentResult deploymentResult = await Task.Run(async () =>
                    {
                        return await packageManager.RemovePackageAsync(package.Id.FullName, RemovalOptions.None);
                    });

                    // ж�سɹ�
                    if (deploymentResult.ExtendedErrorCode is null)
                    {
                        foreach (PackageModel pacakgeItem in AppManagerList)
                        {
                            if (pacakgeItem.Package.Id.FullName.Equals(package.Id.FullName))
                            {
                                // ��ʾ UWP Ӧ��ж�سɹ�֪ͨ
                                AppNotificationBuilder appNotificationBuilder = new();
                                appNotificationBuilder.AddArgument("action", "OpenApp");
                                appNotificationBuilder.AddText(string.Format(ResourceService.GetLocalized("Notification/UWPUninstallSuccessfully"), pacakgeItem.Package.DisplayName));
                                ToastNotificationService.Show(appNotificationBuilder.BuildNotification());

                                AppManagerList.Remove(pacakgeItem);
                                AppManagerCollection.Remove(pacakgeItem);
                                break;
                            }
                        }
                    }

                    // ж��ʧ��
                    else
                    {
                        foreach (PackageModel pacakgeItem in AppManagerList)
                        {
                            if (pacakgeItem.Package.Id.FullName.Equals(package.Id.FullName))
                            {
                                // ��ʾ UWP Ӧ��ж��ʧ��֪ͨ
                                AppNotificationBuilder appNotificationBuilder = new();
                                appNotificationBuilder.AddArgument("action", "OpenApp");
                                appNotificationBuilder.AddText(string.Format(ResourceService.GetLocalized("Notification/UWPUninstallFailed1"), pacakgeItem.Package.DisplayName));
                                appNotificationBuilder.AddText(ResourceService.GetLocalized("Notification/UWPUninstallFailed2"));

                                appNotificationBuilder.AddText(string.Join(Environment.NewLine, new string[]
                                {
                                ResourceService.GetLocalized("Notification/UWPUninstallFailed3"),
                                string.Format(ResourceService.GetLocalized("Notification/UWPUninstallFailed4"), deploymentResult.ExtendedErrorCode is not null ? deploymentResult.ExtendedErrorCode.HResult : Unknown),
                                string.Format(ResourceService.GetLocalized("Notification/UWPUninstallFailed5"), deploymentResult.ErrorText)
                                }));
                                AppNotificationButton openSettingsButton = new(ResourceService.GetLocalized("Notification/OpenSettings"));
                                openSettingsButton.Arguments.Add("action", "OpenSettings");
                                appNotificationBuilder.AddButton(openSettingsButton);
                                ToastNotificationService.Show(appNotificationBuilder.BuildNotification());
                                LogService.WriteLog(LoggingLevel.Information, string.Format("Uninstall app {0} failed", pacakgeItem.Package.DisplayName), deploymentResult.ExtendedErrorCode is not null ? deploymentResult.ExtendedErrorCode : new Exception());
                                pacakgeItem.IsUninstalling = false;
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogService.WriteLog(LoggingLevel.Information, string.Format("Uninstall app {0} failed", package.Id.FullName), e);
                }
            }
        }

        /// <summary>
        /// �鿴Ӧ����Ϣ
        /// </summary>
        private async void OnViewInformationExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (args.Parameter is PackageModel packageItem)
            {
                AppInformation appInformation = new();

                await Task.Run(() =>
                {
                    appInformation.DisplayName = packageItem.DisplayName;

                    try
                    {
                        appInformation.PackageFamilyName = string.IsNullOrEmpty(packageItem.Package.Id.FamilyName) ? Unknown : packageItem.Package.Id.FamilyName;
                    }
                    catch (Exception e)
                    {
                        ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                        appInformation.PackageFamilyName = Unknown;
                    }

                    try
                    {
                        appInformation.PackageFullName = string.IsNullOrEmpty(packageItem.Package.Id.FullName) ? Unknown : packageItem.Package.Id.FullName;
                    }
                    catch (Exception e)
                    {
                        ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                        appInformation.PackageFullName = Unknown;
                    }

                    try
                    {
                        appInformation.Description = string.IsNullOrEmpty(packageItem.Package.Description) ? Unknown : packageItem.Package.Description;
                    }
                    catch (Exception e)
                    {
                        ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                        appInformation.Description = Unknown;
                    }

                    appInformation.PublisherDisplayName = packageItem.PublisherDisplayName;

                    try
                    {
                        appInformation.PublisherId = string.IsNullOrEmpty(packageItem.Package.Id.PublisherId) ? Unknown : packageItem.Package.Id.PublisherId;
                    }
                    catch (Exception e)
                    {
                        ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                        appInformation.PublisherId = Unknown;
                    }

                    appInformation.Version = packageItem.Version;
                    appInformation.InstallDate = packageItem.InstallDate;

                    try
                    {
                        appInformation.Architecture = string.IsNullOrEmpty(packageItem.Package.Id.Architecture.ToString()) ? Unknown : packageItem.Package.Id.Architecture.ToString();
                    }
                    catch (Exception e)
                    {
                        ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                        appInformation.Architecture = Unknown;
                    }

                    appInformation.SignatureKind = ResourceService.GetLocalized(string.Format("AppManager/Signature{0}", packageItem.SignatureKind.ToString()));

                    try
                    {
                        appInformation.ResourceId = string.IsNullOrEmpty(packageItem.Package.Id.ResourceId) ? Unknown : packageItem.Package.Id.ResourceId;
                    }
                    catch (Exception e)
                    {
                        ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                        appInformation.ResourceId = Unknown;
                    }

                    try
                    {
                        appInformation.IsBundle = packageItem.Package.IsBundle ? Yes : No;
                    }
                    catch (Exception e)
                    {
                        ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                        appInformation.IsBundle = Unknown;
                    }

                    try
                    {
                        appInformation.IsDevelopmentMode = packageItem.Package.IsDevelopmentMode ? Yes : No;
                    }
                    catch (Exception e)
                    {
                        ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                        appInformation.IsDevelopmentMode = Unknown;
                    }

                    appInformation.IsFramework = packageItem.IsFramework ? Yes : No;

                    try
                    {
                        appInformation.IsOptional = packageItem.Package.IsOptional ? Yes : No;
                    }
                    catch (Exception e)
                    {
                        ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                        appInformation.IsOptional = Unknown;
                    }

                    try
                    {
                        appInformation.IsResourcePackage = packageItem.Package.IsResourcePackage ? Yes : No;
                    }
                    catch (Exception e)
                    {
                        ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                        appInformation.IsResourcePackage = Unknown;
                    }

                    try
                    {
                        appInformation.IsStub = packageItem.Package.IsStub ? Yes : No;
                    }
                    catch (Exception e)
                    {
                        ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                        appInformation.IsStub = Unknown;
                    }

                    try
                    {
                        appInformation.VertifyIsOK = packageItem.Package.Status.VerifyIsOK() ? Yes : No;
                    }
                    catch (Exception e)
                    {
                        ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                        appInformation.VertifyIsOK = Unknown;
                    }

                    try
                    {
                        IReadOnlyList<AppListEntry> appListEntriesList = packageItem.Package.GetAppListEntries();
                        for (int index = 0; index < appListEntriesList.Count; index++)
                        {
                            appInformation.AppListEntryList.Add(new AppListEntryModel()
                            {
                                DisplayName = appListEntriesList[index].DisplayInfo.DisplayName,
                                Description = appListEntriesList[index].DisplayInfo.Description,
                                AppUserModelId = appListEntriesList[index].AppUserModelId,
                                AppListEntry = appListEntriesList[index],
                                PackageFullName = packageItem.Package.Id.FullName
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                    }

                    try
                    {
                        IReadOnlyList<Package> dependcies = packageItem.Package.Dependencies;

                        if (dependcies.Count > 0)
                        {
                            for (int index = 0; index < dependcies.Count; index++)
                            {
                                try
                                {
                                    appInformation.DependenciesList.Add(new PackageModel()
                                    {
                                        DisplayName = dependcies[index].DisplayName,
                                        PublisherDisplayName = dependcies[index].PublisherDisplayName,
                                        Version = new Version(dependcies[index].Id.Version.Major, dependcies[index].Id.Version.Minor, dependcies[index].Id.Version.Build, dependcies[index].Id.Version.Revision).ToString(),
                                        Package = dependcies[index]
                                    });
                                }
                                catch
                                {
                                    continue;
                                }
                            }
                        }

                        appInformation.DependenciesList.Sort((item1, item2) => item1.DisplayName.CompareTo(item2.DisplayName));
                    }
                    catch (Exception e)
                    {
                        ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                    }
                });

                if (MainWindow.Current.GetFrameContent() is AppManagerPage appManagerPage && Equals(appManagerPage.GetCurrentPageType(), typeof(AppListPage)))
                {
                    appManagerPage.NavigateTo(typeof(AppInformationPage), appInformation, true);
                }
            }
        }

        #endregion ��һ���֣�XamlUICommand �������ʱ���ص��¼�

        #region �ڶ����֣�Ӧ�ù���ҳ�桪�����ص��¼�

        /// <summary>
        /// �������еİ�װ��Ӧ��
        /// </summary>
        private async void OnInstalledAppsClicked(object sender, RoutedEventArgs args)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-settings:appsfeatures"));
        }

        /// <summary>
        /// ������������ݼ���Ӧ��
        /// </summary>
        private async void OnQuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (!string.IsNullOrEmpty(SearchText))
            {
                AppManagerResultKind = AppManagerResultKind.Loading;
                AppManagerCollection.Clear();

                List<PackageModel> filterSortPackageList = await Task.Run(() =>
                {
                    List<PackageModel> filterSortPackageList = [];

                    try
                    {
                        List<PackageModel> conditionWithFrameworkList = [];

                        // ����ѡ���Ƿ�ɸѡ������ܰ�������
                        if (IsAppFramework)
                        {
                            foreach (PackageModel packageItem in AppManagerList)
                            {
                                if (packageItem.IsFramework.Equals(IsAppFramework))
                                {
                                    conditionWithFrameworkList.Add(packageItem);
                                }
                            }
                        }
                        else
                        {
                            conditionWithFrameworkList.AddRange(AppManagerList);
                        }

                        // ����ѡ���Ƿ�ɸѡ�����ض�ǩ�����͵�����
                        List<PackageModel> conditionWithSignatureKindList = [];
                        foreach (PackageModel packageItem in conditionWithFrameworkList)
                        {
                            if (packageItem.SignatureKind.Equals(PackageSignatureKind.Store) && IsStoreSignatureSelected)
                            {
                                conditionWithSignatureKindList.Add(packageItem);
                            }
                            else if (packageItem.SignatureKind.Equals(PackageSignatureKind.System) && IsSystemSignatureSelected)
                            {
                                conditionWithSignatureKindList.Add(packageItem);
                            }
                            else if (packageItem.SignatureKind.Equals(PackageSignatureKind.Enterprise) && IsEnterpriseSignatureSelected)
                            {
                                conditionWithSignatureKindList.Add(packageItem);
                            }
                            else if (packageItem.SignatureKind.Equals(PackageSignatureKind.Developer) && IsDeveloperSignatureSelected)
                            {
                                conditionWithSignatureKindList.Add(packageItem);
                            }
                            else if (packageItem.SignatureKind.Equals(PackageSignatureKind.None) && IsNoneSignatureSelected)
                            {
                                conditionWithSignatureKindList.Add(packageItem);
                            }
                        }

                        List<PackageModel> searchedList = [];

                        // ������������ɸѡ�����ض�ǩ�����͵�����
                        if (string.IsNullOrEmpty(SearchText))
                        {
                            searchedList.AddRange(conditionWithSignatureKindList);
                        }
                        else
                        {
                            foreach (PackageModel packageItem in conditionWithSignatureKindList)
                            {
                                if (packageItem.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || packageItem.PublisherDisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                                {
                                    searchedList.Add(packageItem);
                                }
                            }
                        }

                        // �Թ��˺���б����ݽ�������
                        switch (SelectedAppSortRuleKind)
                        {
                            case AppSortRuleKind.DisplayName:
                                {
                                    if (IsIncrease)
                                    {
                                        searchedList.Sort((item1, item2) => item1.DisplayName.CompareTo(item2.DisplayName));
                                    }
                                    else
                                    {
                                        searchedList.Sort((item1, item2) => item2.DisplayName.CompareTo(item1.DisplayName));
                                    }
                                    break;
                                }
                            case AppSortRuleKind.PublisherName:
                                {
                                    if (IsIncrease)
                                    {
                                        searchedList.Sort((item1, item2) => item1.PublisherDisplayName.CompareTo(item2.PublisherDisplayName));
                                    }
                                    else
                                    {
                                        searchedList.Sort((item1, item2) => item2.PublisherDisplayName.CompareTo(item1.PublisherDisplayName));
                                    }
                                    break;
                                }
                            case AppSortRuleKind.InstallDate:
                                {
                                    if (IsIncrease)
                                    {
                                        searchedList.Sort((item1, item2) => item1.InstalledDate.CompareTo(item2.InstalledDate));
                                    }
                                    else
                                    {
                                        searchedList.Sort((item1, item2) => item2.InstalledDate.CompareTo(item1.InstalledDate));
                                    }
                                    break;
                                }
                        }

                        filterSortPackageList.AddRange(searchedList);
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(LoggingLevel.Error, "Filter and sort package list failed", e);
                    }

                    return filterSortPackageList;
                });

                foreach (PackageModel packageItem in filterSortPackageList)
                {
                    AppManagerCollection.Add(packageItem);
                }

                AppManagerResultKind = AppManagerCollection.Count is 0 ? AppManagerResultKind.Failed : AppManagerResultKind.Successfully;
                AppManagerFailedContent = AppManagerCollection.Count is 0 ? PackageEmptyWithConditionDescription : string.Empty;
            }
        }

        /// <summary>
        /// �ı����������Ϊ��ʱ����ԭԭ��������
        /// </summary>
        private async void OnTextChanged(object sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (sender is AutoSuggestBox autoSuggestBox)
            {
                SearchText = autoSuggestBox.Text;
                if (string.IsNullOrEmpty(SearchText))
                {
                    AppManagerResultKind = AppManagerResultKind.Loading;
                    AppManagerCollection.Clear();

                    List<PackageModel> filterSortPackageList = await Task.Run(() =>
                    {
                        List<PackageModel> filterSortPackageList = [];

                        try
                        {
                            List<PackageModel> conditionWithFrameworkList = [];

                            // ����ѡ���Ƿ�ɸѡ������ܰ�������
                            if (IsAppFramework)
                            {
                                foreach (PackageModel packageItem in AppManagerList)
                                {
                                    if (packageItem.IsFramework.Equals(IsAppFramework))
                                    {
                                        conditionWithFrameworkList.Add(packageItem);
                                    }
                                }
                            }
                            else
                            {
                                conditionWithFrameworkList.AddRange(AppManagerList);
                            }

                            // ����ѡ���Ƿ�ɸѡ�����ض�ǩ�����͵�����
                            List<PackageModel> conditionWithSignatureKindList = [];
                            foreach (PackageModel packageItem in conditionWithFrameworkList)
                            {
                                if (packageItem.SignatureKind.Equals(PackageSignatureKind.Store) && IsStoreSignatureSelected)
                                {
                                    conditionWithSignatureKindList.Add(packageItem);
                                }
                                else if (packageItem.SignatureKind.Equals(PackageSignatureKind.System) && IsSystemSignatureSelected)
                                {
                                    conditionWithSignatureKindList.Add(packageItem);
                                }
                                else if (packageItem.SignatureKind.Equals(PackageSignatureKind.Enterprise) && IsEnterpriseSignatureSelected)
                                {
                                    conditionWithSignatureKindList.Add(packageItem);
                                }
                                else if (packageItem.SignatureKind.Equals(PackageSignatureKind.Developer) && IsDeveloperSignatureSelected)
                                {
                                    conditionWithSignatureKindList.Add(packageItem);
                                }
                                else if (packageItem.SignatureKind.Equals(PackageSignatureKind.None) && IsNoneSignatureSelected)
                                {
                                    conditionWithSignatureKindList.Add(packageItem);
                                }
                            }

                            List<PackageModel> searchedList = [];

                            // ������������ɸѡ�����ض�ǩ�����͵�����
                            if (string.IsNullOrEmpty(SearchText))
                            {
                                searchedList.AddRange(conditionWithSignatureKindList);
                            }
                            else
                            {
                                foreach (PackageModel packageItem in conditionWithSignatureKindList)
                                {
                                    if (packageItem.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || packageItem.PublisherDisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                                    {
                                        searchedList.Add(packageItem);
                                    }
                                }
                            }

                            // �Թ��˺���б����ݽ�������
                            switch (SelectedAppSortRuleKind)
                            {
                                case AppSortRuleKind.DisplayName:
                                    {
                                        if (IsIncrease)
                                        {
                                            searchedList.Sort((item1, item2) => item1.DisplayName.CompareTo(item2.DisplayName));
                                        }
                                        else
                                        {
                                            searchedList.Sort((item1, item2) => item2.DisplayName.CompareTo(item1.DisplayName));
                                        }
                                        break;
                                    }
                                case AppSortRuleKind.PublisherName:
                                    {
                                        if (IsIncrease)
                                        {
                                            searchedList.Sort((item1, item2) => item1.PublisherDisplayName.CompareTo(item2.PublisherDisplayName));
                                        }
                                        else
                                        {
                                            searchedList.Sort((item1, item2) => item2.PublisherDisplayName.CompareTo(item1.PublisherDisplayName));
                                        }
                                        break;
                                    }
                                case AppSortRuleKind.InstallDate:
                                    {
                                        if (IsIncrease)
                                        {
                                            searchedList.Sort((item1, item2) => item1.InstalledDate.CompareTo(item2.InstalledDate));
                                        }
                                        else
                                        {
                                            searchedList.Sort((item1, item2) => item2.InstalledDate.CompareTo(item1.InstalledDate));
                                        }
                                        break;
                                    }
                            }

                            filterSortPackageList.AddRange(searchedList);
                        }
                        catch (Exception e)
                        {
                            LogService.WriteLog(LoggingLevel.Error, "Filter and sort package list failed", e);
                        }

                        return filterSortPackageList;
                    });

                    foreach (PackageModel packageItem in filterSortPackageList)
                    {
                        AppManagerCollection.Add(packageItem);
                    }

                    AppManagerResultKind = AppManagerCollection.Count is 0 ? AppManagerResultKind.Failed : AppManagerResultKind.Successfully;
                    AppManagerFailedContent = AppManagerCollection.Count is 0 ? PackageEmptyWithConditionDescription : string.Empty;
                }
            }
        }

        /// <summary>
        /// ��������ʽ���б��������
        /// </summary>
        private async void OnSortWayClicked(object sender, RoutedEventArgs args)
        {
            if (sender is RadioMenuFlyoutItem radioMenuFlyoutItem && radioMenuFlyoutItem.Tag is string increase)
            {
                IsIncrease = Convert.ToBoolean(increase);

                if (AppManagerResultKind is AppManagerResultKind.Successfully)
                {
                    AppManagerResultKind = AppManagerResultKind.Loading;
                    AppManagerCollection.Clear();

                    List<PackageModel> filterSortPackageList = await Task.Run(() =>
                    {
                        List<PackageModel> filterSortPackageList = [];

                        try
                        {
                            List<PackageModel> conditionWithFrameworkList = [];

                            // ����ѡ���Ƿ�ɸѡ������ܰ�������
                            if (IsAppFramework)
                            {
                                foreach (PackageModel packageItem in AppManagerList)
                                {
                                    if (packageItem.IsFramework.Equals(IsAppFramework))
                                    {
                                        conditionWithFrameworkList.Add(packageItem);
                                    }
                                }
                            }
                            else
                            {
                                conditionWithFrameworkList.AddRange(AppManagerList);
                            }

                            // ����ѡ���Ƿ�ɸѡ�����ض�ǩ�����͵�����
                            List<PackageModel> conditionWithSignatureKindList = [];
                            foreach (PackageModel packageItem in conditionWithFrameworkList)
                            {
                                if (packageItem.SignatureKind.Equals(PackageSignatureKind.Store) && IsStoreSignatureSelected)
                                {
                                    conditionWithSignatureKindList.Add(packageItem);
                                }
                                else if (packageItem.SignatureKind.Equals(PackageSignatureKind.System) && IsSystemSignatureSelected)
                                {
                                    conditionWithSignatureKindList.Add(packageItem);
                                }
                                else if (packageItem.SignatureKind.Equals(PackageSignatureKind.Enterprise) && IsEnterpriseSignatureSelected)
                                {
                                    conditionWithSignatureKindList.Add(packageItem);
                                }
                                else if (packageItem.SignatureKind.Equals(PackageSignatureKind.Developer) && IsDeveloperSignatureSelected)
                                {
                                    conditionWithSignatureKindList.Add(packageItem);
                                }
                                else if (packageItem.SignatureKind.Equals(PackageSignatureKind.None) && IsNoneSignatureSelected)
                                {
                                    conditionWithSignatureKindList.Add(packageItem);
                                }
                            }

                            List<PackageModel> searchedList = [];

                            // ������������ɸѡ�����ض�ǩ�����͵�����
                            if (string.IsNullOrEmpty(SearchText))
                            {
                                searchedList.AddRange(conditionWithSignatureKindList);
                            }
                            else
                            {
                                foreach (PackageModel packageItem in conditionWithSignatureKindList)
                                {
                                    if (packageItem.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || packageItem.PublisherDisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                                    {
                                        searchedList.Add(packageItem);
                                    }
                                }
                            }

                            // �Թ��˺���б����ݽ�������
                            switch (SelectedAppSortRuleKind)
                            {
                                case AppSortRuleKind.DisplayName:
                                    {
                                        if (IsIncrease)
                                        {
                                            searchedList.Sort((item1, item2) => item1.DisplayName.CompareTo(item2.DisplayName));
                                        }
                                        else
                                        {
                                            searchedList.Sort((item1, item2) => item2.DisplayName.CompareTo(item1.DisplayName));
                                        }
                                        break;
                                    }
                                case AppSortRuleKind.PublisherName:
                                    {
                                        if (IsIncrease)
                                        {
                                            searchedList.Sort((item1, item2) => item1.PublisherDisplayName.CompareTo(item2.PublisherDisplayName));
                                        }
                                        else
                                        {
                                            searchedList.Sort((item1, item2) => item2.PublisherDisplayName.CompareTo(item1.PublisherDisplayName));
                                        }
                                        break;
                                    }
                                case AppSortRuleKind.InstallDate:
                                    {
                                        if (IsIncrease)
                                        {
                                            searchedList.Sort((item1, item2) => item1.InstalledDate.CompareTo(item2.InstalledDate));
                                        }
                                        else
                                        {
                                            searchedList.Sort((item1, item2) => item2.InstalledDate.CompareTo(item1.InstalledDate));
                                        }
                                        break;
                                    }
                            }

                            filterSortPackageList.AddRange(searchedList);
                        }
                        catch (Exception e)
                        {
                            LogService.WriteLog(LoggingLevel.Error, "Filter and sort package list failed", e);
                        }

                        return filterSortPackageList;
                    });

                    foreach (PackageModel packageItem in filterSortPackageList)
                    {
                        AppManagerCollection.Add(packageItem);
                    }

                    AppManagerResultKind = AppManagerCollection.Count is 0 ? AppManagerResultKind.Failed : AppManagerResultKind.Successfully;
                    AppManagerFailedContent = AppManagerCollection.Count is 0 ? PackageEmptyWithConditionDescription : string.Empty;
                }
            }
        }

        /// <summary>
        /// �������������б��������
        /// </summary>
        private async void OnSortRuleClicked(object sender, RoutedEventArgs args)
        {
            if (sender is RadioMenuFlyoutItem radioMenuFlyoutItem && radioMenuFlyoutItem.Tag is AppSortRuleKind appSortRuleKind)
            {
                SelectedAppSortRuleKind = appSortRuleKind;

                if (AppManagerResultKind is AppManagerResultKind.Successfully)
                {
                    AppManagerResultKind = AppManagerResultKind.Loading;
                    AppManagerCollection.Clear();

                    List<PackageModel> filterSortPackageList = await Task.Run(() =>
                    {
                        List<PackageModel> filterSortPackageList = [];

                        try
                        {
                            List<PackageModel> conditionWithFrameworkList = [];

                            // ����ѡ���Ƿ�ɸѡ������ܰ�������
                            if (IsAppFramework)
                            {
                                foreach (PackageModel packageItem in AppManagerList)
                                {
                                    if (packageItem.IsFramework.Equals(IsAppFramework))
                                    {
                                        conditionWithFrameworkList.Add(packageItem);
                                    }
                                }
                            }
                            else
                            {
                                conditionWithFrameworkList.AddRange(AppManagerList);
                            }

                            // ����ѡ���Ƿ�ɸѡ�����ض�ǩ�����͵�����
                            List<PackageModel> conditionWithSignatureKindList = [];
                            foreach (PackageModel packageItem in conditionWithFrameworkList)
                            {
                                if (packageItem.SignatureKind.Equals(PackageSignatureKind.Store) && IsStoreSignatureSelected)
                                {
                                    conditionWithSignatureKindList.Add(packageItem);
                                }
                                else if (packageItem.SignatureKind.Equals(PackageSignatureKind.System) && IsSystemSignatureSelected)
                                {
                                    conditionWithSignatureKindList.Add(packageItem);
                                }
                                else if (packageItem.SignatureKind.Equals(PackageSignatureKind.Enterprise) && IsEnterpriseSignatureSelected)
                                {
                                    conditionWithSignatureKindList.Add(packageItem);
                                }
                                else if (packageItem.SignatureKind.Equals(PackageSignatureKind.Developer) && IsDeveloperSignatureSelected)
                                {
                                    conditionWithSignatureKindList.Add(packageItem);
                                }
                                else if (packageItem.SignatureKind.Equals(PackageSignatureKind.None) && IsNoneSignatureSelected)
                                {
                                    conditionWithSignatureKindList.Add(packageItem);
                                }
                            }

                            List<PackageModel> searchedList = [];

                            // ������������ɸѡ�����ض�ǩ�����͵�����
                            if (string.IsNullOrEmpty(SearchText))
                            {
                                searchedList.AddRange(conditionWithSignatureKindList);
                            }
                            else
                            {
                                foreach (PackageModel packageItem in conditionWithSignatureKindList)
                                {
                                    if (packageItem.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || packageItem.PublisherDisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                                    {
                                        searchedList.Add(packageItem);
                                    }
                                }
                            }

                            // �Թ��˺���б����ݽ�������
                            switch (SelectedAppSortRuleKind)
                            {
                                case AppSortRuleKind.DisplayName:
                                    {
                                        if (IsIncrease)
                                        {
                                            searchedList.Sort((item1, item2) => item1.DisplayName.CompareTo(item2.DisplayName));
                                        }
                                        else
                                        {
                                            searchedList.Sort((item1, item2) => item2.DisplayName.CompareTo(item1.DisplayName));
                                        }
                                        break;
                                    }
                                case AppSortRuleKind.PublisherName:
                                    {
                                        if (IsIncrease)
                                        {
                                            searchedList.Sort((item1, item2) => item1.PublisherDisplayName.CompareTo(item2.PublisherDisplayName));
                                        }
                                        else
                                        {
                                            searchedList.Sort((item1, item2) => item2.PublisherDisplayName.CompareTo(item1.PublisherDisplayName));
                                        }
                                        break;
                                    }
                                case AppSortRuleKind.InstallDate:
                                    {
                                        if (IsIncrease)
                                        {
                                            searchedList.Sort((item1, item2) => item1.InstalledDate.CompareTo(item2.InstalledDate));
                                        }
                                        else
                                        {
                                            searchedList.Sort((item1, item2) => item2.InstalledDate.CompareTo(item1.InstalledDate));
                                        }
                                        break;
                                    }
                            }

                            filterSortPackageList.AddRange(searchedList);
                        }
                        catch (Exception e)
                        {
                            LogService.WriteLog(LoggingLevel.Error, "Filter and sort package list failed", e);
                        }

                        return filterSortPackageList;
                    });

                    foreach (PackageModel packageItem in filterSortPackageList)
                    {
                        AppManagerCollection.Add(packageItem);
                    }

                    AppManagerResultKind = AppManagerCollection.Count is 0 ? AppManagerResultKind.Failed : AppManagerResultKind.Successfully;
                    AppManagerFailedContent = AppManagerCollection.Count is 0 ? PackageEmptyWithConditionDescription : string.Empty;
                }
            }
        }

        /// <summary>
        /// ���ݹ��˷�ʽ���б���й���
        /// </summary>
        private void OnFilterWayClicked(object sender, RoutedEventArgs args)
        {
            IsAppFramework = !IsAppFramework;
            needToRefreshData = true;
        }

        /// <summary>
        /// ����ǩ��������й���
        /// </summary>
        private void OnSignatureRuleClicked(object sender, RoutedEventArgs args)
        {
            if (sender is ToggleButton toggleButton && toggleButton.Tag is not null)
            {
                PackageSignatureKind signatureKind = (PackageSignatureKind)toggleButton.Tag;

                if (signatureKind is PackageSignatureKind.Store)
                {
                    IsStoreSignatureSelected = !IsStoreSignatureSelected;
                }
                else if (signatureKind is PackageSignatureKind.System)
                {
                    IsSystemSignatureSelected = !IsSystemSignatureSelected;
                }
                else if (signatureKind is PackageSignatureKind.Enterprise)
                {
                    IsEnterpriseSignatureSelected = !IsEnterpriseSignatureSelected;
                }
                else if (signatureKind is PackageSignatureKind.Developer)
                {
                    IsDeveloperSignatureSelected = !IsDeveloperSignatureSelected;
                }
                else if (signatureKind is PackageSignatureKind.None)
                {
                    IsNoneSignatureSelected = !IsNoneSignatureSelected;
                }

                needToRefreshData = true;
            }
        }

        /// <summary>
        /// ˢ������
        /// </summary>
        private async void OnRefreshClicked(object sender, RoutedEventArgs args)
        {
            AppManagerResultKind = AppManagerResultKind.Loading;
            AppManagerList.Clear();
            AppManagerCollection.Clear();

            List<PackageModel> packageList = await Task.Run(() =>
            {
                List<PackageModel> packageList = [];

                try
                {
                    foreach (Package packageItem in packageManager.FindPackagesForUser(string.Empty))
                    {
                        packageList.Add(new PackageModel()
                        {
                            LogoImage = packageItem.Logo,
                            IsFramework = GetIsFramework(packageItem),
                            AppListEntryCount = GetAppListEntriesCount(packageItem),
                            DisplayName = GetDisplayName(packageItem),
                            InstallDate = GetInstallDate(packageItem),
                            PublisherDisplayName = GetPublisherDisplayName(packageItem),
                            Version = GetVersion(packageItem),
                            SignatureKind = GetSignatureKind(packageItem),
                            InstalledDate = GetInstalledDate(packageItem),
                            Package = packageItem,
                            IsUninstalling = false
                        });
                    }
                }
                catch (Exception e)
                {
                    LogService.WriteLog(LoggingLevel.Error, "Find current user packages failed", e);
                }

                return packageList;
            });

            AppManagerList.AddRange(packageList);

            if (AppManagerList.Count is 0)
            {
                AppManagerResultKind = AppManagerResultKind.Failed;
                AppManagerFailedContent = PackageEmptyDescription;
            }
            else
            {
                List<PackageModel> filterSortPackageList = await Task.Run(() =>
                {
                    List<PackageModel> filterSortPackageList = [];

                    try
                    {
                        List<PackageModel> conditionWithFrameworkList = [];

                        // ����ѡ���Ƿ�ɸѡ������ܰ�������
                        if (IsAppFramework)
                        {
                            foreach (PackageModel packageItem in AppManagerList)
                            {
                                if (packageItem.IsFramework.Equals(IsAppFramework))
                                {
                                    conditionWithFrameworkList.Add(packageItem);
                                }
                            }
                        }
                        else
                        {
                            conditionWithFrameworkList.AddRange(AppManagerList);
                        }

                        // ����ѡ���Ƿ�ɸѡ�����ض�ǩ�����͵�����
                        List<PackageModel> conditionWithSignatureKindList = [];
                        foreach (PackageModel packageItem in conditionWithFrameworkList)
                        {
                            if (packageItem.SignatureKind.Equals(PackageSignatureKind.Store) && IsStoreSignatureSelected)
                            {
                                conditionWithSignatureKindList.Add(packageItem);
                            }
                            else if (packageItem.SignatureKind.Equals(PackageSignatureKind.System) && IsSystemSignatureSelected)
                            {
                                conditionWithSignatureKindList.Add(packageItem);
                            }
                            else if (packageItem.SignatureKind.Equals(PackageSignatureKind.Enterprise) && IsEnterpriseSignatureSelected)
                            {
                                conditionWithSignatureKindList.Add(packageItem);
                            }
                            else if (packageItem.SignatureKind.Equals(PackageSignatureKind.Developer) && IsDeveloperSignatureSelected)
                            {
                                conditionWithSignatureKindList.Add(packageItem);
                            }
                            else if (packageItem.SignatureKind.Equals(PackageSignatureKind.None) && IsNoneSignatureSelected)
                            {
                                conditionWithSignatureKindList.Add(packageItem);
                            }
                        }

                        List<PackageModel> searchedList = [];

                        // ������������ɸѡ�����ض�ǩ�����͵�����
                        if (string.IsNullOrEmpty(SearchText))
                        {
                            searchedList.AddRange(conditionWithSignatureKindList);
                        }
                        else
                        {
                            foreach (PackageModel packageItem in conditionWithSignatureKindList)
                            {
                                if (packageItem.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || packageItem.PublisherDisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                                {
                                    searchedList.Add(packageItem);
                                }
                            }
                        }

                        // �Թ��˺���б����ݽ�������
                        switch (SelectedAppSortRuleKind)
                        {
                            case AppSortRuleKind.DisplayName:
                                {
                                    if (IsIncrease)
                                    {
                                        searchedList.Sort((item1, item2) => item1.DisplayName.CompareTo(item2.DisplayName));
                                    }
                                    else
                                    {
                                        searchedList.Sort((item1, item2) => item2.DisplayName.CompareTo(item1.DisplayName));
                                    }
                                    break;
                                }
                            case AppSortRuleKind.PublisherName:
                                {
                                    if (IsIncrease)
                                    {
                                        searchedList.Sort((item1, item2) => item1.PublisherDisplayName.CompareTo(item2.PublisherDisplayName));
                                    }
                                    else
                                    {
                                        searchedList.Sort((item1, item2) => item2.PublisherDisplayName.CompareTo(item1.PublisherDisplayName));
                                    }
                                    break;
                                }
                            case AppSortRuleKind.InstallDate:
                                {
                                    if (IsIncrease)
                                    {
                                        searchedList.Sort((item1, item2) => item1.InstalledDate.CompareTo(item2.InstalledDate));
                                    }
                                    else
                                    {
                                        searchedList.Sort((item1, item2) => item2.InstalledDate.CompareTo(item1.InstalledDate));
                                    }
                                    break;
                                }
                        }

                        filterSortPackageList.AddRange(searchedList);
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(LoggingLevel.Error, "Filter and sort package list failed", e);
                    }

                    return filterSortPackageList;
                });

                foreach (PackageModel packageItem in filterSortPackageList)
                {
                    AppManagerCollection.Add(packageItem);
                }

                AppManagerResultKind = AppManagerCollection.Count is 0 ? AppManagerResultKind.Failed : AppManagerResultKind.Successfully;
                AppManagerFailedContent = AppManagerCollection.Count is 0 ? PackageEmptyWithConditionDescription : string.Empty;
            }
        }

        /// <summary>
        /// �����˵��رպ��������
        /// </summary>
        private async void OnClosed(object sender, object args)
        {
            if (needToRefreshData)
            {
                AppManagerResultKind = AppManagerResultKind.Loading;
                AppManagerCollection.Clear();

                List<PackageModel> filterSortPackageList = await Task.Run(() =>
                {
                    List<PackageModel> filterSortPackageList = [];

                    try
                    {
                        List<PackageModel> conditionWithFrameworkList = [];

                        // ����ѡ���Ƿ�ɸѡ������ܰ�������
                        if (IsAppFramework)
                        {
                            foreach (PackageModel packageItem in AppManagerList)
                            {
                                if (packageItem.IsFramework.Equals(IsAppFramework))
                                {
                                    conditionWithFrameworkList.Add(packageItem);
                                }
                            }
                        }
                        else
                        {
                            conditionWithFrameworkList.AddRange(AppManagerList);
                        }

                        // ����ѡ���Ƿ�ɸѡ�����ض�ǩ�����͵�����
                        List<PackageModel> conditionWithSignatureKindList = [];
                        foreach (PackageModel packageItem in conditionWithFrameworkList)
                        {
                            if (packageItem.SignatureKind.Equals(PackageSignatureKind.Store) && IsStoreSignatureSelected)
                            {
                                conditionWithSignatureKindList.Add(packageItem);
                            }
                            else if (packageItem.SignatureKind.Equals(PackageSignatureKind.System) && IsSystemSignatureSelected)
                            {
                                conditionWithSignatureKindList.Add(packageItem);
                            }
                            else if (packageItem.SignatureKind.Equals(PackageSignatureKind.Enterprise) && IsEnterpriseSignatureSelected)
                            {
                                conditionWithSignatureKindList.Add(packageItem);
                            }
                            else if (packageItem.SignatureKind.Equals(PackageSignatureKind.Developer) && IsDeveloperSignatureSelected)
                            {
                                conditionWithSignatureKindList.Add(packageItem);
                            }
                            else if (packageItem.SignatureKind.Equals(PackageSignatureKind.None) && IsNoneSignatureSelected)
                            {
                                conditionWithSignatureKindList.Add(packageItem);
                            }
                        }

                        List<PackageModel> searchedList = [];

                        // ������������ɸѡ�����ض�ǩ�����͵�����
                        if (string.IsNullOrEmpty(SearchText))
                        {
                            searchedList.AddRange(conditionWithSignatureKindList);
                        }
                        else
                        {
                            foreach (PackageModel packageItem in conditionWithSignatureKindList)
                            {
                                if (packageItem.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || packageItem.PublisherDisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                                {
                                    searchedList.Add(packageItem);
                                }
                            }
                        }

                        // �Թ��˺���б����ݽ�������
                        switch (SelectedAppSortRuleKind)
                        {
                            case AppSortRuleKind.DisplayName:
                                {
                                    if (IsIncrease)
                                    {
                                        searchedList.Sort((item1, item2) => item1.DisplayName.CompareTo(item2.DisplayName));
                                    }
                                    else
                                    {
                                        searchedList.Sort((item1, item2) => item2.DisplayName.CompareTo(item1.DisplayName));
                                    }
                                    break;
                                }
                            case AppSortRuleKind.PublisherName:
                                {
                                    if (IsIncrease)
                                    {
                                        searchedList.Sort((item1, item2) => item1.PublisherDisplayName.CompareTo(item2.PublisherDisplayName));
                                    }
                                    else
                                    {
                                        searchedList.Sort((item1, item2) => item2.PublisherDisplayName.CompareTo(item1.PublisherDisplayName));
                                    }
                                    break;
                                }
                            case AppSortRuleKind.InstallDate:
                                {
                                    if (IsIncrease)
                                    {
                                        searchedList.Sort((item1, item2) => item1.InstalledDate.CompareTo(item2.InstalledDate));
                                    }
                                    else
                                    {
                                        searchedList.Sort((item1, item2) => item2.InstalledDate.CompareTo(item1.InstalledDate));
                                    }
                                    break;
                                }
                        }

                        filterSortPackageList.AddRange(searchedList);
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(LoggingLevel.Error, "Filter and sort package list failed", e);
                    }

                    return filterSortPackageList;
                });

                foreach (PackageModel packageItem in filterSortPackageList)
                {
                    AppManagerCollection.Add(packageItem);
                }

                AppManagerResultKind = AppManagerCollection.Count is 0 ? AppManagerResultKind.Failed : AppManagerResultKind.Successfully;
                AppManagerFailedContent = AppManagerCollection.Count is 0 ? PackageEmptyWithConditionDescription : string.Empty;
            }

            needToRefreshData = false;
        }

        #endregion �ڶ����֣�Ӧ�ù���ҳ�桪�����ص��¼�

        /// <summary>
        /// ��ȡ����Ӧ���Ƿ�ɹ�
        /// </summary>
        public Visibility GetAppManagerSuccessfullyState(AppManagerResultKind appManagerResultKind, bool isSuccessfully)
        {
            return isSuccessfully ? appManagerResultKind is AppManagerResultKind.Successfully ? Visibility.Visible : Visibility.Collapsed : appManagerResultKind is AppManagerResultKind.Successfully ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// �������Ӧ���Ƿ�ɹ�
        /// </summary>
        public Visibility CheckAppManagerState(AppManagerResultKind appManagerResultKind, AppManagerResultKind comparedAppManagerResultKind)
        {
            return appManagerResultKind.Equals(comparedAppManagerResultKind) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// ��ȡ�Ƿ����ڼ�����
        /// </summary>

        public bool GetIsLoading(AppManagerResultKind appManagerResultKind)
        {
            return !appManagerResultKind.Equals(AppManagerResultKind.Loading);
        }

        /// <summary>
        /// ��ȡӦ�ð��Ƿ�Ϊ��ܰ�
        /// </summary>
        private static bool GetIsFramework(Package package)
        {
            try
            {
                return package.IsFramework;
            }
            catch (Exception e)
            {
                ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                return false;
            }
        }

        /// <summary>
        /// ��ȡӦ�ð��������
        /// </summary>
        private static int GetAppListEntriesCount(Package package)
        {
            try
            {
                return package.GetAppListEntries().Count;
            }
            catch (Exception e)
            {
                ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                return 0;
            }
        }

        /// <summary>
        /// ��ȡӦ�õ���ʾ����
        /// </summary>
        private string GetDisplayName(Package package)
        {
            try
            {
                return string.IsNullOrEmpty(package.DisplayName) ? Unknown : package.DisplayName;
            }
            catch (Exception e)
            {
                ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                return Unknown;
            }
        }

        /// <summary>
        /// ��ȡӦ�õķ�������ʾ����
        /// </summary>
        private string GetPublisherDisplayName(Package package)
        {
            try
            {
                return string.IsNullOrEmpty(package.PublisherDisplayName) ? Unknown : package.PublisherDisplayName;
            }
            catch (Exception e)
            {
                ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                return Unknown;
            }
        }

        /// <summary>
        /// ��ȡӦ�õİ汾��Ϣ
        /// </summary>
        private static string GetVersion(Package package)
        {
            try
            {
                return new Version(package.Id.Version.Major, package.Id.Version.Minor, package.Id.Version.Build, package.Id.Version.Revision).ToString();
            }
            catch (Exception e)
            {
                ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                return new Version().ToString();
            }
        }

        /// <summary>
        /// ��ȡӦ�õİ�װ����
        /// </summary>
        private static string GetInstallDate(Package package)
        {
            try
            {
                return package.InstalledDate.ToString("yyyy/MM/dd HH:mm");
            }
            catch (Exception e)
            {
                ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                return DateTimeOffset.FromUnixTimeSeconds(0).ToString("yyyy/MM/dd HH:mm");
            }
        }

        /// <summary>
        /// ��ȡӦ�ð�ǩ����ʽ
        /// </summary>
        private static PackageSignatureKind GetSignatureKind(Package package)
        {
            try
            {
                return package.SignatureKind;
            }
            catch (Exception e)
            {
                ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                return PackageSignatureKind.None;
            }
        }

        /// <summary>
        /// ��ȡӦ�ð���װ����
        /// </summary>
        private static DateTimeOffset GetInstalledDate(Package package)
        {
            try
            {
                return package.InstalledDate;
            }
            catch (Exception e)
            {
                ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                return DateTimeOffset.FromUnixTimeSeconds(0);
            }
        }
    }
}
