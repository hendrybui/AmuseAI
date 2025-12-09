using Amuse.UI.Commands;
using Amuse.UI.Helpers;
using Amuse.UI.Models;
using Amuse.UI.Services;
using OnnxStack.Core;
using OnnxStack.Core.Video;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Amuse.UI.UserControls
{
    public partial class VideoInputControl : UserControl, INotifyPropertyChanged
    {
        private readonly IFileService _fileService;

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoInputControl" /> class.
        /// </summary>
        public VideoInputControl()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _fileService = App.GetService<IFileService>();
            }

            LoadVideoCommand = new AsyncRelayCommand(LoadVideo);
            ClearVideoCommand = new AsyncRelayCommand(ClearVideo, CanClearVideo);
            CopyVideoCommand = new AsyncRelayCommand(CopyVideo, CanCopyVideo);
            PasteVideoCommand = new AsyncRelayCommand(PasteVideo);
            SaveVideoCommand = new AsyncRelayCommand(SaveVideo, CanSaveVideo);
            InitializeComponent();
        }

        public AsyncRelayCommand LoadVideoCommand { get; }
        public AsyncRelayCommand ClearVideoCommand { get; }
        public AsyncRelayCommand CopyVideoCommand { get; }
        public AsyncRelayCommand PasteVideoCommand { get; }
        public AsyncRelayCommand SaveVideoCommand { get; }

        public static readonly DependencyProperty SourceVideoProperty =
            DependencyProperty.Register(nameof(SourceVideo), typeof(VideoInputModel), typeof(VideoInputControl), new PropertyMetadata(OnSourceChanged));

        public static readonly DependencyProperty VideoWidthProperty =
            DependencyProperty.Register(nameof(VideoWidth), typeof(int), typeof(VideoInputControl), new PropertyMetadata(512, OnSizeChanged));

        public static readonly DependencyProperty VideoHeightProperty =
            DependencyProperty.Register(nameof(VideoHeight), typeof(int), typeof(VideoInputControl), new PropertyMetadata(512, OnSizeChanged));

        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register(nameof(Result), typeof(VideoInputModel), typeof(VideoInputControl));

        public static readonly DependencyProperty PreviewImageProperty =
            DependencyProperty.Register(nameof(PreviewImage), typeof(BitmapSource), typeof(VideoInputControl));

        public static readonly DependencyProperty ToolbarVisibilityProperty =
            DependencyProperty.Register(nameof(ToolbarVisibility), typeof(Visibility), typeof(VideoInputControl));

        public static readonly DependencyProperty VideoSyncProperty =
            DependencyProperty.Register(nameof(VideoSync), typeof(bool), typeof(VideoInputControl));

        public VideoInputModel SourceVideo
        {
            get { return (VideoInputModel)GetValue(SourceVideoProperty); }
            set { SetValue(SourceVideoProperty, value); }
        }

        public int VideoWidth
        {
            get { return (int)GetValue(VideoWidthProperty); }
            set { SetValue(VideoWidthProperty, value); }
        }

        public int VideoHeight
        {
            get { return (int)GetValue(VideoHeightProperty); }
            set { SetValue(VideoHeightProperty, value); }
        }

        public VideoInputModel Result
        {
            get { return (VideoInputModel)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }

        public BitmapSource PreviewImage
        {
            get { return (BitmapSource)GetValue(PreviewImageProperty); }
            set { SetValue(PreviewImageProperty, value); }
        }

        public Visibility ToolbarVisibility
        {
            get { return (Visibility)GetValue(ToolbarVisibilityProperty); }
            set { SetValue(ToolbarVisibilityProperty, value); }
        }

        public bool VideoSync
        {
            get { return (bool)GetValue(VideoSyncProperty); }
            set { SetValue(VideoSyncProperty, value); }
        }


        /// <summary>
        /// Loads the image.
        /// </summary>
        /// <returns></returns>
        private async Task LoadVideo()
        {
            var videoResult = await _fileService.OpenVideoFile();
            if (videoResult is null)
                return;

            SetCurrentValue(SourceVideoProperty, videoResult);
        }


        /// <summary>
        /// Clears the image.
        /// </summary>
        /// <returns></returns>
        private Task ClearVideo()
        {
            SetCurrentValue(SourceVideoProperty, null);
            Result = null;
            //   _progressTimer.Stop();
            return Task.CompletedTask;
        }


        /// <summary>
        /// Determines whether this instance can clear video.
        /// </summary>
        private bool CanClearVideo()
        {
            return Result?.Video != null;
        }


        /// <summary>
        /// Copies the video.
        /// </summary>
        /// <returns></returns>
        private Task CopyVideo()
        {
            Clipboard.SetFileDropList(new StringCollection
            {
                Result.FileName
            });
            return Task.CompletedTask;
        }


        /// <summary>
        /// Determines whether this instance can copy video.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can copy video; otherwise, <c>false</c>.
        /// </returns>
        private bool CanCopyVideo()
        {
            return Result?.Video != null;
        }


        /// <summary>
        /// Pastes the video.
        /// </summary>
        private async Task PasteVideo()
        {
            if (Clipboard.ContainsFileDropList())
            {
                var videoFile = Clipboard.GetFileDropList()
                    .OfType<string>()
                    .FirstOrDefault();
                await LoadFromFile(videoFile);
            }
        }


        /// <summary>
        /// Saves the video.
        /// </summary>
        private async Task SaveVideo()
        {
            await _fileService.SaveAsVideoFile(Result.Video);
        }


        /// <summary>
        /// Determines whether this instance can save video.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can save video; otherwise, <c>false</c>.
        /// </returns>
        private bool CanSaveVideo()
        {
            return Result?.Video != null;
        }


        /// <summary>
        /// Loads a video from file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        private async Task LoadFromFile(string fileName)
        {
            var video = await OnnxVideo.FromFileAsync(fileName);
            if (video == null)
                return;

            var videoResult = new VideoInputModel
            {
                FileName = fileName,
                Video = video
            };
            SetCurrentValue(SourceVideoProperty, videoResult);
        }


        private Task Refresh()
        {
            Result = new VideoInputModel
            {
                Video = SourceVideo.Video,
                FileName = SourceVideo.FileName,
            };
            return Task.CompletedTask;
        }


        private static async void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VideoInputControl control && control.SourceVideo is not null)
            {
                await control.Refresh();
            }
        }


        private static async void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VideoInputControl control && control.SourceVideo is not null)
            {
                await control.Refresh();
            }
        }


        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseEnter" /> attached event is raised on this element. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (App.Current.MainWindow.IsActive)
                Focus();
            base.OnMouseEnter(e);
        }


        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseLeave" /> attached event is raised on this element. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (App.Current.MainWindow.IsActive)
                Keyboard.ClearFocus();
            base.OnMouseLeave(e);
        }


        /// <summary>
        /// Handles the PreviewMouseLeftButtonUp event of the Control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private async void Control_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                await LoadVideo();
            }
        }


        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.DragDrop.PreviewDrop" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.DragEventArgs" /> that contains the event data.</param>
        protected override async void OnPreviewDrop(DragEventArgs e)
        {
            var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (!fileNames.IsNullOrEmpty())
                await LoadFromFile(fileNames.FirstOrDefault());
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
