using GetStoreApp.Views.CustomControls.DialogsAndFlyouts;
using Microsoft.UI.Xaml.Controls;

namespace GetStoreApp.UI.Dialogs.Common
{
    /// <summary>
    /// ��Ļ����֪ͨ�Ի���
    /// </summary>
    public sealed partial class DPIChangedNotifyDialog : ExtendedContentDialog
    {
        public DPIChangedNotifyDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ����Ӧ��
        /// </summary>
        public void OnRestartClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Program.ApplicationRoot.Restart();
        }
    }
}
