using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SaneWpf.Controls;
using SaneWpf.Framework;

namespace SaneWpf.Attached
{
    public static class ViewModelProperties
    {
        public static readonly DependencyProperty InitCommandProperty = DependencyProperty.RegisterAttached(
            "InitCommand", typeof(ICommand), typeof(ViewModelProperties), new PropertyMetadata(InitCommandChangedCallback));

        private static async void InitCommandChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(d) || !(d is FrameworkElement frameworkElement))
                return;

            LoadingDecorator loadingDecorator = null;
            if (frameworkElement is ContentControl c)
            {
                var contentControl = c;
                var content = contentControl.Content;
                contentControl.Content = null;
                loadingDecorator = new LoadingDecorator {Child = (UIElement) content};
                contentControl.Content = loadingDecorator;
            }

            if (e.NewValue is AsyncCommand asyncCommand)
                await asyncCommand.ExecuteAsync(null);
            else if (e.NewValue is ICommand command)
                command.Execute(null);

            loadingDecorator.IsRunning = false;
        }

        public static void SetInitCommand(DependencyObject element, ICommand value) => 
            element.SetValue(InitCommandProperty, value);

        public static ICommand GetInitCommand(DependencyObject element) => 
            (ICommand) element.GetValue(InitCommandProperty);
    }
}
