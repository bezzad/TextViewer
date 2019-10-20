using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace TextViewer
{
    public class SelectableTextViewer : TextViewer
    {
        public static readonly DependencyProperty IsSelectableProperty = DependencyProperty.Register(
            "IsSelectable", typeof(bool), typeof(SelectableTextViewer), new PropertyMetadata(default(bool)));

        public bool IsSelectable
        {
            get => (bool)GetValue(IsSelectableProperty);
            set => SetValue(IsSelectableProperty, value);
        }
        protected Point EmptyPoint { get; set; } = new Point(0, 0);
        protected Point StartSelectionPoint { get; set; }
        protected Point EndSelectionPoint { get; set; }
        protected bool IsMouseDown { get; set; }
        protected Range HighlightRange { get; set; }


        public SelectableTextViewer()
        {
            IsSelectable = true;
            Cursor = Cursors.IBeam;
            HighlightRange = new Range(0, 0);
        }

        // If a child visual object is hit.
        public void CatchHitObject(Point position, bool isStartPoint)
        {
            // Initiate the hit test by setting up a hit test result callback method.
            VisualTreeHelper.HitTest(this, null, result =>
            {
                if (result.VisualHit is WordInfo word)
                {
                    if (isStartPoint)
                        HighlightRange.Start = DrawnWords.IndexOf(word);
                    else
                        HighlightRange.End = DrawnWords.IndexOf(word);
                }

                // Stop the hit test enumeration of objects in the visual tree.
                return HitTestResultBehavior.Stop;
            }, new PointHitTestParameters(position));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsSelectable && IsMouseDown)
            {
                EndSelectionPoint = e.GetPosition(this);
                CatchHitObject(EndSelectionPoint, false);
                HighlightSelectedText();
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            // Retrieve the coordinates of the mouse button event.
            IsMouseDown = true;
            ClearSelection();
            StartSelectionPoint = e.GetPosition(this);
            EndSelectionPoint = StartSelectionPoint;
            CatchHitObject(StartSelectionPoint, true);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            // Retrieve the coordinates of the mouse button event.
            if (IsSelectable && IsMouseDown)
            {
                IsMouseDown = false;
                EndSelectionPoint = e.GetPosition(this);
                CatchHitObject(EndSelectionPoint, false);
                HighlightSelectedText();
            }
        }


        protected void HighlightSelectedText()
        {
            // select words which are within range
            if (HighlightRange.Start != HighlightRange.End)
            {
                var from = Math.Min(HighlightRange.Start, HighlightRange.End);
                var to = Math.Max(HighlightRange.Start, HighlightRange.End);

                for (var i = 0; i < DrawnWords.Count; i++)
                {
                    var word = (WordInfo)DrawnWords[i];
                    if (i >= from && i <= to)
                        word.Select();
                    else
                        word.UnSelect();
                }
            }
            else
            {
                UnSelectWords();
            }
        }


        public void ClearSelection()
        {
            HighlightRange.Start = HighlightRange.End = 0;
            EndSelectionPoint = StartSelectionPoint = EmptyPoint;
            UnSelectWords();
        }

        public void UnSelectWords()
        {
            foreach (var visual in DrawnWords)
                ((WordInfo)visual).UnSelect();
        }
    }
}