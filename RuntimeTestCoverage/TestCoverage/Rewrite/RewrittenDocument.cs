using Microsoft.CodeAnalysis;

namespace TestCoverage.Rewrite
{
    internal class RewrittenDocument
    {
        public AuditVariablesMap AuditVariablesMap { get; private set; }
        public SyntaxTree SyntaxTree { get; private set; }
        public string DocumentPath { get;private set; }

        public RewrittenDocument(AuditVariablesMap auditVariablesMap, SyntaxTree syntaxTree, string documentPath)
        {
            AuditVariablesMap = auditVariablesMap;
            SyntaxTree = syntaxTree;
            DocumentPath = documentPath;
        }
    }
}