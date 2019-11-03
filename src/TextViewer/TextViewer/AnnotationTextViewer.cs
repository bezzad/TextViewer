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
        protected Annotation Annotation { get; set; }
        public bool CopyLinkRefOnClick { get; set; }
        public bool OpenLinkRefOnClick { get; set; }

        public AnnotationTextViewer()
        {
            HyperLinks = new List<WordInfo>();
            Annotation = new Annotation()
            {
                Width = 300,
                Height = 200,
                FlowDirection = FlowDirection,
                Padding = 4,
                Visibility = Visibility.Hidden
            };
        }


        protected override void OnTouchVisualHit(Point position, HitTestResult result)
        {
            base.OnTouchVisualHit(position, result);

            if (result.VisualHit is WordInfo word && HighlightLastWord == null && word.Styles.IsHyperLink)
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

        protected void AnnotationStart(string link)
        {

        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (IsMouseDown) return;

            foreach (var link in HyperLinks)
                if (IsPointOnWordArea(e.GetPosition(this), link))
                    Mouse.SetCursor(Cursors.Hand);
        }

        public override void DrawWord(DrawingVisual visual)
        {
            base.DrawWord(visual);
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

        public override void Render()
        {
            base.Render();


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
