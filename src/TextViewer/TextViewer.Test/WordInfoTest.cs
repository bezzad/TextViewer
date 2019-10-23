﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            Words.Last().Styles.Add(WordStyleType.Width, "5");
            Words.Last().Styles.Add(WordStyleType.Height, "5");
            Words.Last().Styles.Add(WordStyleType.Image, @"iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAA" +
                                                     @"ACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIB" +
                                                     @"KE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==");

            // cover test for parental styles
            Parent.Styles.Add(WordStyleType.VerticalAlign, VerticalAlignment.Top.ToString());
        }

        [TestMethod]
        public void GetAttributeTest()
        {
            for (var i = 0; i < Words.Count; i++)
            {
                var word = Words[i];
                Debug.WriteLine($"Word {i}");
                Assert.AreEqual(word.GetAttribute(WordStyleType.Color), Brushes.Black);
                Assert.AreEqual(word.GetAttribute(WordStyleType.VerticalAlign), VerticalAlignment.Top);
                Assert.AreEqual(word.GetAttribute(WordStyleType.FontWeight), FontWeights.Normal);
                Assert.AreEqual(word.GetAttribute(WordStyleType.MarginBottom), 0.0);
                Assert.AreEqual(word.GetAttribute(WordStyleType.MarginTop), 0.0);
                Assert.AreEqual(word.GetAttribute(WordStyleType.MarginLeft), 0.0);
                Assert.AreEqual(word.GetAttribute(WordStyleType.MarginRight), 0.0);
                Assert.AreEqual(word.GetAttribute(WordStyleType.Height), word.Type == WordType.Image ? 5 : 0.0);
                Assert.AreEqual(word.GetAttribute(WordStyleType.Width), word.Type == WordType.Image ? 5 : 0.0);
                Assert.AreEqual(word.GetAttribute(WordStyleType.FontSize), 0.0);
                Assert.AreEqual(word.GetAttribute(WordStyleType.TextAlign), TextAlignment.Justify);
                Assert.AreEqual(word.GetAttribute(WordStyleType.Display), true);
                if (word.IsImage)
                    Assert.IsNotNull(word.GetAttribute(WordStyleType.Image));
                else
                    Assert.IsNull(word.GetAttribute(WordStyleType.Image));

                var dir = word.GetAttribute(WordStyleType.Direction);
                Assert.IsInstanceOfType(dir, typeof(FlowDirection));
                Assert.AreEqual((FlowDirection) dir == FlowDirection.RightToLeft, word.IsRtl);
            }
        }

        [TestMethod]
        public void SetDirectionTest()
        {
            var word = new WordInfo("test", 0, WordType.Normal, true);
            Assert.AreEqual(word.GetAttribute(WordStyleType.Direction), FlowDirection.RightToLeft);
            Assert.IsTrue(word.IsRtl);

            word.SetDirection(false);
            Assert.AreEqual(word.GetAttribute(WordStyleType.Direction), FlowDirection.LeftToRight);
            Assert.IsFalse(word.IsRtl);

            word.SetDirection(true);
            Assert.AreEqual(word.GetAttribute(WordStyleType.Direction), FlowDirection.RightToLeft);
            Assert.IsTrue(word.IsRtl);
        }

        [TestMethod]
        public void GetFormattedTextTest()
        {
            var fontFamily = new FontFamily("Arial");
            for (var i = 0; i < Words.Count; i++)
            {
                var word = Words[i];
                Debug.WriteLine($"Word {i}");
                Assert.IsNotNull(word.GetFormattedText(fontFamily, 16, 1, 20));
                Assert.IsNotNull(word.Format);
                Assert.IsTrue(word.Width > 0);
                Assert.IsTrue(word.Height > 0);
            }
        }

        [TestMethod]
        public void AddStylesTest()
        {
            var styles = new Dictionary<WordStyleType, string>
            {
                [WordStyleType.Display] = "false",
                [WordStyleType.Color] = "red",
                [WordStyleType.MarginBottom] = "11",
                [WordStyleType.Width] = "12",
                [WordStyleType.Height] = "13",
            };

            for (var i = 0; i < Words.Count; i++)
            {
                var word = Words[i];
                Debug.WriteLine($"Word {i}");
                word.AddStyles(styles);
                Assert.AreEqual(word.GetAttribute(WordStyleType.Color), Brushes.Red);
                Assert.AreEqual(word.GetAttribute(WordStyleType.VerticalAlign), VerticalAlignment.Top);
                Assert.AreEqual(word.GetAttribute(WordStyleType.FontWeight), FontWeights.Normal);
                Assert.AreEqual(word.GetAttribute(WordStyleType.MarginBottom), 11.0);
                Assert.AreEqual(word.GetAttribute(WordStyleType.MarginTop), 0.0);
                Assert.AreEqual(word.GetAttribute(WordStyleType.MarginLeft), 0.0);
                Assert.AreEqual(word.GetAttribute(WordStyleType.MarginRight), 0.0);
                Assert.AreEqual(word.GetAttribute(WordStyleType.Height), 13.0);
                Assert.AreEqual(word.GetAttribute(WordStyleType.Width), 12.0);
                Assert.AreEqual(word.GetAttribute(WordStyleType.FontSize), 0.0);
                Assert.AreEqual(word.GetAttribute(WordStyleType.TextAlign), TextAlignment.Justify);
                Assert.AreEqual(word.GetAttribute(WordStyleType.Display), false);
                if (word.IsImage)
                    Assert.IsNotNull(word.GetAttribute(WordStyleType.Image));
                else
                    Assert.IsNull(word.GetAttribute(WordStyleType.Image));
            }
        }

        [TestMethod]
        public void IsImageTest()
        {
            Assert.IsFalse(Words.First().IsImage);
            Assert.IsTrue(Words.Last().IsImage);
        }


        [TestMethod]
        public void ExtraWidthTest()
        {
            var extendedPad = 5;
            for (var i = 0; i < Words.Count; i++)
            {
                var word = Words[i];
                Debug.WriteLine($"Word {i}");
                var wordWidth = word.Width;
                Assert.IsTrue(word.ExtraWidth.Equals(0));
                word.ExtraWidth = extendedPad;
                if (word.Type.HasFlag(WordType.Attached))
                {
                    Assert.IsTrue(word.ExtraWidth.Equals(0));
                    Assert.IsTrue(word.Width.Equals(wordWidth));
                }
                else
                {
                    Assert.IsTrue(word.ExtraWidth.Equals(5));
                    Assert.IsTrue(word.Width.Equals(wordWidth + extendedPad));
                }
            }
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
