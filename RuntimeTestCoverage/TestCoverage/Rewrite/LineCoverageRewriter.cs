using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCoverage.Rewrite
{
    internal class LineCoverageRewriter : CSharpSyntaxRewriter
    {
        private readonly AuditVariablesMap _auditVariableMapping;
        private readonly AuditVariablePlaceholder[] _auditVariablePlaceholders;
        private int _auditIndex;

        public LineCoverageRewriter(AuditVariablesMap auditVariableMapping, AuditVariablePlaceholder[] auditVariablePlaceholders)
        {
            _auditVariableMapping = auditVariableMapping;
            _auditVariablePlaceholders = auditVariablePlaceholders;
        }

        public AuditVariablesMap AuditVariableMapping
        {
            get { return _auditVariableMapping; }
        }

        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            List<StatementSyntax> statements = new List<StatementSyntax>();

            foreach (var statement in node.Statements)
            {
                statements.Add(CreateLineAuditNdoe(statement));
                statements.Add(statement);
            }

            return base.VisitBlock(SyntaxFactory.Block(statements));
        }

        private StatementSyntax CreateLineAuditNdoe(SyntaxNode node)
        {        
            string varName = AuditVariableMapping.AddVariable(_auditVariablePlaceholders[_auditIndex]);
            _auditIndex++;

            StatementSyntax auditNode = SyntaxFactory.ParseStatement(string.Format("\t{0}.{1}[\"{2}\"]=true;\n", AuditVariableMapping.AuditVariablesClassName, AuditVariableMapping.AuditVariablesDictionaryName, varName));

            SyntaxTriviaList comment = SyntaxFactory.ParseTrailingTrivia(string.Format("//{0}\n", AuditVariableMapping.Map[varName].DocumentPath));

            return auditNode.WithTrailingTrivia(comment);
        }
    }
}