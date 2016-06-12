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
                    AddNodePath(path, methodDeclarationSyntax.Identifier.Text);
                }

                var classDeclarationSyntax = parent as ClassDeclarationSyntax;
                if (classDeclarationSyntax != null)
                {
                    AddNodePath(path, classDeclarationSyntax.Identifier.Text);
                }

                var namespaceDeclarationSyntax = parent as NamespaceDeclarationSyntax;
                if (namespaceDeclarationSyntax != null)
                {
                    AddNodePath(path, namespaceDeclarationSyntax.Name.ToString());
                }

                parent = parent.Parent;
            }

            AddNodePath(path, documentName);
            AddNodePath(path, projectName);

            return path.ToString().TrimEnd('.');
        }

        private static void AddNodePath(StringBuilder pathBuilder, string name)
        {
            pathBuilder.Insert(0, name + ".");
        }
    }
}