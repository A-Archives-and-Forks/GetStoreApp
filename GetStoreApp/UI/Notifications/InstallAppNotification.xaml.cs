using GetStoreApp.Services.Root;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Threading.Tasks;

namespace GetStoreApp.UI.Notifications
{
    public sealed partial class InstallAppNotification : UserControl
    {
        private Popup Popup { get; set; } = new Popup();

        private int Duration = 2000;

        private string FileName;

        private string ErrorMessage;

        public InstallAppNotification(bool installAppState, string fileName, string errorMessage = null, double? duration = null)
        {
            FileName = fileName;
            ErrorMessage = errorMessage;

            InitializeComponent();
            ViewModel.Initialize(installAppState, errorMessage);

            SetPopUpPlacement();

            Popup.Child = this;
            Popup.XamlRoot = App.MainWindow.Content.XamlRoot;

            if (duration.HasValue)
            {
                Duration = Convert.ToInt32(duration * 1000);
            }
        }

        public void InstallAppSuccessLoaded(object sender, RoutedEventArgs args)
        {
            InstallAppSuccess.Text = string.Format(ResourceService.GetLocalized("/Notification/InstallAppSuccess"), FileName);
        }

        public void InstallAppErrorLoaded(object sender, RoutedEventArgs args)
        {
            InstallAppError.Text = string.Format(ResourceService.GetLocalized("/Notification/InstallAppError"), FileName);
        }

        public void InstallAppErrorInformationLoaded(object sender, RoutedEventArgs args)
        {
            InstallAppErrorInformation.Text = string.Format(ResourceService.GetLocalized("/Notification/InstallAppErrorInformation"), ErrorMessage);
        }

        /// <summary>
        /// �ؼ�������ɺ���ʾ��������̬���ÿؼ�λ��
        /// </summary>
        private void NotificationLoaded(object sender, RoutedEventArgs e)
        {
            PopupIn.Begin();
            App.MainWindow.SizeChanged += NotificationPlaceChanged;
        }

        /// <summary>
        /// �ؼ�ж��ʱ�Ƴ���Ӧ���¼�
        /// </summary>
        private void NotificationUnLoaded(object sender, RoutedEventArgs e)
        {
            App.MainWindow.SizeChanged -= NotificationPlaceChanged;
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
            Width = App.MainWindow.Bounds.Width;
            Height = App.MainWindow.Bounds.Height;

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
