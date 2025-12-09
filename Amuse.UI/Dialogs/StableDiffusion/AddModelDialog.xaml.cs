using Amuse.UI.Models;
using Amuse.UI.Models.StableDiffusion;
using Amuse.UI.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Amuse.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for AddModelDialog.xaml
    /// </summary>
    public partial class AddModelDialog : BaseDialog
    {
        private readonly List<string> _invalidOptions;
        private readonly IModelFactory _modelFactory;
        private readonly AmuseSettings _settings;

        private string _modelFolder;
        private string _modelName;
        private ModelTemplateViewModel _modelTemplate;
        private StableDiffusionModelSetJson _modelSetResult;
        private bool _enableTemplateSelection = true;
        private bool _enableNameSelection = true;

        public AddModelDialog(AmuseSettings settings, IModelFactory modelFactory)
        {
            _settings = settings;
            _modelFactory = modelFactory;
            _invalidOptions = _settings.GetModelNames(ModelTemplateCategory.StableDiffusion);
            ValidationResults = new ObservableCollection<ValidationResult>();
            ModelTemplates = _settings.Templates.Where(x => x.Group == ModelTemplateGroup.Fixed && x.Category == ModelTemplateCategory.StableDiffusion).ToList();
            InitializeComponent();
        }

        public AmuseSettings Settings => _settings;
        public List<ModelTemplateViewModel> ModelTemplates { get; }
        public ObservableCollection<ValidationResult> ValidationResults { get; }
        public StableDiffusionModelSetJson ModelSetResult => _modelSetResult;

        public ModelTemplateViewModel ModelTemplate
        {
            get { return _modelTemplate; }
            set { _modelTemplate = value; NotifyPropertyChanged(); CreateModelSet(); }
        }

        public string ModelName
        {
            get { return _modelName; }
            set { _modelName = value; _modelName?.Trim(); NotifyPropertyChanged(); CreateModelSet(); }
        }

        public string ModelFolder
        {
            get { return _modelFolder; }
            set
            {
                _modelFolder = value;
                if (_modelTemplate is not null && _modelTemplate.Group == ModelTemplateGroup.Fixed)
                    _modelName = string.IsNullOrEmpty(_modelFolder)
                        ? string.Empty
                        : Path.GetFileName(_modelFolder);

                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(ModelName));
                CreateModelSet();
            }
        }

        public bool EnableTemplateSelection
        {
            get { return _enableTemplateSelection; }
            set { _enableTemplateSelection = value; NotifyPropertyChanged(); }
        }

        public bool EnableNameSelection
        {
            get { return _enableNameSelection; }
            set { _enableNameSelection = value; NotifyPropertyChanged(); }
        }


        public Task<bool> ShowDialogAsync(ModelTemplateViewModel selectedTemplate = null)
        {
            if (selectedTemplate is not null)
            {
                EnableNameSelection = selectedTemplate.Group == ModelTemplateGroup.Fixed;
                EnableTemplateSelection = false;
                ModelTemplate = selectedTemplate;
                ModelName = selectedTemplate.Group == ModelTemplateGroup.Fixed ? string.Empty : selectedTemplate.Name;
            }

            if (EnableTemplateSelection)
                ModelTemplate = ModelTemplates.First();

            return base.ShowDialogAsync();
        }


        private void CreateModelSet()
        {
            _modelSetResult = null;
            ValidationResults.Clear();
            if (string.IsNullOrEmpty(_modelFolder))
                return;

            _modelSetResult = _modelFactory.CreateStableDiffusionModelSet(ModelName.Trim(), ModelFolder, _modelTemplate.StableDiffusionTemplate);

            // Validate
            if (_enableNameSelection)
                ValidationResults.Add(new ValidationResult("Name", !_invalidOptions.Contains(_modelName.ToLower()) && _modelName.Length > 2 && _modelName.Length < 50));

            ValidationResults.Add(new ValidationResult("Unet Model", File.Exists(_modelSetResult.UnetConfig.OnnxModelPath)));
            if (_modelSetResult.Unet2Config is not null)
                ValidationResults.Add(new ValidationResult("Unet2 Model", File.Exists(_modelSetResult.Unet2Config.OnnxModelPath)));
            ValidationResults.Add(new ValidationResult("Tokenizer Model", File.Exists(_modelSetResult.TokenizerConfig.OnnxModelPath)));
            if (_modelSetResult.Tokenizer2Config is not null)
                ValidationResults.Add(new ValidationResult("Tokenizer2 Model", File.Exists(_modelSetResult.Tokenizer2Config.OnnxModelPath)));
            if (_modelSetResult.Tokenizer3Config is not null)
                ValidationResults.Add(new ValidationResult("Tokenizer3 Model", File.Exists(_modelSetResult.Tokenizer3Config.OnnxModelPath)));
            ValidationResults.Add(new ValidationResult("TextEncoder Model", File.Exists(_modelSetResult.TextEncoderConfig.OnnxModelPath)));
            if (_modelSetResult.TextEncoder2Config is not null)
                ValidationResults.Add(new ValidationResult("TextEncoder2 Model", File.Exists(_modelSetResult.TextEncoder2Config.OnnxModelPath)));
            if (_modelSetResult.TextEncoder3Config is not null)
                ValidationResults.Add(new ValidationResult("TextEncoder3 Model", File.Exists(_modelSetResult.TextEncoder3Config.OnnxModelPath)));
            ValidationResults.Add(new ValidationResult("VaeDecoder Model", File.Exists(_modelSetResult.VaeDecoderConfig.OnnxModelPath)));
            ValidationResults.Add(new ValidationResult("VaeEncoder Model", File.Exists(_modelSetResult.VaeEncoderConfig.OnnxModelPath)));
            if (_modelSetResult.ControlNetUnetConfig is not null)
                ValidationResults.Add(new ValidationResult("ControlNet Model", File.Exists(_modelSetResult.ControlNetUnetConfig.OnnxModelPath)));
            if (_modelSetResult.FlowEstimationConfig is not null)
                ValidationResults.Add(new ValidationResult("FlowEstimation Model", File.Exists(_modelSetResult.FlowEstimationConfig.OnnxModelPath)));
            if (_modelSetResult.ResampleModelConfig is not null)
                ValidationResults.Add(new ValidationResult("Resample Model", File.Exists(_modelSetResult.ResampleModelConfig.OnnxModelPath)));
        }


        protected override bool CanExecuteSave()
        {
            if (string.IsNullOrEmpty(_modelFolder))
                return false;
            if (_modelSetResult is null)
                return false;

            return ValidationResults.Count > 0 && ValidationResults.All(x => x.IsValid);
        }


        protected override Task CancelAsync()
        {
            _modelSetResult = null;
            return base.CancelAsync();
        }

    }
}
