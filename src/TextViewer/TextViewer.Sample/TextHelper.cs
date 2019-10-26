using MethodTimer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
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
                    para.Styles.TextAlign = TextAlignment.Left;
                    textLine = textLine.Replace("<left>", "").Replace("</left>", "");
                }

                if (textLine.StartsWith("<center>", StringComparison.OrdinalIgnoreCase) ||
                    textLine.EndsWith("</center>", StringComparison.OrdinalIgnoreCase))
                {
                    para.Styles.TextAlign = TextAlignment.Center;
                    textLine = textLine.Replace("<center>", "").Replace("</center>", "");
                }

                if (textLine.StartsWith("<right>", StringComparison.OrdinalIgnoreCase) ||
                    textLine.EndsWith("</right>", StringComparison.OrdinalIgnoreCase))
                {
                    para.Styles.TextAlign = TextAlignment.Right;
                    textLine = textLine.Replace("<right>", "").Replace("</right>", "");
                }

                if (textLine.IndexOf("<img", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var imgWord = new WordInfo("img", 0, WordType.Image, para.Styles.IsRtl) { Paragraph = para };
                    para.Words.Add(imgWord);

                    foreach (var word in textLine.Split(" ", StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (word.StartsWith("width"))
                        {
                            var startVal = word.IndexOf("\"", StringComparison.Ordinal) + 1;
                            var w = word.Substring(startVal,
                                word.LastIndexOf("\"", StringComparison.Ordinal) - startVal);
                            imgWord.Styles.Width = double.Parse(w);
                        }
                        else if (word.StartsWith("height"))
                        {
                            var startVal = word.IndexOf("\"", StringComparison.Ordinal) + 1;
                            var h = word.Substring(startVal,
                                word.LastIndexOf("\"", StringComparison.Ordinal) - startVal);
                            imgWord.Styles.Height = double.Parse(h);
                        }
                        else if (word.StartsWith("src"))
                        {
                            var startVal = word.IndexOf("\"", StringComparison.Ordinal) + 1;
                            var src = word.Substring(startVal,
                                word.LastIndexOf("\"", StringComparison.Ordinal) - startVal);
                            imgWord.Styles.SetImage(src);
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

                var content = SetParagraphStyle(text, para);

                if (string.IsNullOrEmpty(content) == false)
                {
                    var words = content.Split(" ");
                    var paraBuffer = "";
                    var offset = 0;
                    foreach (var word in words)
                    {
                        var isRtl = Paragraph.IsRtl(word);
                        var style = new WordStyle(para.Styles.IsRtl);

                        if (word.Length > 10 || !isRtl)
                        {
                            para.AddContent(offset, paraBuffer, style);
                            offset += paraBuffer.Length;
                            paraBuffer = word + " ";

                            if (word.Length > 10) style.FontWeight = FontWeights.Bold;
                            if (!isRtl) style.Foreground = Brushes.Blue;

                            para.AddContent(offset, paraBuffer, style);
                            offset += paraBuffer.Length;
                            paraBuffer = "";
                        }
                        else
                            paraBuffer += word + " ";
                    }

                    if (paraBuffer.Length > 0)
                        para.AddContent(offset, paraBuffer, new WordStyle(para.Styles.IsRtl));
                }

                para.CalculateDirection();
            }

            return paragraphs;
        }
    }
}
