using System.Reflection;
using TestCoverage.Compilation;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    internal interface ITestRunner
    {
        LineCoverage[] RunAllTestsInFixture(CompiledTestFixtureInfo compiledTestFixtureInfo);

        LineCoverage[] RunAllTestsInDocument(RewrittenDocument rewrittenDocument, 
            CompiledItem compiledTestProject,
            Assembly[] allAssemblies);
    }
}