using Amuse.UI.Commands;
using Amuse.UI.Models;
using Amuse.UI.Models.StableDiffusion;
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
    public partial class PaintInputControl : UserControl, INotifyPropertyChanged
    {
        private readonly IFileService _fileService;
        private readonly List<Stroke> _canvasStrokedRemoved = new List<Stroke>();

        private int _drawingToolSize;
        public DateTime _canvasLastUpdate;
        private DrawingAttributes _drawingAttributes;
        private Color _selectedColor = Colors.Black;
        private InkCanvasEditingMode _canvasEditingMode = InkCanvasEditingMode.Ink;
        private DrawingTool _canvasDrawingTool;
        private bool _isPickerOpen;
        private Color _previousColor = Colors.Red;
        private ObservableCollection<Color> _recentColors;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaintInputControl" /> class.
        /// </summary>
        public PaintInputControl()
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
            SelectToolCommand = new AsyncRelayCommand<DrawingTool>(SelectDrawingTool);
            InitializeComponent();
            SetRecentColors();
            SelectedColor = Colors.Gray;
        }

        public AsyncRelayCommand LoadCanvasCommand { get; }
        public AsyncRelayCommand ClearCanvasCommand { get; }
        public AsyncRelayCommand FillCanvasCommand { get; }
        public AsyncRelayCommand CopyCanvasCommand { get; }
        public AsyncRelayCommand PasteCanvasCommand { get; }
        public AsyncRelayCommand SaveCanvasCommand { get; }
        public AsyncRelayCommand UndoCanvasCommand { get; }
        public AsyncRelayCommand RedoCanvasCommand { get; }
        public AsyncRelayCommand<DrawingTool> SelectToolCommand { get; }


        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings", typeof(AmuseSettings), typeof(PaintInputControl));

        public static readonly DependencyProperty SchedulerOptionsProperty =
            DependencyProperty.Register("SchedulerOptions", typeof(SchedulerOptionsModel), typeof(PaintInputControl), new PropertyMetadata(OnSchedulerOptionsChanged));

        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register("Result", typeof(ImageInput), typeof(PaintInputControl), new PropertyMetadata(OnResultChanged));

        public static readonly DependencyProperty ZoomDirectionProperty =
            DependencyProperty.Register("ZoomDirection", typeof(StretchDirection), typeof(PaintInputControl), new PropertyMetadata(StretchDirection.Both));

        public AmuseSettings Settings
        {
            get { return (AmuseSettings)GetValue(SettingsProperty); }
            set { SetValue(SettingsProperty, value); }
        }

        public SchedulerOptionsModel SchedulerOptions
        {
            get { return (SchedulerOptionsModel)GetValue(SchedulerOptionsProperty); }
            set { SetValue(SchedulerOptionsProperty, value); }
        }

        public ImageInput Result
        {
            get { return (ImageInput)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
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

        public DrawingTool CanvasDrawingTool
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
            await _fileService.SaveAsImageFile(Result.Image.ToBitmapImage());
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
            BrushAttributes = new DrawingAttributes
            {
                Color = _selectedColor,
                Height = _drawingToolSize,
                Width = _drawingToolSize
            };

            if (CanvasDrawingTool == DrawingTool.Highlight)
                BrushAttributes.Color = Color.FromArgb(128, BrushAttributes.Color.R, BrushAttributes.Color.G, BrushAttributes.Color.B);

            CanvasEditingMode = CanvasDrawingTool != DrawingTool.Eraser
                ? InkCanvasEditingMode.Ink
                : InkCanvasEditingMode.EraseByPoint;
        }


        /// <summary>
        /// Selects the drawing tool.
        /// </summary>
        /// <param name="selectedTool">The selected tool.</param>
        /// <returns></returns>
        private Task SelectDrawingTool(DrawingTool selectedTool)
        {
            CanvasDrawingTool = selectedTool;
            UpdateBrushAttributes();
            return Task.CompletedTask;
        }


        /// <summary>
        /// Creates the canvas image.
        /// </summary>
        /// <returns></returns>
        public BitmapSource CreateCanvasImage()
        {
            return App.Current.Dispatcher.Invoke<BitmapSource>(() =>
            {
                if (PaintCanvas.ActualWidth == 0)
                    return Utils.CreateEmptyBitmapImage(SchedulerOptions.Width, SchedulerOptions.Height);

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
        /// Shows the crop image dialog.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sourceFile">The source file.</param>
        private async void ShowCropImageDialog(BitmapSource source = null, string sourceFile = null)
        {
            var imageResult = await _fileService.OpenImageFileCropped(SchedulerOptions.Width, SchedulerOptions.Height, source, sourceFile);
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
            Clipboard.SetImage(Result.Image);
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
        /// Called when settings changed.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static async void OnSchedulerOptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PaintInputControl control)
            {
                await control.CreateCanvas();
            }
        }


        /// <summary>
        /// Called when Result changed.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static async void OnResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PaintInputControl control && e.NewValue is ImageInput image && image.FileName != "Canvas Image")
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
        protected override void OnPreviewDrop(DragEventArgs e)
        {
            var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (!fileNames.IsNullOrEmpty())
                ShowCropImageDialog(null, fileNames.FirstOrDefault());
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        #endregion
    }

    public enum DrawingTool
    {
        Brush = 0,
        Highlight = 1,
        Eraser = 2,
    }
}
