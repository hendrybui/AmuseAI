using Amuse.UI.Commands;
using Amuse.UI.Dialogs;
using Amuse.UI.Models;
using Amuse.UI.Models.FeatureExtractor;
using Amuse.UI.Services;
using Microsoft.Extensions.Logging;
using OnnxStack.Core.Image;
using OnnxStack.FeatureExtractor.Common;
using OnnxStack.FeatureExtractor.Pipelines;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Amuse.UI.Views
{
    /// <summary>
    /// Interaction logic for ImageFeatureExtractorView.xaml
    /// </summary>
    public partial class ImageFeatureExtractorView : ViewBase, INavigatable
    {
        private readonly IModelCacheService _modelCacheService;

        private FeatureExtractorInfoModel _featureExtractorInfo;
        private FeatureExtractorResult _resultImage;
        private int _selectedTabIndex;
        private bool _isHistoryView;
        private ImageInput _inputImage;
        private TileMode _tileMode;
        private int _tileSize = 512;
        private int _tileOverlap = 16;
        private int _previousTileSize = 512;
        private TileMode _previousTileMode;
        private int _value;
        private ImageInput _sourceImage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageFeatureExtractorView"/> class.
        /// </summary>
        public ImageFeatureExtractorView()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
                _modelCacheService = App.GetService<IModelCacheService>();

            CancelCommand = new AsyncRelayCommand(Cancel, CanExecuteCancel);
            GenerateCommand = new AsyncRelayCommand(Generate, CanExecuteGenerate);
            ClearHistoryCommand = new AsyncRelayCommand(ClearHistory, CanExecuteClearHistory);
            SaveImageCommand = new AsyncRelayCommand<FeatureExtractorResult>(SaveImage);
            CopyImageCommand = new AsyncRelayCommand<FeatureExtractorResult>(CopyImage);
            RemoveImageCommand = new AsyncRelayCommand<FeatureExtractorResult>(RemoveImage);
            PreviewImageCommand = new AsyncRelayCommand<FeatureExtractorResult>(PreviewImage);
            LoadModelCommand = new AsyncRelayCommand(LoadModelAsync, CanLoadModel);
            UnloadModelCommand = new AsyncRelayCommand(UnloadModelAsync, CanUnloadModel);
            ImageResults = new ObservableCollection<FeatureExtractorResult>();
            FeatureExtractorInfo = new FeatureExtractorInfoModel();
            InitializeComponent();
        }

        public static readonly DependencyProperty SelectedModelProperty =
            DependencyProperty.Register(nameof(SelectedModel), typeof(FeatureExtractorModelSetViewModel), typeof(ImageFeatureExtractorView));

        public AsyncRelayCommand CancelCommand { get; }
        public AsyncRelayCommand GenerateCommand { get; }
        public AsyncRelayCommand ClearHistoryCommand { get; }
        public AsyncRelayCommand<FeatureExtractorResult> SaveImageCommand { get; }
        public AsyncRelayCommand<FeatureExtractorResult> CopyImageCommand { get; }
        public AsyncRelayCommand<FeatureExtractorResult> RemoveImageCommand { get; }
        public AsyncRelayCommand<FeatureExtractorResult> PreviewImageCommand { get; }
        public ObservableCollection<FeatureExtractorResult> ImageResults { get; }
        public AsyncRelayCommand LoadModelCommand { get; }
        public AsyncRelayCommand UnloadModelCommand { get; }

        public FeatureExtractorModelSetViewModel SelectedModel
        {
            get { return (FeatureExtractorModelSetViewModel)GetValue(SelectedModelProperty); }
            set { SetValue(SelectedModelProperty, value); }
        }

        public ImageInput SourceImage
        {
            get { return _sourceImage; }
            set { _sourceImage = value; NotifyPropertyChanged(); }
        }

        public FeatureExtractorResult ResultImage
        {
            get { return _resultImage; }
            set { _resultImage = value; NotifyPropertyChanged(); }
        }

        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set { _selectedTabIndex = value; NotifyPropertyChanged(); }
        }

        public bool IsHistoryView
        {
            get { return _isHistoryView; }
            set { _isHistoryView = value; NotifyPropertyChanged(); }
        }

        public ImageInput InputImage
        {
            get { return _inputImage; }
            set { _inputImage = value; NotifyPropertyChanged(); UpdateInfo(); }
        }

        public FeatureExtractorInfoModel FeatureExtractorInfo
        {
            get { return _featureExtractorInfo; }
            set { _featureExtractorInfo = value; NotifyPropertyChanged(); }
        }

        public TileMode TileMode
        {
            get { return _tileMode; }
            set { _tileMode = value; NotifyPropertyChanged(); }
        }

        public int TileSize
        {
            get { return _tileSize; }
            set { _tileSize = value; NotifyPropertyChanged(); }
        }

        public int TileOverlap
        {
            get { return _tileOverlap; }
            set { _tileOverlap = value; NotifyPropertyChanged(); }
        }

        public int Value
        {
            get { return _value; }
            set { _value = value; NotifyPropertyChanged(); }
        }



        /// <summary>
        /// Called on Navigate image
        /// </summary>
        /// <param name="imageResult">The image result.</param>
        /// <returns></returns>
        public async Task NavigateAsync(IImageResult imageResult)
        {
            if (IsGenerating)
                await Cancel();

            Reset();
            ResultImage = null;
            SourceImage = new ImageInput
            {
                Image = imageResult.Image,
                FileName = "Generated Image"
            };
            UpdateInfo();
            SelectedTabIndex = 0;
            IsHistoryView = false;
        }


        /// <summary>
        /// Called on Navigate video
        /// </summary>
        /// <param name="videoResult">The video result.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task NavigateAsync(IVideoResult videoResult)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Generates this image result.
        /// </summary>
        private async Task Generate()
        {
            IsGenerating = true;
            ResultImage = null;

            try
            {
                using (CancelationTokenSource = new CancellationTokenSource())
                {
                    var timestamp = Stopwatch.GetTimestamp();
                    var inputImage = new OnnxImage(InputImage.Image.GetImageBytes());
                    OnnxImage resultImage = await ExecuteFeatureExtractorAsync(inputImage, CancelationTokenSource.Token);
                    if (resultImage != null)
                    {
                        var elapsed = Stopwatch.GetElapsedTime(timestamp).TotalSeconds;
                        var imageResult = new FeatureExtractorResult(await resultImage.ToBitmapAsync(), FeatureExtractorInfo with { }, elapsed);
                        ResultImage = imageResult;
                        await FileService.AutoSaveImageFile(imageResult, "FeatureExtractor");
                        ImageResults.Add(imageResult);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Logger.LogInformation($"Generate was canceled.");
            }
            catch (Exception ex)
            {
                Logger.LogError("Error during Generate\n{ex}", ex);
            }
            StopStatistics();
            Reset();
        }


        /// <summary>
        /// Determines whether Generate can execute.
        /// </summary>
        private bool CanExecuteGenerate()
        {
            return !IsGenerating && InputImage is not null;
        }


        /// <summary>
        /// Cancels this generation.
        /// </summary>
        /// <returns></returns>
        private Task Cancel()
        {
            CancelationTokenSource?.Cancel();
            return Task.CompletedTask;
        }


        /// <summary>
        /// Determines whether Cancel can execute.
        /// </summary>
        private bool CanExecuteCancel()
        {
            return IsGenerating;
        }


        /// <summary>
        /// Loads the model.
        /// </summary>
        private async Task LoadModelAsync()
        {
            TileMode = TileMode.None;
            TileSize = 512;
            TileOverlap = 16;
            if (SelectedModel.Id == Utils.InternalExtractorCanny)
            {
                Value = 150;
            }
            else if (SelectedModel.Id == Utils.InternalExtractorSoftEdge)
            {
                Value = 0;
            }
            await LoadFeatureExtractorAsync();
            ClearProgress();
        }


        /// <summary>
        /// Determines whether LoadModelAsync can execute.
        /// </summary>
        private bool CanLoadModel()
        {
            if (SelectedModel is null)
                return false;

            return !SelectedModel.IsLoaded;
        }


        /// <summary>
        /// Unloads the model.
        /// </summary>
        private async Task UnloadModelAsync()
        {
            await UnloadFeatureExtractorAsync();
            SelectedModel = null;
        }


        /// <summary>
        /// Determines whether UnloadModelAsync can execute.
        /// </summary>
        private bool CanUnloadModel()
        {
            return SelectedModel?.IsLoaded == true;
        }


        /// <summary>
        /// Clears the history.
        /// </summary>
        private Task ClearHistory()
        {
            ImageResults.Clear();
            return Task.CompletedTask;
        }


        /// <summary>
        /// Determines whether ClearHistory can execute.
        /// </summary>
        private bool CanExecuteClearHistory()
        {
            return ImageResults.Count > 0;
        }


        /// <summary>
        /// Resets this instance.
        /// </summary>
        private void Reset()
        {
            IsGenerating = false;
            ClearProgress();
        }


        /// <summary>
        /// Saves the image.
        /// </summary>
        /// <param name="result">The result.</param>
        private async Task SaveImage(FeatureExtractorResult result)
        {
            await FileService.SaveAsImageFile(result);
        }


        /// <summary>
        /// Copies the image to clipboard.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private Task CopyImage(FeatureExtractorResult result)
        {
            if (result == null)
                return Task.CompletedTask;

            Clipboard.SetImage(result.Image);
            return Task.CompletedTask;
        }


        /// <summary>
        /// Previews the image.
        /// </summary>
        /// <param name="result">The result.</param>
        private async Task PreviewImage(FeatureExtractorResult result)
        {
            var previewDialog = DialogService.GetDialog<PreviewImageDialog>();
            await previewDialog.ShowDialogAsync("Image Preview", result);
        }


        /// <summary>
        /// Removes the image from history list.
        /// </summary>
        /// <param name="result">The result.</param>
        private Task RemoveImage(FeatureExtractorResult result)
        {
            ImageResults.Remove(result);
            if (result == ResultImage)
            {
                ResultImage = null;
            }
            return Task.CompletedTask;
        }


        /// <summary>
        /// Updates the information.
        /// </summary>
        private void UpdateInfo()
        {
            ResultImage = null;
            FeatureExtractorInfo = SelectedModel == null
                ? new FeatureExtractorInfoModel()
                : new FeatureExtractorInfoModel
                {
                    Name = SelectedModel.ModelSet.Name,
                    SampleSize = SelectedModel.ModelSet.SampleSize,
                    Channels = SelectedModel.ModelSet.OutputChannels
                };
        }


        /// <summary>
        /// Loads the feature extractor .
        /// </summary>
        /// <returns></returns>
        protected async Task<FeatureExtractorPipeline> LoadFeatureExtractorAsync()
        {
            if (SelectedModel is null)
                return default;

            return await _modelCacheService.LoadModelAsync(SelectedModel);
        }


        /// <summary>
        /// Unloads the feature extractor.
        /// </summary>
        protected async Task UnloadFeatureExtractorAsync()
        {
            if (SelectedModel is null)
                return;

            await _modelCacheService.UnloadModelAsync(SelectedModel);
        }


        /// <summary>
        /// Executes the feature extractor.
        /// </summary>
        /// <param name="inputImage">The input image.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        protected async Task<OnnxImage> ExecuteFeatureExtractorAsync(OnnxImage inputImage, CancellationToken cancellationToken)
        {
            FeatureExtractorInfo.Name = SelectedModel.Name;
            if (_tileMode != TileMode.None && (_previousTileMode != _tileMode || _previousTileSize != _tileSize))
            {
                await UnloadFeatureExtractorAsync();
                UpdateProgress("Loading Pipeline...", true);
            }

            StartStatistics();
            _previousTileSize = _tileSize;
            _previousTileMode = _tileMode;
            var featureExtractorPipeline = await LoadFeatureExtractorAsync();
            UpdateProgress("Extracting Image Feature...", true);
            var options = new FeatureExtractorOptions(_tileMode, _tileSize, _tileOverlap)
            {
                Value = Value
            };
            return await featureExtractorPipeline.RunAsync(inputImage, options, cancellationToken);
        }

    }
}