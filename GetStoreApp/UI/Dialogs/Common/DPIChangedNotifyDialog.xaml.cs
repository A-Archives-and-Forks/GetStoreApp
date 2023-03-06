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
    }
}
