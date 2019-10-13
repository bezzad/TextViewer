using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using MethodTimer;

namespace TextViewer
{
    public static class WordHelper
    {
        private static readonly Regex RtlCharsPattern = new Regex("[؛؟\u061b-\u06f5]+");
        private static readonly Regex LtrCharsPattern = new Regex("[a-zA-Z0-9۰۱۲۳۴۵۶۷۸۹]");
        private static readonly string InertChars = "\\|«»<>[]{}()'/،.,:!@#$%٪^&~*_-+=~‍‍‍‍\"`×?";

        public static List<Paragraph> Content = new List<Paragraph>();

        /// <summary>
        /// Searches a section of the list for a given element using a binary search
        /// algorithm.
        /// </summary>
        /// <param name="words">list of words, which must be searched</param>
        /// <param name="index">offset to beginning search</param>
        /// <param name="count">count of elements must be searched after offset</param>
        /// <param name="value">the position of mouse on the canvas</param>
        /// <returns></returns>
        public static int BinarySearch(this IList<WordInfo> words, int index, int count, Point value)
        {
            if (index < 0 || count < 0)
                throw new ArgumentOutOfRangeException((index < 0 ? nameof(index) : nameof(count)));
            if (words.Count - index < count)
                throw new ArgumentException("Argument has invalid len from index for count");

            var lo = index;
            var hi = index + count - 1;
            // ReSharper disable once TooWideLocalVariableScope
            int mid; // declared here for performance
            while (lo <= hi)
            {
                mid = (lo + hi) / 2;
                var r = words[mid].CompareTo(value);
                if (r == 0)
                    return mid;
                if (r < 0)
                    hi = mid - 1;
                else
                    lo = mid + 1;
            }

            // return bitwise complement of the first element greater than value.
            // Since hi is less than lo now, ~lo is the correct item.
            return ~lo;
        }

        /// <summary>
        /// Searches a section of the list for a given element using a binary search
        /// algorithm.
        /// </summary>
        public static int BinarySearch(this IList<WordInfo> words, Point value)
        {
            return words.BinarySearch(0, words.Count, value);
        }

        [Time]
        public static List<Paragraph> GetWords(this string path, bool isParaRtl)
        {
            var content = File.ReadAllLines(path, Encoding.UTF8);
            Content = new List<Paragraph>();
            WordInfo lastWordPointer = null;
            var paraOffset = 0;

            // read paragraphs
            foreach (var rawPara in content)
            {
                var offset = 0;
                var words = new List<WordInfo>();
                var para = new Paragraph(paraOffset++, words, isParaRtl);
                var imgTagStarted = false;

                foreach (var word in rawPara.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (word == @"<img")
                    {
                        imgTagStarted = true;
                        words.Add(new WordInfo("img", offset++, isParaRtl));
                        continue;
                    }
                    if (imgTagStarted)
                    {
                        if (word.StartsWith("width"))
                        {
                            var startVal = word.IndexOf("\"", StringComparison.Ordinal) + 1;
                            var w = word.Substring(startVal, word.LastIndexOf("\"", StringComparison.Ordinal) - startVal);
                            words.Last().Styles.Add(StyleType.Width, w);
                        }
                        else if (word.StartsWith("height"))
                        {
                            var startVal = word.IndexOf("\"", StringComparison.Ordinal) + 1;
                            var h = word.Substring(startVal, word.LastIndexOf("\"", StringComparison.Ordinal) - startVal);
                            words.Last().Styles.Add(StyleType.Height, h);
                        }
                        else if (word.StartsWith("src"))
                        {
                            var startVal = word.IndexOf("\"", StringComparison.Ordinal) + 1;
                            var src = word.Substring(startVal, word.LastIndexOf("\"", StringComparison.Ordinal) - startVal);
                            words.Last().Styles.Add(StyleType.Image, src);
                        }
                        if (word == @"/>")
                        {
                            imgTagStarted = false;
                        }
                        continue;
                    }

                    if (word == @"<center>" || word == @"</center>")
                    {
                        para.Styles[StyleType.TextAlign] = "center";
                        continue;
                    }

                    if (word == @"<left>" || word == @"</left>")
                    {
                        para.Styles[StyleType.TextAlign] = "left";
                        continue;;
                    }

                    if (word == @"<right>" || word == @"</right>")
                    {
                        para.Styles[StyleType.TextAlign] = "right";
                        continue;
                    }

                    var splitWords = word.ConvertInertCharsToWord(ref offset, isParaRtl);
                    if (lastWordPointer != null)
                    {
                        var firstW = splitWords.First();
                        firstW.PreviousWord = lastWordPointer;
                        lastWordPointer.NextWord = firstW;
                    }
                    words.AddRange(splitWords);
                    lastWordPointer = splitWords.Last();
                    offset++; // word space
                }

                Content.Add(para);
            }

            return Content;
        }

        
        
        private static void SetStyles(this Word word)
        {
            // set word styles --------------------------------------------------------------------------------
            if (word.Text.Length > 10)
                word.Styles.Add(StyleType.FontWeight, "bold");
            if (word.IsRtl == false)
                word.Styles.Add(StyleType.Color, "Blue");
        }

        private static List<WordInfo> ConvertInertCharsToWord(this string word, ref int offset, bool isContentRtl)
        {
            var res = new List<WordInfo>();
            var wordBuffer = "";

            void AddWord(WordInfo w)
            {
                // set last word to inner word for remove space after word
                if (res.Count > 0)
                {
                    w.PreviousWord = res.Last();
                    w.PreviousWord.NextWord = w;
                    w.PreviousWord.IsInnerWord = true;
                }

                w.SetStyles();
                res.Add(w);
                wordBuffer = ""; // clear buffer
            }

            foreach (var c in word)
            {
                // is current char a inert char?
                if (InertChars.IndexOf(c) >= 0)
                {
                    // first, store buffering word and then keep this char as next word
                    if (wordBuffer.Length > 0)
                    {
                        AddWord(new WordInfo(wordBuffer, offset, wordBuffer.IsRtl()));
                        offset += res.Last().Text.Length;
                    }
                    // inert char as word
                    AddWord(new WordInfo(c.ToString(), offset++, isContentRtl));
                }
                else
                    wordBuffer += c; // keep real word chars
            }

            // keep last word from buffer
            if (wordBuffer.Length > 0)
            {
                AddWord(new WordInfo(wordBuffer, offset, wordBuffer.IsRtl()));
                offset += res.Last().Text.Length;
            }

            return res;
        }

        // All "AL" or "R" of Unicode 6.0 (from http://www.unicode.org/Public/6.0.0/ucd/UnicodeData.txt)
        public static bool IsRtl(this char c)
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
                    if (0x61E <= c && c <= 0x64A) return true;
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
            else if (!char.IsDigit(c) && 
                     Regex.IsMatch(c.ToString(), @"\p{IsArabic}|\p{IsHebrew}")) return true;

            return false;
        }

        /// <summary>
        /// To check whether the given string is Arabic.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Returns True if Arabic</returns>
        public static bool IsRtl(this string input)
        {
            return input.Any(c => c.IsRtl());
        }
    }
}