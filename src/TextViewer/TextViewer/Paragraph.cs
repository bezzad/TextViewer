﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace TextViewer
{
    public class Paragraph : DrawingVisual
    {
        public Paragraph(int offset)
        {
            Offset = offset;
            Words = new List<WordInfo>();
            Lines = new List<Line>();
            Styles = new TextStyle();
        }

        protected List<Range> LinesOffsetRange => Lines.Select(l => new Range(l.StartOffset, l.EndOffset)).ToList();
        public const string InertChars = "\\|«»<>[]{}()'/،.,:!@#$%٪^&~*_-+=~‍‍‍‍\"`×?";
        public new int Offset { get; set; }
        public List<WordInfo> Words { get; protected set; }
        public List<Line> Lines { get; protected set; }
        public TextStyle Styles { get; protected set; }
        public Size Size { get; set; }
        public Point Location { get; set; }
        public Paragraph NextParagraph { get; set; }
        public Paragraph PreviousParagraph { get; set; }
        public int StartCharOffset => LinesOffsetRange.FirstOrDefault().Start;
        public int EndCharOffset => LinesOffsetRange.LastOrDefault().End;



        private WordInfo AddWord(WordInfo w)
        {
            w.Paragraph = this;

            // set last word to inner word for remove space after word
            if (Words.Count > 0)
            {
                w.PreviousWord = Words.Last();
                w.PreviousWord.NextWord = w;
            }
            Words.Add(w);

            return w;
        }

        public Line AddLine(Line line)
        {
            Lines.Add(line);
            line.CurrentParagraph = this;

            return line;
        }

        public void ClearLines()
        {
            Lines.Clear();
        }

        /// <summary>
        /// Attach content to end of this paragraph by same styles
        /// </summary>
        /// <param name="contentOffset">The offset of attached content inflowing of this paragraph</param>
        /// <param name="content">Text of content which is want to attached at this paragraph</param>
        /// <param name="contentStyle">The given styles will be applied on the all of given content</param>
        public void AddContent(int contentOffset, string content, TextStyle contentStyle)
        {
            var wordBuffer = "";
            var offset = contentOffset;

            for (var i = 0; i < content.Length; i++)
            {
                var charPointer = content[i];
                // is current char a space char?
                if (charPointer == ' ')
                {
                    if (wordBuffer.Length > 0)
                        AddWord(new WordInfo(wordBuffer, offset, WordType.Normal, contentStyle))
                            .Styles.SetDirection(IsRtl(wordBuffer));

                    wordBuffer = "";

                    // add space char as word
                    // note: space.IsRtl will complete calculate after adding all words
                    var spaceIsRtl = Styles.IsRtl == Words.LastOrDefault()?.Styles.IsRtl;
                    AddWord(new SpaceWord(contentOffset + i, contentStyle))
                        .Styles.SetDirection(spaceIsRtl);

                    // maybe there are exist multiple sequence space, so we set offset outside of the keeping word buffer.
                    offset = contentOffset + i + 1; // set next word offset
                    continue;
                }
                // is current char a inert char?
                if (InertChars.Contains(charPointer))
                {
                    if (wordBuffer.Length > 0)
                        AddWord(new WordInfo(wordBuffer, offset, WordType.Normal | WordType.Attached, contentStyle))
                            .Styles.SetDirection(IsRtl(wordBuffer));

                    wordBuffer = "";

                    // add inert char as word
                    var isInnerWord = i + 1 < content.Length && content[i + 1] != ' ';
                    var inertIsRtl = IsRtl(charPointer) || Styles.IsRtl == Words.LastOrDefault()?.Styles.IsRtl;
                    AddWord(new WordInfo(charPointer.ToString(), contentOffset + i,
                        isInnerWord ? WordType.Attached | WordType.InertChar : WordType.InertChar, contentStyle))
                        .Styles.SetDirection(inertIsRtl);

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
                AddWord(new WordInfo(wordBuffer, offset, WordType.Normal, contentStyle))
                    .Styles.SetDirection(IsRtl(wordBuffer));
        }

        public void CalculateDirection()
        {
            //
            // calculate all spaces rtl from last word to first word
            foreach (var space in Words.Where(w => w.Type.HasFlag(WordType.Space) || w.Type.HasFlag(WordType.InertChar)).Reverse())
            {
                //  Paragraph.IsRtl  | Word.IsRtl  | Space.IsRtl 
                // ------------------|-------------|--------------
                //        True       |    True     |    True
                //        True       |    False    | NextWord.IsRtl
                //        False      |    True     | NextWord.IsRtl
                //        False      |    False    |    False
                //

                // space.IsRtl default value is: (Paragraph.IsRtl == Word.IsRtl)
                if (space.Styles.IsRtl || space.NextWord == null)
                    space.Styles.SetDirection(Styles.IsRtl);
                else
                    space.Styles.SetDirection(space.NextWord.Styles.IsRtl);
            }
        }
        public int GetLineIndex(int charOffset)
        {
            var index = LinesOffsetRange.BinarySearch(Range.Create(charOffset));
            if (index < 0) throw new KeyNotFoundException(nameof(charOffset) + ": " + charOffset);

            return index;
        }


        public void Build(double maxWidth, FontFamily fontFamily, double fontSize, double pixelsPerDip, double lineHeight, bool isJustify)
        {
            ClearLines(); // clear old lines
            var startPoint = Location = new Point(Styles.IsRtl ? maxWidth : 0, 0);
            Size = new Size(maxWidth, 0);

            // create new line buffer, without cleaning last line
            var lineBuffer = AddLine(new Line(startPoint));

            void RemoveSpaceFromEndOfLine()
            {
                // Note: end of line has no space (is important point for justify)
                while (lineBuffer.Words.LastOrDefault()?.Type.HasFlag(WordType.Space) == true)
                {
                    lineBuffer.Words.RemoveAt(lineBuffer.Words.Count - 1);
                }
            }

            foreach (var word in Words)
            {
                word.SetFormattedText(fontFamily, fontSize, pixelsPerDip, lineHeight);
                var wordWidth = word.Width;
                var wordPointer = word;
                //
                // render attached words as one word by one width
                while (word.PreviousWord?.Type.HasFlag(WordType.Attached) == false && wordPointer.Type.HasFlag(WordType.Attached))
                {
                    wordPointer = wordPointer.NextWord;
                    wordWidth += wordPointer.Width;
                }

                if (lineBuffer.RemainWidth - wordWidth <= 0)
                {
                    if (lineBuffer.Count > 0)
                    {
                        RemoveSpaceFromEndOfLine();
                        lineBuffer.Build(isJustify);
                        startPoint.X = Location.X;
                        startPoint.Y += lineBuffer.Height; // new line
                        lineBuffer = AddLine(new Line(startPoint)); // create new line buffer, without cleaning last line
                    }
                    else // the current word width is more than a line!
                    {
                        if (word.IsImage && word is ImageWord imgWord) // set image scale according by image and page width
                            imgWord.ImageScale = lineBuffer.RemainWidth / word.Styles.Width;
                        else if (word.Format != null)
                            word.Format.MaxTextWidth = Math.Abs(lineBuffer.RemainWidth);
                    }
                }

                // Note: The line should not start with space char
                if (lineBuffer.Count > 0 || word.Type.HasFlag(WordType.Space) == false)
                {
                    lineBuffer.AddWord(word);
                }
            }

            RemoveSpaceFromEndOfLine();
            lineBuffer.Build(false);  // last line of paragraph has no justified!
            startPoint.X = Location.X;
            startPoint.Y += lineBuffer.Height; // new line
            Size = new Size(maxWidth, startPoint.Y - Location.Y);
        }


        public DrawingVisual Render()
        {
            using (var dc = RenderOpen())
                dc.DrawGeometry(Brushes.Transparent, null, new RectangleGeometry(new Rect(Location, Size)));

            return this;
        }

        // All "AL" or "R" of Unicode 6.0 (from http://www.unicode.org/Public/6.0.0/ucd/UnicodeData.txt)
        public static bool IsRtl(char c)
        {
            if (c >= 0x5BE && c <= 0x10B7F)
            {
                if (c <= 0x85E)
                {
                    if (c == 0x5BE) return true;
                    if (c == 0x5C0) return true;
                    if (c == 0x5C3) return true;
                    if (c == 0x5C6) return true;
                    if (0x5D0 <= c && c <= 0x5EA) return true;
                    if (0x5F0 <= c && c <= 0x5F4) return true;
                    if (c == 0x608) return true;
                    if (c == 0x60B) return true;
                    if (c == 0x60D) return true;
                    if (c == 0x61B) return true;
                    if (0x61E <= c && c <= 0x656) return true;
                    if (0x66D <= c && c <= 0x66F) return true;
                    if (0x671 <= c && c <= 0x6D5) return true;
                    if (0x6E5 <= c && c <= 0x6E6) return true;
                    if (0x6EE <= c && c <= 0x6EF) return true;
                    if (0x6FA <= c && c <= 0x70D) return true;
                    if (c == 0x710) return true;
                    if (0x712 <= c && c <= 0x72F) return true;
                    if (0x74D <= c && c <= 0x7A5) return true;
                    if (c == 0x7B1) return true;
                    if (0x7C0 <= c && c <= 0x7EA) return true;
                    if (0x7F4 <= c && c <= 0x7F5) return true;
                    if (c == 0x7FA) return true;
                    if (0x800 <= c && c <= 0x815) return true;
                    if (c == 0x81A) return true;
                    if (c == 0x824) return true;
                    if (c == 0x828) return true;
                    if (0x830 <= c && c <= 0x83E) return true;
                    if (0x840 <= c && c <= 0x858) return true;
                    if (c == 0x85E) return true;
                }
                else if (c == 0x200F) return true;
                else if (c >= 0xFB1D)
                {
                    if (c == 0xFB1D) return true;
                    if (0xFB1F <= c && c <= 0xFB28) return true;
                    if (0xFB2A <= c && c <= 0xFB36) return true;
                    if (0xFB38 <= c && c <= 0xFB3C) return true;
                    if (c == 0xFB3E) return true;
                    if (0xFB40 <= c && c <= 0xFB41) return true;
                    if (0xFB43 <= c && c <= 0xFB44) return true;
                    if (0xFB46 <= c && c <= 0xFBC1) return true;
                    if (0xFBD3 <= c && c <= 0xFD3D) return true;
                    if (0xFD50 <= c && c <= 0xFD8F) return true;
                    if (0xFD92 <= c && c <= 0xFDC7) return true;
                    if (0xFDF0 <= c && c <= 0xFDFC) return true;
                    if (0xFE70 <= c && c <= 0xFE74) return true;
                    if (0xFE76 <= c && c <= 0xFEFC) return true;
                    if (0x10800 <= c && c <= 0x10805) return true;
                    if (c == 0x10808) return true;
                    if (0x1080A <= c && c <= 0x10835) return true;
                    if (0x10837 <= c && c <= 0x10838) return true;
                    if (c == 0x1083C) return true;
                    if (0x1083F <= c && c <= 0x10855) return true;
                    if (0x10857 <= c && c <= 0x1085F) return true;
                    if (0x10900 <= c && c <= 0x1091B) return true;
                    if (0x10920 <= c && c <= 0x10939) return true;
                    if (c == 0x1093F) return true;
                    if (c == 0x10A00) return true;
                    if (0x10A10 <= c && c <= 0x10A13) return true;
                    if (0x10A15 <= c && c <= 0x10A17) return true;
                    if (0x10A19 <= c && c <= 0x10A33) return true;
                    if (0x10A40 <= c && c <= 0x10A47) return true;
                    if (0x10A50 <= c && c <= 0x10A58) return true;
                    if (0x10A60 <= c && c <= 0x10A7F) return true;
                    if (0x10B00 <= c && c <= 0x10B35) return true;
                    if (0x10B40 <= c && c <= 0x10B55) return true;
                    if (0x10B58 <= c && c <= 0x10B72) return true;
                    if (0x10B78 <= c && c <= 0x10B7F) return true;
                }
            }
            else if (!char.IsDigit(c) && Regex.IsMatch(c.ToString(), @"\p{IsArabic}|\p{IsHebrew}")) return true;

            return false;
        }

        /// <summary>
        /// To check whether the given string is Arabic.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Returns True if Arabic</returns>
        public static bool IsRtl(string input)
        {
            return input.Any(IsRtl);
        }

    }
}
