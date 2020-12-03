using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace SaneWpf.Framework.Internal
{
    class AggregateSeverityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var items = value as IEnumerable<ValidationError>;
            if (items == null) return "";

            IssueSeverity severity = IssueSeverity.Info;
            foreach (var issue in items.Select(x => x.ErrorContent).Cast<ValidationIssue>())
            {
                if (issue.Severity > severity)
                {
                    severity = issue.Severity;
                }
            }

            return severity;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }
}
