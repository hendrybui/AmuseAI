using Amuse.UI.Enums;
using Amuse.UI.Helpers;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Amuse.UI.UserControls
{
    public partial class VideoElement : UserControl, INotifyPropertyChanged
    {
        private readonly DispatcherTimer _progressTimer;
        private double _videoLength;
        private double _videoPosition;

        /// <summary>Initializes a new instance of the <see cref="VideoElement" /> class.</summary>
        public VideoElement()
        {
            InitializeComponent();
            _progressTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(50), DispatcherPriority.Background, ProgressUpdate, Dispatcher);
            _progressTimer.Start();
        }

        public static readonly DependencyProperty OriginalProperty =
            DependencyProperty.Register(nameof(Original), typeof(Uri), typeof(VideoElement), new PropertyMetadata<VideoElement>((c) => c.UpdateSplitter()));

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(Uri), typeof(VideoElement), new PropertyMetadata<VideoElement>((c) => c.UpdateSplitter()));

        public static readonly DependencyProperty PreviewProperty =
            DependencyProperty.Register(nameof(Preview), typeof(BitmapSource), typeof(VideoElement));

        public static readonly DependencyProperty SplitterPositionProperty =
            DependencyProperty.Register(nameof(SplitterPosition), typeof(SplitterPosition), typeof(VideoElement), new PropertyMetadata<VideoElement>((c) => c.UpdateSplitter()));

        public static readonly DependencyProperty SplitterVisibilityProperty =
            DependencyProperty.Register(nameof(SplitterVisibility), typeof(SplitterVisibility), typeof(VideoElement), new PropertyMetadata<VideoElement>((c) => c.UpdateSplitter()));

        public static readonly DependencyProperty SplitterDirectionProperty =
            DependencyProperty.Register(nameof(SplitterDirection), typeof(SplitterDirection), typeof(VideoElement), new PropertyMetadata<VideoElement>((c) => c.UpdateSplitter()));

        public static readonly DependencyProperty SplitterColorProperty =
            DependencyProperty.Register(nameof(SplitterColor), typeof(SolidColorBrush), typeof(VideoElement), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(128, 0, 0, 0))));

        public static readonly DependencyProperty IsSplitterEnabledProperty =
            DependencyProperty.Register(nameof(IsSplitterEnabled), typeof(bool), typeof(VideoElement));

        public static readonly DependencyProperty VideoSyncProperty =
            DependencyProperty.Register(nameof(VideoSync), typeof(bool), typeof(VideoElement), new PropertyMetadata<VideoElement>((c) => c.OnSyncVideo()));

        public Uri Original
        {
            get { return (Uri)GetValue(OriginalProperty); }
            set { SetValue(OriginalProperty, value); }
        }

        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public BitmapSource Preview
        {
            get { return (BitmapSource)GetValue(PreviewProperty); }
            set { SetValue(PreviewProperty, value); }
        }

        public SplitterPosition SplitterPosition
        {
            get { return (SplitterPosition)GetValue(SplitterPositionProperty); }
            set { SetValue(SplitterPositionProperty, value); }
        }

        public SplitterVisibility SplitterVisibility
        {
            get { return (SplitterVisibility)GetValue(SplitterVisibilityProperty); }
            set { SetValue(SplitterVisibilityProperty, value); }
        }

        public SplitterDirection SplitterDirection
        {
            get { return (SplitterDirection)GetValue(SplitterDirectionProperty); }
            set { SetValue(SplitterDirectionProperty, value); }
        }

        public SolidColorBrush SplitterColor
        {
            get { return (SolidColorBrush)GetValue(SplitterColorProperty); }
            set { SetValue(SplitterColorProperty, value); }
        }

        public bool IsSplitterEnabled
        {
            get { return (bool)GetValue(IsSplitterEnabledProperty); }
            set { SetValue(IsSplitterEnabledProperty, value); }
        }

        public bool VideoSync
        {
            get { return (bool)GetValue(VideoSyncProperty); }
            set { SetValue(VideoSyncProperty, value); }
        }

        public double VideoLength
        {
            get { return _videoLength; }
            set { _videoLength = value; NotifyPropertyChanged(); }
        }

        public double VideoPosition
        {
            get { return _videoPosition; }
            set { _videoPosition = value; NotifyPropertyChanged(); }
        }

        public bool HasOriginalImage => Original != null;
        public bool HasSourceImage => Source != null;


        /// <summary>
        /// Updates the splitter.
        /// </summary>
        private async Task UpdateSplitter()
        {
            if (!IsSplitterEnabled)
                return;

            GridSplitterContainer.Visibility = Visibility.Hidden;
            if (HasSourceImage)
            {
                AutoHideSplitter();
                if (SplitterPosition == SplitterPosition.Original)
                {
                    GridSplitterColumn.Width = SplitterDirection == SplitterDirection.LeftToRight
                        ? new GridLength(0)
                        : new GridLength(SourceContainer.ActualWidth + 45);
                }
                else if (SplitterPosition == SplitterPosition.Center)
                {
                    GridSplitterColumn.Width = new GridLength(0);
                    await Task.Delay(10);
                    GridSplitterColumn.Width = new GridLength(SourceContainer.ActualWidth / 2 + 30);
                }
                else if (SplitterPosition == SplitterPosition.Source)
                {
                    GridSplitterColumn.Width = SplitterDirection == SplitterDirection.RightToLeft
                        ? new GridLength(0)
                        : new GridLength(SourceContainer.ActualWidth + 45);
                }
            }
        }


        /// <summary>
        /// Auto hide splitter.
        /// </summary>
        private async void AutoHideSplitter()
        {
            GridSplitterContainer.Visibility = Visibility.Visible;
            if (SplitterVisibility == SplitterVisibility.Auto)
            {
                await Task.Delay(3000);
            }

            if (!IsMouseOver || SplitterVisibility == SplitterVisibility.Manual)
            {
                GridSplitterContainer.Visibility = Visibility.Hidden;
            }
        }


        /// <summary>
        /// GridSplitter SizeChanged event, Update Overlay Clip
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SizeChangedEventArgs"/> instance containing the event data.</param>
        private void GridSplitter_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!IsSplitterEnabled)
                return;

            VideoSourceControl.Clip = SplitterDirection == SplitterDirection.LeftToRight
                ? new RectangleGeometry(new Rect(0, 0, e.NewSize.Width, e.NewSize.Height))
                : new RectangleGeometry(new Rect(e.NewSize.Width, 0, VideoSourceControl.ActualWidth, VideoSourceControl.ActualHeight));

            VideoOriginalControl.Clip = SplitterDirection == SplitterDirection.LeftToRight
                ? new RectangleGeometry(new Rect(e.NewSize.Width, 0, Math.Max(0, VideoSourceControl.ActualWidth - e.NewSize.Width), VideoSourceControl.ActualHeight))
                : new RectangleGeometry(new Rect(0, 0, e.NewSize.Width, e.NewSize.Height));
        }


        /// <summary>
        /// MouseEnter
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            if (!IsSplitterEnabled)
                return;

            if (HasSourceImage && SplitterVisibility != SplitterVisibility.Manual)
            {
                GridSplitterContainer.Visibility = Visibility.Visible;
            }
        }


        /// <summary>
        /// MouseLeave
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (!IsSplitterEnabled)
                return;

            if (HasSourceImage && SplitterVisibility != SplitterVisibility.Manual)
            {
                GridSplitterContainer.Visibility = Visibility.Collapsed;
            }
        }


        /// <summary>
        /// Progress timer callback.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ProgressUpdate(object sender, EventArgs e)
        {
            if (VideoSourceControl.HasVideo)
            {
                VideoPosition = VideoSourceControl.Position.TotalSeconds;
                if (VideoLength == 0 && VideoSourceControl.NaturalDuration.HasTimeSpan)
                    VideoLength = VideoSourceControl.NaturalDuration.TimeSpan.TotalSeconds;
            }
        }


        /// <summary>
        /// Handles the Loaded event of the MediaElement control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MediaElement_Loaded(object sender, RoutedEventArgs e)
        {
            VideoSourceControl.LoadedBehavior = MediaState.Play;
            if (IsSplitterEnabled)
                VideoOriginalControl.LoadedBehavior = VideoSourceControl.LoadedBehavior;
        }


        /// <summary>
        /// Handles the MediaOpened event of the MediaElement control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (VideoSourceControl.NaturalDuration.HasTimeSpan)
                VideoLength = VideoSourceControl.NaturalDuration.TimeSpan.TotalSeconds;
        }

        /// <summary>
        /// Handles the MediaEnded event of the MediaElement control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            VideoPosition = VideoLength;
            VideoSourceControl.Position = TimeSpan.FromMilliseconds(1);
            if (IsSplitterEnabled)
                VideoOriginalControl.Position = VideoSourceControl.Position;
        }


        /// <summary>
        /// Handles the MouseDown event of the MediaElement control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void MediaElement_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            VideoSourceControl.LoadedBehavior = VideoSourceControl.LoadedBehavior == MediaState.Pause
                ? MediaState.Play
                : MediaState.Pause;

            if (IsSplitterEnabled)
                VideoOriginalControl.LoadedBehavior = VideoSourceControl.LoadedBehavior;
        }


        /// <summary>
        /// Raises the <see cref="E:System.Windows.FrameworkElement.SizeChanged" /> event, using the specified information as part of the eventual event data.
        /// </summary>
        /// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
        protected override async void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            await UpdateSplitter();
        }


        private Task OnSyncVideo()
        {
            if (VideoSourceControl.HasVideo)
            {
                VideoSourceControl.Position = TimeSpan.FromMilliseconds(1);
                if (VideoOriginalControl.HasVideo)
                    VideoOriginalControl.Position = VideoSourceControl.Position;
            }
            return Task.CompletedTask;
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
