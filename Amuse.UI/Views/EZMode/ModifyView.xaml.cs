using Amuse.UI.Commands;
using Amuse.UI.Dialogs;
using Amuse.UI.Enums;
using Amuse.UI.Exceptions;
using Amuse.UI.Models;
using Amuse.UI.Models.StableDiffusion;
using Microsoft.Extensions.Logging;
using OnnxStack.StableDiffusion.Common;
using OnnxStack.StableDiffusion.Config;
using OnnxStack.StableDiffusion.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Amuse.UI.Views.EZMode
{
    public partial class ModifyView : EZModeViewBase
    {
        private HardwareProfileGroup _profileGroup;
        private HardwareProfileOption _profileOption;
        private HardwareProfileAspectType _selectedResolution;
        private ModelTemplateViewModel _selectedModelTemplate;
        private ModelTemplateViewModel _selectedControlNetTemplate;
        private ModelTemplateViewModel _selectedFeatureExtractorTemplate;

        private bool _isPromptEnhanceEnabled;
        private bool _isNegativePromptEnabled;
        private float _assistantStrength = 0.65f;
        private int _currentSeed;
        private DefaultSetting _selectedCoherency;
        private bool _isRandomSeedEnabled = true;
        private EZModeSettings _modifySettings;
        private ControlNetProfile _selectedControlNetProfile;
        private HardwareProfileQualityType _selectedModelQuality;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifyView"/> class.
        /// </summary>
        public ModifyView()
        {
            _currentSeed = Random.Shared.Next();
            SupportedDiffusers = new() { DiffuserType.ImageToImage, DiffuserType.ControlNet, DiffuserType.ControlNetImage };
            SchedulerOptions = new SchedulerOptionsModel { SchedulerType = SchedulerType.DDPM };
            ShowImagePreviewCommand = new AsyncRelayCommand<ImageResult>(ShowImagePreview);
            ShowVideoPreviewCommand = new AsyncRelayCommand<VideoResultModel>(ShowVideoPreview);
            SelectResolutionCommand = new RelayCommand<HardwareProfileAspectType>(SelectResolution);
            InfoDialogCommand = new AsyncRelayCommand(InfoDialog);
            SettingsDialogCommand = new AsyncRelayCommand(SettingsDialog);
            ShowNormalModeCommand = new AsyncRelayCommand(ShowNormalMode);
            InitializeComponent();
        }

        public static readonly DependencyProperty SwitchUIModeCommandProperty =
            DependencyProperty.Register("SwitchUIModeCommand", typeof(AsyncRelayCommand<UIModeType>), typeof(ModifyView));
        public static readonly DependencyProperty SizeToContentProperty =
            DependencyProperty.Register("SizeToContent", typeof(SizeToContent), typeof(ModifyView));
        public override bool IsUpscalerSupported => false;
        public override bool IsControlNetSupported => false;
        public override bool IsFeatureExtractorSupported => false;
        public AsyncRelayCommand<ImageResult> ShowImagePreviewCommand { get; }
        public AsyncRelayCommand<VideoResultModel> ShowVideoPreviewCommand { get; }
        public RelayCommand<HardwareProfileAspectType> SelectResolutionCommand { get; }
        public AsyncRelayCommand InfoDialogCommand { get; }
        public AsyncRelayCommand SettingsDialogCommand { get; }
        public AsyncRelayCommand ShowNormalModeCommand { get; }
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

        public ModelTemplateViewModel SelectedFeatureExtractorTemplate
        {
            get { return _selectedFeatureExtractorTemplate; }
            set { _selectedFeatureExtractorTemplate = value; NotifyPropertyChanged(); }
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

        public float AssistantStrength
        {
            get { return _assistantStrength; }
            set { _assistantStrength = value; NotifyPropertyChanged(); RefreshSchedulerOptions(); }
        }

        public DefaultSetting SelectedCoherency
        {
            get { return _selectedCoherency; }
            set { _selectedCoherency = value; NotifyPropertyChanged(); }
        }

        public bool IsRandomSeedEnabled
        {
            get { return _isRandomSeedEnabled; }
            set { _isRandomSeedEnabled = value; NotifyPropertyChanged(); }
        }

        public ControlNetProfile SelectedControlNetProfile
        {
            get { return _selectedControlNetProfile; }
            set { _selectedControlNetProfile = value; NotifyPropertyChanged(); UpdateControlNet(_selectedControlNetProfile); }
        }

        public HardwareProfileQualityType SelectedModelQuality
        {
            get { return _selectedModelQuality; }
            set { _selectedModelQuality = value; NotifyPropertyChanged(); RefreshSchedulerOptions(); }
        }


        protected override Task OnSettingsChanged()
        {
            _modifySettings = Settings.EZModeProfile.Modify;

            BatchOptions.ValueTo = 4;
            PromptOptions.Prompt = _modifySettings.DemoPrompt;
            SetResultLayout();
            return base.OnSettingsChanged();
        }


        protected override Task OnDefaultExecutionDeviceChanged()
        {
            var hardwareProfile = DeviceService.GetHardwareProfile();
            _profileGroup = hardwareProfile.Modify;

            OnGenerationModeChanged();

            Logger.LogInformation($"[ModifyView] [OnDefaultExecutionDeviceChanged] - Hardware profile selected, Profile: {hardwareProfile.Name}");
            return base.OnDefaultExecutionDeviceChanged();
        }


        protected override void OnGenerationModeChanged()
        {
            BatchOptions.IsRealtimeEnabled = false;
            ProfileOption = IsVideoGenerationMode
                ? _profileGroup.VideoProfile
                : _profileGroup.ImageProfile;

            SelectResolution(ProfileOption.Aspect);
            SelectedModelTemplate = Settings.Templates.FirstOrDefault(x => x.Id == ProfileOption.ModelId);
            SelectedControlNetProfile = ProfileOption.ControlNet.FirstOrDefault();
            base.OnGenerationModeChanged();
        }


        private void UpdateControlNet(ControlNetProfile selectedControlNetProfile)
        {
            SelectedControlNetTemplate = Settings.Templates.FirstOrDefault(x => x.Id == selectedControlNetProfile?.ModelId);
            SelectedFeatureExtractorTemplate = Settings.Templates.FirstOrDefault(x => x.Id == selectedControlNetProfile?.ExtractorId);

            ResetControlImageCache();
            IsControlNetEnabled = SelectedControlNetTemplate is not null;
            IsFeatureExtractorEnabled = SelectedFeatureExtractorTemplate is not null;
        }


        protected override async Task<GenerateOptions> GetGenerateImageOptionsAsync(CancellationToken cancellationToken)
        {
            var promptOptions = await base.GetGenerateImageOptionsAsync(cancellationToken);
            if (_isPromptEnhanceEnabled)
            {
                promptOptions.Prompt += _modifySettings.ImagePrompt;
                promptOptions.NegativePrompt += _modifySettings.ImageNegativePrompt;
            }
            return promptOptions;
        }


        protected override async Task<GenerateOptions> GetGenerateVideoOptionsAsync(CancellationToken cancellationToken)
        {
            var videoOptions = await base.GetGenerateVideoOptionsAsync(cancellationToken);
            videoOptions.Diffuser = DiffuserType.ControlNetVideo;
            videoOptions.FrameResample = PromptOptions.FrameResample;
            videoOptions.FrameUpSample = PromptOptions.FrameUpSample;
            videoOptions.FrameDownSample = PromptOptions.FrameDownSample;
            videoOptions.MotionContextOverlap = GetMotionContextOverlap();
            if (_isPromptEnhanceEnabled)
            {
                videoOptions.Prompt += _modifySettings.VideoPrompt;
                videoOptions.NegativePrompt += _modifySettings.VideoNegativePrompt;
            }
            return videoOptions;
        }


        protected override async Task GenerateImage()
        {
            try
            {
                if (await ModeratorService.ContainsExplicitContentAsync(PromptOptions.Prompt))
                    throw new OperationCanceledException();

                // Check if model is installed
                if (!_selectedModelTemplate.IsInstalled
                    || (_selectedControlNetTemplate is not null && !_selectedControlNetTemplate.IsInstalled)
                    || (_selectedFeatureExtractorTemplate is not null && !_selectedFeatureExtractorTemplate.IsInstalled))
                {
                    var downloadModelDialog = DialogService.GetDialog<ModelDownloadDialog>();
                    if (!await downloadModelDialog.ShowDialogAsync(_selectedModelTemplate, _selectedControlNetTemplate, _selectedFeatureExtractorTemplate))
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
                    SelectedFeatureExtractorModel = _selectedFeatureExtractorTemplate is null
                       ? default
                       : Settings.FeatureExtractorModelSets.FirstOrDefault(x => x.Id == _selectedFeatureExtractorTemplate.Id);
                    await LoadPipelineAsync();

                    // LoadPipeline will reset Scheduler Options to default
                    // so reset the Resolution/Aspect ratio
                    SelectResolution(_selectedResolution);

                    RefreshSchedulerOptions();

                    if (_isRandomSeedEnabled)
                        RandomizeSeed();

                    StartStatistics();
                    SchedulerOptions.Seed = _currentSeed;
                    if (BatchOptions.IsRealtimeEnabled)
                    {
                        await foreach (var resultImage in GenerateImageRealtimeAsync(CancelationTokenSource.Token))
                        {
                            if (resultImage is null)
                                continue;

                            ResultImage = resultImage;
                               ImageResults.Insert(0, resultImage);
                            if (ImageResults.Count > BatchOptions.ValueTo)
                                ImageResults.RemoveAt(ImageResults.Count - 1);

                            if (BatchOptions.IsRealtimeEnabled && IsRandomSeedEnabled)
                                SchedulerOptions.Seed = 0;

                            Statistics.Reset();
                        }
                    }
                    else if (BatchOptions.IsAutomationEnabled)
                    {
                        var index = 0;
                        await foreach (var resultImage in GenerateImageBatchAsync(CancelationTokenSource.Token))
                        {
                            if (resultImage is null)
                                continue;

                            ResultImage = resultImage;
                            ImageResults[index] = resultImage;
                            index++;
                            if (index < BatchOptions.ValueTo)
                                Statistics.Reset();
                             }
                    }
                    else
                    {
                        // Generate
                        var resultImage = await GenerateImageResultAsync(CancelationTokenSource.Token);
                        ResultImage = resultImage;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Logger.LogInformation("[ModifyView] [Generate] - Generate was canceled");
            }
            catch (UnrecoverableException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError("[ModifyView] [Generate] - Error during Generate\n{ex}", ex);
                await App.UIInvokeAsync(() => DialogService.ShowErrorMessageAsync("Generate Error", ex.Message));
            }
            StopStatistics();
            Reset();
        }


        protected override async Task GenerateVideo()
        {
            try
            {
                if (await ModeratorService.ContainsExplicitContentAsync(PromptOptions.Prompt))
                    throw new OperationCanceledException();

                // Check if model is installed
                if (!_selectedModelTemplate.IsInstalled
                    || (_selectedControlNetTemplate is not null && !_selectedControlNetTemplate.IsInstalled)
                    || (_selectedFeatureExtractorTemplate is not null && !_selectedFeatureExtractorTemplate.IsInstalled))
                {
                    var downloadModelDialog = DialogService.GetDialog<ModelDownloadDialog>();
                    if (!await downloadModelDialog.ShowDialogAsync(_selectedModelTemplate, _selectedControlNetTemplate, _selectedFeatureExtractorTemplate))
                        throw new OperationCanceledException();
                }

                IsGenerating = true;
                ResultVideo = null;
                SetResultLayout();
                using (CancelationTokenSource = new CancellationTokenSource())
                {
                    var variant = default(string);
                    if (CurrentPipeline?.IsLoaded == true)
                    {
                        if (SelectedVariant != variant)
                            await UnloadPipelineAsync();
                    }

                    // Select & Load Model
                    SelectedBaseModel = Settings.StableDiffusionModelSets.FirstOrDefault(x => x.Id == _selectedModelTemplate.Id);
                    SelectedControlNetModel = _selectedControlNetTemplate is null
                        ? default
                        : Settings.ControlNetModelSets.FirstOrDefault(x => x.Id == _selectedControlNetTemplate.Id);
                    SelectedFeatureExtractorModel = _selectedFeatureExtractorTemplate is null
                       ? default
                       : Settings.FeatureExtractorModelSets.FirstOrDefault(x => x.Id == _selectedFeatureExtractorTemplate.Id);
                    await LoadPipelineAsync();

                    // LoadPipeline will reset Scheduler Options to default
                    // so reset the Resolution/Aspect ratio
                    SelectResolution(_selectedResolution);

                    RefreshSchedulerOptions();
                    if (_isRandomSeedEnabled)
                        RandomizeSeed();

                    // Generate
                    StartStatistics();
                    SchedulerOptions.Seed = _currentSeed;
                    if (BatchOptions.IsAutomationEnabled)
                    {
                        var index = 0;
                        await foreach (var resultVideo in GenerateVideoBatchAsync(CancelationTokenSource.Token))
                        {
                            if (resultVideo is null)
                                continue;

                            ResultVideo = resultVideo;
                            VideoResults[index] = resultVideo;
                            index++;
                            if (index < BatchOptions.ValueTo)
                                Statistics.Reset();

                            await SyncVideoAsync();
                        }
                    }
                    else
                    {
                        var resultImage = await GenerateVideoResultAsync(CancelationTokenSource.Token);
                        ResultVideo = resultImage;
                        await SyncVideoAsync();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Logger.LogInformation("[GenerateView] [Generate] - Generate was canceled");
            }
            catch (UnrecoverableException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError("[GenerateView] [Generate] - Error during Generate\n{ex}", ex);
                await App.UIInvokeAsync(() => DialogService.ShowErrorMessageAsync("Generate Error", ex.Message));
            }
            StopStatistics();
            Reset();
        }


        private int GetMotionContextOverlap()
        {
            return SelectedCoherency switch
            {
                DefaultSetting.Minimum => 1,
                DefaultSetting.Medium => 3,
                DefaultSetting.Maximum => 8,
                _ => 0
            };
        }


        protected override bool CanExecuteGenerate()
        {
            return !IsGenerating && (IsVideoGenerationMode && InputVideo is not null) || (!IsVideoGenerationMode && InputImage is not null);
        }


        protected override Task Cancel()
        {
            return base.Cancel();
        }


        private void RefreshSchedulerOptions()
        {
            SchedulerOptions.Strength = 1f;// _generateStrength;
            SchedulerOptions.InferenceSteps = GetSteps();
            SchedulerOptions.ConditioningScale = (1f - _assistantStrength);
        }


        private int GetSteps()
        {
            return _selectedModelQuality switch
            {
                HardwareProfileQualityType.Fast => ProfileOption.Steps[0],
                HardwareProfileQualityType.Balanced => ProfileOption.Steps[1],
                HardwareProfileQualityType.Quality => ProfileOption.Steps[2],
                _ => 8
            };
        }


        private int RandomizeSeed()
        {
            _currentSeed = Random.Shared.Next();
            return _currentSeed;
        }


        protected override async Task ClearHistory()
        {
            await base.ClearHistory();
            SetResultLayout();
        }


        protected override bool CanExecuteClearHistory()
        {
            return ResultImage is not null || ResultVideo is not null;
        }


        private Task ShowImagePreview(ImageResult source)
        {
            ResultImage = source;
            return Task.CompletedTask;
        }


        private Task ShowVideoPreview(VideoResultModel source)
        {
            ResultVideo = source;
            return Task.CompletedTask;
        }


        /// <summary>
        /// Updates the progress Image.
        /// </summary>
        /// <returns>Task.</returns>
        protected override Task UpdateProgressImage(DiffusionProgress progress)
        {
            var index = progress.BatchValue - 1;
            if (index >= 0 && index < ImageResults.Count)
                ImageResults[progress.BatchValue - 1].PreviewImage = PreviewResult;

            return Task.CompletedTask;
        }


        private void SelectResolution(HardwareProfileAspectType resolution)
        {
            SelectedResolution = resolution;
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
        }


        private void SetResultLayout()
        {
            ImageResults.Clear();
            VideoResults.Clear();
            if (BatchOptions.ValueTo > 1)
            {
                for (int i = 0; i < BatchOptions.ValueTo; i++)
                {
                    ImageResults.Add(new ImageResult { Image = Utils.CreateEmptyBitmapImage(SchedulerOptions.Width, SchedulerOptions.Height) });
                    VideoResults.Add(new VideoResultModel { Thumbnail = Utils.CreateEmptyBitmapImage(SchedulerOptions.Width, SchedulerOptions.Height) });
                }
            }

            if (BatchOptions.IsRealtimeEnabled)
                return;

            BatchOptions.IsAutomationEnabled = BatchOptions.ValueTo > 1;
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

        private void ResultImage_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.Source is Image image)
            {
                DragDrop.DoDragDrop(image, new DataObject(typeof(BitmapImage), image.Source), DragDropEffects.All);
            }
        }
    }

}