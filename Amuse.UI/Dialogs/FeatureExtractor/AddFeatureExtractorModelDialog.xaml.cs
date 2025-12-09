using Amuse.UI.Models;
using Amuse.UI.Models.FeatureExtractor;
using Amuse.UI.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Amuse.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for AddFeatureExtractorModelDialog.xaml
    /// </summary>
    public partial class AddFeatureExtractorModelDialog : BaseDialog
    {
        private readonly IModelFactory _modelFactory;
        private readonly AmuseSettings _settings;
        private readonly List<string> _invalidOptions;

        private string _modelName;
        private string _modelFile;
        private FeatureExtractorModelJson _modelSetResult;
        private FeatureExtractorModelTemplate _featureExtractorTemplate;
        private bool _enableTemplateSelection;
        private bool _enableNameSelection;
        private ModelTemplateViewModel _modelTemplate;


        public AddFeatureExtractorModelDialog(AmuseSettings settings, IModelFactory modelFactory)
        {
            _settings = settings;
            _modelFactory = modelFactory;
            _invalidOptions = _settings.GetModelNames(ModelTemplateCategory.FeatureExtractor);
            ValidationResults = new ObservableCollection<ValidationResult>();
            ModelTemplates = _settings.Templates.Where(x => x.Group == ModelTemplateGroup.Fixed && x.Category == ModelTemplateCategory.FeatureExtractor).ToList();
            InitializeComponent();
        }

        public AmuseSettings Settings => _settings;
        public ObservableCollection<ValidationResult> ValidationResults { get; }
        public List<ModelTemplateViewModel> ModelTemplates { get; }

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
                if (_enableNameSelection)
                    _modelName = string.IsNullOrEmpty(_modelFile)
                        ? string.Empty
                        : Path.GetFileNameWithoutExtension(_modelFile);

                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(ModelName));
                Validate();
            }
        }

        public FeatureExtractorModelTemplate FeatureExtractorTemplate
        {
            get { return _featureExtractorTemplate; }
            set { _featureExtractorTemplate = value; NotifyPropertyChanged(); }
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

        public FeatureExtractorModelJson ModelSetResult
        {
            get { return _modelSetResult; }
        }


        public Task<bool> ShowDialogAsync(ModelTemplateViewModel selectedTemplate = null)
        {
            if (selectedTemplate is not null)
            {
                EnableNameSelection = selectedTemplate.Group == ModelTemplateGroup.Fixed;
                EnableTemplateSelection = false;
                ModelTemplate = selectedTemplate;
                ModelName = selectedTemplate.Group == ModelTemplateGroup.Fixed ? string.Empty : selectedTemplate.Name;
                FeatureExtractorTemplate = selectedTemplate.FeatureExtractorTemplate with { };
            }
            else
            {
                FeatureExtractorTemplate = new FeatureExtractorModelTemplate();
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

            _modelSetResult = _modelFactory.CreateFeatureExtractorModelSet(ModelName.Trim(), _modelFile, _featureExtractorTemplate);

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
