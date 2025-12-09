using System.Windows.Media.Imaging;

namespace Amuse.UI.Models
{
    public interface IImageResult
    {
        BitmapSource Image { get; init; }
        string FilePrefix { get; }
        int Width { get; }
        int Height { get; }
    }
}