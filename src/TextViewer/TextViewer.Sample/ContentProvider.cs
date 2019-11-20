using System;
using System.Collections.Generic;
using System.IO;
using TextViewer;
using TextViewerSample.Reader;

namespace TextViewerSample
{
    public class ContentProvider : IContentService
    {
        public static List<List<Paragraph>> Chapters { get; set; }

        public ContentProvider(string path, bool isRtl, string decryptionKey = null)
        {
            Chapters = new List<List<Paragraph>>() { Path.Combine(path).GetParagraphs(true) };
        }

        public IAtom GetParagraph(int chapterIndex, int id)
        {
            throw new NotImplementedException();
        }

        public IAtom GetParagraph(Position pos)
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

        public IAtom GetChapterFirstParagraph(int chapterIndex)
        {
            throw new NotImplementedException();
        }

        public IAtom GetChapterLastParagraph(int chapterIndex)
        {
            throw new NotImplementedException();
        }
    }
}
