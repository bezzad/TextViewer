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
        private List<WordInfo> Words { get; set; }
        private Paragraph Parent { get; set; }
        private double _lineHeight = 22.0;
        private readonly double _fontSize = 20.0;
        private readonly Point _startPoint = new Point(1, 1);
        private double _width = 200.0;
        private readonly FontFamily _arial = new FontFamily("Arial");

        /// <summary>Initialization routine executed before each test is run</summary>
        [TestInitialize]
        public void Setup()
        {
            Parent = new Paragraph(0, false);

            Words = new List<WordInfo>()
            {
                new WordInfo("Test1", 0, WordType.Normal, false) {Paragraph = Parent}, // 0
                new WordInfo(" ", 5, WordType.Space, false) {Paragraph = Parent}, // 1
                new WordInfo("Test2", 6, WordType.Normal, false) {Paragraph = Parent}, // 2
                new WordInfo(" ", 11, WordType.Space, false) {Paragraph = Parent}, // 3
                new WordInfo("Test3", 12, WordType.Normal, false) {Paragraph = Parent}, // 4
                new WordInfo(" ", 17, WordType.Space, false) {Paragraph = Parent}, // 5
                new WordInfo("Test4", 18, WordType.Normal, false) {Paragraph = Parent}, // 6
                new WordInfo(" ", 23, WordType.Space, false) {Paragraph = Parent}, // 7
                new WordInfo("Test", 24, WordType.Normal | WordType.Attached, false) {Paragraph = Parent}, // 8
                new WordInfo(".", 25, WordType.InertChar | WordType.Attached, false) {Paragraph = Parent}, // 9
                new WordInfo("5", 26, WordType.Normal, false) {Paragraph = Parent}, // 10
                new WordInfo(" ", 29, WordType.Space, false) {Paragraph = Parent}, // 11
                new WordInfo("Test6", 30, WordType.Normal, false) {Paragraph = Parent}, // 12
                new WordInfo(" ", 33, WordType.Space, false) {Paragraph = Parent}, // 13
                new WordInfo("تست۱", 36, WordType.Normal, true) {Paragraph = Parent}, // 14
                new WordInfo(" ", 41, WordType.Space, true) {Paragraph = Parent}, // 15
                new WordInfo("تست۲", 42, WordType.Normal, true) {Paragraph = Parent}, // 16
                new WordInfo(" ", 47, WordType.Space, true) {Paragraph = Parent}, // 17
                new WordInfo("تست۳", 48, WordType.Normal, true) {Paragraph = Parent}, // 18
                new WordInfo("img", 55, WordType.Image, false) {Paragraph = Parent} // 19
            };

            for (var i = 1; i < Words.Count; i++)
            {
                Words[i - 1].NextWord = Words[i];
                Words[i].PreviousWord = Words[i - 1];
            }

            // set image width and height
            Words.Last().Styles.Add(StyleType.Width, "100");
            Words.Last().Styles.Add(StyleType.Height, "100");
        }

        [TestMethod]
        public void AddWordTest()
        {
            var line = new Line(_width, Parent, _startPoint);

            Assert.IsTrue(line.Height.Equals(0));
            Assert.IsTrue(line.RemainWidth.Equals(_width));

            for (var i = 0; i < Words.Count; i++)
            {
                var word = Words[i];
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

                if (Parent.IsRtl != word.IsRtl)
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
                        if (Parent.IsRtl)
                            Assert.IsTrue(CompareTo(word.DrawPoint, Words[i - 1].DrawPoint) < 0);
                        else
                            Assert.IsTrue(CompareTo(word.DrawPoint, Words[i - 1].DrawPoint) > 0);
                    }
                }

                if (i > 0) Assert.IsTrue(word.CompareTo(Words[i - 1]) > 0);
            }
        }

        [TestMethod]
        public void RenderTest()
        {
            var line = new Line(_width, Parent, _startPoint);
            var lineCounter = 0;
            Assert.IsTrue(line.CurrentParagraph.Lines.Count == lineCounter++);

            for (var i = 0; i < Words.Count; i++)
            {
                var word = Words[i];
                Debug.WriteLine($"Word {i}");

                word.GetFormattedText(_arial, _fontSize, 1, _lineHeight);
                line.AddWord(word);
            }

            line.Render(); // test after line rendering
            Assert.IsTrue(line.CurrentParagraph.Lines.Count == lineCounter++);

            Debug.WriteLine(line.RemainWidth);
            Assert.IsTrue(line.RemainWidth < 0);

            for (var i = 0; i < Words.Count; i++)
            {
                var word = Words[i];
                Debug.WriteLine($"Word {i}");

                Assert.IsTrue(word.DrawPoint.X > 0);
                Assert.IsTrue(word.DrawPoint.Y > 0);
                Assert.IsTrue(word.Area.Width.Equals(word.Width));
            }

            line.RemainWidth = 100; // change real remain with to test text-align

            // Test justify text-align
            line.Render(true);
            Assert.IsTrue(line.CurrentParagraph.Lines.Count == lineCounter++);

            for (var i = 0; i < Words.Count; i++)
            {
                var word = Words[i];
                Debug.WriteLine($"Justify Word {i}");

                if (word.Type == WordType.Space)
                    Assert.IsTrue(word.ExtraWidth > 0);
                else
                    Assert.IsTrue(word.ExtraWidth.Equals(0));
            }

            // Test center text-align
            Parent.Styles[StyleType.TextAlign] = "center";
            line.Render();
            Assert.IsTrue(line.CurrentParagraph.Lines.Count == lineCounter++);
            Assert.IsTrue(line.Words.First().Area.Location.X.Equals(line.RemainWidth / 2 + _startPoint.X));

            // Test left text-align
            Parent.Styles[StyleType.TextAlign] = "left";
            line.Render();
            Assert.IsTrue(line.CurrentParagraph.Lines.Count == lineCounter++);
            Assert.IsTrue(line.Words.First().Area.Location.X.Equals(line.Location.X));

            // Test right text-align
            Parent.Styles[StyleType.TextAlign] = "right";
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
