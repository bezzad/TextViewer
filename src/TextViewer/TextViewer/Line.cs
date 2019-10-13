﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace TextViewer
{
    public class Line
    {
        protected readonly Stack<WordInfo> NonDirectionalWordsStack;
        public double WordPointOffset { get; set; }

        public List<WordInfo> Words { get; set; }
        public double Width { get; set; }
        public double RemainWidth { get; set; }
        public Point Location { get; set; }
        public Paragraph CurrentParagraph { get; set; }

        public Line(double width, Paragraph para, Point lineLocation)
        {
            RemainWidth = Width = width;
            Location = lineLocation;
            WordPointOffset = Location.X;
            NonDirectionalWordsStack = new Stack<WordInfo>();
            Words = new List<WordInfo>();
            CurrentParagraph = para;
        }

        protected WordInfo SetWordPositionInLine(WordInfo word)
        {
            var startPoint = new Point(WordPointOffset, Location.Y);
            if (CurrentParagraph.IsRtl)
            {
                //     _____________________________________________
                //    |                                             |
                //    |                               __________ +  |  + start position
                //    |                     <--- ... |__________|   | 
                //    |                                             |
                //    |                                             |
                //    |_____________________________________________| 
                //
                word.Area = new Rect(new Point(startPoint.X - word.Width, startPoint.Y), new Size(word.Width, word.Height));
                word.DrawPoint = word.IsRtl ? startPoint : word.Area.Location;
                WordPointOffset -= word.Width + word.SpaceWidth;
            }
            else // ---->
            {
                //     _____________________________________________
                //    |                                             |
                //    |   +__________                               |  + start position
                //    |   |__________|  ... --->                    | 
                //    |                                             |
                //    |                                             |
                //    |_____________________________________________| 
                //
                word.Area = new Rect(startPoint, new Size(word.Width, word.Height));
                word.DrawPoint = word.IsRtl ? startPoint : word.Area.Location;
                WordPointOffset += word.Width + word.SpaceWidth;
            }

            return word;
        }

        protected void SetNonDirectionalWords()
        {
            if (NonDirectionalWordsStack.Any())
                while (NonDirectionalWordsStack.TryPop(out var nonWord))
                    SetWordPositionInLine(nonWord);
        }

        public void AddWord(WordInfo word)
        {
            Words.Add(word);
            if (CurrentParagraph.IsRtl != word.IsRtl)
                NonDirectionalWordsStack.Push(word);
            else
            {
                SetNonDirectionalWords();
                SetWordPositionInLine(word);
            }

            RemainWidth -= word.Width + word.SpaceWidth;
        }

        public void Draw(bool justify = false)
        {
            SetNonDirectionalWords();

            if (RemainWidth > 0)
            {
                WordPointOffset = Location.X;

                if (justify)
                {
                    var extendSpace = RemainWidth / (Words.Count(w => w.IsInnerWord == false) - 1);
                    foreach (var word in Words)
                    {
                        if (word.IsInnerWord == false)
                        {
                            word.SpaceWidth += extendSpace;
                        }

                        SetWordPositionInLine(word);
                    }
                }
                else if (CurrentParagraph.Styles.ContainsKey(StyleType.TextAlign))
                {
                    switch (CurrentParagraph.Styles[StyleType.TextAlign])
                    {
                        case "left":
                            {
                                if (CurrentParagraph.IsRtl)
                                {
                                    WordPointOffset -= RemainWidth;
                                    foreach (var word in Words)
                                        SetWordPositionInLine(word);
                                }

                                break;
                            }

                        case "center":
                            {
                                WordPointOffset += RemainWidth / 2 * (CurrentParagraph.IsRtl ? -1 : 1);
                                foreach (var word in Words)
                                    SetWordPositionInLine(word);
                                break;
                            }

                        case "right":
                            {
                                if (CurrentParagraph.IsRtl == false)
                                {
                                    WordPointOffset += RemainWidth;
                                    foreach (var word in Words)
                                        SetWordPositionInLine(word);
                                }

                                break;
                            }
                    }
                }
            }

            CurrentParagraph.Lines.Add(Words);
        }
    }
}
