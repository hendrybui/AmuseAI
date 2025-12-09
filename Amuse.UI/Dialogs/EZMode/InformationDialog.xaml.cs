using Amuse.UI.Models;

namespace Amuse.UI.Dialogs.EZMode
{
    /// <summary>
    /// Interaction logic for InformationDialog.xaml
    /// </summary>
    public partial class InformationDialog : BaseDialog
    {
        private readonly AmuseSettings _settings;

        public InformationDialog(AmuseSettings settings)
        {
            _settings = settings;
            InitializeComponent();
        }

        public string Version => App.Version;
    }
}
