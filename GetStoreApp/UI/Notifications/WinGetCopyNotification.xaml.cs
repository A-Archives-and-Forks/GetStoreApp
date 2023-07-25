using GetStoreApp.Extensions.DataType.Enums;
using GetStoreApp.Views.CustomControls.Notifications;
using Microsoft.UI.Xaml;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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
                OnPropertyChanged();
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

        /// <summary>
        /// ����ֵ�����仯ʱ֪ͨ����
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
