using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCoverage
{

    public class LineCoveravWalker : CSharpSyntaxWalker
    {
        private List<int> _auditVariablePositions = new List<int>();

        public int[] AuditVariablePositions
        {
            get { return _auditVariablePositions.ToArray(); }
        }

        public override void VisitBlock(BlockSyntax node)
        {
            foreach (var statement in node.Statements)
            {
                _auditVariablePositions.Add(statement.Span.Start);
            }

            base.VisitBlock(node);
        }
    }
    public class LineCoverageRewriter : CSharpSyntaxRewriter
    {
        private readonly AuditVariablesMap _auditVariableMapping;
        private readonly int[] _auditVariableNames;
        private int _auditIndex = 0;
        private int _level = 1;

        public LineCoverageRewriter(AuditVariablesMap auditVariableMapping, int[] auditVariableNames)
        {
            _auditVariableMapping = auditVariableMapping;
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

        //public override SyntaxList<TNode> VisitList<TNode>(SyntaxList<TNode> list)
        //{
        //    SyntaxList<TNode> newList = new SyntaxList<TNode>();

        //    bool isNewLevel = false;

        //    for (int i = 0; i < list.Count; i++)
        //    {
        //        if (list[i].Parent is BlockSyntax)
        //        {
        //            isNewLevel = true;

        //            SyntaxNode newNode = CreateLineAuditNdoe(list[i]);

        //            newList = newList.Add(list[i]);
        //            newList = newList.Add((TNode)newNode);

        //        }
        //        else
        //        {
        //            newList = list;
        //        }
        //    }

        //    _level = isNewLevel ? _level + 1 : _level;

        //    return base.VisitList(newList);
        //}

        private StatementSyntax CreateLineAuditNdoe(SyntaxNode node)
        {
            string varName = AuditVariableMapping.AddVariable(_auditVariableNames[_auditIndex], node);
            _auditIndex++;

            StatementSyntax newNode = SyntaxFactory.ParseStatement(string.Format("{0}{1}.{2}[\"{3}\"]=true;\n", new String('\t', _level), AuditVariableMapping.AuditVariablesClassName, AuditVariableMapping.AuditVariablesDictionaryName, varName));

            return newNode;
        }
    }
}