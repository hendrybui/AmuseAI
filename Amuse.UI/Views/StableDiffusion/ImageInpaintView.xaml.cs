using Amuse.UI.Models;
using Amuse.UI.Models.StableDiffusion;
using OnnxStack.Core.Image;
using OnnxStack.StableDiffusion.Config;
using OnnxStack.StableDiffusion.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace Amuse.UI.Views
{
    /// <summary>
    /// Interaction logic for ImageInpaintView.xaml
    /// </summary>
    public partial class ImageInpaintView : StableDiffusionImageViewBase
    {
        private ImageInput _inputImageMask;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageInpaintView"/> class.
        /// </summary>
        public ImageInpaintView() : base()
        {
            SupportedDiffusers = [DiffuserType.ImageInpaint, DiffuserType.ImageInpaintLegacy];
            SchedulerOptions = new SchedulerOptionsModel { SchedulerType = SchedulerType.DDPM };
            InitializeComponent();
        }

        public override bool IsUpscalerSupported => true;

        public ImageInput InputImageMask
        {
            get { return _inputImageMask; }
            set { _inputImageMask = value; NotifyPropertyChanged(); }
        }


        public override async Task NavigateAsync(IImageResult imageResult)
        {
            InputImageMask = null;
            await base.NavigateAsync(imageResult);
        }

        protected override Task LoadPipelineAsync()
        {
            ResetControlImageCache();
            return base.LoadPipelineAsync();
        }

        protected override async Task<GenerateOptions> GetGenerateOptionsAsync(CancellationToken cancellationToken)
        {
            var generateOptions = await base.GetGenerateOptionsAsync(cancellationToken);
            generateOptions.SchedulerOptions.Strength = 1;  // Make sure strength is 1 for Image Inpainting
            generateOptions.Diffuser = SelectedBaseModel.ModelSet.Diffusers.Contains(DiffuserType.ImageInpaint)
                  ? DiffuserType.ImageInpaint
                  : DiffuserType.ImageInpaintLegacy;
            generateOptions.InputImage = new OnnxImage(InputImage.Image.GetImageBytes());
            generateOptions.InputImageMask = new OnnxImage(InputImageMask.Image.GetImageBytes());

            return generateOptions;
        }


        protected override bool CanExecuteGenerate()
        {
            return base.CanExecuteGenerate() && InputImage is not null;
        }
    }
}