using NUnit.Framework;

namespace TestCoverage.Tests
{
    [TestFixture]
    public class PathHelperTests
    {  

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
