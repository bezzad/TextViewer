using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MethodTimer;
using TextViewer;

namespace TextViewerSample
{
    public static class ContentHelper
    {
        [Time]
        public static List<Paragraph> GetWords(this string path, bool isParaRtl)
        {
            var content = File.ReadAllLines(path, Encoding.UTF8);
            var paras = new List<Paragraph>();
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
                        words.Add(new WordInfo("img", offset++, para.IsRtl, para));
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
                        continue; ;
                    }

                    if (word == @"<right>" || word == @"</right>")
                    {
                        para.Styles[StyleType.TextAlign] = "right";
                        continue;
                    }

                    var splitWords = word.ConvertInertCharsToWord(ref offset, para);
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

                paras.Add(para);
            }

            return paras;
        }

        public static List<WordInfo> ConvertInertCharsToWord(this string word, ref int offset, Paragraph content)
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

                SetStyles(w);
                res.Add(w);
                wordBuffer = ""; // clear buffer
            }

            foreach (var c in word)
            {
                // is current char a inert char?
                if (WordHelper.InertChars.IndexOf(c) >= 0)
                {
                    // first, store buffering word and then keep this char as next word
                    if (wordBuffer.Length > 0)
                    {
                        AddWord(new WordInfo(wordBuffer, offset, wordBuffer.IsRtl(), content));
                        offset += res.Last().Text.Length;
                    }
                    // inert char as word
                    AddWord(new WordInfo(c.ToString(), offset++, content.IsRtl, content));
                }
                else
                    wordBuffer += c; // keep real word chars
            }

            // keep last word from buffer
            if (wordBuffer.Length > 0)
            {
                AddWord(new WordInfo(wordBuffer, offset, wordBuffer.IsRtl(), content));
                offset += res.Last().Text.Length;
            }

            return res;
        }

        private static void SetStyles(Word word)
        {
            // set word styles --------------------------------------------------------------------------------
            if (word.Text.Length > 10)
                word.Styles.Add(StyleType.FontWeight, "bold");
            if (word.IsRtl == false)
                word.Styles.Add(StyleType.Color, "Blue");
        }
    }
}
