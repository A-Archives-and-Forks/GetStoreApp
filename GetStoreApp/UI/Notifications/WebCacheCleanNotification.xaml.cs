using GetStoreApp.Views.CustomControls.Notifications;

namespace GetStoreApp.UI.Notifications
{
    /// <summary>
    /// ��ҳ��������ɹ���Ӧ����֪ͨ��ͼ
    /// </summary>
    public sealed partial class WebCacheCleanNotification : InAppNotification
    {
        public WebCacheCleanNotification(bool setResult = false)
        {
            InitializeComponent();
            ViewModel.Initialize(setResult);
        }
    }
}
