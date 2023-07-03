using GetStoreApp.Views.CustomControls.Notifications;

namespace GetStoreApp.UI.Notifications
{
    /// <summary>
    /// Ӧ����Ϣ���Ƴɹ���Ӧ����֪ͨ��ͼ
    /// </summary>
    public sealed partial class AppInformationCopyNotification : InAppNotification
    {
        public AppInformationCopyNotification(bool copyState = false)
        {
            InitializeComponent();
            ViewModel.Initialize(copyState);
        }
    }
}
