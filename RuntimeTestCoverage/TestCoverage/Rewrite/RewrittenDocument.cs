using Microsoft.CodeAnalysis;

namespace TestCoverage.Rewrite
{
    public class RewrittenDocument
    {
        public SyntaxTree SyntaxTree { get; private set; }
        public string DocumentPath { get;private set; }

        public RewrittenDocument( SyntaxTree syntaxTree, string documentPath)
        {
            SyntaxTree = syntaxTree;
            DocumentPath = documentPath;
        }
    }
}