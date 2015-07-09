using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCoverage.Rewrite
{
    internal class LineCoverageWalker : CSharpSyntaxWalker
    {
        private readonly string _projectName;
        private readonly List<AuditVariablePlaceholder> _auditVariablePositions = new List<AuditVariablePlaceholder>();
        private int _currentMethodSpan;

        public LineCoverageWalker(string projectName)
        {
            _projectName = projectName;
        }

        public AuditVariablePlaceholder[] AuditVariablePlaceholderPositions
        {
            get { return _auditVariablePositions.ToArray(); }
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            _currentMethodSpan = node.Span.Start;
            base.VisitMethodDeclaration(node);
        }

        public override void VisitBlock(BlockSyntax node)
        {
            foreach (var statement in node.Statements)
            {
                string documentName = Path.GetFileNameWithoutExtension(statement.SyntaxTree.FilePath);
                _auditVariablePositions.Add(new AuditVariablePlaceholder(node.SyntaxTree.FilePath,
                    NodePathBuilder.BuildPath(statement, documentName, _projectName), 
                     statement.Span.Start - _currentMethodSpan));
            }

            base.VisitBlock(node);
        }
    }
}