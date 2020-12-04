using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SaneWpf.Attached
{
    public static class StackPanelProperties
    {
        public static readonly DependencyProperty MarginProperty = DependencyProperty.RegisterAttached(
            "Margin", typeof(Thickness), typeof(StackPanelProperties), new PropertyMetadata(MarginChangedCallback));

        private static void MarginChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement frameworkElement)
            {
                if (!frameworkElement.IsLoaded)
                    frameworkElement.Loaded += OnLoaded;
                else
                    OnLoaded(frameworkElement, null);
            }
        }

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is Panel panel)
            {
                panel.Loaded -= OnLoaded;
                var margin = GetMargin(panel);
                foreach (var child in panel.Children.OfType<FrameworkElement>()) 
                    child.Margin = margin;
            }
        }

        public static void SetMargin(DependencyObject element, Thickness value)
        {
            element.SetValue(MarginProperty, value);
        }

        public static Thickness GetMargin(DependencyObject element)
        {
            return (Thickness) element.GetValue(MarginProperty);
        }
    }
}
