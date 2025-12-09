using Amuse.UI.Commands;
using Amuse.UI.Helpers;
using Amuse.UI.Models;
using Amuse.UI.Models.StableDiffusion;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Amuse.UI.UserControls
{
    public partial class StableDiffusionVideoResultControl : UserControl, INotifyPropertyChanged
    {
        private bool _isSplitterEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="StableDiffusionVideoResultControl" /> class.
        /// </summary>
        public StableDiffusionVideoResultControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SchedulerOptionsProperty =
            DependencyProperty.Register(nameof(SchedulerOptions), typeof(SchedulerOptionsModel), typeof(StableDiffusionVideoResultControl));

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(VideoInputModel), typeof(StableDiffusionVideoResultControl));

        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register(nameof(Result), typeof(VideoResultModel), typeof(StableDiffusionVideoResultControl));

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(nameof(Progress), typeof(ProgressInfo), typeof(StableDiffusionVideoResultControl));

        public static readonly DependencyProperty SaveVideoCommandProperty =
            DependencyProperty.Register(nameof(SaveVideoCommand), typeof(AsyncRelayCommand<VideoResultModel>), typeof(StableDiffusionVideoResultControl));

        public static readonly DependencyProperty RemoveVideoCommandProperty =
            DependencyProperty.Register(nameof(RemoveVideoCommand), typeof(AsyncRelayCommand<VideoResultModel>), typeof(StableDiffusionVideoResultControl));

        public static readonly DependencyProperty CopyVideoCommandProperty =
            DependencyProperty.Register(nameof(CopyVideoCommand), typeof(AsyncRelayCommand<VideoResultModel>), typeof(StableDiffusionVideoResultControl));

        public static readonly DependencyProperty UpdateSeedCommandProperty =
            DependencyProperty.Register(nameof(UpdateSeedCommand), typeof(AsyncRelayCommand<int>), typeof(StableDiffusionVideoResultControl));

        public static readonly DependencyProperty PreviewImageProperty =
            DependencyProperty.Register(nameof(PreviewImage), typeof(BitmapSource), typeof(StableDiffusionVideoResultControl));

        public static readonly DependencyProperty VideoSyncProperty =
            DependencyProperty.Register(nameof(VideoSync), typeof(bool), typeof(StableDiffusionVideoResultControl));

        public SchedulerOptionsModel SchedulerOptions
        {
            get { return (SchedulerOptionsModel)GetValue(SchedulerOptionsProperty); }
            set { SetValue(SchedulerOptionsProperty, value); }
        }

        public VideoInputModel Source
        {
            get { return (VideoInputModel)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public VideoResultModel Result
        {
            get { return (VideoResultModel)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }

        public ProgressInfo Progress
        {
            get { return (ProgressInfo)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public AsyncRelayCommand<VideoResultModel> SaveVideoCommand
        {
            get { return (AsyncRelayCommand<VideoResultModel>)GetValue(SaveVideoCommandProperty); }
            set { SetValue(SaveVideoCommandProperty, value); }
        }

        public AsyncRelayCommand<VideoResultModel> RemoveVideoCommand
        {
            get { return (AsyncRelayCommand<VideoResultModel>)GetValue(RemoveVideoCommandProperty); }
            set { SetValue(RemoveVideoCommandProperty, value); }
        }

        public AsyncRelayCommand<VideoResultModel> CopyVideoCommand
        {
            get { return (AsyncRelayCommand<VideoResultModel>)GetValue(CopyVideoCommandProperty); }
            set { SetValue(CopyVideoCommandProperty, value); }
        }

        public AsyncRelayCommand<int> UpdateSeedCommand
        {
            get { return (AsyncRelayCommand<int>)GetValue(UpdateSeedCommandProperty); }
            set { SetValue(UpdateSeedCommandProperty, value); }
        }

        public BitmapSource PreviewImage
        {
            get { return (BitmapSource)GetValue(PreviewImageProperty); }
            set { SetValue(PreviewImageProperty, value); }
        }

        public bool VideoSync
        {
            get { return (bool)GetValue(VideoSyncProperty); }
            set { SetValue(VideoSyncProperty, value); }
        }

        public bool IsSplitterEnabled
        {
            get { return _isSplitterEnabled; }
            set { _isSplitterEnabled = value; NotifyPropertyChanged(); }
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


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        #endregion
    }
}
