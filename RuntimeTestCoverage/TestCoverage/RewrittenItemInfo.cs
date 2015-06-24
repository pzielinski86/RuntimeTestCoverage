using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace TestCoverage
{
    public class RewrittenItemInfo
    {
        private readonly Document _document;

        public RewrittenItemInfo( Document document, SyntaxTree syntaxTree)
        {     
            SyntaxTree = syntaxTree;
            _document = document;
        }

        public Document Document
        {
            get { return _document; }
        }

        public SyntaxTree SyntaxTree { get; set; }

    }
}