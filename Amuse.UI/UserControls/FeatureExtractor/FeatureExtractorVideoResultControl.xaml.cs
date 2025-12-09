using Amuse.UI.Commands;
using Amuse.UI.Models;
using Amuse.UI.Models.FeatureExtractor;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Amuse.UI.UserControls
{
    public partial class FeatureExtractorVideoResultControl : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureExtractorVideoResultControl" /> class.
        /// </summary>
        public FeatureExtractorVideoResultControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty FeatureExtractorInfoProperty =
            DependencyProperty.Register(nameof(FeatureExtractorInfo), typeof(FeatureExtractorInfoModel), typeof(FeatureExtractorVideoResultControl));

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(VideoInputModel), typeof(FeatureExtractorVideoResultControl));

        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register(nameof(Result), typeof(FeatureExtractorVideoResultModel), typeof(FeatureExtractorVideoResultControl));

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(nameof(Progress), typeof(ProgressInfo), typeof(FeatureExtractorVideoResultControl));

        public static readonly DependencyProperty SaveVideoCommandProperty =
            DependencyProperty.Register(nameof(SaveVideoCommand), typeof(AsyncRelayCommand<FeatureExtractorVideoResultModel>), typeof(FeatureExtractorVideoResultControl));

        public static readonly DependencyProperty RemoveVideoCommandProperty =
            DependencyProperty.Register(nameof(RemoveVideoCommand), typeof(AsyncRelayCommand<FeatureExtractorVideoResultModel>), typeof(FeatureExtractorVideoResultControl));

        public static readonly DependencyProperty CopyVideoCommandProperty =
            DependencyProperty.Register(nameof(CopyVideoCommand), typeof(AsyncRelayCommand<FeatureExtractorVideoResultModel>), typeof(FeatureExtractorVideoResultControl));

        public static readonly DependencyProperty PreviewImageProperty =
            DependencyProperty.Register(nameof(PreviewImage), typeof(BitmapSource), typeof(FeatureExtractorVideoResultControl));

        public static readonly DependencyProperty VideoSyncProperty =
            DependencyProperty.Register(nameof(VideoSync), typeof(bool), typeof(FeatureExtractorVideoResultControl));

        public FeatureExtractorInfoModel FeatureExtractorInfo
        {
            get { return (FeatureExtractorInfoModel)GetValue(FeatureExtractorInfoProperty); }
            set { SetValue(FeatureExtractorInfoProperty, value); }
        }

        public VideoInputModel Source
        {
            get { return (VideoInputModel)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public FeatureExtractorVideoResultModel Result
        {
            get { return (FeatureExtractorVideoResultModel)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }

        public ProgressInfo Progress
        {
            get { return (ProgressInfo)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public AsyncRelayCommand<FeatureExtractorVideoResultModel> SaveVideoCommand
        {
            get { return (AsyncRelayCommand<FeatureExtractorVideoResultModel>)GetValue(SaveVideoCommandProperty); }
            set { SetValue(SaveVideoCommandProperty, value); }
        }

        public AsyncRelayCommand<FeatureExtractorVideoResultModel> RemoveVideoCommand
        {
            get { return (AsyncRelayCommand<FeatureExtractorVideoResultModel>)GetValue(RemoveVideoCommandProperty); }
            set { SetValue(RemoveVideoCommandProperty, value); }
        }

        public AsyncRelayCommand<FeatureExtractorVideoResultModel> CopyVideoCommand
        {
            get { return (AsyncRelayCommand<FeatureExtractorVideoResultModel>)GetValue(CopyVideoCommandProperty); }
            set { SetValue(CopyVideoCommandProperty, value); }
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
