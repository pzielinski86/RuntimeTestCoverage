using Microsoft.CodeAnalysis;

namespace TestCoverage.Rewrite
{
    public interface IAuditVariablesWalker
    {
        AuditVariablePlaceholder[] Walk(string projectName,string documentPath,SyntaxNode root);
    }
}