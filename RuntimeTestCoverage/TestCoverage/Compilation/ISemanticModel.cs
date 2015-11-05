using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCoverage.Compilation
{
    public interface ISemanticModel
    {
        object GetConstantValue(SyntaxNode node);
        string GetAssemblyName();

        string GetFullName(ClassDeclarationSyntax classDeclarationSyntax);
    }
}