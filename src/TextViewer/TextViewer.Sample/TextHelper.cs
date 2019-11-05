using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using TextViewer;

namespace TextViewerSample
{
    public static class TextHelper
    {
        public static List<Paragraph> GetParagraphs(this string path, bool isContentRtl)
        {
            var paraOffset = 0;
            var paragraphs = new List<Paragraph>();
            var doc = new HtmlDocument();
            doc.Load(path);

            var body = doc.DocumentNode.SelectSingleNode("//body");
            foreach (var p in body.SelectNodes("//p"))
            {
                var para = new Paragraph(paraOffset++, isContentRtl);
                paragraphs.Add(para);
                var style = new TextStyle(isContentRtl);
                var offset = 0;
                p.ParseInnerHtml(para, style, ref offset);
                para.CalculateDirection();
            }

            return paragraphs;
        }

        private static void ParseInnerHtml(this HtmlNode node, Paragraph parent, TextStyle parentStyle, ref int contentOffset)
        {
            var nodeStyle = new TextStyle(parent.Styles.IsRtl, parentStyle);
            if (node.Name == "b")
                nodeStyle.FontWeight = FontWeights.Bold;

            if (node.Name == "img")
            {
                var src = node.GetAttributeValue("src", null);
                if (src != null)
                {
                    if (src.StartsWith("data:image"))
                        src = src.Substring(src.IndexOf("base64,") + 8);

                    nodeStyle.SetImage(src);
                    parent.Words.Add(new ImageWord(contentOffset++, nodeStyle) { Paragraph = parent });
                }
            }

            if (node.Name == "a")
                nodeStyle.HyperRef = node.GetAttributeValue("href", null);

            if (node.HasAttributes)
            {
                var styles = node.GetAttributeValue("style", null);
                if (styles != null)
                    foreach (var entries in styles.Split(';'))
                    {
                        var values = entries.Split(':');
                        switch (values[0].ToLower().Trim())
                        {
                            case "vertical-align":
                                nodeStyle.VerticalAlign = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), values[1], true);
                                break;
                            case "text-align":
                                parent.Styles.TextAlign = (TextAlignment)Enum.Parse(typeof(TextAlignment), values[1], true);
                                break;
                            case "color":
                                nodeStyle.Foreground = (Brush)new BrushConverter().ConvertFromString(values[1]);
                                break;
                            case "direction":
                                nodeStyle.SetDirection(values[1] == "rtl");
                                break;
                            case "font-weight":
                                nodeStyle.FontWeight = int.Parse(values[1]) > 500 ? FontWeights.Bold : FontWeights.Normal;
                                break;
                            case "font-size":
                                nodeStyle.FontSize = double.Parse(values[1].Replace("px", ""));
                                break;
                            case "margin-bottom":
                                nodeStyle.MarginBottom = double.Parse(values[1].Replace("px", ""));
                                break;
                            case "margin-top":
                                nodeStyle.MarginTop = double.Parse(values[1].Replace("px", ""));
                                break;
                            case "margin-left":
                                nodeStyle.MarginLeft = double.Parse(values[1].Replace("px", ""));
                                break;
                            case "margin-right":
                                nodeStyle.MarginRight = double.Parse(values[1].Replace("px", ""));
                                break;
                        }
                    }

                var dir = node.GetAttributeValue("dir", null);
                if (dir != null) nodeStyle.SetDirection(dir == "rtl");

                var w = node.GetAttributeValue("width", null);
                if (w != null) parent.Words.Last().Styles.Width = double.Parse(w);

                var h = node.GetAttributeValue("height", null);
                if (h != null) parent.Words.Last().Styles.Height = double.Parse(h);
            }

            if (node.HasChildNodes)
            {
                foreach (var child in node.ChildNodes)
                {
                    if (child.NodeType == HtmlNodeType.Text)
                    {
                        parent.AddContent(contentOffset, child.InnerText, nodeStyle);
                        contentOffset += child.InnerText.Length;
                    }
                    else
                        child.ParseInnerHtml(parent, nodeStyle, ref contentOffset);
                }
            }
        }
    }
}
