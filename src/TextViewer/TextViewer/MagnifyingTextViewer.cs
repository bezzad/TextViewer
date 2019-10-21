using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TextViewer
{
    public class MagnifyingTextViewer : SelectableTextViewer
    {
        public static readonly DependencyProperty ZoomFactorProperty = DependencyProperty.Register(nameof(MagnifierZoomFactor), typeof(double), typeof(MagnifyingTextViewer), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(nameof(MagnifierLength), typeof(double), typeof(MagnifyingTextViewer), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(nameof(MagnifierStroke), typeof(SolidColorBrush), typeof(MagnifyingTextViewer), new PropertyMetadata(default(SolidColorBrush)));
        public static readonly DependencyProperty MagnifierEnableProperty = DependencyProperty.Register(nameof(MagnifierEnable), typeof(bool), typeof(MagnifyingTextViewer), new PropertyMetadata(default(bool)));
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
        public bool MagnifierEnable
        {
            get => (bool)GetValue(MagnifierEnableProperty);
            set => SetValue(MagnifierEnableProperty, value);
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
        protected Shape MagnifierCircle { get; set; }
        protected Canvas MagnifierPanel { get; set; }
        protected double Radius => MagnifierLength / 2;

        public MagnifyingTextViewer()
        {
            MagnifierZoomFactor = 3; // 3x
            MagnifierLength = 200;
            MagnifierDistanceFromMouse = MagnifierLength;
            MagnifierStroke = Brushes.Teal;
            MagnifierType = MagnifierType.Circle;

            MagnifierPanel = new Canvas
            {
                IsHitTestVisible = false
            };

            Loaded += delegate { Initial(); };
        }

        protected void Initial()
        {
            if (VisualTreeHelper.GetParent(this) is Panel container)
            {
                MagnifierBrush = new VisualBrush(this)
                {
                    ViewboxUnits = BrushMappingMode.Absolute
                };

                if (MagnifierType == MagnifierType.Circle)
                    MagnifierCircle = new Ellipse
                    {
                        Stroke = MagnifierStroke,
                        Width = MagnifierLength,
                        Height = MagnifierLength,
                        Visibility = Visibility.Hidden,
                        Fill = MagnifierBrush
                    };
                else if(MagnifierType == MagnifierType.Rectangle)
                {
                    MagnifierCircle = new Rectangle
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
                    MagnifierCircle = new Rectangle
                    {
                        Stroke = MagnifierStroke,
                        Width = ActualWidth,
                        Height = MagnifierLength,
                        Visibility = Visibility.Hidden,
                        Fill = MagnifierBrush
                    };
                }
                MagnifierPanel.Children.Add(MagnifierCircle);
                container.Children.Add(MagnifierPanel);
                MouseEnter += delegate { if (MagnifierEnable) MagnifierCircle.Visibility = Visibility.Visible; };
                MouseLeave += delegate { MagnifierCircle.Visibility = Visibility.Hidden; };
                MouseMove += ContentPanelOnMouseMove;
            }
        }

        protected void ContentPanelOnMouseMove(object sender, MouseEventArgs e)
        {
            if (MagnifierEnable == false)
                return;

            Mouse.SetCursor(Cursors.Cross);

            var currentMousePosition = e.GetPosition(this);
            var length = MagnifierCircle.ActualWidth * (1 / MagnifierZoomFactor);

            // Determine whether the magnifying glass should be shown to the
            // the left or right of the mouse pointer.
            if (ActualWidth - currentMousePosition.X > MagnifierCircle.Width + MagnifierDistanceFromMouse - Radius)
                SetLeft(MagnifierCircle, currentMousePosition.X + MagnifierDistanceFromMouse - Radius);
            else
                SetLeft(MagnifierCircle, currentMousePosition.X - MagnifierDistanceFromMouse - Radius);

            // Determine whether the magnifying glass should be shown 
            // above or below the mouse pointer.
            if (ActualHeight - currentMousePosition.Y > MagnifierCircle.Height + MagnifierDistanceFromMouse - Radius)
                SetTop(MagnifierCircle, currentMousePosition.Y + MagnifierDistanceFromMouse - Radius);
            else
                SetTop(MagnifierCircle, currentMousePosition.Y - MagnifierDistanceFromMouse - Radius);

            // Update the visual brush's View-box to magnify a `length` by `length` rectangle,
            // centered on the current mouse position.
            ViewBox = new Rect(currentMousePosition.X - length / 2, currentMousePosition.Y - length / 2, length, length);
        }
    }
}
