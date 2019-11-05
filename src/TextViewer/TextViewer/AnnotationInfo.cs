using System;
using System.Collections.Generic;
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
        public bool DrawRightToLeft { get; set; }


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
                MaxTextWidth = MaxWidth - Padding * 2 - BorderThickness * 2,
                TextAlignment = Styles.TextAlign ?? TextAlignment.Center
            };

            if (lineHeight >= Format.Height)
            {
                Format.MaxTextWidth = 0;
                DrawRightToLeft = Styles.IsRtl;
            }
            else
                DrawRightToLeft = false; // don't need in multiline text

            Width = Math.Max(Math.Min(Format.WidthIncludingTrailingWhitespace + Padding * 2 + BorderThickness * 2, MaxWidth), MinWidth);
            Height = Math.Max(Math.Min(Format.Height + Padding * 2 + BorderThickness * 2 + BubblePeakHeight, MaxHeight), MinHeight);
        }


        public override DrawingVisual Render()
        {
            using (var dc = RenderOpen())
            {
                //                        Width
                //            <------------------------->  
                //    ^                     d = BubblePeakPosition
                //    |                    / \
                //    |        b____2____c/3 4\e___5___f
                // H  |       (1                       6)
                // E  |       a   TTTTTTTTTTTTTTTTTT    g
                // I  |       |   TT              TT    |
                // G  |       |   TT     TEXT     TT    |
                // H  |     11|   TT              TT    |7
                // T  |       |   TT              TT    |
                //    |       |   TTTTTTTTTTTTTTTTTT    |
                //    |       k                         h
                //   _|_    10(j___________9___________i)8
                //
                var a = new Point(Area.X, Area.Y + BubblePeakHeight + CornerRadius);
                var b = new Point(Area.X + CornerRadius, Area.Y + BubblePeakHeight);
                var c = new Point(BubblePeakPosition.X - BubblePeakWidth / 2, Area.Y + BubblePeakHeight);
                var d = BubblePeakPosition;
                var e = new Point(BubblePeakPosition.X + BubblePeakWidth / 2, Area.Y + BubblePeakHeight);
                var f = new Point(Area.X + Width - CornerRadius, Area.Y + BubblePeakHeight);
                var g = new Point(Area.X + Width, Area.Y + BubblePeakHeight + CornerRadius);
                var h = new Point(Area.X + Width, Area.Y + BubblePeakHeight + Height - CornerRadius);
                var i = new Point(Area.X + Width - CornerRadius, Area.Y + BubblePeakHeight + Height);
                var j = new Point(Area.X + CornerRadius, Area.Y + BubblePeakHeight + Height);
                var k = new Point(Area.X, Area.Y + BubblePeakHeight + Height - CornerRadius);

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
                //var transform = BubblePeakPosition.Y > 0 ? new ScaleTransform(1, -1, Width / 2, Height / 2) : null; // rotate around x axis 
                //var pthGeometry = new PathGeometry(new List<PathFigure> { pthFigure }, FillRule.EvenOdd, transform);
                var pthGeometry = new PathGeometry(new List<PathFigure> { pthFigure }, FillRule.EvenOdd, null);

                dc.DrawGeometry(Styles.Background, new Pen(BorderBrush, BorderThickness), pthGeometry);
                dc.DrawText(Format, DrawPoint);
            }

            return this;
        }
    }
}
