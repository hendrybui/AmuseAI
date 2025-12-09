using Amuse.UI.Commands;
using Amuse.UI.Dialogs;
using Amuse.UI.Helpers;
using Amuse.UI.Models;
using Amuse.UI.Models.StableDiffusion;
using Amuse.UI.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Amuse.UI.UserControls
{
    /// <summary>
    /// Interaction logic for PromptInputControl.xaml
    /// </summary>
    public partial class PromptInputControl : UserControl, INotifyPropertyChanged
    {
        private readonly IDialogService _dialogService;
        private readonly IModeratorService _moderatorService;

        private int _promptTokenCount;
        private bool _promptLimitExceded;
        private int _negativePromptTokenCount;
        private bool _negativePromptLimitExceded;
        private ICollectionView _promptCollectionView;
        private ICollectionView _promptNegativeCollectionView;
        private ICollectionView _promptSnippitCollectionView;
        private Visibility _labelVisibility = Visibility.Visible;

        /// <summary>Initializes a new instance of the <see cref="PromptInputControl" /> class.</summary>
        public PromptInputControl()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _dialogService = App.GetService<IDialogService>();
                _moderatorService = App.GetService<IModeratorService>();
            }

            SavePromptCommand = new AsyncRelayCommand<PromptInputType>(SavePrompt);
            AppendPromptCommand = new AsyncRelayCommand<PromptInputModel>(AppendPrompt);
            AppendPromptNegativeCommand = new AsyncRelayCommand<PromptInputModel>(AppendPromptNegative);
            InitializeComponent();
        }

        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register(nameof(Settings), typeof(AmuseSettings), typeof(PromptInputControl), new PropertyMetadata<PromptInputControl>(x => x.OnSettingsChanged()));

        public static readonly DependencyProperty PromptOptionsProperty =
            DependencyProperty.Register(nameof(PromptOptions), typeof(PromptOptionsModel), typeof(PromptInputControl));


        public static readonly DependencyProperty IsNegativePromptSupportedProperty =
            DependencyProperty.Register(nameof(IsNegativePromptSupported), typeof(bool), typeof(PromptInputControl));


        public static readonly DependencyProperty IsGeneratingProperty =
            DependencyProperty.Register(nameof(IsGenerating), typeof(bool), typeof(PromptInputControl));

        public static readonly DependencyProperty PromptTokenLimitProperty =
            DependencyProperty.Register(nameof(PromptTokenLimit), typeof(int), typeof(PromptInputControl));

        public AsyncRelayCommand<PromptInputType> SavePromptCommand { get; }
        public AsyncRelayCommand<PromptInputModel> AppendPromptCommand { get; }
        public AsyncRelayCommand<PromptInputModel> AppendPromptNegativeCommand { get; }

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        public AmuseSettings Settings
        {
            get { return (AmuseSettings)GetValue(SettingsProperty); }
            set { SetValue(SettingsProperty, value); }
        }

        /// <summary>
        /// Gets or sets the PromptOptions.
        /// </summary>
        public PromptOptionsModel PromptOptions
        {
            get { return (PromptOptionsModel)GetValue(PromptOptionsProperty); }
            set { SetValue(PromptOptionsProperty, value); }
        }

        public bool IsNegativePromptSupported
        {
            get { return (bool)GetValue(IsNegativePromptSupportedProperty); }
            set { SetValue(IsNegativePromptSupportedProperty, value); }
        }

        public Visibility LabelVisibility
        {
            get { return _labelVisibility; }
            set { _labelVisibility = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is generating.
        /// </summary>
        public bool IsGenerating
        {
            get { return (bool)GetValue(IsGeneratingProperty); }
            set { SetValue(IsGeneratingProperty, value); }
        }

        public int PromptTokenLimit
        {
            get { return (int)GetValue(PromptTokenLimitProperty); }
            set { SetValue(PromptTokenLimitProperty, value); }
        }

        public int PromptTokenCount
        {
            get { return _promptTokenCount; }
            set { _promptTokenCount = value; NotifyPropertyChanged(); }
        }

        public bool PromptLimitExceded
        {
            get { return _promptLimitExceded; }
            set { _promptLimitExceded = value; NotifyPropertyChanged(); }
        }

        public int NegativePromptTokenCount
        {
            get { return _negativePromptTokenCount; }
            set { _negativePromptTokenCount = value; NotifyPropertyChanged(); }
        }

        public bool NegativePromptLimitExceded
        {
            get { return _negativePromptLimitExceded; }
            set { _negativePromptLimitExceded = value; NotifyPropertyChanged(); }
        }

        public ICollectionView PromptCollectionView
        {
            get { return _promptCollectionView; }
            set { _promptCollectionView = value; NotifyPropertyChanged(); }
        }

        public ICollectionView PromptNegativeCollectionView
        {
            get { return _promptNegativeCollectionView; }
            set { _promptNegativeCollectionView = value; NotifyPropertyChanged(); }
        }

        public ICollectionView PromptSnippitCollectionView
        {
            get { return _promptSnippitCollectionView; }
            set { _promptSnippitCollectionView = value; NotifyPropertyChanged(); }
        }


        /// <summary>
        /// Called when Settings changed.
        /// </summary>
        /// <returns></returns>
        private Task OnSettingsChanged()
        {
            PromptCollectionView = new ListCollectionView(Settings.Prompts);
            PromptCollectionView.Filter = (obj) =>
            {
                if (obj is not PromptInputModel viewModel)
                    return false;
                return viewModel.Type == PromptInputType.Positive;
            };
            PromptNegativeCollectionView = new ListCollectionView(Settings.Prompts);
            PromptNegativeCollectionView.Filter = (obj) =>
            {
                if (obj is not PromptInputModel viewModel)
                    return false;
                return viewModel.Type == PromptInputType.Negative;
            };
            PromptSnippitCollectionView = new ListCollectionView(Settings.Prompts);
            PromptSnippitCollectionView.Filter = (obj) =>
            {
                if (obj is not PromptInputModel viewModel)
                    return false;
                return viewModel.Type == PromptInputType.Snippit;
            };
            return Task.CompletedTask;
        }


        private Task AppendPrompt(PromptInputModel promptInput)
        {
            if (promptInput.Type == PromptInputType.Positive)
                PromptOptions.Prompt = promptInput.Prompt;
            else if (promptInput.Type == PromptInputType.Snippit)
                PromptOptions.Prompt += promptInput.Prompt;

            return Task.CompletedTask;
        }


        private Task AppendPromptNegative(PromptInputModel promptInput)
        {
            if (promptInput.Type == PromptInputType.Negative)
                PromptOptions.NegativePrompt = promptInput.Prompt;
            else if (promptInput.Type == PromptInputType.Snippit)
                PromptOptions.NegativePrompt += promptInput.Prompt;

            return Task.CompletedTask;
        }


        private async Task SavePrompt(PromptInputType promptInputType)
        {
            var promptText = promptInputType == PromptInputType.Positive
                ? PromptOptions.Prompt
                : PromptOptions.NegativePrompt;
            var promptDialog = _dialogService.GetDialog<AddPromptInputDialog>();
            if (await promptDialog.ShowDialogAsync(promptText, promptInputType))
                await Settings.SaveAsync();
        }


        private async void Prompt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(PromptOptions.Prompt))
            {
                PromptTokenCount = 0;
                PromptLimitExceded = false;
                return;
            }

            var tokens = await _moderatorService.ClipTokenizer.EncodeAsync(PromptOptions.Prompt);
            PromptTokenCount = tokens.InputIds.Length;
            PromptLimitExceded = PromptTokenLimit > 0 && PromptTokenCount > PromptTokenLimit;
        }


        private async void NegativePrompt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(PromptOptions.NegativePrompt))
            {
                NegativePromptTokenCount = 0;
                NegativePromptLimitExceded = false;
                return;
            }

            var tokens = await _moderatorService.ClipTokenizer.EncodeAsync(PromptOptions.NegativePrompt);
            NegativePromptTokenCount = tokens.InputIds.Length;
            NegativePromptLimitExceded = PromptTokenLimit > 0 && NegativePromptTokenCount > PromptTokenLimit;
        }


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        #endregion
    }
}
