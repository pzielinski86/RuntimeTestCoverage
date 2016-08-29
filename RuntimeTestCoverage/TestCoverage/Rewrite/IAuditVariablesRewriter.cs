using Microsoft.CodeAnalysis;

namespace TestCoverage.Rewrite
{
    public interface IAuditVariablesRewriter
    {
        RewrittenDocument Rewrite(string projectName, string documentPath, SyntaxNode root);
    }
}