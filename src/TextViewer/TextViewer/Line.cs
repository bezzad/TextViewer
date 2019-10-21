using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace TextViewer
{
    public class Line
    {
        protected readonly Stack<WordInfo> NonDirectionalWordsStack;
        public double WordPointOffset { get; set; }

        public List<WordInfo> Words { get; protected set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double RemainWidth { get; set; }
        public Point Location { get; set; }
        public Paragraph CurrentParagraph { get; set; }
        public int Count => Words.Count;


        public Line(double width, Paragraph para, Point lineLocation)
        {
            RemainWidth = Width = width;
            Location = lineLocation;
            WordPointOffset = Location.X;
            NonDirectionalWordsStack = new Stack<WordInfo>();
            Words = new List<WordInfo>();
            CurrentParagraph = para;
        }

        
        public void AddWord(WordInfo word)
        {
            Words.Add(word);
            if (word.Height > Height) Height = word.Height;

            SetWordPosition(word);
            RemainWidth -= word.Width;
        }

        public void Render(bool justify = false)
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
                        if (word.Type.HasFlag(WordType.Space))
                            word.ExtraWidth = extendSpace;
                        SetWordPosition(word);
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
                                        SetWordPosition(word);
                                }
                                break;
                            }
                        case "center":
                            {
                                WordPointOffset += RemainWidth / 2 * (CurrentParagraph.IsRtl ? -1 : 1);
                                foreach (var word in Words)
                                    SetWordPosition(word);
                                break;
                            }
                        case "right":
                            {
                                if (CurrentParagraph.IsRtl == false)
                                {
                                    WordPointOffset += RemainWidth;
                                    foreach (var word in Words)
                                        SetWordPosition(word);
                                }
                                break;
                            }
                    }
                }

                // maybe last word is non directional word
                PopAllNonDirectionalWords();
            }

            CurrentParagraph.Lines.Add(this);
        }


        protected WordInfo SetWordPositionInLine(WordInfo word)
        {
            var startPoint = new Point(WordPointOffset, Location.Y);

            if (CurrentParagraph.IsRtl) // Left to right paragraph
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
                word.DrawPoint = word.IsRtl ? startPoint : word.Area.Location;
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
                word.DrawPoint = word.IsRtl ? new Point(WordPointOffset + word.Width, startPoint.Y) : word.Area.Location;
                WordPointOffset += word.Width;
            }

            return word;
        }

        protected void PopAllNonDirectionalWords()
        {
            if (NonDirectionalWordsStack.Any())
                while (NonDirectionalWordsStack.TryPop(out var nonWord))
                    SetWordPositionInLine(nonWord);
        }

        protected void SetWordPosition(WordInfo word)
        {
            if (CurrentParagraph.IsRtl != word.IsRtl) NonDirectionalWordsStack.Push(word);
            else
            {
                PopAllNonDirectionalWords();
                SetWordPositionInLine(word);
            }
        }
    }
}
