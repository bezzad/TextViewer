using System;
using MethodTimer;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
        [Time]
        protected void BuildPage(List<List<WordInfo>> content)
        {
            var startPoint = new Point(IsContentRtl ? ActualWidth - Padding.Right : Padding.Left, Padding.Top);
            var nonDirectionalWordsStack = new Stack<WordInfo>();
            var lineWidth = ActualWidth - Padding.Left - Padding.Right;
            var lineRemainWidth = lineWidth;
            var lineBuffer = new List<WordInfo>();
            DrawWords.Clear();

            void AddLine(bool justify)
            {
                if (nonDirectionalWordsStack.Any())
                    while (nonDirectionalWordsStack.TryPop(out var nWord))
                        DrawWords.Add(AddWordToCurrentLine(nWord));

                if (IsJustify && lineRemainWidth > 0 && justify)
                {
                    startPoint.X = IsContentRtl
                        ? ActualWidth - Padding.Right
                        : Padding.Left;

                    var extendSpace = lineRemainWidth / (lineBuffer.Count(w => w.IsInnerWord == false) - 1);
                    foreach (var word in lineBuffer)
                    {
                        if (word.IsInnerWord == false)
                        {
                            word.SpaceWidth += extendSpace;
                        }

                        AddWordToCurrentLine(word);
                    }
                }

                Lines.Add(lineBuffer);
                lineRemainWidth = lineWidth;
                startPoint.Y += lineBuffer.Last().Height; // new line
                startPoint.X = IsContentRtl
                    ? ActualWidth - Padding.Right
                    : Padding.Left;
                nonDirectionalWordsStack.Clear();
                lineBuffer = new List<WordInfo>(); // create new line buffer, without cleaning last line
            }

            WordInfo AddWordToCurrentLine(WordInfo word)
            {
                if (IsContentRtl)
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
                foreach (var word in para)
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
                            AddLine(IsJustify);
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
                    if (IsContentRtl != word.IsRtl)
                        nonDirectionalWordsStack.Push(word);
                    else
                    {
                        if (nonDirectionalWordsStack.Any())
                            while (nonDirectionalWordsStack.TryPop(out var nWord))
                                DrawWords.Add(AddWordToCurrentLine(nWord));

                        DrawWords.Add(AddWordToCurrentLine(word));
                    }

                    lineRemainWidth -= word.Width + word.SpaceWidth;
                }

                AddLine(false); // last line of paragraph
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
            }
        }


    }
}