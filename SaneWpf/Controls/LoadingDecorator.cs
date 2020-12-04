using System.Windows;
using System.Windows.Controls;

namespace SaneWpf.Controls
{
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
            IsTabStop = false;
        }

        private void ContentLoaded(object sender, RoutedEventArgs e)
        {
            Style = null;
            ((FrameworkElement)sender).Loaded -= ContentLoaded;
        }
    }
}
