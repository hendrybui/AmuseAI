using Amuse.UI.Enums;
using Amuse.UI.Models;
using Amuse.UI.Models.StableDiffusion;
using OnnxStack.StableDiffusion.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Amuse.UI.UserControls
{
    /// <summary>
    /// Interaction logic for StableDiffusionVideoInputControl.xaml
    /// </summary>
    public partial class StableDiffusionVideoInputControl : StableDiffusionInputControlBase
    {
        private List<int> _motionFramesOptions;
        private bool _isFrameControlEnabled = true;
        private List<float> _frameRateOptions;

        /// <summary>Initializes a new instance of the <see cref="StableDiffusionVideoInputControl" /> class.</summary>
        public StableDiffusionVideoInputControl()
        {
            FrameRateOptions = new List<float> { 8, 16, 24, 32, 40, 48, 56, 64 };
            IsFrameControlEnabled = true;
            InitializeComponent();
        }


        public List<int> MotionFramesOptions
        {
            get { return _motionFramesOptions; }
            set { _motionFramesOptions = value; NotifyPropertyChanged(); }
        }

        public bool IsFrameControlEnabled
        {
            get { return _isFrameControlEnabled; }
            set { _isFrameControlEnabled = value; }
        }

        public List<float> FrameRateOptions
        {
            get { return _frameRateOptions; }
            set { _frameRateOptions = value; NotifyPropertyChanged(); }
        }


        protected override List<ResolutionOption> GetResolutions(PipelineType pipelineType, ModelType modelType)
        {
            return [
                new ResolutionOption(448, 704, ResolutionType.Vertical, "16:9"),
                new ResolutionOption(384, 640, ResolutionType.Vertical, "16:9"),

                new ResolutionOption(512, 640, ResolutionType.Vertical, "4:3"),
                new ResolutionOption(448, 576, ResolutionType.Vertical, "4:3"),
                new ResolutionOption(384, 512, ResolutionType.Vertical, "4:3"),

                new ResolutionOption(448, 448, ResolutionType.Square, "1:1"),
                new ResolutionOption(512, 512, ResolutionType.Square, "1:1"),
                new ResolutionOption(576, 576, ResolutionType.Square, "1:1"),

                new ResolutionOption(512, 384, ResolutionType.Horizontal, "4:3"),
                new ResolutionOption(576, 448, ResolutionType.Horizontal, "4:3"),
                new ResolutionOption(640, 512, ResolutionType.Horizontal, "4:3"),

                new ResolutionOption(640, 384, ResolutionType.Horizontal, "16:9"),
                new ResolutionOption(704, 448, ResolutionType.Horizontal, "16:9"),
            ];
        }


        protected override async Task OnCurrentPipelineChanged(StableDiffusionPipelineModel oldPipeline, StableDiffusionPipelineModel newPipeline)
        {
            await base.OnCurrentPipelineChanged(oldPipeline, newPipeline);
            if (newPipeline != null)
            {
                ResolutionOptions = GetResolutions(newPipeline.PipelineType, newPipeline.ModelType);
                SelectedResolution = SelectedResolution ?? ResolutionOptions.FirstOrDefault(x => x.Type == ResolutionType.Square && x.Width == newPipeline.SampleSize);
                MotionFramesOptions = Enumerable.Range(newPipeline.ContextSize, 2048)
                    .Where(x => x % newPipeline.ContextSize == 0)
                    .ToList();
            }
        }


        private void MotionFrames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var contextSize = CurrentPipeline?.ContextSize ?? 16;
            PromptOptions.MotionNoiseContext = contextSize;
            IsFrameControlEnabled = DiffuserType == DiffuserType.VideoToVideo || PromptOptions.MotionFrames != contextSize;
        }


        private void FrameRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PromptOptions.VideoOutputFPS = PromptOptions.VideoInputFPS;
        }
    }
}
