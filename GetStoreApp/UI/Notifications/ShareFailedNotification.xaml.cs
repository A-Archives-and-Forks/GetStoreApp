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

        private bool IsMultiSelected = false;

        public ShareFailedNotification(bool isMultiSelected = false, int count = 0)
        {
            IsMultiSelected = isMultiSelected;
            Count = count;

            InitializeComponent();
            ViewModel.Initialize(isMultiSelected);
        }

        public void ShareSelectedFailedLoaded(object sender, RoutedEventArgs args)
        {
            if (IsMultiSelected)
            {
                ShareSelectedFailed.Text = string.Format(ResourceService.GetLocalized("Notification/ShareSelectedFailed"), Count);
            }
        }

        public bool ControlLoad(bool isMultiSelected, string controlName)
        {
            if (controlName is "ShareFailed" && !isMultiSelected)
            {
                return true;
            }
            else if (controlName is "ShareSelectedFailed" && isMultiSelected)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
