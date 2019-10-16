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
            var lineWidth = ActualWidth - Padding.Left - Padding.Right;
            DrawnWords.Clear();
            Line lineBuffer;

            void RemoveSpaceFromEndOfLine()
            {
                // Note: end of line has no space (important for justify)
                while (lineBuffer.Words.Last().Type.HasFlag(WordType.Space))
                {
                    lineBuffer.RemainWidth += lineBuffer.Words.Last().Width;
                    lineBuffer.Words.RemoveAt(lineBuffer.Words.Count - 1);
                    DrawnWords.RemoveAt(DrawnWords.Count - 1);
                }
            }

            foreach (var para in content)
            {
                // todo: clear lines if the style of page changed!
                para.Lines.Clear(); // clear old lines

                // create new line buffer, without cleaning last line
                lineBuffer = new Line(lineWidth, para, startPoint);

                foreach (var word in para.Words)
                {
                    // Note: The line should not start with space char
                    if (lineBuffer.Count == 0 && word.Type.HasFlag(WordType.Space))
                        continue;

                    if (word.IsImage)
                        word.ImageScale = 1;
                    else
                        word.GetFormattedText(FontFamily, FontSize, PixelsPerDip, LineHeight);

                    var wordWidth = word.Width;
                    var wordPointer = word;
                    //
                    // attached words has one width at all
                    while (word.PreviousWord?.Type.HasFlag(WordType.Attached) == false &&
                           wordPointer.Type.HasFlag(WordType.Attached))
                    {
                        wordPointer = wordPointer.NextWord;
                        wordWidth += wordPointer.Width;
                    }

                    if (lineBuffer.RemainWidth - wordWidth <= 0)
                    {
                        if (lineBuffer.Count > 0)
                        {
                            RemoveSpaceFromEndOfLine();

                            lineBuffer.Draw(IsJustify);
                            SetStartPoint(ref startPoint, para, lineBuffer.Height); // new line
                            lineBuffer = new Line(lineWidth, para, startPoint); // create new line buffer, without cleaning last line
                        }
                        else // the current word width is more than a line!
                        {
                            if (word.IsImage) // set image scale according by image and page width
                                word.ImageScale = lineBuffer.RemainWidth / double.Parse(word.Styles[StyleType.Width]);
                            else
                                word.Format.MaxTextWidth = lineBuffer.RemainWidth;
                        }
                    }

                    lineBuffer.AddWord(word);
                    DrawnWords.Add(word);
                }

                RemoveSpaceFromEndOfLine();
                lineBuffer.Draw(false);  // last line of paragraph has no justified!
                SetStartPoint(ref startPoint, para, lineBuffer.Height); // new line

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

            foreach (var word in DrawnWords)
            {
                if (word.GetAttribute(StyleType.Image) is ImageSource img)
                    dc.DrawImage(img, word.Area);
                else
                    dc.DrawText(word.Format, word.DrawPoint);

                if (ShowWireFrame)
                    dc.DrawRectangle(null, WireFramePen, word.Area);

                if (ShowOffset)
                {
                    var ft = new FormattedText(word.Offset.ToString(),
                        CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                        new Typeface(FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                        8, Brushes.BlueViolet, PixelsPerDip);

                    if (word.Type.HasFlag(WordType.Space) || word.Type.HasFlag(WordType.InertChar)) //rotate 90 degree the offset text at the space area
                    {
                        var drawPoint = new Point(word.Area.X + word.Width + 1, word.Area.Y + (word.Type.HasFlag(WordType.Space) ? word.Height / 2 - ft.Width / 2 : 1));
                        dc.PushTransform(new RotateTransform(90, drawPoint.X, drawPoint.Y));
                        dc.DrawText(ft, drawPoint);
                        dc.Pop();
                    }
                    else
                        dc.DrawText(ft, new Point(word.Paragraph.IsRtl ? word.Area.X + word.Width - ft.Width : word.Area.X, word.Area.Y));
                }
            }

            if (ShowWireFrame) // show paragraph area
            {
                foreach (var para in PageContent)
                {
                    var firstWord = para.Lines.First().Words.First();
                    dc.DrawRoundedRectangle(null, new Pen(Brushes.Brown, 0.3) { DashStyle = DashStyles.Solid },
                        new Rect(new Point(Padding.Left, firstWord.DrawPoint.Y),
                                 new Size(ActualWidth - Padding.Left - Padding.Right, para.Lines.Sum(l => l.Height))),
                                 4, 4);
                }
            }
        }
    }
}