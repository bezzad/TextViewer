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
            
            var center = e.GetPosition(this);
            var length = MagnifierCircle.ActualWidth * (1 / ZoomFactor);
            var radius = length / 2;
            ViewBox = new Rect(center.X - radius, center.Y - radius, length, length);
            MagnifierCircle.SetValue(LeftProperty, center.X - MagnifierCircle.ActualWidth / 2);
            MagnifierCircle.SetValue(TopProperty, center.Y - MagnifierCircle.ActualHeight / 2);
        }
    }
}
