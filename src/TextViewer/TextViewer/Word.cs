using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace TextViewer
{
    public abstract class Word
    {
        protected Word(string text, int offset)
        {
            Text = text;
            ImageScale = 1;
            OffsetRange = new Range(offset, offset + text.Length - 1);
            ImpressivePaddingPercent = 0.2; // 20% of word length
            Styles = new Dictionary<StyleType, string>();
            RtlCulture ??= CultureInfo.GetCultureInfo("fa-ir");
            LtrCulture ??= CultureInfo.GetCultureInfo("en-us");
        }

        public static CultureInfo RtlCulture { get; set; }
        public static CultureInfo LtrCulture { get; set; }
        public const string Rtl = "rtl";
        public const string Ltr = "ltr";

        public FormattedText Format { get; set; }
        public Point DrawPoint { get; set; }
        public Rect Area { get; set; }
        public Range OffsetRange { get; protected set; }
        public Paragraph Paragraph { get; set; }
        public Dictionary<StyleType, string> Styles { get; protected set; }
        private double _spaceWidth;
        public double SpaceWidth
        {
            get => IsInnerWord ? 0 : _spaceWidth;
            set => _spaceWidth = value;
        }

        public bool IsInnerWord { get; set; }
        public double ImpressivePaddingPercent { get; set; }
        public string Text { get; set; }
        public double ImageScale { get; set; }
        public double Width => IsImage
            ? double.Parse(Styles[StyleType.Width]) * ImageScale
            : Format?.Width ?? 0;
        public double Height => IsImage
            ? double.Parse(Styles[StyleType.Height]) * ImageScale
            : Format?.Height ?? 0;
        public bool IsImage => Text.Equals("img") && Styles.ContainsKey(StyleType.Image);

        public bool IsRtl => Styles[StyleType.Direction] == Rtl;
        public int Offset => OffsetRange.Start;
    }
}