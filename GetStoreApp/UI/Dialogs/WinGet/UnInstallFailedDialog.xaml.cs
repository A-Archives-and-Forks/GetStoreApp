using GetStoreApp.Services.Controls.Settings.Appearance;
using GetStoreApp.Services.Root;
using GetStoreApp.Views.CustomControls.DialogsAndFlyouts;
using Microsoft.UI.Xaml;
using System;

namespace GetStoreApp.UI.Dialogs.WinGet
{
    /// <summary>
    /// ж��ʧ����ʾ�Ի�����ͼ
    /// </summary>
    public sealed partial class UnInstallFailedDialog : ExtendedContentDialog
    {
        public ElementTheme DialogTheme { get; } = (ElementTheme)Enum.Parse(typeof(ElementTheme), ThemeService.AppTheme.InternalName);

        public UnInstallFailedDialog(string appName)
        {
            InitializeComponent();
            ViewModel.UnInstallFailedContent = string.Format(ResourceService.GetLocalized("Dialog/UnInstallFailedContent"), appName);
        }
    }
}
