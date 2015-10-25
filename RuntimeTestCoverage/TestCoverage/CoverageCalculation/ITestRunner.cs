using System.Reflection;
using Microsoft.CodeAnalysis;
using TestCoverage.Compilation;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    internal interface ITestRunner
    {

        LineCoverage[] RunAllTestsInDocument(RewrittenDocument rewrittenDocument, ISemanticModel sematnModel,Project project, Assembly[] allAssemblies);
    }
}