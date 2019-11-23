using System;
using System.Windows;
using System.Windows.Media;

namespace TextViewer
{
    public class WordInfo : TextInfo, IComparable<WordInfo>
    {
        public WordInfo(string text, int offset, WordType type, TextStyle style = null)
        : base(text, style)
        {
            Type = type;
            Offset = offset;
        }

        public static readonly Brush SelectedBrush = new SolidColorBrush(Colors.DarkCyan) { Opacity = 0.5 };
        public static Brush HyperLinkBrush { get; set; } = Brushes.Blue;
        public static TextDecorationCollection HyperLinkDecoration { get; set; } = TextDecorations.Underline;
        public WordInfo NextWord { get; set; }
        public WordInfo PreviousWord { get; set; }
        public Paragraph Paragraph { get; set; }
        public WordType Type { get; set; }
        public bool IsSelected { get; set; }
        public bool IsImage => Type.HasFlag(WordType.Image);
        public new int Offset { get; }


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
                Styles.IsHyperLink ? HyperLinkBrush : Styles.Foreground,
                pixelsPerDip)
            {
                LineHeight = lineHeight,
                Trimming = TextTrimming.None
            };

            if (Styles.IsHyperLink && HyperLinkDecoration != null)
                Format.SetTextDecorations(HyperLinkDecoration);

            Width = Format.Width;
            Height = lineHeight;
        }

        public override DrawingVisual Render()
        {
            using (var dc = RenderOpen())
            {
                dc.DrawText(Format, DrawPoint);
                dc.DrawGeometry(IsSelected ? SelectedBrush : Brushes.Transparent, null, new RectangleGeometry(Area));
            }

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

        public int CompareTo(WordInfo other)
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
    }
}