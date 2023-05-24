using GetStoreApp.Services.Root;
using GetStoreApp.Views.CustomControls.DialogsAndFlyouts;

namespace GetStoreApp.UI.Dialogs.WinGet
{
    /// <summary>
    /// ��װʧ����ʾ�Ի�����ͼ
    /// </summary>
    public sealed partial class InstallFailedDialog : ExtendedContentDialog
    {
        public InstallFailedDialog(string appName)
        {
            InitializeComponent();
            ViewModel.InstallFailedContent = string.Format(ResourceService.GetLocalized("Dialog/InstallFailedContent"), appName);
        }
    }
}
