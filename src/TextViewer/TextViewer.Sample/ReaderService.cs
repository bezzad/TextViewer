using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using MethodTimer;
using TextViewer;

namespace TextViewerSample
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
            Chapters = new List<List<Paragraph>>();
            PreviousPagesCache = new Dictionary<Position, Position>(1);
        }


        protected IPage CurrentPage { get; set; }

        public double ColumnGap { get; set; }
        public Thickness PagePadding { get; set; }
        public double ParagraphSpace { get; set; }
        public double LineHeight { get; set; }
        public FontFamily FontFamily { get; set; }
        public double FontSize { get; set; }
        public int ChapterCount => Chapters.Count;
        public List<List<Paragraph>> Chapters { get; set; }
        public Dictionary<Position, Position> PreviousPagesCache { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public TextAlignment TextAlign = TextAlignment.Left;


        protected IPage GetRawPage(Position topAtom, double width, double height)
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
                PageWidth = width,
                PageHeight = height,
                TopPosition = (Position)topAtom.Clone(),
                BottomPosition = (Position)topAtom.Clone()
            };

            return page;
        }

        public void Open(string fileName, string decryptionKey = null)
        {
            Chapters = new List<List<Paragraph>>() { Path.Combine(fileName).GetParagraphs(true) };
        }

        public IPage BuildNextPage(IPage current, double width, double height)
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
            var topPos = (Position)current.BottomPosition.Clone();

            if (IsNextAvailable(current.BottomPosition))
            {
                if (topPos.Offset < Chapters[topPos.ChapterIndex][topPos.ParagraphId].EndCharOffset)
                {
                    topPos.Offset++;
                }
                else if (topPos.ParagraphId < Chapters[topPos.ChapterIndex].LastOrDefault()?.Offset)
                {
                    topPos.ParagraphId++;
                    topPos.Offset = 0;
                }
                else if (topPos.ChapterIndex < ChapterCount - 1)
                {
                    topPos.ChapterIndex++;
                    topPos.ParagraphId = 0;
                    topPos.Offset = 0;
                }
            }

            CurrentPage = BuildPageForwardly(topPos, width, height);
            return CurrentPage;
        }
        public IPage BuildPreviousPage(IPage current, double width, double height)
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
            var bottomPos = (Position)current.TopPosition.Clone();

            if (IsPreviousAvailable(current.TopPosition))
            {
                if (bottomPos.Offset > 0)
                {
                    bottomPos.Offset--;
                }
                else if (bottomPos.ParagraphId > 0)
                {
                    bottomPos.ParagraphId--;
                    bottomPos.Offset = Chapters[bottomPos.ChapterIndex][bottomPos.ParagraphId].EndCharOffset;
                }
                else if (bottomPos.ChapterIndex > 0)
                {
                    bottomPos.ChapterIndex--;
                    bottomPos.ParagraphId = Chapters[bottomPos.ChapterIndex].Last().Offset;
                    bottomPos.Offset = Chapters[bottomPos.ChapterIndex][bottomPos.ParagraphId].EndCharOffset;
                }
            }

            // check cache if exist then return rendered previous page
            if (PreviousPagesCache.ContainsKey(bottomPos))
                return BuildPageForwardly(PreviousPagesCache[bottomPos], width, height);

            return CurrentPage = BuildPageBackwardly(bottomPos, width, height);
        }

        [Time] // TODO: clear time after test
        public IPage BuildPageForwardly(Position pos, double width, double height)
        {
            // changing width or height caused to clear the cache
            if (!Width.Equals(width) || !Height.Equals(height))
            {
                PreviousPagesCache.Clear();
                Width = width;
                Height = height;
            }

            var page = GetRawPage(pos, width, height);
            var actualWidth = width - (page.PagePadding.Left + page.PagePadding.Right);
            var actualHeight = height - (page.PagePadding.Top + page.PagePadding.Bottom);

            // split to some paragraphs
            if (page.BottomPosition.ChapterIndex < ChapterCount)
            {
                var chapter = Chapters[page.BottomPosition.ChapterIndex];
                while (page.BottomPosition.ParagraphId < chapter.Count && actualHeight > LineHeight)
                {
                    var para = chapter[page.BottomPosition.ParagraphId]; 
                    para.SetFontRendering(FontInfo);
                    if (para.CreateLines(actualWidth, LineHeight).Any())
                    {
                        var firstLineIndex = para.GetLineIndex(page.BottomPosition.Offset);
                        var lastLineIndex = para.Lines.Count - 1;
                        var atomHeight = (lastLineIndex - firstLineIndex + 1) * LineHeight;

                        if (actualHeight > atomHeight) // has space for current atom
                        {
                            page.AddBlock(para, firstLineIndex, lastLineIndex);
                            actualHeight -= atomHeight + ParagraphMargin.Bottom + ParagraphMargin.Top; // - the margin of between every paragraph
                            page.BottomPosition.Offset = para.GetLineEndOffset(lastLineIndex); // keep page bottom position in from atom lines

#if DEBUG // TODO: Remove after writing unit-test for this methods
                            Debug.Assert(page.Paginator.HasEnoughSpace(), $"Page Count is {page.Paginator.PageCount}!");
#endif
                        }
                        else // has no space for current atom, break to atom some lines
                        {
                            if (actualHeight > LineHeight) // has enough space at least for one line.
                            {
                                // find last offset that doesn't succeeds remainingHeight
                                var lineCount = (int)(actualHeight / LineHeight);
                                lastLineIndex = firstLineIndex + lineCount - 1;
                                if (lastLineIndex > para.Lines.Count - 1)
                                    lastLineIndex = para.Lines.Count - 1;
                                page.AddBlock(para, firstLineIndex, lastLineIndex);
                                page.BottomPosition.Offset = para.GetLineEndOffset(lastLineIndex);
                            }
#if DEBUG // TODO: Remove after writing unit-test for this methods
                            Debug.Assert(page.Paginator.HasEnoughSpace(), $"Page Count is {page.Paginator.PageCount}!");
#endif
                            return page;
                        }
                    }

                    // if has enough space to new line and is not last paragraph of chapter, go to next atom
                    if (page.BottomPosition.ParagraphId + 1 < chapter.Count && actualHeight > LineHeight)
                    {
                        page.BottomPosition.ParagraphId++;
                        page.BottomPosition.Offset = 0; // reset for next atom in current page
                    }
                    else
                        break;
                }
            }

            return page;
        }
        [Time] // TODO: clear time after test
        public IPage BuildPageBackwardly(Position pos, double width, double height)
        {
            var page = GetRawPage(pos, width, height);
            var actualWidth = width - (page.PagePadding.Left + page.PagePadding.Right);
            var actualHeight = height - (page.PagePadding.Top + page.PagePadding.Bottom);

            // split to some paragraphs
            if (page.TopPosition.ChapterIndex >= 0)
            {
                var chapter = Chapters[page.TopPosition.ChapterIndex];
                while (page.TopPosition.ParagraphId >= 0 && actualHeight > LineHeight)
                {
                    var atom = chapter[page.TopPosition.ParagraphId];
                    atom.SetFontRendering(FontInfo);
                    if (atom.CreateLines(actualWidth, LineHeight).Any())
                    {
                        var firstLineIndex = 0;
                        var lastLineIndex = atom.GetLineIndex(page.TopPosition.Offset);
                        var atomHeight = (lastLineIndex - firstLineIndex + 1) * LineHeight;

                        if (actualHeight > atomHeight) // has space for current atom
                        {
                            page.AddBlockToTop(atom, firstLineIndex, lastLineIndex);
                            actualHeight -= atomHeight + ParagraphMargin.Bottom + ParagraphMargin.Top; // - the margin of between every paragraph
                            page.TopPosition.Offset = atom.GetLineStartOffset(firstLineIndex); // keep page top position in from atom lines

#if DEBUG // TODO: Remove after writing unit-test for this methods
                            Debug.Assert(page.Paginator.HasEnoughSpace(), $"Page Count is {page.Paginator.PageCount}!");
#endif
                        }
                        else // has no space for current atom, break to atom some lines
                        {
                            if (actualHeight > LineHeight) // has enough space at least for one line.
                            {
                                // find last offset that doesn't succeeds remainingHeight
                                var lineCount = (int)(actualHeight / LineHeight);
                                firstLineIndex = lastLineIndex - (lineCount - 1);
                                if (firstLineIndex < 0) firstLineIndex = 0;
                                page.AddBlockToTop(atom, firstLineIndex, lastLineIndex);
                                page.TopPosition.Offset = atom.GetLineStartOffset(firstLineIndex);
                            }

#if DEBUG // TODO: Remove after writing unit-test for this methods
                            Debug.Assert(page.Paginator.HasEnoughSpace(), $"Page Count is {page.Paginator.PageCount}!");
#endif
                            return page;
                        }
                    }

                    // if has enough space to new line and is not last paragraph of chapter, go to next atom
                    if (actualHeight > LineHeight)
                    {
                        if (page.TopPosition.ParagraphId - 1 >= 0)
                        {
                            page.TopPosition.ParagraphId--;
                            page.TopPosition.Offset = chapter[page.TopPosition.ParagraphId].Content.Length - 1;
                        }
                        else // This point is the start of the chapter. So must build from first of chapter till which is enough space to the atom.
                            return BuildPageForwardly(page.TopPosition, width, height);
                    }
                    else
                        break;
                }
            }
            return page;
        }

        [Time] // TODO: clear time after test
        public IPage LineForward(IPage current)
        {
            var topPos = current.TopPosition.Clone() as Position;

            if (((Page)current).TextBlocks.FirstOrDefault() is Atom topAtom &&
                topPos?.ParagraphId == topAtom.GetOffset())
            {
                var currentTopLineIndex = topAtom.GetLineIndex(topPos.Offset);
                //
                // is not last line of top paragraph?
                if (currentTopLineIndex < topAtom.Lines.Count - 1)
                {
                    topPos.Offset = topAtom.GetLineStartOffset(currentTopLineIndex + 1); // go next line
                }
                // is not last atom of current chapter?
                else if (topPos.ParagraphId < Chapters[topAtom.ChapterIndex].Count - 1)
                {
                    topPos.ParagraphId++;
                    topPos.Offset = 0;
                }
            }
#if DEBUG
            else
            {
                Debug.WriteLine("Why top position ParagraphId != page top block Id ?");
                Debugger.Break();
            }
#endif
            PreviousPagesCache.Clear();
            return BuildPageForwardly(topPos, Width, Height);
        }
        [Time] // TODO: clear time after test
        public IPage LineBackward(IPage current)
        {
            var topPos = current.TopPosition.Clone() as Position;

            if (((Page)current).TextBlocks.FirstOrDefault() is Atom topAtom &&
                topPos?.ParagraphId == topAtom.GetOffset())
            {
                var currentTopLineIndex = topAtom.GetLineIndex(topPos.Offset);
                //
                // is not first line of top paragraph?
                if (currentTopLineIndex > 0)
                {
                    topPos.Offset = topAtom.GetLineStartOffset(currentTopLineIndex - 1); // go next line
                }
                // is not last atom of current chapter?
                else if (topPos.ParagraphId > 0)
                {
                    topPos.ParagraphId--;
                    topPos.Offset = Chapters[topAtom.ChapterIndex][topPos.ParagraphId].Content.Length - 1;
                }
            }

            PreviousPagesCache.Clear();
            return BuildPageForwardly(topPos, Width, Height);
        }

        public bool IsNextAvailable(Position current)
        {
            var chapterIndex = current?.ChapterIndex ?? 0;
            var bottomParagraphId = current?.ParagraphId ?? 0;
            var bottomAtomOffset = current?.Offset ?? 0;
            var currentChapter = Chapters?.Any() == true ? Chapters[chapterIndex] : null;

            return (chapterIndex < ChapterCount - 1 ||
                    bottomParagraphId < currentChapter?.Count - 1 ||
                    bottomAtomOffset < currentChapter?.LastOrDefault()?.Content.Length - 1);
        }
        public bool IsPreviousAvailable(Position current)
        {
            var chapterIndex = current?.ChapterIndex ?? 0;
            var topParagraphId = current?.ParagraphId ?? 0;
            var topAtomOffset = current?.Offset ?? 0;

            return (chapterIndex > 0 || topParagraphId > 0 || topAtomOffset > 0);
        }
        public bool IsLineForwardAvailable(IPage current)
        {
            if (current != null && IsEndOfChapter(current))
            {
                var height = current.GetContentHeight();
                return Height / 2 < height; // the content height is longer than half page
            }

            return true;
        }
        public bool IsLineBackwardAvailable(IPage current)
        {
            return current != null && !IsStartOfChapter(current);
        }

        public bool IsStartOfChapter(IPage current)
        {
            return current?.TopPosition?.ParagraphId == 0 && current.TopPosition?.Offset == 0;
        }
        public bool IsEndOfChapter(IPage current)
        {
            if (current == null)
                throw new ArgumentNullException(nameof(current));

            if (current.BottomPosition == null)
                throw new ArgumentNullException(nameof(current.BottomPosition));

            return current.BottomPosition.ChapterIndex >= ChapterCount ||
                   current.BottomPosition.ParagraphId >= Chapters[current.BottomPosition.ChapterIndex].Count - 1 &&
                   current.BottomPosition.Offset >= Chapters[current.BottomPosition.ChapterIndex].LastOrDefault()?.Content.Length - 1;
        }
    }
}
