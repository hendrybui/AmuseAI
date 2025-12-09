using System;
using System.Globalization;
using System.Windows.Data;

namespace Amuse.UI.Converters
{
    [ValueConversion(typeof(Enum), typeof(bool))]
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is Enum enumValue)
            {
                if (parameter is Enum enumParameter && enumValue == enumParameter)
                    return true;

                if (enumValue.ToString() == parameter?.ToString())
                    return true;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


    [ValueConversion(typeof(Enum), typeof(bool))]
    public class InverseEnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is Enum enumValue)
            {
                if (parameter is Enum enumParameter && enumValue == enumParameter)
                    return false;

                if (enumValue.ToString() == parameter?.ToString())
                    return false;
            }

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

}
