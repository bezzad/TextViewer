﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TextViewer
{
    public class WordStyle
    {
        public FlowDirection? Direction { get; set; }
        public TextAlignment? TextAlign { get; set; }
        public bool? Display { get; set; }
        public double? MarginBottom { get; set; }
        public double? MarginLeft { get; set; }
        public double? MarginRight { get; set; }
        public double? MarginTop { get; set; }
        public double? FontSize { get; set; }
        public VerticalAlignment? VerticalAlign { get; set; }
        public Brush Foreground { get; set; }
        public FontWeight? FontWeight { get; set; }
        public string HyperRef { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
        public ImageSource Image { get; set; }


        public WordStyle(bool isRtl = false, WordStyle parentStyle = null)
        {
            AddStyle(parentStyle);
            SetDirection(isRtl);
            MarginBottom = MarginLeft = MarginRight = MarginTop = FontSize = Width = Height = 0;
            FontWeight = FontWeights.Normal;
            VerticalAlign = VerticalAlignment.Center;
            Foreground = Brushes.Black;
            TextAlign = isRtl ? TextAlignment.Right : TextAlignment.Left;
            Display = true;
        }

        public void AddStyle(WordStyle style)
        {
            if (style != null)
                foreach (var prop in typeof(WordStyle).GetProperties())
                    prop.SetValue(this, prop.GetValue(style));
        }

        public void SetDirection(bool isRtl)
        {
            Direction = isRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        public void SetImage(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            Image = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        }

        public void SetImage(string base64)
        {
            SetImage(Convert.FromBase64String(base64));
        }
    }
}
