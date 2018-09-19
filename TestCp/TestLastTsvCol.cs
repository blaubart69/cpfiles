using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Spi;

namespace TestCp
{
    [TestClass]
    public class TestLastTsvCol
    {
        [TestMethod]
        public void EmptyLine()
        {
            Assert.AreEqual(String.Empty, Misc.GetLastTsvColumn(String.Empty));  
        }
        [TestMethod]
        public void OnlyOneValue()
        {
            Assert.AreEqual("bumsti", Misc.GetLastTsvColumn("bumsti"));
        }
        [TestMethod]
        public void EmptyElemtOnEnd()
        {
            Assert.AreEqual(String.Empty, Misc.GetLastTsvColumn("bumsti\t"));
        }
        [TestMethod]
        public void TwoEmptyElements()
        {
            Assert.AreEqual(String.Empty, Misc.GetLastTsvColumn("\t"));
        }
        [TestMethod]
        public void TwoValues()
        {
            Assert.AreEqual("b", Misc.GetLastTsvColumn("a\tb"));
        }
    }
}
