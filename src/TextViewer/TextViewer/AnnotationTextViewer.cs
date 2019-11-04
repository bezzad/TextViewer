using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        protected Annotation Annotation { get; set; }
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
                AnnotationStart(word.Text);
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
                        AnnotationStart(word.Styles.HyperRef);
                }
            }
        }

        protected void AnnotationStart(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            if (VisualTreeHelper.GetParent(this) is Panel container)
            {
                if (Annotation == null)
                {
                    Annotation = new Annotation();
                    if (container.Children.Contains(Annotation) == false)
                        container.Children.Add(Annotation);
                }

                InitialAnnotationTextBlock(text);

                var onContainerPosition = Mouse.GetPosition(container);
                var onCanvasPosition = Mouse.GetPosition(this);
                var left = 0.0;
                var right = 0.0;
                var top = onContainerPosition.Y + PeakHeight * 2;
                var bottom = container.ActualHeight - top - Annotation.Height - PeakHeight*2 - 39;

                if (onCanvasPosition.X + Annotation.Width / 2 > ActualWidth - Padding.Right) // if the length of annotation over from right of page
                {
                    Debug.WriteLine("Right Overflow");
                    var rightSpace = ActualWidth - Padding.Right - onCanvasPosition.X;
                    left = onContainerPosition.X + rightSpace - Annotation.Width;
                    right = container.ActualWidth - left - Annotation.Width;
                    Annotation.BubblePeakPosition = new Point(Annotation.Width - rightSpace, -PeakHeight);
                }
                else
                {
                    Debug.WriteLine("Middle Position");
                    left = onContainerPosition.X - Annotation.Width / 2;
                    right = container.ActualWidth - left - Annotation.Width;
                    Annotation.BubblePeakPosition = new Point(Annotation.Width / 2, -PeakHeight);
                }

                Annotation.Margin = new Thickness(left, top, right, bottom);
            }
        }

        private void InitialAnnotationTextBlock(string text)
        {
            Annotation.Text = text;
            Annotation.Padding = 4;
            Annotation.FontFamily = FontFamily;
            Annotation.FontSize = Math.Max(Math.Min(FontSize - 2, MaxFontSize), MinFontSize);
            Annotation.FontWeight = FontWeights.Normal;
            Annotation.TextAlign = TextAlignment.Justify;
            Annotation.FlowDirection = FlowDirection.RightToLeft;
            Annotation.LineHeight = Math.Max(Math.Min(LineHeight, MaxFontSize), MinFontSize);
            Annotation.Visibility = Visibility.Visible;
            Annotation.MaxHeight = ActualHeight / 2;
            Annotation.MaxWidth = ActualWidth / 2;
            Annotation.MinHeight = Annotation.LineHeight;

            var textSize = MeasureString(text, CultureInfo.CurrentCulture, Annotation.FlowDirection,
                Annotation.FontFamily, Annotation.FontWeight, Annotation.FontSize,
                Annotation.MaxWidth - Annotation.Padding * 4, Annotation.LineHeight, Annotation.TextAlign);

            Annotation.Width = Math.Max(Math.Min(textSize.Width + Annotation.Padding * 4 + Annotation.BorderThickness*2, Annotation.MaxWidth), Annotation.MinWidth);
            Annotation.Height = Math.Max(Math.Min(textSize.Height + Annotation.Padding * 4 + Annotation.BorderThickness * 2, Annotation.MaxHeight), Annotation.MinHeight);
        }



        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (IsMouseDown) return;

            foreach (var link in HyperLinks)
                if (IsPointOnWordArea(e.GetPosition(this), link))
                    Mouse.SetCursor(Cursors.Hand);
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


        protected bool IsPointOnWordArea(Point pos, WordInfo word)
        {
            if (pos.Y > word.Area.Y + word.Area.Height || pos.Y < word.Area.Y ||
                pos.X > word.Area.X + word.Area.Width || pos.X < word.Area.X)
                return false;

            return true;
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
