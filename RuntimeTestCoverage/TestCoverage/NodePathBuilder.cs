using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace TestCoverage
{
    public static class NodePathBuilder
    {
        public static string BuildPath(SyntaxNode node, string documentName, string projectName)
        {
            SyntaxNode parent = node;
            StringBuilder path = new StringBuilder();

            while (parent != null)
            {
                var methodDeclarationSyntax = parent as MethodDeclarationSyntax;

                if (methodDeclarationSyntax != null)
                {
                    path.Insert(0, methodDeclarationSyntax.Identifier.Text + ".");                    
                }

                var classDeclarationSyntax = parent as ClassDeclarationSyntax;
                if (classDeclarationSyntax != null)
                {
                    path.Insert(0, classDeclarationSyntax.Identifier.Text + ".");
                }

                var namespaceDeclarationSyntax = parent as NamespaceDeclarationSyntax;
                if (namespaceDeclarationSyntax != null)
                {
                    path.Insert(0, namespaceDeclarationSyntax.Name + ".");
                }

                parent = parent.Parent;
            }

            path.Insert(0, documentName + ".");
            path.Insert(0, projectName + ".");

            return path.ToString().TrimEnd('.');
        }
    }
}