using GetStoreApp.Services.Controls.Settings.Appearance;
using GetStoreApp.Views.CustomControls.DialogsAndFlyouts;
using Microsoft.UI.Xaml;
using System;

namespace GetStoreApp.UI.Dialogs.Common
{
    /// <summary>
    /// Ӧ�ó������������жԻ�����ͼ
    /// </summary>
    public sealed partial class AppRunningDialog : ExtendedContentDialog
    {
        public ElementTheme DialogTheme { get; } = (ElementTheme)Enum.Parse(typeof(ElementTheme), ThemeService.AppTheme.InternalName);

        public AppRunningDialog()
        {
            InitializeComponent();
        }
    }
}
