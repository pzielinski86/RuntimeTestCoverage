using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCoverage.Extensions
{
    public static class SyntaxNodeExtensions
    {
        public static string[] GetUsedNamespaces(this SyntaxNode node)
        {
            return  node.Ancestors().OfType<CompilationUnitSyntax>().First().DescendantNodes().
              OfType<UsingDirectiveSyntax>().Select(x => x.Name.ToString()).ToArray();
        }
        public static MethodDeclarationSyntax[] GetPublicMethods(this SyntaxNode node)
        {
            return
                node.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(m => m.Modifiers.Any(y => y.ValueText == "public"))
                    .ToArray();
        }
        public static ClassDeclarationSyntax GetClassDeclarationSyntax(this SyntaxNode node)
        {
            return node.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
        }

        public static ClassDeclarationSyntax[] GetClassDeclarations(this SyntaxNode node)
        {
            return node.DescendantNodes().OfType<ClassDeclarationSyntax>().ToArray();
        }
    }
}