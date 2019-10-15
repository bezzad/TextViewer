using System.Collections.Generic;
using System.Linq;

namespace TextViewer
{
    public class Paragraph
    {
        public Paragraph(int offset, bool isRtl)
        {
            Offset = offset;
            Words = new List<WordInfo>();
            IsRtl = isRtl;

            Lines = new List<Line>();
            Styles = new Dictionary<StyleType, string>();
        }


        public int Offset { get; set; }
        public List<WordInfo> Words { get; protected set; }
        public List<Line> Lines { get; protected set; }
        public Dictionary<StyleType, string> Styles { get; protected set; }
        public bool IsRtl { get; set; }



        /// <summary>
        /// Attach content to end of this paragraph by same styles
        /// </summary>
        /// <param name="contentOffset">The offset of attached content inflowing of this paragraph</param>
        /// <param name="content">Text of content which is want to attached at this paragraph</param>
        /// <param name="contentStyle">The given styles will be applied on the all of given content</param>
        public void AddContent(int contentOffset, string content, Dictionary<StyleType, string> contentStyle)
        {
            var wordBuffer = "";
            var offset = contentOffset;

            void AddWord(WordInfo w, bool isInnerWord = false)
            {
                // set last word to inner word for remove space after word
                if (Words.Count > 0)
                {
                    w.PreviousWord = Words.Last();
                    w.PreviousWord.NextWord = w;
                }

                w.IsInnerWord = isInnerWord;
                w.AddStyles(contentStyle);

#if DEBUG
                // Todo: Just for test
                if (w.IsRtl != w.Paragraph.IsRtl)
                {
                    w.Styles.Add(StyleType.Color, "#5555FF");
                    w.Styles.Add(StyleType.FontWeight, "bold");
                }
#endif
                Words.Add(w);
                wordBuffer = ""; // clear buffer
            }

            for (var i = 0; i < content.Length; i++)
            {
                var charPointer = content[i];
                if (charPointer == ' ') // end of word!
                {
                    if (wordBuffer.Length > 0)
                        AddWord(new WordInfo(wordBuffer, offset, wordBuffer.IsRtl(), this));

                    // maybe there are exist multiple sequence space, so we set offset outside of the keeping word buffer.
                    offset = contentOffset + i + 1; // set next word offset
                    continue;
                }
                // is current char a inert char?
                if (WordHelper.InertChars.Contains(charPointer))
                {
                    if (wordBuffer.Length > 0)
                        AddWord(new WordInfo(wordBuffer, offset, wordBuffer.IsRtl(), this), true);

                    // inert char as word
                    AddWord(new WordInfo(charPointer.ToString(), contentOffset + i, charPointer.IsRtl() || IsRtl, this),
                        i + 1 < content.Length && content[i + 1] != ' ');

                    offset = contentOffset + i + 1; // set next word offset
                    continue;
                }
                //
                // keep letter or digit in buffer
                wordBuffer += charPointer;
            }
            //
            // keep last word from buffer
            if (wordBuffer.Length > 0)
                AddWord(new WordInfo(wordBuffer, offset, wordBuffer.IsRtl(), this));
        }
    }
}
