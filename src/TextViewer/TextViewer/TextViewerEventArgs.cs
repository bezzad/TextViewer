using System;

namespace TextViewer
{
    public class TextViewerEventArgs : EventArgs
    {
        public string Message { get; set; }
        public MessageType MessageType { get; set; }


        public TextViewerEventArgs(string message, MessageType messageType)
        {
            Message = message;
            MessageType = messageType;
        }
    }
}
