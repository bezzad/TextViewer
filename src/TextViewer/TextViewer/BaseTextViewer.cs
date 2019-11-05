using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TextViewer
{
    public abstract class BaseTextViewer : Canvas
    {
        // Provide a required override for the VisualChildrenCount property.
        protected override int VisualChildrenCount => DrawnWords?.Count ?? 0;

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
        public static readonly DependencyProperty ShowOffsetProperty = DependencyProperty.Register(
            "ShowOffset", typeof(bool), typeof(BaseTextViewer), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty ShowFramePerSecondProperty = DependencyProperty.Register(
            "ShowFramePerSecond", typeof(bool), typeof(BaseTextViewer), new PropertyMetadata(default(bool)));

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
        public List<Paragraph> PageContent { get; set; }
        public double PixelsPerDip { get; set; }
        public double OffsetEmSize { get; set; }
        public double FpsEmSize { get; set; }

        /// <summary>
        /// Note: A margin based on the line-height has been used before this paragraph space.
        /// Default value is 1.6x of line spacing.
        /// </summary>
        public double ParagraphSpace => LineHeight * 0.66;

        public delegate void MessageEventHandler(object sender, TextViewerEventArgs args);
        public event MessageEventHandler Message;
        protected virtual void OnMessage(string message, MessageType messageType)
        {
            Message?.Invoke(this, new TextViewerEventArgs(message, messageType));
        }


        protected BaseTextViewer()
        {
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
            WordWireFramePen = new Pen(Brushes.Red, 0.7) { DashStyle = DashStyles.Dash };
            ParagraphWireFramePen = new Pen(Brushes.Brown, 0.3) { DashStyle = DashStyles.Solid };
            DrawnWords = new VisualCollection(this);
            PixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            OffsetEmSize = 6;
            FpsEmSize = 12;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            // ignore properties like IsMouseOver and IsMouseDirectlyOver and ...
            if (e.Property.Name.StartsWith("IsMouse") == false)
                ReRender();

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
