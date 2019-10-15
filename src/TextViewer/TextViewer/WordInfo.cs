using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace TextViewer
{
    public class WordInfo : Word, IComparable<Point>
    {
        public WordInfo NextWord { get; set; }
        public WordInfo PreviousWord { get; set; }


        public WordInfo(string text, int offset, bool isRtl, Paragraph para)
            : base(text, offset)
        {
            Paragraph = para;
            Styles.Add(StyleType.Direction, isRtl ? Rtl : Ltr);
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

        public object GetAttribute(StyleType style, bool setDefaultValue = false)
        {
            // read word style first
            if (Styles.ContainsKey(style))
                return style.ConvertStyleType(Styles[style]);

            // if word has not style, then use parent style
            if (Paragraph.Styles.ContainsKey(style))
                return style.ConvertStyleType(Paragraph.Styles[style]);

            return style.ConvertStyleType();
        }

        public FormattedText GetFormattedText(FontFamily fontFamily,
            double fontSize,
            double pixelsPerDip,
            double lineHeight)
        {
            var color = (SolidColorBrush)GetAttribute(StyleType.Color, true);
            var fontWeight = (FontWeight)GetAttribute(StyleType.FontWeight, true);

            // Create the initial formatted text string.
            Format = new FormattedText(
                Text,
                IsRtl ? RtlCulture : LtrCulture,
                IsRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                new Typeface(fontFamily, FontStyles.Normal, fontWeight, FontStretches.Normal),
                fontSize,
                color,
                pixelsPerDip)
            {
                LineHeight = lineHeight,
                Trimming = TextTrimming.None
            };

            // calculating space width by the distinction between this offset and the next word offset.
            var nextWordOffset = NextWord?.Offset ?? OffsetRange.End + 1;
            SpaceWidth = fontSize * 0.3 * (nextWordOffset - OffsetRange.End - 1);
            return Format;
        }

        public void AddStyles(Dictionary<StyleType, string> styles)
        {
            foreach (var style in styles)
                Styles[style.Key] = style.Value;
        }


        public override string ToString()
        {
            return $"<--{OffsetRange.Start}--\"{Text}\"--{OffsetRange.End}-->";
        }
    }
}