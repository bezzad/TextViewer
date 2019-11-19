using System.Collections.Generic;
using TextViewer;

namespace TextViewerSample
{
    public interface IContentService
    {
        Paragraph GetParagraph(int chapterIndex, int id);
        Paragraph GetParagraph(Position pos);
        List<string> GetChapters();
        int ChapterCount();
        Paragraph GetChapterFirstParagraph(int chapterIndex);
        Paragraph GetChapterLastParagraph(int chapterIndex);
    }
}
