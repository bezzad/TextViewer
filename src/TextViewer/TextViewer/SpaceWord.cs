using System.Windows.Media;

namespace TextViewer
{
    public class SpaceWord : WordInfo
    {
        public double ExtraWidth { get; set; }
        private double _width;
        public override double Width
        {
            get => _width + ExtraWidth;
            protected set => _width = value;
        }

        public SpaceWord(int offset, bool isRtl, TextStyle style = null)
            : base(" ", offset, WordType.Space, isRtl, style)
        { }

        public override void SetFormattedText(FontFamily fontFamily,
            double fontSize,
            double pixelsPerDip,
            double lineHeight)
        {
            ExtraWidth = 0; // reset extra space
            Width = fontSize * 0.278;
            Height = lineHeight;
        }

        public override DrawingVisual Render()
        {
            using (var dc = RenderOpen())
                dc.DrawGeometry(IsSelected ? SelectedBrush : Brushes.Transparent, null, new RectangleGeometry(Area));

            return this;
        }
    }
}