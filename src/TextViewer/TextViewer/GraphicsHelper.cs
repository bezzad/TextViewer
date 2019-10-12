using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TextViewer
{
    public static class GraphicsHelper
    {
        public static double PixelsPerDip(Visual visual = null)
        {
            return GetDpiInfo(visual).PixelsPerDip;
        }

        public static DpiScale GetDpiInfo(Visual visual = null)
        {
            return VisualTreeHelper.GetDpi(visual ?? System.Windows.Application.Current.MainWindow);
        }

        public static int CompareTo(this Point pLeft, Point pRight)
        {
            if (pLeft.Y > pRight.Y)
                return 1;
            if (pLeft.Y < pRight.Y)
                return -1;
            if (pLeft.X > pRight.X)
                return 1;
            if (pLeft.X < pRight.X)
                return -1;

            return 0;
        }

        public static BitmapSource BitmapFromBase64(this string b64String)
        {
            var bytes = Convert.FromBase64String(b64String);
            using var stream = new MemoryStream(bytes);
            return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        }
    }
}
