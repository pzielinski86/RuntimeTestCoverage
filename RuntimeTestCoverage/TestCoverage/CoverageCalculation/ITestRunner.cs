using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestCoverage.Compilation;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    internal interface ITestRunner
    {
        LineCoverage[] RunTest(Project project,
            RewrittenDocument rewrittenDocument,
            MethodDeclarationSyntax method,
            ISemanticModel semanticModel,
            string[] rewrittenAssemblies);

        LineCoverage[] RunAllTestsInDocument(RewrittenDocument rewrittenDocument, ISemanticModel semanticModel, Project project, string[] rewrittenAssemblies);
    }
}