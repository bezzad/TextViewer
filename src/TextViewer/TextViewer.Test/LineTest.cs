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
        private double _width = 200.0;
        private readonly double _fontSize = 20.0;
        private readonly Point _ltrStartPoint = new Point(1, 1);
        private readonly Point _rtlStartPoint = new Point(199, 1);
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

        /// <summary>Initialization routine executed before each test is run</summary>
        [TestInitialize]
        public void Setup()
        {
            RtlParent = new Paragraph(0, true);
            LtrParent = new Paragraph(1, false);

            LtrWords = new List<WordInfo>()
            {
                new WordInfo("Test1", 0, WordType.Normal, false) {Paragraph = LtrParent}, // 0
                new WordInfo(" ", 5, WordType.Space, false) {Paragraph = LtrParent}, // 1
                new WordInfo("Test2", 6, WordType.Normal, false) {Paragraph = LtrParent}, // 2
                new WordInfo(" ", 11, WordType.Space, false) {Paragraph = LtrParent}, // 3
                new WordInfo("Test3", 12, WordType.Normal, false) {Paragraph = LtrParent}, // 4
                new WordInfo(" ", 17, WordType.Space, false) {Paragraph = LtrParent}, // 5
                new WordInfo("Test4", 18, WordType.Normal, false) {Paragraph = LtrParent}, // 6
                new WordInfo(" ", 23, WordType.Space, false) {Paragraph = LtrParent}, // 7
                new WordInfo("Test", 24, WordType.Normal | WordType.Attached, false) {Paragraph = LtrParent}, // 8
                new WordInfo(".", 25, WordType.InertChar | WordType.Attached, false) {Paragraph = LtrParent}, // 9
                new WordInfo("5", 26, WordType.Normal, false) {Paragraph = LtrParent}, // 10
                new WordInfo(" ", 29, WordType.Space, false) {Paragraph = LtrParent}, // 11
                new WordInfo("Test6", 30, WordType.Normal, false) {Paragraph = LtrParent}, // 12
                new WordInfo(" ", 33, WordType.Space, false) {Paragraph = LtrParent}, // 13
                new WordInfo("تست۱", 36, WordType.Normal, true) {Paragraph = LtrParent}, // 14
                new WordInfo(" ", 41, WordType.Space, true) {Paragraph = LtrParent}, // 15
                new WordInfo("تست۲", 42, WordType.Normal, true) {Paragraph = LtrParent}, // 16
                new WordInfo(" ", 47, WordType.Space, true) {Paragraph = LtrParent}, // 17
                new WordInfo("تست۳", 48, WordType.Normal, true) {Paragraph = LtrParent}, // 18
                new WordInfo("img", 55, WordType.Image, false) {Paragraph = LtrParent} // 19
            };

            RtlWords = new List<WordInfo>()
            {
                new WordInfo("تستی", 0, WordType.Normal, true) {Paragraph = RtlParent}, // 0
                new WordInfo(" ", 5, WordType.Space, true) {Paragraph = RtlParent}, // 1
                new WordInfo("تست۲", 6, WordType.Normal, true) {Paragraph = RtlParent}, // 2
                new WordInfo(" ", 11, WordType.Space, true) {Paragraph = RtlParent}, // 3
                new WordInfo("تست۳", 12, WordType.Normal, true) {Paragraph = RtlParent}, // 4
                new WordInfo(" ", 17, WordType.Space, true) {Paragraph = RtlParent}, // 5
                new WordInfo("تست۴", 18, WordType.Normal, true) {Paragraph = RtlParent}, // 6
                new WordInfo(" ", 23, WordType.Space, true) {Paragraph = RtlParent}, // 7
                new WordInfo("تست", 24, WordType.Normal | WordType.Attached, true) {Paragraph = RtlParent}, // 8
                new WordInfo(".", 25, WordType.InertChar | WordType.Attached, true) {Paragraph = RtlParent}, // 9
                new WordInfo("۵", 26, WordType.Normal, true) {Paragraph = RtlParent}, // 10
                new WordInfo(" ", 29, WordType.Space, false) {Paragraph = RtlParent}, // 11
                new WordInfo("Test6", 30, WordType.Normal, false) {Paragraph = RtlParent}, // 12
                new WordInfo(" ", 33, WordType.Space, false) {Paragraph = RtlParent}, // 13
                new WordInfo("Test7", 36, WordType.Normal, false) {Paragraph = RtlParent}, // 14
                new WordInfo(" ", 41, WordType.Space, true) {Paragraph = RtlParent}, // 15
                new WordInfo("تست۸", 42, WordType.Normal, true) {Paragraph = RtlParent}, // 16
                new WordInfo(" ", 47, WordType.Space, true) {Paragraph = RtlParent}, // 17
                new WordInfo("تست۹", 48, WordType.Normal, true) {Paragraph = RtlParent}, // 18
                new WordInfo("img", 55, WordType.Image, false) {Paragraph = RtlParent} // 19
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
            var ltrLine = new Line(_width, LtrParent, _ltrStartPoint);
            var rtlLine = new Line(_width, RtlParent, _rtlStartPoint);

            Assert.AreEqual(ltrLine.Height, 0);
            Assert.AreEqual(rtlLine.Height, 0);
            Assert.AreEqual(ltrLine.RemainWidth, _width);
            Assert.AreEqual(rtlLine.RemainWidth, _width);

            for (var i = 0; i < LtrWords.Count; i++)
            {
                var ltrWord = LtrWords[i];
                var rtlWord = RtlWords[i];
                Debug.WriteLine($"Word {i}");

                ltrWord.GetFormattedText(_arial, _fontSize, 1, _lineHeight);
                rtlWord.GetFormattedText(_arial, _fontSize, 1, _lineHeight);
                Assert.IsNotNull(ltrWord.Format);
                Assert.IsNotNull(rtlWord.Format);
                Assert.IsTrue(ltrWord.Width > 0);
                Assert.IsTrue(rtlWord.Width > 0);
                ltrLine.AddWord(ltrWord);
                rtlLine.AddWord(rtlWord);
                _width -= ltrWord.Width;
                Assert.IsTrue(ltrLine.Height >= ltrWord.Height); // check change line height
                Assert.IsTrue(rtlLine.Height >= rtlWord.Height); // check change line height
                Assert.AreEqual(ltrLine.RemainWidth, _width); // check remain width
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
                    Assert.IsTrue(rtlWord.DrawPoint.X < 200); // rtl
                    Assert.IsTrue(rtlWord.DrawPoint.Y > 0);

                    if (i > 0)
                        Assert.IsTrue(CompareTo(rtlWord.DrawPoint, RtlWords[i - 1].DrawPoint) < 0); // rtl
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
            var ltrLine = new Line(_width, LtrParent, _ltrStartPoint);
            var rtlLine = new Line(_width, RtlParent, _rtlStartPoint);
            var lineCounter = 0;
            Assert.AreEqual(ltrLine.CurrentParagraph.Lines.Count, lineCounter);
            Assert.AreEqual(rtlLine.CurrentParagraph.Lines.Count, lineCounter++);

            for (var i = 0; i < LtrWords.Count; i++)
            {
                var ltrWord = LtrWords[i];
                var rtlWord = RtlWords[i];
                Debug.WriteLine($"Word {i}");

                ltrWord.GetFormattedText(_arial, _fontSize, 1, _lineHeight);
                rtlWord.GetFormattedText(_arial, _fontSize, 1, _lineHeight);
                ltrLine.AddWord(ltrWord);
                rtlLine.AddWord(rtlWord);
            }

            ltrLine.Render(false); // test after line rendering
            rtlLine.Render(false); // test after line rendering
            Assert.AreEqual(ltrLine.CurrentParagraph.Lines.Count, lineCounter);
            Assert.AreEqual(rtlLine.CurrentParagraph.Lines.Count, lineCounter++);
            Assert.IsTrue(ltrLine.RemainWidth < 0);
            Assert.IsTrue(rtlLine.RemainWidth < 0);

            for (var i = 0; i < LtrWords.Count; i++)
            {
                var ltrWord = LtrWords[i];
                var rtlWord = RtlWords[i];
                Debug.WriteLine($"Word {i}");

                Assert.IsTrue(ltrWord.DrawPoint.X > 0);
                Assert.IsTrue(rtlWord.DrawPoint.X < 200);
                Assert.IsTrue(ltrWord.DrawPoint.Y > 0);
                Assert.IsTrue(rtlWord.DrawPoint.Y > 0);
                Assert.AreEqual(ltrWord.Area.Width, ltrWord.Width);
                Assert.AreEqual(rtlWord.Area.Width, rtlWord.Width);
            }

            ltrLine.RemainWidth = 100; // change real remain with to test text-align
            rtlLine.RemainWidth = 100; // change real remain with to test text-align

            // Test justify text-align
            ltrLine.Render(true);
            rtlLine.Render(true);
            Assert.AreEqual(ltrLine.CurrentParagraph.Lines.Count, lineCounter);
            Assert.AreEqual(rtlLine.CurrentParagraph.Lines.Count, lineCounter++);
            for (var i = 0; i < LtrWords.Count; i++)
            {
                var ltrWord = LtrWords[i];
                var rtlWord = RtlWords[i];
                Debug.WriteLine($"Justify Word {i}");

                if (ltrWord.Type == WordType.Space) Assert.IsTrue(ltrWord.ExtraWidth > 0);
                else Assert.AreEqual(ltrWord.ExtraWidth, 0);

                if (rtlWord.Type == WordType.Space) Assert.IsTrue(rtlWord.ExtraWidth > 0);
                else Assert.AreEqual(rtlWord.ExtraWidth, 0);
            }
            //
            // Test center text-align
            LtrParent.Styles.TextAlign = TextAlignment.Center;
            RtlParent.Styles.TextAlign = TextAlignment.Center;
            ltrLine.Render(false);
            rtlLine.Render(false);
            Assert.AreEqual(ltrLine.CurrentParagraph.Lines.Count, lineCounter);
            Assert.AreEqual(rtlLine.CurrentParagraph.Lines.Count, lineCounter++);
            Assert.AreEqual(ltrLine.Words.First().Area.Location.X, ltrLine.RemainWidth / 2 + _ltrStartPoint.X);
            Assert.AreEqual(rtlLine.Words.First().DrawPoint.X, _rtlStartPoint.X - rtlLine.RemainWidth / 2);
            //
            // Test left text-align
            LtrParent.Styles.TextAlign = TextAlignment.Left;
            RtlParent.Styles.TextAlign = TextAlignment.Left;
            ltrLine.Render(false);
            rtlLine.Render(false);
            Assert.AreEqual(ltrLine.CurrentParagraph.Lines.Count, lineCounter);
            Assert.AreEqual(rtlLine.CurrentParagraph.Lines.Count, lineCounter++);
            Assert.AreEqual(ltrLine.Words.First().Area.Location.X, ltrLine.Location.X);
            Assert.IsTrue(Math.Abs(rtlLine.Words.Last().Area.Location.X) - Math.Abs(rtlLine.RemainWidth - rtlLine.ActualWidth - 200 + rtlLine.Location.X) < 0.00000000001);
            //
            // Test right text-align
            LtrParent.Styles.TextAlign = TextAlignment.Right;
            RtlParent.Styles.TextAlign = TextAlignment.Right;
            ltrLine.Render(false);
            rtlLine.Render(false);
            Assert.AreEqual(ltrLine.CurrentParagraph.Lines.Count, lineCounter);
            Assert.AreEqual(rtlLine.CurrentParagraph.Lines.Count, lineCounter);
            Assert.AreEqual(ltrLine.Words.First().Area.Location.X, ltrLine.RemainWidth + _ltrStartPoint.X);
            Assert.AreEqual(rtlLine.Words.First().DrawPoint.X, rtlLine.Location.X);
        }

        [TestMethod]
        public void RtlWordsInLtrContentTest()
        {
            var content = "This is a test content for use متن تستی راست به چپ in ltr content";
            var ltrPara = new Paragraph(0, false);
            var ltrLine = new Line(_width, ltrPara, new Point(0, 0));
            var offset = 0;
            var wordCounter = 0;

            foreach (var word in content.Split(" "))
            {
                var wordInfo = new WordInfo(word, offset, WordType.Normal, Paragraph.IsRtl(word));
                wordInfo.GetFormattedText(_arial, _fontSize, 1, _lineHeight);
                offset += word.Length;
                var space = new WordInfo(" ", offset++, WordType.Space, wordInfo.Styles.IsRtl);
                space.GetFormattedText(_arial, _fontSize, 1, _lineHeight);

                ltrLine.AddWord(wordInfo);
                Assert.AreEqual(++wordCounter, ltrLine.Words.Count);
                ltrLine.AddWord(space);
                Assert.AreEqual(++wordCounter, ltrLine.Words.Count);
            }

            ltrLine.Render(false);

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
            var rtlPara = new Paragraph(0, true);
            var rtlLine = new Line(_width, rtlPara, new Point(1000, 0));
            var offset = 0; 
            var wordCounter = 0;

            foreach (var word in content.Split(" "))
            {
                var wordInfo = new WordInfo(word, offset, WordType.Normal, Paragraph.IsRtl(word));
                wordInfo.GetFormattedText(_arial, _fontSize, 1, _lineHeight);
                offset += word.Length;
                var space = new WordInfo(" ", offset++, WordType.Space, wordInfo.Styles.IsRtl);
                space.GetFormattedText(_arial, _fontSize, 1, _lineHeight);

                rtlLine.AddWord(wordInfo);
                Assert.AreEqual(++wordCounter, rtlLine.Words.Count);
                rtlLine.AddWord(space);
                Assert.AreEqual(++wordCounter, rtlLine.Words.Count);
            }

            rtlLine.Render(false);

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
