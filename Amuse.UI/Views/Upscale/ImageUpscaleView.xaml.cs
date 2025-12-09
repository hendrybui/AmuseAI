using Amuse.UI.Commands;
using Amuse.UI.Dialogs;
using Amuse.UI.Models;
using Amuse.UI.Models.Upscale;
using Amuse.UI.Services;
using Microsoft.Extensions.Logging;
using OnnxStack.Core.Image;
using OnnxStack.FeatureExtractor.Pipelines;
using OnnxStack.ImageUpscaler.Common;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Amuse.UI.Views
{
    /// <summary>
    /// Interaction logic for UpscaleView.xaml
    /// </summary>
    public partial class ImageUpscaleView : ViewBase, INavigatable
    {
        private readonly IModelCacheService _modelCacheService;

        private UpscaleInfoModel _upscaleInfo;
        private UpscaleResult _resultImage;
        private int _selectedTabIndex;
        private bool _isHistoryView;
        private ImageInput _inputImage;
        private bool _showFullImage;
        private ScrollBarVisibility _scrollBarVisibility;
        private TileMode _tileMode;
        private int _tileSize = 512;
        private int _tileOverlap = 16;
        private int _previousTileSize = 512;
        private TileMode _previousTileMode;
        private ImageInput _sourceImage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageUpscaleView"/> class.
        /// </summary>
        public ImageUpscaleView()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
                _modelCacheService = App.GetService<IModelCacheService>();

            CancelCommand = new AsyncRelayCommand(Cancel, CanExecuteCancel);
            GenerateCommand = new AsyncRelayCommand(Generate, CanExecuteGenerate);
            ClearHistoryCommand = new AsyncRelayCommand(ClearHistory, CanExecuteClearHistory);
            SaveImageCommand = new AsyncRelayCommand<UpscaleResult>(SaveImage);
            CopyImageCommand = new AsyncRelayCommand<UpscaleResult>(CopyImage);
            RemoveImageCommand = new AsyncRelayCommand<UpscaleResult>(RemoveImage);
            PreviewImageCommand = new AsyncRelayCommand<UpscaleResult>(PreviewImage);
            LoadModelCommand = new AsyncRelayCommand(LoadModelAsync, CanLoadModel);
            UnloadModelCommand = new AsyncRelayCommand(UnloadModelAsync, CanUnloadModel);
            ImageResults = new ObservableCollection<UpscaleResult>();
            UpscaleInfo = new UpscaleInfoModel();
            InitializeComponent();
        }

        public static readonly DependencyProperty SelectedModelProperty =
            DependencyProperty.Register(nameof(SelectedModel), typeof(UpscaleModelSetViewModel), typeof(ImageUpscaleView));

        public AsyncRelayCommand CancelCommand { get; }
        public AsyncRelayCommand GenerateCommand { get; }
        public AsyncRelayCommand ClearHistoryCommand { get; }
        public AsyncRelayCommand<UpscaleResult> SaveImageCommand { get; }
        public AsyncRelayCommand<UpscaleResult> CopyImageCommand { get; }
        public AsyncRelayCommand<UpscaleResult> RemoveImageCommand { get; }
        public AsyncRelayCommand<UpscaleResult> PreviewImageCommand { get; }
        public ObservableCollection<UpscaleResult> ImageResults { get; }
        public AsyncRelayCommand LoadModelCommand { get; }
        public AsyncRelayCommand UnloadModelCommand { get; }

        public UpscaleModelSetViewModel SelectedModel
        {
            get { return (UpscaleModelSetViewModel)GetValue(SelectedModelProperty); }
            set { SetValue(SelectedModelProperty, value); }
        }


        public UpscaleResult ResultImage
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

        public ImageInput SourceImage
        {
            get { return _sourceImage; }
            set { _sourceImage = value; NotifyPropertyChanged(); }
        }

        public ImageInput InputImage
        {
            get { return _inputImage; }
            set { _inputImage = value; NotifyPropertyChanged(); UpdateInfo(); }
        }

        public bool ShowFullImage
        {
            get { return _showFullImage; }
            set { _showFullImage = value; NotifyPropertyChanged(); UpdateScrollBar(); }
        }

        public ScrollBarVisibility ScrollBarVisibility
        {
            get { return _scrollBarVisibility; }
            set { _scrollBarVisibility = value; NotifyPropertyChanged(); }
        }

        public UpscaleInfoModel UpscaleInfo
        {
            get { return _upscaleInfo; }
            set { _upscaleInfo = value; NotifyPropertyChanged(); }
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
                    OnnxImage resultImage = await ExecuteUpscalerAsync(inputImage, CancelationTokenSource.Token);
                    if (resultImage != null)
                    {
                        var elapsed = Stopwatch.GetElapsedTime(timestamp).TotalSeconds;
                        var imageResult = new UpscaleResult(await resultImage.ToBitmapAsync(), UpscaleInfo with { }, elapsed);
                        ResultImage = imageResult;
                        await FileService.AutoSaveImageFile(imageResult, "Upscale");
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
                await DialogService.ShowErrorMessageAsync("Upscale Error", ex.Message);
            }
            StopStatistics();
            Reset();
        }


        /// <summary>
        /// Determines whether this instance can execute Generate.
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
        /// Determines whether this instance can execute Cancel.
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
            UpdateProgress("Loading Pipeline...", true);
            await LoadUpscalerAsync();
            TileMode = SelectedModel.ModelSet.TileMode;
            TileSize = SelectedModel.ModelSet.TileSize;
            TileOverlap = SelectedModel.ModelSet.TileOverlap;
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
            await UnloadUpscalerAsync();
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
        /// Determines whether this instance can execute ClearHistory.
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
        private async Task SaveImage(UpscaleResult result)
        {
            await FileService.SaveAsImageFile(result);
        }


        /// <summary>
        /// Copies the image to clipboard.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private Task CopyImage(UpscaleResult result)
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
        private async Task PreviewImage(UpscaleResult result)
        {
            var previewDialog = DialogService.GetDialog<PreviewImageDialog>();
            await previewDialog.ShowDialogAsync("Image Preview", result);
        }


        /// <summary>
        /// Removes the image from history list.
        /// </summary>
        /// <param name="result">The result.</param>
        private Task RemoveImage(UpscaleResult result)
        {
            ImageResults.Remove(result);
            if (result == ResultImage)
            {
                ResultImage = null;
            }
            return Task.CompletedTask;
        }


        /// <summary>
        /// Updates the scroll bars.
        /// </summary>
        private void UpdateScrollBar()
        {
            ScrollBarVisibility = _showFullImage
                ? ScrollBarVisibility.Auto
                : ScrollBarVisibility.Disabled;
        }


        /// <summary>
        /// Updates the display information.
        /// </summary>
        private void UpdateInfo()
        {
            ResultImage = null;
            UpscaleInfo = SelectedModel == null
                ? new UpscaleInfoModel
                {
                    InputWidth = InputImage?.Image?.PixelWidth ?? 0,
                    InputHeight = InputImage?.Image?.PixelHeight ?? 0
                }
                : new UpscaleInfoModel
                {
                    SampleSize = SelectedModel.ModelSet.SampleSize,
                    ScaleFactor = SelectedModel.ModelSet.ScaleFactor,
                    InputWidth = InputImage?.Image?.PixelWidth ?? SelectedModel.ModelSet.SampleSize,
                    InputHeight = InputImage?.Image?.PixelHeight ?? SelectedModel.ModelSet.SampleSize
                };
        }


        /// <summary>
        /// Loads the upscaler.
        /// </summary>
        /// <param name="upscaler">The upscaler.</param>
        protected async Task<ImageUpscalePipeline> LoadUpscalerAsync()
        {
            if (SelectedModel is null)
                return default;

            UpdateInfo();
            return await _modelCacheService.LoadModelAsync(SelectedModel);
        }


        /// <summary>
        /// Unloads the upscaler.
        /// </summary>
        protected async Task UnloadUpscalerAsync()
        {
            if (SelectedModel is null)
                return;

            await _modelCacheService.UnloadModelAsync(SelectedModel);
        }


        /// <summary>
        /// Executes the upscaler.
        /// </summary>
        /// <param name="inputImage">The input image.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        protected async Task<OnnxImage> ExecuteUpscalerAsync(OnnxImage inputImage, CancellationToken cancellationToken)
        {
            if (_tileMode != TileMode.None && (_previousTileMode != _tileMode || _previousTileSize != _tileSize))
            {
                await UnloadUpscalerAsync();
                UpdateProgress("Loading Pipeline...", true);
            }

            StartStatistics();
            _previousTileSize = _tileSize;
            _previousTileMode = _tileMode;
            var upscalePipeline = await LoadUpscalerAsync();
            UpdateProgress("Upscaling Image...", true);
            var options = new UpscaleOptions(_tileMode, _tileSize, _tileOverlap);
            return await Task.Run(() => upscalePipeline.RunAsync(inputImage, options, cancellationToken));
        }

    }
}