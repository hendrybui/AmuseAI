using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Amuse.UI.Converters
{
    [ValueConversion(typeof(Enum), typeof(Visibility))]
    public class EnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is Enum enumValue)
            {
                if (parameter is Enum enumParameter && enumValue == enumParameter)
                    return Visibility.Visible;

                if (enumValue.ToString() == parameter?.ToString())
                    return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(Enum), typeof(Visibility))]
    public class EnumToHiddenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is Enum enumValue)
            {
                if (parameter is Enum enumParameter && enumValue == enumParameter)
                    return Visibility.Visible;

                if (enumValue.ToString() == parameter?.ToString())
                    return Visibility.Visible;
            }

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(Enum), typeof(Visibility))]
    public class InverseEnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is Enum enumValue)
            {
                if (parameter is Enum enumParameter && enumValue == enumParameter)
                    return Visibility.Collapsed;

                if (enumValue.ToString() == parameter?.ToString())
                    return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(Enum), typeof(Visibility))]
    public class InverseEnumToHiddenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is Enum enumValue)
            {
                if (parameter is Enum enumParameter && enumValue == enumParameter)
                    return Visibility.Hidden;

                if (enumValue.ToString() == parameter?.ToString())
                    return Visibility.Hidden;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
