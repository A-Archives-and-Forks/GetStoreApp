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
        public AppInformationDialog()
        {
            InitializeComponent();
            ViewModel.InitializeAppInformation();
        }
    }
}
