using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
            Loaded += OnLoaded;
        }

        ~ExtendedCheckBox()
        {
            Loaded -= OnLoaded;
        }

        public InputSystemCursorShape Cursor
        {
            get { return (InputSystemCursorShape)GetValue(CursorProperty); }
            set { SetValue(CursorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Cursor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CursorProperty =
            DependencyProperty.Register("Cursor", typeof(InputSystemCursorShape), typeof(ExtendedDropDownButton), new PropertyMetadata(InputSystemCursorShape.Arrow));

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            ProtectedCursor = InputSystemCursor.Create(Cursor);
        }
    }
}
