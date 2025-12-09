using OnnxStack.StableDiffusion.Config;
using OnnxStack.StableDiffusion.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace Amuse.UI.Views
{
    /// <summary>
    /// Interaction logic for TextToImageView.xaml
    /// </summary>
    public partial class TextToImageView : StableDiffusionImageViewBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextToImageView"/> class.
        /// </summary>
        public TextToImageView() : base()
        {
            SupportedDiffusers = [DiffuserType.TextToImage];
            InitializeComponent();
        }

        public override bool IsUpscalerSupported => true;
        public override bool IsControlNetSupported => false;
        public override bool IsFeatureExtractorSupported => false;

        protected override async Task<GenerateOptions> GetGenerateOptionsAsync(CancellationToken cancellationToken)
        {
            var generateOptions = await base.GetGenerateOptionsAsync(cancellationToken);
            generateOptions.Diffuser = DiffuserType.TextToImage;
            return generateOptions;
        }
    }
}