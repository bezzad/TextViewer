using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace TextViewer
{
    public class Page : IPage
    {
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


        public void AddBlock(Paragraph para, int startLine, int lastLine)
        {
            throw new NotImplementedException();
        }

        public void AddBlockToTop(Paragraph para, int startLine, int lastLine)
        {
            throw new NotImplementedException();
        }

        public Paragraph GetTopBlock()
        {
            throw new NotImplementedException();
        }

        public Paragraph GetBottomBlock()
        {
            throw new NotImplementedException();
        }

        public double GetContentHeight()
        {
            throw new NotImplementedException();
        }

        public bool IsLoaded()
        {
            return TextBlocks.Count > 0;
        }

        public bool HasEnoughSpace()
        {
            throw new NotImplementedException();
        }

        public bool Equals(IPage other)
        {
            return TopPosition.Equals(other?.TopPosition);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (ReferenceEquals(obj, null)) return false;
            if (obj is IPage other) return Equals(other);
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = BlockCount.GetHashCode();
                hashCode = (hashCode * 397) ^ (TopPosition?.GetHashCode() ?? 0);
                hashCode = (hashCode * 17) ^ (BottomPosition?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public int CompareTo(IPage other)
        {
            return TopPosition.CompareTo(other?.TopPosition);
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
                //Dispatcher?.Invoke(() => Blocks.Clear());
            }
        }
        ~Page() => Dispose(false);
    }
}
