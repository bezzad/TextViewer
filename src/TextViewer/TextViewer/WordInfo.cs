using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace TextViewer
{
    public class WordInfo : Word, IComparable<Point>
    {
        public WordInfo NextWord { get; set; }
        public WordInfo PreviousWord { get; set; }


        public WordInfo(string text, int offset, WordType type, bool isRtl)
            : base(text, offset, type)
        {
            SetDirection(isRtl);
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

        public object GetAttribute(StyleType style)
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
            // Create the initial formatted text string.
            Format = new FormattedText(
                Text,
                IsRtl ? RtlCulture : LtrCulture,
                IsRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                new Typeface(fontFamily, FontStyles.Normal, (FontWeight)GetAttribute(StyleType.FontWeight), FontStretches.Normal),
                fontSize,
                (SolidColorBrush)GetAttribute(StyleType.Color),
                pixelsPerDip)
            {
                LineHeight = lineHeight,
                Trimming = TextTrimming.None
            };

            ExtraWidth = 0; // reset extra space
            return Format;
        }


    }
}