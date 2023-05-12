using GetStoreApp.Services.Root;
using Microsoft.Management.Deployment;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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
            ViewModel.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(ViewModel.SelectedIndex))
            {
                switch (ViewModel.SelectedIndex)
                {
                    // ����Ӧ��ҳ��
                    case 0:
                        {
                            break;
                        }
                    // �Ѱ�װӦ��ҳ��
                    case 1:
                        {
                            break;
                        }
                    // ������Ӧ��ҳ��
                    case 2:
                        {
                            break;
                        }
                }
            }
        }
    }
}
