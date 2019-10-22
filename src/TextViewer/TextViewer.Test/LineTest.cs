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
        /// <summary>An observable dictionary to which a mock will be subscribed</summary>
        private List<WordInfo> LtrWords { get; set; }
        private List<WordInfo> RtlWords { get; set; }
        private Paragraph LtrParent { get; set; }
        private Paragraph RtlParent { get; set; }
        private double _lineHeight = 22.0;
        private readonly double _fontSize = 20.0;
        private readonly Point _startPoint = new Point(1, 1);
        private double _width = 200.0;
        private readonly FontFamily _arial = new FontFamily("Arial");

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

            // set image width and height
            LtrWords.Last().Styles.Add(StyleType.Width, "5");
            LtrWords.Last().Styles.Add(StyleType.Height, "5");
            LtrWords.Last().Styles.Add(StyleType.Image, @"iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAA" +
                                                     @"ACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIB" +
                                                     @"KE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==");

            RtlWords.Last().Styles.Add(StyleType.Width, "5");
            RtlWords.Last().Styles.Add(StyleType.Height, "5");
            RtlWords.Last().Styles.Add(StyleType.Image, @"iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAA" +
                                                        @"ACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIB" +
                                                        @"KE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==");
        }

        [TestMethod]
        public void AddWordTest()
        {
            var line = new Line(_width, LtrParent, _startPoint);

            Assert.IsTrue(line.Height.Equals(0));
            Assert.IsTrue(line.RemainWidth.Equals(_width));

            for (var i = 0; i < LtrWords.Count; i++)
            {
                var word = LtrWords[i];
                Debug.WriteLine($"Word {i}");

                word.GetFormattedText(_arial, _fontSize, 1, _lineHeight);
                Assert.IsNotNull(word.Format);
                Assert.IsTrue(word.Width > 0);
                line.AddWord(word);
                _width -= word.Width;
                Assert.IsTrue(line.Height >= word.Height); // check change line height
                Assert.IsTrue(line.RemainWidth.Equals(_width)); // check remain width
                Assert.IsTrue(true);
                Assert.IsTrue(line.Words.Contains(word));

                if (LtrParent.IsRtl != word.IsRtl)
                {
                    Assert.IsTrue(word.DrawPoint.X.Equals(0));
                    Assert.IsTrue(word.DrawPoint.Y.Equals(0));
                }
                else
                {
                    Assert.IsTrue(word.DrawPoint.X > 0);
                    Assert.IsTrue(word.DrawPoint.Y > 0);

                    if (i > 0)
                    {
                        if (LtrParent.IsRtl)
                            Assert.IsTrue(CompareTo(word.DrawPoint, LtrWords[i - 1].DrawPoint) < 0);
                        else
                            Assert.IsTrue(CompareTo(word.DrawPoint, LtrWords[i - 1].DrawPoint) > 0);
                    }
                }

                if (i > 0) Assert.IsTrue(word.CompareTo(LtrWords[i - 1]) > 0);
            }

            Assert.AreEqual(line.Count, LtrWords.Count);
        }

        [TestMethod]
        public void RenderTest()
        {
            var line = new Line(_width, LtrParent, _startPoint);
            var lineCounter = 0;
            Assert.IsTrue(line.CurrentParagraph.Lines.Count == lineCounter++);

            for (var i = 0; i < LtrWords.Count; i++)
            {
                var word = LtrWords[i];
                Debug.WriteLine($"Word {i}");

                word.GetFormattedText(_arial, _fontSize, 1, _lineHeight);
                line.AddWord(word);
            }

            line.Render(); // test after line rendering
            Assert.IsTrue(line.CurrentParagraph.Lines.Count == lineCounter++);

            Debug.WriteLine(line.RemainWidth);
            Assert.IsTrue(line.RemainWidth < 0);

            for (var i = 0; i < LtrWords.Count; i++)
            {
                var word = LtrWords[i];
                Debug.WriteLine($"Word {i}");

                Assert.IsTrue(word.DrawPoint.X > 0);
                Assert.IsTrue(word.DrawPoint.Y > 0);
                Assert.IsTrue(word.Area.Width.Equals(word.Width));
            }

            line.RemainWidth = 100; // change real remain with to test text-align

            // Test justify text-align
            line.Render(true);
            Assert.IsTrue(line.CurrentParagraph.Lines.Count == lineCounter++);

            for (var i = 0; i < LtrWords.Count; i++)
            {
                var word = LtrWords[i];
                Debug.WriteLine($"Justify Word {i}");

                if (word.Type == WordType.Space)
                    Assert.IsTrue(word.ExtraWidth > 0);
                else
                    Assert.IsTrue(word.ExtraWidth.Equals(0));
            }

            // Test center text-align
            LtrParent.Styles[StyleType.TextAlign] = "center";
            line.Render();
            Assert.IsTrue(line.CurrentParagraph.Lines.Count == lineCounter++);
            Assert.IsTrue(line.Words.First().Area.Location.X.Equals(line.RemainWidth / 2 + _startPoint.X));

            // Test left text-align
            LtrParent.Styles[StyleType.TextAlign] = "left";
            line.Render();
            Assert.IsTrue(line.CurrentParagraph.Lines.Count == lineCounter++);
            Assert.IsTrue(line.Words.First().Area.Location.X.Equals(line.Location.X));

            // Test right text-align
            LtrParent.Styles[StyleType.TextAlign] = "right";
            line.Render();
            Assert.IsTrue(line.CurrentParagraph.Lines.Count == lineCounter);
            Assert.IsTrue(line.Words.First().Area.Location.X.Equals(line.RemainWidth + _startPoint.X));
        }


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
    }
}
