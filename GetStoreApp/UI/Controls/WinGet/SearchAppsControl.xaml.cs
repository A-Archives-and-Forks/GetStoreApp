using GetStoreApp.Services.Root;
using Microsoft.UI.Xaml.Controls;

namespace GetStoreApp.UI.Controls.WinGet
{
    /// <summary>
    /// WinGet �����ҳ�棺����Ӧ�ÿؼ���ͼ
    /// </summary>
    public sealed partial class SearchAppsControl : UserControl
    {
        public SearchAppsControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ���ػ�Ӧ������ͳ����Ϣ
        /// </summary>
        public string LocalizeSearchAppsCountInfo(int count)
        {
            if (count is 0)
            {
                return ResourceService.GetLocalized("WinGet/SearchedAppsCountEmpty");
            }
            else
            {
                return string.Format(ResourceService.GetLocalized("WinGet/SearchedAppsCountInfo"), count);
            }
        }
    }
}
