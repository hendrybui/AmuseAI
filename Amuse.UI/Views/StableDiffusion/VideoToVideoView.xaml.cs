using OnnxStack.Core.Video;
using OnnxStack.StableDiffusion.Common;
using OnnxStack.StableDiffusion.Config;
using OnnxStack.StableDiffusion.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace Amuse.UI.Views
{
    /// <summary>
    /// Interaction logic for VideoToVideoView.xaml
    /// </summary>
    public partial class VideoToVideoView : StableDiffusionVideoViewBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VideoToVideoView"/> class.
        /// </summary>
        public VideoToVideoView() : base()
        {
            SupportedDiffusers = [DiffuserType.VideoToVideo, DiffuserType.ControlNetVideo];
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


        protected override async Task<GenerateOptions> GetGenerateOptionsAsync(CancellationToken cancellationToken)
        {
            var generateOptions = await base.GetGenerateOptionsAsync(cancellationToken);
            generateOptions.MotionStrides = PromptOptions.MotionStrides;
            generateOptions.MotionNoiseContext = PromptOptions.MotionNoiseContext;
            generateOptions.MotionContextOverlap = PromptOptions.MotionContextOverlap;
            generateOptions.MotionFrames = PromptOptions.MotionFrames;
            generateOptions.FrameResample = PromptOptions.FrameResample;
            generateOptions.FrameUpSample = PromptOptions.FrameUpSample;
            generateOptions.FrameDownSample = PromptOptions.FrameDownSample;
            if (generateOptions.Diffuser == DiffuserType.ImageToImage)
                generateOptions.Diffuser = DiffuserType.VideoToVideo;
            else if (generateOptions.Diffuser == DiffuserType.ControlNet || generateOptions.Diffuser == DiffuserType.ControlNetImage)
                generateOptions.Diffuser = DiffuserType.ControlNetVideo;
            return generateOptions;
        }


        protected override Task UpdateProgress(DiffusionProgress progress)
        {
            UpdateStatistics(progress);
            if (progress.StepMax > 0)
            {
                if (progress.StepMax != BatchOptions.StepsValue)
                    BatchOptions.StepsValue = progress.StepMax;

                if (progress.StepValue != BatchOptions.StepValue)
                    BatchOptions.StepValue = progress.StepValue;

                UpdateProgress(progress.StepValue, progress.StepMax, progress.Message);
            }
            else if (!string.IsNullOrEmpty(progress.Message))
            {
                UpdateProgress(progress.Message);
            }
            return Task.CompletedTask;
        }


        protected override Task<OnnxVideo> ExecuteContentFilterAsync(OnnxVideo inputVideo, CancellationToken cancellationToken)
        {
            return Task.FromResult(inputVideo);
        }
    }
}