using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace GetStoreApp.Views.CustomControls.BasicInput
{
    /// <summary>
    /// ��չ��ĸ�ѡ�򣬿�������ָ��λ�ڿؼ���ʱ��ʾ���α�
    /// </summary>
    public sealed partial class ExtendedCheckBox : CheckBox
    {
        public ExtendedCheckBox()
        {
            InitializeComponent();
        }

        public InputSystemCursorShape Cursor
        {
            get { return (InputSystemCursorShape)GetValue(CursorProperty); }
            set { SetValue(CursorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Cursor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CursorProperty =
            DependencyProperty.Register("Cursor", typeof(InputSystemCursorShape), typeof(ExtendedCheckBox), new PropertyMetadata(InputSystemCursorShape.Arrow));

        protected override void OnPointerEntered(PointerRoutedEventArgs args)
        {
            base.OnPointerEntered(args);

            if (ProtectedCursor == null)
            {
                ProtectedCursor = InputSystemCursor.Create(Cursor);
            }
            else
            {
                if ((ProtectedCursor as InputSystemCursor).CursorShape != Cursor)
                {
                    ProtectedCursor = InputSystemCursor.Create(Cursor);
                }
            }
        }
    }
}
