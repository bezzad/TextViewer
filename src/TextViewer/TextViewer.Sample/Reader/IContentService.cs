using System.Collections.Generic;

namespace TextViewerSample.Reader
{
    public interface IContentService
    {
        IAtom GetParagraph(int chapterIndex, int id);
        IAtom GetParagraph(Position pos);
        List<string> GetChapters();
        int ChapterCount();
        IAtom GetChapterFirstParagraph(int chapterIndex);
        IAtom GetChapterLastParagraph(int chapterIndex);
    }
}
