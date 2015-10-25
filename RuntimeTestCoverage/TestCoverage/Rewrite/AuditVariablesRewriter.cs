using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCoverage.Rewrite
{
    public class AuditVariablesRewriter : CSharpSyntaxRewriter, IAuditVariablesRewriter
    {
        private IAuditVariablesMap _auditVariableMapping;
        private readonly IAuditVariablesWalker _auditVariablesWalker;
        private int _auditIndex;
        private AuditVariablePlaceholder[] _auditVariablePlaceholders;

        public AuditVariablesRewriter(IAuditVariablesWalker auditVariablesWalker)
        {
            _auditVariablesWalker = auditVariablesWalker;
        }

        public void Init(IAuditVariablesMap auditVariableMapping)
        {
            _auditIndex = 0;
            _auditVariableMapping = auditVariableMapping;
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
                rewrittenNode = RewriteWithBlockIfRequired(node, node.Statement);
            if (node.Else != null)
            {
                var currentParent =(IfStatementSyntax) rewrittenNode ?? node;
                rewrittenNode = RewriteWithBlockIfRequired(currentParent, currentParent.Else.Statement);
            }

            return rewrittenNode ?? base.VisitIfStatement(node);
        }

        private SyntaxNode RewriteWithBlockIfRequired(SyntaxNode parent,StatementSyntax node)
        {
            if (!(node is BlockSyntax))
            {
                List<StatementSyntax> newStatements = new List<StatementSyntax>
                {
                    CreateLineAuditNode(),
                    node
                };


                var block = SyntaxFactory.Block(newStatements);

                return parent.ReplaceNode(node, block);
            }

            return null;
        }

        public SyntaxNode Rewrite(string projectName, string documentPath, SyntaxNode root, IAuditVariablesMap auditVariableMapping)
        {
            _auditVariablePlaceholders = _auditVariablesWalker.Walk(projectName, documentPath, root);
            Init(auditVariableMapping);

            return Visit(root);
        }
        private StatementSyntax CreateLineAuditNode()
        {
            string varName = _auditVariableMapping.AddVariable(_auditVariablePlaceholders[_auditIndex]);
            _auditIndex++;

            string auditNodeSourceCode = string.Format("\t{0}.{1}.Add(\"{2}\");\n", _auditVariableMapping.AuditVariablesClassName, _auditVariableMapping.AuditVariablesListName, varName);
            StatementSyntax auditNode = SyntaxFactory.ParseStatement(auditNodeSourceCode);

            string commentCode = string.Format("//{0}\n", _auditVariableMapping.Map[varName].DocumentPath);
            SyntaxTriviaList comment = SyntaxFactory.ParseTrailingTrivia(commentCode);

            return auditNode.WithTrailingTrivia(comment);
        }
    }
}