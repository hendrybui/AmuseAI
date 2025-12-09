using Amuse.UI.Models;
using System.Threading.Tasks;

namespace Amuse.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for ViewModelMetadataDialog.xaml
    /// </summary>
    public partial class ViewModelMetadataDialog : BaseDialog
    {
        private ModelTemplateViewModel _modelTemplate;

        public ViewModelMetadataDialog()
        {
            InitializeComponent();
        }


        public ModelTemplateViewModel ModelTemplate
        {
            get { return _modelTemplate; }
            set { _modelTemplate = value; NotifyPropertyChanged(); }
        }


        public Task<bool> ShowDialogAsync(ModelTemplateViewModel modelTemplate)
        {
            ModelTemplate = modelTemplate;
            return base.ShowDialogAsync();
        }

    }
}
