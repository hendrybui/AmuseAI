using Amuse.UI.Commands;
using Amuse.UI.Dialogs;
using Amuse.UI.Models;
using Amuse.UI.Models.FeatureExtractor;
using Amuse.UI.Services;
using Microsoft.Extensions.Logging;
using OnnxStack.Core.Image;
using OnnxStack.Core.Video;
using OnnxStack.FeatureExtractor.Common;
using OnnxStack.FeatureExtractor.Pipelines;
using OnnxStack.StableDiffusion.Common;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Amuse.UI.Views
{
    /// <summary>
    /// Interaction logic for VideoFeatureExtractorView.xaml
    /// </summary>
    public partial class VideoFeatureExtractorView : ViewBase, INavigatable
    {
        private readonly IModelCacheService _modelCacheService;

        private FeatureExtractorInfoModel _featureExtractorInfo;
        private FeatureExtractorVideoResultModel _resultVideo;
        private bool _isHistoryView;
        private VideoInputModel _inputVideo;
        private BitmapSource _previewSource;
        private BitmapSource _previewResult;
        private IProgress<FeatureExtractorProgress> _progressCallback;
        private OnnxVideo _inputVideoCached;
        private TileMode _tileMode;
        private int _tileSize = 512;
        private int _tileOverlap = 16;
        private int _previousTileSize = 512;
        private TileMode _previousTileMode;
        private int _value;
        private VideoInputModel _sourceVideo;
        private bool _videoSync;

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoFeatureExtractorView"/> class.
        /// </summary>
        public VideoFeatureExtractorView()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
                _modelCacheService = App.GetService<IModelCacheService>();

            _progressCallback = CreateProgressCallback();
            CancelCommand = new AsyncRelayCommand(Cancel, CanExecuteCancel);
            GenerateCommand = new AsyncRelayCommand(Generate, CanExecuteGenerate);
            ClearHistoryCommand = new AsyncRelayCommand(ClearHistory, CanExecuteClearHistory);
            SaveVideoCommand = new AsyncRelayCommand<FeatureExtractorVideoResultModel>(SaveVideo);
            CopyVideoCommand = new AsyncRelayCommand<FeatureExtractorVideoResultModel>(CopyVideo);
            RemoveVideoCommand = new AsyncRelayCommand<FeatureExtractorVideoResultModel>(RemoveVideo);
            PreviewVideoCommand = new AsyncRelayCommand<FeatureExtractorVideoResultModel>(PreviewVideo);
            LoadModelCommand = new AsyncRelayCommand(LoadModelAsync, CanLoadModel);
            UnloadModelCommand = new AsyncRelayCommand(UnloadModelAsync, CanUnloadModel);
            VideoResults = new ObservableCollection<FeatureExtractorVideoResultModel>();
            FeatureExtractorInfo = new FeatureExtractorInfoModel();
            UpdateInfo();
            InitializeComponent();
        }

        public static readonly DependencyProperty SelectedModelProperty =
          DependencyProperty.Register(nameof(SelectedModel), typeof(FeatureExtractorModelSetViewModel), typeof(VideoFeatureExtractorView));

        public AsyncRelayCommand CancelCommand { get; }
        public AsyncRelayCommand GenerateCommand { get; }
        public AsyncRelayCommand ClearHistoryCommand { get; set; }
        public AsyncRelayCommand<FeatureExtractorVideoResultModel> SaveVideoCommand { get; set; }
        public AsyncRelayCommand<FeatureExtractorVideoResultModel> CopyVideoCommand { get; set; }
        public AsyncRelayCommand<FeatureExtractorVideoResultModel> RemoveVideoCommand { get; set; }
        public AsyncRelayCommand<FeatureExtractorVideoResultModel> PreviewVideoCommand { get; set; }
        public ObservableCollection<FeatureExtractorVideoResultModel> VideoResults { get; }
        public AsyncRelayCommand LoadModelCommand { get; }
        public AsyncRelayCommand UnloadModelCommand { get; }

        public FeatureExtractorModelSetViewModel SelectedModel
        {
            get { return (FeatureExtractorModelSetViewModel)GetValue(SelectedModelProperty); }
            set { SetValue(SelectedModelProperty, value); }
        }

        public FeatureExtractorVideoResultModel ResultVideo
        {
            get { return _resultVideo; }
            set { _resultVideo = value; NotifyPropertyChanged(); }
        }

        public VideoInputModel SourceVideo
        {
            get { return _sourceVideo; }
            set { _sourceVideo = value; NotifyPropertyChanged(); }
        }

        public VideoInputModel InputVideo
        {
            get { return _inputVideo; }
            set { _inputVideo = value; NotifyPropertyChanged(); UpdateInfo(); _inputVideoCached = null; }
        }

        public FeatureExtractorInfoModel FeatureExtractorInfo
        {
            get { return _featureExtractorInfo; }
            set { _featureExtractorInfo = value; NotifyPropertyChanged(); }
        }

        public BitmapSource PreviewSource
        {
            get { return _previewSource; }
            set { _previewSource = value; NotifyPropertyChanged(); }
        }

        public BitmapSource PreviewResult
        {
            get { return _previewResult; }
            set { _previewResult = value; NotifyPropertyChanged(); }
        }

        public bool IsHistoryView
        {
            get { return _isHistoryView; }
            set { _isHistoryView = value; NotifyPropertyChanged(); }
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

        public bool VideoSync
        {
            get { return _videoSync; }
            set { _videoSync = value; NotifyPropertyChanged(); }
        }


        /// <summary>
        /// Called on Navigate image
        /// </summary>
        /// <param name="imageResult">The image result.</param>
        /// <returns></returns>
        public Task NavigateAsync(IImageResult imageResult)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Called on Navigate video
        /// </summary>
        /// <param name="videoResult">The video result.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task NavigateAsync(IVideoResult videoResult)
        {
            if (IsGenerating)
                await Cancel();

            Reset();
            ResultVideo = null;
            SourceVideo = new VideoInputModel
            {
                Video = videoResult.Video,
                FileName = videoResult.FileName
            };
            IsHistoryView = false;
            UpdateInfo();
        }


        /// <summary>
        /// Generates this Video result.
        /// </summary>
        private async Task Generate()
        {
            IsGenerating = true;
            ResultVideo = null;
            using (CancelationTokenSource = new CancellationTokenSource())
            {
                try
                {
                    var timestamp = Stopwatch.GetTimestamp();
                    if (_inputVideoCached is null)
                        _inputVideoCached = await OnnxVideo.FromFileAsync(_inputVideo.FileName, cancellationToken: CancelationTokenSource.Token);

                    UpdateProgress(0, _inputVideoCached.Frames.Count);
                    var videoResult = await ExecuteFeatureExtractorAsync(_inputVideoCached, CancelationTokenSource.Token);
                    if (videoResult != null)
                    {
                        ResultVideo = await GenerateResultAsync(videoResult, timestamp);
                        VideoResults.Add(ResultVideo);
                        await SyncVideoAsync();
                    }
                }
                catch (OperationCanceledException)
                {
                    Logger.LogInformation("Generate was canceled.");
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error during Generate\n{ex}", ex);
                }
            }
            StopStatistics();
            Reset();
        }


        /// <summary>
        /// Determines whether Generate can execute.
        /// </summary>
        private bool CanExecuteGenerate()
        {
            return !IsGenerating && InputVideo is not null;
        }


        /// <summary>
        /// Cancels this generation.
        /// </summary>
        /// <returns></returns>
        private Task Cancel()
        {
            CancelationTokenSource?.Cancel();
            ClearStatistics();
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
                Value = 200;
            }
            else if (SelectedModel.Id == Utils.InternalExtractorSoftEdge)
            {
                Value = 0;
            }
            UpdateProgress("Loading Pipeline...", true);
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
        /// Unloads the model asynchronous.
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
        /// <returns></returns>
        private Task ClearHistory()
        {
            VideoResults.Clear();
            return Task.CompletedTask;
        }


        /// <summary>
        /// Determines whether ClearHistory can execute.
        /// </summary>
        private bool CanExecuteClearHistory()
        {
            return VideoResults.Count > 0;
        }


        /// <summary>
        /// Saves the video.
        /// </summary>
        /// <param name="model">The model.</param>
        private async Task SaveVideo(FeatureExtractorVideoResultModel model)
        {
            await FileService.SaveAsVideoFile(model);
        }


        /// <summary>
        /// Copies the video to clipboard (filename).
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private Task CopyVideo(FeatureExtractorVideoResultModel result)
        {
            if (result == null)
                return Task.CompletedTask;

            Clipboard.SetFileDropList(new StringCollection
            {
                result.FileName
            });
            return Task.CompletedTask;
        }


        /// <summary>
        /// Previews the video.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private async Task PreviewVideo(FeatureExtractorVideoResultModel result)
        {
            var previewDialog = DialogService.GetDialog<PreviewVideoDialog>();
            await previewDialog.ShowDialogAsync("Video Preview", result);
        }


        /// <summary>
        /// Removes the video from history list.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private Task RemoveVideo(FeatureExtractorVideoResultModel result)
        {
            VideoResults.Remove(result);
            if (result == ResultVideo)
            {
                ResultVideo = null;
                ClearStatistics();
            }
            return Task.CompletedTask;
        }


        /// <summary>
        /// Resets this instance.
        /// </summary>
        private void Reset()
        {
            IsGenerating = false;
            PreviewResult = null;
            PreviewSource = null;
            ClearProgress();
        }


        /// <summary>
        /// Generates the result.
        /// </summary>
        /// <param name="video">The video.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <returns></returns>
        private async Task<FeatureExtractorVideoResultModel> GenerateResultAsync(OnnxVideo video, long timestamp)
        {
            var tempvideoFileName = await FileService.SaveTempVideoFile(video, "VideoFeatureExtractor");
            var videoResult = new FeatureExtractorVideoResultModel
            {
                Video = video,
                FileName = tempvideoFileName,
                Elapsed = Stopwatch.GetElapsedTime(timestamp).TotalSeconds,
                Thumbnail = await video.GetFrame(1).ToBitmapAsync()
            };

            await FileService.AutoSaveVideoFile(videoResult, "VideoFeatureExtractor");
            return videoResult;
        }


        /// <summary>
        /// Updates the display information.
        /// </summary>
        private void UpdateInfo()
        {
            ResultVideo = null;
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
        /// Creates the progress callback.
        /// </summary>
        /// <returns></returns>
        private IProgress<FeatureExtractorProgress> CreateProgressCallback()
        {
            return new Progress<FeatureExtractorProgress>(async (progress) =>
            {
                var preview = await Task.WhenAll(progress.Source.ToBitmapAsync(), progress.Result.ToBitmapAsync());
                if (CancelationTokenSource.IsCancellationRequested)
                    return;

                var frame = Progress.Value + 1;
                PreviewSource = preview[0];
                PreviewResult = preview[1];
                UpdateProgress(frame, _inputVideoCached.Frames.Count, $"Frame: {frame:D2}/{_inputVideoCached.Frames.Count}");
                UpdateStatistics(new DiffusionProgress
                {
                    StepValue = frame,
                    StepMax = _inputVideoCached.Frames.Count,
                    Elapsed = progress.Elapsed
                });
            });
        }


        /// <summary>
        /// Loads the feature extractor.
        /// </summary>
        /// <param name="featureExtractor">The feature extractor.</param>
        protected async Task<FeatureExtractorPipeline> LoadFeatureExtractorAsync()
        {
            if (SelectedModel is null)
                return default;

            UpdateProgress("Loading Pipeline...", true);
            return await _modelCacheService.LoadModelAsync(SelectedModel);
        }


        /// <summary>
        /// Unloads the feature extractors.
        /// </summary>
        protected async Task UnloadFeatureExtractorAsync()
        {
            await _modelCacheService.UnloadModelAsync(SelectedModel);
        }


        /// <summary>
        /// Executes the feature extractor.
        /// </summary>
        /// <param name="inputVideo">The input video.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        protected async Task<OnnxVideo> ExecuteFeatureExtractorAsync(OnnxVideo inputVideo, CancellationToken cancellationToken)
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
            UpdateProgress("Extracting Video Feature...", true);
            var options = new FeatureExtractorOptions(_tileMode, _tileSize, _tileOverlap)
            {
                Value = _value
            };
            return await featureExtractorPipeline.RunAsync(inputVideo, options, _progressCallback, cancellationToken);
        }


        protected Task SyncVideoAsync()
        {
            if (InputVideo == null || ResultVideo == null)
                return Task.CompletedTask;

            if (InputVideo.Video.Duration != ResultVideo.Video.Duration)
                return Task.CompletedTask;

            VideoSync = !VideoSync;
            return Task.CompletedTask;
        }
    }
}