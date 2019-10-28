namespace TextViewer
{
    public class ImageWord : WordInfo
    {
        public override double Height => Styles.Height * ImageScale;
        public override double Width => Styles.Width * ImageScale + ExtraWidth;

        public ImageWord(int offset, WordStyle style = null)
            : base("img", offset, WordType.Image, false, style)
        {
        }
    }
}
