using System.Collections.Generic;
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
                new WordInfo("تست۱", 36, WordType.Normal, true){Paragraph = Parent},  // 14
                new WordInfo(" ", 41, WordType.Space, true){Paragraph = Parent},  // 15
                new WordInfo("تست۲", 42, WordType.Normal, true){Paragraph = Parent},  // 16
                new WordInfo(" ", 47, WordType.Space, true){Paragraph = Parent},  // 17
                new WordInfo("تست۳", 48, WordType.Normal, true){Paragraph = Parent},  // 18
                new WordInfo("img", 55, WordType.Image, false) {Paragraph = Parent} // 19
            };

            for (var i = 1; i < Words.Count; i++)
            {
                Words[i - 1].NextWord = Words[i];
                Words[i].PreviousWord = Words[i - 1];
            }

            // set image width and height
            Words.Last().Styles.Add(StyleType.Width, "79");
            Words.Last().Styles.Add(StyleType.Height, "99");
            Words.Last().Styles.Add(StyleType.Image, 
                @"iVBORw0KGgoAAAANSUhEUgAAAE8AAABjCAYAAADaWl3LAAAAAXNSR0IArs" +
                @"4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsQAAA7EAZUrDhsAAA+g" +
                @"SURBVHhe7ZwJfBTVHcf/ex/ZHIRAEoJJCJAUEKRAGgtROaKASijghQUsLV" +
                @"4IAQRFKkVU6qcoRbBV8eIS1CqHRUQgtAUFESwIFrCGIyjnQgybZHez2SPr" +
                @"/N++yc5OZu+Z3RD9fj7z2f+bmZ3jN//33v+9eW9kFRVfu+FnIkJOf38mAu" +
                @"Luee4pD0CyWkXs2sUvk9+rhbh7Hivc1cjP2TYK4iae+rOdoFuzEqyTHqZr" +
                @"AJIefYRaVweSlnlcMeyDSsA2YhSxNW++AppjR4nd4HKBRqEgNnI1lXsx8z" +
                @"z1f3YQMVG4hkmT6VpGSI5wiLy2huxzNXhhTGrbUISwTJ0JCX/7K7Gt2Z1A" +
                @"/30l8cqGpcvIupZIzEIV1YEvwbF2FSRBZKdDIR1/fAoa0zPpmvgTM/HEyo" +
                @"aW9pmgGFAM9hsG0jXxQzLx5PPmgMFcS1PSYp49Ny4eKUmFgSFIrIRDDAsX" +
                @"UCu2iO558a4lsbJxTp9FU9Iiinia7Z+AZutmmmoZxCJeFCXbtjThEKzdpS" +
                @"Zqz5M9MRMS7TaaanlI6YFReR6Wb3zhsIWgvHgOGr+rJAumpYA9Pp4LnE66" +
                @"NrZE5XncygFFqlr/L8jW66GRCWi5yBMTwTziRpqKgpMVoN93nCa8aLVa+O" +
                @"7wAUie+SA0JiXTtV6k8r6IPQ87MVmIcK+9C5lKVTPhkMa6OtCW3BqdFzLe" +
                @"ZTh2kSZ8sdlsoM3oCFUffw71YyfStdITsec1eR3jDdUb/g1dOnclN5GUlA" +
                @"Tt0tpBgiGBbLaYLXDp8iWoYwR0uRqhYexwAKWSbAuVxGlzwFVUTOySm0tg" +
                @"/779kJubS9LIlk8+ho5Z19AUI+btxc08UArvi662ze0CtZs+g+zsXCJcIp" +
                @"M98aa6d+8OlZWVcPr0aSJip06dyO4KhRwM63cQOxxY4fD4l4yXyDlysnPI" +
                @"OZYuXQq3Dr+NbGPBXMBHbrxALfGISrzqp54nv2xWLbq+iAi2c9dOkl7+1n" +
                @"Lyi2C5hOC+kWbfD977B7Wg6RylpaXkd8+ePcSzESVTfGCO4KKs+JZa4hGV" +
                @"eErTFWp5qDxZSbzi2u7XEm975tln6BbGG6qqqAXgsJipFR53jb2HWkCOL5" +
                @"PJmry6uLgYUlK8WRWLEu5D0m78gFriEV225XG56jL5tdRbyC+XzEzxG+7c" +
                @"co8P8T6JEUU8Oe0NLi8vJ79CmExeL9DIqBEmbrf/ug23YaXEIjP9AI16T6" +
                @"UlFVGJp922iYkgHHD+/Dkwm81QVFQEX311CPLz80nZhwsW7EeOHiWVBYp8" +
                @"ViYHZ0YWPUJonK8xkTITxcFK4o033mg6fvv09mQdbiPHP3eGXBPGfNxaHf" +
                @"sBxSYq8dQvvwDQpYDYphpP+Ycibd68mXgCLuU7ykklgTePIqdOHkf2CwcF" +
                @"04o4cdITHKNI/foVNh1/3xf7moTD45PsylwTP1RR9ehBLfGILtt2zofUUY" +
                @"PIk8aLvmi8SG7C7nCQG8IFQxjkRE0NtFn5CsCVH0g6HBKffhxSH3uIeBUe" +
                @"X6VWNR0fF6xlUTiWtCF9qOXFce111BKP6INkBmxfWn83GcxDmAAY0wKFdd" +
                @"KMP1ArMsgrytMnSVwpBD5A9Dh8mEJIESSLIt6X1bWQynhDzyQ91BUOAHte" +
                @"V3BldgTDhndB+cUuUsbp5TKwNkbcjCbU2B2QqVWDzWSCxl59wTx6LCisFt" +
                @"CtehX0DTa44nDCt3VW6JXiDZhZWqx4LLtrrNBTpwItU+5pmPLIyGQns8MF" +
                @"1cxNF6Ym0b2iA8+hcTkhO0EL6cx5GphYz+R0wXGbEzIVMvIA+Q9JqleYoo" +
                @"rXUsEhHU4mcBcbUYNkFhXjDdCmbXS9KCGA/Xl4ntLLDXSNMFIIh0ginoPJ" +
                @"sumrN4J2827SI4LZBoWMWkzs9GTarK4L50gXV+bXZ8h57k0y0B1ii2TZNq" +
                @"OiEdzf7qapwLCNOX2dCZzaBLCrVMC2DS4w5VcmU9n448CBg9D32TKmfPXv" +
                @"By2uMzQY7m4A85a8TlNezp49C8peQyG7eCRdA0QoXGSJKaDiCDd47MPQ7V" +
                @"fDQZbVF2QFxTB5/iJY+eFWmL9mI/l/Sr9hUDRxTkDhpETSCiPrOx0Y1IEb" +
                @"skvmToff3jaEiIZgG/jDnXth+oIlJB0Is90Npzqam4204iOV50le24YiYM" +
                @"0FT28M00Bm8ns7SE4MrQx7OtUB92iY4DgILU48gyEN5M89Ca69u0CRGbih" +
                @"n65ohJSzHkFqjlfCiQ//DnkFBSCzWJk7Cz4sw52ZQfbNnjgbam2emnV/25" +
                @"rA45mZysV12Qiy/SfAfPIIXSkuUYmH6HR6aOhfQNq5gcAaN+ewDdw714Yk" +
                @"mD/Wz5gMY1K1gcs5JnzBWhjLVwWTpc1mb0esmERd0tbXWyH92EUSOgQCyy" +
                @"V3oQ6sGJtFiDs/H4qT9X6FY8MhrnAI+6DFJiLP83cxLsa7OnbsCA6HA6r6" +
                @"5oE8x9NFzgXbuF8wbeF0nQZSlAowKJVkfznTRuVCOjKZbRhwm5ksiE0wJF" +
                @"2jJr8IvhhqP/JuUPb09JjgcYxGY5NofMT2wLDFC/YUud3tF3pdA8o2qYIv" +
                @"ojEbn623g4URBoXUMiKRNjFz47jNxrSLq+xOsj1HryUdAqTNSsuyhPH3Q/" +
                @"K0J+jRmHNduEAenj/hkBYvHsJ6IBfjhFHQeOi/IMf3q7z3tuiNDuYqqhrs" +
                @"JI0itlEpvQ18RjDtsFIfsbhws2gg4ipepGVHqC9/8G3YS0tfgqllU+mawK" +
                @"C3hYuYAsYkNMeyKBBDhw4lwiFl08qa7ECgt0WCSq2jVvSELJ47io5MfGfr" +
                @"T0AUYfv27TTlBddz37ix4Hr0uFCyqRAadQKoteJ0JISUbcWu6vkFO6YNhk" +
                @"Ro0yaFpPHFDj9LBqsMAmFhanKHww4pyW3oGnGyb0yyLR++CJjGeBFZ/tZy" +
                @"wSwZqXBIAhP26HTeEEcs4iJeIJa9vox4odg47PWgf38VTYEoWTck8VzLFo" +
                @"H8gbs9C9OetTuFh9E+//wLcOrUKdi961O6Jnxw+JjZ7H3zLya2HVua7sNu" +
                @"i2y8DJeQxFMd9A6Ori2b4ffEjz/+GOTl5UHxTeGPAuWGM2PuGEMt8Yimwv" +
                @"OHqNl25YoV5NdERw9ECnqf2CiZoFtsRBUvI8PjPdU/hCceeh3GelKiUOl8" +
                @"JkYrKk9RK3KCiqeZ9hC1gg+WGTZ8GPnFrBsOf5o7TzDWExO3y+3zFo2dnh" +
                @"oNwcXjhAg421AKNm7YQC3pcNJRo2ISVraVaprm0W88U+alBANvPtxcFQmi" +
                @"lnligj3UYqLTNm/TBntxFIy4i2cwCAer3PHFLZW4i1dR0XxGD3LfhAnUEo" +
                @"d6Wz35tQ8QYSYSJe7ide7cmVq+/GXhQmqJA9vN5coNLxIIREDxtOu98x6k" +
                @"AjsEsDCvOO47b0Js1DJPheHoW0h+WfDjOJESUDz1nsjbqOGA3U+GBE/Z16" +
                @"NbD7DbPd3xYuIE4f5EZ/k2aoWPX/H4I5oauvsOiH7l5VchLS1N1PkVZ86c" +
                @"ga3bt/pMeIkUvK6k5GSYXlYGpbeNgFkznqRbfInmWwh+xVMe/opaHtwF3a" +
                @"kF8Omne+DQoUPQv+jXZMoU9qTghfqrOUMF++yi6bfD8AZFW/fBOhh400AY" +
                @"PHgQOOxOyOjQARyu5rMxo8VvTzIGkNw4iD/e41HOk6yrq4X6hnrISu8A1a" +
                @"YrYKkzg1ytgHH3joOc3BwoLCwkN2WxWpmbcTR1fIYDPhicnMe+YizfXg6r" +
                @"Vq0i50Xap6WDirleFIl7PQiumz1nOqS3b0fS/M+WRDqWxa943IE8QmN6Z8" +
                @"+a1/Q0L1UZISU5BRK0+mZPmL0h1ka4ae7+wdKhwP4HrwlhRV24yDsPDisJ" +
                @"7lw026g7I2o9CWZbfnnn6tSFWl7wAvGicEFSksQZsM0lXOH8wT+OnDfhEI" +
                @"WMZIy1oHj88g4/ioUHF5qzes54nmQPa71n9BIrZijwb0pILP7xuGmhbewx" +
                @"cq/JJr96nQYWv/hnYiPYFYVfVBNCuWQRtbwYL12GWj8DkwSzLR4EBROCWz" +
                @"7cP2kqKVvYrIHwBWBviHtj/tax8I+BhHscdj03uyKBPAyHwhVd9P+Oec3b" +
                @"b1LLg3CZ9+gUwa+N+StY2cpD6Mb8Eeq+QvsFW4ew3oaes/ad9+DggcNwht" +
                @"k8RqeF/XYbbGqnIdtZcMhHoswFA694epwTE5Oanef394+HgoKuJI0IZluc" +
                @"JBIOWJPx4Z6UBW02zb1R9ubZbayNC3c/Fu7+rM3uhzZez7jxk8gyc+YcIh" +
                @"ySw+iFwiHoZbggC2pkcB+kwi8dWUQ0XBDuudE2Gi/RlAdBz+N+fpdPKNX6" +
                @"uvWb4PPd+3xuDuHeoNB6hN3Gwk+z8P/LLddQNATLYqH/RsP8p2eTj1AgYZ" +
                @"V55vnPCQ4XCwQrJB+uKIEECnTzXMG4oHhYDkvB4ME3wO0jPK8bBLOtiml2" +
                @"CRGucMgdY0rB7mg+QwdFQXGCCeQPf8IFAwPoaDh7xjs11W+QLCZTpswCtc" +
                @"pbQPcvLiKismBzr7amDkwmE0lr9Xro17c35OZ6wg1uawbhthaEwCiALbe4" +
                @"VDLNyCPHPIO7C/v0I822SGAfnKDnxZobbxxAssK48feQBYVlhYsEtsnGZ+" +
                @"++vdQCWP3OasEcEQ4tQrxYwf1wTbdfdIdt20LvjsLm3ujRnu88s/wkxEMP" +
                @"W7xE+D3tR5s3UUsYLJNvueVm8i2D/ALf6RI/CfGww6JrF29wy4ftRBAiJ6" +
                @"9T0zDfnJwcn6ze6sXDm93wz400JQx6Ve/evWnKA3oceuWLLy6ma4AMUq++" +
                @"7P3ARKsXL9Ry7dkFz0BqqmdkKgrnT/B6+rUOpNWKhwIEyo5CrFq9GqZNLw" +
                @"voqWntvDFwq/Y87KANl5KSEmoJk5riHdfcasWz2KxwjtMaEItBQwZTq5WK" +
                @"16fvddBga/DJYmLx2rLXqNVKxRv9m5GklaHR+vbZRcM3/z9GmnT5Xb0hT4" +
                @"to2wYj3LYtC/7PXxCMX4H8+shR2LplC6S2a+tzfUh+1zx4aHLgTzi16goD" +
                @"BVixfAWZ17Fjxw64+847yVcl0YNWr1lL3j1j5wC+7+jTpyeUTXuQNPpxCS" +
                @"Yc0qo9D1nz9ntw8OD/QvKkcImL52nV4c3GwVYCd/ly3wG6JTjYSxOqJ4WL" +
                @"5OJt/mgrmGpMJGDFpcpkhPc3rKNbQ6PWYvJZ8P8PPzKDbo0fkol3+vT3MG" +
                @"HiJHKjciVzIrqIhcVaR44fTyQr8wLdWOmtI6FDVujzXufPfwkystrTVHNW" +
                @"r/B9nxorYlrmNToBMtt2CEs4ZMmSuVAycCjUVft+xAHTd42+g6Zij6S1Le" +
                @"t9TquLhAT9+18ftnB8Xlj4FrUA3l6zDAyJ8Rv4HZNQpbXSqoNkaQH4EcbN" +
                @"HfL3sKfCAAAAAElFTkSuQmCC");

            // cover test for parental styles
            Parent.Styles.Add(StyleType.VerticalAlign, VerticalAlignment.Top.ToString()); 
        }

        [TestMethod]
        public void GetAttributeTest()
        {
            for (var i = 0; i < Words.Count; i++)
            {
                var word = Words[i];
                Debug.WriteLine($"Word {i}");
                Assert.AreEqual(word.GetAttribute(StyleType.Color), Brushes.Black);
                Assert.AreEqual(word.GetAttribute(StyleType.VerticalAlign), VerticalAlignment.Top);
                Assert.AreEqual(word.GetAttribute(StyleType.FontWeight), FontWeights.Normal);
                Assert.AreEqual(word.GetAttribute(StyleType.MarginBottom), 0.0);
                Assert.AreEqual(word.GetAttribute(StyleType.MarginTop), 0.0);
                Assert.AreEqual(word.GetAttribute(StyleType.MarginLeft), 0.0);
                Assert.AreEqual(word.GetAttribute(StyleType.MarginRight), 0.0);
                Assert.AreEqual(word.GetAttribute(StyleType.Height), word.Type == WordType.Image ? 99 : 0.0);
                Assert.AreEqual(word.GetAttribute(StyleType.Width), word.Type == WordType.Image ? 79 : 0.0);
                Assert.AreEqual(word.GetAttribute(StyleType.FontSize), 0.0);
                Assert.AreEqual(word.GetAttribute(StyleType.TextAlign), TextAlignment.Justify);
                Assert.AreEqual(word.GetAttribute(StyleType.Display), true);
                if(word.IsImage)
                    Assert.IsNotNull(word.GetAttribute(StyleType.Image));
                else
                    Assert.IsNull(word.GetAttribute(StyleType.Image));

                var dir = word.GetAttribute(StyleType.Direction);
                Assert.IsInstanceOfType(dir, typeof(FlowDirection));
                Assert.AreEqual((FlowDirection) dir == FlowDirection.RightToLeft, word.IsRtl);
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
            var styles = new Dictionary<StyleType, string>
            {
                [StyleType.Display] = "false",
                [StyleType.Color] = "red",
                [StyleType.MarginBottom] = "11",
                [StyleType.Width] = "12",
                [StyleType.Height] = "13",
            };

            for (var i = 0; i < Words.Count; i++)
            {
                var word = Words[i];
                Debug.WriteLine($"Word {i}");
                word.AddStyles(styles);
                Assert.AreEqual(word.GetAttribute(StyleType.Color), Brushes.Red);
                Assert.AreEqual(word.GetAttribute(StyleType.VerticalAlign), VerticalAlignment.Top);
                Assert.AreEqual(word.GetAttribute(StyleType.FontWeight), FontWeights.Normal);
                Assert.AreEqual(word.GetAttribute(StyleType.MarginBottom), 11.0);
                Assert.AreEqual(word.GetAttribute(StyleType.MarginTop), 0.0);
                Assert.AreEqual(word.GetAttribute(StyleType.MarginLeft), 0.0);
                Assert.AreEqual(word.GetAttribute(StyleType.MarginRight), 0.0);
                Assert.AreEqual(word.GetAttribute(StyleType.Height), 13.0);
                Assert.AreEqual(word.GetAttribute(StyleType.Width), 12.0);
                Assert.AreEqual(word.GetAttribute(StyleType.FontSize), 0.0);
                Assert.AreEqual(word.GetAttribute(StyleType.TextAlign), TextAlignment.Justify);
                Assert.AreEqual(word.GetAttribute(StyleType.Display), false);
                if (word.IsImage)
                    Assert.IsNotNull(word.GetAttribute(StyleType.Image));
                else
                    Assert.IsNull(word.GetAttribute(StyleType.Image));
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
                Assert.AreEqual(word.ToString(), $"{word.OffsetRange.Start}/`{word.Text}`/{word.OffsetRange.End}");
            }
        }
    }
}
