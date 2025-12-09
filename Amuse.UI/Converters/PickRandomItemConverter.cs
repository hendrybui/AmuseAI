using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace Amuse.UI.Converters
{
    public class PickRandomItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IList listValue)
            {
                return listValue[Random.Shared.Next(0, listValue.Count)];
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
