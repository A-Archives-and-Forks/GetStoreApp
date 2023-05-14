using GetStoreApp.Services.Controls.Settings.Appearance;
using GetStoreApp.Views.CustomControls.DialogsAndFlyouts;
using Microsoft.UI.Xaml;
using System;

namespace GetStoreApp.UI.Dialogs.Common
{
    /// <summary>
    /// ��Ļ����֪ͨ�Ի�����ͼ
    /// </summary>
    public sealed partial class DPIChangedNotifyDialog : ExtendedContentDialog
    {
        public ElementTheme DialogTheme { get; } = (ElementTheme)Enum.Parse(typeof(ElementTheme), ThemeService.AppTheme.InternalName);

        public DPIChangedNotifyDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// �رնԻ���
        /// </summary>
        public void OnCloseDialogClicked(object sender, RoutedEventArgs args)
        {
            Hide();
        }

        /// <summary>
        /// ����Ӧ��
        /// </summary>
        public async void OnRestartClicked(object sender, RoutedEventArgs args)
        {
            Hide();
            await ViewModel.RestartAppsAsync();
        }
    }
}
