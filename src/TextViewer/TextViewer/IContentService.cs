using System.Collections.Generic;

namespace TextViewer
{
    public interface IContentService
    {
        Paragraph GetParagraph(int id);
        List<string> GetChapters();
        Paragraph GetChapterFirstParagraph();
    }
}
