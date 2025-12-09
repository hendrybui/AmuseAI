using Amuse.UI.Commands;
using Amuse.UI.Helpers;
using Amuse.UI.Models;
using Amuse.UI.Services;
using OnnxStack.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace Amuse.UI.UserControls
{
    public partial class PaintSurfaceControl : UserControl, INotifyPropertyChanged
    {
        private readonly IFileService _fileService;
        private readonly List<Stroke> _canvasStrokedRemoved = new List<Stroke>();

        private int _drawingToolSize;
        public DateTime _canvasLastUpdate;
        private DrawingAttributes _drawingAttributes;
        private Color _selectedColor = Colors.Black;
        private InkCanvasEditingMode _canvasEditingMode = InkCanvasEditingMode.Ink;
        private PaintDrawingTool _canvasDrawingTool;
        private bool _isPickerOpen;
        private Color _previousColor = Colors.Red;
        private ObservableCollection<Color> _recentColors;
        private bool _isFitToCurveEnabled;
        private bool _isPressureEnabled = true;
        private double _toolOutlineX;
        private double _toolOutlineY;
        private Visibility _toolOutlineVisibility;


        /// <summary>
        /// Initializes a new instance of the <see cref="PaintSurfaceControl" /> class.
        /// </summary>
        public PaintSurfaceControl()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
                _fileService = App.GetService<IFileService>();

            DrawingToolSize = 10;
            RecentColors = new ObservableCollection<Color>();
            LoadCanvasCommand = new AsyncRelayCommand(LoadCanvas);
            ClearCanvasCommand = new AsyncRelayCommand(ClearCanvas);
            CopyCanvasCommand = new AsyncRelayCommand(CopyCanvas, CanCopyCanvas);
            PasteCanvasCommand = new AsyncRelayCommand(PasteCanvas);
            SaveCanvasCommand = new AsyncRelayCommand(SaveCanvas, CanSaveCanvas);
            UndoCanvasCommand = new AsyncRelayCommand(UndoCanvas, CanUndoCanvas);
            RedoCanvasCommand = new AsyncRelayCommand(RedoCanvas, CanRedoCanvas);
            FillCanvasCommand = new AsyncRelayCommand(FillCanvas);
            SelectToolCommand = new AsyncRelayCommand<PaintDrawingTool>(SelectDrawingTool);
            InitializeComponent();
            SetRecentColors();
            SelectedColor = Colors.Black;
        }

        public AsyncRelayCommand LoadCanvasCommand { get; }
        public AsyncRelayCommand ClearCanvasCommand { get; }
        public AsyncRelayCommand FillCanvasCommand { get; }
        public AsyncRelayCommand CopyCanvasCommand { get; }
        public AsyncRelayCommand PasteCanvasCommand { get; }
        public AsyncRelayCommand SaveCanvasCommand { get; }
        public AsyncRelayCommand UndoCanvasCommand { get; }
        public AsyncRelayCommand RedoCanvasCommand { get; }
        public AsyncRelayCommand<PaintDrawingTool> SelectToolCommand { get; }

        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings", typeof(AmuseSettings), typeof(PaintSurfaceControl));

        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register("Result", typeof(ImageInput), typeof(PaintSurfaceControl), new PropertyMetadata(OnResultChanged));

        public static readonly DependencyProperty ZoomDirectionProperty =
            DependencyProperty.Register("ZoomDirection", typeof(StretchDirection), typeof(PaintSurfaceControl), new PropertyMetadata(StretchDirection.Both));

        public static readonly DependencyProperty SurfaceWidthProperty =
            DependencyProperty.Register("SurfaceWidth", typeof(int), typeof(PaintSurfaceControl), new PropertyMetadata<PaintSurfaceControl>((c) => c.OnSurfaceSizeChanged()) { DefaultValue = 512 });

        public static readonly DependencyProperty SurfaceHeightProperty =
            DependencyProperty.Register("SurfaceHeight", typeof(int), typeof(PaintSurfaceControl), new PropertyMetadata<PaintSurfaceControl>((c) => c.OnSurfaceSizeChanged()) { DefaultValue = 512 });

        public static readonly DependencyProperty OverlayImageProperty =
            DependencyProperty.Register("OverlayImage", typeof(BitmapSource), typeof(PaintSurfaceControl));

        public static readonly DependencyProperty OverlayOpacityProperty =
            DependencyProperty.Register("OverlayOpacity", typeof(double), typeof(PaintSurfaceControl), new PropertyMetadata(0.2));

        public static readonly DependencyProperty GenerateStepsProperty =
            DependencyProperty.Register("GenerateSteps", typeof(int), typeof(PaintSurfaceControl), new PropertyMetadata(6));

        public static readonly DependencyProperty GenerateStrengthProperty =
            DependencyProperty.Register("GenerateStrength", typeof(float), typeof(PaintSurfaceControl), new PropertyMetadata(0.75f));

        public static readonly DependencyProperty GenerateCommandProperty =
            DependencyProperty.Register("GenerateCommand", typeof(AsyncRelayCommand), typeof(PaintSurfaceControl));

        public static readonly DependencyProperty IsGeneratingProperty =
            DependencyProperty.Register("IsGenerating", typeof(bool), typeof(PaintSurfaceControl));

        public static readonly DependencyProperty GenerateCancelCommandProperty =
            DependencyProperty.Register("GenerateCancelCommand", typeof(AsyncRelayCommand), typeof(PaintSurfaceControl));

        public static readonly DependencyProperty IsRandomSeedEnabledProperty =
            DependencyProperty.Register("IsRandomSeedEnabled", typeof(bool), typeof(PaintSurfaceControl));

        public static readonly DependencyProperty IsRealtimeEnabledProperty =
            DependencyProperty.Register("IsRealtimeEnabled", typeof(bool), typeof(PaintSurfaceControl));

        public static readonly DependencyProperty RandomizeSeedCommandProperty =
            DependencyProperty.Register("RandomizeSeedCommand", typeof(AsyncRelayCommand), typeof(PaintSurfaceControl));

        public static readonly DependencyProperty AssistantStrengthProperty =
            DependencyProperty.Register("AssistantStrength", typeof(float), typeof(PaintSurfaceControl), new PropertyMetadata(0.5f));

        public static readonly DependencyProperty IsControlNetEnabledProperty =
            DependencyProperty.Register(nameof(IsControlNetEnabled), typeof(bool), typeof(PaintSurfaceControl));

        public AmuseSettings Settings
        {
            get { return (AmuseSettings)GetValue(SettingsProperty); }
            set { SetValue(SettingsProperty, value); }
        }

        public float GenerateStrength
        {
            get { return (float)GetValue(GenerateStrengthProperty); }
            set { SetValue(GenerateStrengthProperty, value); }
        }

        public float AssistantStrength
        {
            get { return (float)GetValue(AssistantStrengthProperty); }
            set { SetValue(AssistantStrengthProperty, value); }
        }

        public int GenerateSteps
        {
            get { return (int)GetValue(GenerateStepsProperty); }
            set { SetValue(GenerateStepsProperty, value); }
        }

        public double OverlayOpacity
        {
            get { return (double)GetValue(OverlayOpacityProperty); }
            set { SetValue(OverlayOpacityProperty, value); }
        }

        public BitmapSource OverlayImage
        {
            get { return (BitmapSource)GetValue(OverlayImageProperty); }
            set { SetValue(OverlayImageProperty, value); }
        }

        public int SurfaceWidth
        {
            get { return (int)GetValue(SurfaceWidthProperty); }
            set { SetValue(SurfaceWidthProperty, value); }
        }

        public int SurfaceHeight
        {
            get { return (int)GetValue(SurfaceHeightProperty); }
            set { SetValue(SurfaceHeightProperty, value); }
        }

        public AsyncRelayCommand GenerateCommand
        {
            get { return (AsyncRelayCommand)GetValue(GenerateCommandProperty); }
            set { SetValue(GenerateCommandProperty, value); }
        }

        public bool IsGenerating
        {
            get { return (bool)GetValue(IsGeneratingProperty); }
            set { SetValue(IsGeneratingProperty, value); }
        }

        public AsyncRelayCommand GenerateCancelCommand
        {
            get { return (AsyncRelayCommand)GetValue(GenerateCancelCommandProperty); }
            set { SetValue(GenerateCancelCommandProperty, value); }
        }

        public bool IsRandomSeedEnabled
        {
            get { return (bool)GetValue(IsRandomSeedEnabledProperty); }
            set { SetValue(IsRandomSeedEnabledProperty, value); }
        }

        public bool IsRealtimeEnabled
        {
            get { return (bool)GetValue(IsRealtimeEnabledProperty); }
            set { SetValue(IsRealtimeEnabledProperty, value); }
        }

        public AsyncRelayCommand RandomizeSeedCommand
        {
            get { return (AsyncRelayCommand)GetValue(RandomizeSeedCommandProperty); }
            set { SetValue(RandomizeSeedCommandProperty, value); }
        }

        public ImageInput Result
        {
            get { return (ImageInput)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }

        public bool IsControlNetEnabled
        {
            get { return (bool)GetValue(IsControlNetEnabledProperty); }
            set { SetValue(IsControlNetEnabledProperty, value); }
        }

        public bool IsFitToCurveEnabled
        {
            get { return _isFitToCurveEnabled; }
            set { _isFitToCurveEnabled = value; NotifyPropertyChanged(); UpdateBrushAttributes(); }
        }

        public bool IsPressureEnabled
        {
            get { return _isPressureEnabled; }
            set { _isPressureEnabled = value; NotifyPropertyChanged(); UpdateBrushAttributes(); }
        }

        public double ToolOutlineX
        {
            get { return _toolOutlineX; }
            set { _toolOutlineX = value; NotifyPropertyChanged(); }
        }

        public double ToolOutlineY
        {
            get { return _toolOutlineY; }
            set { _toolOutlineY = value; NotifyPropertyChanged(); }
        }

        public Visibility ToolOutlineVisibility
        {
            get { return _toolOutlineVisibility; }
            set { _toolOutlineVisibility = value; NotifyPropertyChanged(); }
        }

        public StretchDirection ZoomDirection
        {
            get { return (StretchDirection)GetValue(ZoomDirectionProperty); }
            set { SetValue(ZoomDirectionProperty, value); }
        }

        public InkCanvasEditingMode CanvasEditingMode
        {
            get { return _canvasEditingMode; }
            set { _canvasEditingMode = value; NotifyPropertyChanged(); }
        }

        public DrawingAttributes BrushAttributes
        {
            get { return _drawingAttributes; }
            set { _drawingAttributes = value; NotifyPropertyChanged(); }
        }

        public int DrawingToolSize
        {
            get { return _drawingToolSize; }
            set
            {
                _drawingToolSize = value;
                NotifyPropertyChanged();
                UpdateBrushAttributes();
            }
        }

        public Color SelectedColor
        {
            get { return _selectedColor; }
            set
            {
                _selectedColor = value;
                NotifyPropertyChanged();
                UpdateBrushAttributes();
            }
        }

        public PaintDrawingTool CanvasDrawingTool
        {
            get { return _canvasDrawingTool; }
            set { _canvasDrawingTool = value; NotifyPropertyChanged(); }
        }

        public ObservableCollection<Color> RecentColors
        {
            get { return _recentColors; }
            set { _recentColors = value; NotifyPropertyChanged(); }
        }

        public bool IsPickerOpen
        {
            get { return _isPickerOpen; }
            set
            {
                _isPickerOpen = value;
                if (!_isPickerOpen && _previousColor != SelectedColor)
                {
                    AddRecentColor(_previousColor);
                    _previousColor = SelectedColor;
                }
                NotifyPropertyChanged();
            }
        }


        /// <summary>
        /// Loads the image.
        /// </summary>
        /// <returns></returns>
        private Task LoadCanvas()
        {
            ShowCropImageDialog();
            return Task.CompletedTask;
        }


        /// <summary>
        /// Clears the image.
        /// </summary>
        /// <returns></returns>
        private async Task ClearCanvas()
        {
            PaintCanvas.Strokes.Clear();
            PaintCanvas.Background = new SolidColorBrush(Colors.White);
            await Task.Delay(500);
            await CreateCanvas();
        }


        /// <summary>
        /// Saves the canvas.
        /// </summary>
        private async Task SaveCanvas()
        {
            await _fileService.SaveAsImageFile(CreateSurfaceImage());
        }


        /// <summary>
        /// Determines whether this instance can save canvas.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can save canvas; otherwise, <c>false</c>.
        /// </returns>
        private bool CanSaveCanvas()
        {
            return Result?.Image != null;
        }


        /// <summary>
        /// Fills the canvas with the SelectedColor.
        /// </summary>
        /// <returns></returns>
        private async Task FillCanvas()
        {
            PaintCanvas.Background = new SolidColorBrush(SelectedColor);
            await Task.Delay(500);
            await CreateCanvas();
        }


        /// <summary>
        /// Saves the Canvas
        /// </summary>
        /// <returns></returns>
        private Task CreateCanvas()
        {
            Result = new ImageInput
            {
                Image = CreateCanvasImage(),
                FileName = "Canvas Image",
            };
            return Task.CompletedTask;
        }


        /// <summary>
        /// Updates the brush attributes.
        /// </summary>
        private void UpdateBrushAttributes()
        {
            var previousShape = BrushAttributes?.StylusTip ?? StylusTip.Ellipse;
            if (CanvasDrawingTool == PaintDrawingTool.RoundBrush)
                previousShape = StylusTip.Ellipse;
            if (CanvasDrawingTool == PaintDrawingTool.SquareBrush)
                previousShape = StylusTip.Rectangle;

            BrushAttributes = new DrawingAttributes
            {
                Color = _selectedColor,
                Height = _drawingToolSize,
                Width = _drawingToolSize,
                IgnorePressure = !_isPressureEnabled,
                FitToCurve = _isFitToCurveEnabled,
                StylusTip = previousShape
            };

            if (CanvasDrawingTool == PaintDrawingTool.Highlight)
                BrushAttributes.Color = Color.FromArgb(128, BrushAttributes.Color.R, BrushAttributes.Color.G, BrushAttributes.Color.B);

            CanvasEditingMode = CanvasDrawingTool != PaintDrawingTool.Eraser
                ? InkCanvasEditingMode.Ink
                : InkCanvasEditingMode.EraseByPoint;
        }


        /// <summary>
        /// Selects the drawing tool.
        /// </summary>
        /// <param name="selectedTool">The selected tool.</param>
        /// <returns></returns>
        private Task SelectDrawingTool(PaintDrawingTool selectedTool)
        {
            CanvasDrawingTool = selectedTool;
            UpdateBrushAttributes();
            return Task.CompletedTask;
        }


        /// <summary>
        /// Creates the canvas image, background and strokes
        /// </summary>
        /// <returns></returns>
        public BitmapSource CreateCanvasImage()
        {
            return App.Current.Dispatcher.Invoke<BitmapSource>(() =>
            {
                if (PaintCanvas.ActualWidth == 0)
                    return Utils.CreateEmptyBitmapImage(SurfaceWidth, SurfaceHeight);

                // Create a RenderTargetBitmap to render the Canvas content.
                var renderBitmap = new RenderTargetBitmap((int)PaintCanvas.ActualWidth, (int)PaintCanvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);

                // Make a drawing visual to render.
                var visual = new DrawingVisual();
                using (DrawingContext context = visual.RenderOpen())
                {
                    context.DrawRectangle(new VisualBrush(PaintCanvas), null, new Rect(new Point(0, 0), new Point(PaintCanvas.ActualWidth, PaintCanvas.ActualHeight)));
                }
                renderBitmap.Render(visual);
                return renderBitmap;
            });
        }


        /// <summary>
        /// Creates the surface image, this incudes canvas and result image at current opacity
        /// </summary>
        /// <returns>The image that can be visualy seen on screen</returns>
        public BitmapSource CreateSurfaceImage()
        {
            return App.Current.Dispatcher.Invoke<BitmapSource>(() =>
            {
                if (SurfaceCanvas.ActualWidth == 0)
                    return Utils.CreateEmptyBitmapImage(SurfaceWidth, SurfaceHeight);

                // Create a RenderTargetBitmap to render the Canvas content.
                var renderBitmap = new RenderTargetBitmap((int)SurfaceCanvas.ActualWidth, (int)SurfaceCanvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);

                // Make a drawing visual to render.
                var visual = new DrawingVisual();
                using (DrawingContext context = visual.RenderOpen())
                {
                    context.DrawRectangle(new VisualBrush(SurfaceCanvas), null, new Rect(new Point(0, 0), new Point(SurfaceCanvas.ActualWidth, SurfaceCanvas.ActualHeight)));
                }
                renderBitmap.Render(visual);
                return renderBitmap;
            });
        }


        /// <summary>
        /// Shows the crop image dialog.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sourceFile">The source file.</param>
        private async void ShowCropImageDialog(BitmapSource source = null, string sourceFile = null)
        {
            var imageResult = await _fileService.OpenImageFileCropped(SurfaceWidth, SurfaceHeight, source, sourceFile);
            if (imageResult == null)
                return;

            PaintCanvas.Background = new ImageBrush(imageResult.Image.ToBitmapImage());
            await Task.Delay(500);
            await CreateCanvas();
        }


        /// <summary>
        /// Copies the image.
        /// </summary>
        /// <returns></returns>
        private Task CopyCanvas()
        {
            Clipboard.SetImage(CreateSurfaceImage());
            return Task.CompletedTask;
        }


        /// <summary>
        /// Determines whether this instance can copy canvas.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can copy canvas; otherwise, <c>false</c>.
        /// </returns>
        private bool CanCopyCanvas()
        {
            return Result?.Image != null;
        }


        /// <summary>
        /// Paste the image.
        /// </summary>
        /// <returns></returns>
        private Task PasteCanvas()
        {
            if (Clipboard.ContainsImage())
                ShowCropImageDialog(Clipboard.GetImage());
            else if (Clipboard.ContainsFileDropList())
            {
                var imageFile = Clipboard.GetFileDropList()
                    .OfType<string>()
                    .FirstOrDefault();
                ShowCropImageDialog(null, imageFile);
            }
            return Task.CompletedTask;
        }


        /// <summary>
        /// Undo the last canvas stroke
        /// </summary>
        /// <returns></returns>
        private async Task UndoCanvas()
        {
            if (PaintCanvas.Strokes.Count == 0)
                return;

            var lastStroke = PaintCanvas.Strokes.Last();
            if (PaintCanvas.Strokes.Remove(lastStroke))
            {
                _canvasStrokedRemoved.Add(lastStroke);
                await CreateCanvas();
            }
        }


        /// <summary>
        /// Determines whether this instance can undo last stroke.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can undo last stroke; otherwise, <c>false</c>.
        /// </returns>
        private bool CanUndoCanvas()
        {
            return PaintCanvas.Strokes.Count > 0;
        }


        /// <summary>
        /// Redo the last canvas stroke
        /// </summary>
        /// <returns></returns>
        private async Task RedoCanvas()
        {
            if (_canvasStrokedRemoved.Count == 0)
                return;

            var lastStroke = _canvasStrokedRemoved.Last();
            if (_canvasStrokedRemoved.Remove(lastStroke))
            {
                PaintCanvas.Strokes.Add(lastStroke);
                await CreateCanvas();
            }
        }


        /// <summary>
        /// Determines whether this instance can redo last canvas stroke.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can redo last canvas stroke; otherwise, <c>false</c>.
        /// </returns>
        private bool CanRedoCanvas()
        {
            return _canvasStrokedRemoved.Count > 0;
        }


        /// <summary>
        /// Sets the recent colors.
        /// </summary>
        private void SetRecentColors()
        {
            RecentColors.Add(Colors.Red);
            RecentColors.Add(Colors.Green);
            RecentColors.Add(Colors.Blue);
            RecentColors.Add(Colors.Gray);
            RecentColors.Add(Colors.Yellow);

            RecentColors.Add(Colors.Orange);
            RecentColors.Add(Colors.Brown);
            RecentColors.Add(Colors.Fuchsia);
            RecentColors.Add(Colors.Black);
            RecentColors.Add(Colors.White);

            RecentColors.Add(Colors.Purple);
            RecentColors.Add(Colors.Crimson);
            RecentColors.Add(Colors.Cyan);
            RecentColors.Add(Colors.Magenta);
            RecentColors.Add(Colors.Lime);
            RecentColors.Add(Colors.HotPink);
        }


        /// <summary>
        /// Adds the color to the recent list.
        /// </summary>
        /// <param name="color">The color.</param>
        private void AddRecentColor(Color color)
        {
            if (RecentColors.IsNullOrEmpty())
                return;

            if (RecentColors.Contains(color))
            {
                RecentColors.Move(RecentColors.IndexOf(color), 0);
                return;
            }

            RecentColors.RemoveAt(RecentColors.Count - 1);
            RecentColors.Add(color);
        }


        /// <summary>
        /// Called when SurfaceSize changed.
        /// </summary>
        private async Task OnSurfaceSizeChanged()
        {
            if (SurfaceHeight > 0 && SurfaceWidth > 0)
                await CreateCanvas();

        }


        /// <summary>
        /// Called when Result changed.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static async void OnResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PaintSurfaceControl control && e.NewValue is ImageInput image && image.FileName != "Canvas Image")
            {
                control.PaintCanvas.Background = new ImageBrush(image.Image);
                await Task.Delay(500); // TODO: Fix race condition
                await control.CreateCanvas();
            }
        }


        /// <summary>
        /// Handles the MouseLeftButtonDown event of the PaintCanvas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private async void PaintCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ToolOutlineVisibility = Visibility.Hidden;
            _canvasStrokedRemoved.Clear();
            await CreateCanvas();
        }


        /// <summary>
        /// Handles the MouseLeftButtonUp event of the PaintCanvas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private async void PaintCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ToolOutlineVisibility = Visibility.Visible;
            await CreateCanvas();
            Focus();
        }


        /// <summary>
        /// Handles the PreviewMouseMove event of the PaintCanvas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private async void PaintCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (DateTime.Now > _canvasLastUpdate)
                {
                    _canvasLastUpdate = DateTime.MaxValue;
                    await CreateCanvas();
                    _canvasLastUpdate = DateTime.Now.AddMilliseconds(Settings.RealtimeRefreshRate);
                }
            }
            else
            {
                RenderToolOutline(e);
            }
        }


        /// <summary>
        /// Renders the tool outline.
        /// </summary>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void RenderToolOutline(MouseEventArgs e)
        {
            var pos = e.GetPosition(PaintCanvasToolSurface);
            int posX = (int)(pos.X - (DrawingToolSize / 2d));
            int posY = (int)(pos.Y - (DrawingToolSize / 2d));
            if (posX != _toolOutlineX)
                ToolOutlineX = posX;
            if (posY != _toolOutlineY)
                ToolOutlineY = posY;
        }


        /// <summary>
        /// Handles the OnMouseWheel event of the PaintCanvas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseWheelEventArgs"/> instance containing the event data.</param>
        private void PaintCanvas_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            DrawingToolSize = e.Delta > 0
                ? Math.Min(100, DrawingToolSize + 1)
                : Math.Max(1, DrawingToolSize - 1);

            RenderToolOutline(e);
        }


        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseEnter" /> attached event is raised on this element. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            Focus();
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
                ShowCropImageDialog(null, fileNames.FirstOrDefault());

            var bitmapImage = e.Data.GetData(typeof(BitmapImage)) as BitmapImage;
            if (bitmapImage is not null)
            {
                PaintCanvas.Background = new ImageBrush(bitmapImage);
                await Task.Delay(500);
                await CreateCanvas();
            }
        }


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        #endregion
    }

    public enum PaintDrawingTool
    {
        RoundBrush = 0,
        SquareBrush = 1,
        Highlight = 2,
        Eraser = 3,
    }
}
