using MethodTimer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TextViewer;

namespace TextViewerSample
{
    public static class TextHelper
    {
        private static string SetParagraphStyle(string textLine, Paragraph para)
        {
            if (textLine[0] == '<') // read HTML tag
            {
                if (textLine.StartsWith("<left>", StringComparison.OrdinalIgnoreCase) ||
                    textLine.EndsWith("</left>", StringComparison.OrdinalIgnoreCase))
                {
                    para.Styles[StyleType.TextAlign] = "left";
                    textLine = textLine.Replace("<left>", "").Replace("</left>", "");
                }

                if (textLine.StartsWith("<center>", StringComparison.OrdinalIgnoreCase) ||
                    textLine.EndsWith("</center>", StringComparison.OrdinalIgnoreCase))
                {
                    para.Styles[StyleType.TextAlign] = "center";
                    textLine = textLine.Replace("<center>", "").Replace("</center>", "");
                }

                if (textLine.StartsWith("<right>", StringComparison.OrdinalIgnoreCase) ||
                    textLine.EndsWith("</right>", StringComparison.OrdinalIgnoreCase))
                {
                    para.Styles[StyleType.TextAlign] = "right";
                    textLine = textLine.Replace("<right>", "").Replace("</right>", "");
                }

                if (textLine.IndexOf("<img", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var imgWord = new WordInfo("img", 0, WordType.Image, para.IsRtlDirection) { Paragraph = para };
                    para.Words.Add(imgWord);

                    foreach (var word in textLine.Split(" ", StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (word.StartsWith("width"))
                        {
                            var startVal = word.IndexOf("\"", StringComparison.Ordinal) + 1;
                            var w = word.Substring(startVal,
                                word.LastIndexOf("\"", StringComparison.Ordinal) - startVal);
                            imgWord.Styles.Add(StyleType.Width, w);
                        }
                        else if (word.StartsWith("height"))
                        {
                            var startVal = word.IndexOf("\"", StringComparison.Ordinal) + 1;
                            var h = word.Substring(startVal,
                                word.LastIndexOf("\"", StringComparison.Ordinal) - startVal);
                            imgWord.Styles.Add(StyleType.Height, h);
                        }
                        else if (word.StartsWith("src"))
                        {
                            var startVal = word.IndexOf("\"", StringComparison.Ordinal) + 1;
                            var src = word.Substring(startVal,
                                word.LastIndexOf("\"", StringComparison.Ordinal) - startVal);
                            imgWord.Styles.Add(StyleType.Image, src);
                        }
                    }

                    textLine = "";
                }
            }

            return textLine;
        }

        [Time]
        public static List<Paragraph> GetWords(this string path, bool isContentRtl)
        {
            var allLines = File.ReadAllLines(path, Encoding.UTF8);
            var paragraphs = new List<Paragraph>();
            var paraOffset = 0;

            // read paragraphs
            foreach (var text in allLines)
            {
                var para = new Paragraph(paraOffset++, isContentRtl);
                paragraphs.Add(para);

                // TODO: below codes just used for reading sample text --------------------------------------------------------------------
                var content = SetParagraphStyle(text, para);
                // TODO: upper codes just used for reading sample text --------------------------------------------------------------------

                if (string.IsNullOrEmpty(content) == false)
                    para.AddContent(0, content, new Dictionary<StyleType, string>());
            }

            return paragraphs;
        }
    }
}
