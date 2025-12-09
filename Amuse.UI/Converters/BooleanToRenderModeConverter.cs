using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Interop;

namespace Amuse.UI.Converters
{
    public class BooleanToRenderModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RenderMode renderModeValue && renderModeValue == RenderMode.Default)
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
            {
                return RenderMode.Default;
            }
            return RenderMode.SoftwareOnly;
        }
    }
}
