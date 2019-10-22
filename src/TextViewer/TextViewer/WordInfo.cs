using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace TextViewer
{
    public class WordInfo : DrawingVisual, IComparable<WordInfo>
    {
        public WordInfo(string text, int offset, WordType type, bool isRtl)
        {
            Text = text;
            Type = type;
            ImageScale = 1;
            OffsetRange = new Range(offset, offset + text.Length - 1);
            Styles = new Dictionary<StyleType, string>();
            SetDirection(isRtl);
        }

        public static readonly Brush SelectedBrush = new SolidColorBrush(Colors.DarkCyan) { Opacity = 0.5 };
        public static readonly CultureInfo RtlCulture = CultureInfo.GetCultureInfo("fa-ir");
        public static readonly CultureInfo LtrCulture = CultureInfo.GetCultureInfo("en-us");
        public static readonly string Rtl = "rtl";
        public static readonly string Ltr = "ltr";

        private double _extraWidth;
        public double ExtraWidth
        {
            get => Type.HasFlag(WordType.Attached) ? 0 : _extraWidth;
            set => _extraWidth = value;
        }
        public WordInfo NextWord { get; set; }
        public WordInfo PreviousWord { get; set; }
        public FormattedText Format { get; set; }
        public Point DrawPoint { get; set; }
        public Rect Area { get; set; }
        public Range OffsetRange { get; protected set; }
        public Paragraph Paragraph { get; set; }
        public Dictionary<StyleType, string> Styles { get; protected set; }
        public string Text { get; set; }
        public WordType Type { get; set; }
        public double ImageScale { get; set; }
        public bool IsSelected { get; set; }
        public double Width => IsImage
            ? (double)GetAttribute(StyleType.Width) * ImageScale + ExtraWidth
            : (Format?.WidthIncludingTrailingWhitespace ?? 0) + ExtraWidth;
        public double Height => IsImage
            ? (double)GetAttribute(StyleType.Height) * ImageScale
            : Format?.Height ?? 0;
        public bool IsImage => Type.HasFlag(WordType.Image);
        public bool IsRtl => Styles[StyleType.Direction] == Rtl;
        public new int Offset => OffsetRange.Start;


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

        public DrawingVisual Render()
        {
            var dc = RenderOpen();

            if (IsImage && GetAttribute(StyleType.Image) is ImageSource img)
                dc.DrawImage(img, Area);
            else if (Type == WordType.Space)
                dc.DrawGeometry(Brushes.Transparent, null, new RectangleGeometry(Area));
            else
                dc.DrawText(Format, DrawPoint);

            if (IsSelected)
                dc.DrawRectangle(SelectedBrush, null, Area);

            dc.Close();

            return this;
        }

        public void Select()
        {
            IsSelected = true;
            Render();
        }

        public void UnSelect()
        {
            if (IsSelected)
            {
                IsSelected = false;
                Render();
            }
        }
        
        public int CompareTo([AllowNull] WordInfo other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            if (Paragraph.Offset > other.Paragraph.Offset) return 1;
            if (Paragraph.Offset < other.Paragraph.Offset) return -1;
            if (Offset > other.Offset) return 1;
            if (Offset < other.Offset) return -1;

            return 0;
        }

        public override string ToString()
        {
            return $"{Offset}/`{Text}`/{OffsetRange.End}";
        }
    }
}