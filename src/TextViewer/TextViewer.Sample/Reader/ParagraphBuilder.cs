using TextViewer;

namespace TextViewerSample.Reader
{
    public static class ParagraphBuilder
    {
        public static Paragraph ConvertToParagraph(this IAtom atom)
        {
            var para = new Paragraph((int) atom.Offset);

            foreach (var tag in atom.TagNodeList)
            {
                var styles = tag.Styles;
            }

            return para;
        }
    }
}
