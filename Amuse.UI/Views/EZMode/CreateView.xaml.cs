using Amuse.UI.Commands;
using Amuse.UI.Dialogs;
using Amuse.UI.Enums;
using Amuse.UI.Exceptions;
using Amuse.UI.Models;
using Amuse.UI.Models.StableDiffusion;
using Microsoft.Extensions.Logging;
using OnnxStack.StableDiffusion.Config;
using OnnxStack.StableDiffusion.Enums;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Amuse.UI.Views.EZMode
{
    /// <summary>
    /// Interaction logic for CreateView.xaml
    /// </summary>
    public partial class CreateView : EZModeViewBase
    {
        private bool _isPromptEnhanceEnabled;
        private bool _isNegativePromptEnabled;

        private HardwareProfileGroup _profileGroup;
        private HardwareProfileOption _profileOption;
        private HardwareProfileAspectType _selectedResolution;
        private ModelTemplateViewModel _selectedModelTemplate;
        private ModelTemplateViewModel _selectedControlNetTemplate;

        private int _surfaceWidth;
        private int _surfaceHeight;
        private bool _isRandomSeedEnabled = true;
        private float _generateStrength = 0.75f;
        private int _generateSteps = 8;
        private int _currentSeed;
        private float _assistantStrength = 0.5f;
        private ICollectionView _presetCollectionView;
        private ControlNetProfile _selectedControlNetProfile;
        private EZModeSettings _createSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateView"/> class.
        /// </summary>
        public CreateView()
        {
            _currentSeed = Random.Shared.Next();
            SupportedDiffusers = new() { DiffuserType.ImageToImage, DiffuserType.ControlNet, DiffuserType.ControlNetImage };
            SchedulerOptions = new SchedulerOptionsModel { SchedulerType = SchedulerType.DDPM };
            ShowPreviewCommand = new AsyncRelayCommand<ImageResult>(ShowPreview);
            SelectResolutionCommand = new RelayCommand<HardwareProfileAspectType>(SelectResolution);
            InfoDialogCommand = new AsyncRelayCommand(InfoDialog);
            SettingsDialogCommand = new AsyncRelayCommand(SettingsDialog);
            ShowNormalModeCommand = new AsyncRelayCommand(ShowNormalMode);
            AutoGenerateCommand = new AsyncRelayCommand(AutoGenerate);
            RandomizeSeedCommand = new AsyncRelayCommand(RandomizeSeed);
            InitializeComponent();
        }

        public static readonly DependencyProperty SwitchUIModeCommandProperty =
            DependencyProperty.Register("SwitchUIModeCommand", typeof(AsyncRelayCommand<UIModeType>), typeof(CreateView));
        public static readonly DependencyProperty SizeToContentProperty =
            DependencyProperty.Register("SizeToContent", typeof(SizeToContent), typeof(CreateView));
        public override bool IsUpscalerSupported => false;
        public override bool IsControlNetSupported => false;
        public override bool IsFeatureExtractorSupported => false;
        public AsyncRelayCommand AutoGenerateCommand { get; }
        public AsyncRelayCommand<ImageResult> ShowPreviewCommand { get; }
        public RelayCommand<HardwareProfileAspectType> SelectResolutionCommand { get; }
        public AsyncRelayCommand InfoDialogCommand { get; }
        public AsyncRelayCommand SettingsDialogCommand { get; }
        public AsyncRelayCommand ShowNormalModeCommand { get; }
        public AsyncRelayCommand RandomizeSeedCommand { get; }

        public SizeToContent SizeToContent
        {
            get { return (SizeToContent)GetValue(SizeToContentProperty); }
            set { SetValue(SizeToContentProperty, value); }
        }

        public AsyncRelayCommand<UIModeType> SwitchUIModeCommand
        {
            get { return (AsyncRelayCommand<UIModeType>)GetValue(SwitchUIModeCommandProperty); }
            set { SetValue(SwitchUIModeCommandProperty, value); }
        }

        public HardwareProfileAspectType SelectedResolution
        {
            get { return _selectedResolution; }
            set { _selectedResolution = value; NotifyPropertyChanged(); }
        }

        public ModelTemplateViewModel SelectedModelTemplate
        {
            get { return _selectedModelTemplate; }
            set { _selectedModelTemplate = value; NotifyPropertyChanged(); }
        }

        public ModelTemplateViewModel SelectedControlNetTemplate
        {
            get { return _selectedControlNetTemplate; }
            set { _selectedControlNetTemplate = value; NotifyPropertyChanged(); }
        }

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

        public HardwareProfileOption ProfileOption
        {
            get { return _profileOption; }
            set { _profileOption = value; NotifyPropertyChanged(); }
        }

        public int GenerateSteps
        {
            get { return _generateSteps; }
            set { _generateSteps = value; NotifyPropertyChanged(); RefreshSchedulerOptions(); }
        }

        public float GenerateStrength
        {
            get { return _generateStrength; }
            set { _generateStrength = value; NotifyPropertyChanged(); RefreshSchedulerOptions(); }
        }

        public bool IsRandomSeedEnabled
        {
            get { return _isRandomSeedEnabled; }
            set { _isRandomSeedEnabled = value; NotifyPropertyChanged(); }
        }

        public int SurfaceWidth
        {
            get { return _surfaceWidth; }
            set { _surfaceWidth = value; NotifyPropertyChanged(); }
        }

        public int SurfaceHeight
        {
            get { return _surfaceHeight; }
            set { _surfaceHeight = value; NotifyPropertyChanged(); }
        }

        public float AssistantStrength
        {
            get { return _assistantStrength; }
            set { _assistantStrength = value; NotifyPropertyChanged(); RefreshSchedulerOptions(); }
        }

        public ICollectionView PresetCollectionView
        {
            get { return _presetCollectionView; }
            set { _presetCollectionView = value; NotifyPropertyChanged(); }
        }

        public ControlNetProfile SelectedControlNetProfile
        {
            get { return _selectedControlNetProfile; }
            set { _selectedControlNetProfile = value; NotifyPropertyChanged(); UpdateControlNet(); RefreshSchedulerOptions(); }
        }


        protected override Task OnSettingsChanged()
        {
            _createSettings = Settings.EZModeProfile.Create;
            PromptOptions.Prompt = _createSettings.DemoPrompt;
            SetResultLayout();
            return base.OnSettingsChanged();
        }


        protected override Task OnDefaultExecutionDeviceChanged()
        {
            var hardwareProfile = DeviceService.GetHardwareProfile();
            _profileGroup = hardwareProfile.Create;

            OnGenerationModeChanged();
            UpdateControlNet();
            SelectResolution(ProfileOption.Aspect);

            Logger.LogInformation($"[CreateView] [OnDefaultExecutionDeviceChanged] - Hardware profile selected, Profile: {hardwareProfile.Name}");
            return base.OnDefaultExecutionDeviceChanged();
        }


        protected override void OnGenerationModeChanged()
        {
            BatchOptions.IsRealtimeEnabled = false;
            ProfileOption = IsVideoGenerationMode
                ? _profileGroup.VideoProfile
                : _profileGroup.ImageProfile;

            SelectedModelTemplate = Settings.Templates.FirstOrDefault(x => x.Id == ProfileOption.ModelId);
            SelectedControlNetProfile = ProfileOption.ControlNet.FirstOrDefault();
            PresetCollectionView = new ListCollectionView(ProfileOption.ControlNet);

            UpdateControlNet();
            base.OnGenerationModeChanged();
        }


        private void UpdateControlNet()
        {
            SelectedControlNetTemplate = Settings.Templates.FirstOrDefault(x => x.Id == SelectedControlNetProfile?.ModelId);

            ResetControlImageCache();
            IsControlNetEnabled = SelectedControlNetTemplate is not null;
            GenerateStrength = IsControlNetEnabled ? 1 : 0.75f;
        }


        protected override async Task<GenerateOptions> GetGenerateImageOptionsAsync(CancellationToken cancellationToken)
        {
            var promptOptions = await base.GetGenerateImageOptionsAsync(cancellationToken);
            if (_isPromptEnhanceEnabled)
            {
                promptOptions.Prompt += _createSettings.ImagePrompt;
                promptOptions.NegativePrompt += _createSettings.ImageNegativePrompt;
            }
            return promptOptions;
        }


        protected override async Task GenerateImage()
        {
            try
            {
                if (await ModeratorService.ContainsExplicitContentAsync(PromptOptions.Prompt))
                    throw new OperationCanceledException();

                // Check if model is installed
                if (!_selectedModelTemplate.IsInstalled || (_selectedControlNetTemplate is not null && !SelectedControlNetTemplate.IsInstalled))
                {
                    var downloadModelDialog = DialogService.GetDialog<ModelDownloadDialog>();
                    if (!await downloadModelDialog.ShowDialogAsync(_selectedModelTemplate, _selectedControlNetTemplate))
                        throw new OperationCanceledException();
                }

                ResultImage = null;
                IsGenerating = true;
                SetResultLayout();
                using (CancelationTokenSource = new CancellationTokenSource())
                {
                    // Select & Load Models
                    SelectedBaseModel = Settings.StableDiffusionModelSets.FirstOrDefault(x => x.Id == _selectedModelTemplate.Id);
                    SelectedControlNetModel = _selectedControlNetTemplate is null
                        ? default
                        : Settings.ControlNetModelSets.FirstOrDefault(x => x.Id == _selectedControlNetTemplate.Id);
                    await LoadPipelineAsync();

                    // LoadPipeline will reset Scheduler Options to default
                    // so reset the Resolution/Aspect ratio
                    SelectResolution(_selectedResolution);
  
                    RefreshSchedulerOptions();

                    // Generate
                    StartStatistics();
                    if (BatchOptions.IsRealtimeEnabled)
                    {
                        SchedulerOptions.Seed = _currentSeed;
                        await foreach (var resultImage in GenerateImageRealtimeAsync(CancelationTokenSource.Token))
                        {
                            if (resultImage is null)
                                continue;

                            ResultImage = resultImage;
                        }
                    }
                    else
                    {
                        if (_isRandomSeedEnabled)
                            GetRandomizeSeed();

                        SchedulerOptions.Seed = _currentSeed;
                        var resultImage = await GenerateImageResultAsync(CancelationTokenSource.Token);
                        ResultImage = resultImage;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Logger.LogInformation("[CreateView] [Generate] - Generate was canceled");
            }
            catch (UnrecoverableException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError("[CreateView] [Generate] - Error during Generate\n{ex}", ex);
                await App.UIInvokeAsync(() => DialogService.ShowErrorMessageAsync("Generate Error", ex.Message));
            }
            StopStatistics();
            Reset();
        }


        protected override bool CanExecuteGenerate()
        {
            return !IsGenerating && (IsVideoGenerationMode && InputVideo is not null) || (!IsVideoGenerationMode && InputImage is not null);
        }


        protected override Task Cancel()
        {
            return base.Cancel();
        }


        protected override void Reset()
        {
            BatchOptions.IsRealtimeEnabled = false;
            base.Reset();
        }


        protected override async Task ClearHistory()
        {
            await base.ClearHistory();
            SetResultLayout();
        }


        protected override bool CanExecuteClearHistory()
        {
            return ResultImage is not null;
        }


        private async Task AutoGenerate()
        {
            BatchOptions.IsRealtimeEnabled = true;
            await GenerateImage();
        }


        private Task RandomizeSeed()
        {
            SchedulerOptions.Seed = GetRandomizeSeed();
            return Task.CompletedTask;
        }


        private int GetRandomizeSeed()
        {
            _currentSeed = Random.Shared.Next();
            return _currentSeed;
        }


        private void RefreshSchedulerOptions()
        {
            SchedulerOptions.Strength =  _generateStrength;
            SchedulerOptions.InferenceSteps = _generateSteps;
            SchedulerOptions.ConditioningScale = (1f - _assistantStrength);
        }


        private Task ShowPreview(ImageResult source)
        {
            ResultImage = source;
            return Task.CompletedTask;
        }


        private void SelectResolution(HardwareProfileAspectType resolution)
        {
            SelectedResolution = resolution;
            if (App.CurrentWindow?.WindowState == WindowState.Normal)
                SizeToContent = SizeToContent.Width;

            if (resolution == HardwareProfileAspectType.Default)
            {

                SchedulerOptions.Width = ProfileOption.Default.Width;
                SchedulerOptions.Height = ProfileOption.Default.Height;
            }
            else if (resolution == HardwareProfileAspectType.Landscape)
            {
                SchedulerOptions.Width = ProfileOption.Landscape.Width;
                SchedulerOptions.Height = ProfileOption.Landscape.Height;
            }
            else if (resolution == HardwareProfileAspectType.Portrait)
            {
                SchedulerOptions.Width = ProfileOption.Portrait.Width;
                SchedulerOptions.Height = ProfileOption.Portrait.Height;
            }
            SurfaceWidth = SchedulerOptions.Width;
            SurfaceHeight = SchedulerOptions.Height;
        }


        private void SetResultLayout()
        {
            ImageResults.Clear();
        }


        private async Task InfoDialog()
        {
            var infoDialog = DialogService.GetDialog<Dialogs.EZMode.InformationDialog>();
            await infoDialog.ShowDialogAsync();
        }


        private async Task SettingsDialog()
        {
            var settingsDialog = DialogService.GetDialog<Dialogs.EZMode.SettingsDialog>();
            if (await settingsDialog.ShowDialogAsync(_isNegativePromptEnabled, _isPromptEnhanceEnabled))
            {
                IsPromptEnhanceEnabled = settingsDialog.IsPromptEnhanceEnabled;
                IsNegativePromptEnabled = settingsDialog.IsNegativePromptEnabled;
            }
        }


        private Task ShowNormalMode()
        {
            SwitchUIModeCommand.Execute(UIModeType.Normal);
            return Task.CompletedTask;
        }
    }
}