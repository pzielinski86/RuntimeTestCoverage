using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestCoverage.Compilation;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    public interface ITestRunner
    {
        LineCoverage[] RunTest(Project project,
            RewrittenDocument rewrittenDocument,
            MethodDeclarationSyntax method,
            ISemanticModel semanticModel,
            string[] rewrittenAssemblies);

        LineCoverage[] RunAllTestsInDocument(RewrittenDocument rewrittenDocument, ISemanticModel semanticModel, Project project, string[] rewrittenAssemblies);
    }
}