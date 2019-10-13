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
        protected void SetStartPoint(ref Point startPoint, Paragraph para, double extendedY = 0)
        {
            startPoint.X = para.IsRtl
                ? ActualWidth - Padding.Right
                : Padding.Left;

            startPoint.Y += extendedY;
        }

        [Time]
        protected void BuildPage(List<Paragraph> content)
        {
            var startPoint = new Point(content.FirstOrDefault()?.IsRtl == true ? ActualWidth - Padding.Right : Padding.Left, Padding.Top);
            var nonDirectionalWordsStack = new Stack<WordInfo>();
            var lineWidth = ActualWidth - Padding.Left - Padding.Right;
            var lineRemainWidth = lineWidth;
            var lineBuffer = new List<WordInfo>();
            DrawWords.Clear();

            void AddLine(Paragraph para, bool justify)
            {
                if (nonDirectionalWordsStack.Any())
                    while (nonDirectionalWordsStack.TryPop(out var nWord))
                        DrawWords.Add(AddWordToCurrentLine(para, nWord));

                if (justify && lineRemainWidth > 0)
                {
                    SetStartPoint(ref startPoint, para, 0);

                    var extendSpace = lineRemainWidth / (lineBuffer.Count(w => w.IsInnerWord == false) - 1);
                    foreach (var word in lineBuffer)
                    {
                        if (word.IsInnerWord == false)
                        {
                            word.SpaceWidth += extendSpace;
                        }

                        AddWordToCurrentLine(para, word);
                    }
                }

                para.Lines.Add(lineBuffer);
                lineRemainWidth = lineWidth;
                SetStartPoint(ref startPoint, para, lineBuffer.Last().Height); // new line
                nonDirectionalWordsStack.Clear();
                lineBuffer = new List<WordInfo>(); // create new line buffer, without cleaning last line
            }

            WordInfo AddWordToCurrentLine(Paragraph para, WordInfo word)
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
                            AddLine(para, IsJustify);
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
                    if (para.IsRtl != word.IsRtl)
                        nonDirectionalWordsStack.Push(word);
                    else
                    {
                        if (nonDirectionalWordsStack.Any())
                            while (nonDirectionalWordsStack.TryPop(out var nWord))
                                DrawWords.Add(AddWordToCurrentLine(para, nWord));

                        DrawWords.Add(AddWordToCurrentLine(para, word));
                    }

                    lineRemainWidth -= word.Width + word.SpaceWidth;
                }

                AddLine(para, false); // last line of paragraph
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