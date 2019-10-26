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
            startPoint.X = para.Styles.IsRtl
                ? ActualWidth - Padding.Right
                : Padding.Left;

            startPoint.Y += extendedY;
        }

        [Time]
        protected void BuildPage(List<Paragraph> content)
        {
            var startPoint = new Point(content.FirstOrDefault()?.Styles.IsRtl == true ? ActualWidth - Padding.Right : Padding.Left, Padding.Top);
            var lineWidth = ActualWidth - Padding.Left - Padding.Right;
            DrawnWords.Clear();
            Line lineBuffer;

            void RemoveSpaceFromEndOfLine()
            {
                // Note: end of line has no space (important for justify)
                while (lineBuffer.Words.LastOrDefault()?.Type.HasFlag(WordType.Space) == true)
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
                para.Location = new Point(Padding.Left, startPoint.Y);
                para.Size = new Size(0, 0);
                DrawnWords.Add(para);

                // create new line buffer, without cleaning last line
                lineBuffer = new Line(lineWidth, para, startPoint);

                foreach (var word in para.Words)
                {
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
                            lineBuffer.Render(IsJustify);
                            SetStartPoint(ref startPoint, para, lineBuffer.Height); // new line
                            lineBuffer = new Line(lineWidth, para, startPoint); // create new line buffer, without cleaning last line
                        }
                        else // the current word width is more than a line!
                        {
                            if (word.IsImage) // set image scale according by image and page width
                                word.ImageScale = lineBuffer.RemainWidth / word.Styles.Width;
                            else
                                word.Format.MaxTextWidth = lineBuffer.RemainWidth;
                        }
                    }

                    // Note: The line should not start with space char
                    if (lineBuffer.Count > 0 || word.Type.HasFlag(WordType.Space) == false)
                    {
                        lineBuffer.AddWord(word);
                        DrawnWords.Add(word);
                    }
                }

                RemoveSpaceFromEndOfLine();
                lineBuffer.Render(false);  // last line of paragraph has no justified!
                SetStartPoint(ref startPoint, para, lineBuffer.Height); // new line
                para.Size = new Size(lineWidth, startPoint.Y - para.Location.Y);
                

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

            foreach (var visual in DrawnWords)
            {
                if (visual is WordInfo word)
                {
                    word.Render();

                    if (ShowWireFrame)
                        dc.DrawRectangle(null, WordWireFramePen, word.Area);

                    if (ShowOffset)
                    {
                        var ft = new FormattedText(word.Offset.ToString(),
                            CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                            new Typeface(FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                            OffsetEmSize, word.IsRtl ? Brushes.Red : Brushes.Blue,
                            PixelsPerDip);

                        if (word.Type.HasFlag(WordType.Space) || word.Type.HasFlag(WordType.InertChar)
                        ) //rotate 90 degree the offset text at the space area
                        {
                            var drawPoint = new Point(word.Area.X + word.Width + 1,
                                word.Area.Y + (word.Type.HasFlag(WordType.Space) ? word.Height / 2 - ft.Width / 2 : 1));
                            dc.PushTransform(new RotateTransform(90, drawPoint.X, drawPoint.Y));
                            dc.DrawText(ft, drawPoint);
                            dc.Pop();
                        }
                        else
                            dc.DrawText(ft,
                                new Point(word.Paragraph.Styles.IsRtl ? word.Area.X + word.Width - ft.Width : word.Area.X,
                                    word.Area.Y));
                    }
                }
                else if (visual is Paragraph para)
                {
                    para.Render();

                    if (ShowWireFrame) // show paragraph area
                    {
                        dc.DrawRoundedRectangle(null, ParagraphWireFramePen,
                            new Rect(para.Location, para.Size), 4, 4);
                    }
                }
            }
        }
    }
}