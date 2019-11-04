using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace TextViewer.Test
{
    [TestClass]
    public class WordInfoTest
    {
        private List<WordInfo> Words { get; set; }
        private Paragraph Parent { get; set; }

        /// <summary>Initialization routine executed before each test is run</summary>
        [TestInitialize]
        public void Setup()
        {
            Parent = new Paragraph(0, false) { Styles = { VerticalAlign = VerticalAlignment.Top } };

            // cover test for parental styles

            Words = new List<WordInfo>()
            {
                new WordInfo("Test1", 0, WordType.Normal, false, Parent.Styles) {Paragraph = Parent}, // 0
                new SpaceWord(5, false, Parent.Styles) {Paragraph = Parent}, // 1
                new WordInfo("Test2", 6, WordType.Normal, false, Parent.Styles) {Paragraph = Parent}, // 2
                new SpaceWord(11, false, Parent.Styles) {Paragraph = Parent}, // 3
                new WordInfo("Test3", 12, WordType.Normal, false, Parent.Styles) {Paragraph = Parent}, // 4
                new SpaceWord(17, false, Parent.Styles) {Paragraph = Parent}, // 5
                new WordInfo("Test4", 18, WordType.Normal, false, Parent.Styles) {Paragraph = Parent}, // 6
                new SpaceWord(23, false, Parent.Styles) {Paragraph = Parent}, // 7
                new WordInfo("Test", 24, WordType.Normal | WordType.Attached, false, Parent.Styles) {Paragraph = Parent}, // 8
                new WordInfo(".", 25, WordType.InertChar | WordType.Attached, false, Parent.Styles) {Paragraph = Parent}, // 9
                new WordInfo("5", 26, WordType.Normal, false, Parent.Styles) {Paragraph = Parent}, // 10
                new SpaceWord(29, false, Parent.Styles) {Paragraph = Parent}, // 11
                new WordInfo("Test6", 30, WordType.Normal, false, Parent.Styles) {Paragraph = Parent}, // 12
                new SpaceWord(33, false, Parent.Styles) {Paragraph = Parent}, // 13
                new WordInfo("تست۱", 36, WordType.Normal, true, Parent.Styles) {Paragraph = Parent}, // 14
                new SpaceWord(41, true, Parent.Styles) {Paragraph = Parent}, // 15
                new WordInfo("تست۲", 42, WordType.Normal, true, Parent.Styles) {Paragraph = Parent}, // 16
                new SpaceWord(47, true, Parent.Styles) {Paragraph = Parent}, // 17
                new WordInfo("تست۳", 48, WordType.Normal, true, Parent.Styles) {Paragraph = Parent}, // 18
                new ImageWord(55, Parent.Styles) {Paragraph = Parent} // 19
            };

            for (var i = 1; i < Words.Count; i++)
            {
                Words[i - 1].NextWord = Words[i];
                Words[i].PreviousWord = Words[i - 1];
            }

            // set image width and height
            Words.Last().Styles.Width = 5.0;
            Words.Last().Styles.Height = 5.0;
            Words.Last().Styles.SetImage(@"iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==");
        }

        [TestMethod]
        public void GetAttributeTest()
        {
            for (var i = 0; i < Words.Count; i++)
            {
                var word = Words[i];
                Debug.WriteLine($"Word {i}");
                Assert.AreEqual(word.Styles.Foreground, Brushes.Black);
                Assert.AreEqual(word.Styles.VerticalAlign, VerticalAlignment.Top);
                Assert.AreEqual(word.Styles.FontWeight, FontWeights.Normal);
                Assert.AreEqual(word.Styles.MarginBottom, 0.0);
                Assert.AreEqual(word.Styles.MarginTop, 0.0);
                Assert.AreEqual(word.Styles.MarginLeft, 0.0);
                Assert.AreEqual(word.Styles.MarginRight, 0.0);
                Assert.AreEqual(word.Styles.Height, word.Type == WordType.Image ? 5 : 0.0);
                Assert.AreEqual(word.Styles.Width, word.Type == WordType.Image ? 5 : 0.0);
                Assert.AreEqual(word.Styles.FontSize, 0.0);
                Assert.IsNull(word.Styles.TextAlign);
                Assert.AreEqual(word.Styles.Display, true);
                if (word.IsImage)
                    Assert.IsNotNull(word.Styles.Image);
                else
                    Assert.IsNull(word.Styles.Image);

                var dir = word.Styles.Direction;
                Assert.AreEqual(dir == FlowDirection.RightToLeft, word.Styles.IsRtl);
            }
        }

        [TestMethod]
        public void SetDirectionTest()
        {
            var word = new WordInfo("test", 0, WordType.Normal, true);
            Assert.AreEqual(word.Styles.Direction, FlowDirection.RightToLeft);
            Assert.IsTrue(word.Styles.IsRtl);

            word.Styles.SetDirection(false);
            Assert.AreEqual(word.Styles.Direction, FlowDirection.LeftToRight);
            Assert.IsFalse(word.Styles.IsRtl);

            word.Styles.SetDirection(true);
            Assert.AreEqual(word.Styles.Direction, FlowDirection.RightToLeft);
            Assert.IsTrue(word.Styles.IsRtl);
        }

        [TestMethod]
        public void GetFormattedTextTest()
        {
            var fontFamily = new FontFamily("Arial");
            for (var i = 0; i < Words.Count; i++)
            {
                var word = Words[i];
                Debug.WriteLine($"Word {i}");
                word.SetFormattedText(fontFamily, 16, 1, 20);
                if (word is ImageWord imgWord)
                    Assert.IsNull(imgWord.Format);
                else if (word is SpaceWord spaceW)
                    Assert.IsNull(spaceW.Format);
                else
                    Assert.IsNotNull(word.Format);

                Assert.IsTrue(word.Width > 0);
                Assert.IsTrue(word.Height > 0);
            }
        }

        [TestMethod]
        public void AddStylesTest()
        {
            var styles = new TextStyle(false)
            {
                Display = false,
                Foreground = Brushes.Red,
                MarginBottom = 11,
                Width = 12,
                Height = 13
            };

            for (var i = 0; i < Words.Count; i++)
            {
                var word = Words[i];
                Debug.WriteLine($"Word {i}");
                word.Styles.AddStyle(styles);
                Assert.AreEqual(word.Styles.Foreground, Brushes.Red);
                Assert.AreEqual(word.Styles.VerticalAlign, VerticalAlignment.Top);
                Assert.AreEqual(word.Styles.FontWeight, FontWeights.Normal);
                Assert.AreEqual(word.Styles.MarginBottom, 11.0);
                Assert.AreEqual(word.Styles.MarginTop, 0.0);
                Assert.AreEqual(word.Styles.MarginLeft, 0.0);
                Assert.AreEqual(word.Styles.MarginRight, 0.0);
                Assert.AreEqual(word.Styles.Height, 13.0);
                Assert.AreEqual(word.Styles.Width, 12.0);
                Assert.AreEqual(word.Styles.FontSize, 0.0);
                Assert.IsNull(word.Styles.TextAlign);
                Assert.AreEqual(word.Styles.Display, false);
                if (word.IsImage)
                    Assert.IsNotNull(word.Styles.Image);
                else
                    Assert.IsNull(word.Styles.Image);
            }
        }

        [TestMethod]
        public void IsImageTest()
        {
            Assert.IsFalse(Words.First().IsImage);
            Assert.IsTrue(Words.Last().IsImage);
        }

        [TestMethod]
        public void CompareToTest()
        {
            for (var i = 1; i < Words.Count; i++)
            {
                var word = Words[i];
                var beforeW = Words[i - 1];
                Assert.IsTrue(word.CompareTo(beforeW) > 0);
                Assert.IsTrue(word.CompareTo(word) == 0);
                Assert.IsTrue(beforeW.CompareTo(word) < 0);
            }
        }

        [TestMethod]
        public void SelectionTest()
        {
            for (var i = 1; i < Words.Count; i++)
            {
                var word = Words[i];
                Assert.IsFalse(word.IsSelected);
                word.Select();
                Assert.IsTrue(word.IsSelected);
                word.UnSelect();
                Assert.IsFalse(word.IsSelected);
            }
        }

        [TestMethod]
        public void ToStringTest()
        {
            for (var i = 1; i < Words.Count; i++)
            {
                var word = Words[i];
                if (word.IsImage == false)
                    Assert.AreEqual(word.ToString(), $"{word.Offset}/`{word.Text}`/{word.Offset + word.Text.Length - 1}");
            }
        }

        [TestMethod]
        public void IsRtlTest()
        {
            var ltrString = @"âs̱čẕžšṣẓʿġq̈ūōēáæabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890۱۲۳۴۵۶۷۸۹۰";
            var rtlChars = new[]
            {
                'ا', 'ب', 'پ', 'ت', '؟', 'ث', 'ج', 'چ', 'ح', 'خ',
                'د', 'ذ', 'ر', 'ز', 'ژ', 'س', 'ش', 'ص', 'ض', 'ط',
                'ظ', 'ع', 'غ', 'ف', 'ق', 'ک', 'گ', 'ل', 'م', 'ن',
                'و', 'ه', 'ی', 'ء', 'آ', 'ة', 'ك', '؛', 'ؤ', 'ئ',
                'ً', 'ٔ', 'ב', 'א', 'מ', 'ת', 'ש', 'ר', 'ה', 'ۀ',
                '\u064C', '\u0651', '\u0622', '\u06C0', '\u0640',
                '\u0643', '\u0686', '\u064A', '\uFEF0'
            };


            for (var i = 0; i < ltrString.Length; i++)
            {
                var c = ltrString[i];
                Assert.IsFalse(Paragraph.IsRtl(c), $"The {c} char at index {i} is not LTR!");
            }

            for (var i = 0; i < rtlChars.Length; i++)
            {
                var c = rtlChars[i];
                Assert.IsTrue(Paragraph.IsRtl(c), $"The {c} char at index {i} is not RTL!");
            }
        }
    }
}
