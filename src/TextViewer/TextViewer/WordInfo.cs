using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace TextViewer
{
    public class WordInfo : IComparable<Point>
    {
        public WordInfo(string text, int offset, WordType type, bool isRtl)
        {
            Text = text;
            Type = type;
            ImageScale = 1;
            OffsetRange = new Range(offset, offset + text.Length - 1);
            ImpressivePaddingPercent = 1; // 20% of word length
            Styles = new Dictionary<StyleType, string>();
            RtlCulture ??= CultureInfo.GetCultureInfo("fa-ir");
            LtrCulture ??= CultureInfo.GetCultureInfo("en-us");

            SetDirection(isRtl);
        }

        public static CultureInfo RtlCulture { get; set; }
        public static CultureInfo LtrCulture { get; set; }
        public const string Rtl = "rtl";
        public const string Ltr = "ltr";

        public WordInfo NextWord { get; set; }
        public WordInfo PreviousWord { get; set; }
        public FormattedText Format { get; set; }
        public Point DrawPoint { get; set; }
        public Rect Area { get; set; }
        public Range OffsetRange { get; protected set; }
        public Paragraph Paragraph { get; set; }
        public Dictionary<StyleType, string> Styles { get; protected set; }
        private double _extraWidth;
        public double ExtraWidth
        {
            get => Type.HasFlag(WordType.Attached) ? 0 : _extraWidth;
            set => _extraWidth = value;
        }
        public double ImpressivePaddingPercent { get; set; }
        public string Text { get; set; }
        public WordType Type { get; set; }
        public double ImageScale { get; set; }
        public double Width => IsImage
            ? double.Parse(Styles[StyleType.Width]) * ImageScale
            : (Format?.WidthIncludingTrailingWhitespace ?? 0) + ExtraWidth;
        public double Height => IsImage
            ? double.Parse(Styles[StyleType.Height]) * ImageScale
            : Format?.Height ?? 0;
        public bool IsImage => Type.HasFlag(WordType.Image) && Styles.ContainsKey(StyleType.Image);
        public bool IsRtl => Styles[StyleType.Direction] == Rtl;
        public int Offset => OffsetRange.Start;


        public void SetDirection(bool isRtl)
        {
            Styles[StyleType.Direction] = isRtl ? Rtl : Ltr;
        }
        public void AddStyles(Dictionary<StyleType, string> styles)
        {
            foreach (var (key, value) in styles)
                Styles[key] = value;
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

        public override string ToString()
        {
            return $"<-- {OffsetRange.Start} \"{Text}\"  {OffsetRange.End} -->";
        }
    }
}