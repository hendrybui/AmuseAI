using OnnxStack.Core.Video;
using System.Windows.Media.Imaging;

namespace Amuse.UI.Models
{
    public interface IVideoResult
    {
        public string FileName { get; set; }
        public OnnxVideo Video { get; set; }
        BitmapSource Thumbnail { get; set; }
        string FilePrefix { get; }
    }
}