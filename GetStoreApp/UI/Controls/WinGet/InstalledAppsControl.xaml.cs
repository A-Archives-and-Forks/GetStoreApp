using GetStoreApp.Services.Root;
using Microsoft.UI.Xaml.Controls;

namespace GetStoreApp.UI.Controls.WinGet
{
    /// <summary>
    /// WinGet �����ҳ�棺�Ѱ�װӦ�ÿؼ���ͼ
    /// </summary>
    public sealed partial class InstalledAppsControl : UserControl
    {
        public InstalledAppsControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ���ػ�Ӧ������ͳ����Ϣ
        /// </summary>
        public string LocalizeInstalledAppsCountInfo(int count)
        {
            if (count is 0)
            {
                return ResourceService.GetLocalized("WinGet/AppsCountEmpty");
            }
            else
            {
                return string.Format(ResourceService.GetLocalized("WinGet/AppsCountInfo"), count);
            }
        }
    }
}
