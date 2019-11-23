using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TextViewerSample.Reader
{
    public struct Position : IComparable<Position>, IComparer<Position>, IEquatable<Position>, IEqualityComparer<Position>
    {
        public int ChapterIndex { get; set; }
        public int ParagraphId { get; set; }
        public int Offset { get; set; }

        public Position(int chapterIndex, int paragraphId, int startOffset)
        {
            ChapterIndex = chapterIndex;
            ParagraphId = paragraphId;
            Offset = startOffset;
        }

        #region Implement IComparable<Position>

        public int CompareTo(int chapterIndex, long paragraphId, int offSet)
        {
            if (ChapterIndex > chapterIndex) return 1;
            if (ChapterIndex < chapterIndex) return -1;
            if (ParagraphId > paragraphId) return 1;
            if (ParagraphId < paragraphId) return -1;
            if (Offset > offSet) return 1;
            if (Offset < offSet) return -1;
            return 0;
        }

        public int CompareTo(Position position)
        {
            return CompareTo(position.ChapterIndex, position.ParagraphId, position.Offset);
        }

        #endregion

        #region Implement IComparer<Position>

        public int Compare(Position x, Position y)
        {
            return x.CompareTo(y);
        }

        #endregion

        #region Implement IEquatable<Position>

        public bool Equals(Position other)
        {
            return ChapterIndex == other.ChapterIndex && ParagraphId == other.ParagraphId && Offset == other.Offset;
        }

        public override bool Equals(object obj)
        {
            if (obj?.GetType() != GetType()) return false;
            return Equals((Position)obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ChapterIndex;
                hashCode = (hashCode * 397) ^ ParagraphId.GetHashCode();
                hashCode = (hashCode * 397) ^ Offset;
                return hashCode;
            }
        }

        #endregion

        #region Implement IEqualityComparer<Position>

        public bool Equals(Position x, Position y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(Position obj)
        {
            return obj.GetHashCode();
        }

        #endregion

        public static bool operator <(Position left, Position right)
        {
            return left.CompareTo(right) < 0;
        }
        public static bool operator >(Position left, Position right)
        {
            return left.CompareTo(right) > 0;
        }
        public static bool operator <=(Position left, Position right)
        {
            return left.CompareTo(right) <= 0;
        }
        public static bool operator >=(Position left, Position right)
        {
            return left.CompareTo(right) >= 0;
        }
        public static bool operator ==(Position left, Position right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Position left, Position right)
        {
            return !left.Equals(right);
        }
    }
}
