using Amuse.UI.Models.StableDiffusion;
using OnnxStack.StableDiffusion.Enums;
using System.Threading.Tasks;

namespace Amuse.UI.Views
{
    /// <summary>
    /// Interaction logic for PaintToImageView.xaml
    /// </summary>
    public partial class PaintToImageView : StableDiffusionImageViewBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaintToImageView"/> class.
        /// </summary>
        public PaintToImageView()
        {
            SupportedDiffusers = new() { DiffuserType.ImageToImage, DiffuserType.ControlNet, DiffuserType.ControlNetImage };
            SchedulerOptions = new SchedulerOptionsModel { SchedulerType = SchedulerType.DDPM };
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

        protected override Task Cancel()
        {
            return base.Cancel();
        }
    }
}