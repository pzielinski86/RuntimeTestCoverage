using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCoverage.Rewrite
{
    public class RewrittenDocument
    {
        public SyntaxTree SyntaxTree { get; private set; }
        public string DocumentPath { get;private set; }
        public bool ContainsTest { get;  }

        public RewrittenDocument(SyntaxTree syntaxTree, string documentPath, bool containsTest)
        {
            SyntaxTree = syntaxTree;
            DocumentPath = documentPath;
            ContainsTest = containsTest;
        }

        public void AddAttributeLists(AttributeListSyntax attrs)
        {
            var newNode=((CompilationUnitSyntax) SyntaxTree.GetRoot()).AddAttributeLists(attrs);
            SyntaxTree = newNode.SyntaxTree;
        }
    }
}