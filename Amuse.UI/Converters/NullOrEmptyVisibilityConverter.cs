using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Amuse.UI.Converters
{
    [ValueConversion(typeof(object), typeof(bool))]
    public class NullOrEmptyVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value?.ToString()))
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
