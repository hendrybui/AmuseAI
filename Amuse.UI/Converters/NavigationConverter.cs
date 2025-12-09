using Amuse.UI.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Amuse.UI.Converters
{
    public class NavigationConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values is null || values.Any(x => x is null))
                return values.Clone();

            var menuId = Enum.Parse<MenuId>(values[0].ToString());
            return menuId switch
            {
                MenuId.Home => new NavigationModel { Menu = MenuId.Home },
                MenuId.Image => new NavigationModel
                {
                    Menu = MenuId.Image,
                    ImageSubmenu = Enum.Parse<ImageSubmenuId>(values[1].ToString()),
                    Image = values[2] as IImageResult
                },
                MenuId.Video => new NavigationModel
                {
                    Menu = MenuId.Video,
                    VideoSubmenu = Enum.Parse<VideoSubmenuId>(values[1].ToString()),
                    Video = values[2] is IVideoResult video ? video  : default,
                    Image = values[2] is IImageResult image ? image : default,
                },
                MenuId.Text => new NavigationModel
                {
                    Menu = MenuId.Text,
                    TextSubmenu = Enum.Parse<TextSubmenuId>(values[1].ToString()),
                },
                MenuId.Model => new NavigationModel { Menu = MenuId.Model },
                _ => default
            };

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}