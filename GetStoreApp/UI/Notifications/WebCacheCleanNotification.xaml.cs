using GetStoreApp.Views.CustomControls.Notifications;
using System.Runtime.InteropServices;

namespace GetStoreApp.UI.Notifications
{
    /// <summary>
    /// ��ҳ��������ɹ���Ӧ����֪ͨ��ͼ
    /// </summary>
    public sealed partial class WebCacheCleanNotification : InAppNotification
    {
        public WebCacheCleanNotification([Optional, DefaultParameterValue(false)] bool setResult)
        {
            XamlRoot = Program.ApplicationRoot.MainWindow.GetMainWindowXamlRoot();
            InitializeComponent();
            ViewModel.Initialize(setResult);
        }
    }
}
