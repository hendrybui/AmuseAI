using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Amuse.UI.Converters
{
    [ValueConversion(typeof(int), typeof(List<int>))]
    public class DivisibleByConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return GetDivisors(intValue)
                 .Where(x => x >= 8)
                 .OrderBy(x => x)
                 .ToList();
            }
            return new List<int>();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }


        private static IEnumerable<int> GetDivisors(int number)
        {
            for (int i = 1; i <= Math.Sqrt(number); i++)
            {
                if (number % i == 0)
                {
                    yield return i;
                    if (i != number / i)
                    {
                        yield return number / i;
                    }
                }
            }
        }
    }
}
