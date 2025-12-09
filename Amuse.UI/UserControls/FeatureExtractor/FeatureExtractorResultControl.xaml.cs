using Amuse.UI.Commands;
using Amuse.UI.Models;
using Amuse.UI.Models.FeatureExtractor;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Amuse.UI.UserControls
{
    public partial class FeatureExtractorResultControl : UserControl, INotifyPropertyChanged
    {

        /// <summary>Initializes a new instance of the <see cref="FeatureExtractorResultControl" /> class.</summary>
        public FeatureExtractorResultControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty FeatureExtractorInfoProperty =
            DependencyProperty.Register(nameof(FeatureExtractorInfo), typeof(FeatureExtractorInfoModel), typeof(FeatureExtractorResultControl));

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(ImageInput), typeof(FeatureExtractorResultControl));

        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register(nameof(Result), typeof(FeatureExtractorResult), typeof(FeatureExtractorResultControl));

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(nameof(Progress), typeof(ProgressInfo), typeof(FeatureExtractorResultControl));

        public static readonly DependencyProperty SaveImageCommandProperty =
            DependencyProperty.Register(nameof(SaveImageCommand), typeof(AsyncRelayCommand<FeatureExtractorResult>), typeof(FeatureExtractorResultControl));

        public static readonly DependencyProperty RemoveImageCommandProperty =
            DependencyProperty.Register(nameof(RemoveImageCommand), typeof(AsyncRelayCommand<FeatureExtractorResult>), typeof(FeatureExtractorResultControl));

        public static readonly DependencyProperty CopyImageCommandProperty =
            DependencyProperty.Register(nameof(CopyImageCommand), typeof(AsyncRelayCommand<FeatureExtractorResult>), typeof(FeatureExtractorResultControl));

        public FeatureExtractorInfoModel FeatureExtractorInfo
        {
            get { return (FeatureExtractorInfoModel)GetValue(FeatureExtractorInfoProperty); }
            set { SetValue(FeatureExtractorInfoProperty, value); }
        }

        public ImageInput Source
        {
            get { return (ImageInput)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public FeatureExtractorResult Result
        {
            get { return (FeatureExtractorResult)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }

        public ProgressInfo Progress
        {
            get { return (ProgressInfo)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public AsyncRelayCommand<FeatureExtractorResult> SaveImageCommand
        {
            get { return (AsyncRelayCommand<FeatureExtractorResult>)GetValue(SaveImageCommandProperty); }
            set { SetValue(SaveImageCommandProperty, value); }
        }

        public AsyncRelayCommand<FeatureExtractorResult> RemoveImageCommand
        {
            get { return (AsyncRelayCommand<FeatureExtractorResult>)GetValue(RemoveImageCommandProperty); }
            set { SetValue(RemoveImageCommandProperty, value); }
        }

        public AsyncRelayCommand<FeatureExtractorResult> CopyImageCommand
        {
            get { return (AsyncRelayCommand<FeatureExtractorResult>)GetValue(CopyImageCommandProperty); }
            set { SetValue(CopyImageCommandProperty, value); }
        }


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
