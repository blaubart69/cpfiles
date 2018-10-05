using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using cp;

namespace TestCp
{
    [TestClass]
    public class ReadInput
    {
        [TestMethod]
        public void OneLineWithoutSize()
        {
            var items = CopyFiles.ReadInputfile(new string[] { "bumsti" });
            var i = items.First();
            Assert.AreEqual("bumsti", i.relativeFilename);
            Assert.IsNull(i.filesize);
        }
        [TestMethod]
        public void OneLineWithSize()
        {
            var items = CopyFiles.ReadInputfile(new string[] { "100\tbumsti" });
            var i = items.First();
            Assert.AreEqual("bumsti", i.relativeFilename);
            Assert.AreEqual((ulong)100, i.filesize.Value);
        }
        [TestMethod]
        public void OneLineWithBadSize()
        {
            var items = CopyFiles.ReadInputfile(new string[] { "kacsi\tbumsti" });
            var i = items.First();
            Assert.AreEqual("bumsti", i.relativeFilename);
            Assert.IsNull(i.filesize);
        }
        [TestMethod]
        public void OneLineWithSizeandMoreColumns()
        {
            var items = CopyFiles.ReadInputfile(new string[] { "doesnotmatter\t99\tbumsti" });
            var i = items.First();
            Assert.AreEqual("bumsti", i.relativeFilename);
            Assert.AreEqual((ulong)99, i.filesize.Value);
        }
    }
}
