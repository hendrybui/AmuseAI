using Amuse.UI.Commands;
using Amuse.UI.Models;
using Amuse.UI.Services;
using OnnxStack.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Amuse.UI.UserControls
{
    public partial class InpaintInputControl : UserControl, INotifyPropertyChanged
    {
        private readonly IFileService _fileService;
        private readonly ResizeAdorner _imageCanvasAdorner;
        private readonly SolidColorBrush _imageCanvasBackground;
        private readonly List<Stroke> _maskStrokedRemoved = new List<Stroke>();

        private int _maskDrawSize;
        private bool _hasMaskChanged;
        private DrawingAttributes _maskAttributes;
        private InkCanvasEditingMode _maskEditingMode = InkCanvasEditingMode.Ink;
        private DateTime _maskLastUpdate;
        private bool _hasSourceResult;
        private bool _isMaskInvertEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="InpaintInputControl" /> class.
        /// </summary>
        public InpaintInputControl()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
                _fileService = App.GetService<IFileService>();

            MaskDrawSize = 20;
            LoadImageCommand = new AsyncRelayCommand(LoadImage);
            ClearImageCommand = new AsyncRelayCommand(ClearImage, CanClearImage);
            CopyImageCommand = new AsyncRelayCommand(CopyImage, CanCopyImage);
            PasteImageCommand = new AsyncRelayCommand(PasteImage);
            SaveImageCommand = new AsyncRelayCommand(SaveImage, CanSaveImage);
            MaskLoadCommand = new AsyncRelayCommand(MaskLoad);
            MaskClearCommand = new AsyncRelayCommand(MaskClear);
            MaskModeCommand = new AsyncRelayCommand<InkCanvasEditingMode>(MaskMode);
            MaskUndoCommand = new AsyncRelayCommand(MaskUndo, CanMaskUndo);
            MaskRedoCommand = new AsyncRelayCommand(MaskRedo, CanMaskRedo);
            MaskSaveCommand = new AsyncRelayCommand(MaskSave, CanMaskSave);
            InitializeComponent();

            _imageCanvasAdorner = new ResizeAdorner(Container, async () => await Refresh());
            _imageCanvasBackground = new SolidColorBrush(Color.FromRgb(128, 128, 128));
        }

        public AsyncRelayCommand CopyImageCommand { get; }
        public AsyncRelayCommand PasteImageCommand { get; }
        public AsyncRelayCommand SaveImageCommand { get; }
        public AsyncRelayCommand MaskLoadCommand { get; }
        public AsyncRelayCommand MaskClearCommand { get; }
        public AsyncRelayCommand<InkCanvasEditingMode> MaskModeCommand { get; }
        public AsyncRelayCommand MaskUndoCommand { get; }
        public AsyncRelayCommand MaskRedoCommand { get; }
        public AsyncRelayCommand MaskSaveCommand { get; }

        public static readonly DependencyProperty LoadImageCommandProperty =
            DependencyProperty.Register(nameof(LoadImageCommand), typeof(AsyncRelayCommand), typeof(InpaintInputControl));

        public static readonly DependencyProperty ClearImageCommandProperty =
            DependencyProperty.Register(nameof(ClearImageCommand), typeof(AsyncRelayCommand), typeof(InpaintInputControl));

        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register(nameof(Result), typeof(ImageInput), typeof(InpaintInputControl));

        public static readonly DependencyProperty MaskResultProperty =
            DependencyProperty.Register(nameof(MaskResult), typeof(ImageInput), typeof(InpaintInputControl));

        public static readonly DependencyProperty IsMaskEnabledProperty =
            DependencyProperty.Register(nameof(IsMaskEnabled), typeof(bool), typeof(InpaintInputControl));

        public static readonly DependencyProperty ZoomDirectionProperty =
            DependencyProperty.Register(nameof(ZoomDirection), typeof(StretchDirection), typeof(InpaintInputControl), new PropertyMetadata(StretchDirection.Both));

        public static readonly DependencyProperty IsCropDialogEnabledProperty =
            DependencyProperty.Register(nameof(IsCropDialogEnabled), typeof(bool), typeof(InpaintInputControl));

        public static readonly DependencyProperty ToolbarVisibilityProperty =
            DependencyProperty.Register(nameof(ToolbarVisibility), typeof(Visibility), typeof(InpaintInputControl));

        public static readonly DependencyProperty SourceImageProperty =
            DependencyProperty.Register(nameof(SourceImage), typeof(ImageInput), typeof(InpaintInputControl), new PropertyMetadata(OnSourceChanged));

        public static readonly DependencyProperty ImageWidthProperty =
            DependencyProperty.Register(nameof(ImageWidth), typeof(int), typeof(InpaintInputControl), new PropertyMetadata(512, OnSizeChanged));

        public static readonly DependencyProperty ImageHeightProperty =
            DependencyProperty.Register(nameof(ImageHeight), typeof(int), typeof(InpaintInputControl), new PropertyMetadata(512, OnSizeChanged));

        public AsyncRelayCommand LoadImageCommand
        {
            get { return (AsyncRelayCommand)GetValue(LoadImageCommandProperty); }
            set { SetValue(LoadImageCommandProperty, value); }
        }

        public AsyncRelayCommand ClearImageCommand
        {
            get { return (AsyncRelayCommand)GetValue(ClearImageCommandProperty); }
            set { SetValue(ClearImageCommandProperty, value); }
        }

        public ImageInput Result
        {
            get { return (ImageInput)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }

        public ImageInput MaskResult
        {
            get { return (ImageInput)GetValue(MaskResultProperty); }
            set { SetValue(MaskResultProperty, value); }
        }

        public bool IsMaskEnabled
        {
            get { return (bool)GetValue(IsMaskEnabledProperty); }
            set { SetValue(IsMaskEnabledProperty, value); }
        }

        public Visibility ToolbarVisibility
        {
            get { return (Visibility)GetValue(ToolbarVisibilityProperty); }
            set { SetValue(ToolbarVisibilityProperty, value); }
        }

        public StretchDirection ZoomDirection
        {
            get { return (StretchDirection)GetValue(ZoomDirectionProperty); }
            set { SetValue(ZoomDirectionProperty, value); }
        }

        public bool IsCropDialogEnabled
        {
            get { return (bool)GetValue(IsCropDialogEnabledProperty); }
            set { SetValue(IsCropDialogEnabledProperty, value); }
        }

        public int ImageWidth
        {
            get { return (int)GetValue(ImageWidthProperty); }
            set { SetValue(ImageWidthProperty, value); }
        }

        public int ImageHeight
        {
            get { return (int)GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }

        public InkCanvasEditingMode MaskEditingMode
        {
            get { return _maskEditingMode; }
            set { _maskEditingMode = value; NotifyPropertyChanged(); }
        }

        public DrawingAttributes MaskAttributes
        {
            get { return _maskAttributes; }
            set { _maskAttributes = value; NotifyPropertyChanged(); }
        }

        public bool HasMaskChanged
        {
            get { return _hasMaskChanged; }
            set { _hasMaskChanged = value; NotifyPropertyChanged(); }
        }

        public int MaskDrawSize
        {
            get { return _maskDrawSize; }
            set
            {
                _maskDrawSize = value;
                NotifyPropertyChanged();
                UpdateMaskAttributes();
            }
        }

        public ImageInput SourceImage
        {
            get { return (ImageInput)GetValue(SourceImageProperty); }
            set { SetValue(SourceImageProperty, value); }
        }

        public bool HasSourceResult
        {
            get { return _hasSourceResult; }
            set { _hasSourceResult = value; NotifyPropertyChanged(); }
        }

        public bool IsMaskInvertEnabled
        {
            get { return _isMaskInvertEnabled; }
            set { _isMaskInvertEnabled = value; NotifyPropertyChanged(); }
        }


        /// <summary>
        /// Loads the image.
        /// </summary>
        /// <returns></returns>
        private async Task LoadImage()
        {
            await ShowDialog();
        }


        /// <summary>
        /// Clears the image.
        /// </summary>
        /// <returns></returns>
        private Task ClearImage()
        {
            SetCurrentValue(SourceImageProperty, null);
            HasSourceResult = false;
            Result = null;
            ImageCanvas.Background = Brushes.Transparent;
            return MaskClear();
        }


        /// <summary>
        /// Can ClearImage
        /// </summary>
        /// <returns><c>true</c> if this instance [can clear image]; otherwise, <c>false</c>.</returns>
        private bool CanClearImage()
        {
            return Result?.Image != null;
        }


        /// <summary>
        /// Copies the image.
        /// </summary>
        /// <returns></returns>
        private Task CopyImage()
        {
            Clipboard.SetImage(Result.Image);
            return Task.CompletedTask;
        }


        /// <summary>
        /// Determines whether this instance can copy the image.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can copy image; otherwise, <c>false</c>.
        /// </returns>
        private bool CanCopyImage()
        {
            return Result?.Image != null;
        }


        /// <summary>
        /// Saves the image.
        /// </summary>
        private async Task SaveImage()
        {
            await _fileService.SaveAsImageFile(Result.Image.ToBitmapImage());
        }


        /// <summary>
        /// Determines whether this instance can save the image.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can save image; otherwise, <c>false</c>.
        /// </returns>
        private bool CanSaveImage()
        {
            return Result?.Image != null;
        }


        /// <summary>
        /// Paste the image.
        /// </summary>
        /// <returns></returns>
        private async Task PasteImage()
        {
            if (Clipboard.ContainsImage())
                await ShowDialog(Clipboard.GetImage());
            else if (Clipboard.ContainsFileDropList())
            {
                var imageFile = Clipboard.GetFileDropList()
                    .OfType<string>()
                    .FirstOrDefault();
                await ShowDialog(null, imageFile);
            }
        }


        /// <summary>
        /// Shows the image dialog.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sourceFile">The source file.</param>
        private async Task ShowDialog(BitmapSource source = null, string sourceFile = null)
        {
            if (IsCropDialogEnabled)
                await ShowCropImageDialog(source, sourceFile);
            else
                await ShowImageDialog(source, sourceFile);
        }


        /// <summary>
        /// Shows the crop image dialog.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sourceFile">The source file.</param>
        private async Task ShowCropImageDialog(BitmapSource source = null, string sourceFile = null)
        {
            var imageResult = await _fileService.OpenImageFileCropped(ImageWidth, ImageHeight, source, sourceFile);
            if (imageResult == null)
                return;

            imageResult.FileName ??= sourceFile ?? "Image Data";
            SetCurrentValue(SourceImageProperty, imageResult);
        }


        /// <summary>
        /// Shows the image picker dialog.
        /// </summary>
        private async Task ShowImageDialog(BitmapSource source = null, string sourceFile = null)
        {
            var imageResult = source == null
                ? await _fileService.OpenImageFile(sourceFile)
                : new ImageInput { Image = source, FileName = "Image Data" };

            if (imageResult == null)
                return;

            SetCurrentValue(SourceImageProperty, imageResult);
        }


        /// <summary>
        /// Refreshes this Image and Mask
        /// </summary>
        private async Task Refresh()
        {
            await CreateImage();
            if (IsMaskEnabled)
                await CreateMask();
        }


        /// <summary>
        /// Updates the source image.
        /// </summary>
        /// <param name="source">The source.</param>
        public async Task UpdateSource(ImageInput source)
        {
            if (IsMaskEnabled)
            {
                ResetImageCanvas();
                UpdateMaskAttributes();
            }
            HasSourceResult = true;
            await Task.Delay(200);
            await Refresh();
        }


        /// <summary>
        /// Resets the image canvas.
        /// </summary>
        private void ResetImageCanvas()
        {
            ImageCanvas.Background = _imageCanvasBackground;
            Container.Width = ImageWidth;
            Container.Height = ImageHeight;
            Canvas.SetLeft(Container, 0.0);
            Canvas.SetTop(Container, 0.0);
        }


        /// <summary>
        /// Loads a mask from file
        /// </summary>
        private async Task MaskLoad()
        {
            var imageResult = await _fileService.OpenImageFileCropped(ImageWidth, ImageHeight);
            if (imageResult == null)
                return;

            await MaskClear();
            MaskCanvas.Background = new ImageBrush(imageResult.Image);
            await CreateMask();
        }


        /// <summary>
        /// Clear the mask
        /// </summary>
        /// <returns></returns>
        private Task MaskClear()
        {
            HasMaskChanged = false;
            MaskCanvas.Strokes.Clear();
            MaskCanvas.Background = Brushes.Transparent;
            return Task.CompletedTask;
        }


        /// <summary>
        /// Save the mask to disk
        /// </summary>
        private async Task MaskSave()
        {
            await _fileService.SaveAsImageFile(MaskResult.Image.ToBitmapImage());
        }


        /// <summary>
        /// Determines whether this instance can save the mask
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can save the mask; otherwise, <c>false</c>.
        /// </returns>
        private bool CanMaskSave()
        {
            return Result?.Image != null;
        }


        /// <summary>
        /// Undo the last mask stroke
        /// </summary>
        /// <returns></returns>
        private async Task MaskUndo()
        {
            if (MaskCanvas.Strokes.Count == 0)
                return;

            var lastStroke = MaskCanvas.Strokes.Last();
            if (MaskCanvas.Strokes.Remove(lastStroke))
            {
                _maskStrokedRemoved.Add(lastStroke);
                await CreateMask();
            }
        }


        /// <summary>
        /// Determines whether this instance can undo last stroke.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can undo last stroke; otherwise, <c>false</c>.
        /// </returns>
        private bool CanMaskUndo()
        {
            return MaskCanvas.Strokes.Count > 0;
        }


        /// <summary>
        /// Redo the last mask stroke
        /// </summary>
        /// <returns></returns>
        private async Task MaskRedo()
        {
            if (_maskStrokedRemoved.Count == 0)
                return;

            var lastStroke = _maskStrokedRemoved.Last();
            if (_maskStrokedRemoved.Remove(lastStroke))
            {
                MaskCanvas.Strokes.Add(lastStroke);
                await CreateMask();
            }
        }


        /// <summary>
        /// Determines whether this instance can redo last mask stroke.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can redo last mask stroke; otherwise, <c>false</c>.
        /// </returns>
        private bool CanMaskRedo()
        {
            return _maskStrokedRemoved.Count > 0;
        }


        /// <summary>
        /// Toggles the MaskMode, Draw or Erase
        /// </summary>
        /// <returns></returns>
        private Task MaskMode(InkCanvasEditingMode mode)
        {
            MaskEditingMode = mode;
            AdornerLayer.GetAdornerLayer(Container)?.Remove(_imageCanvasAdorner);
            if (_maskEditingMode == InkCanvasEditingMode.None)
                AdornerLayer.GetAdornerLayer(Container)?.Add(_imageCanvasAdorner);

            return Task.CompletedTask;
        }


        /// <summary>
        /// Updates the mask attributes.
        /// </summary>
        private void UpdateMaskAttributes()
        {
            var multiplier = ImageWidth >= 1024 || ImageHeight >= 1024 ? 2 : 1;
            MaskAttributes = new DrawingAttributes
            {
                Color = Colors.Black,
                Height = _maskDrawSize * multiplier,
                Width = _maskDrawSize * multiplier,
            };
        }


        /// <summary>
        /// Creates the mask.
        /// </summary>
        /// <returns></returns>
        private Task CreateMask()
        {
            MaskResult = new ImageInput
            {
                Image = CreateBitmap(MaskCanvas, _isMaskInvertEnabled),
                FileName = "Generated Mask",
            };
            HasMaskChanged = false;
            return Task.CompletedTask;
        }


        /// <summary>
        /// Creates the image.
        /// </summary>
        /// <returns>Task.</returns>
        private Task CreateImage()
        {
            Result = new ImageInput
            {
                Image = IsMaskEnabled
                    ? CreateBitmap(ImageCanvas, false)
                    : SourceImage.Image,
                FileName = "Generated Input",
            };
            return Task.CompletedTask;
        }


        /// <summary>
        /// Creates a BitmapSource from Canvas.
        /// </summary>
        /// <returns></returns>
        public BitmapSource CreateBitmap(FrameworkElement frameworkElement, bool invert)
        {
            if (frameworkElement.ActualWidth == 0)
                return Utils.CreateEmptyBitmapImage(ImageWidth, ImageHeight);

            // Create a RenderTargetBitmap to render the Canvas content.
            var renderBitmap = new RenderTargetBitmap((int)frameworkElement.ActualWidth, (int)frameworkElement.ActualHeight, 96, 96, PixelFormats.Pbgra32);

            // Make a drawing visual to render.
            var visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                VisualBrush brush = new VisualBrush(frameworkElement);
                context.DrawRectangle(brush, null, new Rect(new Point(0, 0), new Point(frameworkElement.ActualWidth, frameworkElement.ActualHeight)));

            }
            renderBitmap.Render(visual);
            if (invert)
                return renderBitmap.InvertMaskAlpha();

            return renderBitmap;
        }


        /// <summary>
        /// Handles the MouseLeftButtonDown event of the MaskCanvas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private async void MaskCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            HasMaskChanged = true;
            _maskStrokedRemoved.Clear();
            await Refresh();
        }


        /// <summary>
        /// Handles the MouseLeftButtonUp event of the MaskCanvas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private async void MaskCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            await Refresh();
        }


        /// <summary>
        /// Handles the PreviewMouseMove event of the MaskCanvas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private async void MaskCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (DateTime.Now > _maskLastUpdate)
                {
                    _maskLastUpdate = DateTime.Now.AddMilliseconds(200);
                    await Refresh();
                }
            }
        }


        /// <summary>
        /// Handles the OnMouseWheel event of the MaskCanvas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseWheelEventArgs"/> instance containing the event data.</param>
        private void MaskCanvas_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            MaskDrawSize = e.Delta > 0
                ? Math.Min(100, MaskDrawSize + 1)
                : Math.Max(1, MaskDrawSize - 1);
        }


        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseEnter" /> attached event is raised on this element. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            Keyboard.Focus(this);
            base.OnMouseEnter(e);
        }


        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseLeave" /> attached event is raised on this element. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            Keyboard.ClearFocus();
            base.OnMouseLeave(e);
        }


        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.DragDrop.PreviewDrop" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.DragEventArgs" /> that contains the event data.</param>
        protected override async void OnPreviewDrop(DragEventArgs e)
        {
            var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (!fileNames.IsNullOrEmpty())
                await ShowDialog(null, fileNames.FirstOrDefault());

            var bitmapImage = e.Data.GetData(typeof(BitmapImage)) as BitmapImage;
            if (bitmapImage is not null)
                await ShowImageDialog(bitmapImage);
        }


        /// <summary>
        /// Handles the <see cref="E:SourceChanged" /> event.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static async void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InpaintInputControl control && e.NewValue is ImageInput source)
                await control.UpdateSource(source);
        }


        private static async void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InpaintInputControl control && control.SourceImage is not null)
            {
                await Task.Delay(200);
                await control.Refresh();
            }
        }


        /// <summary>
        /// Handles the PreviewMouseLeftButtonUp event of the Control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private async void Control_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2 && MaskResult is null)
            {
                await ShowDialog();
            }
        }


        /// <summary>
        /// Handles the Checked event of the MaskInvert ToggleButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void MaskInvert_ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            await Refresh();
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
