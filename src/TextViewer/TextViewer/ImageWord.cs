using System.Windows.Media;

namespace TextViewer
{
    public class ImageWord : WordInfo
    {
        public override double Height => Styles.Height * ImageScale;
        public override double Width => Styles.Width * ImageScale + ExtraWidth;

        public ImageWord(int offset, WordStyle style = null)
            : base("img", offset, WordType.Image, false, style)
        {
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
