using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TextViewer
{
    public static class GraphicsHelper
    {
        public static BitmapSource BitmapFromBase64(this string b64String)
        {
            var bytes = Convert.FromBase64String(b64String);
            using var stream = new MemoryStream(bytes);
            return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        }


        
    }
}
