using GetStoreApp.Services.Root;
using Microsoft.UI.Xaml.Controls;

namespace GetStoreApp.UI.Controls.WinGet
{
    public sealed partial class UpgradableAppsControl : Grid
    {
        public UpgradableAppsControl()
        {
            InitializeComponent();
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
    }
}
