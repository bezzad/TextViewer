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


        public static bool IsRtl(this string word)
        {
            return LtrCharsPattern.Matches(word).Count != word.Length;
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
    }
}