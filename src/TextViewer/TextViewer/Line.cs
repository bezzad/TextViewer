using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace TextViewer
{
    public class Line
    {
        protected readonly Stack<WordInfo> NonDirectionalWordsStack;
        public double WordPointOffset { get; set; }

        public List<WordInfo> Words { get; protected set; }
        public double Width => CurrentParagraph?.Size.Width ?? 0;
        public double Height { get; set; }
        public double RemainWidth { get; set; }
        public Point Location { get; set; }
        public Paragraph CurrentParagraph { get; set; }
        public int Count => Words.Count;
        public double ActualWidth => Words.Sum(w => w.Width);


        public Line(Paragraph para, Point lineLocation)
        {
            CurrentParagraph = para;
            RemainWidth = Width;
            Location = lineLocation;
            WordPointOffset = Location.X;
            NonDirectionalWordsStack = new Stack<WordInfo>();
            Words = new List<WordInfo>();
        }


        public void AddWord(WordInfo word)
        {
            Words.Add(word);
            if (word.Height > Height) Height = word.Height;

            SetWordPosition(word);
            RemainWidth -= word.Width;
        }

        public void Render(bool justify)
        {
            // clear non directional words stack
            PopAllNonDirectionalWords();

            if (RemainWidth > 0)
            {
                WordPointOffset = Location.X;

                if (justify)
                {
                    var extendSpace = RemainWidth / Words.Count(w => w.Type.HasFlag(WordType.Space));
                    foreach (var word in Words)
                    {
                        if (word is SpaceWord space) space.ExtraWidth = extendSpace;
                        SetWordPosition(word);
                    }
                }
                else if (CurrentParagraph.Styles.TextAlign.HasValue)
                {
                    switch (CurrentParagraph.Styles.TextAlign.Value)
                    {
                        case TextAlignment.Left:
                            {
                                if (CurrentParagraph.Styles.IsRtl)
                                    WordPointOffset -= RemainWidth;
                                SetWordsPosition();
                                break;
                            }
                        case TextAlignment.Center:
                            {
                                WordPointOffset += RemainWidth / 2 * (CurrentParagraph.Styles.IsRtl ? -1 : 1);
                                SetWordsPosition();
                                break;
                            }
                        case TextAlignment.Right:
                            {
                                if (CurrentParagraph.Styles.IsLtr)
                                    WordPointOffset += RemainWidth;
                                SetWordsPosition();
                                break;
                            }
                    }
                }

                // maybe last word is non directional word
                PopAllNonDirectionalWords();
            }

            CurrentParagraph.Lines.Add(this);
        }

        protected void SetWordsPosition()
        {
            foreach (var word in Words)
                SetWordPosition(word);
        }

        protected WordInfo SetWordPositionInLine(WordInfo word)
        {
            var startPoint = new Point(WordPointOffset, Location.Y);

            if (CurrentParagraph.Styles.IsRtl) // Left to right paragraph
            {
                //     _______________________________________________________
                //    |                                                       |
                //    |                                         __________ +  |  + start position
                //    |                               <--- ... |__________|   | 
                //    |                                                       |
                //    |   __________ _   _____ _   _ _______   _ _________    |
                //    |  |___LTR____|_| |_LTR_|_| |_|__RTL__| |_|___RTL___|   |
                //    |                                                       |
                //    |_______________________________________________________| 
                //
                word.Area = new Rect(new Point(startPoint.X - word.Width, startPoint.Y), new Size(word.Width, word.Height));
                word.DrawPoint = word.Styles.IsRtl ? startPoint : word.Area.Location;
                WordPointOffset -= word.Width;
            }
            else // Left to right paragraph
            {
                //     ________________________________________________________
                //    |                                                        |
                //    |   +__________                                          |  + start position
                //    |   |__________|  ... --->                               | 
                //    |                                                        |
                //    |    __________ _   _____ _   _ _______   _ _________    |
                //    |   |___LTR____|_| |_LTR_|_| |_|__RTL__| |_|___RTL___|   |
                //    |                                                        |
                //    |________________________________________________________| 
                //
                word.Area = new Rect(startPoint, new Size(word.Width, word.Height));
                word.DrawPoint = word.Styles.IsRtl ? new Point(WordPointOffset + word.Width, startPoint.Y) : word.Area.Location;
                WordPointOffset += word.Width;
            }

            if (word.Styles.VerticalAlign.HasValue)
            {
                var size = Math.Abs(word.Styles.FontSize);
                size = size > 0 ? size : 5;
                if (word.Styles.VerticalAlign.Value == VerticalAlignment.Top)
                    word.DrawPoint = new Point(word.DrawPoint.X, word.DrawPoint.Y - size);
                if (word.Styles.VerticalAlign.Value == VerticalAlignment.Bottom)
                    word.DrawPoint = new Point(word.DrawPoint.X, word.DrawPoint.Y + size);
            }

            return word;
        }

        protected void PopAllNonDirectionalWords()
        {
            if (NonDirectionalWordsStack.Any())
                while (NonDirectionalWordsStack.Any())
                    SetWordPositionInLine(NonDirectionalWordsStack.Pop());
        }

        protected void SetWordPosition(WordInfo word)
        {
            if (CurrentParagraph.Styles.IsRtl != word.Styles.IsRtl)
                NonDirectionalWordsStack.Push(word);
            else
            {
                PopAllNonDirectionalWords();
                SetWordPositionInLine(word);
            }
        }
    }
}
