using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using TestCoverage.Compilation;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    internal interface ITestRunner
    {
        LineCoverage[] RunAllTestsInDocument(RewrittenDocument rewrittenDocument,
            ISemanticModel semanticModel,
            Project project,
            _Assembly[] allAssemblies);
    }
}