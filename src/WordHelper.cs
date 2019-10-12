using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace SvgTextViewer
{
    public static class WordHelper
    {
        private static readonly Regex RtlCharsPattern = new Regex("[\u061b-\u06f5]+");
        private static readonly Regex LtrCharsPattern = new Regex("[a-zA-Z0-9۰۱۲۳۴۵۶۷۸۹]");
        private static readonly string InertChars = "،.»«[]{}()'/\\:!@#$%^&~*-+\"`";


        public static List<List<WordInfo>> Content = new List<List<WordInfo>>();

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

        public static List<List<WordInfo>> GetWords(this string path, bool isContentRtl)
        {
            var content = File.ReadAllLines(path, Encoding.UTF8);
            Content = new List<List<WordInfo>>();

            foreach (var rawPara in content)
            {
                var offset = 0;
                var words = new List<WordInfo>();

                foreach (var word in rawPara.Split(' '))
                {
                    var splitWords = word.ConvertInertCharToWord();
                    for (var i = 0; i < splitWords.Count; i++)
                    {
                        var w = splitWords[i];
                        var wordInfo = new WordInfo(w, offset, w.IsRtl(isContentRtl));
                        //
                        // define some test styles
                        if (w.Length > 10)
                            wordInfo.Styles.Add(StyleType.FontWeight, new InlineStyle(StyleType.FontWeight, "bold"));
                        if (wordInfo.IsRtl == false)
                            wordInfo.Styles.Add(StyleType.Color, new InlineStyle(StyleType.Color, "Blue"));

                        if (i < splitWords.Count - 1)
                            wordInfo.IsInnerWord = true;

                        words.Add(wordInfo);
                        offset += w.Length;
                    }

                    offset++; // word space
                }

                Content.Add(words);
            }

            return Content;
        }
        

        public static bool IsRtl(this string word, bool isContentRtl)
        {
            var res = LtrCharsPattern.Matches(word).Count == word.Length;

            if (word.Any(c => InertChars.IndexOf(c) < 0) == false)
                return isContentRtl;

            return !res;
        }

        private static List<string> ConvertInertCharToWord(this string word)
        {
            var res = new List<string>();
            var wordBuffer = "";

            foreach (var c in word)
            {
                if (InertChars.IndexOf(c) >= 0)
                {
                    if (wordBuffer.Length > 0)
                    {
                        res.Add(wordBuffer);
                        wordBuffer = "";
                    }
                    res.Add(c.ToString());
                }
                else
                    wordBuffer += c;
            }

            if (wordBuffer.Length > 0)
                res.Add(wordBuffer);

            return res;
        }
    }
}