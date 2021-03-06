﻿using System;
using System.ComponentModel;
using System.Diagnostics;
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
        public IPage PageContent { get; set; }
        public delegate void MessageEventHandler(object sender, TextViewerEventArgs args);
        public event MessageEventHandler Message;
        protected virtual void OnMessage(string message, MessageType messageType)
        {
            Message?.Invoke(this, new TextViewerEventArgs(message, messageType));
        }

        protected bool BuildPage()
        {
            if (!(PageContent?.BlockCount > 0)) return false;

            var startPoint = new Point(PageContent.TextBlocks.FirstOrDefault()?.Styles.IsRtl == true ? ActualWidth - Padding.Right : Padding.Left, Padding.Top);
            var lineWidth = ActualWidth - Padding.Left - Padding.Right;
            if (lineWidth < MinWidth)
                return false; // the page has not enough space

            ClearDrawnWords();
            Line lineBuffer;

            void RemoveSpaceFromEndOfLine()
            {
                // Note: end of line has no space (important for justify)
                while (lineBuffer.Words.LastOrDefault()?.Type.HasFlag(WordType.Space) == true)
                {
                    lineBuffer.Words.RemoveAt(lineBuffer.Words.Count - 1);
                    RemoveDrawnWord(VisualChildrenCount - 1);
                }
            }

            foreach (var para in PageContent.TextBlocks)
            {
                // todo: clear lines if the style of page changed!
                para.ClearLines(); // clear old lines
                para.Location = new Point(Padding.Left, startPoint.Y);
                para.Size = new Size(lineWidth, 0);
                AddDrawnWord(para);

                // create new line buffer, without cleaning last line
                lineBuffer = para.AddLine(new Line(startPoint));

                foreach (var word in para.Words)
                {
                    word.SetFormattedText(FontFamily, FontSize, PixelsPerDip, LineHeight);

                    var wordWidth = word.Width;
                    var wordPointer = word;
                    //
                    // render attached words as one word by one width
                    while (word.PreviousWord?.Type.HasFlag(WordType.Attached) == false && wordPointer.Type.HasFlag(WordType.Attached))
                    {
                        wordPointer = wordPointer.NextWord;
                        wordWidth += wordPointer.Width;
                    }

                    if (lineBuffer.RemainWidth - wordWidth <= 0)
                    {
                        if (lineBuffer.Count > 0)
                        {
                            RemoveSpaceFromEndOfLine();
                            lineBuffer.Build(IsJustify);
                            // go to new line
                            startPoint.X = para.Styles.IsRtl ? ActualWidth - Padding.Right : Padding.Left;
                            startPoint.Y += lineBuffer.Height;
                            lineBuffer = para.AddLine(new Line(startPoint)); // create new line buffer, without cleaning last line
                        }
                        else // the current word width is more than a line!
                        {
                            if (word.IsImage && word is ImageWord imgWord) // set image scale according by image and page width
                                imgWord.ImageScale = lineBuffer.RemainWidth / word.Styles.Width;
                            else if (word.Format != null)
                                word.Format.MaxTextWidth = Math.Abs(lineBuffer.RemainWidth);
                        }
                    }

                    // Note: The line should not start with space char
                    if (lineBuffer.Count > 0 || word.Type.HasFlag(WordType.Space) == false)
                    {
                        lineBuffer.AddWord(word);
                        AddDrawnWord(word);
                    }
                }

                RemoveSpaceFromEndOfLine();
                lineBuffer.Build(false);  // last line of paragraph has no justified!
                // go to new line
                startPoint.X = para.Styles.IsRtl ? ActualWidth - Padding.Right : Padding.Left;
                startPoint.Y += lineBuffer.Height;
                para.Size = new Size(lineWidth, startPoint.Y - para.Location.Y);


                // + ParagraphSpace
                startPoint.Y += ParagraphSpace;
            }

            return true;
        }

        protected override void OnRender(DrawingContext dc)
        {
            var sw = ShowFramePerSecond ? Stopwatch.StartNew() : null;

            try
            {
                base.OnRender(dc);

                if (DesignerProperties.GetIsInDesignMode(this) ||
                    BuildPage() == false) return;

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
                                OffsetEmSize, word.Styles.IsRtl ? Brushes.Red : Brushes.Blue,
                                PixelsPerDip);

                            if (word.Type.HasFlag(WordType.Space) || word.Type.HasFlag(WordType.InertChar)) //rotate 90 degree the offset text at the space area
                            {
                                var drawPoint = new Point(word.Area.X + word.Width + 1,
                                    word.Area.Y + (word.Type.HasFlag(WordType.Space) ? word.Height / 2 - ft.Width / 2 : 1));
                                dc.PushTransform(new RotateTransform(90, drawPoint.X, drawPoint.Y));
                                dc.DrawText(ft, drawPoint);
                                dc.Pop();
                            }
                            else
                                dc.DrawText(ft, new Point(word.Paragraph.Styles.IsRtl ? word.Area.X + word.Width - ft.Width : word.Area.X, word.Area.Y));
                        }
                    }
                    else if (visual is Paragraph para)
                    {
                        para.Render();

                        if (ShowWireFrame) // show paragraph area
                            dc.DrawRoundedRectangle(null, ParagraphWireFramePen, new Rect(para.Location, para.Size), 4, 4);
                    }
                }
            }
            finally
            {
                if (ShowFramePerSecond && sw != null)
                {
                    sw.Stop();
                    var ems = sw.ElapsedMilliseconds > 0 ? sw.ElapsedMilliseconds : 1;
                    var ft = new FormattedText($"FPS: {1000 / ems}, ExecTime: {ems}ms",
                        CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                        new Typeface(FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                        FpsEmSize, Brushes.Blue, PixelsPerDip);

                    dc.DrawText(ft, new Point(1, 1));
                }
            }
        }
    }
}