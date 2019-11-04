using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace TextViewer
{
    public class AnnotationTextViewer : SelectableTextViewer
    {
        protected List<WordInfo> HyperLinks { get; set; }
        protected AnnotationInfo Annotation { get; set; }
        protected TextInfo AnnotationReferenceText { get; set; }
        public bool CopyLinkRefOnClick { get; set; }
        public bool OpenLinkRefOnClick { get; set; }
        public double MaxFontSize { get; set; }
        public double MinFontSize { get; set; }


        public AnnotationTextViewer()
        {
            MaxFontSize = 30;
            MinFontSize = 8;
            HyperLinks = new List<WordInfo>();
        }


        protected override void OnTouchVisualHit(Point position, HitTestResult result)
        {
            base.OnTouchVisualHit(position, result);
            if (result.VisualHit is WordInfo word)
            {
                AnnotationStart(word.Text, word); // todo: remove this line after test ------------------------------------------------------------------------------------------------------------------
                if (HighlightLastWord == null && word.Styles.IsHyperLink)
                {
                    // is external link
                    if (word.Styles.HyperRef.StartsWith("http"))
                    {
                        if (CopyLinkRefOnClick)
                        {
                            Clipboard.SetText(word.Styles.HyperRef);
                            OnMessage(Properties.Resources.LinkCopied, MessageType.Info);
                        }

                        if (OpenLinkRefOnClick) // copy web link in clipboard
                        {
                            OpenUrl(word.Styles.HyperRef);
                            OnMessage(Properties.Resources.LinkOpened, MessageType.Info);
                        }
                    }
                    else
                        AnnotationStart(word.Styles.HyperRef, word);
                }
            }
            else
            {
                AnnotationReferenceText = null;
                Annotation = null;
                Render();
            }
        }

        public void AnnotationStart(string text, TextInfo refText)
        {
            if (string.IsNullOrEmpty(text) || refText == null)
                return;

            Annotation = new AnnotationInfo(text, Paragraph.IsRtl(text));
            AnnotationReferenceText = refText;
            Render();
        }

        protected void BuildAnnotation()
        {
            Annotation.MaxHeight = ActualHeight / 2 - Padding.Bottom - Padding.Top;
            Annotation.MaxWidth = ActualWidth / 2 - Padding.Right - Padding.Left;
            Annotation.MinHeight = Math.Max(LineHeight, Annotation.CornerRadius * 2 + Annotation.BubblePeakHeight + 1);
            Annotation.MinWidth = Annotation.CornerRadius * 2 + Annotation.BubblePeakWidth + 1;
            Annotation.Styles.Background = new SolidColorBrush(Colors.Bisque) { Opacity = 0.97 };
            Annotation.Styles.Foreground = Brushes.Teal;
            Annotation.Styles.FontSize = -2;
            Annotation.Styles.TextAlign = TextAlignment.Justify;
            // set width and height
            Annotation.SetFormattedText(FontFamily, FontSize, PixelsPerDip, LineHeight);


            double left;
            var top = AnnotationReferenceText.Area.Y + AnnotationReferenceText.Height + 5;
            Annotation.BubblePeakPosition = new Point(AnnotationReferenceText.Area.X + AnnotationReferenceText.Width / 2, top);

            if (Annotation.BubblePeakPosition.X + Annotation.Width / 2 > ActualWidth - Padding.Right) // if the length of annotation over from right of page
            {
                left = ActualWidth - Padding.Right - Annotation.Width;
            }
            else if (Annotation.BubblePeakPosition.X - Annotation.Width / 2 < Padding.Left) // if the length of annotation over from left of page
            {
                left = Padding.Left;
            }
            else // 98% of annotations are here
            {
                left = Annotation.BubblePeakPosition.X - Annotation.Width / 2;
            }

            Annotation.DrawPoint = new Point(left + Annotation.Padding, top + Annotation.Padding);
            Annotation.Area = new Rect(new Point(left, top), new Size(Annotation.Width, Annotation.Height));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (IsMouseDown) return;

            foreach (var link in HyperLinks)
                if (IsPointOnWordArea(e.GetPosition(this), link))
                    Mouse.SetCursor(Cursors.Hand);
        }
        protected bool IsPointOnWordArea(Point pos, WordInfo word)
        {
            if (pos.Y > word.Area.Y + word.Area.Height || pos.Y < word.Area.Y ||
                pos.X > word.Area.X + word.Area.Width || pos.X < word.Area.X)
                return false;

            return true;
        }

        public override void AddDrawnWord(DrawingVisual visual)
        {
            base.AddDrawnWord(visual);
            if (visual is WordInfo word && word.Styles.IsHyperLink)
                HyperLinks.Add(word);
        }
        public override void RemoveDrawnWord(int index)
        {
            var visual = DrawnWords[index];
            if (visual is WordInfo word && word.Styles.IsHyperLink)
                HyperLinks.Remove(word);

            base.RemoveDrawnWord(index);
        }
        public override void ClearDrawnWords()
        {
            base.ClearDrawnWords();
            HyperLinks.Clear();
        }
        public void OpenUrl(string url)
        {
            url = url.Replace("&", "^&");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}")
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true,
                    CreateNoWindow = true
                });
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (string.IsNullOrEmpty(Annotation?.Text) == false && AnnotationReferenceText != null)
            {
                BuildAnnotation();
                AddDrawnWord(Annotation.Render());
            }
        }
    }
}
