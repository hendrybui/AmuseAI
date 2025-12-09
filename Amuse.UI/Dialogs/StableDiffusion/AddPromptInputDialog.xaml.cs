using Amuse.UI.Models;
using System.Threading.Tasks;

namespace Amuse.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for AddPromptInputDialog.xaml
    /// </summary>
    public partial class AddPromptInputDialog : BaseDialog
    {
        private readonly AmuseSettings _settings;

        private string _promptText;
        private PromptInputType _promptInputType;
        private PromptInputModel _updatePromptInput;

        public AddPromptInputDialog(AmuseSettings settings)
        {
            _settings = settings;
            InitializeComponent();
        }

        public string PromptText
        {
            get { return _promptText; }
            set { _promptText = value; NotifyPropertyChanged(); }
        }

        public PromptInputType PromptInputType
        {
            get { return _promptInputType; }
            set { _promptInputType = value; NotifyPropertyChanged(); }
        }


        public Task<bool> ShowDialogAsync(string promptText, PromptInputType promptInputType)
        {
            Title = "Add Saved Prompt";
            PromptText = promptText;
            PromptInputType = promptInputType;
            return base.ShowDialogAsync();
        }


        public Task<bool> ShowDialogAsync(PromptInputModel promptInput = null)
        {
            _updatePromptInput = promptInput;
            Title = _updatePromptInput is null ? "Add Saved Prompt" : "Update Saved Prompt";
            PromptText = _updatePromptInput?.Prompt;
            PromptInputType = _updatePromptInput?.Type ?? PromptInputType.Positive;
            return base.ShowDialogAsync();
        }


        protected override Task SaveAsync()
        {
            var index = 0;
            if (_updatePromptInput is not null)
            {
                index = _settings.Prompts.IndexOf(_updatePromptInput);
                _settings.Prompts.Remove(_updatePromptInput);
            }
            _settings.Prompts.Insert(index, new PromptInputModel(PromptText, PromptInputType));
            return base.SaveAsync();
        }


        protected override bool CanExecuteSave()
        {
            return !string.IsNullOrWhiteSpace(_promptText);
        }

    }
}
