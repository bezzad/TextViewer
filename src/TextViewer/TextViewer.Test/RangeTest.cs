using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace TextViewer.Test
{
    [TestClass]
    public class RangeTest
    {
        [TestMethod]
        public void BiggerThanOperatorTest()
        {
            var leftRange = new Range(5, 7);

            Assert.IsTrue(leftRange > 1);
            Assert.IsTrue(leftRange > 4);
            Assert.IsTrue(leftRange > 0);

            Assert.IsFalse(leftRange > 5);
            Assert.IsFalse(leftRange > 6);
            Assert.IsFalse(leftRange > 7);

            Assert.IsFalse(leftRange > 8);
            Assert.IsFalse(leftRange > 10);
        }

        [TestMethod]
        public void BiggerThanEqualOperatorTest()
        {
            var leftRange = new Range(5, 7);

            Assert.IsTrue(leftRange >= 1);
            Assert.IsTrue(leftRange >= 4);
            Assert.IsTrue(leftRange >= 0);

            Assert.IsTrue(leftRange >= 5);
            Assert.IsTrue(leftRange >= 6);
            Assert.IsTrue(leftRange >= 7);

            Assert.IsFalse(leftRange >= 8);
            Assert.IsFalse(leftRange >= 10);
        }

        [TestMethod]
        public void LowerThanOperatorTest()
        {
            var leftRange = new Range(5, 7);

            Assert.IsFalse(leftRange < 1);
            Assert.IsFalse(leftRange < 4);
            Assert.IsFalse(leftRange < 0);

            Assert.IsFalse(leftRange < 5);
            Assert.IsFalse(leftRange < 6);
            Assert.IsFalse(leftRange < 7);

            Assert.IsTrue(leftRange < 8);
            Assert.IsTrue(leftRange < 10);
        }

        [TestMethod]
        public void LowerThanOrEqualOperatorTest()
        {
            var leftRange = new Range(5, 7);

            Assert.IsFalse(leftRange <= 1);
            Assert.IsFalse(leftRange <= 4);
            Assert.IsFalse(leftRange <= 0);

            Assert.IsTrue(leftRange <= 5);
            Assert.IsTrue(leftRange <= 6);
            Assert.IsTrue(leftRange <= 7);

            Assert.IsTrue(leftRange <= 8);
            Assert.IsTrue(leftRange <= 10);
        }

        [TestMethod]
        public void EqualOperatorTest()
        {
            var leftRange = new Range(5, 7);

            Assert.IsFalse(leftRange == 1);
            Assert.IsFalse(leftRange == 4);
            Assert.IsFalse(leftRange == 0);

            Assert.IsTrue(leftRange == 5);
            Assert.IsTrue(leftRange == 6);
            Assert.IsTrue(leftRange == 7);

            Assert.IsFalse(leftRange == 8);
            Assert.IsFalse(leftRange == 10);
        }

        [TestMethod]
        public void NotEqualOperatorTest()
        {
            var leftRange = new Range(5, 7);

            Assert.IsTrue(leftRange != 1);
            Assert.IsTrue(leftRange != 4);
            Assert.IsTrue(leftRange != 0);

            Assert.IsFalse(leftRange != 5);
            Assert.IsFalse(leftRange != 6);
            Assert.IsFalse(leftRange != 7);

            Assert.IsTrue(leftRange != 8);
            Assert.IsTrue(leftRange != 10);
        }

        [TestMethod]
        public void EqualsTest()
        {
            var leftRange = new Range(5, 8);

            Assert.IsTrue(leftRange.Equals(new Range(5, 8)));
            Assert.IsFalse(leftRange.Equals(new Range(4, 6)));
            Assert.IsFalse(leftRange.Equals(new Range(3, 7)));
            Assert.IsFalse(leftRange.Equals(new Range(3, 9)));
            Assert.IsFalse(leftRange.Equals(new Range(3, 8)));
            Assert.IsFalse(leftRange.Equals(new Range(5, 9)));
            Assert.IsFalse(leftRange.Equals(new Range(9, 10)));
            Assert.IsFalse(leftRange.Equals(new Range(3, 4)));
            Assert.IsFalse(leftRange.Equals(new Range(1, 10)));

            Assert.IsTrue(leftRange.Equals(5));
            Assert.IsTrue(leftRange.Equals(6));
            Assert.IsTrue(leftRange.Equals(7));
            Assert.IsTrue(leftRange.Equals(8));
            Assert.IsFalse(leftRange.Equals(9));
            Assert.IsFalse(leftRange.Equals(10));
            Assert.IsFalse(leftRange.Equals(4));
            Assert.IsFalse(leftRange.Equals(0));
        }

        [TestMethod]
        public void CompareToTest()
        {
            var leftRange = new Range(5, 8);

            Assert.IsTrue(leftRange.CompareTo(new Range(5, 8)) == 0);
            Assert.IsTrue(leftRange.CompareTo(new Range(4, 6)) == 1);
            Assert.IsTrue(leftRange.CompareTo(new Range(3, 7)) == 1);
            Assert.IsTrue(leftRange.CompareTo(new Range(3, 9)) == -1);
            Assert.IsTrue(leftRange.CompareTo(new Range(3, 8)) == 0);
            Assert.IsTrue(leftRange.CompareTo(new Range(5, 9)) == -1);
            Assert.IsTrue(leftRange.CompareTo(new Range(9, 10)) == -1);
            Assert.IsTrue(leftRange.CompareTo(new Range(3, 4)) == 1);
            Assert.IsTrue(leftRange.CompareTo(new Range(4, 4)) == 1);
            Assert.IsTrue(leftRange.CompareTo(new Range(5, 5)) == 0);
            Assert.IsTrue(leftRange.CompareTo(new Range(6, 6)) == 0);
            Assert.IsTrue(leftRange.CompareTo(new Range(7, 7)) == 0);
            Assert.IsTrue(leftRange.CompareTo(new Range(8, 8)) == 0);
            Assert.IsTrue(leftRange.CompareTo(new Range(9, 9)) == -1);

            Assert.IsTrue(leftRange.CompareTo(5) == 0);
            Assert.IsTrue(leftRange.CompareTo(6) == 0);
            Assert.IsTrue(leftRange.CompareTo(7) == 0);
            Assert.IsTrue(leftRange.CompareTo(8) == 0);
            Assert.IsTrue(leftRange.CompareTo(9) == -1);
            Assert.IsTrue(leftRange.CompareTo(10) == -1);
            Assert.IsTrue(leftRange.CompareTo(4) == 1);
            Assert.IsTrue(leftRange.CompareTo(0) == 1);
        }

        [TestMethod]
        public void RangeKeyBinarySearchTest()
        {
            var dicRangePerIndex = new List<Range>
            {
                new Range(0, 193),   // 0
                new Range(194, 280), // 1
                new Range(281, 360), // 2
                new Range(361, 431), // 3
                new Range(431, 500), // 4
                new Range(501, 600), // 5
                new Range(601, 680)  // 6
            };

            Assert.AreEqual(0, dicRangePerIndex.BinarySearch(Range.Create(0)));
            Assert.AreEqual(0, dicRangePerIndex.BinarySearch(Range.Create(193)));
            Assert.AreEqual(0, dicRangePerIndex.BinarySearch(Range.Create(192)));
            Assert.AreNotEqual(0, dicRangePerIndex.BinarySearch(Range.Create(194)));
            Assert.AreEqual(3, dicRangePerIndex.BinarySearch(Range.Create(400)));
            Assert.AreEqual(6, dicRangePerIndex.BinarySearch(Range.Create(680)));
            Assert.IsTrue(dicRangePerIndex.BinarySearch(Range.Create(-1)) < 0);
            Assert.IsTrue(dicRangePerIndex.BinarySearch(Range.Create(681)) < 0);
        }
    }
}
