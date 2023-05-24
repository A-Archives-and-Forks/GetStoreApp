using GetStoreApp.Extensions.DataType.Enums;
using GetStoreApp.Views.CustomControls.DialogsAndFlyouts;

namespace GetStoreApp.UI.Dialogs.WinGet
{
    /// <summary>
    /// <summary>
    /// �����豸�Ի�����ͼ
    /// </summary>
    public sealed partial class RebootDialog : ExtendedContentDialog
    {
        public RebootDialog(WinGetOptionArgs options, string appName)
        {
            InitializeComponent();
            ViewModel.InitializeRebootContent(options, appName);
        }
    }
}
