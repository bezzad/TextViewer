using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TextViewer
{
    public abstract class BaseTextViewer : Canvas
    {
        protected BaseTextViewer()
        {
            EdgeMode = EdgeMode.Unspecified; // To have crisp drawing, set Aliased
            TextFormattingMode = TextFormattingMode.Ideal;
            TextHintingMode = TextHintingMode.Fixed;
            TextRenderingMode = TextRenderingMode.Aliased; // fix words in zooming quality display

            WordWireFramePen = new Pen(Brushes.Red, 0.7) { DashStyle = DashStyles.Dash };
            ParagraphWireFramePen = new Pen(Brushes.Brown, 0.3) { DashStyle = DashStyles.Solid };
            DrawnWords = new VisualCollection(this);
            PixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            OffsetEmSize = 6;
            FpsEmSize = 12;
        }



        // Provide a required override for the VisualChildrenCount property.
        protected override int VisualChildrenCount => DrawnWords?.Count ?? 0;

        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(nameof(FontSize), typeof(double), typeof(BaseTextViewer), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty LineHeightProperty = DependencyProperty.Register(nameof(LineHeight), typeof(int), typeof(BaseTextViewer), new PropertyMetadata(default(int)));
        public static readonly DependencyProperty IsJustifyProperty = DependencyProperty.Register(nameof(IsJustify), typeof(bool), typeof(BaseTextViewer), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(nameof(Padding), typeof(Thickness), typeof(BaseTextViewer), new PropertyMetadata(default(Thickness)));
        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register(nameof(FontFamily), typeof(FontFamily), typeof(BaseTextViewer), new PropertyMetadata(default(FontFamily)));
        public static readonly DependencyProperty ShowWireFrameProperty = DependencyProperty.Register(nameof(ShowWireFrame), typeof(bool), typeof(BaseTextViewer), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty ShowOffsetProperty = DependencyProperty.Register(nameof(ShowOffset), typeof(bool), typeof(BaseTextViewer), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty ShowFramePerSecondProperty = DependencyProperty.Register(nameof(ShowFramePerSecond), typeof(bool), typeof(BaseTextViewer), new PropertyMetadata(default(bool)));

        public EdgeMode EdgeMode
        {
            get => (EdgeMode)GetValue(RenderOptions.EdgeModeProperty);
            set => SetValue(RenderOptions.EdgeModeProperty, value);
        }
        /// <summary>
        /// Decide which algorithm should be used when formatting the text.
        /// You can choose between Ideal (the default value) and Display.
        /// You would normally want to leave this property untouched,
        /// since the Ideal setting will be best for most situations,
        /// but in cases where you need to render very small text,
        /// the Display setting can sometimes yield a better result.
        /// </summary>
        public TextFormattingMode TextFormattingMode
        {
            get => (TextFormattingMode)GetValue(TextOptions.TextFormattingModeProperty);
            set => SetValue(TextOptions.TextFormattingModeProperty, value);
        }
        /// <summary>
        /// Used for specifying how text should be rendered with respect to animated or static text
        /// </summary>
        public TextHintingMode TextHintingMode
        {
            get => (TextHintingMode)GetValue(TextOptions.TextHintingModeProperty);
            set => SetValue(TextOptions.TextHintingModeProperty, value);
        }
        /// <summary>
        /// Control of which anti-aliasing algorithm is used when rendering text.
        /// It has the biggest effect in combination with the Display setting for the TextFormattingMode property.
        /// </summary>
        public TextRenderingMode TextRenderingMode
        {
            get => (TextRenderingMode)GetValue(TextOptions.TextRenderingModeProperty);
            set => SetValue(TextOptions.TextRenderingModeProperty, value);
        }
        public bool ShowFramePerSecond
        {
            get => (bool)GetValue(ShowFramePerSecondProperty);
            set => SetValue(ShowFramePerSecondProperty, value);
        }
        public bool ShowWireFrame
        {
            get => (bool)GetValue(ShowWireFrameProperty);
            set => SetValue(ShowWireFrameProperty, value);
        }
        public bool ShowOffset
        {
            get => (bool)GetValue(ShowOffsetProperty);
            set => SetValue(ShowOffsetProperty, value);
        }
        public FontFamily FontFamily
        {
            get => (FontFamily)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }
        public int LineHeight
        {
            get => (int)GetValue(LineHeightProperty);
            set => SetValue(LineHeightProperty, value);
        }
        public bool IsJustify
        {
            get => (bool)GetValue(IsJustifyProperty);
            set => SetValue(IsJustifyProperty, value);
        }
        public Thickness Padding
        {
            get => (Thickness)GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }

        public readonly VisualCollection DrawnWords;
        public Pen WordWireFramePen { get; set; }
        public Pen ParagraphWireFramePen { get; set; }
        public double PixelsPerDip { get; set; }
        public double OffsetEmSize { get; set; }
        public double FpsEmSize { get; set; }
        /// <summary>
        /// Note: A margin based on the line-height has been used before this paragraph space.
        /// Default value is 1.6x of line spacing.
        /// </summary>
        public double ParagraphSpace => LineHeight * 0.66;



        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (IsLoaded)
            {
                var visualAffectedProperties = new[]
                {
                    nameof(FontFamily),
                    nameof(FontSize),
                    nameof(WordWireFramePen),
                    nameof(ParagraphWireFramePen),
                    nameof(PixelsPerDip),
                    nameof(OffsetEmSize),
                    nameof(FpsEmSize),
                    nameof(Padding),
                    nameof(IsJustify),
                    nameof(ShowOffset),
                    nameof(ShowWireFrame),
                    nameof(ShowFramePerSecond),
                    nameof(ParagraphSpace),
                    nameof(LineHeight),
                    nameof(EdgeMode),
                    nameof(TextFormattingMode),
                    nameof(TextHintingMode),
                    nameof(TextRenderingMode)
                };

                if (e.NewValue != e.OldValue && visualAffectedProperties.Contains(e.Property.Name))
                    ReRender(); // re-render view
            }
        }

        public virtual void AddDrawnWord(DrawingVisual visual)
        {
            DrawnWords.Add(visual);
        }
        public virtual void RemoveDrawnWord(int index)
        {
            DrawnWords.RemoveAt(index);
        }
        public virtual void ClearDrawnWords()
        {
            DrawnWords.Clear();
        }

        public void ReRender() { InvalidateVisual(); }


        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= DrawnWords.Count)
                throw new ArgumentOutOfRangeException();

            return DrawnWords[index];
        }
    }
}
