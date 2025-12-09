using Amuse.UI.Commands;
using Amuse.UI.Dialogs;
using Amuse.UI.Models;
using Amuse.UI.Models.StableDiffusion;
using Microsoft.Extensions.Logging;
using OnnxStack.Core.Video;
using OnnxStack.FeatureExtractor.Common;
using OnnxStack.ImageUpscaler.Common;
using OnnxStack.StableDiffusion.Common;
using OnnxStack.StableDiffusion.Config;
using OnnxStack.StableDiffusion.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Amuse.UI.Views
{
    public class StableDiffusionVideoViewBase : StableDiffusionViewBase, INavigatable
    {
        private PromptOptionsModel _promptOptionsModel;
        private SchedulerOptionsModel _schedulerOptions;
        private BatchOptionsModel _batchOptions;
        private VideoResultModel _resultVideo;
        private int _selectedTabIndex;
        private bool _isHistoryView;
        private bool _realtimeHasChanged;
        private VideoInputModel _inputVideo;
        private OnnxVideo _inputVideoCached;
        private OnnxVideo _inputControlVideoCached;
        private BitmapSource _previewSource;
        private BitmapSource _previewResult;
        private bool _videoSync;
        private IProgress<FeatureExtractorProgress> _extractorProgressCallback;
        private VideoInputModel _sourceVideo;

        /// <summary>
        /// Initializes a new instance of the <see cref="StableDiffusionVideoViewBase"/> class.
        /// </summary>
        public StableDiffusionVideoViewBase()
        {
            _extractorProgressCallback = CreateExtractorProgressCallback();
            CancelCommand = new AsyncRelayCommand(Cancel, CanExecuteCancel);
            GenerateCommand = new AsyncRelayCommand(Generate, CanExecuteGenerate);
            SaveHistoryCommand = new AsyncRelayCommand(SaveHistory, CanExecuteSaveHistory);
            ClearHistoryCommand = new AsyncRelayCommand(ClearHistory, CanExecuteClearHistory);
            SaveVideoCommand = new AsyncRelayCommand<VideoResultModel>(SaveVideo);
            CopyVideoCommand = new AsyncRelayCommand<VideoResultModel>(CopyVideo);
            RemoveVideoCommand = new AsyncRelayCommand<VideoResultModel>(RemoveVideo);
            PreviewVideoCommand = new AsyncRelayCommand<VideoResultModel>(PreviewVideo);
            UpdateSeedCommand = new AsyncRelayCommand<int>(UpdateSeed);
            PromptOptions = new PromptOptionsModel();
            SchedulerOptions = new SchedulerOptionsModel();
            BatchOptions = new BatchOptionsModel();
            VideoResults = new ObservableCollection<VideoResultModel>();
        }

        public AsyncRelayCommand CancelCommand { get; }
        public AsyncRelayCommand GenerateCommand { get; }
        public AsyncRelayCommand SaveHistoryCommand { get; }
        public AsyncRelayCommand ClearHistoryCommand { get; }
        public AsyncRelayCommand<VideoResultModel> SaveVideoCommand { get; }
        public AsyncRelayCommand<VideoResultModel> CopyVideoCommand { get; set; }
        public AsyncRelayCommand<VideoResultModel> RemoveVideoCommand { get; set; }
        public AsyncRelayCommand<VideoResultModel> PreviewVideoCommand { get; set; }
        public AsyncRelayCommand<int> UpdateSeedCommand { get; set; }
        public List<DiffuserType> SupportedDiffusers { get; init; }
        public ObservableCollection<VideoResultModel> VideoResults { get; }

        public PromptOptionsModel PromptOptions
        {
            get { return _promptOptionsModel; }
            set { _promptOptionsModel = value; NotifyPropertyChanged(); }
        }

        public SchedulerOptionsModel SchedulerOptions
        {
            get { return _schedulerOptions; }
            set { _schedulerOptions = value; NotifyPropertyChanged(); }
        }

        public BatchOptionsModel BatchOptions
        {
            get { return _batchOptions; }
            set { _batchOptions = value; NotifyPropertyChanged(); }
        }

        public VideoResultModel ResultVideo
        {
            get { return _resultVideo; }
            set { _resultVideo = value; NotifyPropertyChanged(); }
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

        public bool RealtimeHasChanged
        {
            get { return _realtimeHasChanged; }
            set { _realtimeHasChanged = value; NotifyPropertyChanged(); }
        }

        public VideoInputModel InputVideo
        {
            get { return _inputVideo; }
            set
            {
                _inputVideo = value;
                _inputVideoCached = null;
                _inputControlVideoCached = null;
                PromptOptions.VideoInputFPS = _inputVideo?.Video?.FrameRate ?? 15;
                PromptOptions.VideoOutputFPS = PromptOptions.VideoInputFPS;
                NotifyPropertyChanged();
            }
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

        public bool VideoSync
        {
            get { return _videoSync; }
            set { _videoSync = value; NotifyPropertyChanged(); }
        }

        public VideoInputModel SourceVideo
        {
            get { return _sourceVideo; }
            set { _sourceVideo = value; NotifyPropertyChanged(); }
        }


        /// <summary>
        /// Called on Navigate
        /// </summary>
        /// <param name="imageResult">The image result.</param>
        /// <returns></returns>
        public virtual Task NavigateAsync(IImageResult imageResult)
        {
            throw new NotImplementedException();
        }


        public async Task NavigateAsync(IVideoResult videoResult)
        {
            if (IsGenerating)
                await Cancel();

            Reset();
            ResultVideo = null;

            var stableDiffustionResult = videoResult as VideoResultModel;
            if (stableDiffustionResult != null)
            {
                //if (stableDiffustionResult.Model.ModelSet.Diffusers.Contains(DiffuserType.ImageToImage))
                //{
                //    SelectedBaseModel = stableDiffustionResult.Model;
                //}
                PromptOptions = PromptOptionsModel.FromGenerateVideoOptions(stableDiffustionResult.PromptOptions);
                SchedulerOptions = SchedulerOptionsModel.FromSchedulerOptions(stableDiffustionResult.SchedulerOptions);
            }

            SourceVideo = new VideoInputModel
            {
                FileName = videoResult.FileName,
                Video = videoResult.Video,
            };

            SelectedTabIndex = 0;
            IsHistoryView = false;
        }


        /// <summary>
        /// Generates this image result.
        /// </summary>
        private async Task Generate()
        {
            try
            {
                IsGenerating = true;
                ResultVideo = null;
                VideoResults.Add(new VideoResultModel());
                using (CancelationTokenSource = new CancellationTokenSource())
                {
                    if (await ModeratorService.ContainsExplicitContentAsync(PromptOptions.Prompt))
                        throw new OperationCanceledException();

                    if (BatchOptions.IsAutomationEnabled)
                    {
                        await foreach (var resultVideo in GenerateVideoBatchAsync(CancelationTokenSource.Token))
                        {
                            if (resultVideo is null)
                                continue;

                            ResultVideo = resultVideo;
                            if (BatchOptions.DisableHistory)
                                continue;

                            if (VideoResults.Count > Settings.HistoryMaxItems)
                                VideoResults.RemoveAt(0);
                            VideoResults.Remove(x => x.Video is null);
                            VideoResults.Add(resultVideo);
                            VideoResults.Add(new VideoResultModel());

                            Statistics.Reset();
                            await SyncVideoAsync();
                        }
                    }
                    else
                    {
                        var resultImage = await GenerateVideoResultAsync(CancelationTokenSource.Token);
                        ResultVideo = resultImage;

                        if (VideoResults.Count > Settings.HistoryMaxItems)
                            VideoResults.RemoveAt(0);
                        VideoResults.Remove(x => x.Video is null);
                        VideoResults.Add(resultImage);

                        await SyncVideoAsync();
                    }

                    StopStatistics();
                }
            }
            catch (OperationCanceledException)
            {
                Logger.LogInformation($"Generate was canceled.");
            }
            catch (Exception ex)
            {
                Logger.LogError("Error during Generate\n{ex}", ex);
                await App.UIInvokeAsync(() => DialogService.ShowErrorMessageAsync("Generate Error", ex.Message));
                await UnloadPipelineAsync();
            }
            Reset();
        }


        /// <summary>
        /// Determines whether this instance can execute Generate.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can execute Generate; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool CanExecuteGenerate()
        {
            return !IsGenerating
               && CurrentPipeline?.IsLoaded == true
               && !CanLoadPipeline()
               && InputVideo is not null;
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
        /// Clears the history.
        /// </summary>
        /// <returns></returns>
        private async Task ClearHistory()
        {
            await FileService.DeleteTempVideoFile(VideoResults);
            VideoResults.Clear();
            ResultVideo = null;
        }


        /// <summary>
        /// Determines whether this instance can execute ClearHistory.
        /// </summary>
        private bool CanExecuteClearHistory()
        {
            return VideoResults.Count > 0;
        }


        /// <summary>
        /// Saves the history.
        /// </summary>
        private async Task SaveHistory()
        {
            var createVideoDialog = DialogService.GetDialog<CreateVideoDialog>();
            await createVideoDialog.ShowDialogAsync(VideoResults.SelectMany(x => x.Video.Frames));
        }


        /// <summary>
        /// Determines whether this instance can execute SaveHistory.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can execute save history]; otherwise, <c>false</c>.
        /// </returns>
        private bool CanExecuteSaveHistory()
        {
            return VideoResults.Count > 0;
        }


        /// <summary>
        /// Resets this instance.
        /// </summary>
        private void Reset()
        {
            StopStatistics();
            PreviewSource = null;
            PreviewResult = null;
            IsGenerating = false;
            VideoResults.Remove(x => x.Video is null);
            BatchOptions.StepValue = 0;
            BatchOptions.BatchValue = 0;
            ClearProgress();
        }


        /// <summary>
        /// Saves the video.
        /// </summary>
        /// <param name="result">The result.</param>
        private async Task SaveVideo(VideoResultModel result)
        {
            if (result == null)
                return;

            await FileService.SaveAsVideoFile(result);
        }


        /// <summary>
        /// Copies the video.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private Task CopyVideo(VideoResultModel result)
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
        /// Updates the seed.
        /// </summary>
        /// <param name="seedValue">The seed value.</param>
        /// <returns></returns>
        private Task UpdateSeed(int seedValue)
        {
            if (seedValue <= 0)
                seedValue = Random.Shared.Next();

            SchedulerOptions.Seed = seedValue;
            return Task.CompletedTask;
        }


        /// <summary>
        /// Previews the video.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private async Task PreviewVideo(VideoResultModel result)
        {
            var previewDialog = DialogService.GetDialog<PreviewVideoDialog>();
            await previewDialog.ShowDialogAsync("Video Preview", result);
        }

        /// <summary>
        /// Removes the video.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private async Task RemoveVideo(VideoResultModel result)
        {
            if (result == null)
                return;

            await FileService.DeleteTempVideoFile(result);
            VideoResults.Remove(result);
            if (result == ResultVideo)
            {
                ResultVideo = null;
                ClearStatistics();
            }
        }


        /// <summary>
        /// Synchronizes the InputVideo and ResultVideo.
        /// </summary>
        /// <returns>Task.</returns>
        protected Task SyncVideoAsync()
        {
            if (InputVideo == null || ResultVideo == null)
                return Task.CompletedTask;

            if (InputVideo.Video.Duration != ResultVideo.Video.Duration)
                return Task.CompletedTask;

            VideoSync = !VideoSync;
            return Task.CompletedTask;
        }


        /// <summary>
        /// Gets the prompt options
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        protected virtual async Task<GenerateOptions> GetGenerateOptionsAsync(CancellationToken cancellationToken)
        {
            var inputFPS = PromptOptions.VideoInputFPS;
            var generateOptions = PromptOptionsModel.ToGenerateVideoOptions(PromptOptions, SchedulerOptions, MemoryInfo);
            if (_inputVideoCached == null || _inputVideoCached.FrameRate != inputFPS || _inputVideoCached.Width != SchedulerOptions.Width || _inputVideoCached.Height != SchedulerOptions.Height)
            {
                UpdateProgress("Processing Input Video...");
                int? videoWidth = _inputVideo.Video.Height > _inputVideo.Video.Width ? SelectedBaseModel.ModelSet.SampleSize : null;
                int? videoHeight = _inputVideo.Video.Width > _inputVideo.Video.Height ? SelectedBaseModel.ModelSet.SampleSize : null;
                _inputControlVideoCached = null;
                _inputVideoCached = await OnnxVideo.FromFileAsync(_inputVideo.FileName, inputFPS, videoWidth, videoHeight, CancelationTokenSource.Token);
                _inputVideoCached.Resize(SchedulerOptions.Height, SchedulerOptions.Width);
            }

            if (IsControlNetEnabled)
            {
                generateOptions.ControlNet = await LoadControlNetAsync();
                var controlNetDiffuserType = SchedulerOptions.Strength >= 1 && CurrentPipeline.ModelType != ModelType.Instruct
                        ? DiffuserType.ControlNet
                        : DiffuserType.ControlNetImage;

                if (_inputControlVideoCached == null)
                    _inputControlVideoCached = await ExecuteFeatureExtractorAsync(_inputVideoCached, cancellationToken);

                generateOptions.InputVideo = _inputVideoCached;
                generateOptions.InputContolVideo = _inputControlVideoCached;
                generateOptions.Diffuser = controlNetDiffuserType;
                return generateOptions;
            }

            if (_inputVideoCached != null)
                _inputVideoCached = await ExecuteFeatureExtractorAsync(_inputVideoCached, cancellationToken);

            generateOptions.Diffuser = DiffuserType.ImageToImage;
            generateOptions.InputVideo = _inputVideoCached;
            return generateOptions;
        }


        /// <summary>
        /// Resets the control image cache.
        /// </summary>
        protected virtual void ResetControlVideoCache()
        {
            _inputControlVideoCached = null;
        }


        /// <summary>
        /// Gets the batch options.
        /// </summary>
        /// <returns></returns>
        protected virtual GenerateBatchOptions GetBatchOptions(GenerateOptions generateOptions)
        {
            return new GenerateBatchOptions(generateOptions)
            {
                BatchType = BatchOptions.BatchType,
                ValueTo = BatchOptions.ValueTo,
                Increment = BatchOptions.Increment,
                ValueFrom = BatchOptions.ValueFrom
            };
        }


        /// <summary>
        /// Generates the result.
        /// </summary>
        /// <param name="imageBytes">The image bytes.</param>
        /// <param name="promptOptions">The prompt options.</param>
        /// <param name="schedulerOptions">The scheduler options.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <returns></returns>
        protected async Task<VideoResultModel> GenerateResultAsync(OnnxVideo video, GenerateOptions generateOptions, long timestamp)
        {
            video.NormalizeBrightness();
            return await App.UIInvokeAsync(async () =>
            {
                var tempvideoFileName = await FileService.SaveTempVideoFile(video, "VideoToVideo");
                var videoResult = new VideoResultModel
                {
                    Video = video,
                    ModelName = SelectedBaseModel.Name,
                    FileName = tempvideoFileName,
                    PromptOptions = generateOptions,
                    SchedulerOptions = generateOptions.SchedulerOptions,
                    DiffuserType = generateOptions.Diffuser,
                    PipelineType = SelectedBaseModel.ModelSet.PipelineType,
                    Elapsed = Stopwatch.GetElapsedTime(timestamp).TotalSeconds,
                    Thumbnail = await video.GetFrame(1).ToBitmapAsync()
                };

                await FileService.AutoSaveVideoFile(videoResult, "VideoToVideo");
                return videoResult;
            });
        }


        protected override async Task UpdateProgress(DiffusionProgress progress)
        {
            UpdateStatistics(progress);
            if (progress.BatchTensor is not null)
            {
                PreviewResult = await progress.BatchTensor.ToBitmapAsync();
                PreviewSource = await _inputVideoCached.Frames[progress.BatchValue - 1].ToBitmapAsync();
            }

            if (progress.StepMax > 0)
            {
                if (progress.BatchMax != _inputVideoCached.Frames.Count)
                {
                    if (BatchOptions.BatchValue != progress.BatchValue - 1)
                    {
                        BatchOptions.StepValue = 0;
                        BatchOptions.BatchValue = progress.BatchValue - 1;
                    }

                    BatchOptions.BatchsValue = progress.BatchMax;
                    BatchOptions.StepsValue = _inputVideoCached.Frames.Count * progress.StepMax;
                    BatchOptions.StepValue++;
                }
                else
                {
                    var max = _inputVideoCached.Frames.Count * progress.StepMax;
                    var value = ((progress.BatchValue - 1) * progress.StepMax) + progress.StepValue;
                    var progressText = $"Frame: {progress.BatchValue:D2}/{_inputVideoCached.Frames.Count}  |  Step: {progress.StepValue:D2}/{progress.StepMax:D2}";
                    UpdateProgress(value, max, progressText);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(progress.Message))
                    UpdateProgress(progress.Message);
            }
        }


        /// <summary>
        /// Executes the stable diffusion process.
        /// </summary>
        /// <param name="promptOptions">The prompt options.</param>
        /// <param name="schedulerOptions">The scheduler options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        protected async Task<OnnxVideo> ExecuteStableDiffusionAsync(GenerateOptions options, CancellationToken cancellationToken)
        {
            StartStatistics();
            var stableDiffusionPipeline = await LoadBaseModelAsync();
            return await stableDiffusionPipeline.GenerateVideoAsync(options, ProgressCallback, cancellationToken);
        }


        /// <summary>
        /// Executes the upscaler.
        /// </summary>
        /// <param name="inputVideo">The input video.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        protected async Task<OnnxVideo> ExecuteUpscalerAsync(OnnxVideo inputVideo, CancellationToken cancellationToken)
        {
            if (CurrentPipeline.UpscaleModel is not null)
            {
                var upscalePipeline = await LoadUpscalerAsync();
                UpdateProgress("Upscaling Video...", true);
                var tileMode = CurrentPipeline.UpscaleModel.ModelSet.TileMode;
                var tileSize = CurrentPipeline.UpscaleModel.ModelSet.TileSize;
                var tileOverlap = CurrentPipeline.UpscaleModel.ModelSet.TileOverlap;
                var isLowMemory = MemoryInfo.IsLowMemoryPipelineEnabled;
                var options = new UpscaleOptions(tileMode, tileSize, tileOverlap, isLowMemory);
                var result = await upscalePipeline.RunAsync(inputVideo, options, cancellationToken: cancellationToken);
                return result;
            }
            return inputVideo;
        }


        /// <summary>
        /// Executes the feature extractor.
        /// </summary>
        /// <param name="inputVideo">The input video.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        protected async Task<OnnxVideo> ExecuteFeatureExtractorAsync(OnnxVideo inputVideo, CancellationToken cancellationToken)
        {
            if (CurrentPipeline.FeatureExtractorModel is not null)
            {
                var featureExtractorPipeline = await LoadFeatureExtractorAsync();
                UpdateProgress("Extracting Video Feature...", true);
                var options = new FeatureExtractorOptions(isLowMemoryEnabled: MemoryInfo.IsLowMemoryPipelineEnabled);
                var result = await featureExtractorPipeline.RunAsync(inputVideo, options, _extractorProgressCallback, cancellationToken: cancellationToken);
                return result;
            }
            return inputVideo;
        }


        /// <summary>
        /// Executes the ContentFilter.
        /// </summary>
        /// <param name="inputVideo">The input video.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        protected virtual async Task<OnnxVideo> ExecuteContentFilterAsync(OnnxVideo inputVideo, CancellationToken cancellationToken)
        {
            if (CurrentPipeline.ContentFilterModel is not null && ModeratorService.IsContentFilterEnabled)
            {
                var contentFilterPipeline = await LoadContentFilterAsync();
                UpdateProgress("Running Content Filter...", true);
                var isLowMemoryEnabled = MemoryInfo.IsLowMemoryPipelineEnabled;
                var result = await Task.Run(() => contentFilterPipeline.RunAsync(inputVideo, 0.030f, isLowMemoryEnabled, cancellationToken: cancellationToken));
                return result;
            }
            return inputVideo;
        }


        protected virtual async Task<VideoResultModel> GenerateVideoResultAsync(CancellationToken cancellationToken)
        {
            var timestamp = Stopwatch.GetTimestamp();
            var generateOptions = await GetGenerateOptionsAsync(cancellationToken);
            var result = await ExecuteStableDiffusionAsync(generateOptions, cancellationToken);
            result = await ExecuteContentFilterAsync(result, cancellationToken);
            result = await ExecuteUpscalerAsync(result, cancellationToken);
            return await GenerateResultAsync(result, generateOptions, timestamp);
        }


        protected virtual async IAsyncEnumerable<VideoResultModel> GenerateVideoBatchAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var timestamp = Stopwatch.GetTimestamp();
            var defaultGenerateOptions = await GetGenerateOptionsAsync(cancellationToken);
            var batchOptions = GetBatchOptions(defaultGenerateOptions);
            var batchGenerateOptions = GenerateBatch(batchOptions, defaultGenerateOptions.SchedulerOptions);
            BatchOptions.BatchValue = 0;
            BatchOptions.BatchsValue = batchGenerateOptions.Count;
            foreach (var batchGenerateOption in batchGenerateOptions)
            {
                var generateOptions = defaultGenerateOptions with
                {
                    SchedulerOptions = batchGenerateOption
                };
                var result = await ExecuteStableDiffusionAsync(generateOptions, cancellationToken);
                result = await ExecuteContentFilterAsync(result, cancellationToken);
                result = await ExecuteUpscalerAsync(result, cancellationToken);
                yield return await GenerateResultAsync(result, generateOptions, timestamp);
                BatchOptions.BatchValue++;
            }
        }


        private List<SchedulerOptions> GenerateBatch(GenerateBatchOptions batchOptions, SchedulerOptions schedulerOptions)
        {
            var seed = schedulerOptions.Seed == 0 ? Random.Shared.Next() : schedulerOptions.Seed;
            if (batchOptions.BatchType == BatchOptionType.Seed)
            {

                if (batchOptions.ValueTo <= 1)
                    return [schedulerOptions with { Seed = seed }];

                var random = new Random(seed);
                return Enumerable.Range(0, Math.Max(1, (int)batchOptions.ValueTo - 1))
                    .Select(x => random.Next())
                    .Prepend(seed)
                    .Select(x => schedulerOptions with { Seed = x })
                    .ToList();
            }

            if (batchOptions.BatchType == BatchOptionType.Scheduler)
            {
                return CurrentPipeline.SupportedSchedulers
                  .Select(x => schedulerOptions with { SchedulerType = x })
                  .ToList();
            }

            var totalIncrements = (int)Math.Max(1, (batchOptions.ValueTo - batchOptions.ValueFrom) / batchOptions.Increment) + 1;
            if (batchOptions.BatchType == BatchOptionType.Step)
            {
                return Enumerable.Range(0, totalIncrements)
                   .Select(x => schedulerOptions with { Seed = seed, InferenceSteps = (int)(batchOptions.ValueFrom + (batchOptions.Increment * x)) })
                   .ToList();
            }

            if (batchOptions.BatchType == BatchOptionType.Guidance)
            {
                return Enumerable.Range(0, totalIncrements)
                  .Select(x => schedulerOptions with { Seed = seed, GuidanceScale = batchOptions.ValueFrom + (batchOptions.Increment * x) })
                  .ToList();
            }

            return new List<SchedulerOptions>();
        }


        private IProgress<FeatureExtractorProgress> CreateExtractorProgressCallback()
        {
            return new Progress<FeatureExtractorProgress>((progress) =>
            {
                if (CancelationTokenSource.IsCancellationRequested)
                    return;

                var value = Progress.Value + 1;
                UpdateProgress(value, _inputVideoCached.Frames.Count, $"Extracting Feature: {value:D2}/{_inputVideoCached.Frames.Count}");
            });
        }
    }
}