namespace Amuse.UI.Models
{
    public class NavigationModel
    {
        public MenuId Menu { get; set; }
        public ImageSubmenuId ImageSubmenu { get; set; }
        public VideoSubmenuId VideoSubmenu { get; set; }
        public TextSubmenuId TextSubmenu { get; set; }
        public IImageResult Image { get; set; }
        public IVideoResult Video { get; set; }
    }

    public enum MenuId
    {
        Home = 0,
        Image = 1,
        Video = 2,
        Text = 3,
        Model = 4
    }

    public enum ImageSubmenuId
    {
        TextToImage = 0,
        ImageToImage = 1,
        PaintToImage = 2,
        ImageInpaint = 3,
        Upscaler = 4,
        FeatureExtractor = 5
    }

    public enum VideoSubmenuId
    {
        TextToVideo = 0,
        ImageToVideo = 1,
        VideoToVideo = 2,
        FrameToFrame = 3,
        Upscaler = 4,
        FeatureExtractor = 5
    }

    public enum TextSubmenuId
    {
        TextToText = 0
    }
}
