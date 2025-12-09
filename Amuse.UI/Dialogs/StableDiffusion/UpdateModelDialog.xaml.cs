using Amuse.UI.Models;
using Amuse.UI.Models.StableDiffusion;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Amuse.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for UpdateModelDialog.xaml
    /// </summary>
    public partial class UpdateModelDialog :BaseDialog
    {
        private readonly List<string> _invalidOptions;
        private readonly AmuseSettings _settings;

        private UpdateModelSetViewModel _updateModelSet;
        private StableDiffusionModelSetJson _modelSetResult;
        private string _validationError;

        public UpdateModelDialog(AmuseSettings settings)
        {
            _settings = settings;
            _invalidOptions = _settings.GetModelNames(ModelTemplateCategory.StableDiffusion);
            InitializeComponent();
        }

        public AmuseSettings Settings => _settings;


        public UpdateModelSetViewModel UpdateModelSet
        {
            get { return _updateModelSet; }
            set { _updateModelSet = value; NotifyPropertyChanged(); }
        }

        public string ValidationError
        {
            get { return _validationError; }
            set { _validationError = value; NotifyPropertyChanged(); }
        }

        public StableDiffusionModelSetJson ModelSetResult
        {
            get { return _modelSetResult; }
        }


        public Task<bool> ShowDialogAsync(StableDiffusionModelSetJson modelSet)
        {
            _invalidOptions.Remove(modelSet.Name);
            UpdateModelSet = UpdateModelSetViewModel.FromModelSet(_settings, modelSet);
            return base.ShowDialogAsync();
        }


        protected override Task SaveAsync()
        {
            ValidationError = string.Empty;
            _modelSetResult = UpdateModelSetViewModel.ToModelSet(_updateModelSet);
            if (_invalidOptions.Contains(_modelSetResult.Name))
            {
                ValidationError = $"Model with name '{_modelSetResult.Name}' already exists";
                return Task.CompletedTask;
            }

            if (!File.Exists(_modelSetResult.UnetConfig.OnnxModelPath))
                ValidationError = $"Unet model file not found";

            if (!File.Exists(_modelSetResult.TokenizerConfig.OnnxModelPath))
                ValidationError = $"Tokenizer model file not found";

            if (_modelSetResult.Tokenizer2Config is not null)
                if (!File.Exists(_modelSetResult.Tokenizer2Config.OnnxModelPath))
                    ValidationError = $"Tokenizer2 model file not found";

            if (_modelSetResult.Tokenizer3Config is not null)
                if (!File.Exists(_modelSetResult.Tokenizer3Config.OnnxModelPath))
                    ValidationError = $"Tokenizer3 model file not found";

            if (!File.Exists(_modelSetResult.TextEncoderConfig.OnnxModelPath))
                ValidationError = $"TextEncoder model file not found";

            if (_modelSetResult.TextEncoder2Config is not null)
                if (!File.Exists(_modelSetResult.TextEncoder2Config.OnnxModelPath))
                    ValidationError = $"TextEncoder2 model file not found";

            if (_modelSetResult.TextEncoder3Config is not null)
                if (!File.Exists(_modelSetResult.TextEncoder3Config.OnnxModelPath))
                    ValidationError = $"TextEncoder3 model file not found";


            if (!File.Exists(_modelSetResult.VaeDecoderConfig.OnnxModelPath))
                ValidationError = $"VaeDecoder model file not found";

            if (!File.Exists(_modelSetResult.VaeEncoderConfig.OnnxModelPath))
                ValidationError = $"VaeEncoder model file not found";

            if (_modelSetResult.ControlNetUnetConfig is not null)
                if (!File.Exists(_modelSetResult.ControlNetUnetConfig.OnnxModelPath))
                    ValidationError = $"ControlNet model file not found";

            if (_modelSetResult.FlowEstimationConfig is not null)
                if (!File.Exists(_modelSetResult.FlowEstimationConfig.OnnxModelPath))
                    ValidationError = $"FlowEstimation model file not found";

            if (_modelSetResult.ResampleModelConfig is not null)
                if (!File.Exists(_modelSetResult.ResampleModelConfig.OnnxModelPath))
                    ValidationError = $"Resample model file not found";

            if (!string.IsNullOrEmpty(ValidationError))
                return Task.CompletedTask;

            return base.SaveAsync();
        }


        protected override Task CancelAsync()
        {
            _modelSetResult = null;
            UpdateModelSet = null;
            return base.CancelAsync();
        }

    }
}
