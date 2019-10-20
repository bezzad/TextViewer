﻿using System;
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
        //protected Brush SelectedBrush { get; set; }
        protected Range HighlightRange { get; set; }


        public SelectableTextViewer()
        {
            IsSelectable = true;
            //SelectedBrush = new SolidColorBrush(Colors.DarkCyan) { Opacity = 0.5 };
            Cursor = Cursors.IBeam;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsMouseDown)
            {
                EndSelectionPoint = e.GetPosition(this);
                //Render();
            }
        }

        // If a child visual object is hit, toggle its opacity to visually indicate a hit.
        public HitTestResultBehavior HitCallback(HitTestResult result)
        {
            if (result.VisualHit is WordInfo word)
            {
                word.Select();
            }

            // Stop the hit test enumeration of objects in the visual tree.
            return HitTestResultBehavior.Stop;
        }


        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            // Retrieve the coordinates of the mouse button event.
            IsMouseDown = true;
            ClearSelection();
            StartSelectionPoint = e.GetPosition(this);
            EndSelectionPoint = StartSelectionPoint;

            // Initiate the hit test by setting up a hit test result callback method.
            VisualTreeHelper.HitTest(this, null, HitCallback, new PointHitTestParameters(StartSelectionPoint));
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            // Retrieve the coordinates of the mouse button event.
            if (IsMouseDown)
            {
                IsMouseDown = false;
                EndSelectionPoint = e.GetPosition(this);
                //Render();
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (IsSelectable && (IsMouseDown || HighlightRange != null))
            {
                HighlightSelectedText(dc);
            }
        }

        protected void HighlightSelectedText()
        {

        }

        protected void HighlightSelectedText(DrawingContext dc)
        {
//            int GetCorrectWordIndex(Point selectedPoint)
//            {
//                var result = -1;

//                if (DrawnWords.LastOrDefault()?.CompareTo(selectedPoint) < 0)
//                    result = DrawnWords.Count - 1;
//                else if (DrawnWords.FirstOrDefault()?.CompareTo(selectedPoint) > 0)
//                    result = 0;
//                else
//                    result = DrawnWords.BinarySearch(selectedPoint); // DrawnWords.FindIndex(w => w.CompareTo(selectedPoint) == 0); 
//#if DEBUG
//                if (result < 0)
//                    dc.DrawEllipse(Brushes.Red, null, selectedPoint, 5, 5);
//#endif

//                return result;
//            }

//            if (StartSelectionPoint.CompareTo(EndSelectionPoint) != 0)
//            {
//                //var forceToFind = StartSelectionPoint.Value.CompareTo(EndSelectionPoint.Value) > 0;
//                var startWord = GetCorrectWordIndex(StartSelectionPoint);
//                var endWord = GetCorrectWordIndex(EndSelectionPoint);

//                if (startWord < 0 && endWord >= 0) // startWord is out of word bound but endWord is in correct range
//                    startWord = GetCorrectWordIndex(StartSelectionPoint);

//                if (endWord < 0 && startWord >= 0) // endWord is out of word bound but startWord is in correct range
//                    endWord = GetCorrectWordIndex(EndSelectionPoint);

//                if ((startWord < 0 || endWord < 0) == false)
//                    HighlightRange = new Range(startWord, endWord);

//                if (IsMouseDown == false)
//                    StartSelectionPoint = EndSelectionPoint = EmptyPoint;
//            }

//            if (HighlightRange == null)
//                return;

//            var from = Math.Min(HighlightRange.Start, HighlightRange.End);
//            var to = Math.Max(HighlightRange.Start, HighlightRange.End);

//            for (var w = from; w <= to; w++)
//            {
//                var currentWord = ((WordInfo)DrawnWords[w]).Area;
//                var isFirstOfLineWord = w == from || !((WordInfo)DrawnWords[w - 1]).DrawPoint.Y.Equals(currentWord.Y);

//                if (isFirstOfLineWord == false)
//                {
//                    var previousWord = (WordInfo)DrawnWords[w - 1];
//                    var startX = previousWord.IsRtl ? previousWord.DrawPoint.X : previousWord.DrawPoint.X + previousWord.Width;
//                    var width = currentWord.X - startX + currentWord.Width;

//                    currentWord = new Rect(new Point(startX, currentWord.Y), new Size(width, currentWord.Height));
//                }
//                dc.DrawRectangle(SelectedBrush, null, currentWord);
//            }
        }

        public void ClearSelection()
        {
            HighlightRange = null;
            EndSelectionPoint = StartSelectionPoint = EmptyPoint;
            foreach (var visual in DrawnWords)
            {
                var word = (WordInfo) visual;
                word.UnSelect();
            }
        }
    }
}