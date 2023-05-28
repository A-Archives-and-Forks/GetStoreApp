using Microsoft.UI.Xaml.Controls;

namespace GetStoreApp.UI.Controls.Settings.Common
{
    /// <summary>
    /// WinGet ��������ÿؼ���ͼ
    /// </summary>
    public sealed partial class WinGetConfigControl : UserControl
    {
        public WinGetConfigControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// �ж������汾�Ƿ�ͬ����
        /// </summary>
        public bool IsBothVersionExisted(bool isOfficialVersionExisted, bool isDevVersionExisted)
        {
            return isOfficialVersionExisted && isDevVersionExisted;
        }

        /// <summary>
        /// �ж� WinGet ������Ƿ����
        /// </summary>
        public bool IsWinGetExisted(bool isOfficialVersionExisted, bool isDevVersionExisted)
        {
            return isOfficialVersionExisted || isDevVersionExisted;
        }
    }
}
