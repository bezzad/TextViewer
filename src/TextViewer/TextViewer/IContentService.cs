using System.Collections.Generic;

namespace TextViewer
{
    public interface IContentService
    {
        Paragraph GetParagraph(int id);
        List<string> GetChapters();
        int ChapterCount();
        Paragraph GetChapterFirstParagraph();
    }
}
