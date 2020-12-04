using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SaneWpf.Attached
{
    public static class TextBoxProperties
    {
        public static readonly DependencyProperty IntOnlyProperty = DependencyProperty.RegisterAttached(
            "IntOnly", typeof(bool), typeof(TextBoxProperties), new PropertyMetadata(IntOnlyChanged));

        private static bool IsInt(string text)
        {
            return text.All(c => char.IsDigit(c));
        }

        private static void IntOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextBox textBox)) return;

            if ((bool) e.NewValue)
            {
                textBox.PreviewTextInput += TextBoxPreviewTextInput;
                textBox.PreviewKeyDown += TextBoxOnPreviewKeyDown;
            }
            else
            {
                textBox.PreviewTextInput -= TextBoxPreviewTextInput;
                textBox.PreviewKeyDown -= TextBoxOnPreviewKeyDown;
            }
        }

        private static void TextBoxOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Back)
            {
                var textBox = (TextBox)sender;
                if (textBox.SelectionLength == textBox.Text.Length || textBox.Text.Length == 1 || Keyboard.IsKeyDown(Key.LeftCtrl) ||
                    Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    e.Handled = true;
                    return;
                }
            }

            if (e.Key == Key.Delete)
            {
                var textBox = (TextBox)sender;
                if (textBox.SelectionLength == textBox.Text.Length || textBox.Text.Length == 1 || Keyboard.IsKeyDown(Key.LeftCtrl) ||
                    Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private static void TextBoxPreviewTextInput(object sender, TextCompositionEventArgs textCompositionEventArgs) =>
            textCompositionEventArgs.Handled = !IsInt(textCompositionEventArgs.Text);

        public static void SetIntOnly(DependencyObject element, bool value) => element.SetValue(IntOnlyProperty, value);

        public static bool GetIntOnly(DependencyObject element) => (bool) element.GetValue(IntOnlyProperty);
    }
}
