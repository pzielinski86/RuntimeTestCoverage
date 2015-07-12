using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCoverage.Tests
{
    [TestFixture]
    public class PathHelperTests
    {
        [Test]
        public void GetRewrittenFilePath()
        {
            const string path = "c:\test.dll";
            const string expectedRewrittenPath = "c:\test.dll_testcoverage";

            string rewrittenPath=PathHelper.GetRewrittenFilePath(path);

            Assert.That(rewrittenPath, Is.EqualTo(expectedRewrittenPath));
        }
    }
}
