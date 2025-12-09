using System;
using System.Globalization;
using System.Windows.Data;

namespace Amuse.UI.Converters
{
    public class ValueToEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum enumValue)
            {
                return System.Convert.ChangeType(enumValue, enumValue.GetTypeCode());
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.Parse(targetType, value.ToString()) as Enum;
        }
    }
}
