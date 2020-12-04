using System.Windows;
using System.Windows.Input;
using SaneWpf.Framework;

namespace SaneWpf.Attached
{
    public static class ViewModelProperties
    {
        public static readonly DependencyProperty InitCommandProperty = DependencyProperty.RegisterAttached(
            "InitCommand", typeof(ICommand), typeof(ViewModelProperties), new PropertyMetadata(InitCommandChangedCallback));

        private static void InitCommandChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement frameworkElement)
            {
                frameworkElement.Loaded += FrameworkElementOnLoaded;
                frameworkElement.Style = (Style) Application.Current.FindResource("BusyAnimationStyle");
            }

            async void FrameworkElementOnLoaded(object sender, RoutedEventArgs _)
            {
                frameworkElement.Loaded -= FrameworkElementOnLoaded;

                if (e.NewValue is AsyncCommand asyncCommand)
                    await asyncCommand.ExecuteAsync(null);
                else if (e.NewValue is ICommand command)
                    command.Execute(null);

                frameworkElement.Style = null;
            }
        }

        public static void SetInitCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(InitCommandProperty, value);
        }

        public static ICommand GetInitCommand(DependencyObject element)
        {
            return (ICommand) element.GetValue(InitCommandProperty);
        }
    }
}
