using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCoverage.CoverageCalculation
{
    public interface ITestsExtractor
    {
        ClassDeclarationSyntax[] GetTestClasses(SyntaxNode testClass);
        TestFixtureDetails GetTestFixtureDetails(ClassDeclarationSyntax fixtureNode);
    }
}