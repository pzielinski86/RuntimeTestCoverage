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
            List<StatementSyntax> newStatements = new List<StatementSyntax>();

            foreach (var statement in node.Statements)
            {
                newStatements.Add(CreateLineAuditNode());
                newStatements.Add(statement);
            }

            return base.VisitBlock(SyntaxFactory.Block(newStatements));
        }

        public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
        {
            SyntaxNode rewrittenNode = null;

            if (node.Statement != null)
            {
                rewrittenNode = RewriteWithBlockIfRequired(node, node.Statement);
            }

            return base.VisitIfStatement((IfStatementSyntax)rewrittenNode ?? node);
        }

        public override SyntaxNode VisitElseClause(ElseClauseSyntax node)
        {
            if (node.Statement is IfStatementSyntax)
                return base.VisitElseClause(node);

            SyntaxNode rewrittenNode = null;

            if (node.Statement != null)
            {
                rewrittenNode = RewriteWithBlockIfRequired(node, node.Statement);
            }

            return base.VisitElseClause((ElseClauseSyntax)rewrittenNode ?? node);
        }

        private SyntaxNode RewriteWithBlockIfRequired(SyntaxNode parent,StatementSyntax node)
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

            string initVariableCode = _auditVariablePlaceholders[_auditIndex].ToString();
            string auditNodeSourceCode =
                $"\t{AuditVariablesMap.AuditVariablesListClassName}.{AuditVariablesMap.AuditVariablesListName}.Add({initVariableCode});\n";

            _auditIndex++;

            return SyntaxFactory.ParseStatement(auditNodeSourceCode);

        }
    }
}