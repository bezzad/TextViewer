using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TextViewer.Test
{
    [TestClass]
    public class WordStyleTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            var rtlStyle = new WordStyle(true);
            Assert.IsNotNull(rtlStyle);
            Assert.AreEqual(FlowDirection.RightToLeft, rtlStyle.Direction);
            Assert.IsTrue(rtlStyle.Display);
            Assert.IsTrue(rtlStyle.IsRtl);
            Assert.IsFalse(rtlStyle.IsLtr);
            Assert.AreEqual(WordStyle.RtlCulture, rtlStyle.Language);
            Assert.AreEqual(Brushes.Black, rtlStyle.Foreground);
            Assert.AreEqual(0, rtlStyle.Width);
            Assert.AreEqual(0, rtlStyle.Height);
            Assert.AreEqual(0, rtlStyle.FontSize);
            Assert.AreEqual(0, rtlStyle.MarginBottom);
            Assert.AreEqual(0, rtlStyle.MarginTop);
            Assert.AreEqual(0, rtlStyle.MarginRight);
            Assert.AreEqual(0, rtlStyle.MarginLeft);
            Assert.AreEqual(FontWeights.Normal, rtlStyle.FontWeight);
            Assert.IsNull(rtlStyle.TextAlign);
            Assert.IsNull(rtlStyle.VerticalAlign);
            Assert.IsNull(rtlStyle.Image);
            Assert.IsNull(rtlStyle.HyperRef);

            rtlStyle.Foreground = Brushes.Red;
            rtlStyle.FontSize = 16;
            rtlStyle.FontWeight = FontWeights.Bold;
            rtlStyle.Width = 100;
            rtlStyle.TextAlign = TextAlignment.Center;

            var ltrStyle = new WordStyle(false, rtlStyle);
            Assert.IsNotNull(ltrStyle);
            Assert.AreEqual(FlowDirection.LeftToRight, ltrStyle.Direction);
            Assert.IsTrue(ltrStyle.Display);
            Assert.IsTrue(ltrStyle.IsLtr);
            Assert.IsFalse(ltrStyle.IsRtl);
            Assert.AreEqual(WordStyle.LtrCulture, ltrStyle.Language);
            Assert.AreEqual(Brushes.Red, ltrStyle.Foreground);
            Assert.AreEqual(100, ltrStyle.Width);
            Assert.AreEqual(0, ltrStyle.Height);
            Assert.AreEqual(16, ltrStyle.FontSize);
            Assert.AreEqual(0, ltrStyle.MarginBottom);
            Assert.AreEqual(0, ltrStyle.MarginTop);
            Assert.AreEqual(0, ltrStyle.MarginRight);
            Assert.AreEqual(0, ltrStyle.MarginLeft);
            Assert.AreEqual(FontWeights.Bold, ltrStyle.FontWeight);
            Assert.AreEqual(TextAlignment.Center, ltrStyle.TextAlign);
            Assert.IsNull(ltrStyle.VerticalAlign);
            Assert.IsNull(ltrStyle.Image);
            Assert.IsNull(ltrStyle.HyperRef);
        }


        [TestMethod]
        public void AddStyleTest()
        {
            var wordStyle = new WordStyle(true)
            {
                Foreground = Brushes.Blue,
                Display = true,
                FontWeight = FontWeights.ExtraBold,
                TextAlign = TextAlignment.Justify,
                VerticalAlign = VerticalAlignment.Center,
                HyperRef = "test:url"
            };

            var newStyle = new WordStyle(true)
            {
                Foreground = Brushes.Red,
                Display = false
            };

            wordStyle.AddStyle(newStyle); // override style2 by style

            Assert.IsNotNull(wordStyle);
            Assert.AreEqual(FlowDirection.RightToLeft, wordStyle.Direction);
            Assert.IsFalse(wordStyle.Display);
            Assert.IsTrue(wordStyle.IsRtl);
            Assert.AreEqual(WordStyle.RtlCulture, wordStyle.Language);
            Assert.AreEqual(Brushes.Red, wordStyle.Foreground);
            Assert.AreEqual(0, wordStyle.Width);
            Assert.AreEqual(0, wordStyle.Height);
            Assert.AreEqual(0, wordStyle.FontSize);
            Assert.AreEqual(0, wordStyle.MarginBottom);
            Assert.AreEqual(0, wordStyle.MarginTop);
            Assert.AreEqual(0, wordStyle.MarginRight);
            Assert.AreEqual(0, wordStyle.MarginLeft);
            Assert.AreEqual(FontWeights.Normal, wordStyle.FontWeight);
            Assert.AreEqual(TextAlignment.Justify, wordStyle.TextAlign);
            Assert.AreEqual(VerticalAlignment.Center, wordStyle.VerticalAlign);
            Assert.IsNull(wordStyle.Image);
            Assert.IsNotNull(wordStyle.HyperRef);
        }

        [TestMethod]
        public void SetDirectionTest()
        {
            var style = new WordStyle(true)
            {
                Foreground = Brushes.Red,
                Display = false
            };

            Assert.IsTrue(style.IsRtl);
            Assert.AreEqual(FlowDirection.RightToLeft, style.Direction);
            style.SetDirection(false);
            Assert.IsTrue(style.IsLtr);
            Assert.AreEqual(FlowDirection.LeftToRight, style.Direction);
            style.SetDirection(true);
            Assert.IsTrue(style.IsRtl);
            Assert.AreEqual(FlowDirection.RightToLeft, style.Direction);
        }

        [TestMethod]
        public void SetImageTest()
        {
            var style = new WordStyle(true)
            {
                Foreground = Brushes.Red,
                Display = false
            };

            Assert.AreEqual(0, style.Width);
            Assert.AreEqual(0, style.Height);
            Assert.IsNull(style.Image);

            // set image width and height
            style.Width = 5.0;
            style.Height = 5.0;
            style.SetImage(@"iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==");

            Assert.AreEqual(5, style.Width);
            Assert.AreEqual(5, style.Height);
            Assert.IsNotNull(style.Image);
        }
    }
}
