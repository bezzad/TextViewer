using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using TextViewer;
using TextViewerSample.Reader;

namespace TextViewerSample
{
    public class Page : IPage
    {
        public Page()
        {
            TextBlocks = new List<Paragraph>();
        }


        private volatile bool _disposed;
        public bool IsDisposed
        {
            get => _disposed;
            protected set => _disposed = value;
        }
        public FlowDirection Direction { get; set; }
        public Position TopPosition { get; set; }
        public Position BottomPosition { get; set; }
        public List<Paragraph> TextBlocks { get; set; }
        public int BlockCount => TextBlocks.Count;
        public double PageWidth { get; set; }
        public double PageHeight { get; set; }
        public double LineHeight { get; set; }
        public double FontSize { get; set; }
        public TextAlignment TextAlign { get; set; }
        public FontFamily FontFamily { get; set; }
        public Thickness PagePadding { get; set; }
        public double ParagraphSpace { get; set; }
        public CultureInfo Language { get; set; }


        public void AddBlock(Paragraph para)
        {
            TextBlocks.Add(para);
        }

        public void AddBlockToTop(Paragraph para)
        {
            TextBlocks.Insert(0, para);
        }

        public Paragraph GetTopBlock()
        {
            return TextBlocks.FirstOrDefault();
        }

        public Paragraph GetBottomBlock()
        {
            return TextBlocks.LastOrDefault();
        }

        public double GetContentHeight()
        {
            double height = 0;
            for (var i = 0; i < BlockCount; i++)
            {
                var atom = TextBlocks[i];
                if (i == 0) // first atom
                {
                    var startLine = atom.GetLineIndex(TopPosition.Offset);
                    var shownLinesCount = atom.Lines.Count - startLine - 1; // start line ... end
                    height += shownLinesCount * LineHeight + ParagraphSpace;
                }
                else if (i == BlockCount - 1) // last atom
                {
                    var endLine = atom.GetLineIndex(BottomPosition.Offset);
                    var shownLinesCount = endLine + 1; // 0 ... end line
                    height += shownLinesCount * LineHeight + ParagraphSpace;
                }
                else // middle atoms
                {
                    height += atom.Lines.Count * LineHeight + ParagraphSpace;
                }
            }

            return height;
        }

        public bool IsLoaded()
        {
            return TextBlocks.Count > 0;
        }

        public bool HasEnoughSpace()
        {
            var contentHeight = GetContentHeight();
            return PageHeight > contentHeight + PagePadding.Bottom + PagePadding.Top;
        }

        public bool Equals(IPage other)
        {
            if (other is Page page)
                return TopPosition.Equals(page.TopPosition);

            return false;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (ReferenceEquals(obj, null)) return false;
            if (obj is IPage other) return Equals(other);
            return false;
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = BlockCount.GetHashCode();
                hashCode = (hashCode * 397) ^ (TopPosition.GetHashCode());
                hashCode = (hashCode * 17) ^ (BottomPosition.GetHashCode());
                return hashCode;
            }
        }

        public int CompareTo(IPage other)
        {
            if (other is Page page)
                return TopPosition.CompareTo(page.TopPosition);

            return -1;
        }

        public static bool operator <(Page left, Page right)
        {
            return left.CompareTo(right) < 0;
        }
        public static bool operator >(Page left, Page right)
        {
            return left.CompareTo(right) > 0;
        }
        public static bool operator <=(Page left, Page right)
        {
            return left.CompareTo(right) <= 0;
        }
        public static bool operator >=(Page left, Page right)
        {
            return left.CompareTo(right) >= 0;
        }
        public static bool operator ==(Page left, Page right)
        {
            return left?.Equals(right) == true;
        }
        public static bool operator !=(Page left, Page right)
        {
            return !left?.Equals(right) == true;
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (disposing)
            {
                IsDisposed = true;
                // Expect thread cross exception
                // Dispatcher?.Invoke(() => ...);
                TextBlocks.Clear(); 
                TextBlocks = null;
            }
        }
        ~Page() => Dispose(false);
    }
}
