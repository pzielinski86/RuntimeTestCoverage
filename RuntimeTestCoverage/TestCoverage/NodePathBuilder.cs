using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace TestCoverage
{
    internal static class NodePathBuilder
    {
        public static string BuildPath(SyntaxNode node)
        {
            var parent = node;
            StringBuilder path = new StringBuilder();

            while (parent != null)
            {
                var methodDeclarationSyntax = parent as MethodDeclarationSyntax;
                if (methodDeclarationSyntax != null)
                    path.Insert(0, methodDeclarationSyntax.Identifier.Text + ".");

                var classDeclarationSyntax = parent as ClassDeclarationSyntax;
                if (classDeclarationSyntax != null)
                    path.Insert(0, classDeclarationSyntax.Identifier.Text + ".");

                var namespaceDeclarationSyntax = parent as NamespaceDeclarationSyntax;
                if (namespaceDeclarationSyntax != null)
                    path.Insert(0, namespaceDeclarationSyntax.Name + ".");

                parent = parent.Parent;
            }

            return path.ToString().TrimEnd('.');
        }
    }
}