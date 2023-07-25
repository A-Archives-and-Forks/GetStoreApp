using GetStoreApp.Extensions.DataType.Enums;
using GetStoreApp.Views.CustomControls.Notifications;
using Microsoft.UI.Xaml;
using System.ComponentModel;

namespace GetStoreApp.UI.Notifications
{
    /// <summary>
    /// WinGet �����Ӧ�ð�װ��ж�ء�����ָ���Ӧ����֪ͨ
    /// </summary>
    public sealed partial class WinGetCopyNotification : InAppNotification, INotifyPropertyChanged
    {
        private WinGetOptionArgs _optionArgs;

        public WinGetOptionArgs OptionArgs
        {
            get { return _optionArgs; }

            set
            {
                _optionArgs = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OptionArgs)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public WinGetCopyNotification(FrameworkElement element, WinGetOptionArgs optionArgs) : base(element)
        {
            InitializeComponent();
            OptionArgs = optionArgs;
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
