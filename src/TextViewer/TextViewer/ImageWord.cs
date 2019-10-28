using System.Windows.Media;

namespace TextViewer
{
    public class ImageWord : WordInfo
    {
        public double ImageScale { get; set; }
        public override double Height => Styles.Height * ImageScale;
        public override double Width => Styles.Width * ImageScale;

        public ImageWord(int offset, WordStyle style = null)
            : base(null, offset, WordType.Image, false, style)
        {
            ImageScale = 1;
        }

        public override void SetFormattedText(FontFamily fontFamily, double fontSize, double pixelsPerDip, double lineHeight)
        {
            ImageScale = 1;
        }

        public override DrawingVisual Render()
        {
            var dc = RenderOpen();

            dc.DrawImage(Styles.Image, Area);
            dc.DrawGeometry(IsSelected ? SelectedBrush : Brushes.Transparent, null, new RectangleGeometry(Area));

            dc.Close();

            return this;
        }
    }
}
