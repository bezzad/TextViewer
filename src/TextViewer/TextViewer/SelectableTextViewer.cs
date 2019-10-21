using System;
using System.Diagnostics;
using System.Linq;
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

        private void SetHighlightRange(int index, bool isStart)
        {
            if (isStart)
                HighlightRange.Start = index;
            else
                HighlightRange.End = index;
        }

        // If a child visual object is hit.
        public void CatchHitObject(Point position, bool isStartPoint)
        {
            // Initiate the hit test by setting up a hit test result callback method.
            VisualTreeHelper.HitTest(this, null, result =>
            {
                var selectionFound = false;
                if (result.VisualHit is WordInfo word)
                {
                    SetHighlightRange(DrawnWords.IndexOf(word), isStartPoint);
                    selectionFound = true;
                }
                else
                {
                    foreach (var para in PageContent)
                        foreach (var line in para.Lines)
                        {
                            if (line.Location.Y <= position.Y && line.Location.Y + line.Height >= position.Y)
                            {
                                selectionFound = true;
                                var firstWord = line.Words.FirstOrDefault();
                                var lastWord = line.Words.LastOrDefault();
                                if (firstWord != null && lastWord != null)
                                {
                                    Debug.WriteLine("Selected Line: " + line.Location);
                                    if (line.CurrentParagraph.IsRtl && position.X >= firstWord.Area.X || // RTL line
                                        !line.CurrentParagraph.IsRtl && position.X <= firstWord.Area.X) // LTR line
                                    {
                                        SetHighlightRange(DrawnWords.IndexOf(firstWord), isStartPoint);
                                        Debug.WriteLine($"1 Selected Word Index: {DrawnWords.IndexOf(firstWord)}  isStartPoint: {isStartPoint}");
                                    }
                                    else if (line.CurrentParagraph.IsRtl &&
                                             position.X <= lastWord.Area.X || // RTL line
                                             !line.CurrentParagraph.IsRtl &&
                                             position.X >= lastWord.Area.X) // LTR line
                                    {
                                        SetHighlightRange(DrawnWords.IndexOf(lastWord), isStartPoint);
                                        Debug.WriteLine($"2 Selected Word Index: {DrawnWords.IndexOf(firstWord)}  isStartPoint: {isStartPoint}");
                                    }
                                    else
                                    {
                                        SetHighlightRange(DrawnWords.IndexOf(firstWord), isStartPoint);
                                        Debug.WriteLine($"3 Selected Word Index: {DrawnWords.IndexOf(firstWord)}  isStartPoint: {isStartPoint}");
                                    }
                                }
                                else // There are no lines! Be sure to click on the space between paragraphs.
                                {
                                    Debugger.Break();
                                }
                            }
                        }
                }

                if (selectionFound == false)
                {
                    Debugger.Break();
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
                UnSelectWords();
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