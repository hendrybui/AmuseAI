using Amuse.UI.Commands;
using Amuse.UI.Models;
using Amuse.UI.Models.FeatureExtractor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Amuse.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for UpdateFeatureExtractorModelDialog.xaml
    /// </summary>
    public partial class UpdateFeatureExtractorModelDialog : BaseDialog
    {
        private readonly List<string> _invalidOptions;
        private readonly AmuseSettings _settings;

        private FeatureExtractorModelJson _modelSetResult;
        private UpdateFeatureExtractorModelSetViewModel _updateModelSet;
        private string _validationError;
        private bool _isControlNetSupported;

        public UpdateFeatureExtractorModelDialog(AmuseSettings settings)
        {
            _settings = settings;
            _invalidOptions = _settings.GetModelNames(ModelTemplateCategory.FeatureExtractor);
            SaveCommand = new AsyncRelayCommand(SaveAsync, CanExecuteSave);
            CancelCommand = new AsyncRelayCommand(CancelAsync);
            InitializeComponent();
        }

        public AmuseSettings Settings => _settings;


        public UpdateFeatureExtractorModelSetViewModel UpdateModelSet
        {
            get { return _updateModelSet; }
            set { _updateModelSet = value; NotifyPropertyChanged(); }
        }

        public string ValidationError
        {
            get { return _validationError; }
            set { _validationError = value; NotifyPropertyChanged(); }
        }

        public FeatureExtractorModelJson ModelSetResult
        {
            get { return _modelSetResult; }
        }

        public bool IsControlNetSupported
        {
            get { return _isControlNetSupported; }
            set { _isControlNetSupported = value; NotifyPropertyChanged(); }
        }

        public Task<bool> ShowDialogAsync(FeatureExtractorModelJson modelSet, bool isControlNetSupported)
        {
            _invalidOptions.Remove(modelSet.Name);
            UpdateModelSet = UpdateFeatureExtractorModelSetViewModel.FromModelSet(modelSet);
            IsControlNetSupported = isControlNetSupported;
            return base.ShowDialogAsync();
        }

        private bool Validate()
        {
            if (_updateModelSet == null)
                return false;

            _modelSetResult = UpdateFeatureExtractorModelSetViewModel.ToModelSet(_updateModelSet);
            if (_modelSetResult == null)
                return false;

            if (_invalidOptions.Contains(_modelSetResult.Name, StringComparer.OrdinalIgnoreCase))
            {
                ValidationError = $"Model with name '{_modelSetResult.Name}' already exists";
                return false;
            }

            if (!File.Exists(_modelSetResult.OnnxModelPath))
            {
                ValidationError = $"ContolNet model file not found";
                return false;
            }

            ValidationError = null;
            return true;
        }


        protected override Task SaveAsync()
        {
            if (!Validate())
                return Task.CompletedTask;

            return base.SaveAsync();
        }


        protected override bool CanExecuteSave()
        {
            return Validate();
        }


        protected override Task CancelAsync()
        {
            _modelSetResult = null;
            return base.CancelAsync();
        }

    }
}
