using Amuse.UI.Models;
using Amuse.UI.Models.StableDiffusion;
using OnnxStack.Core;
using OnnxStack.Core.Image;
using OnnxStack.FeatureExtractor.Common;
using OnnxStack.StableDiffusion.Common;
using OnnxStack.StableDiffusion.Config;
using OnnxStack.StableDiffusion.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace Amuse.UI.Views
{
    /// <summary>
    /// Interaction logic for ImageToVideoView.xaml
    /// </summary>
    public partial class ImageToVideoView : StableDiffusionVideoViewBase
    {
        private ImageInput _inputImage;
        private OnnxImage _inputImageCached;
        private OnnxImage _inputControlImageCached;
        private ImageInput _sourceImage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageToVideoView"/> class.
        /// </summary>
        public ImageToVideoView() : base()
        {
            SupportedDiffusers = [DiffuserType.ImageToVideo];
            InitializeComponent();
        }

        public override bool IsUpscalerSupported => true;
        public override bool IsControlNetSupported => true;
        public override bool IsFeatureExtractorSupported => true;

        public ImageInput SourceImage
        {
            get { return _sourceImage; }
            set { _sourceImage = value; NotifyPropertyChanged(); }
        }

        public ImageInput InputImage
        {
            get { return _inputImage; }
            set
            {
                _inputImage = value;
                _inputImageCached = null;
                _inputControlImageCached = null;
                NotifyPropertyChanged();
            }
        }


        public override Task NavigateAsync(IImageResult navigationResult)
        {
            if (navigationResult is ImageResult imageResult)
            {
                if (imageResult.Image != null)
                {
                    // HasInputResult = true;
                    SourceImage = new ImageInput
                    {
                        Image = imageResult.Image,
                        FileName = "Generated Image"
                    };
                }

                PromptOptions = PromptOptionsModel.FromGenerateOptions(imageResult.PromptOptions);
                SchedulerOptions = SchedulerOptionsModel.FromSchedulerOptions(imageResult.SchedulerOptions);
                SelectedTabIndex = 0;
                IsHistoryView = false;
            }
            return Task.CompletedTask;
        }


        protected override bool CanExecuteGenerate()
        {
            return !IsGenerating
               && CurrentPipeline?.IsLoaded == true
               && !CanLoadPipeline()
               && InputImage is not null;
        }


        protected override async Task<GenerateOptions> GetGenerateOptionsAsync(CancellationToken cancellationToken)
        {
            var generateOptions = PromptOptionsModel.ToGenerateVideoOptions(PromptOptions, SchedulerOptions, MemoryInfo);
            if (IsControlNetEnabled)
            {
                generateOptions.ControlNet = await LoadControlNetAsync();
                var controlNetDiffuserType = SchedulerOptions.Strength >= 1
                        ? DiffuserType.ControlNet
                        : DiffuserType.ControlNetImage;

                if (_inputControlImageCached == null)
                    _inputControlImageCached = await GetInputImage(InputImage, true, cancellationToken);

                if (controlNetDiffuserType == DiffuserType.ControlNetImage)
                {
                    if (_inputImageCached == null)
                        _inputImageCached = await GetInputImage(InputImage, false, cancellationToken);

                    generateOptions.InputImage = _inputImageCached;
                }

                generateOptions.InputContolImage = _inputControlImageCached;
                generateOptions.Diffuser = controlNetDiffuserType;
                return generateOptions;
            }

            _inputImageCached ??= await GetInputImage(InputImage, true, cancellationToken);
            generateOptions.Diffuser = DiffuserType.ImageToVideo;
            generateOptions.InputImage = _inputImageCached;
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


        private async Task<OnnxImage> GetInputImage(ImageInput inputImage, bool extractFeatures, CancellationToken cancellationToken = default)
        {
            if (inputImage == null)
                return default;

            var imageBytes = inputImage.Image.GetImageBytes();
            if (imageBytes.IsNullOrEmpty())
                return default;

            if (extractFeatures)
                return await ExecuteFeatureExtractorAsync(new OnnxImage(imageBytes), cancellationToken);

            return new OnnxImage(imageBytes);
        }


        protected async Task<OnnxImage> ExecuteFeatureExtractorAsync(OnnxImage inputImage, CancellationToken cancellationToken)
        {
            if (CurrentPipeline.FeatureExtractorModel is not null)
            {
                var featureExtractorPipeline = await LoadFeatureExtractorAsync();
                UpdateProgress("Extracting Image Feature...", true);
                var result = await Task.Run(() => featureExtractorPipeline.RunAsync(inputImage, new FeatureExtractorOptions(), cancellationToken));
                return result;
            }
            return inputImage;
        }

    }
}