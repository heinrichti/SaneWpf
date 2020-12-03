using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace SaneWpf.Framework.Internal
{
    internal class ErrorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var items = value as IEnumerable<ValidationError>;
            if (items == null) return "";

            var builder = new StringBuilder();

            using (var enumerator = items.Select(x => x.ErrorContent).Select(x => x is Validation v ? v : Validation.Error(x.ToString())).GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    builder.Append(enumerator.Current.Message);
                }

                while (enumerator.MoveNext())
                {
                    builder.AppendLine();
                    builder.Append(enumerator.Current.Message);
                }
            }

            return builder.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
