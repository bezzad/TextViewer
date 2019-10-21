using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TextViewer.Test
{
    [TestClass]
    public class WordInfoTest
    {
        /// <summary>An observable dictionary to which a mock will be subscribed</summary>
        private List<WordInfo> Words { get; set; }
        private Paragraph Parent { get; set; }

        /// <summary>Initialization routine executed before each test is run</summary>
        [TestInitialize]
        public void Setup()
        {
            Parent = new Paragraph(0, false);

            Words = new List<WordInfo>()
            {
                new WordInfo("Test1", 0, WordType.Normal, false) {Paragraph = Parent},
                new WordInfo(" ", 5, WordType.Space, false) {Paragraph = Parent},
                new WordInfo("Test2", 6, WordType.Normal, false) {Paragraph = Parent},
                new WordInfo(" ", 11, WordType.Space, false) {Paragraph = Parent},
                new WordInfo("Test3", 12, WordType.Normal, false) {Paragraph = Parent},
                new WordInfo(" ", 17, WordType.Space, false) {Paragraph = Parent},
                new WordInfo("Test4", 18, WordType.Normal, false) {Paragraph = Parent},
                new WordInfo(" ", 23, WordType.Space, false) {Paragraph = Parent},
                new WordInfo("Test", 24, WordType.Normal | WordType.Attached, false) {Paragraph = Parent},
                new WordInfo(".", 25, WordType.InertChar | WordType.Attached, false) {Paragraph = Parent},
                new WordInfo("5", 26, WordType.Normal, false) {Paragraph = Parent},
                new WordInfo(" ", 29, WordType.Space, false) {Paragraph = Parent},
                new WordInfo("Test6", 30, WordType.Normal, false) {Paragraph = Parent},
                new WordInfo(" ", 33, WordType.Space, false) {Paragraph = Parent},
                new WordInfo("تست۱", 36, WordType.Normal, true), // test without parent paragraph
                new WordInfo(" ", 41, WordType.Space, true), // test without parent paragraph
                new WordInfo("تست۲", 42, WordType.Normal, true), // test without parent paragraph
                new WordInfo(" ", 47, WordType.Space, true), // test without parent paragraph
                new WordInfo("تست۳", 48, WordType.Normal, true), // test without parent paragraph
                new WordInfo("img", 55, WordType.Image, false) {Paragraph = Parent}
            };

            for (var i = 1; i < Words.Count; i++)
            {
                Words[i - 1].NextWord = Words[i];
                Words[i].PreviousWord = Words[i - 1];
            }
        }

        [TestMethod]
        public void GetAttributeTest()
        {
            foreach (var word in Words)
            {
                Assert.AreEqual(word.GetAttribute(StyleType.Color), Brushes.Black);
                Assert.AreEqual(word.GetAttribute(StyleType.VerticalAlign), VerticalAlignment.Center);
                Assert.AreEqual(word.GetAttribute(StyleType.FontWeight), FontWeights.Normal);
                Assert.AreEqual(word.GetAttribute(StyleType.MarginBottom), 0);
                Assert.AreEqual(word.GetAttribute(StyleType.MarginTop), 0);
                Assert.AreEqual(word.GetAttribute(StyleType.MarginLeft), 0);
                Assert.AreEqual(word.GetAttribute(StyleType.MarginRight), 0);
                Assert.AreEqual(word.GetAttribute(StyleType.Height), 0);
                Assert.AreEqual(word.GetAttribute(StyleType.Width), 0);
                Assert.AreEqual(word.GetAttribute(StyleType.FontSize), 0);
                Assert.AreEqual(word.GetAttribute(StyleType.TextAlign), TextAlignment.Justify);
                Assert.AreEqual(word.GetAttribute(StyleType.Display), true);
                Assert.IsNull(word.GetAttribute(StyleType.Image));

                var dir = word.GetAttribute(StyleType.Direction);
                Assert.IsInstanceOfType(dir, typeof(FlowDirection));
                Assert.AreEqual((FlowDirection)dir == FlowDirection.RightToLeft, word.IsRtl);
            }
        }

        [TestMethod]
        public void SetDirectionTest()
        {
            var word = new WordInfo("test", 0, WordType.Normal, true);
            Assert.AreEqual(word.GetAttribute(StyleType.Direction), FlowDirection.RightToLeft);
            Assert.IsTrue(word.IsRtl);

            word.SetDirection(false);
            Assert.AreEqual(word.GetAttribute(StyleType.Direction), FlowDirection.LeftToRight);
            Assert.IsFalse(word.IsRtl);

            word.SetDirection(true);
            Assert.AreEqual(word.GetAttribute(StyleType.Direction), FlowDirection.RightToLeft);
            Assert.IsTrue(word.IsRtl);
        }

        [TestMethod]
        public void GetFormattedTextTest()
        {
            var fontFamily = new FontFamily("Arial");
            foreach (var word in Words)
            {
                Assert.IsNotNull(word.GetFormattedText(fontFamily, 16, 1, 20));
                Assert.IsNotNull(word.Format);
                Assert.IsTrue(word.Width > 0);
                Assert.IsTrue(word.Height > 0);
            }
        }

        [TestMethod]
        public void AddStylesTest()
        {
            var styles = new Dictionary<StyleType, string>
            {
                [StyleType.Display] = "false",
                [StyleType.Color] = "red",
                [StyleType.MarginBottom] = "11"
            };

            foreach (var word in Words)
            {
                word.AddStyles(styles);
                Assert.AreEqual(word.GetAttribute(StyleType.Color), Brushes.Red);
                Assert.AreEqual(word.GetAttribute(StyleType.VerticalAlign), VerticalAlignment.Center);
                Assert.AreEqual(word.GetAttribute(StyleType.FontWeight), FontWeights.Normal);
                Assert.AreEqual(word.GetAttribute(StyleType.MarginBottom), 11.0);
                Assert.AreEqual(word.GetAttribute(StyleType.MarginTop), 0);
                Assert.AreEqual(word.GetAttribute(StyleType.MarginLeft), 0);
                Assert.AreEqual(word.GetAttribute(StyleType.MarginRight), 0);
                Assert.AreEqual(word.GetAttribute(StyleType.Height), 0);
                Assert.AreEqual(word.GetAttribute(StyleType.Width), 0);
                Assert.AreEqual(word.GetAttribute(StyleType.FontSize), 0);
                Assert.AreEqual(word.GetAttribute(StyleType.TextAlign), TextAlignment.Justify);
                Assert.AreEqual(word.GetAttribute(StyleType.Display), false);
                Assert.IsNull(word.GetAttribute(StyleType.Image));
            }
        }

        [TestMethod]
        public void IsImageTest()
        {
        }


        [TestMethod]
        public void ExtraWidthTest()
        {
        }
    }
}
