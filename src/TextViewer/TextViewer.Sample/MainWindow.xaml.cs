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
        public Model MainModel { get; set; }
        public static ReaderService ReaderService { get; set; }



        public MainWindow()
        {
            InitializeComponent();

            DataContext = MainModel = new Model();
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

            ReaderService = new ReaderService
            {
                PixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip,
                ContentProvider = new ContentProvider(Path.Combine(Environment.CurrentDirectory, "Data\\LtrSample.html"), false)
            };

            DpiChanged += delegate { ReaderService.PixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip; };
            Loaded += delegate { BtnLoadSampleChecking(); };
        }

        private void BtnLoadSampleChecking()
        {
            if (BtnLoadSample.IsChecked == true)
            {
                MainModel.CurrentPage = new Page();
                var paragraphs = Path.Combine(Environment.CurrentDirectory, "Data\\LtrSample.html").GetParagraphs(false);
                foreach (var para in paragraphs)
                    MainModel.CurrentPage.AddBlock(para);

                BtnLoadSample.Content = "LtrContentSample";
            }
            else
            {
                MainModel.CurrentPage = new Page();
                var paragraphs = Path.Combine(Environment.CurrentDirectory, "Data\\RtlSample.html").GetParagraphs(true);
                foreach (var para in paragraphs)
                    MainModel.CurrentPage.AddBlock(para);

                BtnLoadSample.Content = "RtrContentSample";
            }

            Reader.ReRender();
        }

        private void ScrollUp(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ScrollDown(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ScrollRight(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ScrollLeft(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
