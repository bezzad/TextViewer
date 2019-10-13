using System.Collections.Generic;

namespace TextViewer
{
    public class Paragraph
    {
        public Paragraph(int offset, List<WordInfo> content, bool isRtl)
        {
            Offset = offset;
            Words = content;
            IsRtl = isRtl;

            Lines = new List<Line>();
            Styles = new Dictionary<StyleType, string>();
        }

        public int Offset { get; set; }
        public List<WordInfo> Words { get; set; }
        public List<Line> Lines { get; set; }
        public Dictionary<StyleType, string> Styles { get; set; }
        public bool IsRtl { get; set; }
    }
}
