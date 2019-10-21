using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace TextViewer
{
    public class Paragraph : DrawingVisual
    {
        public Paragraph(int offset, bool isRtl)
        {
            Offset = offset;
            IsRtl = isRtl;
            Words = new List<WordInfo>();
            Lines = new List<Line>();
            Styles = new Dictionary<StyleType, string>();
        }


        public new int Offset { get; set; }
        public List<WordInfo> Words { get; protected set; }
        public List<Line> Lines { get; protected set; }
        public Dictionary<StyleType, string> Styles { get; protected set; }
        public bool IsRtl { get; set; }
        public Size Size { get; set; }
        public Point Location { get; set; }



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

            void AddWord(WordInfo w)
            {
                w.Paragraph = this;

                // set last word to inner word for remove space after word
                if (Words.Count > 0)
                {
                    w.PreviousWord = Words.Last();
                    w.PreviousWord.NextWord = w;
                }
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
                // is current char a space char?
                if (charPointer == ' ')
                {
                    if (wordBuffer.Length > 0)
                        AddWord(new WordInfo(wordBuffer, offset, WordType.Normal, wordBuffer.IsRtl()));

                    // add space char as word
                    // note: space.IsRtl will complete calculate after adding all words
                    var spaceIsRtl = IsRtl == Words.LastOrDefault()?.IsRtl;
                    AddWord(new WordInfo(charPointer.ToString(), contentOffset + i,
                        WordType.Space, spaceIsRtl));

                    // maybe there are exist multiple sequence space, so we set offset outside of the keeping word buffer.
                    offset = contentOffset + i + 1; // set next word offset
                    continue;
                }
                // is current char a inert char?
                if (WordHelper.InertChars.Contains(charPointer))
                {
                    if (wordBuffer.Length > 0)
                        AddWord(new WordInfo(wordBuffer, offset, WordType.Normal | WordType.Attached, wordBuffer.IsRtl()));

                    // add inert char as word
                    var isInnerWord = i + 1 < content.Length && content[i + 1] != ' ';
                    AddWord(new WordInfo(charPointer.ToString(), contentOffset + i,
                            isInnerWord ? WordType.Attached | WordType.InertChar : WordType.InertChar,
                            charPointer.IsRtl() || IsRtl));

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
                AddWord(new WordInfo(wordBuffer, offset, WordType.Normal, wordBuffer.IsRtl()));
            //
            // calculate all space rtl from last word to first word
            foreach (var space in Words.Where(w => w.Type.HasFlag(WordType.Space)).Reverse())
            {
                //  Paragraph.IsRtl  | Word.IsRtl  | Space.IsRtl 
                // ------------------|-------------|--------------
                //        True       |    True     |    True
                //        True       |    False    | NextWord.IsRtl
                //        False      |    True     | NextWord.IsRtl
                //        False      |    False    |    False
                //

                // space.IsRtl default value is: (Paragraph.IsRtl == Word.IsRtl)
                if (space.IsRtl || space.NextWord == null)
                    space.SetDirection(IsRtl);
                else
                    space.SetDirection(space.NextWord.IsRtl);
            }
        }

        public DrawingVisual Render()
        {
            var dc = RenderOpen();
            dc.DrawGeometry(Brushes.Transparent, null, new RectangleGeometry(new Rect(Location, Size)));
            dc.Close();

            return this;
        }
    }
}
