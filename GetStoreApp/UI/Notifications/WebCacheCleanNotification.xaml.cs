using GetStoreApp.Views.CustomControls.Notifications;
using Microsoft.UI.Xaml;

namespace GetStoreApp.UI.Notifications
{
    /// <summary>
    /// ��ҳ��������Ӧ����֪ͨ
    /// </summary>
    public sealed partial class WebCacheCleanNotification : InAppNotification
    {
        public WebCacheCleanNotification(FrameworkElement element) : base(element)
        {
            InitializeComponent();
        }
    }
}
