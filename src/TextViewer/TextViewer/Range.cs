using System;
using System.Collections.Generic;

namespace TextViewer
{
    public struct Range : IEquatable<int>, IEquatable<Range>, IEqualityComparer<Range>, IEqualityComparer<int>, IComparable<int>, IComparable<Range>, ICloneable
    {
        public int Start { get; }
        public int End { get; }

        public Range(int start, int end)
        {
            Start = start;
            End = end;
        }

        public bool IsOneNumber()
        {
            return Start == End;
        }
        public bool Equals(int other)
        {
            return Start <= other && other <= End;
        }
        public bool Equals(Range other)
        {
            if (other.IsOneNumber())
                return Equals(other.Start);

            return Start == other.Start && End == other.End;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;

            if (obj is Range range)
                return Equals(range);

            if (obj is int num)
                return Equals(num);

            return false;
        }
        public bool Equals(Range x, Range y)
        {
            return x.Equals(y);
        }
        public bool Equals(int x, int y)
        {
            return x.Equals(y);
        }

        public override int GetHashCode()
        {
            // Ref: https://stackoverflow.com/questions/263400
            unchecked // Overflow is fine, just wrap
            {
                var hash = 17;
                hash = hash * 23 + Start.GetHashCode();
                hash = hash * 31 + End.GetHashCode();
                return hash;
            }
        }

        public object Clone()
        {
            return Create(Start, End);
        }
        public int GetHashCode(Range obj)
        {
            return obj.GetHashCode();
        }
        public int GetHashCode(int obj)
        {
            return obj.GetHashCode();
        }

        public int CompareTo(int other)
        {
            if (Start > other) return 1;
            if (End < other) return -1;
            return 0;
        }

        public int CompareTo(Range other)
        {
            if (other.IsOneNumber())
                return CompareTo(other.Start);

            return End.CompareTo(other.End);
        }

        public override string ToString()
        {
            return $"{Start}-{End}";
        }

        public static bool operator <(Range left, int right)
        {
            return left.CompareTo(right) < 0;
        }
        public static bool operator >(Range left, int right)
        {
            return left.CompareTo(right) > 0;
        }
        public static bool operator <=(Range left, int right)
        {
            return left.CompareTo(right) <= 0;
        }
        public static bool operator >=(Range left, int right)
        {
            return left.CompareTo(right) >= 0;
        }
        public static bool operator ==(Range left, int right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Range left, int right)
        {
            return !left.Equals(right);
        }


        public static Range Create(int start, int end)
        {
            return new Range(start, end);
        }
        public static Range Create(int number)
        {
            return new Range(number, number);
        }
    }
}
