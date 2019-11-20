using System.Collections.Generic;

namespace TextViewerSample.Reader
{
    public interface ITagNode
    {
        string Value { get; set; }
        int Type { get; set; }
        string Name { get; set; }
        string ChapterName { get; set; }
        long Offset { get; set; }
        Dictionary<string, string> Styles { get; set; }
        Dictionary<string, string> Attributes { get; set; }
    }
}
