using GetStoreApp.Helpers.Root;
using GetStoreApp.Helpers.Window;
using GetStoreApp.Services.Controls.Settings.Appearance;
using GetStoreApp.Services.Root;
using GetStoreApp.WindowsAPI.PInvoke.DwmApi;
using GetStoreApp.WindowsAPI.PInvoke.Shell32;
using GetStoreApp.WindowsAPI.PInvoke.User32;
using Microsoft.UI.Windowing;
using System;
using System.Runtime.InteropServices;
using Windows.Graphics;
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

        public TrayMenuWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ��ȡ�����ڵĴ��ھ��
        /// </summary>
        public IntPtr GetWindowHandle()
        {
            IntPtr MainWindowHandle = WindowNative.GetWindowHandle(this);

            return MainWindowHandle != IntPtr.Zero
                ? MainWindowHandle
                : throw new ApplicationException(ResourceService.GetLocalized("Resources/WindowHandleInitializeFailed"));
        }

        public void InitializeWindow()
        {
            IntPtr MainWindowHandle = GetWindowHandle();
            newWndProc = new WNDPROC(NewWindowProc);
            oldWndProc = SetWindowLongAuto(MainWindowHandle, WindowLongIndexFlags.GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(newWndProc));

            int setValue = 0;
            int setResult = DwmApiLibrary.DwmSetWindowAttribute(Program.ApplicationRoot.TrayMenuWindow.GetWindowHandle(), DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, ref setValue, Marshal.SizeOf<int>());
            if (setResult is not 0)
            {
                DwmApiLibrary.DwmSetWindowAttribute(Program.ApplicationRoot.TrayMenuWindow.GetWindowHandle(), DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE_OLD, ref setValue, Marshal.SizeOf<int>());
            }

            // ʹ���ص���������ʾӦ�ô��ڡ�
            OverlappedPresenter presenter = OverlappedPresenter.CreateForContextMenu();
            presenter.IsAlwaysOnTop = true;
            AppWindow.SetPresenter(presenter);

            // ���ô�����չ��ʽΪ���ߴ���
            SetWindowLongAuto(GetWindowHandle(), WindowLongIndexFlags.GWL_EXSTYLE, (IntPtr)WindowStyleEx.WS_EX_TOOLWINDOW);

            AppWindow.MoveAndResize(new RectInt32(0, 0, 0, 0));
            AppWindow.Show();
            AppWindow.Hide();
        }

        /// <summary>
        /// ���ô��ڵ�λ��
        /// </summary>
        public void SetWindowPosition(APPBARDATA appbarData, MONITORINFO monitorInfo, PointInt32 windowPos)
        {
            switch (appbarData.uEdge)
            {
                // ������λ������Ļ���
                // ��ʱֻ��Ҫ�жϴ��ڵײ��Ƿ񳬹���Ļ�ĵײ��߽�
                case AppBarEdge.ABE_LEFT:
                    {
                        bool outofScreen = windowPos.Y + AppWindow.Size.Height > monitorInfo.rcMonitor.bottom;
                        if (outofScreen)
                        {
                            AppWindow.Move(new PointInt32(windowPos.X, windowPos.Y - AppWindow.Size.Height));
                        }
                        else
                        {
                            AppWindow.Move(new PointInt32(windowPos.X, windowPos.Y));
                        }
                        break;
                    }
                // ������λ������Ļ����
                // ��ʱֻ���жϴ����Ҳ��Ƿ񳬹���Ļ���Ҳ�߽�
                case AppBarEdge.ABE_TOP:
                    {
                        bool outofScreen = windowPos.X + AppWindow.Size.Width > monitorInfo.rcMonitor.right;
                        if (outofScreen)
                        {
                            AppWindow.Move(new PointInt32(windowPos.X - AppWindow.Size.Width, windowPos.Y));
                        }
                        else
                        {
                            AppWindow.Move(new PointInt32(windowPos.X, windowPos.Y));
                        }
                        break;
                    }
                // ������λ������Ļ�Ҳ�
                // ��ʱֻ��Ҫ�жϴ��ڵײ��Ƿ񳬹���Ļ�ĵײ��߽�
                case AppBarEdge.ABE_RIGHT:
                    {
                        bool outofScreen = windowPos.Y + AppWindow.Size.Height > monitorInfo.rcMonitor.bottom;
                        if (outofScreen)
                        {
                            AppWindow.Move(new PointInt32(windowPos.X - AppWindow.Size.Width, windowPos.Y - AppWindow.Size.Height));
                        }
                        else
                        {
                            AppWindow.Move(new PointInt32(windowPos.X - AppWindow.Size.Width, windowPos.Y));
                        }
                        break;
                    }
                // ������λ������Ļ�ײ�
                // ��ʱֻ���жϴ����Ҳ��Ƿ񳬹���Ļ���Ҳ�߽�
                case AppBarEdge.ABE_BOTTOM:
                    {
                        bool outofScreen = windowPos.X + AppWindow.Size.Width > monitorInfo.rcMonitor.right;
                        if (outofScreen)
                        {
                            AppWindow.Move(new PointInt32(windowPos.X - AppWindow.Size.Width, windowPos.Y - AppWindow.Size.Height));
                        }
                        else
                        {
                            AppWindow.Move(new PointInt32(windowPos.X, windowPos.Y - AppWindow.Size.Height));
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// �������ڵĴ�С
        /// </summary>
        public void SetWindowSize()
        {
            AppWindow.Resize(new SizeInt32(
                DPICalcHelper.ConvertEpxToPixel(Program.ApplicationRoot.TrayIcon.Handle, Convert.ToInt32(TrayMenuFlyout.ActualWidth) + 2),
                DPICalcHelper.ConvertEpxToPixel(Program.ApplicationRoot.TrayIcon.Handle, Convert.ToInt32(TrayMenuFlyout.ActualHeight) + 2)
                ));
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
