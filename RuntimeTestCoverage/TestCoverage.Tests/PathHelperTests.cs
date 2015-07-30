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

        [Test]
        public void GetCoverageDllName()
        {
            const string expectedDllName = "Logic_(.+).dll";
            string coverageDllName = PathHelper.GetCoverageDllName("Logic");
            
            Assert.That(coverageDllName, Is.StringMatching(expectedDllName));
        }

        [Test]
        public void GetDllNameFromCoverageDll_Should_ReturnOriginalName_When_ProvidedNameIsNotCoverageDll()
        {
            string originalDll = PathHelper.GetDllNameFromCoverageDll("Coverage");

            Assert.That(originalDll, Is.EqualTo("Coverage"));
        }

        [Test]
        public void GetDllNameFromCoverageDll_Should_ReturnOriginalDll_When_ProvidedNameIsCoverageDll()
        {
            string originalDll = PathHelper.GetDllNameFromCoverageDll("Coverage_525252-343-4343-43-43.dll");

            Assert.That(originalDll, Is.EqualTo("Coverage"));
        }

        [Test]
        public void GetDllNameFromCoverageDll_Should_ReturnOriginalDll_When_ProvidedNameIsCoverageDllWithAdditionalUnderscore()
        {
            string originalDll = PathHelper.GetDllNameFromCoverageDll("Covera_ge_25252-25325-253252-5252.dll");

            Assert.That(originalDll, Is.EqualTo("Covera_ge"));
        }
    }
}
