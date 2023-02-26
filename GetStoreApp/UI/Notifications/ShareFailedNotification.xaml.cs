using GetStoreApp.Services.Controls.Settings.Appearance;
using GetStoreApp.Services.Root;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GetStoreApp.UI.Notifications
{
    public sealed partial class ShareFailedNotification : UserControl
    {
        public ElementTheme NotificationTheme { get; } = (ElementTheme)Enum.Parse(typeof(ElementTheme), ThemeService.AppTheme.InternalName);

        private Popup Popup { get; set; } = new Popup();

        private int Duration = 2000;

        private int Count = 0;

        private bool IsMultiSelected = false;

        public ShareFailedNotification([Optional, DefaultParameterValue(false)] bool isMultiSelected, [Optional, DefaultParameterValue(0)] int count, [Optional] double? duration)
        {
            IsMultiSelected = isMultiSelected;
            Count = count;

            InitializeComponent();
            ViewModel.Initialize(isMultiSelected);

            SetPopUpPlacement();

            Popup.Child = this;
            Popup.XamlRoot = Program.ApplicationRoot.MainWindow.GetMainWindowXamlRoot();

            if (duration.HasValue)
            {
                Duration = Convert.ToInt32(duration * 1000);
            }
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
