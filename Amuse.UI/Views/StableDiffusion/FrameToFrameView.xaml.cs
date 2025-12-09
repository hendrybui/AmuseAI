using OnnxStack.Core.Video;
using OnnxStack.StableDiffusion.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace Amuse.UI.Views
{
    /// <summary>
    /// Interaction logic for FrameToFrameView.xaml
    /// </summary>
    public partial class FrameToFrameView : StableDiffusionVideoViewBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FrameToFrameView"/> class.
        /// </summary>
        public FrameToFrameView() : base()
        {
            SupportedDiffusers = [DiffuserType.ImageToImage, DiffuserType.ControlNet, DiffuserType.ControlNetImage];
            InitializeComponent();
        }

        public override bool IsUpscalerSupported => true;
        public override bool IsControlNetSupported => true;
        public override bool IsFeatureExtractorSupported => true;

        protected override Task LoadPipelineAsync()
        {
            ResetControlVideoCache();
            return base.LoadPipelineAsync();
        }


        protected override Task<OnnxVideo> ExecuteContentFilterAsync(OnnxVideo inputVideo, CancellationToken cancellationToken)
        {
            return Task.FromResult(inputVideo);
        }
    }
}