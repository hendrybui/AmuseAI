using Amuse.UI.Commands;
using Amuse.UI.Models;
using Amuse.UI.Services;
using OnnxStack.Core.Image;
using OnnxStack.Core.Video;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.IO;

namespace Amuse.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for CreateVideoDialog.xaml
    /// </summary>
    public partial class CreateVideoDialog : BaseDialog
    {

        private readonly AmuseSettings _settings;
        private readonly IFileService _fileService;
        private readonly IDialogService _dialogService;

        private List<OnnxImage> _videoFrames;
        private int _videoWidth;
        private int _videoHeight;
        private int _videoSeconds;
        private float _videoFrameRate;
        private string _videoFileName;
        private bool _hasResult;

        public CreateVideoDialog(AmuseSettings settings, IFileService fileService, IDialogService dialogService)
        {
            GenerateCommand = new AsyncRelayCommand(GenerateAsync);
            InitializeComponent();
            _settings = settings;
            _fileService = fileService;
            _dialogService = dialogService;
        }

        public AsyncRelayCommand GenerateCommand { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }

        public List<OnnxImage> VideoFrames
        {
            get { return _videoFrames; }
            set { _videoFrames = value; NotifyPropertyChanged(); }
        }

        public int VideoWidth
        {
            get { return _videoWidth; }
            set { _videoWidth = value; NotifyPropertyChanged(); }
        }

        public int VideoHeight
        {
            get { return _videoHeight; }
            set { _videoHeight = value; NotifyPropertyChanged(); }
        }


        public int VideoSeconds
        {
            get { return _videoSeconds; }
            set
            {
                _videoSeconds = value;
                _videoFrameRate = (float)_videoFrames.Count / _videoSeconds;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(VideoFrameRate));
            }
        }


        public float VideoFrameRate
        {
            get { return _videoFrameRate; }
            set
            {
                _videoFrameRate = value;
                _videoSeconds = (int)(_videoFrames.Count / _videoFrameRate);
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(VideoSeconds));
            }
        }



        public string VideoFileName
        {
            get { return _videoFileName; }
            set { _videoFileName = value; NotifyPropertyChanged(); }
        }



        public bool HasResult
        {
            get { return _hasResult; }
            set { _hasResult = value; NotifyPropertyChanged(); }
        }


        public Task<bool> ShowDialogAsync(IEnumerable<OnnxImage> images)
        {
            VideoFrames = images.ToList();
            VideoWidth = _videoFrames[0].Width;
            VideoHeight = _videoFrames[0].Height;
            VideoSeconds = 5;
            return base.ShowDialogAsync();
        }


        private async Task GenerateAsync()
        {
            HasResult = false;
            VideoFileName = string.Empty;
            var tempVideoFile = _fileService.GetTempFileName("mp4");
            using (CancellationTokenSource = new CancellationTokenSource())
            {
                try
                {
                    await VideoHelper.WriteVideoFramesAsync(tempVideoFile, _videoFrames, _videoFrameRate, _videoWidth, _videoHeight, cancellationToken: CancellationTokenSource.Token);
                    VideoFileName = tempVideoFile;
                    HasResult = true;
                }
                catch (OperationCanceledException)
                {

                }
                catch (Exception)
                {

                }
            }
        }

        protected override async Task SaveAsync()
        {
            var videoOutputFile = await _dialogService.SaveFileDialogAsync("Save Video", "VideoHistory.mp4", _settings.DirectoryVideo, "mp4 files (*.mp4)|*.mp4", "mp4");
            if (string.IsNullOrEmpty(videoOutputFile))
                return;

            File.Move(_videoFileName, videoOutputFile, true);
            await base.SaveAsync();
        }


        protected override async Task CancelAsync()
        {
            await CancelGenerate();
            await base.CancelAsync();
        }


        protected override async Task WindowClose()
        {
            await CancelGenerate();
            await base.WindowClose();
        }


        private async Task CancelGenerate()
        {
            try
            {
                HasResult = false;
                await CancellationTokenSource.CancelAsync();
            }
            catch (System.Exception)
            {

            }
        }


        /// <summary>
        /// Handles the Loaded event of the MediaElement control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MediaElement_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not MediaElement mediaElement)
                return;

            mediaElement.LoadedBehavior = MediaState.Play;
        }


        /// <summary>
        /// Handles the MediaEnded event of the MediaElement control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (sender is not MediaElement mediaElement)
                return;

            mediaElement.Position = TimeSpan.FromMilliseconds(1);
        }


        /// <summary>
        /// Handles the MouseDown event of the MediaElement control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void MediaElement_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is not MediaElement mediaElement)
                return;

            mediaElement.LoadedBehavior = mediaElement.LoadedBehavior == MediaState.Pause
                ? MediaState.Play
                : MediaState.Pause;
        }
    }
}
