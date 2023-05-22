using GetStoreApp.Services.Controls.Settings.Appearance;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GetStoreApp.UI.Notifications
{
    /// <summary>
    /// Ӧ����Ϣ���Ƴɹ���Ӧ����֪ͨ��ͼ
    /// </summary>
    public sealed partial class AppInformationCopyNotification : UserControl
    {
        public ElementTheme NotificationTheme { get; } = (ElementTheme)Enum.Parse(typeof(ElementTheme), ThemeService.AppTheme.InternalName);

        private Popup Popup { get; set; } = new Popup();

        private int Duration = 2000;

        public AppInformationCopyNotification(bool copyState, [Optional] double? duration)
        {
            InitializeComponent();
            ViewModel.Initialize(copyState);

            SetPopUpPlacement();

            Popup.Child = this;
            Popup.XamlRoot = Program.ApplicationRoot.MainWindow.GetMainWindowXamlRoot();

            if (duration.HasValue)
            {
                Duration = Convert.ToInt32(duration * 1000);
            }
        }

        /// <summary>
        /// �ؼ�������ɺ���ʾ��������̬���ÿؼ�λ��
        /// </summary>
        private void NotificationLoaded(object sender, RoutedEventArgs args)
        {
            PopupIn.Begin();
            Program.ApplicationRoot.MainWindow.SizeChanged += NotificationPlaceChanged;
        }

        /// <summary>
        /// �ؼ�ж��ʱ�Ƴ���Ӧ���¼�
        /// </summary>
        private void NotificationUnLoaded(object sender, RoutedEventArgs args)
        {
            Program.ApplicationRoot.MainWindow.SizeChanged -= NotificationPlaceChanged;
        }

        /// <summary>
        /// ���ڴ�С����ʱ�޸�Ӧ����֪ͨ�����λ��
        /// </summary>
        private void NotificationPlaceChanged(object sender, WindowSizeChangedEventArgs args)
        {
            SetPopUpPlacement();
        }

        /// <summary>
        /// Ӧ����֪ͨ���ض�����ʾ���ʱ����
        /// </summary>
        private async void PopupInCompleted(object sender, object e)
        {
            await Task.Delay(Duration);
            PopupOut.Begin();
        }

        /// <summary>
        /// Ӧ����֪ͨ���ض���ж�����ʱ�������رտؼ�
        /// </summary>
        public void PopupOutCompleted(object sender, object e)
        {
            Popup.IsOpen = false;
        }

        /// <summary>
        /// ����PopUp����ʾλ��
        /// </summary>
        private void SetPopUpPlacement()
        {
            Width = Program.ApplicationRoot.MainWindow.Bounds.Width;
            Height = Program.ApplicationRoot.MainWindow.Bounds.Height;

            Popup.VerticalOffset = 75;
        }

        /// <summary>
        /// ��ʾ����
        /// </summary>
        public void Show()
        {
            Popup.IsOpen = true;
        }
    }
}
