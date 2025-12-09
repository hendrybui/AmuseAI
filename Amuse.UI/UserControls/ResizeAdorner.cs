using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Amuse.UI.UserControls
{
    public class ResizeAdorner : Adorner
    {
        private readonly Border _container;
        private readonly VisualCollection _visuals;
        private readonly Thumb _topLeft, _topRight, _bottomLeft, _bottomRight;
        private Point _startPoint;
        private bool _isDragging;
        private Action _refreshCallback;

        public ResizeAdorner(UIElement adornedElement, Action refreshCallback) : base(adornedElement)
        {
            IsHitTestVisible = true;
            MouseLeftButtonDown += MoveAdorner_MouseLeftButtonDown;
            MouseMove += MoveAdorner_MouseMove;
            MouseLeftButtonUp += MoveAdorner_MouseLeftButtonUp;

            _visuals = new VisualCollection(this);
            _container = adornedElement as Border;
            _topLeft = CreateThumb(Cursors.SizeNWSE);
            _topRight = CreateThumb(Cursors.SizeNESW);
            _bottomLeft = CreateThumb(Cursors.SizeNESW);
            _bottomRight = CreateThumb(Cursors.SizeNWSE);
            _topLeft.DragDelta += (s, e) => Resize(e.HorizontalChange, e.VerticalChange, true, true);
            _topRight.DragDelta += (s, e) => Resize(e.HorizontalChange, e.VerticalChange, false, true);
            _bottomLeft.DragDelta += (s, e) => Resize(e.HorizontalChange, e.VerticalChange, true, false);
            _bottomRight.DragDelta += (s, e) => Resize(e.HorizontalChange, e.VerticalChange, false, false);
            _visuals.Add(_topLeft);
            _visuals.Add(_topRight);
            _visuals.Add(_bottomLeft);
            _visuals.Add(_bottomRight);
            _refreshCallback = refreshCallback;
        }

        protected override int VisualChildrenCount => _visuals.Count;
        protected override Visual GetVisualChild(int index) => _visuals[index];


        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
        }


        protected override Size ArrangeOverride(Size finalSize)
        {
            if (AdornedElement is FrameworkElement adorned)
            {
                double offset = -_topLeft.Width / 2;
                _topLeft.Arrange(new Rect(offset, offset, _topLeft.Width, _topLeft.Height));
                _topLeft.Arrange(new Rect(offset, offset, _topLeft.Width, _topLeft.Height));
                _topRight.Arrange(new Rect(adorned.ActualWidth - offset - _topRight.Width, offset, _topRight.Width, _topRight.Height));
                _bottomLeft.Arrange(new Rect(offset, adorned.ActualHeight - offset - _bottomLeft.Height, _bottomLeft.Width, _bottomLeft.Height));
                _bottomRight.Arrange(new Rect(adorned.ActualWidth - offset - _bottomRight.Width, adorned.ActualHeight - offset - _bottomRight.Height, _bottomRight.Width, _bottomRight.Height));
            }
            return finalSize;
        }


        protected override void OnMouseEnter(MouseEventArgs e)
        {
            Cursor = Cursors.SizeAll;
            //_container.BorderBrush = Brushes.White;
           // _container.BorderThickness = new Thickness(1);
            foreach (var thumb in _visuals.OfType<Thumb>())
            {
                thumb.Visibility = Visibility.Visible;
            }
            base.OnMouseEnter(e);
        }


        protected override void OnMouseLeave(MouseEventArgs e)
        {
            Cursor = null;
           // _container.BorderBrush = null;
           // _container.BorderThickness = new Thickness(0);
            foreach (var thumb in _visuals.OfType<Thumb>())
            {
                thumb.Visibility = Visibility.Hidden;
            }
            base.OnMouseLeave(e);
        }


        private void Resize(double deltaX, double deltaY, bool adjustLeft, bool adjustTop)
        {
            if (AdornedElement is not FrameworkElement element)
                return;

            double newWidth = element.Width + deltaX * (adjustLeft ? -1 : 1);
            double newHeight = element.Height + deltaY * (adjustTop ? -1 : 1);
            newWidth = Math.Max(newWidth, 10);
            newHeight = Math.Max(newHeight, 10);
            if (adjustLeft)
                Canvas.SetLeft(element, Canvas.GetLeft(element) + (element.Width - newWidth));
            if (adjustTop)
                Canvas.SetTop(element, Canvas.GetTop(element) + (element.Height - newHeight));

            element.Width = newWidth;
            element.Height = newHeight;
            InvalidateArrange();

        }


        private void MoveAdorner_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = Mouse.GetPosition(this);
            _isDragging = true;
            CaptureMouse();
            e.Handled = true;
        }


        private void MoveAdorner_MouseMove(object sender, MouseEventArgs e)
        {
            var currentPoint = Mouse.GetPosition(this);
            if (_isDragging && AdornedElement is FrameworkElement element)
            {
                var left = Canvas.GetLeft(element);
                var top = Canvas.GetTop(element);
                var deltaX = _startPoint.X - currentPoint.X;
                var deltaY = _startPoint.Y - currentPoint.Y;
                Canvas.SetLeft(element, left - deltaX);
                Canvas.SetTop(element, top - deltaY);
            }
        }


        private void MoveAdorner_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                ReleaseMouseCapture();
                e.Handled = true;
                _refreshCallback.Invoke();
            }
        }


        private void Thumb_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            _refreshCallback.Invoke();
        }


        private Thumb CreateThumb(Cursor cursor)
        {
            var size = 14.0;
            var ellipseFactory = new FrameworkElementFactory(typeof(Ellipse));
            ellipseFactory.SetValue(Shape.FillProperty, Brushes.White);
            ellipseFactory.SetValue(Shape.StrokeProperty, Brushes.Black);
            ellipseFactory.SetValue(FrameworkElement.WidthProperty, size);
            ellipseFactory.SetValue(FrameworkElement.HeightProperty, size);

            var thumb = new Thumb
            {
                Width = size,
                Height = size,
                Cursor = cursor,
                Visibility = Visibility.Hidden,
                Template = new ControlTemplate(typeof(Thumb))
                {
                    VisualTree = ellipseFactory
                }
            };

            thumb.PreviewMouseUp += Thumb_MouseLeftButtonUp;
            return thumb;
        }

    }
}
