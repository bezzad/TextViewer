using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace TextViewer
{
    public class AnnotationTextViewer : SelectableTextViewer
    {
        public List<WordInfo> HyperLinks { get; protected set; }

        public AnnotationTextViewer()
        {
            HyperLinks = new List<WordInfo>();
        }


        protected override void OnTouchVisualHit(Point position, HitTestResult result)
        {
            base.OnTouchVisualHit(position, result);

            if (result.VisualHit is WordInfo word && HighlightLastWord == null && word.Styles.IsHyperLink)
            {
                // open link

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
    }
}
