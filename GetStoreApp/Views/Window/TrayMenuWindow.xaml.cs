using GetStoreApp.Helpers.Root;
using GetStoreApp.Helpers.Window;
using GetStoreApp.Services.Controls.Settings.Appearance;
using GetStoreApp.Services.Root;
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
        private WindowProc newWndProc = null;
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
            newWndProc = new WindowProc(NewWindowProc);
            oldWndProc = SetWindowLongAuto(MainWindowHandle, WindowLongIndexFlags.GWL_WNDPROC, newWndProc);

            // ʹ���ص���������ʾӦ�ô��ڡ�
            OverlappedPresenter presenter = OverlappedPresenter.CreateForContextMenu();
            presenter.IsAlwaysOnTop = true;
            AppWindow.SetPresenter(presenter);

            // ���ô�����չ��ʽΪ���ߴ���
            SetWindowLongAuto(GetWindowHandle(), WindowLongIndexFlags.GWL_EXSTYLE, WindowStyleEx.WS_EX_TOOLWINDOW);

            User32Library.SetWindowPos(
                GetWindowHandle(),
                IntPtr.Zero,
                0,
                0,
                0,
                0,
                SetWindowPosFlags.SWP_NOREDRAW | SetWindowPosFlags.SWP_NOOWNERZORDER
                );
            AppWindow.Show();
            AppWindow.Hide();
        }

        /// <summary>
        /// ����ָ�����ڵ�����
        /// </summary>
        public IntPtr SetWindowLongAuto(IntPtr hWnd, WindowLongIndexFlags nIndex, WindowStyleEx styleEx)
        {
            if (IntPtr.Size == 8)
            {
                return User32Library.SetWindowLongPtr(hWnd, nIndex, styleEx);
            }
            else
            {
                return User32Library.SetWindowLong(hWnd, nIndex, styleEx);
            }
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
                            User32Library.SetWindowPos(
                                GetWindowHandle(),
                                IntPtr.Zero,
                                windowPos.X,
                                windowPos.Y - AppWindow.Size.Height,
                                0,
                                0,
                                SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOREDRAW | SetWindowPosFlags.SWP_NOZORDER
                                );
                        }
                        else
                        {
                            User32Library.SetWindowPos(
                                GetWindowHandle(),
                                IntPtr.Zero,
                                windowPos.X,
                                windowPos.Y,
                                0,
                                0,
                                SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOREDRAW | SetWindowPosFlags.SWP_NOZORDER
                                );
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
                            User32Library.SetWindowPos(
                                GetWindowHandle(),
                                IntPtr.Zero,
                                windowPos.X - AppWindow.Size.Width,
                                windowPos.Y,
                                0,
                                0,
                                SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOREDRAW | SetWindowPosFlags.SWP_NOZORDER
                                );
                        }
                        else
                        {
                            User32Library.SetWindowPos(
                                GetWindowHandle(),
                                IntPtr.Zero,
                                windowPos.X,
                                windowPos.Y,
                                0,
                                0,
                                SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOREDRAW | SetWindowPosFlags.SWP_NOZORDER
                                );
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
                            User32Library.SetWindowPos(
                                GetWindowHandle(),
                                IntPtr.Zero,
                                windowPos.X - AppWindow.Size.Width, windowPos.Y - AppWindow.Size.Height,
                                0,
                                0,
                                SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOREDRAW | SetWindowPosFlags.SWP_NOZORDER
                                );
                        }
                        else
                        {
                            User32Library.SetWindowPos(
                                GetWindowHandle(),
                                IntPtr.Zero,
                                windowPos.X - AppWindow.Size.Width,
                                windowPos.Y,
                                0,
                                0,
                                SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOREDRAW | SetWindowPosFlags.SWP_NOZORDER
                                );
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
                            User32Library.SetWindowPos(
                                GetWindowHandle(),
                                IntPtr.Zero,
                                windowPos.X - AppWindow.Size.Width,
                                windowPos.Y - AppWindow.Size.Height,
                                0,
                                0,
                                SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOREDRAW | SetWindowPosFlags.SWP_NOZORDER
                                );
                        }
                        else
                        {
                            User32Library.SetWindowPos(
                                GetWindowHandle(),
                                IntPtr.Zero,
                                windowPos.X,
                                windowPos.Y - AppWindow.Size.Height,
                                0,
                                0,
                                SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOREDRAW | SetWindowPosFlags.SWP_NOZORDER
                                );
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
            User32Library.SetWindowPos(
                GetWindowHandle(),
                IntPtr.Zero,
                0,
                0,
                DPICalcHelper.ConvertEpxToPixel(Program.ApplicationRoot.TrayIcon.Handle, Convert.ToInt32(TrayMenuFlyout.ActualWidth) + 2),
                DPICalcHelper.ConvertEpxToPixel(Program.ApplicationRoot.TrayIcon.Handle, Convert.ToInt32(TrayMenuFlyout.ActualHeight) + 2),
                SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOREDRAW | SetWindowPosFlags.SWP_NOZORDER
                );
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
