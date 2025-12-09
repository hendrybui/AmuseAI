using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Amuse.UI.Converters
{
    public class DoubleAddConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue && parameter is double additional)
            {
                return doubleValue + additional;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


    public class DivideByConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue && int.TryParse(parameter?.ToString(), out var divisor))
            {
                return intValue / divisor;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class MutiplyByConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!int.TryParse(parameter?.ToString(), out var mutiplier))
                return value;

            if (value is int iValue)
                return iValue * mutiplier;
            else if (value is long lValue)
                return lValue * mutiplier;
            else if (value is float fValue)
                return fValue * mutiplier;
            else if (value is double dValue)
                return dValue * mutiplier;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


    public class LessThanToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue && double.TryParse(parameter?.ToString(), out var doubleTarget))
            {
                return doubleValue <= doubleTarget;
            }
            else if (value is float floatValue && float.TryParse(parameter?.ToString(), out var floatTarget))
            {
                return floatValue <= floatTarget;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


    public class GreaterThanToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue && double.TryParse(parameter?.ToString(), out var doubleTarget))
            {
                return doubleValue > doubleTarget;
            }
            else if (value is float floatValue && float.TryParse(parameter?.ToString(), out var floatTarget))
            {
                return floatValue > floatTarget;
            }
            else if (value is int intValue && int.TryParse(parameter?.ToString(), out var intTarget))
            {
                return intValue > intTarget;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


    public class LessThanToHiddenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue && double.TryParse(parameter?.ToString(), out var doubleTarget))
            {
                return doubleValue <= doubleTarget
                    ? Visibility.Hidden
                    : Visibility.Visible;
            }
            else if (value is float floatValue && float.TryParse(parameter?.ToString(), out var floatTarget))
            {
                return floatValue <= floatTarget
                    ? Visibility.Hidden
                    : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
