using System;
using System.Windows;
using System.Windows.Media;

namespace TextViewer
{
    public class TextInfo : DrawingVisual
    {
        public TextInfo(string text, bool isRtl, TextStyle style = null)
        {
            Text = text;
            Styles = new TextStyle(isRtl, style);
        }
        
        
        public FormattedText Format { get; set; }
        public Point DrawPoint { get; set; }
        public Rect Area { get; set; }
        public TextStyle Styles { get; protected set; }
        public string Text { get; set; }
        public virtual double Width { get; protected set; }
        public virtual double Height { get; protected set; }

        public virtual void SetFormattedText(
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
                Trimming = TextTrimming.None
            };

            Width = Format.WidthIncludingTrailingWhitespace;
            Height = Format.Height;
        }

        public virtual DrawingVisual Render()
        {
            var dc = RenderOpen();

            dc.DrawText(Format, DrawPoint);
            dc.DrawGeometry(Brushes.Transparent, null, new RectangleGeometry(Area));

            dc.Close();

            return this;
        }


        public override string ToString()
        {
            return Text;
        }
    }
}
