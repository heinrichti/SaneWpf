using System.Windows;
using System.Windows.Controls;

namespace SaneWpf.Controls
{
    public class LoadingDecorator : Decorator
    {
        public LoadingDecorator() => Loaded += OnLoaded;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var frameworkElement = (FrameworkElement)Child;
            frameworkElement.Loaded += OnChildLoaded;
            Child = new ContentControl {Style = (Style) FindResource("BusyAnimationStyle")};
        }

        private void OnChildLoaded(object sender, RoutedEventArgs e)
        {
            var child = (FrameworkElement) sender;
            Child = child;
            child.Loaded -= OnLoaded;
        }
    }
}
