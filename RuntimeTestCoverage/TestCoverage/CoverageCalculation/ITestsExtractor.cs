using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCoverage.CoverageCalculation
{
    public interface ITestsExtractor
    {
        ClassDeclarationSyntax[] GetTestClasses(SyntaxNode testClass);
        TestCase[] GetTestCases(ClassDeclarationSyntax root);
    }
}