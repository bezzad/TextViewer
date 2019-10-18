using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TextViewer
{
    public abstract class BaseTextViewer : Canvas
    {
        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
            "FontSize", typeof(double), typeof(BaseTextViewer), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty LineHeightProperty = DependencyProperty.Register(
            "LineHeight", typeof(int), typeof(BaseTextViewer), new PropertyMetadata(default(int)));
        public static readonly DependencyProperty IsJustifyProperty = DependencyProperty.Register(
            "IsJustify", typeof(bool), typeof(BaseTextViewer), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(
            "Padding", typeof(Thickness), typeof(BaseTextViewer), new PropertyMetadata(default(Thickness)));
        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register(
            "FontFamily", typeof(FontFamily), typeof(BaseTextViewer), new PropertyMetadata(default(FontFamily)));
        public static readonly DependencyProperty ShowWireFrameProperty = DependencyProperty.Register(
            "ShowWireFrame", typeof(bool), typeof(BaseTextViewer), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty ParagraphSpaceProperty = DependencyProperty.Register(
            "ParagraphSpace", typeof(double), typeof(BaseTextViewer), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty ShowOffsetProperty = DependencyProperty.Register(
            "ShowOffset", typeof(bool), typeof(BaseTextViewer), new PropertyMetadata(default(bool)));

        
        public double ParagraphSpace
        {
            get => (double)GetValue(ParagraphSpaceProperty);
            set => SetValue(ParagraphSpaceProperty, value);
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

        public Pen WordWireFramePen { get; set; }
        public Pen ParagraphWireFramePen { get; set; }
        public List<Paragraph> PageContent { get; set; }
        public List<WordInfo> DrawnWords { get; set; }
        public double PixelsPerDip { get; set; }


        protected BaseTextViewer()
        {
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
            WordWireFramePen = new Pen(Brushes.Red, 0.7) { DashStyle = DashStyles.Dash };
            ParagraphWireFramePen = new Pen(Brushes.Brown, 0.3) { DashStyle = DashStyles.Solid };
            DrawnWords = new List<WordInfo>();
            PixelsPerDip = GraphicsHelper.PixelsPerDip(this);
            ParagraphSpace = 10;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            // ignore properties like IsMouseOver and IsMouseDirectlyOver and ...
            if (e.Property.Name.StartsWith("IsMouse") == false)
                Render();

        }

        public virtual void Render() { InvalidateVisual(); }
    }
}
