using System.Reflection;
using System.Runtime.InteropServices;
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
        _Assembly Assembly { get; }
        _Assembly EmitAndSave();
        ISemanticModel GetSemanticModel(SyntaxTree syntaxTree);
    }
}