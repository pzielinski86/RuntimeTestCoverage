using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCoverage.Extensions
{
    public static class SyntaxNodeExtensions
    {
        public static ClassDeclarationSyntax GetClassDeclarationSyntax(this SyntaxNode node)
        {
            return node.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
        }
    }
}