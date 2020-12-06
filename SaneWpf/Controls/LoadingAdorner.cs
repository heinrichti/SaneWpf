using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace SaneWpf.Controls
{
    public class LoadingAdorner : Adorner
    {
        private readonly FrameworkElement _visual;
        
        public LoadingAdorner(UIElement adornedElement) : base(adornedElement)
        {
            _visual = new Control
            {
                Style = (Style) FindResource("LoadingIndicator"),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            AddVisualChild(_visual);
        }

        protected override Visual GetVisualChild(int index) => _visual;
        protected override int VisualChildrenCount => 1;

        protected override Size MeasureOverride(Size constraint)
        {
            _visual.Measure(constraint);
            return constraint;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _visual.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            return _visual.RenderSize;
        }

    }
}
