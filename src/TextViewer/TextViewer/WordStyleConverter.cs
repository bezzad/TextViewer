using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace TextViewer
{
    public static class WordStyleConverter
    {
        public static object GetDefaultStyleValue(this WordStyleType style)
        {
            switch (style)
            {
                case WordStyleType.MarginBottom:
                case WordStyleType.MarginLeft:
                case WordStyleType.MarginRight:
                case WordStyleType.MarginTop:
                case WordStyleType.FontSize:
                case WordStyleType.Width:
                case WordStyleType.Height: return 0.0;
                case WordStyleType.FontWeight: return FontWeights.Normal;
                case WordStyleType.VerticalAlign: return VerticalAlignment.Center;
                case WordStyleType.Color: return Brushes.Black;
                case WordStyleType.TextAlign: return TextAlignment.Justify;
                case WordStyleType.Display: return true;
                default: return null;
            }
        }

        public static object ConvertStyle(this WordStyleType style, string value = null)
        {
            if (value == null) return style.GetDefaultStyleValue();

            switch (style)
            {
                case WordStyleType.MarginBottom:
                case WordStyleType.MarginLeft:
                case WordStyleType.MarginRight:
                case WordStyleType.MarginTop:
                case WordStyleType.FontSize:
                case WordStyleType.Width:
                case WordStyleType.Height: return double.TryParse(value, out var d) ? d : 0.0;
                case WordStyleType.FontWeight:
                    return typeof(FontWeights)
                               .GetProperty(value, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Static)?
                               .GetValue(null) ?? FontWeights.Normal;
                case WordStyleType.TextAlign: return Enum.TryParse(value, out TextAlignment ta) ? ta : TextAlignment.Justify;
                case WordStyleType.Display: return !bool.TryParse(value, out var disp) || disp;
                case WordStyleType.VerticalAlign: return Enum.TryParse(value, out VerticalAlignment va) ? va : VerticalAlignment.Center;
                case WordStyleType.Direction: return value.Equals(WordInfo.Ltr, StringComparison.OrdinalIgnoreCase) ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
                case WordStyleType.Color: return new BrushConverter().ConvertFromString(value);
                case WordStyleType.Image: return value.BitmapFromBase64();
                case WordStyleType.Href: return value;
                default: return null;
            }
        }
    }
}
