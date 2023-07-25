using GetStoreApp.Services.Root;
using GetStoreApp.UI.Notifications;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GetStoreApp.UI.Controls.Settings.Advanced
{
    /// <summary>
    /// ��־��¼�ؼ�
    /// </summary>
    public sealed partial class LogControl : Expander
    {
        public LogControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ����־�ļ���
        /// </summary>
        public async void OnOpenLogFolderClicked(object sender, RoutedEventArgs args)
        {
            await LogService.OpenLogFolderAsync();
        }

        /// <summary>
        /// ���������־��¼
        /// </summary>
        public void OnClearClicked(object sender, RoutedEventArgs args)
        {
            bool result = LogService.ClearLog();
            new LogCleanNotification(this, result).Show();
        }
    }
}
