using GetStoreApp.Helpers.Root;
using GetStoreApp.Services.Controls.Settings.Appearance;
using GetStoreApp.WindowsAPI.PInvoke.User32;
using Microsoft.UI.Windowing;
using System;
using System.Runtime.InteropServices;
using Windows.Graphics;
using WinRT;
using WinRT.Interop;

namespace GetStoreApp.Views.Window
{
    /// <summary>
    /// Ӧ���������Ҽ��˵�����
    /// </summary>
    public sealed partial class TrayMenuWindow : WinUIWindow
    {
        private WNDPROC newWndProc = null;
        private IntPtr oldWndProc = IntPtr.Zero;

        public IntPtr Handle { get; }

        public TrayMenuWindow()
        {
            InitializeComponent();
            Handle = WindowNative.GetWindowHandle(this);
            TrayMenuFlyout.XamlRoot = Content.XamlRoot;
        }

        public void InitializeWindow()
        {
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
                        if (MinWidth >= 0)
                        {
                            minMaxInfo.ptMinTrackSize.X = DPICalcHelper.ConvertEpxToPixel(hWnd, MinWidth);
                        }
                        if (MinHeight >= 0)
                        {
                            minMaxInfo.ptMinTrackSize.Y = DPICalcHelper.ConvertEpxToPixel(hWnd, MinHeight);
                        }
                        if (MaxWidth > 0)
                        {
                            minMaxInfo.ptMaxTrackSize.X = DPICalcHelper.ConvertEpxToPixel(hWnd, MaxWidth);
                        }
                        if (MaxHeight > 0)
                        {
                            minMaxInfo.ptMaxTrackSize.Y = DPICalcHelper.ConvertEpxToPixel(hWnd, MaxHeight);
                        }

                        minMaxInfo.ptMinTrackSize.Y = 0;
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
                                ViewModel.WindowTheme = RegistryHelper.GetSystemUsesTheme();
                            }
                        });
                        break;
                    }
            }
            return User32Library.CallWindowProc(oldWndProc, hWnd, Msg, wParam, lParam);
        }
    }
}
