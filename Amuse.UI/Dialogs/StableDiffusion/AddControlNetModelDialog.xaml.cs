using Amuse.UI.Helpers;
using Amuse.UI.Models;
using Amuse.UI.Models.StableDiffusion;
using Amuse.UI.Services;
using OnnxStack.StableDiffusion.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Amuse.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for AddControlNetModelDialog.xaml
    /// </summary>
    public partial class AddControlNetModelDialog : BaseDialog
    {
        private readonly List<string> _invalidOptions;
        private readonly IModelFactory _modelFactory;
        private readonly AmuseSettings _settings;

        private string _modelName;
        private string _modelFile;
        private ControlNetModelJson _modelSetResult;
        private ControlNetModelTemplate _controlNetTemplate;
        private bool _enableNameSelection = true;
        private bool _enableTemplateSelection = true;
        private ModelTemplateViewModel _modelTemplate;

        public AddControlNetModelDialog(AmuseSettings settings, IModelFactory modelFactory)
        {
            _settings = settings;
            _modelFactory = modelFactory;
            _invalidOptions = _settings.GetModelNames(ModelTemplateCategory.ControlNet);
            ValidationResults = new ObservableCollection<ValidationResult>();
            ModelTemplates = _settings.Templates.Where(x => x.Group == ModelTemplateGroup.Fixed && x.Category == ModelTemplateCategory.ControlNet).ToList();

            SelectedPipelineTypes = Enum.GetValues<PipelineType>().ToSelectList();
            InitializeComponent();
        }

        public List<ModelTemplateViewModel> ModelTemplates { get; }
        public ObservableCollection<ValidationResult> ValidationResults { get; }
        public List<SelectItem<PipelineType>> SelectedPipelineTypes { get; set; }
        public ControlNetModelJson ModelSetResult => _modelSetResult;

        public ModelTemplateViewModel ModelTemplate
        {
            get { return _modelTemplate; }
            set { _modelTemplate = value; NotifyPropertyChanged(); Validate(); }
        }

        public string ModelName
        {
            get { return _modelName; }
            set { _modelName = value; _modelName?.Trim(); NotifyPropertyChanged(); Validate(); }
        }

        public string ModelFile
        {
            get { return _modelFile; }
            set
            {
                _modelFile = value;
                if (EnableNameSelection)
                    _modelName = string.IsNullOrEmpty(_modelFile)
                        ? string.Empty
                        : Path.GetFileNameWithoutExtension(_modelFile);

                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(ModelName));
                Validate();
            }
        }


        public ControlNetModelTemplate ControlNetTemplate
        {
            get { return _controlNetTemplate; }
            set { _controlNetTemplate = value; NotifyPropertyChanged(); }
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
                ControlNetTemplate = selectedTemplate.ControlNetTemplate with { };
            }
            else
            {
                ControlNetTemplate = new ControlNetModelTemplate();
                EnableNameSelection = true;
                EnableTemplateSelection = false;
                ModelTemplate = ModelTemplates.First();
            }

            return base.ShowDialogAsync();
        }


        private void Validate()
        {
            _modelSetResult = null;
            ValidationResults.Clear();
            if (string.IsNullOrEmpty(_modelFile))
                return;

            _controlNetTemplate.PipelineTypes = SelectedPipelineTypes.ToSelectedList();
            _modelSetResult = _modelFactory.CreateControlNetModelSet(_modelName, _modelFile, _controlNetTemplate);

            // Validate
            if (_enableNameSelection)
                ValidationResults.Add(new ValidationResult("Name", !_invalidOptions.Contains(_modelName, StringComparer.OrdinalIgnoreCase) && _modelName.Length > 2 && _modelName.Length < 50));

            ValidationResults.Add(new ValidationResult("Model", File.Exists(_modelSetResult.OnnxModelPath)));
        }


        protected override Task SaveAsync()
        {
            Validate();
            return base.SaveAsync();
        }


        protected override bool CanExecuteSave()
        {
            if (string.IsNullOrEmpty(_modelFile))
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
