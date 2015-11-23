using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace TestCoverage.Compilation
{
    public interface ICompiledItem
    {
        Project Project { get; }
        CSharpCompilation Compilation { get; }
        bool IsEmitted { get; set; }
        string DllPath { get; }
        string EmitAndSave();
        ISemanticModel GetSemanticModel(SyntaxTree syntaxTree);
    }
}