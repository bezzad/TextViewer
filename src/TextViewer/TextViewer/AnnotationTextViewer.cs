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
        public bool CopyLinkRefOnClick { get; set; }
        public bool OpenLinkRefOnClick { get; set; }
        public double MaxFontSize { get; set; }
        public double MinFontSize { get; set; }
        public double PeakHeight { get; set; }


        public AnnotationTextViewer()
        {
            MaxFontSize = 30;
            MinFontSize = 8;
            PeakHeight = 10;
            HyperLinks = new List<WordInfo>();
        }


        protected override void OnTouchVisualHit(Point position, HitTestResult result)
        {
            base.OnTouchVisualHit(position, result);
            if (result.VisualHit is WordInfo word)
            {
                AnnotationStart(word.Text, position); // todo: remove this line after test ------------------------------------------------------------------------------------------------------------------
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
                        AnnotationStart(word.Styles.HyperRef, position);
                }
            }
        }

        protected void AnnotationStart(string text, Point position)
        {
            if (string.IsNullOrEmpty(text))
                return;

            InitialAnnotationTextBlock(text);

            var left = 0.0;
            var right = 0.0;
            var top = position.Y + PeakHeight * 2;
            var bottom = ActualHeight - top - Annotation.Height - PeakHeight * 2 - 39;

            if (position.X + Annotation.Width / 2 > ActualWidth - Padding.Right) // if the length of annotation over from right of page
            {
                Debug.WriteLine("Right Overflow");
                var rightSpace = ActualWidth - Padding.Right - position.X;
                left = position.X + rightSpace - Annotation.Width;
                right = ActualWidth - left - Annotation.Width;
                Annotation.BubblePeakPosition = new Point(Annotation.Width - rightSpace, -PeakHeight);
            }
            else
            {
                Debug.WriteLine("Middle Position");
                left = position.X - Annotation.Width / 2;
                right = ActualWidth - left - Annotation.Width;
                Annotation.BubblePeakPosition = new Point(Annotation.Width / 2, -PeakHeight);
            }

            Annotation.DrawPoint = new Point(left, top);
        }

        private void InitialAnnotationTextBlock(string text)
        {
            Annotation = new AnnotationInfo(text, Paragraph.IsRtl(text));

            Annotation.MaxHeight = ActualHeight / 2 - Annotation.Padding * 2;
            Annotation.MaxWidth = ActualWidth / 2 - Annotation.Padding * 2;
            Annotation.MinHeight = LineHeight;
            Annotation.MinWidth = Annotation.CornerRadius * 2 + Annotation.BubblePeakWidth + 1;

            Annotation.Styles.Background = new SolidColorBrush(Colors.Bisque) { Opacity = 0.97 };
            Annotation.Styles.Foreground = Brushes.Teal;
            Annotation.Styles.FontSize = -2;
            Annotation.Styles.TextAlign = TextAlignment.Justify;

            Annotation.SetFormattedText(FontFamily, FontSize, PixelsPerDip, LineHeight);
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
    }
}
