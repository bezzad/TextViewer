using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace SvgTextViewer
{
    public class WordInfo : Word, IComparable<Point>
    {
        public WordInfo(string text, int offset, bool isRtl)
            : base(text, offset)
        {
            Styles.Add(StyleType.Direction, new InlineStyle(StyleType.Direction, isRtl ? "rtl" : "ltr"));
        }

        public int CompareTo([AllowNull] Point other)
        {
            var impressivePadding = ImpressivePaddingPercent * Area.Width;
            var wordX = Area.Location.X + impressivePadding;
            var wordXw = Area.Location.X + Area.Width - impressivePadding;
            var wordY = Area.Location.Y;
            var wordYh = Area.Location.Y + Area.Height;
            var mouseX = other.X;
            var mouseY = other.Y;

            if (wordYh < mouseY) return -1;
            if (mouseY < wordY) return 1;
            if (wordXw < mouseX) return -1;
            if (mouseX < wordX) return 1;
            return 0;
        }

        public FormattedText GetFormattedText(FontFamily fontFamily, 
            double fontSize, 
            double pixelsPerDip,
            double lineHeight)
        {
            // Create the initial formatted text string.
            Format = new FormattedText(
                Text,
                IsRtl ? RtlCulture : LtrCulture,
                IsRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                new Typeface(fontFamily, FontStyles.Normal,
                    Styles.ContainsKey(StyleType.FontWeight) ? FontWeights.Bold : FontWeights.Normal,
                    FontStretches.Normal),
                fontSize,
                Styles.ContainsKey(StyleType.Color)
                    ? (SolidColorBrush) new BrushConverter().ConvertFromString(Styles[StyleType.Color].Value)
                    : Brushes.Black,
                pixelsPerDip)
            {
                LineHeight = lineHeight
            };

            SpaceWidth = fontSize * 0.3;
            Width = Format.Width;
            Height = lineHeight;
            return Format;
        }


        public override string ToString()
        {
            return $"<--{OffsetRange.Start}--\"{Text}\"--{OffsetRange.End}-->";
        }
    }
}