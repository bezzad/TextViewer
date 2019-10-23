using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TextViewer
{
    public class WordInfo : DrawingVisual, IComparable<WordInfo>
    {
        public WordInfo(string text, int offset, WordType type, bool isRtl)
        {
            Text = text;
            Type = type;
            ImageScale = 1;
            Offset = offset;
            Styles = new Dictionary<WordStyleType, object>();
            SetDirection(isRtl);
        }

        public static readonly Brush SelectedBrush = new SolidColorBrush(Colors.DarkCyan) { Opacity = 0.5 };
        public static readonly CultureInfo RtlCulture = CultureInfo.GetCultureInfo("fa-ir");
        public static readonly CultureInfo LtrCulture = CultureInfo.GetCultureInfo("en-us");

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
        public Paragraph Paragraph { get; set; }
        public Dictionary<WordStyleType, object> Styles { get; protected set; }
        public string Text { get; set; }
        public WordType Type { get; set; }
        public double ImageScale { get; set; }
        public bool IsSelected { get; set; }
        public double Width => IsImage
            ? (double)GetAttribute(WordStyleType.Width) * ImageScale + ExtraWidth
            : (Format?.WidthIncludingTrailingWhitespace ?? 0) + ExtraWidth;
        public double Height => IsImage
            ? (double)GetAttribute(WordStyleType.Height) * ImageScale
            : Format?.Height ?? 0;
        public bool IsImage => Type.HasFlag(WordType.Image);
        public bool IsRtl => (FlowDirection)GetAttribute(WordStyleType.Direction) == FlowDirection.RightToLeft;
        public new int Offset { get; }


        public void SetDirection(bool isRtl)
        {
            Styles[WordStyleType.Direction] = isRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        public void AddStyles(Dictionary<WordStyleType, object> styles)
        {
            if (styles != null)
                foreach (var (key, value) in styles)
                    Styles[key] = value;
        }

        public object GetAttribute(WordStyleType style)
        {
            // read word style first
            if (Styles?.ContainsKey(style) == true)
                return ConvertStyle(style, Styles[style]);

            // if word has not style, then use parent style
            if (Paragraph?.Styles?.ContainsKey(style) == true)
                return ConvertStyle(style, Paragraph.Styles[style]);

            return ConvertStyle(style);
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
                (FlowDirection)GetAttribute(WordStyleType.Direction),
                new Typeface(fontFamily, FontStyles.Normal, (FontWeight)GetAttribute(WordStyleType.FontWeight), FontStretches.Normal),
                fontSize,
                (SolidColorBrush)GetAttribute(WordStyleType.Color),
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

            if (IsImage && GetAttribute(WordStyleType.Image) is ImageSource img)
                dc.DrawImage(img, Area);
            else
                dc.DrawText(Format, DrawPoint);

            dc.DrawGeometry(IsSelected ? SelectedBrush : Brushes.Transparent, null, new RectangleGeometry(Area));

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
            return $"{Offset}/`{Text}`/{Offset + Text.Length - 1}";
        }

        public object ConvertStyle(WordStyleType style, object value = null)
        {
            switch (style)
            {
                case WordStyleType.MarginBottom:
                case WordStyleType.MarginLeft:
                case WordStyleType.MarginRight:
                case WordStyleType.MarginTop:
                case WordStyleType.FontSize:
                case WordStyleType.Width:
                case WordStyleType.Height: return value is double d ? d : 0.0;
                case WordStyleType.FontWeight: return value is FontWeight fw ? fw : FontWeights.Normal;
                case WordStyleType.TextAlign: return value is TextAlignment ta ? ta : TextAlignment.Justify;
                case WordStyleType.Display: return !(value is bool tf) || tf;
                case WordStyleType.VerticalAlign: return value is VerticalAlignment va ? va : VerticalAlignment.Center;
                case WordStyleType.Direction: return value is FlowDirection flow ? flow : Paragraph.IsRtlDirection ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                case WordStyleType.Color: return value is string color ? new BrushConverter().ConvertFromString(color) : Brushes.Black;
                case WordStyleType.Href: return value;
                case WordStyleType.Image: return value is byte[] bytes ? CreateImage(bytes) : null;

                default: return null;
            }
        }


        protected ImageSource CreateImage(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        }
    }
}