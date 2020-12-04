using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SaneWpf.Controls
{
    public class AutoIndexGrid : Grid
    {
        public static readonly DependencyProperty RowBreakProperty = DependencyProperty.RegisterAttached(
            "RowBreak", typeof(bool), typeof(AutoIndexGrid),
            new FrameworkPropertyMetadata(RowBreakChangedCallback));

        private static void RowBreakChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (VisualTreeHelper.GetParent(d) is AutoIndexGrid grid) 
                RefreshChildIndexes(grid);
        }

        public static void SetRowBreak(DependencyObject element, bool value) => element.SetValue(RowBreakProperty, value);

        public static bool GetRowBreak(DependencyObject element) => (bool) element.GetValue(RowBreakProperty);

        public static readonly DependencyProperty RowHeightProperty = DependencyProperty.RegisterAttached(
            "RowHeight", typeof(GridLength), typeof(AutoIndexGrid),
            new FrameworkPropertyMetadata(RowHeightChangedCallback));

        private static void RowHeightChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (VisualTreeHelper.GetParent(d) is AutoIndexGrid grid)
                RefreshChildIndexes(grid);
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            RefreshChildIndexes(this);
        }

        public static void SetRowHeight(DependencyObject element, GridLength value) => element.SetValue(RowHeightProperty, value);

        public static GridLength GetRowHeight(DependencyObject element) => (GridLength) element.GetValue(RowHeightProperty);

        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
            "Columns", typeof(string), typeof(AutoIndexGrid), new FrameworkPropertyMetadata(ColumnsChangedCallback));

        private static void ColumnsChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) => RefreshColumns((AutoIndexGrid) d);

        public string Columns
        {
            get { return (string) GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        private static void RefreshChildIndexes(AutoIndexGrid grid)
        {
            var columns = grid.ColumnDefinitions.Count;

            var currentColumn = 0;
            var currentRow = 0;

            var rowDefinitions = new List<RowDefinition>();
            var currentRowDefinition = new RowDefinition {Height = GridLength.Auto};

            for (int i = 0; i < grid.Children.Count; i++)
            {
                var gridChild = grid.Children[i];

                if (GetRowBreak(gridChild) && currentColumn != 0)
                {
                    rowDefinitions.Add(currentRowDefinition);
                    currentRowDefinition = new RowDefinition {Height = GridLength.Auto};
                    currentColumn = 0;
                    ++currentRow;
                }

                var rowHeight = GetRowHeight(gridChild);
                if (rowHeight != GridLength.Auto)
                    currentRowDefinition = new RowDefinition() {Height = rowHeight};

                SetRow(gridChild, currentRow);
                SetColumn(gridChild, currentColumn);

                if (++currentColumn == columns || i == grid.Children.Count - 1)
                {
                    rowDefinitions.Add(currentRowDefinition);
                    currentRowDefinition = new RowDefinition {Height = GridLength.Auto};
                    ++currentRow;
                    currentColumn = 0;
                }
            }

            grid.RowDefinitions.Clear();

            foreach (var rowDefinition in rowDefinitions) 
                grid.RowDefinitions.Add(rowDefinition);
        }

        private static void RefreshColumns(AutoIndexGrid grid)
        {
            if (string.IsNullOrWhiteSpace(grid.Columns))
                return;

            grid.ColumnDefinitions.Clear();
            var columnDefinitions = grid.Columns.Split(new [] {' ', ','});
            
            foreach (var columnDefinition in GetColumnDefinitions(columnDefinitions)) 
                grid.ColumnDefinitions.Add(columnDefinition);
        }

        private static IEnumerable<ColumnDefinition> GetColumnDefinitions(string[] columnDefinitions)
        {
            foreach (var columnDefinition in columnDefinitions)
            {
                if (columnDefinition == "*")
                    yield return new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)};
                else if (columnDefinition.EndsWith("*"))
                    yield return new ColumnDefinition {Width = new GridLength(double.Parse(columnDefinition.Substring(0, columnDefinition.Length - 1)), GridUnitType.Star)};
                else if (string.Equals(columnDefinition, "Auto", StringComparison.InvariantCultureIgnoreCase))
                    yield return new ColumnDefinition {Width = GridLength.Auto};
                else
                    yield return new ColumnDefinition {Width = new GridLength(double.Parse(columnDefinition))};
                
            }
        }
    }
}
