using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using Windows.Storage.Streams;

namespace GetStoreApp.Views.Pages
{
    /// <summary>
    /// WinGet �����ҳ��
    /// </summary>
    public sealed partial class WinGetPage : Page
    {
        public WinGetPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// �����������հ�װ����ͼ��
        /// </summary>
        public async void OnTaskEmptyLoaded(object sender, RoutedEventArgs args)
        {
            InMemoryRandomAccessStream memoryStream = new InMemoryRandomAccessStream();
            DataWriter datawriter = new DataWriter(memoryStream.GetOutputStreamAt(0));
            datawriter.WriteBytes(Properties.Resources.TaskEmpty);
            await datawriter.StoreAsync();
            BitmapImage image = new BitmapImage();
            await image.SetSourceAsync(memoryStream);
            (sender as Image).Source = image;
        }
    }
}
