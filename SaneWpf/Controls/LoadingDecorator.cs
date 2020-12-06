using System;
using System.Windows;
using System.Windows.Documents;

namespace SaneWpf.Controls
{
    public class LoadingDecorator : AdornerDecorator
    {
        private LoadingAdorner _adorner;

        public LoadingDecorator()
        {
            Loaded += OnLoaded;
            IsRunning = true;
        }

        private void OnLoaded(object sender, EventArgs eventArgs)
        {
            _adorner = new LoadingAdorner(this);
            IsRunningChangedCallback(this, new DependencyPropertyChangedEventArgs(IsRunningProperty,
                !IsRunning, IsRunning));
        }

        public static readonly DependencyProperty IsRunningProperty = DependencyProperty.Register(
            "IsRunning", typeof(bool), typeof(LoadingDecorator), new PropertyMetadata(IsRunningChangedCallback));

        private static void IsRunningChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var loadingDecorator = (LoadingDecorator)d;

            if (!loadingDecorator.IsLoaded) return;

            if ((bool) e.NewValue && !(bool) e.OldValue)
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(loadingDecorator);
                adornerLayer.Add(loadingDecorator._adorner);
                if (loadingDecorator.Child != null)
                    loadingDecorator.Child.IsEnabled = false;
            }

            if (!(bool) e.NewValue && (bool) e.OldValue)
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(loadingDecorator);
                adornerLayer.Remove(loadingDecorator._adorner);
                if (loadingDecorator.Child != null)
                    loadingDecorator.Child.IsEnabled = true;
            }
        }

        public bool IsRunning
        {
            get { return (bool) GetValue(IsRunningProperty); }
            set { SetValue(IsRunningProperty, value); }
        }
    }
}
