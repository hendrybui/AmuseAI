using Amuse.UI.Models;
using System.Threading.Tasks;

namespace Amuse.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for ModelLicenceDialog.xaml
    /// </summary>
    public partial class ModelLicenceDialog : BaseDialog
    {
        private readonly AmuseSettings _settings;
        private ModelTemplateViewModel _modelTemplate;

        public ModelLicenceDialog(AmuseSettings settings)
        {
            _settings = settings;
            InitializeComponent();
        }

        public ModelTemplateViewModel ModelTemplate
        {
            get { return _modelTemplate; }
            set { _modelTemplate = value; NotifyPropertyChanged(); }
        }


        public async Task<bool> ShowDialogAsync(ModelTemplateViewModel modelTemplate)
        {
            ModelTemplate = modelTemplate;
            return await base.ShowDialogAsync();
        }


        protected override async Task SaveAsync()
        {
            await _settings.SaveAsync();
            await base.SaveAsync();
        }


        protected override bool CanExecuteSave()
        {
            return ModelTemplate?.IsLicenceAccepted ?? false;
        }


        protected override Task CancelAsync()
        {
            ModelTemplate.IsLicenceAccepted = false;
            return base.CancelAsync();
        }


        protected override Task WindowClose()
        {
            ModelTemplate.IsLicenceAccepted = false;
            return base.WindowClose();
        }
    }
}
