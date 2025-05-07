using GetStoreApp.Extensions.DataType.Enums;
using GetStoreApp.Helpers.Root;
using GetStoreApp.Models.Controls.WinGet;
using GetStoreApp.Services.Root;
using GetStoreApp.UI.TeachingTips;
using GetStoreApp.Views.Windows;
using GetStoreApp.WindowsAPI.ComTypes;
using GetStoreApp.WindowsAPI.PInvoke.Ole32;
using Microsoft.Management.Deployment;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation.Diagnostics;

// ���� CA1822��IDE0060 ����
#pragma warning disable CA1822,IDE0060

namespace GetStoreApp.Views.Pages
{
    /// <summary>
    /// WinGet �Ѱ�װӦ�ý���
    /// </summary>
    public sealed partial class WinGetInstalledPage : Page, INotifyPropertyChanged
    {
        private readonly string InstalledAppsCountInfo = ResourceService.GetLocalized("WinGet/InstalledAppsCountInfo");
        private readonly string Unknown = ResourceService.GetLocalized("WinGet/Unknown");
        private readonly string InstalledAppsEmptyDescription = ResourceService.GetLocalized("WinGet/InstalledAppsEmptyDescription");
        private readonly string InstalledAppsFailed = ResourceService.GetLocalized("WinGet/InstalledAppsFailed");
        private readonly string InstalledFindAppsFailed = ResourceService.GetLocalized("WinGet/InstalledFindAppsFailed");
        private readonly string InstalledCatalogReferenceFailed = ResourceService.GetLocalized("WinGet/InstalledCatalogReferenceFailed");
        private readonly string InstalledNotSelectSource = ResourceService.GetLocalized("WinGet/InstalledNotSelectSource");
        private readonly Guid CLSID_OpenControlPanel = new("06622D85-6856-4460-8DE1-A81921B41C4B");
        private readonly Lock InstalledAppsLock = new();
        private IOpenControlPanel openControlPanel;
        private WinGetPage WinGetPageInstance;

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

        private InstalledAppsResultKind _installedAppsResultKind;

