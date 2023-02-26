using GetStoreApp.Extensions.DataType.Enums;
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
    /// ��ݲ���Ӧ����֪ͨ��ͼ
    /// </summary>
    public sealed partial class QuickOperationNotification : UserControl
    {
        public ElementTheme NotificationTheme { get; } = (ElementTheme)Enum.Parse(typeof(ElementTheme), ThemeService.AppTheme.InternalName);

        private Popup Popup { get; set; } = new Popup();

        private int Duration = 2000;

        public QuickOperationNotification(QuickOperationType operationType, [Optional, DefaultParameterValue(false)] bool isPinnedSuccessfully, [Optional] double? duration)
        {
            InitializeComponent();
            ViewModel.Initialize(operationType, isPinnedSuccessfully);

            SetPopUpPlacement();

            Popup.Child = this;
            Popup.XamlRoot = Program.ApplicationRoot.MainWindow.GetMainWindowXamlRoot();

            if (duration.HasValue)
            {
                Duration = Convert.ToInt32(duration * 1000);
            }
        }

        public bool ControlLoad(QuickOperationType operationType, bool isPinnedSuccessfully, string controlName)
        {
            if (controlName is "DesktopShortcutSuccess" && operationType is QuickOperationType.DesktopShortcut && isPinnedSuccessfully)
            {
                return true;
            }
            else if (controlName is "DesktopShortcutFailed" && operationType is QuickOperationType.DesktopShortcut && !isPinnedSuccessfully)
            {
                return true;
            }
            else if (controlName is "StartScreenSuccess" && operationType is QuickOperationType.StartScreen && isPinnedSuccessfully)
            {
                return true;
            }
            else if (controlName is "StartScreenFailed" && operationType is QuickOperationType.StartScreen && !isPinnedSuccessfully)
            {
                return true;
            }
            else if (controlName is "TaskbarSuccess" && operationType is QuickOperationType.Taskbar && isPinnedSuccessfully)
            {
                return true;
            }
            else if (controlName is "TaskbarFailed" && operationType is QuickOperationType.Taskbar && !isPinnedSuccessfully)
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
