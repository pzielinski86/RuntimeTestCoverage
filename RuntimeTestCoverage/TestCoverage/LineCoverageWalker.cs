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
        private int currentMethodSpan;

        public AuditVariablePlaceholder[] AuditVariablePlaceholderPositions
        {
            get { return _auditVariablePositions.ToArray(); }
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            currentMethodSpan = node.Span.Start;
            base.VisitMethodDeclaration(node);
        }

        public override void VisitBlock(BlockSyntax node)
        {
            foreach (var statement in node.Statements)
            {
                _auditVariablePositions.Add(new AuditVariablePlaceholder(node.SyntaxTree.FilePath,NodePathBuilder.BuildPath(statement), statement.Span.Start- currentMethodSpan));
            }

            base.VisitBlock(node);
        }
    }
}