        public InstalledAppsResultKind InstalledAppsResultKind
        {
            get { return _installedAppsResultKind; }

            set
            {
                if (!Equals(_installedAppsResultKind, value))
                {
                    _installedAppsResultKind = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InstalledAppsResultKind)));
                }
            }
        }

        private string _installedFailedContent;

        public string InstalledFailedContent
        {
            get { return _installedFailedContent; }

            set
            {
                if (!Equals(_installedFailedContent, value))
                {
                    _installedFailedContent = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InstalledFailedContent)));
                }
            }
        }

        private List<InstalledAppsModel> InstalledAppsList { get; } = [];

        private ObservableCollection<InstalledAppsModel> InstalledAppsCollection { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public WinGetInstalledPage()
        {
            InitializeComponent();

            Task.Run(() =>
            {
                int createResult = Ole32Library.CoCreateInstance(CLSID_OpenControlPanel, IntPtr.Zero, CLSCTX.CLSCTX_INPROC_SERVER | CLSCTX.CLSCTX_INPROC_HANDLER | CLSCTX.CLSCTX_LOCAL_SERVER | CLSCTX.CLSCTX_REMOTE_SERVER, typeof(IOpenControlPanel).GUID, out IntPtr ppv);

                if (createResult is 0)
                {
                    openControlPanel = (IOpenControlPanel)Program.StrategyBasedComWrappers.GetOrCreateObjectForComInstance(ppv, CreateObjectFlags.Unwrap);
                }
            });
        }

        #region ��һ���֣���д�����¼�

        /// <summary>
        /// ��������ҳ�津�����¼�
        /// </summary>
        protected override async void OnNavigatedTo(NavigationEventArgs args)
        {
            base.OnNavigatedTo(args);

            if (args.Parameter is WinGetPage winGetPage && WinGetPageInstance is null)
            {
                WinGetPageInstance = winGetPage;
                await InitializeSearchAppsDataAsync();
            }
        }

        #endregion ��һ���֣���д�����¼�

        #region �ڶ����֣�XamlUICommand �������ʱ���ص��¼�

        /// <summary>
        /// ����ж������
        /// </summary>
        private async void OnCopyUninstallTextExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (args.Parameter is string appId && !string.IsNullOrEmpty(appId))
            {
                string copyContent = string.Format("winget uninstall {0}", appId);
                bool copyResult = CopyPasteHelper.CopyTextToClipBoard(copyContent);

                await MainWindow.Current.ShowNotificationAsync(new MainDataCopyTip(DataCopyKind.WinGetUninstall, copyResult));
            }
        }

        /// <summary>
        /// ж��Ӧ��
        /// </summary>
        private async void OnUninstallExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (args.Parameter is InstalledAppsModel installedApps && WinGetPageInstance is not null)
            {
                // ���õ�ǰӦ�õĿ�ж��״̬
                installedApps.IsUninstalling = true;

                await WinGetPageInstance.AddTaskAsync(new PackageOperationModel()
                {
                    PackageOperationKind = PackageOperationKind.Download,
                    AppID = installedApps.AppID,
                    AppName = installedApps.AppName,
                    AppVersion = installedApps.CatalogPackage.InstalledVersion.Version,
                    PackageOperationProgress = 0,
                    PackageUninstallProgressState = PackageUninstallProgressState.Queued,
                    PackageVersionId = null,
                    DownloadedFileSize = FileSizeHelper.ConvertFileSizeToString(0),
                    TotalFileSize = FileSizeHelper.ConvertFileSizeToString(0),
                    PackageDownloadProgress = null,
                    InstalledApps = installedApps,
                });
            }
        }

        #endregion �ڶ����֣�XamlUICommand �������ʱ���ص��¼�

        #region �������֣��Ѱ�װӦ�ÿؼ��������ص��¼�

        /// <summary>
        /// ��������ʽ���б��������
        /// </summary>
        private void OnSortWayClicked(object sender, RoutedEventArgs args)
        {
            if (sender is RadioMenuFlyoutItem radioMenuFlyoutItem && radioMenuFlyoutItem.Tag is string increase && (InstalledAppsResultKind is InstalledAppsResultKind.Successfully || InstalledAppsResultKind is InstalledAppsResultKind.SearchResult))
            {
                IsIncrease = Convert.ToBoolean(increase);
                UpdateSearchInstalledApps();
            }
        }

        /// <summary>
        /// �������������б��������
        /// </summary>
        private void OnSortRuleClicked(object sender, RoutedEventArgs args)
        {
            if (sender is RadioMenuFlyoutItem radioMenuFlyoutItem && radioMenuFlyoutItem.Tag is AppSortRuleKind appSortRuleKind && (InstalledAppsResultKind is InstalledAppsResultKind.Successfully || InstalledAppsResultKind is InstalledAppsResultKind.SearchResult))
            {
                SelectedAppSortRuleKind = appSortRuleKind;
                UpdateSearchInstalledApps();
            }
        }

        /// <summary>
        /// �����Ѱ�װӦ������
        /// </summary>
        private async void OnRefreshClicked(object sender, RoutedEventArgs args)
        {
            await InitializeSearchAppsDataAsync();
        }

        /// <summary>
        /// ���������
        /// </summary>

        private void OnTaskManagerClicked(object sender, RoutedEventArgs args)
        {
            WinGetPageInstance?.ShowTaskManager();
        }

        /// <summary>
        /// �򿪿������ĳ����빦��
        /// </summary>
        private void OnControlPanelClicked(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>
            {
                openControlPanel?.Open("Microsoft.ProgramsAndFeatures", null, IntPtr.Zero);
            });
        }

        /// <summary>
        /// ������������ݼ���Ӧ��
        /// </summary>
        private void OnQuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (!string.IsNullOrEmpty(SearchText))
            {
                InstalledAppsResultKind = InstalledAppsResultKind.Querying;
                InstalledAppsCollection.Clear();

                InstalledAppsLock.Enter();
                try
                {
                    foreach (InstalledAppsModel installedAppsItem in InstalledAppsList)
                    {
                        if (string.IsNullOrEmpty(SearchText))
                        {
                            InstalledAppsCollection.Add(installedAppsItem);
                        }
                        else
                        {
                            if (installedAppsItem.AppName.Contains(SearchText) || installedAppsItem.AppPublisher.Contains(SearchText))
                            {
                                InstalledAppsCollection.Add(installedAppsItem);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                }
                finally
                {
                    InstalledAppsLock.Exit();
                }
                InstalledAppsResultKind = InstalledAppsResultKind.SearchResult;
            }
        }

        /// <summary>
        /// �ı����������Ϊ��ʱ����ԭԭ��������
        /// </summary>
        private void OnTextChanged(object sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (sender is AutoSuggestBox autoSuggestBox)
            {
                SearchText = autoSuggestBox.Text;
                InstalledAppsCollection.Clear();
                if (string.IsNullOrEmpty(SearchText) && InstalledAppsResultKind is InstalledAppsResultKind.SearchResult)
                {
                    InstalledAppsLock.Enter();
                    try
                    {
                        foreach (InstalledAppsModel installedAppsItem in InstalledAppsList)
                        {
                            InstalledAppsCollection.Add(installedAppsItem);
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                    }
                    finally
                    {
                        InstalledAppsLock.Exit();
                    }
                    InstalledAppsResultKind = InstalledAppsResultKind.Successfully;
                }
            }
        }

        #endregion �������֣��Ѱ�װӦ�ÿؼ��������ص��¼�

        /// <summary>
        /// ��ʼ���Ѱ�װӦ������
        /// </summary>
        private async Task InitializeSearchAppsDataAsync()
        {
            InstalledAppsResultKind = InstalledAppsResultKind.Querying;

            InstalledAppsLock.Enter();
            try
            {
                InstalledAppsList.Clear();
                InstalledAppsCollection.Clear();
            }
            catch (Exception e)
            {
                ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
            }
            finally
            {
                InstalledAppsLock.Exit();
            }

            PackageCatalogReference packageCatalogReference = await Task.Run(() =>
            {
                PackageManager packageManager = new();
                return packageManager.GetLocalPackageCatalog(LocalPackageCatalog.InstalledPackages);
            });

            if (packageCatalogReference is not null)
            {
                (ConnectResult connectResult, FindPackagesResult findPackagesResult, List<InstalledAppsModel> upgradableAppsList) = await Task.Run(() =>
                {
                    return InstalledAppsAsync(packageCatalogReference);
                });

                if (connectResult.Status is ConnectResultStatus.Ok)
                {
                    if (findPackagesResult.Status is FindPackagesResultStatus.Ok)
                    {
                        if (upgradableAppsList.Count is 0)
                        {
                            InstalledAppsResultKind = InstalledAppsResultKind.Failed;
                            InstalledFailedContent = InstalledAppsEmptyDescription;
                        }
                        else
                        {
                            InstalledAppsLock.Enter();
                            try
                            {
                                foreach (InstalledAppsModel installedAppsItem in upgradableAppsList)
                                {
                                    InstalledAppsList.Add(installedAppsItem);
                                }

                                foreach (InstalledAppsModel installedAppsItem in InstalledAppsList)
                                {
                                    if (string.IsNullOrEmpty(SearchText))
                                    {
                                        InstalledAppsCollection.Add(installedAppsItem);
                                    }
                                    else
                                    {
                                        if (installedAppsItem.AppName.Contains(SearchText) || installedAppsItem.AppPublisher.Contains(SearchText))
                                        {
                                            InstalledAppsCollection.Add(installedAppsItem);
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                            }
                            finally
                            {
                                InstalledAppsLock.Exit();
                            }

                            InstalledAppsResultKind = string.IsNullOrEmpty(SearchText) ? InstalledAppsResultKind.Successfully : InstalledAppsResultKind.SearchResult;
                        }
                    }
                    else
                    {
                        InstalledAppsResultKind = InstalledAppsResultKind.Failed;
                        InstalledFailedContent = string.Format(InstalledAppsFailed, InstalledFindAppsFailed, findPackagesResult.ExtendedErrorCode is not null ? findPackagesResult.ExtendedErrorCode.HResult : Unknown);
                    }
                }
                else
                {
                    InstalledAppsResultKind = InstalledAppsResultKind.Failed;
                    InstalledFailedContent = string.Format(InstalledAppsFailed, InstalledCatalogReferenceFailed, findPackagesResult.ExtendedErrorCode is not null ? findPackagesResult.ExtendedErrorCode.HResult : Unknown);
                }
            }
            else
            {
                InstalledAppsResultKind = InstalledAppsResultKind.Failed;
                InstalledFailedContent = InstalledNotSelectSource;
            }
        }

        /// <summary>
        /// ���´��������Ѱ�װӦ�ý��
        /// </summary>
        private void UpdateSearchInstalledApps()
        {
            InstalledAppsResultKind = InstalledAppsResultKind.Querying;
            InstalledAppsLock.Enter();
            try
            {
                InstalledAppsCollection.Clear();
                if (SelectedAppSortRuleKind is AppSortRuleKind.DisplayName)
                {
                    if (IsIncrease)
                    {
                        InstalledAppsList.Sort((item1, item2) => item1.AppName.CompareTo(item2.AppName));
                    }
                    else
                    {
                        InstalledAppsList.Sort((item1, item2) => item2.AppName.CompareTo(item1.AppName));
                    }
                }
                else
                {
                    if (IsIncrease)
                    {
                        InstalledAppsList.Sort((item1, item2) => item1.AppPublisher.CompareTo(item2.AppPublisher));
                    }
                    else
                    {
                        InstalledAppsList.Sort((item1, item2) => item2.AppPublisher.CompareTo(item1.AppPublisher));
                    }
                }

                foreach (InstalledAppsModel installedAppsItem in InstalledAppsList)
                {
                    if (string.IsNullOrEmpty(SearchText))
                    {
                        InstalledAppsCollection.Add(installedAppsItem);
                    }
                    else
                    {
                        if (installedAppsItem.AppName.Contains(SearchText) || installedAppsItem.AppPublisher.Contains(SearchText))
                        {
                            InstalledAppsCollection.Add(installedAppsItem);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
            }
            finally
            {
                InstalledAppsLock.Exit();
            }
            InstalledAppsResultKind = string.IsNullOrEmpty(SearchText) ? InstalledAppsResultKind.Successfully : InstalledAppsResultKind.SearchResult;
        }

        /// <summary>
        /// ��ȡ�Ѱ�װӦ��
        /// </summary>
        private async Task<(ConnectResult, FindPackagesResult, List<InstalledAppsModel>)> InstalledAppsAsync(PackageCatalogReference packageCatalogReference)
        {
            (ConnectResult connectResult, FindPackagesResult findPackagesResult, List<InstalledAppsModel> installedAppsList) installedAppsResult = ValueTuple.Create<ConnectResult, FindPackagesResult, List<InstalledAppsModel>>(null, null, null);

            try
            {
                ConnectResult connectResult = await packageCatalogReference.ConnectAsync();
                installedAppsResult.connectResult = connectResult;

                if (connectResult is not null && connectResult.Status is ConnectResultStatus.Ok)
                {
                    FindPackagesOptions findPackagesOptions = new();
                    FindPackagesResult findPackagesResult = await connectResult.PackageCatalog.FindPackagesAsync(findPackagesOptions);
                    installedAppsResult.findPackagesResult = findPackagesResult;

                    if (findPackagesResult is not null && findPackagesResult.Status is FindPackagesResultStatus.Ok)
                    {
                        List<InstalledAppsModel> installedAppsList = [];

                        for (int index = 0; index < findPackagesResult.Matches.Count; index++)
                        {
                            MatchResult matchItem = findPackagesResult.Matches[index];

                            if (matchItem.CatalogPackage is not null && !string.IsNullOrEmpty(matchItem.CatalogPackage.InstalledVersion.Publisher))
                            {
                                bool isUninstalling = false;
                                InstalledAppsLock.Enter();
                                try
                                {
                                    foreach (InstalledAppsModel installedAppsItem in InstalledAppsList)
                                    {
                                        if (Equals(matchItem.CatalogPackage.InstalledVersion.Id, installedAppsItem.AppID) && Equals(matchItem.CatalogPackage.InstalledVersion.Version, installedAppsItem.AppVersion))
                                        {
                                            isUninstalling = true;
                                            break;
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
                                }
                                finally
                                {
                                    InstalledAppsLock.Exit();
                                }

                                installedAppsList.Add(new InstalledAppsModel()
                                {
                                    AppID = matchItem.CatalogPackage.InstalledVersion.Id,
                                    AppName = string.IsNullOrEmpty(matchItem.CatalogPackage.InstalledVersion.DisplayName) ? Unknown : matchItem.CatalogPackage.InstalledVersion.DisplayName,
                                    AppPublisher = string.IsNullOrEmpty(matchItem.CatalogPackage.InstalledVersion.Publisher) ? Unknown : matchItem.CatalogPackage.InstalledVersion.Publisher,
                                    AppVersion = string.IsNullOrEmpty(matchItem.CatalogPackage.InstalledVersion.Version) ? Unknown : matchItem.CatalogPackage.InstalledVersion.Version,
                                    IsUninstalling = isUninstalling,
                                    CatalogPackage = matchItem.CatalogPackage,
                                });
                            }
                        }

                        if (SelectedAppSortRuleKind is AppSortRuleKind.DisplayName)
                        {
                            if (IsIncrease)
                            {
                                installedAppsList.Sort((item1, item2) => item1.AppName.CompareTo(item2.AppName));
                            }
                            else
                            {
                                installedAppsList.Sort((item1, item2) => item2.AppName.CompareTo(item1.AppName));
                            }
                        }
                        else
                        {
                            if (IsIncrease)
                            {
                                installedAppsList.Sort((item1, item2) => item1.AppPublisher.CompareTo(item2.AppPublisher));
                            }
                            else
                            {
                                installedAppsList.Sort((item1, item2) => item2.AppPublisher.CompareTo(item1.AppPublisher));
                            }
                        }

                        installedAppsResult.installedAppsList = installedAppsList;
                    }
                }
            }
            catch (Exception e)
            {
                LogService.WriteLog(LoggingLevel.Error, "Uninstall winget app failed", e);
            }

            return installedAppsResult;
        }

        /// <summary>
        /// ��ȡ����Ӧ���Ƿ�ɹ�
        /// </summary>
        private Visibility GetInstalledAppsSuccessfullyState(InstalledAppsResultKind installedAppsResultKind, int count, bool isSuccessfully)
        {
            if (isSuccessfully)
            {
                if (InstalledAppsResultKind.Equals(InstalledAppsResultKind.Successfully))
                {
                    return Visibility.Visible;
                }
                else if (InstalledAppsResultKind.Equals(InstalledAppsResultKind.SearchResult))
                {
                    return count > 0 ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
            else
            {
                if (InstalledAppsResultKind.Equals(InstalledAppsResultKind.Successfully))
                {
                    return Visibility.Collapsed;
                }
                else if (InstalledAppsResultKind.Equals(InstalledAppsResultKind.SearchResult))
                {
                    return count > 0 ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// �������Ӧ���Ƿ�ɹ�
        /// </summary>
        private Visibility CheckInstalledAppsState(InstalledAppsResultKind installedAppsResultKind, InstalledAppsResultKind comparedInstalledAppsResultKind)
        {
            return installedAppsResultKind.Equals(comparedInstalledAppsResultKind) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// ��ȡ�Ƿ�����������
        /// </summary>

        private bool GetIsInstalling(InstalledAppsResultKind installedAppsResultKind)
        {
            return !installedAppsResultKind.Equals(InstalledAppsResultKind.Querying);
        }
    }
}
