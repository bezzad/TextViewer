using System.Windows;
using System.Windows.Documents;
using TextViewer;

namespace TextViewerSample.Converters
{
    public class TextViewerBehavior : DependencyObject
    {
        public static readonly DependencyProperty PageContentProperty =
            DependencyProperty.RegisterAttached(nameof(TextViewer.TextViewer.PageContent), typeof(IPage),
                typeof(TextViewerBehavior), new UIPropertyMetadata(null, OnPageContentChanged));

        public static IPage GetPageContent(DependencyObject obj)
        {
            return (IPage)obj.GetValue(PageContentProperty);
        }

        public static void SetPageContent(DependencyObject obj, FlowDocument value)
        {
            obj.SetValue(PageContentProperty, value);
        }

        private static void OnPageContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextViewer.TextViewer pageViewer)
            {
                if (e.NewValue is IPage page)
                    pageViewer.PageContent = page;
            }
        }
    }
}
