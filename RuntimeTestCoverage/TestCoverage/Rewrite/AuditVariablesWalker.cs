using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCoverage.Rewrite
{
    public class AuditVariablesWalker : CSharpSyntaxWalker, IAuditVariablesWalker
    {
        private string _projectName;
        private string _documentPath;
        private readonly List<AuditVariablePlaceholder> _auditVariablePlaceholders = new List<AuditVariablePlaceholder>();

        public void Init(string projectName, string documentPath)
        {
            _auditVariablePlaceholders.Clear();
            _projectName = projectName;
            _documentPath = documentPath;
        }

        public AuditVariablePlaceholder[] InsertedAuditVariables
        {
            get { return _auditVariablePlaceholders.ToArray(); }
        }

        public override void VisitBlock(BlockSyntax node)
        {
            foreach (StatementSyntax statement in node.Statements)
            {
                CreateAuditVariable(statement);
            }

            base.VisitBlock(node);
        }

        private void CreateAuditVariable(StatementSyntax statement)
        {
            string documentName = Path.GetFileNameWithoutExtension(_documentPath);

            string nodePath = NodePathBuilder.BuildPath(statement, documentName, _projectName);
            var auditVariablePlaceholder = new AuditVariablePlaceholder(_documentPath,
                nodePath,
                statement.Span.Start);

            _auditVariablePlaceholders.Add(auditVariablePlaceholder);
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            if (!(node.Statement is BlockSyntax))
            {
                CreateAuditVariable(node.Statement);
            }

            base.VisitIfStatement(node);
        }

        public override void VisitElseClause(ElseClauseSyntax node)
        {
            if (!(node.Statement is BlockSyntax))
            {
                CreateAuditVariable(node.Statement);
            }

            base.VisitElseClause(node);
        }

        public AuditVariablePlaceholder[] Walk(string projectName, string documentPath, SyntaxNode root)
        {
            Init(projectName, documentPath);
            Visit(root);

            return InsertedAuditVariables;
        }
    }
}