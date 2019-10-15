using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MethodTimer;
using TextViewer;

namespace TextViewerSample
{
    public static class TextHelper
    {
        private static void SetDefaultStyles(this Word word)
        {
            if (word.Text.Length > 10)
                word.Styles.Add(StyleType.FontWeight, "bold");
            if (word.IsRtl == false)
                word.Styles.Add(StyleType.Color, "Blue");
        }


        private static string SetParagraphStyle(string textLine, Paragraph para)
        {
            if (textLine.StartsWith("<left>", StringComparison.OrdinalIgnoreCase) || textLine.EndsWith("</left>", StringComparison.OrdinalIgnoreCase))
            {
                para.Styles[StyleType.TextAlign] = "left";
                return textLine.Replace("<left>", "").Replace("</left>", "");
            }

            if (textLine.StartsWith("<center>", StringComparison.OrdinalIgnoreCase) || textLine.EndsWith("</center>", StringComparison.OrdinalIgnoreCase))
            {
                para.Styles[StyleType.TextAlign] = "center";
                return textLine.Replace("<center>", "").Replace("</center>", "");
            }

            if (textLine.StartsWith("<right>", StringComparison.OrdinalIgnoreCase) || textLine.EndsWith("</right>", StringComparison.OrdinalIgnoreCase))
            {
                para.Styles[StyleType.TextAlign] = "right";
                return textLine.Replace("<right>", "").Replace("</right>", "");
            }

            if (textLine.IndexOf("<img", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var imgWord = new WordInfo("img", 0, para.IsRtl, para);
                para.Words.Add(imgWord);

                foreach (var word in textLine.Split(" ", StringSplitOptions.RemoveEmptyEntries))
                {
                    if (word.StartsWith("width"))
                    {
                        var startVal = word.IndexOf("\"", StringComparison.Ordinal) + 1;
                        var w = word.Substring(startVal, word.LastIndexOf("\"", StringComparison.Ordinal) - startVal);
                        imgWord.Styles.Add(StyleType.Width, w);
                    }
                    else if (word.StartsWith("height"))
                    {
                        var startVal = word.IndexOf("\"", StringComparison.Ordinal) + 1;
                        var h = word.Substring(startVal, word.LastIndexOf("\"", StringComparison.Ordinal) - startVal);
                        imgWord.Styles.Add(StyleType.Height, h);
                    }
                    else if (word.StartsWith("src"))
                    {
                        var startVal = word.IndexOf("\"", StringComparison.Ordinal) + 1;
                        var src = word.Substring(startVal, word.LastIndexOf("\"", StringComparison.Ordinal) - startVal);
                        imgWord.Styles.Add(StyleType.Image, src);
                    }
                }
            }

            return "";
        }

        [Time]
        public static List<Paragraph> GetWords(this string path, bool isContentRtl)
        {
            var allLines = File.ReadAllLines(path, Encoding.UTF8);
            var paragraphs = new List<Paragraph>();
            var paraOffset = 0;
            var wordBuffer = "";

            void AddWord(WordInfo w, Paragraph para, bool isInnerWord = false)
            {
                // set last word to inner word for remove space after word
                if (para.Words.Count > 0)
                {
                    w.PreviousWord = para.Words.Last();
                    w.PreviousWord.NextWord = w;
                }

                w.IsInnerWord = isInnerWord;
                w.SetDefaultStyles();
                para.Words.Add(w);
                wordBuffer = ""; // clear buffer
            }

            // read paragraphs
            foreach (var text in allLines)
            {
                var textLine = text;
                var para = new Paragraph(paraOffset++, new List<WordInfo>(), isContentRtl);
                paragraphs.Add(para);
                var offset = 0;

                for (var i = 0; i < textLine.Length; i++)
                {
                    var charPointer = textLine[i];
                    if (charPointer == ' ') // end of word!
                    {
                        if (wordBuffer.Length > 0)
                            AddWord(new WordInfo(wordBuffer, offset, wordBuffer.IsRtl(), para), para);

                        // maybe there are exist multiple sequence space, so we set offset outside of the keeping word buffer.
                        offset = i + 1;
                        continue;
                    }

                    // TODO: below codes just used for reading sample text --------------------------------------------------------------------
                    if (charPointer == '<') // read HTML tag
                    {
                        textLine = SetParagraphStyle(textLine, para);
                        i = -1;
                        continue; // read again
                    }
                    // TODO: upper codes just used for reading sample text --------------------------------------------------------------------


                    // is current char a inert char?
                    if (WordHelper.InertChars.Contains(charPointer))
                    {
                        if (wordBuffer.Length > 0)
                            AddWord(new WordInfo(wordBuffer, offset, wordBuffer.IsRtl(), para), para, true);

                        // inert char as word
                        AddWord(new WordInfo(charPointer.ToString(), i, charPointer.IsRtl() || para.IsRtl, para), para,
                            i + 1 < textLine.Length && textLine[i + 1] != ' ');
                        offset = i + 1; // 1 is length of inert char from `i` offset
                        continue;
                    }

                    //
                    // keep letter or digit in buffer
                    wordBuffer += charPointer;
                }

                // keep last word from buffer
                if (wordBuffer.Length > 0)
                    AddWord(new WordInfo(wordBuffer, offset, wordBuffer.IsRtl(), para), para);
            }

            return paragraphs;
        }
    }
}
