using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using TextViewer;

namespace TextViewerSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var fonts = Fonts.SystemFontFamilies.OrderBy(f => f.Source).ToList();
            var sizes = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72, 96 };
            CmbFontFamily.ItemsSource = fonts;
            CmbFontFamily.SelectedIndex = fonts.FindIndex(f => f.Source == "Arial");
            CmbFontSize.ItemsSource = sizes;
            CmbFontSize.SelectedIndex = sizes.IndexOf(18);
            CmbLineHeight.ItemsSource = sizes;
            CmbLineHeight.SelectedIndex = sizes.IndexOf(22);

            BtnRtlSampleChecked(this, null);

            DpiChanged += delegate
            {
                Reader.PixelsPerDip = GraphicsHelper.PixelsPerDip(this);
                Reader.Render();
            };
        }

        private void BtnLtrSampleChecked(object sender, RoutedEventArgs e)
        {
            Reader.PageContent = Path.Combine(Environment.CurrentDirectory, "Data\\LtrContentSample.txt").GetWords(false);
            BtnLtrSample.IsChecked = true;
            BtnRtlSample.IsChecked = false;
            Reader.Render();
        }

        private void BtnRtlSampleChecked(object sender, RoutedEventArgs e)
        {
            Reader.PageContent = Path.Combine(Environment.CurrentDirectory, "Data\\RtlContentSample.txt").GetWords(true);
            BtnRtlSample.IsChecked = true;
            BtnLtrSample.IsChecked = false;
            Reader.Render();
        }
    }
}
