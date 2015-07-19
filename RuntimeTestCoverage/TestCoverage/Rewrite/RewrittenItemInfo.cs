using Microsoft.CodeAnalysis;

namespace TestCoverage.Rewrite
{
    public class RewrittenItemInfo
    {
        private readonly string _documentPath;

        public RewrittenItemInfo(string documentPath, SyntaxTree syntaxTree)
        {
            SyntaxTree = syntaxTree;
            _documentPath = documentPath;
        }

        public string DocumentPath
        {
            get { return _documentPath; }
        }

        public SyntaxTree SyntaxTree { get; set; }
    }
}