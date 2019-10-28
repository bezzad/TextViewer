using System.Collections.Generic;
using System.Windows.Media;

namespace TextViewer
{
    public class SpaceWord : WordInfo
    {
        public static Dictionary<double, FormattedText> EveryFontSizeSpaces { get; private set; }


        public SpaceWord(int offset, bool isRtl, WordStyle style = null)
            : base(" ", offset, WordType.Space, isRtl, style)
        {
            EveryFontSizeSpaces ??= new Dictionary<double, FormattedText>();
        }



        public override FormattedText GetFormattedText(FontFamily fontFamily,
            double fontSize,
            double pixelsPerDip,
            double lineHeight)
        {
            //ExtraWidth = 0; // reset extra space

            //if (EveryFontSizeSpaces.ContainsKey(GetHashCode(fontSize, pixelsPerDip, lineHeight)))
            //    return EveryFontSizeSpaces[GetHashCode(fontSize, pixelsPerDip, lineHeight)];

            //return EveryFontSizeSpaces[GetHashCode(fontSize, pixelsPerDip, lineHeight)] = base.GetFormattedText(fontFamily, fontSize, pixelsPerDip, lineHeight);

            return base.GetFormattedText(fontFamily, fontSize, pixelsPerDip, lineHeight);
        }

        public override DrawingVisual Render()
        {
            var dc = RenderOpen();
            dc.DrawGeometry(IsSelected ? SelectedBrush : Brushes.Transparent, null, new RectangleGeometry(Area));
            dc.Close();

            return this;
        }

        public int GetHashCode(double fontSize, double ppd, double lineHeight)
        {
            unchecked // Overflow is fine, just wrap
            {
                var hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + fontSize.GetHashCode();
                hash = hash * 23 + ppd.GetHashCode();
                hash = hash * 23 + lineHeight.GetHashCode();
                return hash;
            }
        }
    }
}