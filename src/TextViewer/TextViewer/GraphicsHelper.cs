using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TextViewer
{
    public static class GraphicsHelper
    {
        public static BitmapSource BitmapFromBase64(this string b64String)
        {
            var bytes = Convert.FromBase64String(b64String);
            using var stream = new MemoryStream(bytes);
            return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        }


        public static object ConvertStyleType(this StyleType style, string value = null)
        {
            if (value == null) // return default value
                switch (style)
                {
                    case StyleType.MarginBottom:
                    case StyleType.MarginLeft:
                    case StyleType.MarginRight:
                    case StyleType.MarginTop:
                    case StyleType.FontSize:
                    case StyleType.Width:
                    case StyleType.Height: return 0.0;
                    case StyleType.FontWeight: return FontWeights.Normal;
                    case StyleType.VerticalAlign: return VerticalAlignment.Center;
                    case StyleType.Color: return Brushes.Black;
                    case StyleType.TextAlign: return TextAlignment.Justify;
                    case StyleType.Display: return true;
                    default: return null;
                }

            switch (style)
            {
                case StyleType.MarginBottom:
                case StyleType.MarginLeft:
                case StyleType.MarginRight:
                case StyleType.MarginTop:
                case StyleType.FontSize:
                case StyleType.Width:
                case StyleType.Height: return double.TryParse(value, out var d) ? d : 0.0;
                case StyleType.FontWeight:
                    return typeof(FontWeights)
                               .GetProperty(value, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Static)?
                               .GetValue(null) ?? FontWeights.Normal;
                case StyleType.TextAlign: return Enum.TryParse(value, out TextAlignment ta) ? ta : TextAlignment.Justify;
                case StyleType.Display: return !bool.TryParse(value, out var disp) || disp;
                case StyleType.VerticalAlign: return Enum.TryParse(value, out VerticalAlignment va) ? va : VerticalAlignment.Center;
                case StyleType.Direction: return value.Equals(WordInfo.Ltr, StringComparison.OrdinalIgnoreCase) ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
                case StyleType.Color: return new BrushConverter().ConvertFromString(value);
                case StyleType.Image: return value.BitmapFromBase64();
                case StyleType.Href: return value;
                default: return null;
            }
        }
    }
}
