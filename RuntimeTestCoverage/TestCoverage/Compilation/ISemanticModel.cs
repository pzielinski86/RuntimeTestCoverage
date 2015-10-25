using Microsoft.CodeAnalysis;

namespace TestCoverage.Compilation
{
    public interface ISemanticModel
    {
        string GetSymbolName(SyntaxNode node);

    }
}