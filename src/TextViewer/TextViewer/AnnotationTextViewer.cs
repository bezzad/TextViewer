using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TextViewer
{
    public class AnnotationTextViewer : SelectableTextViewer
    {
        protected List<WordInfo> HyperLinks { get; set; }
        protected AnnotationBox Annotation { get; set; }
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
            Annotation = new AnnotationBox();
        }


        protected override void OnTouchVisualHit(Point position, HitTestResult result)
        {
            base.OnTouchVisualHit(position, result);
            if (result.VisualHit is WordInfo word)
            {
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
            else ClearAnnotation();
        }

        public void AnnotationStart(string text, TextInfo refText)
        {
            if (string.IsNullOrEmpty(text) || refText == null)
                return;

            ClearAnnotation();
            Annotation.Text = text;
            Annotation.TextDirection = Paragraph.IsRtl(text) ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            AnnotationReferenceText = refText;
            BuildAnnotation();
        }

        protected void ClearAnnotation()
        {
            if (Annotation != null)
                Annotation.Visibility = Visibility.Hidden;

            AnnotationReferenceText = null;
        }

        protected void BuildAnnotation()
        {
            if (string.IsNullOrEmpty(Annotation?.Text) == false && AnnotationReferenceText != null)
            {
                Annotation.MaxHeight = ActualHeight * 0.5 - Padding.Bottom - Padding.Top;
                Annotation.MaxWidth = ActualWidth * 0.6 - Padding.Right - Padding.Left;
                Annotation.MinHeight = Math.Max(LineHeight, Annotation.CornerRadius * 2 + Annotation.BubblePeakHeight + 1);
                Annotation.MinWidth = Annotation.CornerRadius * 2 + Annotation.BubblePeakWidth + 1 + Annotation.BorderThickness.Right * 2 + Annotation.Padding.Right * 2;
                Annotation.Background = new SolidColorBrush(Colors.Bisque) { Opacity = 0.97 };
                Annotation.Foreground = Brushes.Teal;
                Annotation.FontSize = FontSize - 2;
                Annotation.TextAlign = TextAlignment.Justify;

                if (Parent is Canvas mainCanvas)
                {
                    var posInScreen = PointToScreen(new Point(AnnotationReferenceText.Area.X + AnnotationReferenceText.Width / 2, 
                        AnnotationReferenceText.Area.Y + AnnotationReferenceText.Height / 2));
                    Annotation.Open(posInScreen, mainCanvas);
                }
            }
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
            BuildAnnotation();
        }
    }
}
