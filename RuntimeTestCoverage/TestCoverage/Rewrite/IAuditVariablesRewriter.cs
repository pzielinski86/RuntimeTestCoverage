using Microsoft.CodeAnalysis;

namespace TestCoverage.Rewrite
{
    public interface IAuditVariablesRewriter
    {
        SyntaxNode Rewrite(string projectName, string documentPath, SyntaxNode root, IAuditVariablesMap auditVariableMapping);
    }
}