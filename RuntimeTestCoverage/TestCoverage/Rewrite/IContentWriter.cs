using Microsoft.CodeAnalysis;

namespace TestCoverage.Rewrite
{
    public interface IContentWriter
    {
        void Write(string documentPath, SyntaxTree syntaxTree);
    }
}