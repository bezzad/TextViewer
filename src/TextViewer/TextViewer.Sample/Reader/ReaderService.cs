using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using MethodTimer;
using TextViewer;

namespace TextViewerSample.Reader
{
    public class ReaderService
    {
        public ReaderService()
        {
            LineHeight = 25;
            PagePadding = new Thickness(0, 8, 0, 0);  // 96 is 1 inch margins.
            ParagraphSpace = LineHeight * 0.6;  // 96 is 1 inch margins.
            ColumnGap = 30;
            FontFamily = new FontFamily("BNazanin");
            FontSize = 16;
            PreviousPagesCache = new Dictionary<Position, Position>(1);
        }


        protected Page CurrentPage { get; set; }
        public double ColumnGap { get; set; }
        public Thickness PagePadding { get; set; }
        public double ParagraphSpace { get; set; }
        public double LineHeight { get; set; }
        public FontFamily FontFamily { get; set; }
        public double FontSize { get; set; }
        public int ChapterCount => ContentProvider.ChapterCount();
        public IContentService ContentProvider { get; set; }
        public Dictionary<Position, Position> PreviousPagesCache { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double PixelsPerDip { get; set; }
        public TextAlignment TextAlign = TextAlignment.Left;


        protected Page GetRawPage(Position pageTopPosition)
        {
            var page = new Page()
            {
                TextAlign = TextAlign,
                Direction = FlowDirection.RightToLeft,
                LineHeight = LineHeight,
                PagePadding = PagePadding,
                ParagraphSpace = ParagraphSpace,
                FontFamily = FontFamily,
                FontSize = FontSize,
                Language = CultureInfo.GetCultureInfo("fa-IR"),
                PageWidth = Width,
                PageHeight = Height,
                TopPosition = pageTopPosition,
                BottomPosition = pageTopPosition
            };

            return page;
        }

        public void Open(string fileName, string decryptionKey = null)
        {
            ContentProvider = new ContentProvider(fileName, true, decryptionKey);
        }

        public IPage BuildNextPage(Page current)
        {
            //         
            //         |            |
            //         |  --------  |
            //         |  --------  |
            //         |  .-------  | --+
            //         |____________|   |
            //          ____________    |
            //         |            |   |
            //         |  -------x  | <-+     x Next page top position for build page forwardly
            //         |  --------  |
            //         |  --------  |
            //         |            |
            //

            // Cache current page top position according by bottom position key
            PreviousPagesCache[current.BottomPosition] = current.TopPosition;

            // create new page top position from end (bottom position) of current page
            var newPageTopPosition = current.BottomPosition;

            if (IsNextAvailable(current))
            {
                if (newPageTopPosition.Offset < ContentProvider.GetParagraph(newPageTopPosition.ChapterIndex, newPageTopPosition.ParagraphId).EndCharOffset)
                {
                    newPageTopPosition.Offset++;
                }
                else if (current.GetBottomBlock().NextParagraph != null)
                {
                    newPageTopPosition.ParagraphId = current.GetBottomBlock().NextParagraph.Offset;
                    newPageTopPosition.Offset = 0;
                }
                else if (newPageTopPosition.ChapterIndex < ChapterCount - 1)
                {
                    newPageTopPosition.ChapterIndex++;
                    newPageTopPosition.ParagraphId = 0;
                    newPageTopPosition.Offset = 0;
                }
            }

            CurrentPage = BuildPageForwardly(newPageTopPosition);
            return CurrentPage;
        }
        public IPage BuildPreviousPage(Page current, double width, double height)
        {
            //         
            //          |            |
            //          |  --------  |
            //          |  --------  |
            //      +-> |  x-------  |  x Previous page bottom position for build page backwardly
            //      |   |____________|
            //      |    ____________
            //      |   |            |
            //      +-- |  -------   |
            //          |  --------  |
            //          |  --------  |
            //          |            |
            //
            // 

            // create new page bottom position from begin (top position) of current page
            var newPageBottomPosition = current.TopPosition;

            if (IsPreviousAvailable(current))
            {
                if (newPageBottomPosition.Offset > ContentProvider.GetParagraph(newPageBottomPosition.ChapterIndex, newPageBottomPosition.ParagraphId).StartCharOffset)
                {
                    newPageBottomPosition.Offset--;
                }
                else if (current.GetBottomBlock().PreviousParagraph != null)
                {
                    var previousPara = current.GetBottomBlock().PreviousParagraph;
                    newPageBottomPosition.ParagraphId = previousPara.Offset;
                    newPageBottomPosition.Offset = previousPara.EndCharOffset;
                }
                else if (newPageBottomPosition.ChapterIndex > 0)
                {
                    newPageBottomPosition.ChapterIndex--;
                    var chapterLastPara = ContentProvider.GetChapterLastParagraph(newPageBottomPosition.ChapterIndex);
                    newPageBottomPosition.ParagraphId = (int)chapterLastPara.Offset;
                    newPageBottomPosition.Offset = chapterLastPara.EndCharOffset;
                }
            }

            // check cache if exist then return rendered previous page
            if (PreviousPagesCache.ContainsKey(newPageBottomPosition))
                return BuildPageForwardly(PreviousPagesCache[newPageBottomPosition]);

            return CurrentPage = BuildPageBackwardly(newPageBottomPosition);
        }

        [Time]
        public Page BuildPageForwardly(Position pageTopPosition)
        {
            var page = GetRawPage(pageTopPosition);
            var actualWidth = page.PageWidth - page.PagePadding.Left - page.PagePadding.Right;
            var actualHeight = page.PageHeight - page.PagePadding.Top - page.PagePadding.Bottom;

            if (page.BottomPosition.ChapterIndex < ContentProvider.ChapterCount())
            {
                var para = ContentProvider.GetParagraph(pageTopPosition.ChapterIndex, page.TopPosition.ParagraphId);

                do
                {
                    //para.Build(actualWidth, FontFamily, FontSize, PixelsPerDip, LineHeight, TextAlign == TextAlignment.Justify);
                    //var firstLineIndex = para.GetLineIndex(page.BottomPosition.Offset);
                    //var lastLineIndex = para.Lines.Count - 1;
                    // //var paraHeight = para.Size.Height - (firstLineIndex * LineHeight) - (lastLineIndex);
                    //para = para.NextParagraph;
                } while (actualHeight > LineHeight && para != null);

            }

            return null;
        }


        [Time]
        public Page BuildPageBackwardly(Position pageBottomPosition)
        {
            return null;
        }



        public bool IsNextAvailable(Page current)
        {
            if (current == null)
                throw new ArgumentNullException(nameof(current));

            if (current.BottomPosition == null)
                throw new ArgumentNullException(nameof(current.BottomPosition));

            var bottomPara = ContentProvider.GetParagraph(current.BottomPosition);
            return bottomPara.NextAtomOffset >= 0 || bottomPara.EndCharOffset > current.BottomPosition.Offset;
        }
        public bool IsPreviousAvailable(Page current)
        {
            if (current == null)
                throw new ArgumentNullException(nameof(current));

            if (current.TopPosition == null)
                throw new ArgumentNullException(nameof(current.TopPosition));


            var topPara = ContentProvider.GetParagraph(current.TopPosition);
            return topPara.PrevAtomOffset >= 0 || current.TopPosition.Offset > topPara.StartCharOffset;
        }
        public bool IsLineForwardAvailable(Page current)
        {
            if (current != null && IsEndOfChapter(current))
            {
                var height = current.GetContentHeight();
                return Height / 2 < height; // the content height is longer than half page
            }

            return true;
        }
        public bool IsLineBackwardAvailable(Page current)
        {
            return current != null && !IsStartOfChapter(current);
        }

        public bool IsStartOfChapter(Page current)
        {
            if (current == null)
                throw new ArgumentNullException(nameof(current));

            if (current.TopPosition == null)
                throw new ArgumentNullException(nameof(current.TopPosition));

            var topPara = ContentProvider.GetParagraph(current.TopPosition);
            return topPara.PrevAtomOffset >= 0 && current.TopPosition.Offset <= topPara.StartCharOffset;
        }
        public bool IsEndOfChapter(Page current)
        {
            if (current == null)
                throw new ArgumentNullException(nameof(current));

            if (current.BottomPosition == null)
                throw new ArgumentNullException(nameof(current.BottomPosition));

            var bottomPara = ContentProvider.GetParagraph(current.BottomPosition);
            return bottomPara.NextAtomOffset >= 0 && bottomPara.EndCharOffset <= current.BottomPosition.Offset;
        }
    }
}
