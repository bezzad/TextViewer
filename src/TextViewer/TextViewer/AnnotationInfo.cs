using System;
using System.Windows;
using System.Windows.Media;

namespace TextViewer
{
    public sealed class AnnotationInfo : TextInfo
    {
        public AnnotationInfo(string text, bool isRtl, TextStyle style = null)
        : base(text, isRtl, style)
        {
            BorderThickness = 1;
            CornerRadius = 10;
            BubblePeakWidth = 16;
            BubblePeakHeight = 10;
            BorderBrush = Brushes.Teal;
            Styles.FontWeight = FontWeights.Normal;
            Padding = 5;
        }

        public Point BubblePeakPosition { get; set; }
        public double CornerRadius { get; set; }
        public double BubblePeakWidth { get; set; }
        public double BubblePeakHeight { get; set; }
        public Brush BorderBrush { get; set; }
        public double BorderThickness { get; set; }
        public double Padding { get; set; }
        public double MinWidth { get; set; }
        public double MinHeight { get; set; }
        public double MaxWidth { get; set; }
        public double MaxHeight { get; set; }


        public override void SetFormattedText(
            FontFamily fontFamily,
            double fontSize,
            double pixelsPerDip,
            double lineHeight)
        {
            if (Math.Abs(Styles.FontSize) > 0)
                fontSize += Styles.FontSize;

            // Create the initial formatted text string.
            Format = new FormattedText(
                Text,
                Styles.Language,
                Styles.Direction,
                new Typeface(fontFamily, FontStyles.Normal, Styles.FontWeight, FontStretches.Normal),
                fontSize,
                Styles.Foreground,
                pixelsPerDip)
            {
                LineHeight = lineHeight,
                MaxTextWidth = MaxWidth,
                Trimming = TextTrimming.None
            };

            Width = Math.Max(Math.Min(Format.WidthIncludingTrailingWhitespace + (Padding * 2) + BorderThickness * 2, MaxWidth), MinWidth);
            Height = Math.Max(Math.Min(Format.Height + (Padding * 2) + BorderThickness * 2 + BubblePeakHeight, MaxHeight), MinHeight);
        }


        public override DrawingVisual Render()
        {
            var dc = RenderOpen();
            /*
            //                        Width
            //            <------------------------->  
            //    ^                     d
            //    |                    / \
            //    |        b____2____c/3 4\e___5___f
            // H  |       (1                       6)
            // E  |       a                         g
            // I  |       |                         |
            // G  |       |                         |
            // H  |     11|                         |7
            // T  |       |                         |
            //    |       |                         |
            //    |       k                         h
            //   _|_    10(j___________9___________i)8
            //
            var a = new Point(0, CornerRadius);
            var b = new Point(CornerRadius, 0);
            var c = new Point(BubblePeakPosition.X - BubblePeakWidth / 2, 0);
            var d = new Point(BubblePeakPosition.X, BubblePeakPosition.Y);
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
        */
            dc.DrawGeometry(Styles.Background, new Pen(BorderBrush, BorderThickness), new RectangleGeometry(Area) /*pthGeometry*/);
            dc.DrawText(Format, DrawPoint);


            dc.Close();

            return this;
        }
    }
}
