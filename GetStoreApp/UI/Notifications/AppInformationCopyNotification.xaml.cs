using GetStoreApp.Views.CustomControls.Notifications;
using System.Runtime.InteropServices;

namespace GetStoreApp.UI.Notifications
{
    /// <summary>
    /// Ӧ����Ϣ���Ƴɹ���Ӧ����֪ͨ��ͼ
    /// </summary>
    public sealed partial class AppInformationCopyNotification : InAppNotification
    {
        public AppInformationCopyNotification([Optional, DefaultParameterValue(false)] bool copyState)
        {
            InitializeComponent();
            XamlRoot = Program.ApplicationRoot.MainWindow.GetMainWindowXamlRoot();
            ViewModel.Initialize(copyState);
        }
    }
}
