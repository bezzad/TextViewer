using System.Windows;
using TextViewer;

namespace TextViewerSample
{
    public class Model : DependencyObject
    {
        public static readonly DependencyProperty CurrentPageProperty = DependencyProperty.Register(nameof(CurrentPage), typeof(IPage), typeof(Model), new PropertyMetadata(default(IPage)));

        public IPage CurrentPage
        {
            get => (IPage)GetValue(CurrentPageProperty);
            set => SetValue(CurrentPageProperty, value);
        }

        public Model()
        {
            CurrentPage = new Page();
        }

    }
}
