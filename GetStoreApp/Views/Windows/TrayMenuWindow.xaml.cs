using GetStoreApp.Helpers.Root;
using GetStoreApp.Services.Controls.Settings.Appearance;
using GetStoreApp.Services.Window;
using GetStoreApp.Views.Pages;
using GetStoreApp.WindowsAPI.PInvoke.User32;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Graphics;
using Windows.System;
using WinRT;
using WinRT.Interop;

namespace GetStoreApp.Views.Windows
{
    /// <summary>
    /// Ӧ���������Ҽ��˵�����
    /// </summary>
    public sealed partial class TrayMenuWindow : Window, INotifyPropertyChanged
    {
        private WNDPROC newWndProc = null;
        private IntPtr oldWndProc = IntPtr.Zero;

        public IntPtr Handle { get; }

        private ElementTheme _windowTheme;

        public ElementTheme WindowTheme
        {
            get { return _windowTheme; }

            set
            {
                _windowTheme = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TrayMenuWindow()
        {
            InitializeComponent();
            Handle = WindowNative.GetWindowHandle(this);

            newWndProc = new WNDPROC(NewWindowProc);
            oldWndProc = SetWindowLongAuto(Handle, WindowLongIndexFlags.GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(newWndProc));

            unchecked { SetWindowLongAuto(Handle, WindowLongIndexFlags.GWL_STYLE, (IntPtr)WindowStyle.WS_POPUPWINDOW); }
            SetWindowLongAuto(Handle, WindowLongIndexFlags.GWL_EXSTYLE, (IntPtr)WindowStyleEx.WS_EX_TOOLWINDOW);

            // �������ڵĴ�С
            AppWindow.MoveAndResize(new RectInt32(-1, -1, 1, 1));
            AppWindow.Presenter.As<OverlappedPresenter>().IsAlwaysOnTop = true;
            AppWindow.Show();
        }

        /// <summary>
        /// ��Ӧ�õ���Ŀ��ҳ
        /// </summary>
        public async void OnProjectDescriptionClicked(object sender, RoutedEventArgs args)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/Gaoyifei1011/GetStoreApp"));
        }

        /// <summary>
        /// ��Ӧ�á����ڡ�ҳ��
        /// </summary>
        public void OnAboutAppClicked(object sender, RoutedEventArgs args)
        {
            // ������ǰ��
            Program.ApplicationRoot.MainWindow.Show();

            if (NavigationService.GetCurrentPageType() != typeof(AboutPage))
            {
                NavigationService.NavigateTo(typeof(AboutPage));
            }
        }

        /// <summary>
        /// �˳�Ӧ��
        /// </summary>
        public void OnExitClicked(object sender, RoutedEventArgs args)
        {
            Program.ApplicationRoot.Dispose();
        }

        /// <summary>
        /// ������
        /// </summary>
        public void OnSettingsClicked(object sender, RoutedEventArgs args)
        {
            // ������ǰ��
            Program.ApplicationRoot.MainWindow.Show();

            if (NavigationService.GetCurrentPageType() != typeof(SettingsPage))
            {
                NavigationService.NavigateTo(typeof(SettingsPage));
            }
        }

        /// <summary>
        /// ��ʾ / ���ش���
        /// </summary>
        public void OnShowOrHideWindowClicked(object sender, RoutedEventArgs args)
        {
            // ���ش���
            if (Program.ApplicationRoot.MainWindow.Visible)
            {
                Program.ApplicationRoot.MainWindow.AppWindow.Hide();
            }
            // ��ʾ����
            else
            {
                Program.ApplicationRoot.MainWindow.Show();
            }
        }

        /// <summary>
        /// ����ָ�����ڵĴ�������
        /// </summary>
        private IntPtr SetWindowLongAuto(IntPtr hWnd, WindowLongIndexFlags nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
            {
                return User32Library.SetWindowLongPtr(hWnd, nIndex, dwNewLong);
            }
            else
            {
                return User32Library.SetWindowLong(hWnd, nIndex, dwNewLong);
            }
        }

        /// <summary>
        /// ����ֵ�����仯ʱ֪ͨ����
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// ������Ϣ����
        /// </summary>
        private IntPtr NewWindowProc(IntPtr hWnd, WindowMessage Msg, IntPtr wParam, IntPtr lParam)
        {
            switch (Msg)
            {
                // ���ڴ�С��������ʱ����Ϣ
                case WindowMessage.WM_GETMINMAXINFO:
                    {
                        MINMAXINFO minMaxInfo = Marshal.PtrToStructure<MINMAXINFO>(lParam);
                        minMaxInfo.ptMinTrackSize.X = DPICalcHelper.ConvertEpxToPixel(hWnd, 0);
                        minMaxInfo.ptMinTrackSize.Y = DPICalcHelper.ConvertEpxToPixel(hWnd, 0);
                        Marshal.StructureToPtr(minMaxInfo, lParam, true);
                        break;
                    }
                // ϵͳ����ѡ�������ʱ����Ϣ
                case WindowMessage.WM_SETTINGCHANGE:
                    {
                        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
                        {
                            if (ThemeService.NotifyIconMenuTheme == ThemeService.NotifyIconMenuThemeList[1])
                            {
                                WindowTheme = RegistryHelper.GetSystemUsesTheme();
                            }
                        });
                        break;
                    }
            }
            return User32Library.CallWindowProc(oldWndProc, hWnd, Msg, wParam, lParam);
        }
    }
}
