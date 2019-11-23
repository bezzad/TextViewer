using System.Collections.Generic;

namespace TextViewerSample.Reader
{
    public interface IAtom // native reader paragraph
    {
        uint Offset { get; set; }
        int NextAtomOffset { get; set; }
        int PrevAtomOffset { get; set; }
        uint ChapterIndex { get; set; }
        string ChapterName { get; set; }
        List<ITagNode> TagNodeList { get; set; }
        List<int> HighlightStartCharOffsets { get; set; }
        bool HasHighlight { get; set; }
        int StartCharOffset { get; set; }
        int EndCharOffset { get; set; }


        ITagNode GetNode(int index);
        bool IsImage();
        int GetNextAtomOffset();
        int GetPrevAtomOffset();
        int GetOffset();
        int GetLastHighlightOffset();
        int GetHighlightOffsetAfter(int offset);
        int GetHighlightOffsetBefore(int offset);
    }
}
