using Amuse.UI.Models;
using Amuse.UI.Models.Upscale;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Amuse.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for UpdateUpscaleModelDialog.xaml
    /// </summary>
    public partial class UpdateUpscaleModelDialog : BaseDialog
    {
        private readonly List<string> _invalidOptions;
        private readonly AmuseSettings _settings;

        private UpscaleModelJson _modelSetResult;
        private UpdateUpscaleModelSetViewModel _updateModelSet;
        private string _validationError;

        public UpdateUpscaleModelDialog(AmuseSettings settings)
        {
            _settings = settings;
            _invalidOptions = _settings.GetModelNames(ModelTemplateCategory.Upscaler);
            InitializeComponent();
        }

        public AmuseSettings Settings => _settings;
        public UpscaleModelJson ModelSetResult => _modelSetResult;

        public UpdateUpscaleModelSetViewModel UpdateModelSet
        {
            get { return _updateModelSet; }
            set { _updateModelSet = value; NotifyPropertyChanged(); }
        }

        public string ValidationError
        {
            get { return _validationError; }
            set { _validationError = value; NotifyPropertyChanged(); }
        }


        public Task<bool> ShowDialogAsync(UpscaleModelJson modelSet)
        {
            _invalidOptions.Remove(modelSet.Name);
            UpdateModelSet = UpdateUpscaleModelSetViewModel.FromModelSet(modelSet);
            return base.ShowDialogAsync();
        }


        private bool Validate()
        {
            if (_updateModelSet == null)
                return false;

            _modelSetResult = UpdateUpscaleModelSetViewModel.ToModelSet(_updateModelSet);
            if (_modelSetResult == null)
                return false;

            if (_invalidOptions.Contains(_modelSetResult.Name, StringComparer.OrdinalIgnoreCase))
            {
                ValidationError = $"Model with name '{_modelSetResult.Name}' already exists";
                return false;
            }

            if (!File.Exists(_modelSetResult.OnnxModelPath))
            {
                ValidationError = $"Upscale model file not found";
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
