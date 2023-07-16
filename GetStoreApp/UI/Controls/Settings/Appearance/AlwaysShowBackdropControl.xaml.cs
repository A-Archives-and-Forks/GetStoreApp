using GetStoreApp.Services.Controls.Settings.Appearance;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using WinRT;

namespace GetStoreApp.UI.Controls.Settings.Appearance
{
    /// <summary>
    /// ����ҳ�棺ʼ����ʾ����ɫ���ÿؼ�
    /// </summary>
    public sealed partial class AlwaysShowBackdropControl : Grid, INotifyPropertyChanged
    {
        private bool _alwaysShowBackdropValue = AlwaysShowBackdropService.AlwaysShowBackdropValue;

        public bool AlwaysShowBackdropValue
        {
            get { return _alwaysShowBackdropValue; }

            set
            {
                _alwaysShowBackdropValue = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AlwaysShowBackdropValue)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public AlwaysShowBackdropControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ���ذ�ť�л�ʱ�޸���Ӧ����
        /// </summary>
        public async void OnToggled(object sender, RoutedEventArgs args)
        {
            ToggleSwitch toggleSwitch = sender.As<ToggleSwitch>();
            if (toggleSwitch is not null)
            {
                await AlwaysShowBackdropService.SetAlwaysShowBackdropAsync(toggleSwitch.IsOn);
                AlwaysShowBackdropValue = toggleSwitch.IsOn;
            }
        }
    }
}
