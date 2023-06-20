using GetStoreApp.Extensions.DataType.Enums;
using GetStoreApp.Views.CustomControls.Notifications;

namespace GetStoreApp.UI.Notifications
{
    /// <summary>
    /// WinGet �����Ӧ�ð�װ��ж�ء�����ָ���Ӧ����֪ͨ��ͼ
    /// </summary>
    public sealed partial class WinGetCopyNotification : InAppNotification
    {
        public WinGetCopyNotification(bool copyState = false, WinGetOptionArgs optionArgs = WinGetOptionArgs.SearchInstall)
        {
            XamlRoot = Program.ApplicationRoot.MainWindow.GetMainWindowXamlRoot();
            InitializeComponent();
            ViewModel.Initialize(copyState, optionArgs);
        }

        public bool ControlLoaded(bool copyState, WinGetOptionArgs optionArgs, string controlName)
        {
            if (controlName is "SearchInstallCopySuccess" && copyState && optionArgs == WinGetOptionArgs.SearchInstall)
            {
                return true;
            }
            else if (controlName is "SearchInstallCopyFailed" && !copyState && optionArgs == WinGetOptionArgs.SearchInstall)
            {
                return true;
            }
            else if (controlName is "UnInstallCopySuccess" && copyState && optionArgs == WinGetOptionArgs.UnInstall)
            {
                return true;
            }
            else if (controlName is "UnInstallCopyFailed" && !copyState && optionArgs == WinGetOptionArgs.UnInstall)
            {
                return true;
            }
            else if (controlName is "UpgradeInstallCopySuccess" && copyState && optionArgs == WinGetOptionArgs.UpgradeInstall)
            {
                return true;
            }
            else if (controlName is "UpgradeInstallCopyFailed" && !copyState && optionArgs == WinGetOptionArgs.UpgradeInstall)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
