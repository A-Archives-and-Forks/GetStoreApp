using Microsoft.UI.Xaml.Controls;

namespace GetStoreAppWebView.Views.Dialogs
{
    /// <summary>
    /// ������ں���Ϣ�Ի���
    /// </summary>
    public sealed partial class BrowserInformationDialog : ContentDialog
    {
        public string BrowserRuntimeVersion { get; set; }

        public BrowserInformationDialog(string browserRuntimeVersion)
        {
            InitializeComponent();
            BrowserRuntimeVersion = browserRuntimeVersion;
        }
    }
}
