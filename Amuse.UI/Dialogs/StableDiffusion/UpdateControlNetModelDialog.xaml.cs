using Amuse.UI.Helpers;
using Amuse.UI.Models;
using Amuse.UI.Models.StableDiffusion;
using OnnxStack.StableDiffusion.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Amuse.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for UpdateControlNetModelDialog.xaml
    /// </summary>
    public partial class UpdateControlNetModelDialog : BaseDialog
    {
        private readonly List<string> _invalidOptions;
        private readonly AmuseSettings _settings;

        private ControlNetModelJson _modelSetResult;
        private UpdateControlNetModelSetViewModel _updateModelSet;
        private List<SelectItem<PipelineType>> _selectedPipelineTypes;
        private string _validationError;

        public UpdateControlNetModelDialog(AmuseSettings settings)
        {
            _settings = settings;
            _invalidOptions = _settings.GetModelNames(ModelTemplateCategory.ControlNet);
            InitializeComponent();
        }

        public AmuseSettings Settings => _settings;
        public ControlNetModelJson ModelSetResult => _modelSetResult;

        public UpdateControlNetModelSetViewModel UpdateModelSet
        {
            get { return _updateModelSet; }
            set { _updateModelSet = value; NotifyPropertyChanged(); }
        }

        public string ValidationError
        {
            get { return _validationError; }
            set { _validationError = value; NotifyPropertyChanged(); }
        }

        public List<SelectItem<PipelineType>> SelectedPipelineTypes
        {
            get { return _selectedPipelineTypes; }
            set { _selectedPipelineTypes = value; NotifyPropertyChanged(); }
        }


        public Task<bool> ShowDialogAsync(ControlNetModelJson modelSet, List<PipelineType> pipelineTypes)
        {   
            _invalidOptions.Remove(modelSet.Name);
            UpdateModelSet = UpdateControlNetModelSetViewModel.FromModelSet(modelSet);
            SelectedPipelineTypes = Enum.GetValues<PipelineType>().ToSelectList(pipelineTypes);
            return base.ShowDialogAsync();
        }


        private bool Validate()
        {
            if (_updateModelSet == null)
                return false;

            _updateModelSet.ModelTemplate.PipelineTypes = SelectedPipelineTypes.ToSelectedList();
            _modelSetResult = UpdateControlNetModelSetViewModel.ToModelSet(_updateModelSet);
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
            return  base.CancelAsync();
        }

    }
}
