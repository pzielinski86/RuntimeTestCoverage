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
            const string expectedRewrittenPath = "c:\test.dll.coverage";

            string rewrittenPath=PathHelper.GetRewrittenFilePath(path);
            
            Assert.That(rewrittenPath, Is.EqualTo(expectedRewrittenPath));
        }

        [Test]
        public void GetCoverageDllName_ShouldReturn_CoverageDll_When_ProvidedDllIsNotCoverageDll()
        {
            const string expectedDllName = "Logic_COVERAGE.dll";
            string coverageDllName = PathHelper.GetCoverageDllName("Logic");
            
            Assert.That(coverageDllName, Is.EqualTo(expectedDllName));
        }

        [Test]
        public void GetCoverageDllName_ShouldReturn_ProvidedDllNameWithoutModifications_When_ProvidedDllIsAlreadyCoverageDll()
        {
            const string expectedDllName = "Logic_COVERAGE.dll";
            string coverageDllName = PathHelper.GetCoverageDllName("Logic_COVERAGE.dll");

            Assert.That(coverageDllName, Is.EqualTo(expectedDllName));
        }
    }
}
