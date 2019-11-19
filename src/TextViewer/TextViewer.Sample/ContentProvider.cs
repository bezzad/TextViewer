using System;
using System.Collections.Generic;
using System.IO;
using TextViewer;

namespace TextViewerSample
{
    public class ContentProvider : IContentService
    {
        public static List<List<Paragraph>> Chapters { get; set; }

        public ContentProvider(string path, bool isRtl, string decryptionKey = null)
        {
            Chapters = new List<List<Paragraph>>() { Path.Combine(path).GetParagraphs(true) };
        }

        public Paragraph GetParagraph(int chapterIndex, int id)
        {
            throw new NotImplementedException();
        }

        public Paragraph GetParagraph(Position pos)
        {
            throw new NotImplementedException();
        }

        public List<string> GetChapters()
        {
            throw new NotImplementedException();
        }

        public int ChapterCount()
        {
            throw new NotImplementedException();
        }

        public Paragraph GetChapterFirstParagraph(int chapterIndex)
        {
            throw new NotImplementedException();
        }

        public Paragraph GetChapterLastParagraph(int chapterIndex)
        {
            throw new NotImplementedException();
        }
    }
}
