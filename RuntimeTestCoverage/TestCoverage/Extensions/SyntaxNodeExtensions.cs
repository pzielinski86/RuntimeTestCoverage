using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCoverage.Extensions
{
    public static class SyntaxNodeExtensions
    {
        public static string[] GetUsedNamespaces(this SyntaxNode node)
        {
            return node.Ancestors().OfType<CompilationUnitSyntax>().First().DescendantNodes().
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

        public static BaseMethodDeclarationSyntax GetParentMethod(this SyntaxNode node)
        {
            return node.Ancestors().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault();
        }

        public static MethodDeclarationSyntax GetMethodAt(this SyntaxNode root, int position)
        {
            var method =
                root.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>().
                    FirstOrDefault(x => x.FullSpan.Start < position && x.FullSpan.End > position);

            return method;
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