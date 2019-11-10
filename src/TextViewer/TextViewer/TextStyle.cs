using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TextViewer
{
    public class TextStyle : IDisposable
    {
        public static readonly CultureInfo RtlCulture = CultureInfo.GetCultureInfo("fa-ir");
        public static readonly CultureInfo LtrCulture = CultureInfo.GetCultureInfo("en-us");

        public FlowDirection Direction { get; set; }
        public CultureInfo Language => Direction == FlowDirection.RightToLeft ? RtlCulture : LtrCulture;
        public bool IsRtl => Direction == FlowDirection.RightToLeft;
        public bool IsLtr => !IsRtl;
        public TextAlignment? TextAlign { get; set; }
        public bool Display { get; set; }
        public double MarginBottom { get; set; }
        public double MarginLeft { get; set; }
        public double MarginRight { get; set; }
        public double MarginTop { get; set; }
        public double FontSize { get; set; }
        public VerticalAlignment? VerticalAlign { get; set; }
        public Brush Foreground { get; set; }
        public Brush Background { get; set; }
        public FontWeight FontWeight { get; set; }
        public string HyperRef { get; set; }
        public bool IsHyperLink => !string.IsNullOrEmpty(HyperRef);
        public double Width { get; set; }
        public double Height { get; set; }
        public ImageSource Image { get; set; }


        public TextStyle(bool isRtl, TextStyle style = null)
        {
            MarginBottom = MarginLeft = MarginRight = MarginTop = FontSize = Width = Height = 0;
            FontWeight = FontWeights.Normal;
            Foreground = Brushes.Black;
            Display = true;

            AddStyle(style);
            SetDirection(isRtl);
        }

        public void AddStyle(TextStyle style)
        {
            if (style != null)
                foreach (var prop in typeof(TextStyle).GetProperties().Where(p => p.CanWrite && p.CanRead))
                {
                    var val = prop.GetValue(style);
                    if (val != null)
                        prop.SetValue(this, val);
                }
        }

        public void SetDirection(bool isRtl)
        {
            Direction = isRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        public void SetImage(byte[] bytes)
        {
            // ReSharper disable once ConvertToUsingDeclaration
            using (var stream = new MemoryStream(bytes))
                Image = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        }

        public void SetImage(string base64)
        {
            SetImage(Convert.FromBase64String(base64));
        }

        ~TextStyle()
        {
            Dispose();
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose()
        {
            Background = Foreground = null;
            Image = null;
        }
    }
}
