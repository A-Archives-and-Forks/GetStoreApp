using GetStoreApp.Services.Controls.Settings.Appearance;
using GetStoreApp.Views.CustomControls.DialogsAndFlyouts;
using Microsoft.UI.Xaml;
using System;

namespace GetStoreApp.UI.Dialogs.About
{
    /// <summary>
    /// Ӧ����Ϣ�Ի�����ͼ
    /// </summary>
    public sealed partial class AppInformationDialog : ExtendedContentDialog
    {
        public ElementTheme DialogTheme { get; } = (ElementTheme)Enum.Parse(typeof(ElementTheme), ThemeService.AppTheme.InternalName);

        public AppInformationDialog()
        {
            InitializeComponent();
            ViewModel.InitializeAppInformation();
        }

        /// <summary>
        /// �رնԻ���
        /// </summary>
        public void OnCloseDialogClicked(object sender, RoutedEventArgs args)
        {
            Hide();
        }

        /// <summary>
        /// ����Ӧ����Ϣ
        /// </summary>
        public void OnCopyAppInformationClicked(object sender, RoutedEventArgs args)
        {
            ViewModel.CopyAppInformation();
            Hide();
        }
    }
}
