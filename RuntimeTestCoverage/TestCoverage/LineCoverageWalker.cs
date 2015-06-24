using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCoverage
{
    public class LineCoverageWalker : CSharpSyntaxWalker
    {
        private readonly List<int> _auditVariablePositions = new List<int>();

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
}