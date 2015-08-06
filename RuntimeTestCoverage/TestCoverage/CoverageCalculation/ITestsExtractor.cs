using Microsoft.CodeAnalysis;

namespace TestCoverage.CoverageCalculation
{
    public interface ITestsExtractor
    {
        SyntaxNode[] GetTestClasses(SyntaxNode testClass);
        SyntaxNode[] GetTestMethods(SyntaxNode root);
    }
}