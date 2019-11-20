using System;
using System.Collections.Generic;

namespace TextViewerSample.Reader
{
    // based on from xml_node_type in pugixml.hpp
    public enum TagNodeType
    {
        None = 0,
        Document = 1,
        Element = 2,
        PcData = 3,
        Cdata = 4,
        Comment = 5,
        ProcInst = 6,
        Declaration = 7,
        DocType = 8
    }

    public class TagNode : ITagNode
    {
        public TagNode(string chapterName, string value)
        {
            Value = value;
            ChapterName = chapterName;
            Styles = new Dictionary<string, string>();
        }
        public TagNode(string chapterName, long offset, int? start = null, int? end = null)
        {
            ChapterName = chapterName;
            Offset = offset;
            Styles = new Dictionary<string, string>();
            Name = "";

            //Type = Epub.Instance.GetType(chapterName, (uint)offset);
            //if (Type == 0) return;

            //Value = start == null || end == null
            //    ? Encoding.UTF8.GetString(WesternEncoding.GetBytes(Epub.Instance.GetValue(chapterName, (uint) offset)))
            //    : Encoding.UTF8.GetString(WesternEncoding.GetBytes(Epub.Instance.GetValue(chapterName, (uint) offset, start.Value, end.Value)));
            //Value = Value?.ReplaceAll(StringHelper.ParagraphSplitters, ' ')
            //    .ReplaceAll(StringHelper.WrongHalfSpaces, StringHelper.HalfSpace);
        }

        public string Value { get; set; }
        public int Type { get; set; }
        public TagNodeType NodeType => (TagNodeType)Type;
        public string Name { get; set; }
        public string ChapterName { get; set; }
        public long Offset { get; set; }
        public Dictionary<string, string> Styles { get; set; }
        public Dictionary<string, string> Attributes { get; set; }


        public int GetTextLength()
        {
            return Value?.Length ?? 0;
        }
        public bool IsNull()
        {
            return NodeType == TagNodeType.None;
        }
        public bool IsImage()
        {
            return "img".Equals(Name) || "image".Equals(Name);
        }
        public byte[] GetImageBytes()
        {
            return null;
        }
        public bool IsValidImage()
        {
            throw new NotImplementedException();
        }
        public bool IsBreakNode()
        {
            return Value?.Equals("\n") == true;
        }
        public bool IsLink()
        {
            throw new NotImplementedException();
        }
        public bool IsContentNode()
        {
            return NodeType == TagNodeType.PcData;
        }

        public string GetAttribute(string key)
        {
            throw new NotImplementedException();
        }
    }
}
