using OnnxStack.StableDiffusion.Enums;
using System.Threading.Tasks;

namespace Amuse.UI.Views
{
    /// <summary>
    /// Interaction logic for ImageToImage.xaml
    /// </summary>
    public partial class ImageToImageView : StableDiffusionImageViewBase
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageToImageView"/> class.
        /// </summary>
        public ImageToImageView() : base()
        {
            SupportedDiffusers = new() { DiffuserType.ImageToImage, DiffuserType.ControlNet, DiffuserType.ControlNetImage };
            InitializeComponent();
        }

        public override bool IsUpscalerSupported => true;
        public override bool IsControlNetSupported => true;
        public override bool IsFeatureExtractorSupported => true;

        protected override Task LoadPipelineAsync()
        {
            ResetControlImageCache();
            return base.LoadPipelineAsync();
        }

        protected override bool CanExecuteGenerate()
        {
            return base.CanExecuteGenerate() && InputImage is not null;
        }
    }
}