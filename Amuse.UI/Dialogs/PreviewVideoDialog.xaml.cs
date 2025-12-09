using Amuse.UI.Commands;
using Amuse.UI.Models;
using Amuse.UI.Services;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Amuse.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for PreviewVideoDialog.xaml
    /// </summary>
    public partial class PreviewVideoDialog : BaseDialog
    {
        private readonly IFileService _fileService;

        private bool _isActualSize;
        private int _videoHeight;
        private int _videoWidth;
        private Stretch _videoStrech;
        private IVideoResult _videoResult;
        private ScrollBarVisibility _scrollBarVisibility;
        private readonly int _previewMaximum = 680;

        public PreviewVideoDialog(IFileService fileService)
        {
            _fileService = fileService;
            CopyVideoCommand = new AsyncRelayCommand(CopyVideo);
            SaveVideoCommand = new AsyncRelayCommand(SaveVideo);
            InitializeComponent();
        }

        public AsyncRelayCommand CopyVideoCommand { get; }
        public AsyncRelayCommand SaveVideoCommand { get; }

        public IVideoResult VideoResult
        {
            get { return _videoResult; }
            set { _videoResult = value; NotifyPropertyChanged(); }
        }
        public ScrollBarVisibility ScrollBarVisibility
        {
            get { return _scrollBarVisibility; }
            set { _scrollBarVisibility = value; NotifyPropertyChanged(); }
        }

        public bool IsActualSize
        {
            get { return _isActualSize; }
            set { _isActualSize = value; NotifyPropertyChanged(); UpdateScrollBar(); }
        }

        public Stretch VideoStrech
        {
            get { return _videoStrech; }
            set { _videoStrech = value; NotifyPropertyChanged(); }
        }

        public int VideoHeight
        {
            get { return _videoHeight; }
            set { _videoHeight = value; NotifyPropertyChanged(); }
        }

        public int VideoWidth
        {
            get { return _videoWidth; }
            set { _videoWidth = value; NotifyPropertyChanged(); }
        }


        private void UpdateScrollBar()
        {
            VideoStrech = _isActualSize
                ? Stretch.None
                : Stretch.Uniform;
         
            VideoWidth = _isActualSize ? VideoResult.Video.Width : _previewMaximum;
            VideoHeight = _isActualSize ? VideoResult.Video.Height : _previewMaximum;
        }


        public Task<bool> ShowDialogAsync(string title, IVideoResult videoResult)
        {
            Title = title;
            VideoResult = videoResult;
            IsActualSize = false;
            return base.ShowDialogAsync();
        }


        private Task CopyVideo()
        {
            Clipboard.SetFileDropList(new StringCollection
            {
                _videoResult.FileName
            });
            return Task.CompletedTask;
        }


        private async Task SaveVideo()
        {
            await _fileService.SaveAsVideoFile(_videoResult);
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


        private void MediaElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not MediaElement mediaElement)
                return;

            mediaElement.LoadedBehavior = mediaElement.LoadedBehavior == MediaState.Pause
                 ? MediaState.Play
                 : MediaState.Pause;
        }
    }
}
