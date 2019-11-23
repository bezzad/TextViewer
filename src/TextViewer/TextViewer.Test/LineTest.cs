using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TextViewer.Test
{
    [TestClass]
    public class LineTest
    {
        private List<WordInfo> LtrWords { get; set; }
        private List<WordInfo> RtlWords { get; set; }
        private Paragraph LtrParent { get; set; }
        private Paragraph RtlParent { get; set; }
        private double _lineHeight = 22.0;
        private const double W = 400.0;
        private readonly Size _resetSize = new Size(W, 0);
        private readonly double _fontSize = 20.0;
        private readonly Point _ltrStartPoint = new Point(1, 1);
        private readonly Point _rtlStartPoint = new Point(W - 1, 1);
        private readonly FontFamily _arial = new FontFamily("Arial");

        public static int CompareTo(Point pLeft, Point pRight)
        {
            if (pLeft.Y > pRight.Y)
                return 1;
            if (pLeft.Y < pRight.Y)
                return -1;
            if (pLeft.X > pRight.X)
                return 1;
            if (pLeft.X < pRight.X)
                return -1;

            return 0;
        }

        public static bool IsClose(double left, double right)
        {
            return Math.Abs(left - right) < 0.5;
        }

        /// <summary>Initialization routine executed before each test is run</summary>
        [TestInitialize]
        public void Setup()
        {
            RtlParent = new Paragraph(0) { Size = _resetSize };
            LtrParent = new Paragraph(1) { Size = _resetSize };
            RtlParent.Styles.SetDirection(true);
            LtrParent.Styles.SetDirection(false);

            LtrWords = new List<WordInfo>()
            {
                new WordInfo("Test1", 0, WordType.Normal) {Paragraph = LtrParent}, // 0
                new SpaceWord(5) {Paragraph = LtrParent}, // 1
                new WordInfo("Test2", 6, WordType.Normal) {Paragraph = LtrParent}, // 2
                new SpaceWord(11) {Paragraph = LtrParent}, // 3
                new WordInfo("Test3", 12, WordType.Normal) {Paragraph = LtrParent}, // 4
                new SpaceWord(17) {Paragraph = LtrParent}, // 5
                new WordInfo("Test4", 18, WordType.Normal) {Paragraph = LtrParent}, // 6
                new SpaceWord(23) {Paragraph = LtrParent}, // 7
                new WordInfo("Test", 24, WordType.Normal | WordType.Attached) {Paragraph = LtrParent}, // 8
                new WordInfo(".", 25, WordType.InertChar | WordType.Attached) {Paragraph = LtrParent}, // 9
                new WordInfo("5", 26, WordType.Normal) {Paragraph = LtrParent}, // 10
                new SpaceWord(29) {Paragraph = LtrParent}, // 11
                new WordInfo("Test6", 30, WordType.Normal) {Paragraph = LtrParent}, // 12
                new SpaceWord(33) {Paragraph = LtrParent}, // 13
                new WordInfo("تست۱", 36, WordType.Normal) {Paragraph = LtrParent, Styles = { Direction = FlowDirection.RightToLeft }}, // 14
                new SpaceWord(41) {Paragraph = LtrParent, Styles = { Direction = FlowDirection.RightToLeft }}, // 15
                new WordInfo("تست۲", 42, WordType.Normal) {Paragraph = LtrParent, Styles = { Direction = FlowDirection.RightToLeft }}, // 16
                new SpaceWord(47) {Paragraph = LtrParent, Styles = { Direction = FlowDirection.RightToLeft }}, // 17
                new WordInfo("تست۳", 48, WordType.Normal) {Paragraph = LtrParent, Styles = { Direction = FlowDirection.RightToLeft }}, // 18
                new ImageWord(55) {Paragraph = LtrParent} // 19
            };

            RtlWords = new List<WordInfo>()
            {
                new WordInfo("تستی", 0, WordType.Normal) {Paragraph = RtlParent, Styles = { Direction = FlowDirection.RightToLeft }}, // 0
                new SpaceWord(5) {Paragraph = RtlParent, Styles = { Direction = FlowDirection.RightToLeft }}, // 1
                new WordInfo("تست۲", 6, WordType.Normal) {Paragraph = RtlParent, Styles = { Direction = FlowDirection.RightToLeft }}, // 2
                new SpaceWord(11) {Paragraph = RtlParent, Styles = { Direction = FlowDirection.RightToLeft }}, // 3
                new WordInfo("تست۳", 12, WordType.Normal) {Paragraph = RtlParent, Styles = { Direction = FlowDirection.RightToLeft }}, // 4
                new SpaceWord(17) {Paragraph = RtlParent, Styles = { Direction = FlowDirection.RightToLeft }}, // 5
                new WordInfo("تست۴", 18, WordType.Normal) {Paragraph = RtlParent, Styles = { Direction = FlowDirection.RightToLeft }}, // 6
                new SpaceWord(23) {Paragraph = RtlParent, Styles = { Direction = FlowDirection.RightToLeft }}, // 7
                new WordInfo("تست", 24, WordType.Normal | WordType.Attached) {Paragraph = RtlParent, Styles = { Direction = FlowDirection.RightToLeft }}, // 8
                new WordInfo(".", 25, WordType.InertChar | WordType.Attached) {Paragraph = RtlParent, Styles = { Direction = FlowDirection.RightToLeft }}, // 9
                new WordInfo("۵", 26, WordType.Normal) {Paragraph = RtlParent}, // 10
                new SpaceWord(29) {Paragraph = RtlParent, Styles = { Direction = FlowDirection.LeftToRight }}, // 11
                new WordInfo("Test6", 30, WordType.Normal) {Paragraph = RtlParent, Styles = { Direction = FlowDirection.LeftToRight }}, // 12
                new SpaceWord(33) {Paragraph = RtlParent, Styles = { Direction = FlowDirection.LeftToRight }}, // 13
                new WordInfo("Test7", 36, WordType.Normal) {Paragraph = RtlParent, Styles = { Direction = FlowDirection.LeftToRight }}, // 14
                new SpaceWord(41) {Paragraph = RtlParent, Styles = { Direction = FlowDirection.RightToLeft }}, // 15
                new WordInfo("تست۸", 42, WordType.Normal) {Paragraph = RtlParent}, // 16
                new SpaceWord(47) {Paragraph = RtlParent, Styles = { Direction = FlowDirection.RightToLeft }}, // 17
                new WordInfo("تست۹", 48, WordType.Normal) {Paragraph = RtlParent, Styles = { Direction = FlowDirection.RightToLeft }}, // 18
                new ImageWord( 55) {Paragraph = RtlParent} // 19
            };

            for (var i = 1; i < LtrWords.Count; i++)
            {
                LtrWords[i - 1].NextWord = LtrWords[i];
                LtrWords[i].PreviousWord = LtrWords[i - 1];

                RtlWords[i - 1].NextWord = RtlWords[i];
                RtlWords[i].PreviousWord = RtlWords[i - 1];
            }

            var img = @"iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";

            // set image width and height
            LtrWords.Last().Styles.Width = 5.0;
            LtrWords.Last().Styles.Height = 5.0;
            LtrWords.Last().Styles.SetImage(img);

            RtlWords.Last().Styles.Width = 5.0;
            RtlWords.Last().Styles.Height = 5.0;
            RtlWords.Last().Styles.SetImage(img);
        }

        [TestMethod]
        public void AddWordTest()
        {
            LtrParent.Styles.TextAlign = TextAlignment.Left;
            RtlParent.Styles.TextAlign = TextAlignment.Right;
            LtrParent.Size = _resetSize;
            RtlParent.Size = _resetSize;
            var ltrLine = LtrParent.AddLine(new Line(_ltrStartPoint));
            var rtlLine = RtlParent.AddLine(new Line(_rtlStartPoint));

            Assert.AreEqual(ltrLine.Height, 0);
            Assert.AreEqual(rtlLine.Height, 0);
            Assert.AreEqual(ltrLine.RemainWidth, W);
            Assert.AreEqual(rtlLine.RemainWidth, W);

            for (var i = 0; i < LtrWords.Count; i++)
            {
                var ltrWord = LtrWords[i];
                var rtlWord = RtlWords[i];
                Debug.WriteLine($"Word {i}");

                ltrWord.SetFormattedText(_arial, _fontSize, 1, _lineHeight);
                rtlWord.SetFormattedText(_arial, _fontSize, 1, _lineHeight);
                if (ltrWord.IsImage || ltrWord.Type == WordType.Space)
                    Assert.IsNull(ltrWord.Format);
                else
                    Assert.IsNotNull(ltrWord.Format);


                if (rtlWord.IsImage || rtlWord.Type == WordType.Space)
                    Assert.IsNull(rtlWord.Format);
                else
                    Assert.IsNotNull(rtlWord.Format);
                Assert.IsTrue(ltrWord.Width > 0);
                Assert.IsTrue(rtlWord.Width > 0);
                ltrLine.AddWord(ltrWord);
                rtlLine.AddWord(rtlWord);
                //W -= ltrWord.Width;
                Assert.IsTrue(ltrLine.Height >= ltrWord.Height); // check change line height
                Assert.IsTrue(rtlLine.Height >= rtlWord.Height); // check change line height

                Assert.IsTrue(IsClose(ltrLine.RemainWidth, W - ltrLine.ActualWidth)); // check remain width
                Assert.IsTrue(ltrLine.Words.Contains(ltrWord));
                Assert.IsTrue(rtlLine.Words.Contains(rtlWord));

                if (LtrParent.Styles.IsRtl != ltrWord.Styles.IsRtl)
                {
                    Assert.AreEqual(ltrWord.DrawPoint.X, 0);
                    Assert.AreEqual(ltrWord.DrawPoint.Y, 0);
                }
                else
                {
                    Assert.IsTrue(ltrWord.DrawPoint.X > 0); // ltr
                    Assert.IsTrue(ltrWord.DrawPoint.Y > 0);

                    if (i > 0)
                        Assert.IsTrue(CompareTo(ltrWord.DrawPoint, LtrWords[i - 1].DrawPoint) > 0); // ltr
                }

                if (RtlParent.Styles.IsRtl != rtlWord.Styles.IsRtl)
                {
                    Assert.AreEqual(rtlWord.DrawPoint.X, 0);
                    Assert.AreEqual(rtlWord.DrawPoint.Y, 0);
                }
                else
                {
                    Assert.IsTrue(rtlWord.DrawPoint.X < W); // rtl
                    Assert.IsTrue(rtlWord.DrawPoint.Y > 0);

                    if (i > 0)
                        Assert.IsTrue(CompareTo(rtlWord.DrawPoint, RtlWords[i - 1].DrawPoint) <= 0); // rtl
                }

                if (i > 0)
                {
                    Assert.IsTrue(ltrWord.CompareTo(LtrWords[i - 1]) > 0);
                    Assert.IsTrue(rtlWord.CompareTo(RtlWords[i - 1]) > 0);
                }
            }

            Assert.AreEqual(ltrLine.Count, LtrWords.Count);
            Assert.AreEqual(rtlLine.Count, RtlWords.Count);
        }


        [TestMethod]
        public void RenderTest()
        {
            LtrParent.Styles.TextAlign = TextAlignment.Left;
            RtlParent.Styles.TextAlign = TextAlignment.Right;
            LtrParent.Size = _resetSize;
            RtlParent.Size = _resetSize;
            var ltrLine = LtrParent.AddLine(new Line(_ltrStartPoint));
            var rtlLine = RtlParent.AddLine(new Line(_rtlStartPoint));
            var lineCounter = 1;
            Assert.AreEqual(ltrLine.CurrentParagraph.Lines.Count, lineCounter);
            Assert.AreEqual(rtlLine.CurrentParagraph.Lines.Count, lineCounter);

            for (var i = 0; i < LtrWords.Count - 1; i++)
            {
                var ltrWord = LtrWords[i];
                var rtlWord = RtlWords[i];
                Debug.WriteLine($"Word {i}");

                ltrWord.SetFormattedText(_arial, _fontSize, 1, _lineHeight);
                rtlWord.SetFormattedText(_arial, _fontSize, 1, _lineHeight);
                ltrLine.AddWord(ltrWord);
                rtlLine.AddWord(rtlWord);
            }

            ltrLine.Build(false); // test after line rendering
            rtlLine.Build(false); // test after line rendering
            Assert.AreEqual(ltrLine.CurrentParagraph.Lines.Count, lineCounter);
            Assert.AreEqual(rtlLine.CurrentParagraph.Lines.Count, lineCounter);
            Assert.IsTrue(ltrLine.RemainWidth < 0);
            Assert.IsTrue(rtlLine.RemainWidth < 0);

            for (var i = 0; i < LtrWords.Count - 1; i++)
            {
                var ltrWord = LtrWords[i];
                var rtlWord = RtlWords[i];
                Debug.WriteLine($"Word {i}");

                Assert.IsTrue(ltrWord.DrawPoint.X > 0);
                Assert.IsTrue(rtlWord.DrawPoint.X < W);
                Assert.IsTrue(ltrWord.DrawPoint.Y > 0);
                Assert.IsTrue(rtlWord.DrawPoint.Y > 0);
                Assert.AreEqual(ltrWord.Area.Width, ltrWord.Width);
                Assert.AreEqual(rtlWord.Area.Width, rtlWord.Width);
            }

            while (rtlLine.RemainWidth < 1)
                rtlLine.CurrentParagraph.Size = new Size(rtlLine.CurrentParagraph.Size.Width + 100, rtlLine.CurrentParagraph.Size.Height);
            while (ltrLine.RemainWidth < 1)
                ltrLine.CurrentParagraph.Size = new Size(ltrLine.CurrentParagraph.Size.Width + 100, ltrLine.CurrentParagraph.Size.Height);

            // Test justify text-align
            ltrLine.Build(true);
            Assert.AreEqual(ltrLine.CurrentParagraph.Lines.Count, lineCounter);
            for (var i = 0; i < ltrLine.Words.Count - 1; i++)
            {
                var w = ltrLine.Words[i];
                Debug.WriteLine($"Justify LTR Word {i}");

                if (w is SpaceWord wSpace) Assert.IsTrue(wSpace.ExtraWidth > 0);
            }
            rtlLine.Build(true);
            Assert.AreEqual(rtlLine.CurrentParagraph.Lines.Count, lineCounter);
            for (var i = 0; i < rtlLine.Words.Count - 1; i++)
            {
                var w = rtlLine.Words[i];
                Debug.WriteLine($"Justify RTL Word {i}");

                if (w is SpaceWord wSpace) Assert.IsTrue(wSpace.ExtraWidth > 0);
            }
            //
            // Test center text-align
            LtrParent.Styles.TextAlign = TextAlignment.Center;
            RtlParent.Styles.TextAlign = TextAlignment.Center;
            ltrLine.Build(false);
            rtlLine.Build(false);
            Assert.AreEqual(ltrLine.CurrentParagraph.Lines.Count, lineCounter);
            Assert.AreEqual(rtlLine.CurrentParagraph.Lines.Count, lineCounter);
            Assert.AreEqual(ltrLine.Words.First().Area.Location.X, ltrLine.RemainWidth / 2 + _ltrStartPoint.X);
            Assert.AreEqual(rtlLine.Words.First().DrawPoint.X, _rtlStartPoint.X - rtlLine.RemainWidth / 2);
            //
            // Test left text-align
            LtrParent.Styles.TextAlign = TextAlignment.Left;
            RtlParent.Styles.TextAlign = TextAlignment.Left;
            ltrLine.Build(false);
            rtlLine.Build(false);
            Assert.AreEqual(ltrLine.CurrentParagraph.Lines.Count, lineCounter);
            Assert.AreEqual(rtlLine.CurrentParagraph.Lines.Count, lineCounter);
            Assert.AreEqual(ltrLine.Words.First().Area.Location.X, ltrLine.Location.X);
            // Assert.IsTrue(IsClose(Math.Abs(rtlLine.Words.Last().Area.Left), Math.Abs(rtlLine.RemainWidth - rtlLine.ActualWidth - rtlLine.Width + rtlLine.Location.X)));
            //
            // Test right text-align
            LtrParent.Styles.TextAlign = TextAlignment.Right;
            RtlParent.Styles.TextAlign = TextAlignment.Right;
            ltrLine.Build(false);
            rtlLine.Build(false);
            Assert.AreEqual(ltrLine.CurrentParagraph.Lines.Count, lineCounter);
            Assert.AreEqual(rtlLine.CurrentParagraph.Lines.Count, lineCounter);
            Assert.AreEqual(ltrLine.Words.First().Area.Location.X, ltrLine.RemainWidth + _ltrStartPoint.X);
            Assert.AreEqual(rtlLine.Words.First().DrawPoint.X, rtlLine.Location.X);
        }

        [TestMethod]
        public void RtlWordsInLtrContentTest()
        {
            var content = "This is a test content for use متن تستی راست به چپ in ltr content";
            var ltrPara = new Paragraph(0);
            var ltrLine = ltrPara.AddLine(new Line(new Point(0, 0)));
            var offset = 0;
            var wordCounter = 0;

            foreach (var word in content.Split(" "))
            {
                var wordInfo = new WordInfo(word, offset, WordType.Normal);
                wordInfo.Styles.SetDirection(Paragraph.IsRtl(word));
                wordInfo.SetFormattedText(_arial, _fontSize, 1, _lineHeight);
                offset += word.Length;
                var space = new SpaceWord(offset++);
                space.Styles.SetDirection(wordInfo.Styles.IsRtl);
                space.SetFormattedText(_arial, _fontSize, 1, _lineHeight);

                ltrLine.AddWord(wordInfo);
                Assert.AreEqual(++wordCounter, ltrLine.Words.Count);
                ltrLine.AddWord(space);
                Assert.AreEqual(++wordCounter, ltrLine.Words.Count);
            }

            ltrLine.Build(false);

            for (var i = 0; i < ltrLine.Words.Count - 1; i++)
            {
                Debug.WriteLine($"Word {i}'th");
                var word = ltrLine.Words[i];
                var nextWord = ltrLine.Words[i + 1];
                if (word.Styles.IsRtl && nextWord.Styles.IsRtl)      // 0X   <--2--  <--1--  NX
                    Assert.IsTrue(word.Area.X > nextWord.Area.X);
                else if (word.Styles.IsLtr && nextWord.Styles.IsLtr) // 0X   --1-->  --2-->  NX
                    Assert.IsTrue(word.Area.X < nextWord.Area.X);
                else
                    Assert.IsTrue(word.Area.X < nextWord.Area.X);
            }
        }

        [TestMethod]
        public void LtrWordsInRtlContentTest()
        {
            var content = "این یک متن راست به چپ برای تست left to right content می‌باشد.";
            var rtlPara = new Paragraph(0);
            rtlPara.Styles.SetDirection(true);
            var rtlLine = rtlPara.AddLine(new Line(new Point(1000, 0)));
            var offset = 0;
            var wordCounter = 0;

            foreach (var word in content.Split(" "))
            {
                var wordInfo = new WordInfo(word, offset, WordType.Normal);
                wordInfo.Styles.SetDirection(Paragraph.IsRtl(word));
                wordInfo.SetFormattedText(_arial, _fontSize, 1, _lineHeight);
                offset += word.Length;
                var space = new SpaceWord(offset++);
                space.Styles.SetDirection(wordInfo.Styles.IsRtl);
                space.SetFormattedText(_arial, _fontSize, 1, _lineHeight);

                rtlLine.AddWord(wordInfo);
                Assert.AreEqual(++wordCounter, rtlLine.Words.Count);
                rtlLine.AddWord(space);
                Assert.AreEqual(++wordCounter, rtlLine.Words.Count);
            }

            rtlLine.Build(false);

            for (var i = 0; i < rtlLine.Words.Count - 1; i++)
            {
                Debug.WriteLine($"Word {i}'th");
                var word = rtlLine.Words[i];
                var nextWord = rtlLine.Words[i + 1];
                if (word.Styles.IsRtl && nextWord.Styles.IsRtl)      // 0X   <--2--  <--1--  1000X
                    Assert.IsTrue(word.Area.X > nextWord.Area.X);
                else if (word.Styles.IsLtr && nextWord.Styles.IsLtr) // 0X   --1-->  --2-->  1000X
                    Assert.IsTrue(word.Area.X < nextWord.Area.X);
                else
                    Assert.IsTrue(word.Area.X > nextWord.Area.X);
            }
        }

    }
}
