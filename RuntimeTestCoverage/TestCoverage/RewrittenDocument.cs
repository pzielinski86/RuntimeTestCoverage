using Microsoft.CodeAnalysis;

namespace TestCoverage
{
    class RewrittenDocument
    {
        public AuditVariablesMap AuditVariablesMap { get; private set; }
        public SyntaxTree SyntaxTree { get; private set; }

        public RewrittenDocument(AuditVariablesMap auditVariablesMap, SyntaxTree syntaxTree)
        {
            AuditVariablesMap = auditVariablesMap;
            SyntaxTree = syntaxTree;
        }
    }
}