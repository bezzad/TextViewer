using System;
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
                var selectedWordIndex = -1;
                if (result.VisualHit is WordInfo word)
                    selectedWordIndex = DrawnWords.IndexOf(word);
                else if (result.VisualHit is Paragraph para)
                {
                    foreach (var line in para.Lines)
                    {
                        if (line.Location.Y <= position.Y && line.Location.Y + line.Height >= position.Y)
                        {
                            var lastWord = line.Words.LastOrDefault();
                            if (lastWord != null)
                            {
                                if (line.CurrentParagraph.IsRtl && position.X <= lastWord.Area.X || // RTL line
                                    !line.CurrentParagraph.IsRtl && position.X >= lastWord.Area.X)  // LTR line
                                    selectedWordIndex = DrawnWords.IndexOf(lastWord);
                                else
                                    selectedWordIndex = DrawnWords.IndexOf(line.Words.FirstOrDefault());

                                break;
                            }
                        }
                    }
                }
                else // There are no lines! Be sure to click on the space between paragraphs.
                {
                    foreach (var paragraph in PageContent)
                    {
                        if (position.Y < paragraph.Location.Y)
                        {
                            selectedWordIndex = DrawnWords.IndexOf(paragraph.Lines.FirstOrDefault()?.Words.FirstOrDefault());
                            break;
                        }
                    }

                    if (selectedWordIndex < 0)
                        selectedWordIndex = DrawnWords.IndexOf(PageContent.LastOrDefault()?.Lines.LastOrDefault()?.Words.LastOrDefault());
                }

                // Set Highlight Range 
                if (selectedWordIndex >= 0)
                {
                    if (isStartPoint)
                        HighlightRange.Start = selectedWordIndex;
                    else
                        HighlightRange.End = selectedWordIndex;
                }

                // Stop the hit test enumeration of objects in the visual tree.
                return HitTestResultBehavior.Stop;
            }, new PointHitTestParameters(position));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsSelectable && IsMouseDown)
            {
                CatchHitObject(e.GetPosition(this), false);
                HighlightSelectedText();
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            // Retrieve the coordinates of the mouse button event.
            IsMouseDown = true;
            ClearSelection();
            CatchHitObject(e.GetPosition(this), true);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            // Retrieve the coordinates of the mouse button event.
            if (IsSelectable && IsMouseDown)
            {
                IsMouseDown = false;
                CatchHitObject(e.GetPosition(this), false);
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
                    if (DrawnWords[i] is WordInfo word)
                    {
                        if (i >= from && i <= to)
                            word.Select();
                        else
                            word.UnSelect();
                    }
                }
            }
            else
                UnSelectWords();
        }


        public void ClearSelection()
        {
            HighlightRange.Start = HighlightRange.End = 0;
            UnSelectWords();
        }

        public void UnSelectWords()
        {
            foreach (var visual in DrawnWords)
                if (visual is WordInfo word)
                    word.UnSelect();
        }
    }
}