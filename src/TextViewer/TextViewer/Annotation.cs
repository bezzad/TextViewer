using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TextViewer
{
    public sealed class Annotation : Decorator
    {
        private readonly Pen _pen;
        private readonly ScrollViewer _scrollBar;
        private readonly TextBlock _textViewer;

        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(nameof(Padding), typeof(double), typeof(Annotation), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register(nameof(BorderThickness), typeof(double), typeof(Annotation), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(Annotation), new PropertyMetadata(default(Brush)));
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(Annotation), new PropertyMetadata(default(Brush)));
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(Annotation), new PropertyMetadata(default(Brush)));
        public static readonly DependencyProperty BubblePeakWidthProperty = DependencyProperty.Register(nameof(BubblePeakWidth), typeof(double), typeof(Annotation), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(nameof(CornerRadius), typeof(double), typeof(Annotation), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty BubblePeakPositionProperty = DependencyProperty.Register(nameof(BubblePeakPosition), typeof(Point), typeof(Annotation), new PropertyMetadata(default(Point)));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(Annotation), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty TextAlignProperty = DependencyProperty.Register(nameof(TextAlign), typeof(TextAlignment), typeof(Annotation), new PropertyMetadata(default(TextAlignment)));

        public TextAlignment TextAlign
        {
            get => (TextAlignment)GetValue(TextAlignProperty);
            set => SetValue(TextAlignProperty, value);
        }
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public Point BubblePeakPosition
        {
            get => (Point)GetValue(BubblePeakPositionProperty);
            set => SetValue(BubblePeakPositionProperty, value);
        }
        public double CornerRadius
        {
            get => (double)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        public double BubblePeakWidth
        {
            get => (double)GetValue(BubblePeakWidthProperty);
            set => SetValue(BubblePeakWidthProperty, value);
        }
        public Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }
        public Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }
        public Brush BorderBrush
        {
            get => (Brush)GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }
        public double BorderThickness
        {
            get => (double)GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }
        public double Padding
        {
            get => (double)GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }



        public Annotation()
        {
            BorderThickness = 1;
            CornerRadius = 10;
            BubblePeakWidth = 16;
            BorderBrush = Brushes.Teal;
            _pen = new Pen(BorderBrush, BorderThickness);
            _textViewer = new TextBlock() { TextWrapping = TextWrapping.Wrap };
            _scrollBar = new ScrollViewer()
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = _textViewer
            };
            Padding = 5;
            BubblePeakPosition = new Point(CornerRadius + BubblePeakWidth, 0);
            Foreground = Brushes.Teal;
            Background = new SolidColorBrush(Colors.Bisque) { Opacity = 0.97 };
            TextAlign = TextAlignment.Justify;
            Child = _scrollBar;
        }


        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (IsLoaded)
            {
                if (e.Property.Name == nameof(BubblePeakPosition))
                {
                    if (BubblePeakPosition.X < CornerRadius + BubblePeakWidth)
                        BubblePeakPosition = new Point(CornerRadius + BubblePeakWidth,
                            BubblePeakPosition.Y < ActualHeight ? 0 : ActualHeight);

                    if (BubblePeakPosition.X > ActualWidth - CornerRadius - BubblePeakWidth)
                        BubblePeakPosition = new Point(ActualWidth - CornerRadius - BubblePeakWidth,
                            BubblePeakPosition.Y < ActualHeight ? 0 : ActualHeight);
                }
            }

            if (e.Property.Name == nameof(Text) && _textViewer != null)
                _textViewer.Text = Text;
            else if (e.Property.Name == nameof(BorderBrush) && _pen != null)
                _pen.Brush = BorderBrush;
            else if (e.Property.Name == nameof(BorderThickness) && _pen != null)
                _pen.Thickness = BorderThickness;
            else if (e.Property.Name == nameof(FlowDirection) && _textViewer != null)
                _textViewer.FlowDirection = FlowDirection;
            else if (e.Property.Name == nameof(TextAlign) && _textViewer != null)
                _textViewer.TextAlignment = TextAlign;
            else if (e.Property.Name == nameof(Foreground) && _textViewer != null)
                _textViewer.Foreground = Foreground;
            else if (e.Property.Name == nameof(Padding) && _scrollBar != null && _textViewer != null)
                _scrollBar.Margin = _textViewer.Padding = new Thickness(Padding);

        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            //
            //                  d
            //                 / \
            //     b____2____c/3 4\e___5___f
            //    (1                       6)
            //    a                         g
            //    |                         |
            //    |                         |
            //  11|                         |7
            //    |                         |
            //    |                         |
            //    k                         h
            //  10(j___________9___________i)8
            //
            var a = new Point(0, CornerRadius);
            var b = new Point(CornerRadius, 0);
            var c = new Point(BubblePeakPosition.X - BubblePeakWidth / 2, 0);
            var d = new Point(BubblePeakPosition.X, -CornerRadius);
            var e = new Point(BubblePeakPosition.X + BubblePeakWidth / 2, 0);
            var f = new Point(ActualWidth - CornerRadius, 0);
            var g = new Point(ActualWidth, 10);
            var h = new Point(ActualWidth, ActualHeight - CornerRadius);
            var i = new Point(ActualWidth - CornerRadius, ActualHeight);
            var j = new Point(CornerRadius, ActualHeight);
            var k = new Point(0, ActualHeight - CornerRadius);

            var pathSegments = new List<PathSegment>
            {
                new ArcSegment(b, new Size(CornerRadius, CornerRadius), 0, false, SweepDirection.Clockwise, true),
                new LineSegment(c, true),
                new LineSegment(d, true),
                new LineSegment(e, true),
                new LineSegment(f, true),
                new ArcSegment(g, new Size(CornerRadius, CornerRadius), 0, false, SweepDirection.Clockwise, true),
                new LineSegment(h, true),
                new ArcSegment(i, new Size(CornerRadius, CornerRadius), 0, false, SweepDirection.Clockwise, true),
                new LineSegment(j, true),
                new ArcSegment(k, new Size(CornerRadius, CornerRadius), 0, false, SweepDirection.Clockwise, true),
                new LineSegment(a, true)
            };

            var pthFigure = new PathFigure(a, pathSegments, false) { IsFilled = true };
            var transform = BubblePeakPosition.Y > 0 ? new ScaleTransform(1, -1, ActualWidth / 2, ActualHeight / 2) : null; // rotate around x axis 
            var pthGeometry = new PathGeometry(new List<PathFigure> { pthFigure }, FillRule.EvenOdd, transform);
            drawingContext.DrawGeometry(Background, _pen, pthGeometry);
        }
    }
}
