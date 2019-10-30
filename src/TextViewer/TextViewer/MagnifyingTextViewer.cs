using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TextViewer
{
    public class MagnifyingTextViewer : AnnotationTextViewer
    {
        public static readonly DependencyProperty ZoomFactorProperty = DependencyProperty.Register(nameof(MagnifierZoomFactor), typeof(double), typeof(MagnifyingTextViewer), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(nameof(MagnifierLength), typeof(double), typeof(MagnifyingTextViewer), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(nameof(MagnifierStroke), typeof(SolidColorBrush), typeof(MagnifyingTextViewer), new PropertyMetadata(default(SolidColorBrush)));
        public static readonly DependencyProperty DistanceFromMouseProperty = DependencyProperty.Register(nameof(MagnifierDistanceFromMouse), typeof(double), typeof(MagnifyingTextViewer), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty MagnifierTypeProperty = DependencyProperty.Register(nameof(MagnifierType), typeof(MagnifierType), typeof(MagnifyingTextViewer), new PropertyMetadata(default(MagnifierType)));

        public MagnifierType MagnifierType
        {
            get => (MagnifierType)GetValue(MagnifierTypeProperty);
            set => SetValue(MagnifierTypeProperty, value);
        }
        public double MagnifierDistanceFromMouse
        {
            get => (double)GetValue(DistanceFromMouseProperty);
            set => SetValue(DistanceFromMouseProperty, value);
        }
        public SolidColorBrush MagnifierStroke
        {
            get => (SolidColorBrush)GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }
        public double MagnifierLength
        {
            get => (double)GetValue(RadiusProperty);
            set => SetValue(RadiusProperty, value);
        }
        public double MagnifierZoomFactor
        {
            get => (double)GetValue(ZoomFactorProperty);
            set => SetValue(ZoomFactorProperty, value);
        }

        protected Rect ViewBox
        {
            get => MagnifierBrush.Viewbox;
            set => MagnifierBrush.Viewbox = value;
        }
        protected VisualBrush MagnifierBrush { get; set; }
        protected Shape MagnifierShape { get; set; }
        protected Canvas MagnifierPanel { get; set; }
        protected double Radius => MagnifierLength / 2;
        protected bool MagnifierEnable => MagnifierType != MagnifierType.None;

        public MagnifyingTextViewer()
        {
            MagnifierZoomFactor = 3; // 3x
            MagnifierLength = 200;
            MagnifierDistanceFromMouse = MagnifierLength;
            MagnifierStroke = Brushes.Teal;
            MagnifierType = MagnifierType.None;
            MagnifierPanel = new Canvas
            {
                IsHitTestVisible = false
            };

            Loaded += delegate
            {
                if (VisualTreeHelper.GetParent(this) is Panel container)
                    container.Children.Add(MagnifierPanel);

                Initial();
            };
        }

        protected void Initial()
        {
            if (MagnifierEnable)
            {
                MagnifierBrush = new VisualBrush(this)
                {
                    ViewboxUnits = BrushMappingMode.Absolute
                };

                if (MagnifierType == MagnifierType.Circle)
                    MagnifierShape = new Ellipse
                    {
                        Stroke = MagnifierStroke,
                        Width = MagnifierLength,
                        Height = MagnifierLength,
                        Visibility = Visibility.Hidden,
                        Fill = MagnifierBrush
                    };
                else if (MagnifierType == MagnifierType.Rectangle)
                {
                    MagnifierShape = new Rectangle
                    {
                        Stroke = MagnifierStroke,
                        Width = MagnifierLength,
                        Height = MagnifierLength,
                        Visibility = Visibility.Hidden,
                        Fill = MagnifierBrush
                    };
                }
                else if (MagnifierType == MagnifierType.Sticker)
                {
                    MagnifierShape = new Rectangle
                    {
                        Stroke = MagnifierStroke,
                        Width = ActualWidth,
                        Height = MagnifierLength,
                        Visibility = Visibility.Hidden,
                        Fill = MagnifierBrush
                    };
                }
                MagnifierPanel.Children.Add(MagnifierShape);

                MouseEnter += delegate { if (MagnifierEnable) MagnifierShape.Visibility = Visibility.Visible; };
                MouseLeave += delegate { MagnifierShape.Visibility = Visibility.Hidden; };
                MouseMove += ContentPanelOnMouseMove;
            }
        }

        protected void ContentPanelOnMouseMove(object sender, MouseEventArgs e)
        {
            if (MagnifierEnable == false)
                return;

            Mouse.SetCursor(Cursors.Cross);

            var currentMousePosition = e.GetPosition(this);

            if (MagnifierType != MagnifierType.Sticker)
            {
                // Determine whether the magnifying glass should be shown to the
                // the left or right of the mouse pointer.
                if (ActualWidth - currentMousePosition.X > MagnifierShape.Width + MagnifierDistanceFromMouse - Radius)
                    SetLeft(MagnifierShape, currentMousePosition.X + MagnifierDistanceFromMouse - Radius);
                else
                    SetLeft(MagnifierShape, currentMousePosition.X - MagnifierDistanceFromMouse - Radius);

                // Determine whether the magnifying glass should be shown 
                // above or below the mouse pointer.
                if (ActualHeight - currentMousePosition.Y > MagnifierShape.Height + MagnifierDistanceFromMouse - Radius)
                    SetTop(MagnifierShape, currentMousePosition.Y + MagnifierDistanceFromMouse - Radius);
                else
                    SetTop(MagnifierShape, currentMousePosition.Y - MagnifierDistanceFromMouse - Radius);
            }
            else
            {
                SetLeft(MagnifierShape, 0);
                SetTop(MagnifierShape, 0);
            }

            // Update the visual brush's View-box to magnify a `length` by `length` rectangle,
            // centered on the current mouse position.
            var xLength = MagnifierShape.ActualWidth * (1 / MagnifierZoomFactor);
            var yLength = MagnifierShape.ActualHeight * (1 / MagnifierZoomFactor);
            ViewBox = new Rect(currentMousePosition.X - xLength / 2, currentMousePosition.Y - yLength / 2, xLength, yLength);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property.Name == nameof(MagnifierType))
            {
                Initial();
                if (e.OldValue is MagnifierType omt && omt == MagnifierType.Sticker)
                    Padding = new Thickness(Padding.Left, Padding.Top - MagnifierLength, Padding.Right, Padding.Bottom);
                else if(e.NewValue is MagnifierType nmt && nmt == MagnifierType.Sticker)
                    Padding = new Thickness(Padding.Left, Padding.Top + MagnifierLength, Padding.Right, Padding.Bottom);
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (MagnifierType == MagnifierType.Sticker)
                MagnifierShape.Width = ActualWidth;
        }
    }
}
