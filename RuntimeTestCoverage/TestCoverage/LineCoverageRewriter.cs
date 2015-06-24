using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCoverage
{
    public class LineCoverageRewriter : CSharpSyntaxRewriter
    {
        private readonly AuditVariablesMap _auditVariableMapping;
        private readonly string _documentName;
        private readonly int[] _auditVariableNames;
        private int _auditIndex = 0;

        public LineCoverageRewriter(AuditVariablesMap auditVariableMapping, string documentName,int[] auditVariableNames)
        {
            _auditVariableMapping = auditVariableMapping;
            _documentName = documentName;
            _auditVariableNames = auditVariableNames;
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
            string varName = AuditVariableMapping.AddVariable(_auditVariableNames[_auditIndex],_documentName, node);
            _auditIndex++;

            StatementSyntax newNode = SyntaxFactory.ParseStatement(string.Format("\t{0}.{1}[\"{2}\"]=true;\n", AuditVariableMapping.AuditVariablesClassName, AuditVariableMapping.AuditVariablesDictionaryName, varName));

            return newNode;
        }
    }
}