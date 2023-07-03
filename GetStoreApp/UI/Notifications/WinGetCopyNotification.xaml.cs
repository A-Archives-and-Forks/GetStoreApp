using GetStoreApp.Extensions.DataType.Enums;
using GetStoreApp.Views.CustomControls.Notifications;

namespace GetStoreApp.UI.Notifications
{
    /// <summary>
    /// WinGet �����Ӧ�ð�װ��ж�ء�����ָ���Ӧ����֪ͨ��ͼ
    /// </summary>
    public sealed partial class WinGetCopyNotification : InAppNotification
    {
        public WinGetCopyNotification(WinGetOptionArgs optionArgs)
        {
            InitializeComponent();
            ViewModel.Initialize(optionArgs);
        }

        public bool ControlLoaded(WinGetOptionArgs optionArgs, string controlName)
        {
            if (controlName is "SearchInstallCopySuccess" && optionArgs == WinGetOptionArgs.SearchInstall)
            {
                return true;
            }
            else if (controlName is "UnInstallCopySuccess" && optionArgs == WinGetOptionArgs.UnInstall)
            {
                return true;
            }
            else if (controlName is "UpgradeInstallCopySuccess" && optionArgs == WinGetOptionArgs.UpgradeInstall)
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
