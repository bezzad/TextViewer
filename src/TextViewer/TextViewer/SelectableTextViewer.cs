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
        protected WordInfo HighlightFirstWord { get; set; }
        protected WordInfo HighlightLastWord { get; set; }


        public SelectableTextViewer()
        {
            IsSelectable = true;
            Cursor = Cursors.IBeam;
        }


        // If a child visual object is hit.
        public void CatchHitObject(Point position, bool isStartPoint)
        {
            // Initiate the hit test by setting up a hit test result callback method.
            VisualTreeHelper.HitTest(this, null, result =>
            {
                WordInfo selectedWord = null;

                if (result.VisualHit is WordInfo word)
                    selectedWord = word;
                else if (result.VisualHit is Paragraph para)
                {
                    foreach (var line in para.Lines)
                    {
                        if (line.Location.Y <= position.Y && line.Location.Y + line.Height >= position.Y)
                        {
                            var lastWord = line.Words.LastOrDefault();
                            if (lastWord != null)
                            {
                                if (line.CurrentParagraph.Styles.IsRtl && position.X <= lastWord.Area.X || // RTL line
                                    !line.CurrentParagraph.Styles.IsRtl && position.X >= lastWord.Area.X)  // LTR line
                                    selectedWord = lastWord;
                                else
                                    selectedWord = line.Words.FirstOrDefault();

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
                            selectedWord = paragraph.Lines.FirstOrDefault()?.Words.FirstOrDefault();
                            break;
                        }
                    }

                    if (selectedWord == null)
                        selectedWord = PageContent.LastOrDefault()?.Lines.LastOrDefault()?.Words.LastOrDefault();
                }

                // Set Highlight Range 
                if (selectedWord != null)
                {
                    if (isStartPoint)
                        HighlightFirstWord = selectedWord;
                    else
                        HighlightLastWord = selectedWord;
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
            if (DrawnWords?.Count > 0)
            {
                // select words which are within range
                if (HighlightFirstWord != HighlightLastWord)
                {
                    var isFirstWordBeginOfHighlight = HighlightLastWord?.CompareTo(HighlightFirstWord) > 0;
                    var from = isFirstWordBeginOfHighlight ? HighlightFirstWord : HighlightLastWord;
                    var to = isFirstWordBeginOfHighlight ? HighlightLastWord : HighlightFirstWord;

                    foreach (var visual in DrawnWords)
                    {
                        if (visual is WordInfo word)
                        {
                            if (word.CompareTo(from) >= 0 && word.CompareTo(to) <= 0)
                                word.Select();
                            else
                                word.UnSelect();
                        }
                    }
                }
                else
                    UnSelectWords();
            }
        }


        public void ClearSelection()
        {
            HighlightFirstWord = HighlightLastWord = null;
            UnSelectWords();
        }

        public void UnSelectWords()
        {
            if (DrawnWords?.Count > 0)
            {
                foreach (var visual in DrawnWords)
                    if (visual is WordInfo word)
                        word.UnSelect();
            }
        }

        public override void Render()
        {
            base.Render();

            UnSelectWords();
            HighlightSelectedText();
        }
    }
}