using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TextViewer
{
    public class MagnifyingTextViewer : SelectableTextViewer
    {
        public static readonly DependencyProperty ZoomFactorProperty = DependencyProperty.Register(
            "ZoomFactor", typeof(double), typeof(MagnifyingTextViewer), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(
            "Radius", typeof(double), typeof(MagnifyingTextViewer), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
            "Stroke", typeof(SolidColorBrush), typeof(MagnifyingTextViewer), new PropertyMetadata(default(SolidColorBrush)));
        public static readonly DependencyProperty MagnifierEnableProperty = DependencyProperty.Register(
            "MagnifierEnable", typeof(bool), typeof(MagnifyingTextViewer), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty DistanceFromMouseProperty = DependencyProperty.Register(
            "DistanceFromMouse", typeof(double), typeof(MagnifyingTextViewer), new PropertyMetadata(default(double)));

        public double DistanceFromMouse
        {
            get => (double) GetValue(DistanceFromMouseProperty);
            set => SetValue(DistanceFromMouseProperty, value);
        }
        public bool MagnifierEnable
        {
            get => (bool) GetValue(MagnifierEnableProperty);
            set => SetValue(MagnifierEnableProperty, value);
        }
        public SolidColorBrush Stroke
        {
            get => (SolidColorBrush)GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }
        public double Radius
        {
            get => (double)GetValue(RadiusProperty);
            set => SetValue(RadiusProperty, value);
        }
        public double ZoomFactor
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
        protected Ellipse MagnifierCircle { get; set; }
        protected Canvas MagnifierPanel { get; set; }


        public MagnifyingTextViewer()
        {
            DistanceFromMouse = 10;
            ZoomFactor = 4; // 3x
            Radius = 100;
            Stroke = Brushes.Teal;

            MagnifierPanel = new Canvas
            {
                IsHitTestVisible = false
            };

            Loaded += delegate
            {
                if (VisualTreeHelper.GetParent(this) is Panel container)
                {
                    MagnifierBrush = new VisualBrush(this)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute
                    };

                    MagnifierCircle = new Ellipse
                    {
                        Stroke = Stroke,
                        Width = 2 * Radius,
                        Height = 2 * Radius,
                        Visibility = Visibility.Hidden,
                        Fill = MagnifierBrush
                    };

                    MagnifierPanel.Children.Add(MagnifierCircle);
                    container.Children.Add(MagnifierPanel);
                    MouseEnter += delegate { if (MagnifierEnable) MagnifierCircle.Visibility = Visibility.Visible; };
                    MouseLeave += delegate { MagnifierCircle.Visibility = Visibility.Hidden; };
                    MouseMove += ContentPanelOnMouseMove;
                }
            };
        }

        protected void ContentPanelOnMouseMove(object sender, MouseEventArgs e)
        {
            if(MagnifierEnable == false)
                return;

            Mouse.SetCursor(Cursors.Cross);

            var currentMousePosition = e.GetPosition(this);
            var length = MagnifierCircle.ActualWidth * (1 / ZoomFactor);
            var radius = length / 2;

            // Determine whether the magnifying glass should be shown to the
            // the left or right of the mouse pointer.
            if (ActualWidth - currentMousePosition.X > MagnifierCircle.Width + DistanceFromMouse)
            {
                Canvas.SetLeft(MagnifierCircle, currentMousePosition.X + DistanceFromMouse);
            }
            else
            {
                Canvas.SetLeft(MagnifierCircle,
                    currentMousePosition.X - DistanceFromMouse - MagnifierCircle.Width);
            }

            // Determine whether the magnifying glass should be shown 
            // above or below the mouse pointer.
            if (ActualHeight - currentMousePosition.Y > MagnifierCircle.Height + DistanceFromMouse)
            {
                Canvas.SetTop(MagnifierCircle, currentMousePosition.Y + DistanceFromMouse);
            }
            else
            {
                Canvas.SetTop(MagnifierCircle,
                    currentMousePosition.Y - DistanceFromMouse - MagnifierCircle.Height);
            }
            // Update the visual brush's Viewbox to magnify a `length` by `length` rectangle,
            // centered on the current mouse position.
            ViewBox = new Rect(currentMousePosition.X - radius, currentMousePosition.Y - radius, length, length);
        }
    }
}
