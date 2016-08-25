using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace TestCoverage.Rewrite
{
    public class AuditVariablesRewriter : CSharpSyntaxRewriter, IAuditVariablesRewriter
    {
        private readonly IAuditVariablesWalker _auditVariablesWalker;
        private int _auditIndex;
        private AuditVariablePlaceholder[] _auditVariablePlaceholders;

        public AuditVariablesRewriter(IAuditVariablesWalker auditVariablesWalker)
        {
            _auditVariablesWalker = auditVariablesWalker;
        }

        public void Init()
        {
            _auditIndex = 0;
        }

        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            var newStatements = new List<StatementSyntax>();

            foreach (var statement in node.Statements)
            {
                newStatements.Add(CreateLineAuditNode());
                newStatements.Add(statement);
            }
            var syntaxList = SyntaxFactory.List<StatementSyntax>(newStatements);

            var block = SyntaxFactory.Block(node.OpenBraceToken, syntaxList, node.CloseBraceToken);

            return base.VisitBlock(block);
        }

        public override SyntaxNode VisitWhileStatement(WhileStatementSyntax node)
        { 
            SyntaxNode rewrittenNode = null;

            if (node.Statement != null)
                rewrittenNode = RewriteWithBlockIfRequired(node, node.Statement);

            return base.VisitWhileStatement((WhileStatementSyntax)rewrittenNode ?? node);
        }

        public override SyntaxNode VisitForStatement(ForStatementSyntax node)
        {
            SyntaxNode rewrittenNode = null;

            if (node.Statement != null)
                rewrittenNode = RewriteWithBlockIfRequired(node, node.Statement);

            return base.VisitForStatement((ForStatementSyntax)rewrittenNode ?? node);
        }

        public override SyntaxNode VisitForEachStatement(ForEachStatementSyntax node)
        {
            SyntaxNode rewrittenNode = null;

            if (node.Statement != null)
                rewrittenNode = RewriteWithBlockIfRequired(node, node.Statement);

            return base.VisitForEachStatement((ForEachStatementSyntax)rewrittenNode ?? node);
        }

        public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
        {
            SyntaxNode rewrittenNode = null;

            if (node.Statement != null)
                rewrittenNode = RewriteWithBlockIfRequired(node, node.Statement);

            return base.VisitIfStatement((IfStatementSyntax)rewrittenNode ?? node);
        }

        public override SyntaxNode VisitElseClause(ElseClauseSyntax node)
        {
            if (node.Statement is IfStatementSyntax)
                return base.VisitElseClause(node);

            SyntaxNode rewrittenNode = null;

            if (node.Statement != null)
                rewrittenNode = RewriteWithBlockIfRequired(node, node.Statement);

            return base.VisitElseClause((ElseClauseSyntax)rewrittenNode ?? node);
        }

        private SyntaxNode RewriteWithBlockIfRequired(SyntaxNode parent, StatementSyntax node)
        {
            if (!(node is BlockSyntax))
            {
                List<StatementSyntax> newStatements = new List<StatementSyntax>
                {
                    node
                };

                var block = SyntaxFactory.Block(newStatements);

                return parent.ReplaceNode(node, block);
            }

            return null;
        }

        public SyntaxNode Rewrite(string projectName, string documentPath, SyntaxNode root)
        {
            _auditVariablePlaceholders = _auditVariablesWalker.Walk(projectName, documentPath, root);
            Init();

            return Visit(root);
        }
        private StatementSyntax CreateLineAuditNode()
        {
            string key = _auditVariablePlaceholders[_auditIndex].GetKey();
            string initVariableCode = _auditVariablePlaceholders[_auditIndex].GetInitializationCode();

            string auditNodeSourceCode =
                $"\t{AuditVariablesMap.AuditVariablesListClassName}.{AuditVariablesMap.AuditVariablesListName}[@\"{key}\"] = " +
                $"{initVariableCode};\n";

            _auditIndex++;

            return SyntaxFactory.ParseStatement(auditNodeSourceCode);

        }
    }
}