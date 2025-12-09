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

namespace Amuse.UI.UserControls
{
    public partial class ImageElement : UserControl, INotifyPropertyChanged
    {
        /// <summary>Initializes a new instance of the <see cref="ImageElement" /> class.</summary>
        public ImageElement()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty OriginalProperty =
            DependencyProperty.Register(nameof(Original), typeof(BitmapSource), typeof(ImageElement), new PropertyMetadata<ImageElement>((c) => c.UpdateSplitter()));

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(BitmapSource), typeof(ImageElement), new PropertyMetadata<ImageElement>((c) => c.UpdateSplitter()));

        public static readonly DependencyProperty PreviewProperty =
            DependencyProperty.Register(nameof(Preview), typeof(BitmapSource), typeof(ImageElement));

        public static readonly DependencyProperty SplitterPositionProperty =
            DependencyProperty.Register(nameof(SplitterPosition), typeof(SplitterPosition), typeof(ImageElement), new PropertyMetadata<ImageElement>((c) => c.UpdateSplitter()));

        public static readonly DependencyProperty SplitterVisibilityProperty =
            DependencyProperty.Register(nameof(SplitterVisibility), typeof(SplitterVisibility), typeof(ImageElement), new PropertyMetadata<ImageElement>((c) => c.UpdateSplitter()));

        public static readonly DependencyProperty SplitterDirectionProperty =
            DependencyProperty.Register(nameof(SplitterDirection), typeof(SplitterDirection), typeof(ImageElement), new PropertyMetadata<ImageElement>((c) => c.UpdateSplitter()));

        public static readonly DependencyProperty CanvasProperty =
            DependencyProperty.Register(nameof(Canvas), typeof(BitmapSource), typeof(ImageElement));

        public static readonly DependencyProperty IsSaveCanvasEnabledProperty =
            DependencyProperty.Register(nameof(IsSaveCanvasEnabled), typeof(bool), typeof(ImageElement));

        public static readonly DependencyProperty SplitterColorProperty =
            DependencyProperty.Register(nameof(SplitterColor), typeof(SolidColorBrush), typeof(ImageElement), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(128, 0, 0, 0))));

        public static readonly DependencyProperty IsSplitterEnabledProperty =
            DependencyProperty.Register(nameof(IsSplitterEnabled), typeof(bool), typeof(ImageElement), new PropertyMetadata<ImageElement>((c) => c.UpdateSplitter()));

        public BitmapSource Original
        {
            get { return (BitmapSource)GetValue(OriginalProperty); }
            set { SetValue(OriginalProperty, value); }
        }

        public BitmapSource Source
        {
            get { return (BitmapSource)GetValue(SourceProperty); }
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

        public BitmapSource Canvas
        {
            get { return (BitmapSource)GetValue(CanvasProperty); }
            set { SetValue(CanvasProperty, value); }
        }

        public bool IsSaveCanvasEnabled
        {
            get { return (bool)GetValue(IsSaveCanvasEnabledProperty); }
            set { SetValue(IsSaveCanvasEnabledProperty, value); }
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
        /// Creates the canvas BitmapSource.
        /// </summary>
        /// <returns>BitmapSource.</returns>
        private BitmapSource CreateCanvasSource()
        {
            var visual = new DrawingVisual();
            var renderBitmap = new RenderTargetBitmap(Source.PixelWidth, Source.PixelHeight, 96, 96, PixelFormats.Pbgra32);
            using (var drawingContext = visual.RenderOpen())
            {
                drawingContext.DrawRectangle(new VisualBrush(SourceContainer), null, new Rect(new Point(0, 0), new Point(Source.PixelWidth, Source.PixelHeight)));
            }
            renderBitmap.Render(visual);
            return renderBitmap;
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

            ImageSourceControl.Clip = SplitterDirection == SplitterDirection.LeftToRight
                ? new RectangleGeometry(new Rect(0, 0, e.NewSize.Width, e.NewSize.Height))
                : new RectangleGeometry(new Rect(e.NewSize.Width, 0, ImageSourceControl.ActualWidth, ImageSourceControl.ActualHeight));

            ImageOriginalControl.Clip = SplitterDirection == SplitterDirection.LeftToRight
                ? new RectangleGeometry(new Rect(e.NewSize.Width, 0, Math.Max(0, ImageSourceControl.ActualWidth - e.NewSize.Width), ImageSourceControl.ActualHeight))
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
        /// Handles the MouseUp event of the GridSplitter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void GridSplitter_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsSplitterEnabled)
                return;

            if (IsSaveCanvasEnabled && HasOriginalImage && HasSourceImage)
                Canvas = CreateCanvasSource();
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


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        #endregion
    }
}
