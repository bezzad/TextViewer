using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TextViewer.Test
{
    [TestClass]
    public class ParagraphTest
    {
        private string _rtlContent;
        private string[] _rtlWords;
        private string _ltrContent;
        private string[] _ltrWords;

        /// <summary>Initialization routine executed before each test is run</summary>
        [TestInitialize]
        public void Setup()
        {
            _rtlContent = "این محتوا برای تست می‌باشد. این content برای تست می‌باشد. this content is for testing. این متن تست پاراگراف می‌باشد";
            _ltrContent = "This is the test content, for test paragraph and text parsing. این متن is for testing";
            _rtlWords = _rtlContent.Split(' ');
            _ltrWords = _ltrContent.Split(' ');
        }

        [TestMethod]
        public void AddContent()
        {
            var rtlPara = new Paragraph(0, true);
            var ltrPara = new Paragraph(0, false);

            rtlPara.AddContent(0, _rtlContent, null);
            ltrPara.AddContent(0, _ltrContent, null);

            // words + spaces + inert words   >   2 * words
            Assert.IsTrue(rtlPara.Words.Count > _rtlWords.Length * 2);
            Assert.IsTrue(ltrPara.Words.Count > _ltrWords.Length * 2);


        }
    }
}
