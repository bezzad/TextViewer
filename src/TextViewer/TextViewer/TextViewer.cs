using MethodTimer;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace TextViewer
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this TextViewer control in a XAML file.
    ///
    /// Step 1a) Using this TextViewer control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:TextViewer="clr-namespace:TextViewer"
    ///
    ///
    /// Step 1b) Using this TextViewer control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:TextViewer="clr-namespace:TextViewer;assembly=TextViewer"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Select TextViewer project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <TextViewer:TextViewer/>
    ///
    /// </summary>
    public class TextViewer : BaseTextViewer
    {
        private readonly Stack<WordInfo> _nonDirectionalWordsStack = new Stack<WordInfo>();

        protected void SetStartPoint(ref Point startPoint, Paragraph para, double extendedY = 0)
        {
            startPoint.X = para.IsRtl
                ? ActualWidth - Padding.Right
                : Padding.Left;

            startPoint.Y += extendedY;
        }

        protected WordInfo SetWordPositionInLine(Paragraph para, WordInfo word, ref Point startPoint)
        {
            if (para.IsRtl)
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
                startPoint.X -= word.Width + word.SpaceWidth;
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
                startPoint.X += word.Width + word.SpaceWidth;
            }

            return word;
        }

        protected void AddLine(Paragraph para, List<WordInfo> lineBuffer, ref Point startPoint, double lineWidth, double lineRemainWidth, bool justify)
        {
            if (_nonDirectionalWordsStack.Any())
                while (_nonDirectionalWordsStack.TryPop(out var nWord))
                    SetWordPositionInLine(para, nWord, ref startPoint);

            if (lineRemainWidth > 0)
            {
                if (justify)
                {
                    SetStartPoint(ref startPoint, para, 0);

                    var extendSpace = lineRemainWidth / (lineBuffer.Count(w => w.IsInnerWord == false) - 1);
                    foreach (var word in lineBuffer)
                    {
                        if (word.IsInnerWord == false)
                        {
                            word.SpaceWidth += extendSpace;
                        }

                        SetWordPositionInLine(para, word, ref startPoint);
                    }
                }
                else if (para.Styles.ContainsKey(StyleType.TextAlign))
                {
                    if (para.Styles[StyleType.TextAlign] == "left")
                    {
                        if (para.IsRtl)
                        {
                            SetStartPoint(ref startPoint, para, 0);
                            startPoint.X -= lineRemainWidth;
                            foreach (var word in lineBuffer)
                                SetWordPositionInLine(para, word, ref startPoint);
                        }
                    }
                    else if (para.Styles[StyleType.TextAlign] == "center")
                    {
                        SetStartPoint(ref startPoint, para, 0);
                        startPoint.X += lineRemainWidth / 2 * (para.IsRtl ? -1 : 1);
                        foreach (var word in lineBuffer)
                            SetWordPositionInLine(para, word, ref startPoint);
                    }
                    else if (para.Styles[StyleType.TextAlign] == "right")
                    {
                        if (para.IsRtl == false)
                        {
                            SetStartPoint(ref startPoint, para, 0);
                            startPoint.X += lineRemainWidth;
                            foreach (var word in lineBuffer)
                                SetWordPositionInLine(para, word, ref startPoint);
                        }
                    }
                }
            }

            para.Lines.Add(lineBuffer);
            SetStartPoint(ref startPoint, para, lineBuffer.Last().Height); // new line
            _nonDirectionalWordsStack.Clear();
        }

        [Time]
        protected void BuildPage(List<Paragraph> content)
        {
            var startPoint = new Point(content.FirstOrDefault()?.IsRtl == true ? ActualWidth - Padding.Right : Padding.Left, Padding.Top);
            var lineWidth = ActualWidth - Padding.Left - Padding.Right;
            var lineRemainWidth = lineWidth;
            var lineBuffer = new List<WordInfo>();
            DrawWords.Clear();

            foreach (var para in content)
            {
                // todo: clear lines if the style of page changed!
                para.Lines.Clear(); // clear old lines

                foreach (var word in para.Words)
                {
                    if (word.IsImage)
                        word.ImageScale = 1;
                    else
                        word.GetFormattedText(FontFamily, FontSize, PixelsPerDip, LineHeight);

                    var wordWidth = word.Width;
                    var wordPointer = word;

                    while (word.PreviousWord?.IsInnerWord != true && wordPointer.IsInnerWord)
                    {
                        wordPointer = wordPointer.NextWord;
                        wordWidth += wordPointer.Width;
                    }

                    if (lineRemainWidth - wordWidth <= 0)
                    {
                        if (lineBuffer.Count > 0)
                        {
                            lineRemainWidth += lineBuffer.Last().SpaceWidth; // end of line has no space (important for justify)
                            AddLine(para, lineBuffer, ref startPoint, lineWidth, lineRemainWidth, IsJustify);
                            lineBuffer = new List<WordInfo>(); // create new line buffer, without cleaning last line
                            lineRemainWidth = lineWidth;
                        }
                        else // the current word width is more than a line!
                        {
                            if (word.IsImage) // set image scale according by image and page width
                                word.ImageScale = lineRemainWidth / double.Parse(word.Styles[StyleType.Width]);
                            else
                                word.Format.MaxTextWidth = lineRemainWidth;
                        }
                    }

                    lineBuffer.Add(word);
                    DrawWords.Add(word);
                    if (para.IsRtl != word.IsRtl)
                        _nonDirectionalWordsStack.Push(word);
                    else
                    {
                        if (_nonDirectionalWordsStack.Any())
                            while (_nonDirectionalWordsStack.TryPop(out var nWord))
                                SetWordPositionInLine(para, nWord, ref startPoint);

                        SetWordPositionInLine(para, word, ref startPoint);
                    }

                    lineRemainWidth -= word.Width + word.SpaceWidth;
                }

                AddLine(para, lineBuffer, ref startPoint, lineWidth, lineRemainWidth, false); // last line of paragraph
                lineBuffer = new List<WordInfo>(); // create new line buffer, without cleaning last line
                lineRemainWidth = lineWidth;

                // + ParagraphSpace
                startPoint.Y += ParagraphSpace;
            }
        }


        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            BuildPage(PageContent);

            foreach (var word in DrawWords)
            {
                if (word.Styles.ContainsKey(StyleType.Image))
                {
                    var img = word.Styles[StyleType.Image].BitmapFromBase64();
                    dc.DrawImage(img, word.Area);
                }
                else
                    dc.DrawText(word.Format, word.DrawPoint);

                if (ShowWireFrame)
                    dc.DrawRectangle(null, WireFramePen, word.Area);

                if (ShowOffset)
                    dc.DrawText(new FormattedText(word.Offset.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                        new Typeface(FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                        10, Brushes.BlueViolet, PixelsPerDip),
                        new Point(word.IsRtl ? word.DrawPoint.X : word.Area.X, (word.IsRtl ? word.DrawPoint.Y : word.Area.Y) - 10));
            }

            if (ShowWireFrame) // show paragraph area
            {
                foreach (var para in PageContent)
                {
                    var firstWord = para.Words.First();
                    dc.DrawRoundedRectangle(null, new Pen(Brushes.Brown, 0.3) { DashStyle = DashStyles.Solid },
                        new Rect(new Point(Padding.Left, firstWord.DrawPoint.Y),
                                 new Size(ActualWidth - Padding.Left - Padding.Right, para.Lines.Sum(l => l[0].Height))),
                                 4, 4);
                }
            }
        }
    }
}