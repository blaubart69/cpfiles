using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Spi;

namespace TestCp
{
    [TestClass]
    public class TestMisc
    {
        [TestMethod]
        public void TestGetDirectoryName()
        {
            Assert.AreEqual(@"c:\temp", Misc.GetDirectoryName(@"c:\temp\dummy.txt"));
            Assert.AreEqual(@"\\?\c:\temp", Misc.GetDirectoryName(@"\\?\c:\temp\dummy.txt"));
            Assert.AreEqual(@"\\?\c:\temp", Misc.GetDirectoryName(@"\\?\c:\temp\dummy"));
        }
    }
}
