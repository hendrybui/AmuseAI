using Amuse.UI.Enums;
using Amuse.UI.Models.StableDiffusion;
using System.Linq;
using System.Threading.Tasks;

namespace Amuse.UI.UserControls
{
    /// <summary>
    /// Interaction logic for StableDiffusionInputControl.xaml
    /// </summary>
    public partial class StableDiffusionInputControl : StableDiffusionInputControlBase
    {
        private bool _isResolutionEnabled = true;

        /// <summary>Initializes a new instance of the <see cref="StableDiffusionInputControl" /> class.</summary>
        public StableDiffusionInputControl()
        {
            InitializeComponent();
        }


        public bool IsResolutionEnabled
        {
            get { return _isResolutionEnabled; }
            set
            {
                _isResolutionEnabled = value;
                if (SchedulerOptions != null && _isResolutionEnabled)
                {
                    SelectedResolution = ResolutionOptions.FirstOrDefault(x => x.Width == SchedulerOptions.Width && x.Height == SchedulerOptions.Height)
                        ?? ResolutionOptions.FirstOrDefault(x => x.Type == ResolutionType.Square && x.Width == CurrentPipeline.SampleSize);
                }

                NotifyPropertyChanged();
            }
        }


        protected override async Task OnCurrentPipelineChanged(StableDiffusionPipelineModel oldPipeline, StableDiffusionPipelineModel newPipeline)
        {
            await base.OnCurrentPipelineChanged(oldPipeline, newPipeline);
            if (newPipeline != null)
            {
                ResolutionOptions = GetResolutions(newPipeline.PipelineType, newPipeline.ModelType);
                if (IsResolutionEnabled)
                {
                    SelectedResolution = SelectedResolution ?? ResolutionOptions.FirstOrDefault(x => x.Type == ResolutionType.Square && x.Width == newPipeline.SampleSize);
                }
            }
        }

    }
}
