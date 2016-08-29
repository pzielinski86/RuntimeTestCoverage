using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestCoverage.Compilation;

namespace TestCoverage.CoverageCalculation
{
    public interface ITestsExtractor
    {
        bool ContainsTests(SyntaxNode node);
        ClassDeclarationSyntax[] GetTestClasses(SyntaxNode testClass);
        TestFixtureDetails GetTestFixtureDetails(ClassDeclarationSyntax fixtureNode, ISemanticModel semanticModel);
        bool IsAttributeTestFixture(AttributeSyntax node);
    }
}