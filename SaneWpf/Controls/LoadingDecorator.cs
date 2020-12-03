using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace SaneWpf.Controls
{
    [ContentProperty(nameof(Content))]
    public class LoadingDecorator : ContentControl
    {
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            if (oldContent is FrameworkElement oldFrameworkElement)
            {
                oldFrameworkElement.Loaded -= ContentLoaded;
            }

            if (newContent is FrameworkElement frameworkElement)
            {
                frameworkElement.Loaded += ContentLoaded;
            }
        }

        public LoadingDecorator()
        {
            Style = (Style) FindResource("BusyAnimationStyle");
        }

        private void ContentLoaded(object sender, RoutedEventArgs e)
        {
            Style = null;
            ((FrameworkElement)sender).Loaded -= ContentLoaded;
        }
    }
}
