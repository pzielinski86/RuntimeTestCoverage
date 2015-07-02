using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCoverage
{
    internal class LineCoverageWalker : CSharpSyntaxWalker
    {
        private readonly List<AuditVariablePlaceholder> _auditVariablePositions =new List<AuditVariablePlaceholder>();

        public AuditVariablePlaceholder[] AuditVariablePlaceholderPositions
        {
            get { return _auditVariablePositions.ToArray(); }
        }

        public override void VisitBlock(BlockSyntax node)
        {
            foreach (var statement in node.Statements)
            {
                _auditVariablePositions.Add(new AuditVariablePlaceholder(GetPath(statement),statement.Span.Start));
            }

            base.VisitBlock(node);
        }

        private string GetPath(SyntaxNode node)
        {
            var parent = node.Parent;
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