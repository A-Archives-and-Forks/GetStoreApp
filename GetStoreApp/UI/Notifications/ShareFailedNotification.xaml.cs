using GetStoreApp.Services.Root;
using GetStoreApp.Views.CustomControls.Notifications;
using Microsoft.UI.Xaml;

namespace GetStoreApp.UI.Notifications
{
    /// <summary>
    /// ����ʧ����Ϣ��ʾ֪ͨ��ͼ
    /// </summary>
    public sealed partial class ShareFailedNotification : InAppNotification
    {
        private int Count = 0;

        public ShareFailedNotification(bool isMultiSelected = false, int count = 0)
        {
            Count = count;

            InitializeComponent();
            ViewModel.Initialize(isMultiSelected);
        }

        public void ShareSelectedFailedLoaded(object sender, RoutedEventArgs args)
        {
            ShareSelectedFailed.Text = string.Format(ResourceService.GetLocalized("Notification/ShareSelectedFailed"), Count);
        }
    }
}
