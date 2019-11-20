using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace TextViewer
{
    public interface IPage : IDisposable, IEquatable<IPage>, IComparable<IPage>
    {
        FlowDirection Direction { get; set; }
        List<Paragraph> TextBlocks { get; set; }
        int BlockCount { get; }
        double PageWidth { get; set; }
        double PageHeight { get; set; }
        double LineHeight { get; set; }
        double FontSize { get; set; }
        TextAlignment TextAlign { get; set; }
        FontFamily FontFamily { get; set; }
        Thickness PagePadding { get; set; }
        double ParagraphSpace { get; set; }
        CultureInfo Language { set; get; }
        bool IsDisposed { get; }
    }
}
