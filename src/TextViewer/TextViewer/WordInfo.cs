using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace TextViewer
{
    public class WordInfo : DrawingVisual, IComparable<WordInfo>
    {
        public WordInfo(string text, int offset, WordType type, bool isRtl, WordStyle style = null)
        {
            Text = text;
            Type = type;
            Offset = offset;
            Styles = new WordStyle(isRtl, style);
        }

        public static readonly Brush SelectedBrush = new SolidColorBrush(Colors.DarkCyan) { Opacity = 0.5 };
        public static Brush HyperLinkBrush { get; set; } = Brushes.Blue;
        public static TextDecorationCollection HyperLinkDecoration { get; set; } = TextDecorations.Underline;
        public WordInfo NextWord { get; set; }
        public WordInfo PreviousWord { get; set; }
        public FormattedText Format { get; set; }
        public Point DrawPoint { get; set; }
        public Rect Area { get; set; }
        public Paragraph Paragraph { get; set; }
        public WordStyle Styles { get; protected set; }
        public string Text { get; set; }
        public WordType Type { get; set; }
        public bool IsSelected { get; set; }
        public virtual double Width { get; protected set; }
        public virtual double Height { get; protected set; }
        public bool IsImage => Type.HasFlag(WordType.Image);
        public new int Offset { get; }


        public virtual void SetFormattedText(FontFamily fontFamily,
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

        public virtual DrawingVisual Render()
        {
            var dc = RenderOpen();

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
    }
}