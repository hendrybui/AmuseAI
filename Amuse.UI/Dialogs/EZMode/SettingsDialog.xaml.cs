using Amuse.UI.Commands;
using Amuse.UI.Models;
using System;
using System.Threading.Tasks;

namespace Amuse.UI.Dialogs.EZMode
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : BaseDialog
    {
        private readonly AmuseSettings _settings;

        private bool _isNegativePromptEnabled;
        private bool _isPromptEnhanceEnabled;

        public SettingsDialog(AmuseSettings settings)
        {
            _settings = settings;
            OpenDirectoryCommand = new AsyncRelayCommand<string>(OpenDirectory);
            InitializeComponent();
        }

        public AmuseSettings Settings => _settings;
        public AsyncRelayCommand<string> OpenDirectoryCommand { get; }

        public bool IsNegativePromptEnabled
        {
            get { return _isNegativePromptEnabled; }
            set { _isNegativePromptEnabled = value; NotifyPropertyChanged(); }
        }

        public bool IsPromptEnhanceEnabled
        {
            get { return _isPromptEnhanceEnabled; }
            set { _isPromptEnhanceEnabled = value; NotifyPropertyChanged(); }
        }


        public Task<bool> ShowDialogAsync(bool isNegativePromptEnabled, bool _isPromptEnhanceEnabled)
        {
            IsPromptEnhanceEnabled = _isPromptEnhanceEnabled;
            IsNegativePromptEnabled = isNegativePromptEnabled;
            return ShowDialogAsync();
        }


        private Task OpenDirectory(string directory)
        {
            try
            {
                Utils.NavigateToUrl(directory);
            }
            catch (Exception)
            {
            }
            return Task.CompletedTask;
        }
    }
}
