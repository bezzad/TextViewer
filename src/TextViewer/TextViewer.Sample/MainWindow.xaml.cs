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
            var magnifierTypes = Enum.GetNames(typeof(MagnifierType)).ToList();
            CmbFontFamily.ItemsSource = fonts;
            CmbFontFamily.SelectedIndex = fonts.FindIndex(f => f.Source == "Arial");
            CmbFontSize.ItemsSource = sizes;
            CmbFontSize.SelectedIndex = sizes.IndexOf(18);
            CmbLineHeight.ItemsSource = sizes;
            CmbLineHeight.SelectedIndex = sizes.IndexOf(22);
            CmbMagnifier.ItemsSource = magnifierTypes;
            CmbMagnifier.SelectedIndex = magnifierTypes.IndexOf(Reader.MagnifierType.ToString());
            BtnLoadSample.Checked += delegate { BtnLoadSampleChecking(); };
            BtnLoadSample.Unchecked += delegate { BtnLoadSampleChecking(); };

            BtnLoadSampleChecking();

            DpiChanged += delegate
            {
                Reader.PixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
                Reader.Render();
            };
        }

        private void BtnLoadSampleChecking()
        {
            if (BtnLoadSample.IsChecked == true)
            {
                Reader.PageContent = Path.Combine(Environment.CurrentDirectory, "Data\\LtrContentSample.txt").GetWords(false);
                BtnLoadSample.Content = "LtrContentSample";
            }
            else
            {
                Reader.PageContent = Path.Combine(Environment.CurrentDirectory, "Data\\RtlContentSample.txt").GetWords(true);
                BtnLoadSample.Content = "RtrContentSample";
            }

            Reader.Render();
        }
    }
}
