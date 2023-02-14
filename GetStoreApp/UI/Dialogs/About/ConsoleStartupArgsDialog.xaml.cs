using GetStoreApp.Services.Controls.Settings.Appearance;
using GetStoreApp.Views.CustomControls.DialogsAndFlyouts;
using Microsoft.UI.Xaml;
using System;

namespace GetStoreApp.UI.Dialogs.About
{
    /// <summary>
    /// ����̨��������Ի�����ͼ
    /// </summary>
    public sealed partial class ConsoleStartupArgsDialog : ExtendedContentDialog
    {
        public ElementTheme DialogTheme { get; } = (ElementTheme)Enum.Parse(typeof(ElementTheme), ThemeService.AppTheme.InternalName);

        public ConsoleStartupArgsDialog()
        {
            XamlRoot = Program.ApplicationRoot.MainWindow.GetMainWindowXamlRoot();
            InitializeComponent();
        }
    }
}
