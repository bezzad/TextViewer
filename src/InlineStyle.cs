namespace SvgTextViewer
{
    public class InlineStyle
    {
        public StyleType Type { get; set; }
        public string Value { get; set; }

        public InlineStyle() { }

        public InlineStyle(StyleType type, string value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Type}: {Value}; ";
        }
    }
}