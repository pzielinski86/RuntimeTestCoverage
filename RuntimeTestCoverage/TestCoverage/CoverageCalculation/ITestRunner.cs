using System.Reflection;
using Microsoft.CodeAnalysis;
using TestCoverage.Compilation;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    internal interface ITestRunner
    {
        LineCoverage[] RunAllTestsInDocument(RewrittenDocument rewrittenDocument, ISemanticModel semanticModel,
            MetadataReference[] allReferences, Assembly[] allAssemblies,string projectName);
    }
}