using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TextViewer.Test
{
    [TestClass]
    public class PositionTest
    {
        [TestMethod]
        public void BiggerThanOperatorTest()
        {
            var leftPos = new Position(5, 5, 5);

            Assert.IsTrue(leftPos > new Position(0, 0, 0));
            Assert.IsTrue(leftPos > new Position(5, 5, 0));
            Assert.IsTrue(leftPos > new Position(5, 0, 5));
            Assert.IsTrue(leftPos > new Position(0, 5, 5));

            Assert.IsFalse(leftPos > new Position(5, 5, 5));

            Assert.IsFalse(leftPos > new Position(5, 5, 6));
            Assert.IsFalse(leftPos > new Position(5, 6, 0));
            Assert.IsFalse(leftPos > new Position(6, 0, 0));
        }

        [TestMethod]
        public void BiggerThanEqualOperatorTest()
        {
            var leftPos = new Position(5, 5, 5);

            Assert.IsTrue(leftPos >= new Position(0, 0, 0));
            Assert.IsTrue(leftPos >= new Position(5, 5, 0));
            Assert.IsTrue(leftPos >= new Position(5, 0, 5));
            Assert.IsTrue(leftPos >= new Position(0, 5, 5));

            Assert.IsTrue(leftPos >= new Position(5, 5, 5));

            Assert.IsFalse(leftPos >= new Position(5, 5, 6));
            Assert.IsFalse(leftPos >= new Position(5, 6, 0));
            Assert.IsFalse(leftPos >= new Position(6, 0, 0));
        }

        [TestMethod]
        public void LowerThanOperatorTest()
        {
            var leftPos = new Position(5, 5, 5);

            Assert.IsFalse(leftPos < new Position(0, 0, 0));
            Assert.IsFalse(leftPos < new Position(5, 5, 0));
            Assert.IsFalse(leftPos < new Position(5, 0, 5));
            Assert.IsFalse(leftPos < new Position(0, 5, 5));

            Assert.IsFalse(leftPos < new Position(5, 5, 5));

            Assert.IsTrue(leftPos < new Position(5, 5, 6));
            Assert.IsTrue(leftPos < new Position(5, 6, 0));
            Assert.IsTrue(leftPos < new Position(6, 0, 0));
        }

        [TestMethod]
        public void LowerThanOrEqualOperatorTest()
        {
            var leftPos = new Position(5, 5, 5);

            Assert.IsFalse(leftPos <= new Position(0, 0, 0));
            Assert.IsFalse(leftPos <= new Position(5, 5, 0));
            Assert.IsFalse(leftPos <= new Position(5, 0, 5));
            Assert.IsFalse(leftPos <= new Position(0, 5, 5));

            Assert.IsTrue(leftPos <= new Position(5, 5, 5));

            Assert.IsTrue(leftPos <= new Position(5, 5, 6));
            Assert.IsTrue(leftPos <= new Position(5, 6, 0));
            Assert.IsTrue(leftPos <= new Position(6, 0, 0));
        }

        [TestMethod]
        public void EqualOperatorTest()
        {
            var leftPos = new Position(5, 5, 5);

            Assert.IsFalse(leftPos == new Position(0, 0, 0));
            Assert.IsFalse(leftPos == new Position(5, 5, 0));
            Assert.IsFalse(leftPos == new Position(5, 0, 5));
            Assert.IsFalse(leftPos == new Position(0, 5, 5));

            Assert.IsTrue(leftPos == new Position(5, 5, 5));

            Assert.IsFalse(leftPos == new Position(5, 5, 6));
            Assert.IsFalse(leftPos == new Position(5, 6, 0));
            Assert.IsFalse(leftPos == new Position(6, 0, 0));
        }

        [TestMethod]
        public void NotEqualOperatorTest()
        {
            var leftPos = new Position(5, 5, 5);

            Assert.IsTrue(leftPos != new Position(0, 0, 0));
            Assert.IsTrue(leftPos != new Position(5, 5, 0));
            Assert.IsTrue(leftPos != new Position(5, 0, 5));
            Assert.IsTrue(leftPos != new Position(0, 5, 5));

            Assert.IsFalse(leftPos != new Position(5, 5, 5));

            Assert.IsTrue(leftPos != new Position(5, 5, 6));
            Assert.IsTrue(leftPos != new Position(5, 6, 0));
            Assert.IsTrue(leftPos != new Position(6, 5, 0));
            Assert.IsTrue(leftPos != new Position(6, 0, 0));
        }

        [TestMethod]
        public void CompareTest()
        {
            var leftPos = new Position(5, 5, 5);

            Assert.AreEqual(leftPos.Compare(leftPos, new Position(0, 0, 0)), 1);
            Assert.AreEqual(leftPos.Compare(leftPos, new Position(5, 5, 0)), 1);
            Assert.AreEqual(leftPos.Compare(leftPos, new Position(5, 0, 5)), 1);
            Assert.AreEqual(leftPos.Compare(leftPos, new Position(0, 5, 5)), 1);
            Assert.AreEqual(leftPos.Compare(leftPos, new Position(5, 5, 5)), 0);
            Assert.AreEqual(leftPos.Compare(leftPos, new Position(5, 5, 6)), -1);
            Assert.AreEqual(leftPos.Compare(leftPos, new Position(5, 6, 0)), -1);
            Assert.AreEqual(leftPos.Compare(leftPos, new Position(6, 0, 0)), -1);
        }
    }
}
