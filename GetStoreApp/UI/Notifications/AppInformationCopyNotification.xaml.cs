using GetStoreApp.Views.CustomControls.Notifications;
using Microsoft.UI.Xaml;

namespace GetStoreApp.UI.Notifications
{
    /// <summary>
    /// Ӧ����Ϣ����Ӧ����֪ͨ
    /// </summary>
    public sealed partial class AppInformationCopyNotification : InAppNotification
    {
        public AppInformationCopyNotification(FrameworkElement element) : base(element)
        {
            InitializeComponent();
        }
    }
}
