using Microsoft.UI.Xaml.Controls;

namespace GetStoreApp.Views.Pages
{
    /// <summary>
    /// WinGet �����ҳ��
    /// </summary>
    public sealed partial class WinGetPage : Page
    {
        public WinGetPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ��ʼ�� WinGet �������ͼģ��
        /// </summary>
        private void OnInitializeSuccessLoaded()
        {
            SearchApps.ViewModel.WinGetVMInstance = ViewModel;
            UpgradableApps.ViewModel.WinGetVMInstance = ViewModel;
        }

        /// <summary>
        /// �ж� WinGet ������Ƿ����
        /// </summary>
        public bool IsWinGetExisted(bool isOfficialVersionExisted, bool isDevVersionExisted, bool needReverseValue)
        {
            bool result = isOfficialVersionExisted || isDevVersionExisted;
            if (needReverseValue)
            {
                return !result;
            }
            else
            {
                return result;
            }
        }
    }
}
