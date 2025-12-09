using Amuse.UI.Models.StableDiffusion;
using OnnxStack.StableDiffusion.Common;
using OnnxStack.StableDiffusion.Config;
using OnnxStack.StableDiffusion.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace Amuse.UI.Views
{
    /// <summary>
    /// Interaction logic for TextToVideoView.xaml
    /// </summary>
    public partial class TextToVideoView : StableDiffusionVideoViewBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextToVideoView"/> class.
        /// </summary>
        public TextToVideoView() : base()
        {
            SupportedDiffusers = [DiffuserType.TextToVideo];
            InitializeComponent();
        }

        public override bool IsUpscalerSupported => true;
        public override bool IsControlNetSupported => false;
        public override bool IsFeatureExtractorSupported => false;

        protected override Task<GenerateOptions> GetGenerateOptionsAsync(CancellationToken cancellationToken)
        {
            var generateOptions = PromptOptionsModel.ToGenerateVideoOptions(PromptOptions, SchedulerOptions, MemoryInfo);
            generateOptions.Diffuser = DiffuserType.TextToVideo;
            return Task.FromResult(generateOptions);
        }

        protected override bool CanExecuteGenerate()
        {
            return !IsGenerating
               && CurrentPipeline?.IsLoaded == true
               && !CanLoadPipeline();
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

    }
}