using System;
using System.Collections.Generic;

namespace TextViewer
{
    public class Position : IComparable<Position>, IComparer<Position>, IEquatable<Position>, IEqualityComparer<Position>, ICloneable
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
            return x?.CompareTo(y) ?? -1;
        }

        #endregion

        #region Implement IEquatable<Position>

        public bool Equals(Position other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ChapterIndex == other.ChapterIndex && ParagraphId == other.ParagraphId && Offset == other.Offset;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Position)obj);
        }

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

        public object Clone()
        {
            return new Position(ChapterIndex, ParagraphId, Offset);
        }

        #endregion

        #region Implement IEqualityComparer<Position>

        public bool Equals(Position x, Position y)
        {
            if (ReferenceEquals(null, y)) return false;
            if (ReferenceEquals(x, y)) return true;
            return x.ChapterIndex == y.ChapterIndex && x.ParagraphId == y.ParagraphId && x.Offset == y.Offset;
        }

        public int GetHashCode(Position obj)
        {
            unchecked
            {
                var hashCode = obj.ChapterIndex;
                hashCode = (hashCode * 397) ^ obj.ParagraphId.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.Offset;
                return hashCode;
            }
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
            return left?.Equals(right) == true;
        }
        public static bool operator !=(Position left, Position right)
        {
            return !left?.Equals(right) == true;
        }
    }
}
