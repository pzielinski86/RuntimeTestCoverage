using Microsoft.CodeAnalysis;

namespace TestCoverage.Compilation
{
    public interface ISemanticModel
    {
        object GetConstantValue(SyntaxNode node);
        string GetAssemblyName();
    }
}