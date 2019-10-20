using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace TextViewer
{
    public class WordInfo : DrawingVisual
    {
        public WordInfo(string text, int offset, WordType type, bool isRtl)
        {
            Text = text;
            Type = type;
            ImageScale = 1;
            OffsetRange = new Range(offset, offset + text.Length - 1);
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


        public void SetDirection(bool isRtl)
        {
            Styles[StyleType.Direction] = isRtl ? Rtl : Ltr;
        }
        public void AddStyles(Dictionary<StyleType, string> styles)
        {
            foreach (var (key, value) in styles)
                Styles[key] = value;
        }

        public object GetAttribute(StyleType style)
        {
            // read word style first
            if (Styles?.ContainsKey(style) == true)
                return style.ConvertStyleType(Styles[style]);

            // if word has not style, then use parent style
            if (Paragraph?.Styles?.ContainsKey(style) == true)
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

        public DrawingVisual Render()
        {
            var dc = RenderOpen();

            if (GetAttribute(StyleType.Image) is ImageSource img)
                dc.DrawImage(img, Area);
            else
                dc.DrawText(Format, DrawPoint);

            dc.Close();

            return this;
        }
    }
}