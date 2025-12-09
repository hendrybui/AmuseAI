using System;
using System.Globalization;
using System.Windows.Data;

namespace Amuse.UI.Converters
{

    public class DownloadSpeedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double bytesPerSecond)
            {
                const double KB = 1024;
                const double MB = KB * 1024;
                const double GB = MB * 1024;

                if (bytesPerSecond >= GB)
                {
                    return $"{bytesPerSecond / GB:0.00} GB/s";
                }
                else if (bytesPerSecond >= MB)
                {
                    return $"{bytesPerSecond / MB:0.00} MB/s";
                }
                else if (bytesPerSecond >= KB)
                {
                    return $"{bytesPerSecond / KB:0.00} KB/s";
                }
                else if (bytesPerSecond > 0)
                {
                    return $"{bytesPerSecond:0.00} B/s";
                }
            }
            return "0.00 B/s";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

   
    public class DownloadAmountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double bytes)
            {
                const long KB = 1024;
                const long MB = KB * 1024;
                const long GB = MB * 1024;

                if (bytes >= GB)
                {
                    return $"{(double)bytes / GB:0.00} GB";
                }
                else if (bytes >= MB)
                {
                    return $"{(double)bytes / MB:0.00} MB";
                }
                else if (bytes >= KB)
                {
                    return $"{(double)bytes / KB:0.00} KB";
                }
                else
                {
                    return $"{bytes} Bytes";
                }
            }
            return "0 Bytes";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }



    public class DownloadTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double seconds)
            {
                TimeSpan time = TimeSpan.FromSeconds(seconds);

                if (time.TotalHours >= 1)
                {
                    return $"{(int)time.TotalHours}h {time.Minutes}m {time.Seconds:00}s";
                }
                else if (time.TotalMinutes >= 1)
                {
                    return $"{(int)time.TotalMinutes}m {time.Seconds:00}s";
                }
                else
                {
                    return $"{time.Seconds:00}s";
                }
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
