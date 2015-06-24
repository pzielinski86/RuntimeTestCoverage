using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace TestCoverage
{
    public class RewrittenItemInfo
    {
        private readonly string _documentName;
        private readonly SyntaxTree _syntaxTree;

        public RewrittenItemInfo(string documentName, SyntaxTree syntaxTree)
        {
            _documentName = documentName;
            _syntaxTree = syntaxTree;
        }

        public SyntaxTree Tree
        {
            get { return _syntaxTree; }
        }

        public string DocumentName
        {
            get { return _documentName; }
        }
    }
}