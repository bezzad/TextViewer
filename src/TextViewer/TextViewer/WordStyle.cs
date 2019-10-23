using System.Windows;
using System.Windows.Media;

namespace TextViewer
{
    public class WordStyle
    {
        public FlowDirection Direction { get; set; }
        public TextAlignment TextAlign { get; set; }
        public bool Display { get; set; }
        public double MarginBottom { get; set; }
        public double MarginLeft { get; set; }
        public double MarginRight { get; set; }
        public double MarginTop { get; set; }
        public double FontSize { get; set; }
        public VerticalAlignment VerticalAlign { get; set; }
        public SolidColorBrush Color { get; set; }
        public FontWeight FontWeight { get; set; }
        public string Href { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public ImageSource Image { get; set; }


        public WordStyle(bool isRtl = false)
        {
            Direction = isRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            MarginBottom = MarginLeft = MarginRight = MarginTop = FontSize = Width = Height = 0;
            FontWeight = FontWeights.Normal;
            VerticalAlign = VerticalAlignment.Center;
            Color = Brushes.Black;
            TextAlign = isRtl ? TextAlignment.Right : TextAlignment.Left;
            Display = true;
        }
    }
}
