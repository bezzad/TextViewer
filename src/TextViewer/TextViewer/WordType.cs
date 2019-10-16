using System;

namespace TextViewer
{
    [Flags]
    public enum WordType
    {
        Normal = 1,
        InertChar = 2,
        Image = 4,
        Space = 8,
        Attached = 16
    }
}
